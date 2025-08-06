using CustomPlayerEffects;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.UI.Extension;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.MicroHID.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp173Events;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using LabMorePlugins.Ability;
using LabMorePlugins.Enums;
using LabMorePlugins.Patchs;
using MapGeneration.Distributors;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.Spectating;
using PlayerRoles.Subroutines;
using PlayerStatsSystem;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace LabMorePlugins.API
{
    public class Events:CustomEventsHandler
    {
        public static bool IsLabby = false;
        public static List<ushort> SCP1068 = new List<ushort>();
        public static List<ushort> SCP1056 = new List<ushort>();
        public static List<ushort> HIDCDIY = new List<ushort>();
        public static System.Random Random = new System.Random();
        public override void OnPlayerEscaping(PlayerEscapingEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Player.Role == RoleTypeId.FacilityGuard)
            {
                if (Plugin.Instance.Config.FFcanEsx)
                {
                    ev.NewRole = Plugin.Instance.Config.BAESCRoleName;
                }
            }
        }
        public override void OnServerRoundStarted()
        {
            var SCP1056Item = ItemType.Medkit.SpawnItem(RoleTypeId.ClassD.GetRandomSpawnLocation());
            SCP1056.Add(SCP1056Item.Serial);
            var SCP1068Item = ItemType.SCP2176.SpawnItem(RoleTypeId.Scp096.GetRandomSpawnLocation());
            SCP1068.Add(SCP1068Item.Serial);
            var HIDCD = ItemType.Coin.SpawnItem(RoleTypeId.NtfCaptain.GetRandomSpawnLocation());
            HIDCD.Transform.localScale = new Vector3(10f, 5f, 5f);
            HIDCDIY.Add(HIDCD.Serial);
            var SCP181 = SAPI.GetRandomSpecialPlayer(RoleTypeId.ClassD);
            SRoleSystem.Add(RoleName.SCP181, SCP181.PlayerId);
            SCP181.setRankName("SCP-181", "pick");
            SCP181.SendHint("你是[SCP181]\n你的运气很好|概率打开一个权限门", 20);
            if (Player.List.Count >= 10)
            {
                var SCP131 = SAPI.GetRandomSpecialPlayer(RoleTypeId.ClassD);
                if (SCP131 !=null)
                {
                    SCP131.setRankName("SCP131", "yellow");
                    SCP131.Position = RoleTypeId.Scientist.GetRandomSpawnLocation();
                    SCP131.SetScale(new Vector3(0.5f, 0.5f, 0.5f));
                    SRoleSystem.Add(RoleName.SCP131, SCP131.PlayerId);
                    SCP131.MaxHealth = 60;
                    SCP131.SendHint("你是[SCP131]|SCP173会因为你无法移动\n你不能捡任何东西", 15);
                }
            }
            
        }
        public override void OnScp173AddingObserver(Scp173AddingObserverEventArgs ev)
        {
            if (SRoleSystem.IsRole(ev.Target.PlayerId, RoleName.SCP131))
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerPickingUpItem(PlayerPickingUpItemEventArgs ev)
        {
            if (ev.Player == null) return;
            if (HIDCDIY.Contains(ev.Pickup.Serial))
            {
                if (ev.Player.CurrentItem.Base is InventorySystem.Items.MicroHID.MicroHIDItem micro)
                {
                    if (micro.TryGetSubcomponent(out EnergyManagerModule energyManagerModule)&&energyManagerModule.Energy >=50)
                    {
                        energyManagerModule.ServerSetEnergy(ev.Player.CurrentItem.Serial, energyManagerModule.Energy + 20);
                    }
                }
            }
            if (SRoleSystem.IsRole(ev.Player.PlayerId, RoleName.SCP131))
            {
                ev.IsAllowed = false;
            }
        }
        public override void OnServerRoundStarting(RoundStartingEventArgs ev)
        {
            IsLabby = false;
            foreach (var item in Player.List)
            {
                item.SetRole(RoleTypeId.Spectator);
            }
        }
        public override void OnServerWaitingForPlayers()
        {
            IsLabby = true;
            short StartTime = GameCore.RoundStart.singleton.NetworkTimer;
            UnityEngine.GameObject.Find("StartRound").gameObject.transform.localScale = Vector3.zero;
            AdminToy.TryGet(new AdminToys.ShootingTarget(), out var adminToy);
            adminToy.Position = RoleTypeId.Tutorial.GetRandomSpawnLocation();
            adminToy.Spawn();
            Hint hint = new Hint()
            {
                AutoText = a =>
                {
                    string Infp = "";
                    if (IsLabby == true)
                    {
                        if (Player.List.Count >= 2)
                        {
                            Infp = $"回合即将开始|服务器人数[{Player.List.Count}]\n倒计时[{StartTime}]";
                        }
                        else
                        {
                            Infp = $"回合已锁|服务器人数[{Player.List.Count}]";
                        }
                    }
                    else if (Round.IsLobbyLocked)
                    {
                        Infp = $"回合已锁|服务器人数[{Player.List.Count}]";
                    }
                    else if (IsLabby)
                    {
                        Infp = $"";
                    }
                    return Infp;
                },
                YCoordinate = 100,
                Alignment = HintServiceMeow.Core.Enum.HintAlignment.Center,
                FontSize = 25,
            };
            foreach (var item in Player.List)
            {
                if (IsLabby == true)
                {
                    item.AddHint(hint);
                }
            }
        }
        public override void OnPlayerThrewItem(PlayerThrewItemEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (SCP1068.Contains(ev.Pickup.Serial))
            {
                Warhead.Shake();
            }
        }
        public override void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            Plugin.Instance.SavePlayerData();
            GameCore.Console.AddLog($"玩家{ev.Player.Nickname}离开了服务器|Steam64ID为{ev.Player.UserId}|已保存经验信息", Color.blue);
        }
        public override void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleTypeId.Spectator)
            {
                var TotalTime = SpawnProtected.SpawnDuration;
                var NtfSpawnWave = RespawnWaves.Get(new Respawning.Waves.NtfSpawnWave());
                var CISpawnWave = RespawnWaves.Get(new Respawning.Waves.ChaosSpawnWave());
                string NextTeam = "[不知道]";
                if (CISpawnWave.RespawnTokens == 0 && NtfSpawnWave.RespawnTokens > 0)
                {
                    NextTeam = "[<color=blue>九尾特遣队</color>]";
                }
                else if (NtfSpawnWave.RespawnTokens == 0 && CISpawnWave.RespawnTokens > 0)
                {
                    NextTeam = "[<color=green>混沌特遣队</color>]";
                }
                else if (NtfSpawnWave.RespawnTokens == 0 && CISpawnWave.RespawnTokens == 0)
                {
                    NextTeam = "[无可用支援]";
                }
                else if ((int)NtfSpawnWave.TimeLeft < (int)CISpawnWave.TimeLeft)
                {
                    NextTeam = "[<color=blue>九尾特遣队</color>]";
                }
                else if ((int)CISpawnWave.TimeLeft < (int)NtfSpawnWave.TimeLeft)
                {
                    NextTeam = "[<color=green>混沌特遣队</color>]";
                }
                else
                {
                    NextTeam = "[不知道]";
                }
                Hint hint = new Hint()
                {
                    AutoText = g =>
                    {
                        return $"你还剩[{TotalTime}]秒就可以去白给了\n下一波刷{NextTeam}\n九尾狐时间[{NtfSpawnWave.TimeLeft}]\n混沌时间[{CISpawnWave.TimeLeft}]";
                    },
                    YCoordinate = 600,
                    Alignment = HintServiceMeow.Core.Enum.HintAlignment.Right,
                    FontSize = 25
                };
                ev.Player.AddHint(hint);
            }
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
            if (ev.Player.Team == Team.SCPs)
            {
                Hint hint = new Hint()
                {
                    AutoText = g =>
                    {
                        string SCPInfo = "<color=red>------</color>\n";
                        int ZombieCount = 0;
                        foreach (Player player in Player.List)
                        {
                            if (player.Team == Team.SCPs)
                            {
                                switch(player.Role)
                                {
                                    case RoleTypeId.Scp049:
                                    case RoleTypeId.Scp173:
                                    case RoleTypeId.Scp096:
                                    case RoleTypeId.Scp106:
                                    case RoleTypeId.Scp939:
                                    case RoleTypeId.Scp3114:
                                        SCPInfo += $"{GetSCPName(player)}";
                                        break;
                                    default:
                                        break;
                                }
                                if (player.Role == RoleTypeId.Scp0492)
                                {
                                    ZombieCount++;
                                }
                                if (player.RoleBase is Scp079Role scp079)
                                {
                                    scp079.SubroutineModule.TryGetSubroutine(out Scp079AuxManager scp079AuxManager);
                                    scp079.SubroutineModule.TryGetSubroutine(out Scp079TierManager scp079TierManager);
                                    SCPInfo += $"<color=red>SCP079[在线] =>[Lv.{scp079TierManager.AccessTierLevel}|电力:{scp079AuxManager.CurrentAux}]\n</color>";
                                }
                            }
                        }
                        return SCPInfo + $"<color=red>\n------\n小僵尸数量[{ZombieCount}]</color>";
                    },
                };
            }
        }
        public static string GetSCPName(Player player)
        {
            return $"<color=red>[{player.Role}] => [HP:{player.Health} AHP:{player.ArtificialHealth}]</color>\n";
        }
        public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            if (IsLabby == true)
            {
                ev.Player.SetRole(RoleTypeId.Tutorial);
                ev.Player.AddItem(ItemType.GunFRMG0);
                ev.Player.AddItem(ItemType.Coin);

            }
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
                    if (Plugin.Instance.Config.SCP106Pock)
                    {
                        ev.Player.EnableEffect(new CustomPlayerEffects.PocketCorroding());
                    }
                    if (ev.DamageHandler.IsDamageType() == DamageType.Scp207)
                    {
                        ev.IsAllowed = false;
                    }
                    if (ev.DamageHandler.IsDamageType() == DamageType.Poison)
                    {
                        ev.IsAllowed = false;
                    }
                }
            }

        }
        public override void OnServerCommandExecuted(CommandExecutedEventArgs ev)
        {
            if (ev.CommandType == LabApi.Features.Enums.CommandType.RemoteAdmin)
            {
                Player player = Player.Get(ev.Sender.SenderId);
                var Time = DateTime.Now.ToString();
                string AdminLog = $"[{Time}|{ev.Sender.SenderId}|{ev.Sender.Nickname}|{player.IpAddress}|{ev.CommandName}]";
                File.AppendAllText(Plugin.AdminLogs, AdminLog);
            }
        }
        public override void OnPlayerDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player != null&&ev.Attacker!=null)
            {
                if (ev.Player.Role == RoleTypeId.Scp106)
                {
                    Scp106Ability.OnPlayerDeathOrRoleChange(ev.Player.ReferenceHub);
                }
                if (SAPI.PlayerAudioPlayers.TryGetValue(ev.Player, out _))
                {
                    SAPI.RemovePlayerAudio(ev.Player);
                }
                SRoleSystem.RemoveRole(ev.Player.PlayerId);
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
        public override void OnPlayerReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
        {
            if(ev.FirearmItem.Base.TryGetSubcomponent(out Firearm firearm))
            {
                var Max = firearm.GetTotalMaxAmmo();
                var Total = firearm.GetTotalStoredAmmo();
                if (Max > Total)
                {
                    ev.Player.AddAmmo(ev.FirearmItem.Type, 55);
                }
            }
        }
        public override void OnPlayerUsedItem(PlayerUsedItemEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (SCP1056.Contains(ev.UsableItem.Serial))
            {
                ev.Player.SetScale(new Vector3(0.5f, 0.5f, 0.5f));
                ev.Player.SendHint("变小了awa", 5);
            }
        }
        public override void OnPlayerInteractingDoor(PlayerInteractingDoorEventArgs ev)
        {
            if (ev.Player == null) return;
            foreach (var item in ev.Player.Items)
            {
                if (item.Base is InventorySystem.Items.Keycards.KeycardItem keycardItem)
                {
                    if (SAPI.CheckKeycardAccess(keycardItem, ev.Door.Permissions))
                    {
                        ev.IsAllowed = true;
                    }
                }
            }
            if (SRoleSystem.IsRole(ev.Player.PlayerId, RoleName.SCP181) && Random.Next(1, 5) >= 2 && !ev.Door.IsLocked)
            {
                ev.Door.IsOpened = true;
                ev.Player.GetPlayerUi().CommonHint.ShowOtherHint("你打开了这道权限门", 6);
            }
        }
        public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev)
        {
            if (ev.OldRole == RoleTypeId.Scp106)
            {
                Scp106Ability.OnPlayerDeathOrRoleChange(ev.Player.ReferenceHub);
            }
        }
    }
}
