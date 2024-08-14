using Content.Shared.Maps;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Tiles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TileLayerGridDataComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<Vector2i, List<ProtoId<ContentTileDefinition>>> TileStacks = new();
}
