using Content.Server.Mind.Components;
using static Robust.Shared.Utility.SpriteSpecifier;
using Content.Shared.Wedding;

namespace Content.Server.Wedding;

public class WeddingRingSystem : SharedWeddingRingSystem
{
    public override void Initialize()
    {
        base.Initialize();

        // TODO: subscribe for couple change event
        SubscribeNetworkEvent<MarriedArrayEvent>(OnGetMarried);
    }

    [Obsolete]
    public IEnumerable<EntityUid> GetUnmarried()
    {
        List<EntityUid> married = new();
        foreach (var ring in EntityQuery<StoreUIDComponent>())
        {
            if (ring.UID != null)
            {
                married.Add(ring.Owner);
            }
        }


        foreach (var mind in EntityQuery<MindComponent>())
        {
            if (mind.Mind?.CurrentJob?.Name != "Priest" && !married.Contains(mind.Mind!.CurrentEntity!.Value))
            {
                yield return mind.Mind!.CurrentEntity.Value;

            }
        }
    }

    [Obsolete]
    public void OnGetMarried(MarriedArrayEvent message, EntitySessionEventArgs args)
    {
        message.Unmarried = GetUnmarried();

        RaiseNetworkEvent(message);
    }
}
