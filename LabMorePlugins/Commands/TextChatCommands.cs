using CommandSystem;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabMorePlugins.API;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins.Commands
{
    public class TextChatCommands
    {
        [CommandHandler(typeof(ClientCommandHandler))]
        public class BC : ICommand
        {
            public string Command => "BroadcastChat";

            public string[] Aliases => new string[] { "bc" };

            public string Description => "全体聊天";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
                Player player = Player.Get(playerCommandSender.SenderId);
                if (player == null)
                {
                    response = "null";
                    return false;
                }
                if (arguments.Count < 1)
                {
                    response = "用法:.bc 内容";
                    return false;
                }
                string Message = string.Join("", arguments);
                ChatSystem.SendChatMessage(player, Message, ChatSystem.ChatType.BroadcastChat);
                Logger.Debug($"[Chat-BC]{player.Nickname}发送{Message}");
                response = "OK";
                return true;
            }
        }
        [CommandHandler(typeof(ClientCommandHandler))]
        public class TeamChat : ICommand
        {
            public string Command => "TeamChat";

            public string[] Aliases => new string[] { "c" };

            public string Description => "队伍聊天";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
                Player player = Player.Get(playerCommandSender.SenderId);
                if (player == null)
                {
                    response = "null";
                    return false;
                }
                if (arguments.Count < 1)
                {
                    response = "用法:.c 内容";
                    return false;
                }
                string Msg = string.Join("", arguments);
                ChatSystem.SendChatMessage(player, Msg, ChatSystem.ChatType.TeamChat);
                Logger.Debug($"[Chat-C]{player.Nickname}发送{Msg}");
                response = "OK";
                return true;
            }
        }
        [CommandHandler(typeof(ClientCommandHandler))]
        public class AC : ICommand
        {
            public string Command => "AdminChat";

            public string[] Aliases => new string[] { "ac" };

            public string Description => "管理员私聊";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
                Player player = Player.Get(playerCommandSender.SenderId);
                if (player == null)
                {
                    response = "null";
                    return false;
                }
                if (arguments.Count < 1)
                {
                    response = "用法:.ac 内容";
                    return false;
                }
                string Msg = string.Join("", arguments);
                ChatSystem.SendChatMessage(player, Msg, ChatSystem.ChatType.AdminPrivateChat);
                Logger.Debug($"[Chat-AC]{player.Nickname}发送{Msg}");
                response = "OK";
                return true;
            }
        }
    }
}
