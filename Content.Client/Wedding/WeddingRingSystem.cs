using Robust.Client.Graphics;
using Content.Shared.Wedding;
namespace Content.Client.Wedding;

public class WeddingRingSystem : EntitySystem
{
    public IEnumerable<EntityUid> Unmarried;

    public override void Initialize()
    {
        base.Initialize();

        // TODO: subscribe for couple change event
        SubscribeNetworkEvent<MarriedArrayEvent>(OnGetMarried);
    }

    public void CallGetUnmarried()
    {
        RaiseNetworkEvent(new MarriedArrayEvent());
    }

    public void OnGetMarried(MarriedArrayEvent message, EntitySessionEventArgs args)
    {
        if (message.Unmarried != null)
        {
            Unmarried = message.Unmarried;
        }
    }
}

public sealed class WeddingCoupleEntry
{
    public string Name;
    public Texture? Icon;

    public WeddingCoupleEntry(string name, Texture? icon)
    {
        Name = name;
        Icon = icon;
    }
}
