using CommandSystem;
using LabApi.Features.Wrappers;
using LabMorePlugins.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class ACChat : ICommand
    {
        string ICommand.Command => "RemoteAdminChat";

        string[] ICommand.Aliases => new string[] { "ac" };

        string ICommand.Description => "与管理员聊小秘密";

        bool ICommand.Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            CommandSender commandSender = sender as CommandSender;
            Player player = Player.Get(commandSender.SenderId);
            if (player == null)
            {
                response = "Null";
                return false;
            }
            if (arguments.Count < 1)
            {
                response = "用法:.ac 内容";
                return false;
            }
            string message = arguments.ElementAt(0);
            SSSChat.AddText(SSSChat.ChatModles.ACChat, 5f, message, player);
            response ="OK";
            return true;
        }
    }
}
