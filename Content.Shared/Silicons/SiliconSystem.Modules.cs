using System.Diagnostics.CodeAnalysis;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Components;
using Content.Shared.Silicons.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Silicons;

public sealed partial class SiliconSystem
{
    public void InitializeModules()
    {
        SubscribeLocalEvent<SiliconModuleComponent, EntGotInsertedIntoContainerMessage>(OnModuleInserted);
        SubscribeLocalEvent<SiliconModuleComponent, EntGotRemovedFromContainerMessage>(OnModuleRemoved);
    }

    private void OnModuleInserted(EntityUid uid, SiliconModuleComponent component, EntGotInsertedIntoContainerMessage args)
    {
        var chassisEnt = args.Container.Owner;
        if (!TryComp<SiliconChassisComponent>(chassisEnt, out var chassis))
            return;
        if (args.Container != chassis.ModuleContainer)
            return;

        TryAddModuleToEntity(uid, chassisEnt, component, chassis);
    }

    private void OnModuleRemoved(EntityUid uid, SiliconModuleComponent component, EntGotRemovedFromContainerMessage args)
    {
        var chassisEnt = args.Container.Owner;
        if (!TryComp<SiliconChassisComponent>(chassisEnt, out var chassis))
            return;
        if (args.Container != chassis.ModuleContainer)
            return;

        TryRemoveModuleFromEntity(uid, chassisEnt, component, chassis);
    }

    public bool TryAddModuleToEntity(EntityUid moduleEnt, EntityUid chassisEnt, SiliconModuleComponent? module = null, SiliconChassisComponent? chassis = null)
    {
        if (!Resolve(moduleEnt, ref module) || !Resolve(chassisEnt, ref chassis))
            return false;

        if (chassis.ModuleAmount >= chassis.MaxModules)
            return false;

        if (!chassis.ModuleWhitelist?.IsValid(moduleEnt) ?? false)
            return false;

        module.InstalledEntity = chassisEnt;
        chassis.ModuleContainer.Insert(moduleEnt);

        var xform = Transform(chassisEnt);
        foreach (var item in module.ProvidedItems)
        {
            if (!TryAddInnateTool(chassisEnt, item, out var itemEnt, chassis, xform))
            {
                _sawmill.Error($"Failed to install module item '{item}' into {ToPrettyString(chassisEnt)}");
                // TODO: make this error handling more graceful.
                continue;
            }
            module.ProvidedItemsEnts.Add(itemEnt.Value);
        }

        _actions.AddActions(chassisEnt, module.ProvidedActions, moduleEnt);

        //TODO: misc other borg abilities
        // implementation notes from dear emogarbage
        // - we need generic systems for adding armor/various upgrades
        // - logical way to do this is to steal from xenoarch
        // - allow modules to define components to just attach to chassis
        // - oh boy this sounds fun i hope there's no bugs :godo:

        return true;
    }

    public bool TryRemoveModuleFromEntity(EntityUid moduleEnt, EntityUid chassisEnt, SiliconModuleComponent? module = null, SiliconChassisComponent? chassis = null)
    {
        if (!Resolve(moduleEnt, ref module) || !Resolve(chassisEnt, ref chassis))
            return false;

        foreach (var item in new HashSet<EntityUid>(module.ProvidedItemsEnts))
        {
            if (!TryRemoveInnateTool(chassisEnt, item, chassis))
            {
                _sawmill.Error($"Failed to remove module item '{item}' into {ToPrettyString(chassisEnt)}");
                continue;
            }
            module.ProvidedItemsEnts.Remove(item);
        }

        _actions.RemoveProvidedActions(chassisEnt, moduleEnt);

        module.InstalledEntity = null;
        chassis.ModuleContainer.Remove(moduleEnt);
        return true;
    }

    private bool TryAddInnateTool(EntityUid uid, string tool, [NotNullWhen(true)] out EntityUid? toolEnt,
        SiliconChassisComponent? component = null, TransformComponent? xform = null)
    {
        toolEnt = null;
        if (!Resolve(uid, ref component, ref xform))
            return false;

        var item = Spawn(tool, xform.Coordinates);
        var handname = $"silicon-hand-{component.HandCounter}";
        _hands.AddHand(uid, handname, HandLocation.Middle);
        if (!_hands.TryPickup(uid, item, handname, false))
        {
            Del(item);
            return false;
        }

        toolEnt = item;
        AddComp<UnremoveableComponent>(item);
        return true;
    }

    private bool TryRemoveInnateTool(EntityUid uid, EntityUid tool, SiliconChassisComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        foreach (var hand in new HashSet<Hand>(_hands.EnumerateHands(uid)))
        {
            if (hand.IsEmpty || hand.HeldEntity != tool)
                continue;
            Del(tool);
            _hands.RemoveHand(uid, hand.Name);
            return true;
        }
        return false;
    }
}
