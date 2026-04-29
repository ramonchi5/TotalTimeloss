using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.TimeFormatters;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System.Linq;

namespace TotalTimeloss.UI.Components;

public class TotalTimeloss : IComponent
{
    protected LiveSplitState CurrentState { get; set; }
    protected TotalTimelossSettings Settings { get; set; }

    public float PaddingTop => 0;
    public float PaddingLeft => 0;
    public float PaddingBottom => 0;
    public float PaddingRight => 0;

    public IDictionary<string, Action> ContextMenuControls => null!;

    public TotalTimeloss(LiveSplitState state)
    {
        CurrentState = state;
        Settings = new TotalTimelossSettings();
    }

    // Formats a TimeSpan without leading zeros.
    // Examples: 27:34.96 | 1:05:22.10 | 7.34 | -12.00
    private string FormatTime(TimeSpan t, TimeAccuracy accuracy)
    {
        string decimals = accuracy switch
        {
            TimeAccuracy.Hundredths   => $".{Math.Abs(t.Milliseconds) / 10:D2}",
            TimeAccuracy.Tenths       => $".{Math.Abs(t.Milliseconds) / 100:D1}",
            TimeAccuracy.Milliseconds => $".{Math.Abs(t.Milliseconds):D3}",
            _                         => ""
        };

        bool negative = t < TimeSpan.Zero;
        string sign = negative ? "-" : "";
        int totalHours = (int)Math.Abs(t.TotalHours);
        int totalMinutes = (int)Math.Abs(t.TotalMinutes);
        int seconds = Math.Abs(t.Seconds);

        if (totalHours >= 1)
            return $"{sign}{totalHours}:{Math.Abs(t.Minutes):D2}:{seconds:D2}{decimals}";
        else if (totalMinutes >= 1)
            return $"{sign}{totalMinutes}:{seconds:D2}{decimals}";
        else
            return $"{sign}{seconds}{decimals}";
    }

    private void DrawBackground(Graphics g, float width, float height)
    {
        if (Settings.BackgroundGradient == GradientType.Plain)
        {
            if (Settings.BackgroundColor.A > 0)
            {
                using var brush = new SolidBrush(Settings.BackgroundColor);
                g.FillRectangle(brush, 0, 0, width, height);
            }
        }
        else
        {
            var mode = Settings.BackgroundGradient == GradientType.Horizontal
                ? LinearGradientMode.Horizontal
                : LinearGradientMode.Vertical;
            using var brush = new LinearGradientBrush(
                new RectangleF(0, 0, Math.Max(width, 1), Math.Max(height, 1)),
                Settings.BackgroundColor,
                Settings.BackgroundColor2,
                mode);
            g.FillRectangle(brush, 0, 0, width, height);
        }
    }

    private void DrawTextWithEffects(Graphics g, string text, Font font, Color textColor, RectangleF rect, StringFormat format, LiveSplit.Options.LayoutSettings settings)
    {
        if (string.IsNullOrEmpty(text) || font == null || rect.Width <= 0 || rect.Height <= 0)
            return;

        bool hasShadow = GetLayoutSetting(settings, "DropShadows", false);
        Color shadowColor = GetLayoutSetting(settings, "ShadowsColor", Color.Transparent);
        Color outlineColor = GetLayoutSetting(settings, "TextOutlineColor", Color.Transparent);

        SizeF measured = g.MeasureString(text, font);
        float x = rect.X;
        if (format.Alignment == StringAlignment.Far)
            x = rect.Right - measured.Width;
        else if (format.Alignment == StringAlignment.Center)
            x = rect.X + (rect.Width - measured.Width) / 2f;
        float y = rect.Y;

        using var nearFormat = new StringFormat(format)
        {
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };
        nearFormat.FormatFlags |= StringFormatFlags.NoWrap;

        if (g.TextRenderingHint == TextRenderingHint.AntiAlias && outlineColor.A > 0)
        {
            float fontSize = GetFontSize(g, font);
            using var gp = new GraphicsPath();
            using var outline = new Pen(outlineColor, GetOutlineSize(fontSize)) { LineJoin = LineJoin.Round };

            if (hasShadow && shadowColor.A > 0)
            {
                using var shadowBrush = new SolidBrush(shadowColor);
                gp.AddString(text, font.FontFamily, (int)font.Style, fontSize, new RectangleF(x + 1f, y + 1f, 9999, 9999), nearFormat);
                g.FillPath(shadowBrush, gp);
                gp.Reset();
                gp.AddString(text, font.FontFamily, (int)font.Style, fontSize, new RectangleF(x + 2f, y + 2f, 9999, 9999), nearFormat);
                g.FillPath(shadowBrush, gp);
                gp.Reset();
            }

            gp.AddString(text, font.FontFamily, (int)font.Style, fontSize, new RectangleF(x, y, 9999, 9999), nearFormat);
            g.DrawPath(outline, gp);
            using var textBrush = new SolidBrush(textColor);
            g.FillPath(textBrush, gp);
        }
        else
        {
            if (hasShadow && shadowColor.A > 0)
            {
                using var shadowBrush = new SolidBrush(shadowColor);
                g.DrawString(text, font, shadowBrush, x + 1f, y + 1f, nearFormat);
                g.DrawString(text, font, shadowBrush, x + 2f, y + 2f, nearFormat);
            }

            using var textBrush = new SolidBrush(textColor);
            g.DrawString(text, font, textBrush, x, y, nearFormat);
        }
    }

    private void DrawTextWithEffectsClipped(Graphics g, string text, Font font, Color textColor, RectangleF rect, StringFormat format, LiveSplit.Options.LayoutSettings settings)
    {
        if (string.IsNullOrEmpty(text) || rect.Width <= 0 || rect.Height <= 0)
            return;

        GraphicsState saved = g.Save();
        try
        {
            g.SetClip(rect);
            DrawTextWithEffects(g, text, font, textColor, rect, format, settings);
        }
        finally
        {
            g.Restore(saved);
        }
    }

    private void DrawLabelWithEffects(Graphics g, string text, Font font, Color textColor, RectangleF rect, StringFormat format, LiveSplit.Options.LayoutSettings settings)
    {
        DrawTextWithEffectsClipped(g, text, font, textColor, rect, format, settings);

        if (Settings.UnderlineLabels)
            DrawUnderlineClipped(g, text, font, textColor, rect, format, settings);
    }

    private void DrawUnderlineClipped(Graphics g, string text, Font font, Color color, RectangleF rect, StringFormat format, LiveSplit.Options.LayoutSettings settings)
    {
        if (string.IsNullOrEmpty(text) || font == null || color.A == 0 || rect.Width <= 0 || rect.Height <= 0)
            return;

        if (!Settings.UnderlineLabelSpaces && string.IsNullOrWhiteSpace(text))
            return;

        SizeF measured = Settings.UnderlineLabelSpaces
            ? MeasureTextIncludingSpaces(g, text, font)
            : g.MeasureString(text, font);
        float textX = rect.X;
        if (format.Alignment == StringAlignment.Far)
            textX = rect.Right - measured.Width;
        else if (format.Alignment == StringAlignment.Center)
            textX = rect.X + (rect.Width - measured.Width) / 2f;

        float underlineLeft = textX;
        float underlineRight = textX + measured.Width;

        if (Settings.UnderlineLabelSpaces)
        {
            RectangleF fullBounds = MeasureCharacterBounds(
                g,
                text,
                font,
                textX,
                rect.Y,
                format,
                0,
                text.Length,
                true);
            underlineLeft = fullBounds.Left;
            underlineRight = fullBounds.Right;
        }
        else
        {
            int visibleStart = 0;
            while (visibleStart < text.Length && char.IsWhiteSpace(text[visibleStart]))
                visibleStart++;

            int visibleEnd = text.Length - 1;
            while (visibleEnd >= visibleStart && char.IsWhiteSpace(text[visibleEnd]))
                visibleEnd--;

            if (visibleEnd < visibleStart)
                return;

            RectangleF visibleBounds = MeasureCharacterBounds(
                g,
                text,
                font,
                textX,
                rect.Y,
                format,
                visibleStart,
                visibleEnd - visibleStart + 1,
                false);
            underlineLeft = visibleBounds.Left;
            underlineRight = visibleBounds.Right;
        }

        float startX = Clamp(underlineLeft, rect.Left, rect.Right);
        float endX = Clamp(underlineRight, rect.Left, rect.Right);
        if (endX - startX <= 0.5f)
            return;

        float fontSize = GetFontSize(g, font);
        float thickness = Math.Max(1.25f, fontSize * 0.09f);
        float y = Clamp(rect.Y + measured.Height - Math.Max(2f, thickness * 1.5f), rect.Top, rect.Bottom - thickness);
        bool hasShadow = GetLayoutSetting(settings, "DropShadows", false);
        Color shadowColor = GetLayoutSetting(settings, "ShadowsColor", Color.Transparent);
        Color outlineColor = GetLayoutSetting(settings, "TextOutlineColor", Color.Transparent);

        GraphicsState saved = g.Save();
        try
        {
            g.SetClip(rect);
            if (hasShadow && shadowColor.A > 0)
            {
                using var shadowPen = new Pen(shadowColor, thickness);
                g.DrawLine(shadowPen, startX + 1f, y + 1f, endX + 1f, y + 1f);
                g.DrawLine(shadowPen, startX + 2f, y + 2f, endX + 2f, y + 2f);
            }

            if (outlineColor.A > 0)
            {
                using var outlinePen = new Pen(outlineColor, thickness + Math.Max(1f, thickness * 0.75f));
                g.DrawLine(outlinePen, startX, y, endX, y);
            }

            using var pen = new Pen(color, thickness);
            g.DrawLine(pen, startX, y, endX, y);
        }
        finally
        {
            g.Restore(saved);
        }
    }

    private SizeF MeasureTextIncludingSpaces(Graphics g, string text, Font font)
    {
        RectangleF bounds = MeasureCharacterBounds(
            g,
            text,
            font,
            0f,
            0f,
            StringFormat.GenericDefault,
            0,
            text.Length,
            true);

        SizeF fallback = g.MeasureString(text, font);
        return new SizeF(Math.Max(bounds.Width, fallback.Width), Math.Max(bounds.Height, fallback.Height));
    }

    private RectangleF MeasureCharacterBounds(Graphics g, string text, Font font, float x, float y, StringFormat format, int start, int length, bool measureTrailingSpaces)
    {
        using var rangeFormat = new StringFormat(format)
        {
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };
        rangeFormat.FormatFlags |= StringFormatFlags.NoWrap;
        if (measureTrailingSpaces)
            rangeFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        rangeFormat.SetMeasurableCharacterRanges(new[] { new CharacterRange(start, length) });

        Region[] ranges = g.MeasureCharacterRanges(text, font, new RectangleF(x, y, 9999f, 9999f), rangeFormat);
        try
        {
            if (ranges.Length > 0)
                return ranges[0].GetBounds(g);
        }
        finally
        {
            foreach (Region range in ranges)
                range.Dispose();
        }

        SizeF fallback = g.MeasureString(text.Substring(start, length), font);
        return new RectangleF(x, y, fallback.Width, fallback.Height);
    }

    private float GetFontSize(Graphics g, Font font)
    {
        if (font.Unit == GraphicsUnit.Point)
            return font.Size * g.DpiY / 72;

        return font.Size;
    }

    private float GetOutlineSize(float fontSize)
    {
        return 2.1f + (fontSize * 0.055f);
    }

    private static T GetLayoutSetting<T>(LiveSplit.Options.LayoutSettings settings, string propertyName, T fallback)
    {
        try
        {
            PropertyInfo prop = settings.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return fallback;

            object value = prop.GetValue(settings, null);
            if (value is T typedValue)
                return typedValue;
        }
        catch
        {
        }

        return fallback;
    }

    private TimeSpan? GetBestPossibleTime(LiveSplitState state)
    {
        if (state.CurrentPhase is TimerPhase.Running or TimerPhase.Paused)
        {
            TimeSpan? delta = LiveSplitStateHelper.GetLastDelta(
                state,
                state.CurrentSplitIndex,
                BestSegmentsComparisonGenerator.ComparisonName,
                state.CurrentTimingMethod) ?? TimeSpan.Zero;

            TimeSpan? liveDelta = state.CurrentTime[state.CurrentTimingMethod]
                - state.CurrentSplit.Comparisons[BestSegmentsComparisonGenerator.ComparisonName][state.CurrentTimingMethod];

            if (liveDelta > delta)
                delta = liveDelta;

            return delta + state.Run.Last().Comparisons[BestSegmentsComparisonGenerator.ComparisonName][state.CurrentTimingMethod];
        }
        else if (state.CurrentPhase == TimerPhase.Ended)
        {
            return state.Run.Last().SplitTime[state.CurrentTimingMethod];
        }
        else
        {
            return state.Run.Last().Comparisons[BestSegmentsComparisonGenerator.ComparisonName][state.CurrentTimingMethod];
        }
    }

    public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
    {
        var sob = SumOfBest.CalculateSumOfBest(
            state.Run, state.Settings.SimpleSumOfBest, true, state.CurrentTimingMethod) ?? TimeSpan.Zero;
        var bpt = GetBestPossibleTime(state) ?? TimeSpan.Zero;
        var tl = bpt - sob;

        var sobStr = FormatTime(sob, Settings.Accuracy);
        var bptStr = FormatTime(bpt, Settings.Accuracy);
        var tlStr = FormatTime(tl, Settings.Accuracy);
        var tlDisplayStr = tl > TimeSpan.Zero ? "+" + tlStr : "";
        var sobDisplayStr = Settings.ShowTime1 ? sobStr : string.Empty;
        var tlValueDisplayStr = Settings.ShowTime2 ? tlDisplayStr : string.Empty;
        var bptDisplayStr = Settings.ShowTime3 ? bptStr : string.Empty;

        var horizontalPad = 5f;
        var columnWidth = width / 3f;
        var rowHeight = g.MeasureString(sobStr, state.LayoutSettings.TextFont).Height;
        var innerRowGap = Math.Max(Settings.InnerRowGap, -rowHeight + 2f);

        bool showLabels = Settings.Display2Rows;
        VerticalHeight = showLabels ? rowHeight * 2 + innerRowGap : rowHeight;
        HorizontalWidth = width;

        DrawBackground(g, HorizontalWidth, VerticalHeight);

        Color layoutTextColor = state.LayoutSettings.TextColor;
        Color defaultTimelossColor = GetLayoutSetting(state.LayoutSettings, "BehindLosingTimeColor", layoutTextColor);

        Color label1Color = Settings.OverrideTextColor ? Settings.Label1Color : layoutTextColor;
        Color label2Color = Settings.OverrideTextColor ? Settings.Label2Color : layoutTextColor;
        Color label3Color = Settings.OverrideTextColor ? Settings.Label3Color : layoutTextColor;

        Color time1Color = Settings.OverrideTimeColor ? Settings.Time1Color : layoutTextColor;
        Color time2Color = Settings.OverrideTimeColor ? Settings.Time2Color : defaultTimelossColor;
        Color time3Color = Settings.OverrideTimeColor ? Settings.Time3Color : layoutTextColor;

        using var leftFormat = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
            FormatFlags = StringFormatFlags.NoWrap,
            Trimming = StringTrimming.None
        };
        using var rightFormat = new StringFormat
        {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Near,
            FormatFlags = StringFormatFlags.NoWrap,
            Trimming = StringTrimming.None
        };

        float valueY = 0f;

        if (showLabels)
        {
            DrawLabelsRow(g, state, width, columnWidth, horizontalPad, rowHeight, leftFormat, rightFormat,
                label1Color, label2Color, label3Color);
            valueY = rowHeight + innerRowGap;
        }

        DrawTimesRow(g, state, width, columnWidth, rowHeight, valueY, leftFormat, rightFormat,
            sobDisplayStr, tlValueDisplayStr, bptDisplayStr, time1Color, time2Color, time3Color);
    }

    private void DrawLabelsRow(
        Graphics g,
        LiveSplitState state,
        float width,
        float columnWidth,
        float horizontalPad,
        float rowHeight,
        StringFormat leftFormat,
        StringFormat rightFormat,
        Color label1Color,
        Color label2Color,
        Color label3Color)
    {
        string label1 = Settings.Label1Text ?? string.Empty;
        string label2 = Settings.Label2Text ?? string.Empty;
        string label3 = Settings.Label3Text ?? string.Empty;

        float leftEdge = horizontalPad;
        float rightEdge = Math.Max(leftEdge, width - horizontalPad);
        float middleLeft = columnWidth;
        float rightColumnLeft = columnWidth * 2f;

        float label3BaseX = rightColumnLeft;
        if (string.IsNullOrEmpty(label2))
        {
            float label3Width = g.MeasureString(label3, state.LayoutSettings.TextFont).Width;
            if (label3Width > rightEdge - rightColumnLeft)
                label3BaseX = Math.Max(middleLeft, rightEdge - label3Width);
        }

        RectangleF label1Rect = MoveRectWithinRow(leftEdge, rightEdge, Settings.Label1XOffset, rowHeight);
        RectangleF label2Rect = MoveRectWithinRow(middleLeft, rightEdge, Settings.Label2XOffset, rowHeight);
        RectangleF label3Rect = MoveRectWithinRow(label3BaseX, rightEdge, Settings.Label3XOffset, rowHeight);

        DrawLabelWithEffects(g, label1, state.LayoutSettings.TextFont, label1Color, label1Rect, leftFormat, state.LayoutSettings);
        DrawLabelWithEffects(g, label2, state.LayoutSettings.TextFont, label2Color, label2Rect, leftFormat, state.LayoutSettings);
        DrawLabelWithEffects(g, label3, state.LayoutSettings.TextFont, label3Color, label3Rect, leftFormat, state.LayoutSettings);
    }

    private static RectangleF MoveRectWithinRow(float baseX, float rightEdge, float offset, float height)
    {
        float x = baseX + offset;
        return new RectangleF(x, 0f, Math.Max(0f, rightEdge - x), height);
    }

    private void DrawTimesRow(
        Graphics g,
        LiveSplitState state,
        float width,
        float columnWidth,
        float rowHeight,
        float valueY,
        StringFormat leftFormat,
        StringFormat rightFormat,
        string sobStr,
        string tlDisplayStr,
        string bptStr,
        Color time1Color,
        Color time2Color,
        Color time3Color)
    {
        float sobWidth = g.MeasureString(sobStr, state.LayoutSettings.TimesFont).Width;
        float bptWidth = g.MeasureString(bptStr, state.LayoutSettings.TimesFont).Width;

        float sobX = columnWidth - sobWidth - Settings.MoveSobTimeLeft;
        float middleX = columnWidth + Settings.MiddleValueXOffset;
        float bptX = width - bptWidth - Settings.MoveBptTimeLeft;

        DrawTextWithEffects(g, sobStr, state.LayoutSettings.TimesFont, time1Color, new RectangleF(sobX, valueY, 9999f, rowHeight), leftFormat, state.LayoutSettings);
        DrawTextWithEffects(g, tlDisplayStr, state.LayoutSettings.TimesFont, time2Color, new RectangleF(middleX, valueY, 9999f, rowHeight), leftFormat, state.LayoutSettings);
        DrawTextWithEffects(g, bptStr, state.LayoutSettings.TimesFont, time3Color, new RectangleF(bptX, valueY, 9999f, rowHeight), leftFormat, state.LayoutSettings);
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }

    public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
    {
    }

    public float VerticalHeight { get; set; }
    public float MinimumWidth => 20;
    public float HorizontalWidth { get; set; }
    public float MinimumHeight => 20;

    public string ComponentName => "Total Timeloss";

    public Control GetSettingsControl(LayoutMode mode)
    {
        Settings.Mode = mode;
        Settings.PrepareForDisplay();
        return Settings;
    }

    public void SetSettings(XmlNode settings)
    {
        Settings.SetSettings(settings);
    }

    public XmlNode GetSettings(XmlDocument document)
    {
        return Settings.GetSettings(document);
    }

    public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
    {
        invalidator?.Invalidate(0, 0, width, height);
    }

    public int GetSettingsHashCode()
    {
        return Settings.GetSettingsHashCode();
    }

    public void Dispose()
    {
    }
}
