using Robust.Shared.Serialization;

namespace Content.Shared.Wedding;

public partial class SharedWeddingRingSystem : EntitySystem
{

}

[Serializable, NetSerializable]
public sealed class MarriedArrayEvent : EntityEventArgs
{
    public IEnumerable<EntityUid>? Unmarried = default;

    public MarriedArrayEvent()
    {

    }

    public MarriedArrayEvent(IEnumerable<EntityUid>? unmarried)
    {
        Unmarried = unmarried;
    }
}
