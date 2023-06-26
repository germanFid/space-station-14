using Content.Server.Damage.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Interaction.Events;
using Content.Shared.Mantrap;
using Content.Shared.StepTrigger.Systems;
using Content.Server.DoAfter;
using Content.Shared.DoAfter;

namespace Content.Server.Mantrap;

public sealed class MantrapSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MantrapComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<MantrapComponent, BeforeDamageUserOnTriggerEvent>(BeforeDamageOnTrigger);
        SubscribeLocalEvent<MantrapComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<MantrapComponent, TriggerEvent>(OnTrigger);

        SubscribeLocalEvent<MantrapComponent, ManTrapDoAfterEvent>(OnCharge);
    }

    private void OnUseInHand(EntityUid uid, MantrapComponent component, UseInHandEvent args)
    {
        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(args.User, component.ChargingTime, new ManTrapDoAfterEvent(), uid, target : uid, used: args.User)
        {
            BreakOnUserMove = true,
            BreakOnTargetMove = true,
        });
    }

    private void OnStepTriggerAttempt(EntityUid uid, MantrapComponent component, ref StepTriggerAttemptEvent args)
    {
        args.Continue |= component.IsActive;
    }

    private void BeforeDamageOnTrigger(EntityUid uid, MantrapComponent component, BeforeDamageUserOnTriggerEvent args)
    {
            // We need to stun someone, how got into mantrap
            // And maybe increase damage on mass
            // or if target is animal/space/werewolf
    }

    private void OnTrigger(EntityUid uid, MantrapComponent component, TriggerEvent args)
    {
        component.IsActive = false;
        UpdateVisuals(uid);
    }

     private void OnCharge(EntityUid uid, MantrapComponent component, DoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;
        component.IsActive = !component.IsActive;
        _popupSystem.PopupEntity(component.IsActive
            ? Loc.GetString("mantrap-on-activate")
            : Loc.GetString("mantrap-on-deactivate"),
            uid,
            args.User);

        UpdateVisuals(uid);
    }

    private void UpdateVisuals(EntityUid uid, MantrapComponent? mantrap = null, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref mantrap, ref appearance, false))
        {
            return;
        }

        _appearance.SetData(uid, MantrapVisuals.Visual,
            mantrap.IsActive ? MantrapVisuals.Armed : MantrapVisuals.Unarmed, appearance);
        Dirty(uid);
    }
}
