using GameCore;
using LabApi.Features.Wrappers;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlayerStatsSystem.SyncedStatMessages;
using Paths = LabApi.Loader.Features.Paths.PathManager;
using Log = LabApi.Features.Console.Logger;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Extension;
using MEC;
using LabMorePlugins;

namespace LabXWSPlugins.API
{
    public class TextChatSystem
    {
        private static readonly List<ChatMessage> _messageHistory = new List<ChatMessage>();
        private const int MaxHistoryCount = 100;
        private const int DisplayCount = 4;
        private const float DisplayDuration = 6f;
        private static readonly string LogDirectory = Path.Combine(Plugin.ChatHistoryPath);
        public enum MessageType
        {
            bcMessage,
            acMessage,
            cMessage
        }
        public class ChatMessage
        {
            public DateTime Timestamp { get; }
            public string SenderNickname { get; }
            public string Content { get; }
            public MessageType Type { get; }
            public Team SenderTeam { get; }

            public ChatMessage(string senderNickname, string content, MessageType type, Team senderTeam)
            {
                Timestamp = DateTime.Now;
                SenderNickname = senderNickname;
                Content = content;
                Type = type;
                SenderTeam = senderTeam;
            }

            public string FormatForDisplay()
            {
                string formattedMessage;
                switch (Type)
                {
                    case MessageType.bcMessage:
                        formattedMessage = $"[全体聊天]|{SenderNickname}|{Content}";
                        break;
                    case MessageType.acMessage:
                        formattedMessage = $"[管理员求助]|{SenderNickname}|{Content}";
                        break;
                    case MessageType.cMessage:
                        formattedMessage = $"[阵营聊天]|{SenderNickname}|{Content}";
                        break;
                    default:
                        formattedMessage = $"[未知]|{SenderNickname}|{Content}";
                        break;
                }
                return formattedMessage.EndsWith("\n") ? formattedMessage : formattedMessage + "\n";
            }

            public string FormatForLog()
            {
                return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {FormatForDisplay()}";
            }
        }
        static TextChatSystem()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }
        public static void SendMessage(Player sender, string content, MessageType type)
        {
            if (sender == null || string.IsNullOrWhiteSpace(content))
                return;

            var message = new ChatMessage(sender.Nickname, content, type, sender.Team);
            _messageHistory.Insert(0, message);

            WriteMessageToLog(message);
            if (_messageHistory.Count > MaxHistoryCount)
            {
                _messageHistory.RemoveRange(MaxHistoryCount, _messageHistory.Count - MaxHistoryCount);
            }
            switch (type)
            {
                case MessageType.bcMessage:
                    HandleBroadcastMessage(sender, message);
                    break;
                case MessageType.acMessage:
                    HandleAdminChatMessage(sender, message);
                    break;
                case MessageType.cMessage:
                    HandleTeamChatMessage(sender, message);
                    break;
            }
        }
        private static void WriteMessageToLog(ChatMessage message)
        {
            try
            {
                string today = DateTime.Now.ToString("yyyy年MM月dd日");
                string logFilePath = Path.Combine(LogDirectory, $"{today}.txt");

                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(message.FormatForLog());
                }
            }
            catch (Exception ex)
            {
                Log.Error($"写入聊天日志失败: {ex}");
            }
        }
        private static void HandleBroadcastMessage(Player sender, ChatMessage message)
        {
            var recentMessages = GetRecentMessages(DisplayCount);

            foreach (var player in Player.List)
            {
                var messagesToShow = new List<string>();
                if (player == sender)
                {
                    messagesToShow.Add(message.FormatForDisplay());
                }
                messagesToShow.AddRange(recentMessages
                    .Where(m => m.Type == MessageType.bcMessage)
                    .Select(m => m.FormatForDisplay()));
                if (player.Team == sender.Team)
                {
                    messagesToShow.AddRange(recentMessages
                        .Where(m => m.Type == MessageType.cMessage)
                        .Select(m => m.FormatForDisplay()));
                }
                if (player.RemoteAdminAccess || player.IsHost)
                {
                    messagesToShow.AddRange(recentMessages
                        .Where(m => m.Type == MessageType.acMessage)
                        .Select(m => m.FormatForDisplay()));
                }

                if (messagesToShow.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var msg in messagesToShow.Distinct())
                    {
                        sb.Append(msg);
                    }

                    Hint hint = new Hint()
                    {
                        Alignment = HintServiceMeow.Core.Enum.HintAlignment.Right,
                        FontSize = 25,
                        YCoordinate = 150,
                        AutoText = a => sb.ToString(),
                    };
                    player.AddHint(hint);
                    hint.HideAfter(DisplayDuration);
                }
            }
        }
        private static void HandleAdminChatMessage(Player sender, ChatMessage message)
        {
            var recentAdminMessages = GetRecentMessages(DisplayCount, MessageType.acMessage);

            foreach (var player in Player.List.Where(p => p.RemoteAdminAccess || p.IsHost))
            {
                var sb = new StringBuilder();
                sb.Append(message.FormatForDisplay());

                foreach (var msg in recentAdminMessages.Select(m => m.FormatForDisplay()))
                {
                    sb.Append(msg);
                }

                Hint hint = new Hint()
                {
                    Alignment = HintServiceMeow.Core.Enum.HintAlignment.Right,
                    FontSize = 25,
                    YCoordinate = 150,
                    AutoText = a => sb.ToString(),
                };

                player.AddHint(hint);
                hint.HideAfter(DisplayDuration);
            }
        }
        private static void HandleTeamChatMessage(Player sender, ChatMessage message)
        {
            var recentTeamMessages = GetRecentMessages(DisplayCount, MessageType.cMessage);
            var recentAdminMessages = GetRecentMessages(DisplayCount, MessageType.acMessage);

            foreach (var player in Player.List)
            {
                var sb = new StringBuilder();

                if (player.Team == sender.Team)
                {
                    sb.Append(message.FormatForDisplay());

                    foreach (var msg in recentTeamMessages.Select(m => m.FormatForDisplay()))
                    {
                        sb.Append(msg);
                    }

                    if ((player.RemoteAdminAccess || player.IsHost) && player != sender)
                    {
                        foreach (var msg in recentAdminMessages.Select(m => m.FormatForDisplay()))
                        {
                            sb.Append(msg);
                        }
                    }
                }
                else if (player == sender && (player.RemoteAdminAccess || player.IsHost))
                {
                    foreach (var msg in recentAdminMessages.Select(m => m.FormatForDisplay()))
                    {
                        sb.Append(msg);
                    }
                }

                if (sb.Length > 0)
                {
                    Hint hint = new Hint()
                    {
                        Alignment = HintServiceMeow.Core.Enum.HintAlignment.Right,
                        FontSize = 25,
                        YCoordinate = 150,
                        AutoText = a => sb.ToString(),
                    };

                    player.AddHint(hint);
                    hint.HideAfter(DisplayDuration);
                }
            }
        }
        private static IEnumerable<ChatMessage> GetRecentMessages(int count, MessageType? filterType = null)
        {
            var query = _messageHistory.AsEnumerable();

            if (filterType.HasValue)
            {
                query = query.Where(m => m.Type == filterType.Value);
            }

            return query.Take(count);
        }

        public static void ClearHistory()
        {
            _messageHistory.Clear();
        }

        public static IReadOnlyList<ChatMessage> GetFullHistory()
        {
            return _messageHistory.AsReadOnly();
        }

        public static string[] GetLogFiles()
        {
            try
            {
                return Directory.GetFiles(LogDirectory, "*.txt")
                    .OrderByDescending(f => f)
                    .ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}

