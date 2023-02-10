using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.Components;

[RegisterComponent, NetworkedComponent]
public sealed class SiliconChassisComponent : Component
{
    /// <summary>
    /// The maximum amount of modules that can be
    /// installed in this chassis
    /// </summary>
    [DataField("maxModules", required: true), ViewVariables(VVAccess.ReadWrite)]
    public int MaxModules;

    /// <summary>
    /// A container which holds all of the modules
    /// that are currently installed.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Container ModuleContainer = default!;

    /// <summary>
    /// The amount of modules that are currently installed
    /// </summary>
    public int ModuleAmount => ModuleContainer.ContainedEntities.Count;

    /// <summary>
    /// a small counter used to incremement the number of hands.
    /// this is used for getting unique hand names.
    /// </summary>
    [DataField("handCounter")]
    public int HandCounter;

    /// <summary>
    /// A whitelist determining what modules can be added.
    /// </summary>
    [DataField("moduleWhitelist")]
    public EntityWhitelist? ModuleWhitelist;
}
