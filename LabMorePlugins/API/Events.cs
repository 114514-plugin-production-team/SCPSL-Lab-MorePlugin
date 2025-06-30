using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using LabMorePlugins.Enums;
using LabMorePlugins.Patchs;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.Spectating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace LabMorePlugins.API
{
    public class Events:CustomEventsHandler
    {
        public override void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleTypeId.Spectator)
            {
                Hint sphint = new Hint()
                {
                    AutoText = a =>
                    {
                        var p = Player.Get(a.PlayerDisplay.ReferenceHub);
                        if (p.RoleBase is SpectatorRole spectatorRole)
                        {
                            var spRole = Player.Get(spectatorRole.SyncedSpectatedNetId);
                            return $"你正在观看<color=red>{spRole.Nickname}</color>的操作\n已经查出他是[<color=yellow>Lv.{spRole.GetPlayerData().Level}</color>]入|角色为{spRole.Role.GetRoleName()}";
                        }
                        else
                        {
                            return "";
                        }
                    },
                    YCoordinate = 950,
                    Alignment = HintServiceMeow.Core.Enum.HintAlignment.Center,
                    FontSize = 25
                };
                ev.Player.GetPlayerDisplay().AddHint(sphint);
            }
        }
        public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            Plugin.Instance.SavePlayerData();
            Timing.CallDelayed(0.1f, () =>
            {
                Plugin.Instance.LoadPlayerData();
            });
            Hint hint = new Hint()
            {
                AutoText = arg =>
                {
                    var p = Player.Get(arg.PlayerDisplay.ReferenceHub);
                    return $"<color=33f3ff>你好{p.Nickname}|[Lv.{p.GetPlayerData().Level}]</color>";
                },
                YCoordinate = 1000,
                Alignment = HintServiceMeow.Core.Enum.HintAlignment.Center,
                FontSize = 20
            };
            Hint hint1 = new Hint()
            {
                AutoText = arg =>
                {
                    string round_time = Regex.Replace(Round.Duration.ToString(), "\\.\\d+$", string.Empty);
                    return $"<color=33f3ff>|Tps:{Server.Tps}/{Server.MaxTps}|回合时长[{round_time}]|</color>";
                },
                YCoordinate = 20,
                Alignment = HintServiceMeow.Core.Enum.HintAlignment.Center,
                FontSize = 20
            };
            ev.Player.GetPlayerDisplay().AddHint(hint);
            ev.Player.GetPlayerDisplay().AddHint(hint1);
        }
        public override void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
            if (ev.Attacker != null&&ev.Player!=null)
            {
                if (ev.Attacker.Role == PlayerRoles.RoleTypeId.Scp106)
                {
                    if (!SRoleSystem.IsRole(ev.Player.PlayerId, Enums.RoleName.SCP550)&&Plugin.Instance.Config.SCP106Pock)
                    {
                        ev.Player.EnableEffect(new CustomPlayerEffects.PocketCorroding());
                    }
                }
            }

        }
        public override void OnPlayerDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player != null&&ev.Attacker!=null)
            {
                ev.Player.setRankName("", "");
                ev.Player.SetScale(Vector3.one);
                ev.Player.DisableAllEffects();
                SCPChannelFixSecond.scpChannel.Remove(ev.Player.PlayerId);
                var killer = ev.Attacker;
                if (killer.IsHuman&&ev.Player.IsHuman)
                {
                    var data = Plugin.Instance.GetPlayerData(ev.Attacker.UserId);
                    data.Exp += Plugin.Instance.Config.PeopleKillSCP;
                }
                else if (killer.IsSCP&&ev.Player.IsHuman)
                {
                    var data = Plugin.Instance.GetPlayerData(ev.Attacker.UserId);
                    data.Exp += Plugin.Instance.Config.SCPKillPeople;
                }
                else if (killer.IsHuman&&ev.Player.IsSCP)
                {
                    var data = Plugin.Instance.GetPlayerData(ev.Attacker.UserId);
                    data.Exp += Plugin.Instance.Config.PeopleKillSCP;
                }
                var data2 = Plugin.Instance.GetPlayerData(killer.UserId);
                Plugin.Instance.CheckLevelUp(data2, killer);
                Plugin.Instance.SavePlayerData();
            }

        }
        public override void OnServerRoundEnded(RoundEndedEventArgs ev)
        {
            Plugin.Instance.SavePlayerData();
            Server.FriendlyFire = true;
            foreach (Player player in Player.List)
            {
                SRoleSystem.RemoveRole(player.PlayerId);
                player.SendHint("<size=30><color=blue>回合结束</color>\n<color=blue>[友伤]</color>已开启</size>", 15);
            }
            Round.IsLocked = false;
            Player mvpPlayer = null;
            int maxKills = 0;
            if (Plugin.killCounts.Any())
            {
                maxKills = Plugin.killCounts.Values.Max();
                var candidates = Plugin.killCounts
                    .Where(kvp => kvp.Value == maxKills)
                    .Select(kvp => kvp.Key)
                    .ToList();

                if (candidates.Any())
                {
                    int randomIndex = SAPI.Random.Next(0, candidates.Count);
                    mvpPlayer = candidates[randomIndex];
                }
                if (mvpPlayer == null)
                {
                    Server.SendBroadcast($"<color=blue>[回合结束]</color>\n===---------===\n<color=blue>[未能找到本局MVPQwQ]</color>", 10);
                    return;
                }
                var data = Plugin.Instance.GetPlayerData(mvpPlayer.UserId);
                data.Exp += 35;
                Server.SendBroadcast($"<color=blue>[回合结束]</color>\n===---------===\n<color=blue>本局MVP[{mvpPlayer.Nickname}]</color>", 10);
                Plugin.killCounts.Clear();
            }
        }
        public override void OnServerWaveRespawned(WaveRespawnedEventArgs ev)
        {
            if (ev.Wave.Faction == PlayerRoles.Faction.FoundationStaff)
            {
                Timing.CallDelayed(0.1f, () =>
                {
                    Server.SendBroadcast($"{Plugin.Instance.Config.SpawnNTF}", 6);
                });
                
            }
            else if (ev.Wave.Faction == PlayerRoles.Faction.FoundationEnemy)
            {
                Timing.CallDelayed(0.1f, () =>
                {
                    Server.SendBroadcast($"{Plugin.Instance.Config.SpawnCI}", 6);
                });
                
            }
        }
        public override void OnPlayerUsedItem(PlayerUsedItemEventArgs ev)
        {
            if (ev.Player!=null)
            {
                
            }
        }
        public override void OnPlayerInteractingDoor(PlayerInteractingDoorEventArgs ev)
        {
            foreach (var item in ev.Player.Items)
            {
                if (item.Base is InventorySystem.Items.Keycards.KeycardItem)
                {
                    
                }
            }
        }
    }
}
