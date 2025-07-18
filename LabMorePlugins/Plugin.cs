using HarmonyLib;
using HintServiceMeow.UI.Extension;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using LabMorePlugins.API;
using MapGeneration;
using MEC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;
using Version = System.Version;

namespace LabMorePlugins
{
    public class Plugin : Plugin<Class1>
    {
        public override string Name => "Lab综合插件";

        public override string Description => "Lab综合插件";

        public override string Author => "HUI(3145186196)";

        public override Version Version => new Version(1,0);
        public static Class1 config;
        public static CoroutineHandle CoroutineHandle;
        public override Version RequiredApiVersion => new Version(LabApi.Features.LabApiProperties.CompiledVersion);
        public static Dictionary<string, PlayerData> playerData = new Dictionary<string, PlayerData>();
        private string dataFilePath;
        public static string AdminLogs;
        public static readonly object KillCountsLock = new object();
        public static Dictionary<Player, int> killCounts = new Dictionary<Player, int>();
        public API.Events Events { get; set; } = new Events();
        public static Plugin Instance { get; private set; }
        public override void Enable()
        {
            Instance = this;
            LabApi.Events.CustomHandlers.CustomHandlersManager.RegisterEventsHandler(Events);
            string expFolder = Path.Combine(LabApi.Loader.Features.Paths.PathManager.LabApi.ToString(), "Exp");
            if (!Directory.Exists(expFolder))
                Directory.CreateDirectory(expFolder);

            dataFilePath = Path.Combine(expFolder, $"{Server.Port}-Exp.json");
            if(!File.Exists(dataFilePath))
            {
                File.Create(dataFilePath);
            }
            AdminLogs = Path.Combine(LabApi.Loader.Features.Paths.PathManager.LabApi.ToString(), "AdminLogs", Server.Port.ToString(), "Log.txt");
            if (!File.Exists(AdminLogs))
            {
                File.Create (AdminLogs);
            }
            LoadPlayerData();
            var harmony = new Harmony("hui.sl.moreplugin");
            harmony.PatchAll();
            PlayerEvents.Spawned += OnSpawned;
            PlayerEvents.Joined += OnJoined;
            PlayerEvents.Left += OnLeft;
            Logger.Debug("SCP-SL-Lab多功能插件已启动");
            Logger.Debug("本项目已开源GUTHUB:https://github.com/114514-plugin-production-team/SCPSL-Lab-MorePlugin");
            Logger.Debug("插件作者:灰(QQ:3145186196)||如有BUG可以联系QQ本人不经常看Github");
        }
        public override void Disable()
        {
            Instance = null;
            SavePlayerData();
            LabApi.Events.CustomHandlers.CustomHandlersManager.UnregisterEventsHandler(Events);
            PlayerEvents.Spawned -= OnSpawned;
            PlayerEvents.Joined -= OnJoined;
            PlayerEvents.Left -= OnLeft;
            Logger.Debug("SCP-SL-Lab多功能插件已关闭");
        }
        public void LoadPlayerData()
        {
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                playerData = JsonConvert.DeserializeObject<Dictionary<string, PlayerData>>(json) ?? new Dictionary<string, PlayerData>();
            }
        }

        public void SavePlayerData()
        {
            string json = JsonConvert.SerializeObject(playerData, Formatting.Indented);
            File.WriteAllText(dataFilePath, json);
        }

        public PlayerData GetPlayerData(string userId)
        {
            if (!playerData.ContainsKey(userId))
            {
                playerData[userId] = new PlayerData();
            }
            return playerData[userId];
        }


        public void AddExp(Player player, int exp)
        {
            var data = GetPlayerData(player.UserId);
            data.Exp += exp;
            SavePlayerData();
            UpdatePlayerNickname(player);
        }

        public void UpdatePlayerNickname(Player player)
        {
            var data = GetPlayerData(player.UserId);
            player.DisplayName = $"Lv.{data.Level}|{player.Nickname}";
        }
        public void CheckLevelUp(PlayerData data, Player player)
        {
            if (data.Exp >= config.LevelCanUp)
            {
                data.Level++;
                data.Exp -= config.LevelCanUp;
                UpdatePlayerNickname(player);
            }
        }
        public void OnSpawned(PlayerSpawnedEventArgs ev)
        {
            if(ev.Player!=null)
            {
                if (ev.Player.Role == PlayerRoles.RoleTypeId.ClassD)
                {
                    ev.Player.AddItem(ItemType.KeycardJanitor);
                }
                if (ev.Player.Role == PlayerRoles.RoleTypeId.FacilityGuard)
                {
                    ev.Player.ClearInventory();
                    ev.Player.AddItem(ItemType.GunE11SR);
                    ev.Player.AddItem(ItemType.ArmorLight);
                    ev.Player.AddItem(ItemType.KeycardMTFPrivate);
                    ev.Player.GetPlayerUi().CommonHint.ShowOtherHint("你得到了加强",6);
                }
            }
        }
        public void OnLeft(PlayerLeftEventArgs ev)
        {
            SavePlayerData();
        }
        public void OnJoined(PlayerJoinedEventArgs ev)
        {
            if (ev.Player!=null)
            {
                Logger.Info($"玩家{ev.Player.Nickname}加入服务器|Steam64ID为{ev.Player.UserId}");
            }
        }
    }
}
