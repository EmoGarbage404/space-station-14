using Content.Shared.Interaction;
using Content.Shared.Movement.Components;
using Content.Shared.Silicons.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Silicons;

public sealed partial class SiliconSystem
{
    public void InitializeLegs()
    {
        SubscribeLocalEvent<SiliconLegComponent, EntGotRemovedFromContainerMessage>(OnLegRemoved);
        // TODO: replace with a ui or some shit
        SubscribeLocalEvent<SiliconLegComponent, AfterInteractEvent>(OnAfterInteractLeg);
    }

    private void OnAfterInteractLeg(EntityUid uid, SiliconLegComponent component, AfterInteractEvent args)
    {
        if (args.Target is not { } target)
            return;
        TryAddLegToEntity(args.Used, target, component);
    }

    private void OnLegRemoved(EntityUid uid, SiliconLegComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (component.InstalledEntity == null)
            return;
        var chassisEnt = args.Container.Owner;
        if (!TryComp<SiliconChassisComponent>(chassisEnt, out var chassis))
            return;
        if (args.Container != chassis.ModuleContainer)
            return;

        TryRemoveLegFromEntity(uid, chassisEnt, component, chassis);
    }

    public bool TryAddLegToEntity(EntityUid legEnt, EntityUid chassisEnt, SiliconLegComponent? leg = null, SiliconChassisComponent? chassis = null)
    {
        if (!Resolve(legEnt, ref leg) || !Resolve(chassisEnt, ref chassis))
            return false;

        if (chassis.LegAmount >= chassis.MaxLegs)
            return false;

        if (leg.InstalledEntity != null)
            return false;

        if (!chassis.LegWhitelist?.IsValid(legEnt) ?? false)
            return false;

        leg.InstalledEntity = chassisEnt;
        chassis.LegContainer.Insert(legEnt);
        UpdateMovementSpeedFromLegs(chassisEnt, chassis);
        return true;
    }

    public bool TryRemoveLegFromEntity(EntityUid legEnt, EntityUid chassisEnt, SiliconLegComponent? module = null, SiliconChassisComponent? chassis = null)
    {
        if (!Resolve(legEnt, ref module) || !Resolve(chassisEnt, ref chassis))
            return false;

        if (module.InstalledEntity == null)
            return false;

        module.InstalledEntity = null;
        var success = chassis.ModuleContainer.Remove(legEnt);
        if (success)
            UpdateMovementSpeedFromLegs(chassisEnt, chassis);
        return success;
    }

    private void UpdateMovementSpeedFromLegs(EntityUid uid, SiliconChassisComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var walkSpeed = 0f;
        var sprintSpeed = 0f;
        var acceleration = 0f;
        foreach (var leg in component.LegContainer.ContainedEntities)
        {
            if (!TryComp<MovementSpeedModifierComponent>(leg, out var legModifier))
                continue;

            walkSpeed += legModifier.BaseWalkSpeed;
            sprintSpeed += legModifier.BaseSprintSpeed;
            acceleration += legModifier.Acceleration;
        }

        walkSpeed /= component.MaxLegs;
        sprintSpeed /= component.MaxLegs;
        acceleration /= component.MaxLegs;
        _movementSpeedModifier.ChangeBaseSpeed(uid, walkSpeed, sprintSpeed, acceleration);
    }
}
