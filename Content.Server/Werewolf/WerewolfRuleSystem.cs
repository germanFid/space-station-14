using Content.Server.Actions;
using Content.Server.Mind.Components;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Content.Server.Werewolf;
using Content.Shared.Werewolf;
using Content.Server.Polymorph.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Audio;
using Content.Server.Popups;
using Content.Shared.Damage;

namespace Content.Server.GameTicking.Rules;

public sealed class WerewolfRuleSystem : GameRuleSystem<WerewolfComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WerewolfComponent, TransformToWerwolfEvent>(OnWerewolfTransform);
    }
    public void MakeWerewolfPlayer (MindComponent mind)
    {
        if (mind.Mind!.OwnedEntity != null && !EntityManager.HasComponent<WerewolfComponent>((EntityUid) mind.Mind.OwnedEntity))
        {

            var action = new InstantAction(_prototypeManager.Index<InstantActionPrototype>("TransformToWerwolf"));
            _action.AddAction(mind.Mind.OwnedEntity.Value, action, null);
            action.Cooldown = (_gameTiming.CurTime, _gameTiming.CurTime + TimeSpan.FromSeconds(900));
            var antagPrototype = _prototypeManager.Index<AntagPrototype>("Werewolf");
            var werewolfRole = new WerewolfRole(mind.Mind, antagPrototype);
            mind.Mind.AddRole(werewolfRole);
            var _addedSound = new SoundPathSpecifier("/Audio/Werewolf/werewolfrolesign.ogg");
            _audioSystem.PlayGlobal(_addedSound, (EntityUid) mind.Mind.OwnedEntity);
            werewolfRole.GreetWerewolf((EntityUid) mind.Mind.OwnedEntity);
            EntityManager.AddComponent<WerewolfComponent>((EntityUid) mind.Mind.OwnedEntity);//.TransformationAction = action;
        }
    }

    private void OnWerewolfTransform(EntityUid uid, WerewolfComponent component, TransformToWerwolfEvent args)
    {
        var WerewolfEnt = _polymorphSystem.PolymorphEntity(uid, "WerewolfPoly");
        if (EntityManager.TryGetComponent(WerewolfEnt, out WerewolfComponent? targetComp)
                && EntityManager.TryGetComponent(uid, out WerewolfComponent? uComp))
        {
            targetComp.HumanUid = uid;
            targetComp.PupasEaten = uComp.PupasEaten;
            _audioSystem.PlayPvs("/Audio/Werewolf/werewolftransformsound.ogg", WerewolfEnt.Value);
            if (targetComp.TransformationAction != null)
                targetComp.TransformationAction.Cooldown = (_gameTiming.CurTime, _gameTiming.CurTime + TimeSpan.FromSeconds(180));
            _popupSystem.PopupEntity(Loc.GetString("transform-to-werewolf-popup"), (EntityUid) WerewolfEnt, (EntityUid) WerewolfEnt);
        }


    }
}
