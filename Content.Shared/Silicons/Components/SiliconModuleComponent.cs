using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Silicons.Components;

[RegisterComponent]
public sealed class SiliconModuleComponent : Component
{
    /// <summary>
    /// The entity this module is installed into.
    /// Can be null.
    /// </summary>
    [DataField("installedEntity")]
    public EntityUid? InstalledEntity;

    /// <summary>
    /// The items that installing this modules gives to the silicon
    /// </summary>
    [DataField("providedItems", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string?> ProvidedItems = new();

    /// <summary>
    /// All of the entities themselves which were provided by the module.
    /// Stored here for deletion later.
    /// </summary>
    [DataField("providedItemsEnts")]
    public HashSet<EntityUid> ProvidedItemsEnts = new();

    /// <summary>
    /// Actions that this module provides the silicon when installed
    /// </summary>
    [DataField("providedActions")]
    public HashSet<ActionType> ProvidedActions = new();
}
