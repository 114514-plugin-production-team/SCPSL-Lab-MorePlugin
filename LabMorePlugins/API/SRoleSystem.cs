using LabApi.Features.Wrappers;
using LabMorePlugins.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins.API
{
    public class SRoleSystem
    {
        private static Dictionary<int, RoleName> _playerRoles = new Dictionary<int, RoleName>();
        /// <summary>
        /// 添加特殊角色
        /// </summary>
        /// <param name="role">特殊角色名称</param>
        /// <param name="playerId">playerid</param>
        public static void Add(RoleName role, int playerId)
        {
            try
            {
                if (_playerRoles.ContainsKey(playerId))
                {
                    _playerRoles[playerId] = role;
                    return;
                }

                _playerRoles.Add(playerId, role);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加角色时出错: {ex.Message}");
            }
        }
        public static void RemoveRole(int playerId)
        {
            try
            {
                if (!_playerRoles.ContainsKey(playerId))
                {
                    return; 
                }
                _playerRoles.Remove(playerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"移除角色时出错: {ex.Message}");
                throw; 
            }
        }
        public static bool IsRole(int playerId, RoleName role)
        {
            try
            {
                if (!_playerRoles.ContainsKey(playerId))
                {
                    return false;
                }

                return _playerRoles[playerId] == role;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查角色时出错: {ex.Message}");
                return false;
            }
        }
        public static RoleName? GetPlayerRole(int playerId)
        {
            if (_playerRoles.TryGetValue(playerId, out var role))
            {
                return role;
            }
            return null;
        }
    }
}
