using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;

using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using LiveSplit.TimeFormatters;
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

    public IDictionary<string, Action> ContextMenuControls => null;

    public TotalTimeloss(LiveSplitState state)
    {
        CurrentState = state;
        Settings = new TotalTimelossSettings();
    }

    // Formats a TimeSpan without leading zeros.
    // Examples: 27:34.96 | 1:05:22.10 | -3:12.00
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

        if (totalHours >= 1)
            return $"{sign}{totalHours}:{Math.Abs(t.Minutes):D2}:{Math.Abs(t.Seconds):D2}{decimals}";
        else
            return $"{sign}{Math.Abs(t.Minutes)}:{Math.Abs(t.Seconds):D2}{decimals}";
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
            // If fully transparent, let LiveSplit render its own background
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

    private void DrawTextWithEffects(Graphics g, string text, Font font, Color textColor, float x, float y, StringFormat format, LiveSplit.Options.LayoutSettings settings)
    {
        // Get shadow and outline settings from LayoutSettings
        var dropShadowsProp = settings.GetType().GetProperty("DropShadows", BindingFlags.Public | BindingFlags.Instance);
        var shadowsColorProp = settings.GetType().GetProperty("ShadowsColor", BindingFlags.Public | BindingFlags.Instance);
        var textOutlineColorProp = settings.GetType().GetProperty("TextOutlineColor", BindingFlags.Public | BindingFlags.Instance);

        bool hasShadow = (bool?)dropShadowsProp?.GetValue(settings) ?? false;
        Color shadowColor = (Color?)shadowsColorProp?.GetValue(settings) ?? Color.FromArgb(0, 0, 0, 0);
        Color outlineColor = (Color?)textOutlineColorProp?.GetValue(settings) ?? Color.FromArgb(0, 0, 0, 0);

        // Draw using GraphicsPath like SimpleLabel does
        if (g.TextRenderingHint == TextRenderingHint.AntiAlias && outlineColor.A > 0)
        {
            float fontSize = GetFontSize(g, font);
            using var outlineBrush = new SolidBrush(outlineColor);
            using var gp = new GraphicsPath();
            using var outline = new Pen(outlineColor, GetOutlineSize(fontSize)) { LineJoin = LineJoin.Round };

            if (hasShadow)
            {
                using var shadowBrush = new SolidBrush(shadowColor);
                gp.AddString(text, font.FontFamily, (int)font.Style, fontSize, new RectangleF(x + 1f, y + 1f, 9999, 9999), format);
                g.FillPath(shadowBrush, gp);
                gp.Reset();
                gp.AddString(text, font.FontFamily, (int)font.Style, fontSize, new RectangleF(x + 2f, y + 2f, 9999, 9999), format);
                g.FillPath(shadowBrush, gp);
                gp.Reset();
            }

            gp.AddString(text, font.FontFamily, (int)font.Style, fontSize, new RectangleF(x, y, 9999, 9999), format);
            g.DrawPath(outline, gp);
            using var textBrush = new SolidBrush(textColor);
            g.FillPath(textBrush, gp);
        }
        else
        {
            // Fallback to simple DrawString if outline is disabled or TextRenderingHint is not AntiAlias
            if (hasShadow)
            {
                using var shadowBrush = new SolidBrush(shadowColor);
                g.DrawString(text, font, shadowBrush, x + 1f, y + 1f, format);
                g.DrawString(text, font, shadowBrush, x + 2f, y + 2f, format);
            }

            using var textBrush = new SolidBrush(textColor);
            g.DrawString(text, font, textBrush, x, y, format);
        }
    }

    private float GetFontSize(Graphics g, Font font)
    {
        if (font.Unit == GraphicsUnit.Point)
        {
            return font.Size * g.DpiY / 72;
        }

        return font.Size;
    }

    private float GetOutlineSize(float fontSize)
    {
        return 2.1f + (fontSize * 0.055f);
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
        var tl  = bpt - sob;

        var sobStr = FormatTime(sob, Settings.Accuracy);
        var bptStr = FormatTime(bpt, Settings.Accuracy);
        var tlStr  = FormatTime(tl,  Settings.Accuracy);

        var minPad      = 5f;
        var columnWidth = width / 3f;

        // Measure one value to know how tall a text row is
        var rowHeight = g.MeasureString(sobStr, state.LayoutSettings.TextFont).Height;

        bool showLabels = Settings.Display2Rows;
        VerticalHeight = (showLabels ? rowHeight * 2 : rowHeight) + minPad * 2;
        HorizontalWidth = width;

        // --- Background ---
        DrawBackground(g, HorizontalWidth, VerticalHeight);

        // --- Colors ---
        Color labelColor = Settings.OverrideTextColor
            ? Settings.TextColor
            : state.LayoutSettings.TextColor;
        Color valueColor = Settings.OverrideTimeColor
            ? Settings.TimeColor
            : state.LayoutSettings.TextColor;

        var drawFormat = new StringFormat();
        float valueY = minPad;

        // Column X positions (left-aligned inside each third)
        float x0 = minPad;
        float x1 = columnWidth + minPad;
        float x2 = columnWidth * 2 + minPad;

        // --- Labels row (optional) ---
        if (showLabels)
        {
            DrawTextWithEffects(g, "SoB", state.LayoutSettings.TextFont, labelColor, x0, minPad, drawFormat, state.LayoutSettings);
            DrawTextWithEffects(g, "", state.LayoutSettings.TextFont, labelColor, x1, minPad, drawFormat, state.LayoutSettings);
            DrawTextWithEffects(g, "BPT", state.LayoutSettings.TextFont, labelColor, x2, minPad, drawFormat, state.LayoutSettings);
            valueY = minPad + rowHeight;
        }

        // --- Values row ---
        DrawTextWithEffects(g, sobStr, state.LayoutSettings.TimesFont, valueColor, x0, valueY, drawFormat, state.LayoutSettings);
        DrawTextWithEffects(g, "+" + tlStr, state.LayoutSettings.TimesFont, valueColor, x1, valueY, drawFormat, state.LayoutSettings);
        DrawTextWithEffects(g, bptStr, state.LayoutSettings.TimesFont, valueColor, x2, valueY, drawFormat, state.LayoutSettings);
    }

    public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
    {
    }

    public float VerticalHeight { get; set; }
    public float MinimumWidth   => 20;
    public float HorizontalWidth { get; set; }
    public float MinimumHeight  => 20;

    public string ComponentName => "Total Timeloss";

    public Control GetSettingsControl(LayoutMode mode)
    {
        Settings.Mode = mode;
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
