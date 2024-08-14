using System.Linq;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Tiles;

public sealed class TileLayerGridDataSystem : EntitySystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDefMan = default!;
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

    public ProtoId<ContentTileDefinition>? RemoveTopMostTile(TileRef tileRef)
    {
        if (!TryComp<TileLayerGridDataComponent>(tileRef.GridUid, out var layerComp))
            return null;

        layerComp.TileStacks[tileRef.GridIndices].RemoveAt(0);
        return layerComp.TileStacks[tileRef.GridIndices].FirstOrNull();
    }

    public void AddTileToTop(TileRef tileRef, ContentTileDefinition toAdd)
    {
        if (!TryComp<TileLayerGridDataComponent>(tileRef.GridUid, out var layerComp))
            return;

        layerComp.TileStacks.GetOrNew(tileRef.GridIndices).Insert(0, toAdd.ID);
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
                    var tilePrototype = (ContentTileDefinition) _tileDefMan[curTile];

                    ent.Comp1.TileStacks[indices].Add(tilePrototype.ID);

                    if (tilePrototype.BaseTurf is { } baseTurf)
                    {
                        curTile = baseTurf;
                    }
                    else
                    {
                        curTile = null;
                    }
                }
            }

            // Add our map-specific tiles in
            if (false)
            {

            }
            else // Well it is a space game, i guess.
            {
                ent.Comp1.TileStacks[indices].Add(ContentTileDefinition.SpaceID);
            }

            // Validate the stack.
            var stackSize = ent.Comp1.TileStacks[indices].Count;
            for (var i = stackSize; i > 0; i--)
            {
                var curIndex = ent.Comp1.TileStacks[indices].Count - i;
                var curTile = ent.Comp1.TileStacks[indices][curIndex];

                if (ent.Comp1.TileStacks[indices].TryGetValue(curIndex + 1, out var nextTile))
                {
                    var curTilePrototype = (ContentTileDefinition) _tileDefMan[curTile];
                    var nextTilePrototype = (ContentTileDefinition) _tileDefMan[nextTile];

                    if (curTilePrototype.RequiredSubfloor.Count == 0)
                        continue;

                    if (curTilePrototype.RequiredSubfloor.Any(nextTilePrototype.Tags.Contains))
                        continue;

                    ent.Comp1.TileStacks[indices].RemoveAt(curIndex);
                }
            }
        }

        Dirty(ent);
    }
}
