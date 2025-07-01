using Christmas.Scp2536.Gifts;
using GameCore;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Features.Wrappers;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace LabMorePlugins.API
{
    public static class SAPI
    {
        public static Random Random = new Random();
        public static List<Player> SpecialPlayerList = new List<Player>();
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
        public static void SpawnItem(Vector3 pos, ItemType itemType)
        {
            Pickup.Create(itemType, pos);
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
    }
}
