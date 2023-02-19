using Content.Server.Mind.Components;
using Content.Shared.Silicons;
using Content.Shared.Silicons.Components;

namespace Content.Server.Silicons;

public sealed partial class SiliconSystem : SharedSiliconSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        InitializeMind();
    }

    public void InitializeMind()
    {

    }

    public bool TryAddMindReceptacleToEntity(EntityUid mindEnt, EntityUid chassisEnt, SiliconMindReceptacleComponent? mind = null, SiliconChassisComponent? chassis = null)
    {
        if (!Resolve(mindEnt, ref mind) || !Resolve(chassisEnt, ref chassis))
            return false;

        if (chassis.HasMindReceptacle)
            return false;

        if (mind.InstalledEntity != null)
            return false;

        if (!chassis.MindReceptacleWhitelist?.IsValid(mindEnt) ?? false)
            return false;

        mind.InstalledEntity = chassisEnt;
        chassis.MindReceptacleSlot.Insert(mindEnt);
        // TODO: this should be eventified
        CompOrNull<MindComponent>(mindEnt)?.Mind?.TransferTo(chassisEnt);
        return true;
    }

    public bool TryRemoveMindReceptacleFromEntity(EntityUid mindEnt, EntityUid chassisEnt, SiliconMindReceptacleComponent? mind = null, SiliconChassisComponent? chassis = null)
    {
        if (!Resolve(mindEnt, ref mind) || !Resolve(chassisEnt, ref chassis))
            return false;

        if (mind.InstalledEntity == null)
            return false;

        mind.InstalledEntity = null;
        if (!chassis.MindReceptacleSlot.Remove(mindEnt))
            return false;
        CompOrNull<MindComponent>(chassisEnt)?.Mind?.TransferTo(mindEnt);
        return true;
    }
}
