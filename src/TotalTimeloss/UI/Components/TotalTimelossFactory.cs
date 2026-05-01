using System;

using LiveSplit.Model;
using LiveSplit.UI.Components;

using TotalTimeloss.UI.Components;

[assembly: ComponentFactory(typeof(TotalTimelossFactory))]

namespace TotalTimeloss.UI.Components;

public class TotalTimelossFactory : IComponentFactory
{
    public string ComponentName => "Total Timeloss";

    public string Description => "Displays configurable Sum of Best, Total Timeloss, and Best Possible Time values.";

    public ComponentCategory Category => ComponentCategory.Information;

    public IComponent Create(LiveSplitState state)
    {
        return new TotalTimeloss(state);
    }

    public string UpdateName => ComponentName;

    public string XMLURL => "";

    public string UpdateURL => "";

    public Version Version => Version.Parse("1.0.0");
}
