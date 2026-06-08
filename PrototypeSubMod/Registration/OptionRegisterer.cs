using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options.Attributes;

namespace PrototypeSubMod.Registration;

public static class OptionRegisterer
{
    public static PrototypeOptions Options { get; } = OptionsPanelHandler.RegisterModOptions<PrototypeOptions>();
    
    [Menu("ProtoModOptions")]
    public class PrototypeOptions : ConfigFile
    {
        [Choice("ProtoPathfindingQuality", TooltipLanguageId = "PathfindingQualityTooltip")]
        public PathfindingQuality PathfindingQuality = PathfindingQuality.ProtoQualityLow;
    }

    public enum PathfindingQuality
    {
        ProtoQualityDisabled,
        ProtoQualityLow,
        ProtoQualityHigh
    }
}