using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;

namespace Content.Shared.Interaction;

public sealed class WhitelistUseableSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WhitelistUseableComponent, CanBeUsedAttemptEvent>(OnCanBeUsedAttempt);
        SubscribeLocalEvent<WhitelistUseableComponent, AttemptMeleeEvent>(OnMeleeAttempt);
    }

    private void OnCanBeUsedAttempt(EntityUid uid, WhitelistUseableComponent component, ref CanBeUsedAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!_whitelist.IsWhitelistPass(component.Whitelist, args.User))
            args.Cancel();
    }

    private void OnMeleeAttempt(EntityUid uid, WhitelistUseableComponent component, ref AttemptMeleeEvent args)
    {
        if (args.Cancelled)
            return;

        if (!_whitelist.IsWhitelistPass(component.Whitelist, args.User))
            args.Cancelled = true;
    }
}
