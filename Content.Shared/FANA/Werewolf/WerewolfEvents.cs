using Content.Shared.Actions;

namespace Content.Shared.Werewolf;

public readonly struct EntityWerewolfTransformationEvent
{
    public readonly EntityUid Target;

    public EntityWerewolfTransformationEvent(EntityUid target)
    {
        Target = target;
    }
};

public sealed class TransformToWerwolfEvent : InstantActionEvent { };
