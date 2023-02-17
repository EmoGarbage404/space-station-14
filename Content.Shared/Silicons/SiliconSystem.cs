using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Movement.Systems;
using Content.Shared.Silicons.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Silicons;

public sealed partial class SiliconSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;

    private ISawmill _sawmill = default!;

    private const string ModuleContainerId = "module-container";
    private const string LegContainerId = "leg-container";

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SiliconChassisComponent, ComponentStartup>(OnChassisStartup);
        SubscribeLocalEvent<SiliconChassisComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SiliconChassisComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiers);

        InitializeModules();
        InitializeLegs();

        _sawmill = Logger.GetSawmill("silicons");
    }

    private void OnChassisStartup(EntityUid uid, SiliconChassisComponent component, ComponentStartup args)
    {
        component.ModuleContainer = _container.EnsureContainer<Container>(uid, ModuleContainerId);
        component.LegContainer = _container.EnsureContainer<Container>(uid, LegContainerId);
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
}
