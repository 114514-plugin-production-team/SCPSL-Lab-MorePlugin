using LabApi.Features.Wrappers;
using LabMorePlugins.Interfaces;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabMorePlugins.Ability
{
    public class S106Black : Interfaces.Ability
    {
        public override string Name => "老头回口袋";

        public override string Description => "老头回口袋";

        public override int KeyId => 106;

        public override KeyCode KeyCode => KeyCode.Y;

        public override float Cooldown => 90;

        public override void Register()
        {
            throw new NotImplementedException();
        }

        public override void Unregister()
        {
            throw new NotImplementedException();
        }

        protected override void ActivateAbility(Player player)
        {
            if (player.Role == PlayerRoles.RoleTypeId.Scp106)
            {
                Timing.RunCoroutine(Check106(player));
                player.Position = Room.Get(MapGeneration.RoomName.Pocket).FirstOrDefault().Position + Vector3.up;
                for (int i = 20; i >= 0; i--)
                {
                    player.SendHint($"回血中,还剩{i}", 20);
                }
                Timing.CallDelayed(20f, () =>
                {
                    player.Position = Room.Get(MapGeneration.FacilityZone.HeavyContainment).FirstOrDefault().Position + Vector3.up;
                    
                });
            }
        }
        public IEnumerator<float> Check106(Player player)
        {
            while(player.Role == PlayerRoles.RoleTypeId.Scp106)
            {
                if (player.Room.Name == MapGeneration.RoomName.Pocket)
                {
                    player.Heal(25);
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}
