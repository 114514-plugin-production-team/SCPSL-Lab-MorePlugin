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
    public class BCChat : ICommand
    {
        string ICommand.Command => "bc";

        string[] ICommand.Aliases => Array.Empty<string>();

        string ICommand.Description => "广播聊天";

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
                response = "用法:.bc 内容";
                return false;
            }
            string message = arguments.ElementAt(0);
            SSSChat.AddText(SSSChat.ChatModles.BCChat, 5f, message, player);
            response = "OK";
            return true;
        }
    }
}
