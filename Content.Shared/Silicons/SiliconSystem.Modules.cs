using System.Diagnostics.CodeAnalysis;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Silicons.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Silicons;

public abstract partial class SharedSiliconSystem
{
    public void InitializeModules()
    {
        SubscribeLocalEvent<SiliconModuleComponent, EntGotRemovedFromContainerMessage>(OnModuleRemoved);
        // TODO: replace with a ui or some shit
        SubscribeLocalEvent<SiliconModuleComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, SiliconModuleComponent component, AfterInteractEvent args)
    {
        if (args.Target is not { } target)
            return;
        args.Handled = TryAddModuleToEntity(args.Used, target, component);
    }

    private void OnModuleRemoved(EntityUid uid, SiliconModuleComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (component.InstalledEntity == null)
            return;
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

        if (module.InstalledEntity != null)
            return false;

        if (!chassis.ModuleWhitelist?.IsValid(moduleEnt) ?? false)
            return false;

        module.InstalledEntity = chassisEnt;

        var xform = Transform(chassisEnt);
        foreach (var item in module.ProvidedItems)
        {
            if (!TryAddInnateTool(chassisEnt, item, out var itemEnt, chassis, xform))
            {
                _sawmill.Error($"Failed to install module item '{item}' into {ToPrettyString(chassisEnt)}");
                // TODO: make this error handling more graceful.
                // it should probably attempt to undo the items
                // it has just added. do i care? BLEECH - emo
                return false;
            }
            module.ProvidedItemsEnts.Add(itemEnt.Value);
        }

        _actions.AddActions(chassisEnt, module.ProvidedActions, moduleEnt);

        var ev = new ModuleInstalledEvent(chassisEnt);
        RaiseLocalEvent(moduleEnt, ev);
        chassis.ModuleContainer.Insert(moduleEnt);
        return true;
    }

    public bool TryRemoveModuleFromEntity(EntityUid moduleEnt, EntityUid chassisEnt, SiliconModuleComponent? module = null, SiliconChassisComponent? chassis = null)
    {
        if (!Resolve(moduleEnt, ref module) || !Resolve(chassisEnt, ref chassis))
            return false;

        if (module.InstalledEntity == null)
            return false;

        foreach (var item in new HashSet<EntityUid>(module.ProvidedItemsEnts))
        {
            if (!TryRemoveInnateTool(chassisEnt, item, chassis))
            {
                _sawmill.Error($"Failed to remove module item '{item}' into {ToPrettyString(chassisEnt)}");
                return false;
            }
            module.ProvidedItemsEnts.Remove(item);
        }

        _actions.RemoveProvidedActions(chassisEnt, moduleEnt);

        module.InstalledEntity = null;
        var ev = new ModuleRemovedEvent(chassisEnt);
        RaiseLocalEvent(moduleEnt, ev);
        return chassis.ModuleContainer.Remove(moduleEnt);
    }

    private bool TryAddInnateTool(EntityUid uid,
        string? tool,
        [NotNullWhen(true)] out EntityUid? toolEnt,
        SiliconChassisComponent? component = null,
        TransformComponent? xform = null)
    {
        toolEnt = null;
        if (!Resolve(uid, ref component, ref xform))
            return false;

        var item = Spawn(tool, xform.Coordinates);
        var handname = $"silicon-hand-{component.HandCounter}";
        component.HandCounter++;
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
