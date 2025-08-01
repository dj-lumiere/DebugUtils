using DebugUtils.Repr.Records;

namespace DebugUtils.Repr;

public static partial class ReprExtensions
{


    public static ReprConfig GetContainerConfig(this ReprConfig config)
    {
        return config.ContainerReprMode switch
        {
            ContainerReprMode.UseParentConfig => config,
            ContainerReprMode.UseSimpleFormats => ReprConfig.ContainerDefaults,
            ContainerReprMode.UseCustomConfig => config.CustomContainerConfig ??
                                                 ReprConfig.ContainerDefaults,
            _ => ReprConfig.GlobalDefaults
        };
    }
}