using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Werewolf;

[Serializable, NetSerializable]
public sealed class WerewolfOnPupaEatDoAfterEvent : SimpleDoAfterEvent
{
}
