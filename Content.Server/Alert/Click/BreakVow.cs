using Content.Server.Abilities.Mime;
using Content.Shared.Alert;

namespace Content.Server.Alert.Click;

///<summary>
/// Break your mime vows
///</summary>
[DataDefinition]
public sealed class BreakVow : IAlertClick
{
    public void AlertClicked(EntityUid player)
    {
        var entManager = IoCManager.Resolve<IEntityManager>();

        if (entManager.TryGetComponent<MimePowersComponent?>(player, out var mimePowers))
        {
            entManager.System<MimePowersSystem>().BreakVow(player, mimePowers);
        }
    }
}
