using Content.Server.Chat.Managers;
using Content.Server.Roles;
using Content.Shared.Roles;


namespace Content.Server.Werewolf
{
    public sealed class WerewolfRole : Role
    {
        [Dependency] private readonly WerewolfSystem _werewolf = default!;
        public AntagPrototype Prototype { get; }

        public WerewolfRole(Mind.Mind mind, AntagPrototype antagPrototype) : base(mind)
        {
            Prototype = antagPrototype;
            Name = Loc.GetString(antagPrototype.Name);
            Antagonist = antagPrototype.Antagonist;
        }

        public override string Name { get; }
        public override bool Antagonist { get; }

        public void GreetWerewolf (EntityUid uid)
        {
            if (Mind.TryGetSession(out var session))
            {
                var chatMgr = IoCManager.Resolve<IChatManager>();
                chatMgr.DispatchServerMessage(session, Loc.GetString("werewolf-role-greeting"));
            }
        }
    }
}
