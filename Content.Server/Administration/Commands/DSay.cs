using Content.Server.Chat.Systems;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class DSay : IConsoleCommand
    {
        public string Command => "dsay";

        public string Description => Loc.GetString("dsay-command-description");

        public string Help => Loc.GetString("dsay-command-help-text", ("command", Command));

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (shell.Player is not IPlayerSession player)
            {
                shell.WriteLine("shell-only-players-can-run-this-command");
                return;
            }

            if (player.AttachedEntity is not { Valid: true } entity)
                return;

            if (args.Length < 1)
                return;

            var message = string.Join(" ", args).Trim();
            if (string.IsNullOrEmpty(message))
                return;

            var entityManager = IoCManager.Resolve<IEntityManager>();
            var chat = entityManager.System<ChatSystem>();
            chat.TrySendInGameOOCMessage(entity, message, InGameOOCChatType.Dead, false, shell, player);
        }
    }
}
