using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Silicons.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Silicons;

public sealed partial class SiliconSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    private ISawmill _sawmill = default!;

    private const string ModuleContainerId = "module-container";

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SiliconChassisComponent, ComponentStartup>(OnChassisStartup);
        InitializeModules();

        _sawmill = Logger.GetSawmill("silicons");
    }

    private void OnChassisStartup(EntityUid uid, SiliconChassisComponent component, ComponentStartup args)
    {
        component.ModuleContainer = _container.EnsureContainer<Container>(uid, ModuleContainerId);
    }
}
