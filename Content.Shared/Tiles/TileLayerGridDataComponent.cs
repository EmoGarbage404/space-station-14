using Content.Shared.Maps;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Tiles;

[RegisterComponent, NetworkedComponent]
public sealed partial class TileLayerGridDataComponent : Component
{
    [DataField]
    public Dictionary<Vector2i, List<ProtoId<ContentTileDefinition>>> TileStacks = new();
}
