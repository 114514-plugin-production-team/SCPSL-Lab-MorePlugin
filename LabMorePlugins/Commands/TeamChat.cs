using CommandSystem;
using LabApi.Features.Wrappers;
using LabMorePlugins.API;
using LabXWSPlugins.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class TeamChat : ICommand
    {
        string ICommand.Command => "TeamChat";

        string[] ICommand.Aliases => new string[] { "c" };

        string ICommand.Description => "队伍聊天";

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
                response = "用法:.c 内容";
                return false;
            }
            string message = arguments.ElementAt(0);
            TextChatSystem.SendMessage(player, message, TextChatSystem.MessageType.cMessage);
            response = "OK";
            return true;
        }
    }
}
