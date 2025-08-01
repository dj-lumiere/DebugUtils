using DebugUtils.Repr.Records;

namespace DebugUtils.Repr;

public static partial class ReprExtensions
{
    /// <summary>
    /// Determines the appropriate configuration to use for formatting container contents
    /// based on the current configuration's container handling mode.
    /// </summary>
    /// <param name="config">The current representation configuration.</param>
    /// <returns>
    /// The configuration that should be used when formatting elements within containers.
    /// This may be the same configuration, a simplified version, or a custom configuration
    /// depending on the ContainerReprMode setting.
    /// </returns>
    /// <remarks>
    /// <para>Container configuration modes:</para>
    /// <list type="bullet">
    /// <item><description>UseParentConfig: Uses the same configuration as the parent</description></item>
    /// <item><description>UseSimpleFormats: Uses simplified formatting for container contents</description></item>
    /// <item><description>UseCustomConfig: Uses a user-specified custom configuration</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var config = new ReprConfig(ContainerReprMode: ContainerReprMode.UseSimpleFormats);
    /// var containerConfig = config.GetContainerConfig();
    /// // containerConfig will be ReprConfig.ContainerDefaults
    /// </code>
    /// </example>
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