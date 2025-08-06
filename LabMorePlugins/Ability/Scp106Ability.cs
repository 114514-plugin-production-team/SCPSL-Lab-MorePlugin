using LabApi.Features.Wrappers;
using LabMorePlugins.API;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace LabMorePlugins.Ability
{
    public class Scp106Ability : BaseAbility
    {
        public override AbilityType Type { get; set; } = AbilityType.Toggle;
        public override int ID { get; set; } = 106;
        public override string Name { get; set; } = "Scp106回口袋";
        public override KeyCode KeyCode { get; set; } = KeyCode.Y;
        public override string Description { get; set; } = "Scp106回口袋";
        private const float CooldownDuration = 90f;
        // 存储玩家冷却状态
        private static readonly Dictionary<ReferenceHub, CoroutineHandle> _cooldowns = new Dictionary<ReferenceHub, CoroutineHandle>();
        protected override void EventForPlayer(Player player, SSKeybindSetting keybind)
        {
            if (keybind.SettingId != ID) return;
            if (player.Role != RoleTypeId.Scp106) return;

            if (_cooldowns.ContainsKey(player.ReferenceHub))
            {
                player.SendHint("<color=red>技能冷却中，请等待！</color>", 3f);
                return;
            }

            if (player.Room.Name != MapGeneration.RoomName.Pocket)
            {
                player.Position = Room.Get(MapGeneration.RoomName.Pocket).First().Position + Vector3.up;
                player.SendHint("<color=green>已进入口袋维度，20秒后自动返回！</color>", 5f);

                _cooldowns[player.ReferenceHub] = Timing.CallDelayed(CooldownDuration, () =>
                {
                    _cooldowns.Remove(player.ReferenceHub);
                    player.SendHint($"<color=#00FFFF>技能已就绪！按 [{KeyCode.ToString()}] 使用。</color>", 3f);
                });

                Timing.CallDelayed(20f, () =>
                {
                    if (player == null || !player.IsAlive) return;

                    if (Warhead.IsDetonated)
                    {
                        player.Position = RoleTypeId.NtfCaptain.GetRandomSpawnLocation();
                    }
                    else
                    {
                        player.Position = SAPI.GetRandomRoomByZone(MapGeneration.FacilityZone.Entrance).Position + Vector3.up;
                    }
                });
            }
        }
        public static void OnPlayerDeathOrRoleChange(ReferenceHub hub)
        {
            if (_cooldowns.TryGetValue(hub, out var coroutine))
            {
                Timing.KillCoroutines(coroutine);
                _cooldowns.Remove(hub);
            }
        }
    }
}
