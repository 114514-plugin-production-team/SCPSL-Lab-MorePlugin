using Christmas.Scp2536.Gifts;
using GameCore;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Log = LabApi.Features.Console.Logger;
using Logger = LabApi.Features.Console.Logger;
using Random = System.Random;

namespace LabMorePlugins.API
{
    public static class SAPI
    {
        public static Random Random = new Random();
        public static List<Player> SpecialPlayerList = new List<Player>();
        public static readonly Dictionary<Player, AudioPlayer> PlayerAudioPlayers = new Dictionary<Player, AudioPlayer>();
        public static readonly Dictionary<Player, Speaker> PlayerSpeakers = new Dictionary<Player, Speaker>();
        public static IEnumerable<Player> GetRole(RoleTypeId role)
        {
            return from player in Player.List
                   where player.Role == role
                   select player;
        }
        public static IEnumerable<Player> GetTeam(Team team)
        {
            return from player in Player.List
                   where player.Team == team
                   select player;
        }
        public static Room GetRandomRoom()
        {
            List<Room> rooms = Room.List.ToList();
            return rooms[Random.Next(0, rooms.Count)];
        }
        public static Room GetRandomRoomByZone(MapGeneration.FacilityZone zoneType)
        {
            List<Room> rooms = Room.List.Where(room => room.Zone == zoneType).ToList();
            if (rooms.Count == 0) return null;
            return rooms[Random.Next(0, rooms.Count)];
        }
        public static List<Player> RandomPlayers(IEnumerable<Player> playerList, int count)
        {
            var players = playerList.ToList();
            if (count >= players.Count)
                return players;
            if (count <= 0)
                return new List<Player>();
            var result = new List<Player>(count);
            var tempList = new List<Player>(players);

            for (int i = 0; i < count; i++)
            {
                int index = Random.Next(0, tempList.Count);
                result.Add(tempList[index]);
                tempList.RemoveAt(index);
            }

            return result;
        }
        public static Player GetRandomPlayer(List<Player> playerList)
        {
            if (playerList.Any())
            {
                return playerList[Random.Next(0, playerList.Count() - 1)];
            }

            return null;
        }
        public static Pickup SpawnItem(Vector3 pos, ItemType itemType)
        {
            return Pickup.Create(itemType, pos);
        }
        public static Player GetRandomSpecialPlayer(RoleTypeId roleTypeId)
        {
            List<Player> players = new List<Player>();
            foreach (Player player in SpecialPlayerList)
            {
                if (player.Role == roleTypeId)
                {
                    players.Add(player);
                }
            }
            if (players.Any())
            {
                var randomPlayer = players[Random.Next(0, players.Count() - 1)];
                SpecialPlayerList.Remove(randomPlayer);
                return randomPlayer;
            }

            return null;
        }
        public static Room GetRandomRoom(MapGeneration.RoomName type)
        {
            if (type == MapGeneration.RoomName.Unnamed)
                return null;

            List<Room> validRooms = Room.List.Where(x => x.Name == type).ToList();

            // return validRooms[Random.Range(0, validRooms.Count)];
            return validRooms.First();
        }
        public static Vector3 GetRelativePosition(Vector3 position, Room room) => room.Name == MapGeneration.RoomName.Outside ? position : room.Transform.TransformPoint(position);
        public static Quaternion GetRelativeRotation(Vector3 rotation, Room room)
        {
            if (rotation.x == -1f)
                rotation.x = UnityEngine.Random.Range(0f, 360f);

            if (rotation.y == -1f)
                rotation.y = UnityEngine.Random.Range(0f, 360f);

            if (rotation.z == -1f)
                rotation.z = UnityEngine.Random.Range(0f, 360f);

            if (room == null)
                return Quaternion.Euler(rotation);

            return room.Name == RoomName.Outside ? Quaternion.Euler(rotation) : room.Transform.rotation * Quaternion.Euler(rotation);
        }
        public static Color32 GetColor32(Color color)
        {
            return new Color32(
                     (byte)(Mathf.Clamp01(color.r) * 255f),
                     (byte)(Mathf.Clamp01(color.g) * 255f),
                     (byte)(Mathf.Clamp01(color.b) * 255f),
                     (byte)(Mathf.Clamp01(color.a) * 255f)
                  );
        }
        public static Vector3 QuaternionToEulerAnglesManual(Quaternion q)
        {
            float x = Mathf.Atan2(2 * (q.w * q.x + q.y * q.z), 1 - 2 * (q.x * q.x + q.y * q.y));
            float y = Mathf.Asin(2 * (q.w * q.y - q.z * q.x));
            float z = Mathf.Atan2(2 * (q.w * q.z + q.x * q.y), 1 - 2 * (q.y * q.y + q.z * q.z));

            x *= Mathf.Rad2Deg;
            y *= Mathf.Rad2Deg;
            z *= Mathf.Rad2Deg;

            return new Vector3(x, y, z);
        }
        public static void PlayFollowingSound(Player player, string audioPath, string audioName, float volume = 1f, bool loop = false)
        {
            try
            {
                if (PlayerAudioPlayers.ContainsKey(player))
                {
                    RemovePlayerAudio(player);
                }

                string uniqueName = $"PlayerAudio{player.PlayerId}";

                var audioPlayer = AudioPlayer.Create(
                    uniqueName,
                    owners: new List<ReferenceHub> { player.ReferenceHub },
                    destroyWhenAllClipsPlayed: true
                );

                if (audioPlayer == null)
                {
                    Log.Error("无法创建音频播放器");
                    return;
                }
                if (!AudioClipStorage.AudioClips.ContainsKey(audioName))
                {
                    string fullPath = Path.Combine(audioPath, audioName + ".ogg");
                    if (!AudioClipStorage.LoadClip(fullPath, audioName))
                    {
                        Log.Error($"无法加载音频文件: {fullPath}");
                        return;
                    }
                }

                var speaker = audioPlayer.AddSpeaker(
                    $"Speaker{player.PlayerId}",
                    player.Position,
                    volume,
                    true, // 3D音效
                    1f,   // 最小距离
                    15f   // 最大距离
                );
                audioPlayer.AddClip(audioName, volume, loop);
                audioPlayer.Condition = hub =>
                {
                    var targetPlayer = Player.Get(hub);
                    return targetPlayer != null &&
                           Vector3.Distance(targetPlayer.Position, speaker.Position) <= 15f;
                };
                PlayerAudioPlayers[player] = audioPlayer;
                PlayerSpeakers[player] = speaker;
                UpdateSpeakerPosition(player);

                Logger.Debug($"已为玩家 {player.Nickname} 创建跟随音频");
            }
            catch (Exception e)
            {
                Logger.Error($"创建跟随音频出错: {e}");
            }
        }
        private static void UpdateSpeakerPosition(Player player)
        {
            Timing.RunCoroutine(UpdatePositionCoroutine(player));
        }

        private static IEnumerator<float> UpdatePositionCoroutine(Player player)
        {
            while (player != null && player.IsAlive && PlayerSpeakers.TryGetValue(player, out var speaker))
            {
                speaker.Position = player.Position;
                yield return Timing.WaitForSeconds(0.1f);
            }
        }
        
        public static void RemovePlayerAudio(Player player)
        {
            try
            {
                if (PlayerAudioPlayers.TryGetValue(player, out var audioPlayer))
                {
                    audioPlayer.Destroy();
                    PlayerAudioPlayers.Remove(player);
                    PlayerSpeakers.Remove(player);
                    Log.Debug($"已移除玩家 {player.Nickname} 的音频播放器");
                }
            }
            catch (Exception e)
            {
                Log.Error($"移除玩家音频出错: {e}");
            }
        }
        public static bool CheckKeycardAccess(InventorySystem.Items.Keycards.KeycardItem keycard, DoorPermissionFlags requiredPermissions)
        {
            try
            {
                var field = keycard.GetType().GetField("Details",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field == null) return false;

                var details = field.GetValue(keycard) as Array;
                if (details == null) return false;

                foreach (var detail in details)
                {
                    if (detail is IDoorPermissionProvider provider)
                    {
                        var permissions = provider.GetPermissions(null);
                        if ((permissions & requiredPermissions) == requiredPermissions)
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error checking keycard permissions: {ex}");
            }

            return false;
        }
    }
}

