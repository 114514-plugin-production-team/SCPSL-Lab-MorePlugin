using LabApi.Features.Wrappers;
using LabMorePlugins.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabMorePlugins.Interfaces
{
    public abstract class Ability : IAbility
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract int KeyId { get; }
        public abstract KeyCode KeyCode { get; }
        public abstract float Cooldown { get; }

        public abstract void Register();
        public abstract void Unregister();
        public void OnKeyPressed(Player player)
        {
            if (player == null)
            {
                return;
            }
            CooldownController component = player.GameObject.GetComponent<CooldownController>();
            if (!component.IsAbilityAvailable(this.Name))
            {
                return;
            }
            component.SetCooldownForAbility(this.Name, this.Cooldown);
        }
        protected abstract void ActivateAbility(Player player);
    }
}
