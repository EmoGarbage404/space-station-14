using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.Components;

[RegisterComponent, NetworkedComponent]
public sealed class SiliconChassisComponent : Component
{
    #region Modules
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
    /// a counter used to get unique hand names.
    /// </summary>
    [DataField("handCounter")]
    public int HandCounter;

    /// <summary>
    /// A whitelist determining what modules can be added.
    /// </summary>
    [DataField("moduleWhitelist")]
    public EntityWhitelist? ModuleWhitelist;
    #endregion

    #region Legs
    /// <summary>
    /// The amount of legs that must be installed for the
    /// silicon to move at max speed.
    /// </summary>
    [DataField("maxLegs"), ViewVariables(VVAccess.ReadWrite)]
    public int MaxLegs = 2;

    /// <summary>
    /// A container which holds all of the modules
    /// that are currently installed.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Container LegContainer = default!;

    /// <summary>
    /// The amount of legs that are currently installed
    /// </summary>
    public int LegAmount => LegContainer.ContainedEntities.Count;

    /// <summary>
    /// A whitelist determining what legs can be added.
    /// </summary>
    [DataField("legWhitelist")]
    public EntityWhitelist? LegWhitelist;
    #endregion

    /// <summary>
    /// Intrinsic slowdown penalty from the chassis.
    /// Multiplicative
    /// </summary>
    [DataField("chassisSlowdown")]
    public float ChassisSlowdown = 1;
}
