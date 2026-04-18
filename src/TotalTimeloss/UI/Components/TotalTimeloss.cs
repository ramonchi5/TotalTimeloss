using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.UI;
using LiveSplit.UI.Components;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TotalTimeloss.UI.Components;

//[GlobalFontConsumer(GlobalFont.TimesFont | GlobalFont.TextFont)]
public class TotalTimeloss : IComponent
{
    protected LiveSplitState CurrentState { get; set; }
    public float PaddingTop => 0;
    public float PaddingLeft => 0;
    public float PaddingBottom => 0;
    public float PaddingRight => 0;


    public IDictionary<string, Action> ContextMenuControls => null;

    public TotalTimeloss(LiveSplitState state)
    {
        CurrentState = state;
    }

    private TimeSpan? GetBestPossibleTime(LiveSplitState state)
    {
        if (state.CurrentPhase is TimerPhase.Running or TimerPhase.Paused)
        {
            TimeSpan? delta = LiveSplitStateHelper.GetLastDelta(state, state.CurrentSplitIndex, BestSegmentsComparisonGenerator.ComparisonName, state.CurrentTimingMethod) ?? TimeSpan.Zero;
            TimeSpan? liveDelta = state.CurrentTime[state.CurrentTimingMethod] - state.CurrentSplit.Comparisons[BestSegmentsComparisonGenerator.ComparisonName][state.CurrentTimingMethod];
            if (liveDelta > delta)
            {
                delta = liveDelta;
            }

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
        var sob = SumOfBest.CalculateSumOfBest(state.Run, state.Settings.SimpleSumOfBest, true, state.CurrentTimingMethod) ?? new TimeSpan(0);
        var bpt = GetBestPossibleTime(state) ?? new TimeSpan(0);
        var tl = bpt - sob;

        var format = "d\\:hh\\:mm\\:ss\\.ff";
        var sobStr = sob.ToString(format);
        var bptStr = bpt.ToString(format);
        var tlStr  = tl.ToString(format);

        //don't have to calc the exact size, we can calc the theoretical max once
        //var theorethicalMax = "0:00:00:00.00";

        var sobMeasurement = g.MeasureString(sobStr, state.LayoutSettings.TextFont);


        //var pbtMeasurement = g.MeasureString(bptStr, state.LayoutSettings.TextFont);
        //var tlMeasurement = g.MeasureString(tlStr, state.LayoutSettings.TextFont);

        var minPad = 5f;
        var columnWidth = width / 3;

        VerticalHeight = (sobMeasurement.Height * 2) + (minPad * 2);
        

        var text = $"sob: {sob} tl {tl} bpt {bpt}";

        System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
        System.Drawing.SolidBrush white = new System.Drawing.SolidBrush(System.Drawing.Color.White);

        var measurement = g.MeasureString(text, state.LayoutSettings.TextFont);
        //VerticalHeight = 500;// measurement.Height;
        HorizontalWidth = width;// measurement.Width;

        g.FillRectangle(myBrush, 0, 0, HorizontalWidth, VerticalHeight);
        System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
        //g.DrawString(text, state.LayoutSettings.TextFont, white, 0, 0, drawFormat);

        g.DrawString("Sob", state.LayoutSettings.TextFont, white, minPad, minPad, drawFormat);
        g.DrawString("Tl" , state.LayoutSettings.TextFont, white, minPad + columnWidth + minPad, minPad, drawFormat);
        g.DrawString("Bpt", state.LayoutSettings.TextFont, white, minPad + columnWidth + minPad + columnWidth + minPad, minPad, drawFormat);

        g.DrawString(sobStr, state.LayoutSettings.TextFont, white, minPad, minPad + sobMeasurement.Height, drawFormat);
        g.DrawString(tlStr , state.LayoutSettings.TextFont, white, minPad + columnWidth + minPad, minPad + sobMeasurement.Height, drawFormat);
        g.DrawString(bptStr, state.LayoutSettings.TextFont, white, minPad + columnWidth + minPad + columnWidth + minPad, minPad + sobMeasurement.Height, drawFormat);



    }

    public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
    {
        
    }

    public float VerticalHeight{ get; set; }

    public float MinimumWidth => 20;

    public float HorizontalWidth { get; set; }

    public float MinimumHeight => 20;

    public string ComponentName => "Total timeloss";

    public Control GetSettingsControl(LayoutMode mode)
    {
        //Settings.Mode = mode;
        return null; // Settings;
    }

    public void SetSettings(System.Xml.XmlNode settings)
    {
        //Settings.SetSettings(settings);
    }

    public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
    {
        return new XmlDocument();// Settings.GetSettings(document);
    }

    public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
    {
        invalidator?.Invalidate(0, 0, width, height);
    }

    public int GetSettingsHashCode()
    {
        return 0;// Settings.GetSettingsHashCode();
    }

    public void Dispose()
    {
    }
}
