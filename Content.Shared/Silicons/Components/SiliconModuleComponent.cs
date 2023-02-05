using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Silicons.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed class SiliconModuleComponent : Component
{
    /// <summary>
    /// The entity this module is installed into.
    /// Can be null.
    /// </summary>
    [DataField("installedEntity")]
    public EntityUid? InstalledEntity;

    [DataField("providedItems", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> ProvidedItems = new();

    [DataField("providedItemsEnts")]
    public HashSet<EntityUid> ProvidedItemsEnts = new();

    [DataField("providedActions")]
    public HashSet<ActionType> ProvidedActions = new();
}
