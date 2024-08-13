using System.Linq;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Tiles;

public sealed class TileLayerGridDataSystem : EntitySystem
{
    // TODO: perf-wise. it's probably worth a reverse look dict for this just cause of the amount of times this gets modified.
    [Dependency] private readonly ITileDefinitionManager _tileDefMan = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GridInitializeEvent>(OnGridInit);

        SubscribeLocalEvent<TileLayerGridDataComponent, MapInitEvent>(OnMapInit);
    }

    private void OnGridInit(GridInitializeEvent ev)
    {
        EnsureComp<TileLayerGridDataComponent>(ev.EntityUid);
    }

    private void OnMapInit(Entity<TileLayerGridDataComponent> ent, ref MapInitEvent args)
    {
        if (TryComp<MapGridComponent>(ent, out var mapGrid))
            BuildInitialGridData((ent, ent, mapGrid));
    }

    public List<ProtoId<ContentTileDefinition>> GetTileData(TileRef tileRef)
    {
        if (!TryComp<TileLayerGridDataComponent>(tileRef.GridUid, out var layerComp))
            return new List<ProtoId<ContentTileDefinition>>();

        return layerComp.TileStacks[tileRef.GridIndices];
    }

    public void BuildInitialGridData(Entity<TileLayerGridDataComponent, MapGridComponent> ent)
    {
        var xform = Transform(ent);

        var mapBaseTurf = 1 == 0
            ? string.Empty
            : ContentTileDefinition.SpaceID ;

        var tileEnumerator = _map.GetAllTilesEnumerator(ent, ent);
        while (tileEnumerator.MoveNext(out var nullableTileRef))
        {
            // why the fuck can this even be null to begin with?
            if (nullableTileRef is not { } tileRef)
                continue;

            var indices = tileRef.GridIndices;
            ent.Comp1.TileStacks.GetOrNew(indices);

            // Build our stack of tiles
            {
                string? curTile = _tileDefMan[tileRef.Tile.TypeId].ID;
                while (curTile != null)
                {
                    var tilePrototype = _prototype.Index<ContentTileDefinition>(curTile);

                    ent.Comp1.TileStacks[indices].Add(tilePrototype.ID);

                    if (tilePrototype.BaseTurf is { } baseTurf)
                    {
                        curTile = baseTurf;
                    }
                    else if (curTile != mapBaseTurf)
                    {
                        curTile = mapBaseTurf;
                    }
                    else
                    {
                        curTile = null;
                    }
                }
            }

            // Validate the stack.
            var stackSize = ent.Comp1.TileStacks[indices].Count;
            for (var i = stackSize; i > 0; i--)
            {
                var curIndex = ent.Comp1.TileStacks[indices].Count - i;
                var curTile = ent.Comp1.TileStacks[indices][curIndex];

                if (ent.Comp1.TileStacks[indices].TryGetValue(curIndex + 1, out var nextTile))
                {
                    var curTilePrototype = _prototype.Index(curTile);
                    var nextTilePrototype = _prototype.Index(nextTile);

                    if (curTilePrototype.RequiredSubfloor.Count == 0)
                        continue;

                    if (curTilePrototype.RequiredSubfloor.Any(nextTilePrototype.Tags.Contains))
                        continue;

                    ent.Comp1.TileStacks[indices].RemoveAt(curIndex);
                }
            }
        }
    }
}
