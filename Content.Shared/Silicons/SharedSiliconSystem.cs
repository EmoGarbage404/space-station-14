using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Movement.Systems;
using Content.Shared.Silicons.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Silicons;

public abstract partial class SharedSiliconSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;

    private ISawmill _sawmill = default!;

    private const string ModuleContainerId = "module-container";
    private const string LegContainerId = "leg-container";
    private const string MindReceptacleSlotId = "mind-receptacle-container-slot";

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SiliconChassisComponent, ComponentStartup>(OnChassisStartup);
        SubscribeLocalEvent<SiliconChassisComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SiliconChassisComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiers);

        SubscribeLocalEvent<SiliconChassisComponent, DamageModifyEvent>(RelayEventToParts);

        InitializeModules();
        InitializeLegs();

        _sawmill = Logger.GetSawmill("silicons");
    }

    private void OnChassisStartup(EntityUid uid, SiliconChassisComponent component, ComponentStartup args)
    {
        component.ModuleContainer = _container.EnsureContainer<Container>(uid, ModuleContainerId);
        component.LegContainer = _container.EnsureContainer<Container>(uid, LegContainerId);
        component.MindReceptacleSlot = _container.EnsureContainer<ContainerSlot>(uid, MindReceptacleSlotId);
    }

    private void OnMapInit(EntityUid uid, SiliconChassisComponent component, MapInitEvent args)
    {
        UpdateMovementSpeedFromLegs(uid, component);
        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }

    private void OnRefreshMovementSpeedModifiers(EntityUid uid, SiliconChassisComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(component.ChassisSlowdown, component.ChassisSlowdown);
    }

    private void RelayEventToParts(EntityUid uid, SiliconChassisComponent component, object args)
    {
        foreach (var module in component.ModuleContainer.ContainedEntities)
        {
            RaiseLocalEvent(module, args);
        }

        foreach (var leg in component.LegContainer.ContainedEntities)
        {
            RaiseLocalEvent(leg, args);
        }

        if (component.MindReceptacleSlot.ContainedEntity is { } mind)
            RaiseLocalEvent(mind, args);
    }
}
