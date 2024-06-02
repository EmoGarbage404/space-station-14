using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Interaction.Components;

/// <summary>
/// This is used for items that can only be used for interactions by entities that pass a whitelist.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(WhitelistUseableSystem))]
public sealed partial class WhitelistUseableComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();
}
