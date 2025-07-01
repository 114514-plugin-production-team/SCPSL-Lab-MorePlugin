using CommandSystem;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class MyPos : ICommand
    {
        string ICommand.Command => "MyPosition";

        string[] ICommand.Aliases => new string[] { "mypos" };

        string ICommand.Description => "查看自己所在位置(没啥用)";

        bool ICommand.Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            CommandSender commandSender = sender as CommandSender;
            Player player = Player.Get(commandSender.SenderId);
            if (player == null)
            {
                response = "Null";
                return false;
            }
            response = $"你的位置[{player.Position}]";
            return true;
        }
    }
}
