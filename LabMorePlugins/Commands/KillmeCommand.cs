using CommandSystem;
using LabApi.Features.Wrappers;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class KillmeCommand : ICommand
    {
        public string Command => "killme";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "自杀命令";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
            Player player = Player.Get(playerCommandSender.SenderId);
            if (player == null)
            {
                response = "null";
                return false;
            }
            player.Kill("自杀");
            response = "OK";
            return true;
        }
    }
}
