using Content.Server.Atmos.Piping.Trinary.Components;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.Atmos.Piping;
using Content.Shared.Audio;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.Atmos.Piping.Trinary.EntitySystems
{
    [UsedImplicitly]
    public sealed class GasSwitchSystem : EntitySystem
    {
        [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GasSwitchComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<GasSwitchComponent, ActivateInWorldEvent>(OnActivate);
            SubscribeLocalEvent<GasSwitchComponent, ExaminedEvent>(OnExamined);
        }

        private void OnExamined(EntityUid uid, GasSwitchComponent valve, ExaminedEvent args)
        {
            if (!Comp<TransformComponent>(valve.Owner).Anchored || !args.IsInDetailsRange) // Not anchored? Out of range? No status.
                return;

            if (Loc.TryGetString("t-valve-examined", out var str,
                        ("statusColor", valve.Switch ? "green" : "orange"),
                        ("open", valve.Switch)
            ))
                args.PushMarkup(str);
        }

        private void OnStartup(EntityUid uid, GasSwitchComponent component, ComponentStartup args)
        {
            // We call set in startup so it sets the appearance, node state, etc.
            Set(uid, component, component.Switch);
        }

        private void OnActivate(EntityUid uid, GasSwitchComponent component, ActivateInWorldEvent args)
        {
            Toggle(uid, component);
            SoundSystem.Play(component.ValveSound.GetSound(), Filter.Pvs(component.Owner), component.Owner, AudioHelpers.WithVariation(0.25f));
        }

        public void Set(EntityUid uid, GasSwitchComponent component, bool value)
        {
            component.Switch = value;
            if (TryComp(uid, out NodeContainerComponent? nodeContainer)
                && nodeContainer.TryGetNode(component.Inlet1Name, out PipeNode? inlet1)
                && nodeContainer.TryGetNode(component.Inlet2Name, out PipeNode? inlet2)
                && nodeContainer.TryGetNode(component.OutletName, out PipeNode? outlet))
            {
                if (TryComp<AppearanceComponent>(component.Owner,out var appearance))
                {
                    _appearance.SetData(uid, FilterVisuals.Enabled, component.Switch, appearance);
                }
                if (component.Switch)
                {
                    inlet1.AddAlwaysReachable(outlet);
                    outlet.AddAlwaysReachable(inlet1);
                    inlet2.RemoveAlwaysReachable(outlet);
                    outlet.RemoveAlwaysReachable(inlet2);
                    _ambientSoundSystem.SetAmbience(component.Owner, true);
                }
                else
                {
                    inlet2.AddAlwaysReachable(outlet);
                    outlet.AddAlwaysReachable(inlet2);
                    inlet1.RemoveAlwaysReachable(outlet);
                    outlet.RemoveAlwaysReachable(inlet1);
                    _ambientSoundSystem.SetAmbience(component.Owner, false);
                }
            }
        }

        public void Toggle(EntityUid uid, GasSwitchComponent component)
        {
            Set(uid, component, !component.Switch);
        }
    }
}
