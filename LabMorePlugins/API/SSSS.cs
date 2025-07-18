using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Spawnpoints;
using PlayerRoles.Voice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoiceChat;
using YamlDotNet.Core.Tokens;

namespace LabMorePlugins.API
{
    public static class SSSS
    {
        public static void SetScale(this Player player, Vector3 vector3)
        {
            player.GameObject.transform.localScale = vector3;
        }
        public static bool IsRA(this Player player)
        {
            if (player.RemoteAdminAccess||player.ReferenceHub.isOwned)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static Vector3 GetRandomSpawnLocation(this RoleTypeId roleType)
        {
            if (!PlayerRoleLoader.TryGetRoleTemplate(roleType, out PlayerRoleBase @base))
                return Vector3.zero;

            if (!(@base is IFpcRole fpc))
                return Vector3.zero;

            ISpawnpointHandler spawn = fpc.SpawnpointHandler;
            if (spawn is null)
                return Vector3.zero;

            if (!spawn.TryGetSpawnpoint(out Vector3 pos, out float _))
                return Vector3.zero;

            return pos;
        }
        public static void setRankName(this Player player, string name, string color)
        {
            player.ReferenceHub.serverRoles.SetColor(color);
            player.ReferenceHub.serverRoles.SetText(name);
        }
        public static Pickup SpawnItem(this ItemType itemType, Vector3 pos)
        {
            Pickup pickup = Pickup.Create(itemType, pos);
            return pickup;
        }

        public static string GetRoleName(this RoleTypeId role)
        {
            switch (role)
            {
                case RoleTypeId.ClassD: return "D级人员";
                case RoleTypeId.Scientist: return "科学家";
                case RoleTypeId.FacilityGuard: return "设施警卫";
                case RoleTypeId.NtfPrivate:
                case RoleTypeId.NtfSergeant:
                case RoleTypeId.NtfSpecialist:
                case RoleTypeId.NtfCaptain:
                    return "九尾狐特遣队";
                case RoleTypeId.ChaosConscript:
                case RoleTypeId.ChaosRepressor:
                case RoleTypeId.ChaosRifleman:
                case RoleTypeId.ChaosMarauder:
                    return "混沌分裂者";
                case RoleTypeId.Scp049: return "SCP-049";
                case RoleTypeId.Scp0492: return "SCP-049-2";
                case RoleTypeId.Scp079: return "SCP-079";
                case RoleTypeId.Scp096: return "SCP-096";
                case RoleTypeId.Scp106: return "SCP-106";
                case RoleTypeId.Scp173: return "SCP-173";
                case RoleTypeId.Scp939: return "SCP-939";
                default: return role.ToString();
            }
        }
        public static PlayerData GetPlayerData(this Player player)
        {
            if (!Plugin.playerData.ContainsKey(player.UserId))
            {
                Plugin.playerData[player.UserId] = new PlayerData();
            }
            return Plugin.playerData[player.UserId];
        }
        public static void AddExp(this Player player, int 数值)
        {
            player.GetPlayerData().Exp += 数值;
        }
        public static void AddLevel(this Player player, int 数值)
        {
            player.GetPlayerData().Level += 数值;
        }
        public static void SetLevel(this Player player, int 数值)
        {
            player.GetPlayerData().Level = 数值;
        }
        public static void SetExp(this Player player, int 数值)
        {
            player.GetPlayerData().Exp = 数值;
        }
    }
}
