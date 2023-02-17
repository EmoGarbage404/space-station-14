namespace Content.Shared.Silicons.Components;

[RegisterComponent]
public sealed class SiliconLegComponent : Component
{
    /// <summary>
    /// The entity this leg is installed into.
    /// Can be null.
    /// </summary>
    [DataField("installedEntity")]
    public EntityUid? InstalledEntity;
}
