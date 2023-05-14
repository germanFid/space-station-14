using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.Actions;
using Robust.Shared.Containers;
using Content.Shared.DoAfter;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Werewolf;
using System.Linq;
using Content.Server.Polymorph.Systems;
using Content.Shared.Destructible;
using Content.Shared.Chemistry.Components;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Atmos;
using Content.Server.Atmos.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Content.Shared.Humanoid;
using Content.Server.Cloning;
using Content.Server.Actions;
using Content.Shared.Chat;
using Robust.Shared.Player;
using Content.Server.Chat.Managers;

namespace Content.Server.Werewolf
{
    public sealed partial class WerewolfSystem : EntitySystem
    {
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
        [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
        [Dependency] private readonly PuddleSystem _puddleSystem = default!;
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ActionsSystem _action = default!;
        [Dependency] private readonly IChatManager _chat = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<WerewolfComponent, ComponentStartup>(OnStartup);

            SubscribeLocalEvent<WerewolfComponent, WerewolfTransformationInHumanEvent>(OnTransformation);
            SubscribeLocalEvent<WerewolfComponent, WerewolfPupationActionEvent>(OnPupation);
            SubscribeLocalEvent<WerewolfComponent, WerewolfEatPupaActionEvent>(OnPupaEat);
            SubscribeLocalEvent<WerewolfComponent, WerewolfEvolveActionEvent>(OnEvolve);

            SubscribeLocalEvent<WerewolfComponent, WerewolfOnPupaEatDoAfterEvent>(OnDoAfterPupaEat);
            SubscribeLocalEvent<WerewolfComponent, WerewolfOnPupationDoAfterEvent>(OnDoAfterPupation);


            SubscribeLocalEvent<PupaComponent, DestructionEventArgs>(OnPupaBreak);
            SubscribeLocalEvent<WerewolfComponent, CloningEvent>(OnWerewolfCloning);
        }

        private void OnStartup(EntityUid uid, WerewolfComponent component, ComponentStartup args)
        {
            if (component.PupationAction != null)
                _actionsSystem.AddAction(uid, component.PupationAction, null);
            if (component.TransformationAction != null)
                _actionsSystem.AddAction(uid, component.TransformationAction, null);
            if (component.EatPupaAction != null)
                _actionsSystem.AddAction(uid, component.EatPupaAction, null);
        }

        private void OnWerewolfCloning(EntityUid uid, WerewolfComponent comp, ref CloningEvent args)
        {
            var action = new InstantAction(_prototypeManager.Index<InstantActionPrototype>("TransformToWerwolf"));
            var NewComp = EntityManager.AddComponent<WerewolfComponent>((EntityUid) args.Target);
            NewComp.PupasEaten = comp.PupasEaten;
            NewComp.TransformationAction = action;
            _action.AddAction(args.Target, action, null);
        }
        private void OnTransformation(EntityUid uid, WerewolfComponent component, WerewolfTransformationInHumanEvent args)
        {
            _polymorphSystem.Revert(uid, null);
            if (EntityManager.TryGetComponent(uid, out WerewolfComponent? uComp)
                && EntityManager.TryGetComponent(uComp.HumanUid, out WerewolfComponent? targetComp))
            {
                targetComp.PupasEaten = uComp.PupasEaten;
                if (targetComp.TransformationAction != null)
                    targetComp.TransformationAction.Cooldown = (_gameTiming.CurTime, _gameTiming.CurTime + TimeSpan.FromSeconds(180));
            }
        }

        private void OnEvolve(EntityUid uid, WerewolfComponent component, WerewolfEvolveActionEvent args)
        {
            var EvWerewolf = _polymorphSystem.PolymorphEntity(uid, "WerewolfEvolvePoly");
            _chat.ChatMessageToManyFiltered(Filter.Broadcast(), ChatChannel.Server,
                Loc.GetString("werewolf-evolve-msg"),
                Loc.GetString("werewolf-evolve-msg-wrapped"),
                EntityUid.Invalid, false, false, Color.OrangeRed, "/Audio/Werewolf/werewolfevolve.ogg");
        }

        private void OnPupation(EntityUid uid, WerewolfComponent component, WerewolfPupationActionEvent args)
        {
            if (args.Handled || component.PupationWhitelist?.IsValid(args.Target, EntityManager) != true)
                return;

            args.Handled = true;
            var target = args.Target;
            if (EntityManager.TryGetComponent(target, out MobStateComponent? targetState) && HasComp<HumanoidAppearanceComponent>(target))
            {
                switch (targetState.CurrentState)
                {
                    case MobState.Critical:
                    case MobState.Dead:
                        if (component.SoundPupation != null)
                        {
                            var SoundPar = component.SoundPupation.Params;
                            SoundPar.Volume = 3;
                            SoundPar.MaxDistance = 15;
                            _audioSystem.PlayPvs(component.SoundPupation, uid, SoundPar);
                        }
                        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(uid, component.PupationTime, new WerewolfOnPupationDoAfterEvent(), uid, target: target, used: uid)
                        {
                            BreakOnTargetMove = true,
                            BreakOnUserMove = true,
                        });
                        break;
                    default:
                        _popupSystem.PopupEntity(Loc.GetString("pupation-action-popup-message-fail-target-alive"), uid, uid);
                        break;

                }
            return;
            }
        }

        private void OnDoAfterPupation(EntityUid uid, WerewolfComponent component, DoAfterEvent args)
        {
            if (args.Handled || args.Cancelled)
                return;

            args.Handled = true;
            if (args.Args.Target != null)
            {
                var xform = Transform((EntityUid) args.Args.Target);
                var PupaUid = Spawn(component.PupaPrototype, xform.MapPosition);
                var PupaContainer = _containerSystem.MakeContainer<Container>(PupaUid, "pupa_container");
                Comp<PupaComponent>(PupaUid).pupaUid = PupaUid;
                PupaContainer.Insert(args.Args.Target.Value);
                Comp<PupaComponent>(PupaUid).PupaContainer = PupaContainer;
            }
        }

        private void OnDoAfterPupaEat(EntityUid uid, WerewolfComponent component, DoAfterEvent args)
        {
            if (args.Handled || args.Cancelled)
                return;
            args.Handled = true;
            if (args.Args.Target != null)
            {
                QueueDel(args.Args.Target.Value);
                Comp<WerewolfComponent>(uid).PupasEaten++;
                if (Comp<WerewolfComponent>(uid).PupasEaten == 3)
                {
                     if (component.WerewolfEvolveAction != null && component.TransformationAction != null)
                        _actionsSystem.AddAction(component.HumanUid, component.WerewolfEvolveAction, null);
                }
            }
        }

        private void OnPupaBreak (EntityUid uid, PupaComponent component, DestructionEventArgs args)
        {
            if(component.werewolf != EntityUid.Invalid && component.State != PupaState.Finished)
            {
                var EntUid = _containerSystem.EmptyContainer(component.PupaContainer, true);
               foreach (var EntittyUid in EntUid)
               {
                _containerSystem.RemoveEntity(EntittyUid, component.pupaUid);
               }
            }
            if (component.State == PupaState.Finished)
            {
                Solution bloodSolution = new();
                var tileMix = _atmosphereSystem.GetTileMixture(Transform(uid).GridUid, null, _transformSystem.GetGridOrMapTilePosition(uid), true);
                tileMix?.AdjustMoles(Gas.Miasma, 90f);
                bloodSolution.AddReagent("Blood", 200);
                _puddleSystem.TrySpillAt(uid, bloodSolution, out _);
            }

        }

        private void OnPupaEat(EntityUid uid, WerewolfComponent component, WerewolfEatPupaActionEvent args)
        {
            if (args.Handled || component.PupaEatWhitelist?.IsValid(args.Target, EntityManager) != true)
                return;

            args.Handled = true;
            var target = args.Target;
            if (Comp<PupaComponent>(target).State == PupaState.Finished)
            {
                if (component.SoundPupaEat != null)
                        {
                            var SoundPar = component.SoundPupaEat.Params;
                            SoundPar.Volume = 3;
                            SoundPar.MaxDistance = 15;
                            _audioSystem.PlayPvs(component.SoundPupaEat, uid, SoundPar);
                        }
                _doAfterSystem.TryStartDoAfter(new DoAfterArgs(uid, component.eatPupaTime, new WerewolfOnPupaEatDoAfterEvent(), uid, target: target, used: uid)
                        {
                            BreakOnTargetMove = true,
                            BreakOnUserMove = true,
                        });
            }
        }
        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var comp in EntityQuery<PupaComponent>().ToList())
            {
                if (comp.State != PupaState.Finished && comp.Accumulator >= comp.MaxAccumulator)
                {
                    if (comp.pupaUid != EntityUid.Invalid)
                    {
                        var xform = Transform(comp.pupaUid);
                        var NewPupaUid = Spawn(comp.PupaFinPrototype, xform.MapPosition);
                        var newpupacomp = Comp<PupaComponent>(NewPupaUid);
                        newpupacomp.State = PupaState.Finished;
                        newpupacomp.werewolf = comp.werewolf;
                        newpupacomp.pupaUid = comp.pupaUid;
                        newpupacomp.Accumulator = newpupacomp.MaxAccumulator;
                        QueueDel(comp.pupaUid);
                    }
                    else
                    {
                        comp.State = PupaState.Finished;
                    }
                }
                else if (comp.State != PupaState.Finished)
                {
                    comp.Accumulator += frameTime;
                }
            }
        }

    }
}
