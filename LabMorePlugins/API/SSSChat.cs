using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins.API
{
    public class SSSChat
    {
        public enum ChatModles
        {
            BCChat,
            TeamChat,
            ACChat
        }
        private static readonly List<TextEntry> TextQueue = new List<TextEntry>();
        private static DateTime lastUpdateTime = DateTime.MinValue;
        public static void AddText(ChatModles mode, float duration, string content, Player sender = null)
        {
            lock (TextQueue)
            {
                TextQueue.Add(new TextEntry
                {
                    Mode = mode,
                    Content = content,
                    DisplayTime = duration,
                    AddTime = DateTime.Now,
                    Sender = sender
                });
            }
        }
        public static void UpdateDisplay()
        {
            if ((DateTime.Now - lastUpdateTime).TotalSeconds < 1)
                return;

            lastUpdateTime = DateTime.Now;

            lock (TextQueue)
            {
                var expiredMessages = TextQueue.Where(x =>
                    (DateTime.Now - x.AddTime).TotalSeconds >= x.DisplayTime).ToList();

                foreach (var msg in expiredMessages)
                {
                    TextQueue.Remove(msg);
                }
                ProcessMessagesForPlayers(Player.List, ChatModles.BCChat);

                foreach (var teamGroup in TextQueue
                    .Where(x => x.Mode == ChatModles.TeamChat)
                    .GroupBy(x => x.Sender?.Team))
                {
                    if (teamGroup.Key == null) continue;

                    var teamPlayers = Player.List.Where(p => p.Team == teamGroup.Key);
                    ProcessMessagesForPlayers(teamPlayers, ChatModles.TeamChat);
                }
                var admins = Player.List.Where(p => p.ReferenceHub.serverRoles.RemoteAdmin);
                ProcessMessagesForPlayers(admins, ChatModles.ACChat);
            }
        }
        private static void ProcessMessagesForPlayers(IEnumerable<Player> players, ChatModles mode)
        {
            var messages = TextQueue
                .Where(x => x.Mode == mode)
                .OrderBy(x => x.AddTime)
                .ToList();

            if (!messages.Any())
                return;

            foreach (var player in players)
            {
                if (mode == ChatModles.ACChat)
                {
                    if (!player.ReferenceHub.serverRoles.RemoteAdmin)
                        continue;

                    string combinedContent = string.Join("\n", messages.Select(msg =>
                        FormatContent(msg.Mode, msg.Content, msg.Sender)));
                    Hint hint = new Hint()
                    {
                        Text = combinedContent,
                        Alignment = HintServiceMeow.Core.Enum.HintAlignment.Left,
                        YCoordinate = 200,
                        FontSize = 20
                    };
                    player.AddHint(hint);
                    Timing.CallDelayed(5f, () =>
                    {
                        player.RemoveHint(hint);
                    });
                }
                else
                {
                    string combinedContent = string.Join("\n", messages.Select(msg =>
                        FormatContent(msg.Mode, msg.Content, msg.Sender)));
                    Hint hint = new Hint()
                    {
                        Text = combinedContent,
                        Alignment = HintServiceMeow.Core.Enum.HintAlignment.Left,
                        YCoordinate = 200,
                        FontSize = 20
                    };
                    player.AddHint(hint);
                    Timing.CallDelayed(5f, () =>
                    {
                        player.RemoveHint(hint);
                    });
                }

            }
        }
        private static string FormatContent(ChatModles mode, string content, Player sender)
        {
            string prefix;
            switch (mode)
            {
                case ChatModles.BCChat:
                    prefix = "[广播]";
                    break;
                case ChatModles.TeamChat:
                    prefix = "[阵营消息]";
                    break;
                case ChatModles.ACChat:
                    prefix = "[管理员求助]";
                    break;
                default:
                    prefix = "[消息]";
                    break;
            }
            return $"[{sender?.Nickname}][{prefix}]:{content}";
        }
        public class TextEntry
        {
            public ChatModles Mode { get; set; }
            public string Content { get; set; }
            public float DisplayTime { get; set; }
            public DateTime AddTime { get; set; }
            public Player Sender { get; set; }
        }
    }
}
