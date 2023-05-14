using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Werewolf;

[NetworkedComponent]
public abstract class SharedPupaComponent : Component
{
    [DataField("state")]
    public PupaState State = PupaState.Assimilating;
}

[Serializable, NetSerializable]
public enum PupaState : byte
{
    Assimilating,
    Finished,
}
