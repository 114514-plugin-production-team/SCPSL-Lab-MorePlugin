using GameCore;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlayerStatsSystem.SyncedStatMessages;
using Log = LabApi.Features.Console.Logger;

namespace LabMorePlugins.API
{
    public class ChatSystem
    {
        private static readonly List<ChatMessage> _chatHistory = new List<ChatMessage>();
        private static readonly Dictionary<Player, List<Hint>> _activeHints = new Dictionary<Player, List<Hint>>();
        private const float BaseYPosition = 100f;  // 初始Y坐标
        private const float LineHeight = 5f;     // 每行高度
        private const float MessageSpacing = 5f;  // 消息间距
        private const int MaxVisibleMessages = 100; // 最大显示消息数
        public enum ChatType
        {
            /// <summary>
            /// Chat privately with admins
            /// </summary>
            AdminPrivateChat,
            /// <summary>
            /// Chat with all players
            /// </summary>
            BroadcastChat,
            /// <summary>
            /// Chat with all teammates
            /// </summary>
            TeamChat
        }
        public class ChatMessage
        {
            public ChatType Type { get; }
            public Player Player { get; }
            public string Message { get; }
            public DateTime TimeSent { get; }
            public RoleTypeId Role { get; }
            public Team Team { get; }

            public ChatMessage(ChatType type, Player player, string message, RoleTypeId role, Team team)
            {
                Type = type;
                Player = player;
                Message = message ?? throw new ArgumentNullException(nameof(message));
                TimeSent = DateTime.Now;
                Role = role;
                Team = team;
            }

            public string FormatForDisplay()
            {
                string prefix = "";
                switch (Type)
                {
                    case ChatType.AdminPrivateChat:
                        prefix = "<color=red>[管理员私聊]</color>";
                        break;
                    case ChatType.BroadcastChat:
                        prefix = "<color=yellow>[全体聊天]</color>";
                        break;
                    case ChatType.TeamChat:
                        prefix = "<color=green>[队伍聊天]</color>";
                        break;
                }

                return $"{prefix} {Player.Nickname}: {Message}";
            }
        }

        public static void SendChatMessage(Player sender, string message, ChatType type)
        {
            if (sender == null || string.IsNullOrWhiteSpace(message))
                return;

            var chatMessage = new ChatMessage(
                type,
                sender,
                message,
                sender.Role,
                sender.Team
            );

            // 添加到历史记录
            _chatHistory.Insert(0, chatMessage);
            SaveTextHistory(chatMessage);
            // 显示消息
            DisplayChatMessage(chatMessage);

            Timing.CallDelayed(6f, () =>
            {
                _chatHistory.Remove(chatMessage);
                ClearHintsForMessage(chatMessage);
            });
        }
        private static void DisplayChatMessage(ChatMessage message)
        {
            IEnumerable<Player> recipients = GetRecipients(message);

            foreach (var player in recipients)
            {
                if (!_activeHints.TryGetValue(player, out var hints))
                {
                    hints = new List<Hint>();
                    _activeHints[player] = hints;
                }

                // 清除现有提示
                foreach (var hint in hints)
                {
                    player.RemoveHint(hint);
                }
                hints.Clear();

                // 获取该玩家应该看到的所有消息
                var messagesToShow = GetMessagesForPlayer(player).Take(MaxVisibleMessages).ToList();

                // 计算并显示每条消息
                float currentY = BaseYPosition;
                foreach (var msg in messagesToShow)
                {
                    currentY += LineHeight + MessageSpacing;
                    var hint = new Hint
                    {
                        Alignment = HintAlignment.Left,
                        YCoordinate = currentY,
                        FontSize = 20,
                        LineHeight = currentY += LineHeight + MessageSpacing,
                        AutoText = a =>
                        {
                            return msg.FormatForDisplay();
                        }
                    };
                    hint.HideAfter(6f);
                    player.AddHint(hint);
                    hints.Add(hint);
                }
            }
        }
        private static IEnumerable<ChatMessage> GetMessagesForPlayer(Player player)
        {
            return _chatHistory.Where(msg =>
                msg.Type == ChatType.BroadcastChat ||
                (msg.Type == ChatType.AdminPrivateChat && (player.ReferenceHub.isOwned || player.RemoteAdminAccess)) ||
                (msg.Type == ChatType.TeamChat && msg.Team == player.Team) ||
                msg.Player == player);
        }

        private static IEnumerable<Player> GetRecipients(ChatMessage message)
        {
            switch (message.Type)
            {
                case ChatType.BroadcastChat:
                    return Player.List;
                case ChatType.AdminPrivateChat:
                    return Player.List.Where(p => p.ReferenceHub.isOwned || p.RemoteAdminAccess || p == message.Player);
                case ChatType.TeamChat:
                    return Player.List.Where(p => p.Team == message.Team || p == message.Player);
                default:
                    return Enumerable.Empty<Player>();
            }
        }
        private static void ClearHintsForMessage(ChatMessage message)
        {
            foreach (var playerHints in _activeHints)
            {
                var hintsToRemove = playerHints.Value
                    .Where(h => h.Text == message.FormatForDisplay())
                    .ToList();

                foreach (var hint in hintsToRemove)
                {
                    playerHints.Key.RemoveHint(hint);
                    playerHints.Value.Remove(hint);
                }
            }
        }
        public static void SaveTextHistory(ChatMessage message)
        {
            try
            {
                // 确保目录存在
                string directoryPath = Plugin.ChatHistoryPath;
                // 创建/追加到当天的日志文件
                string Mode = message.FormatForDisplay();
                string filePath = Path.Combine(directoryPath, $"{DateTime.Now:yyyy年MM月dd日}.txt");
                string logEntry = $"[{DateTime.Now:yyyy年MM月dd日HH时mm分ss秒}][{message.Player.Nickname}][{message.Role.GetChineseName()}][{Mode}-{message.Message}]{Environment.NewLine}";

                File.AppendAllText(filePath, logEntry);
            }
            catch (Exception ex)
            {
                Log.Error($"保存聊天记录失败: {ex}");
            }
        }
    }
}
