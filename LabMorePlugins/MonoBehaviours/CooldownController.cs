using GameCore;
using LabMorePlugins.API;
using LabMorePlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Log = LabApi.Features.Console.Logger;

namespace LabMorePlugins.MonoBehaviours
{
    public class CooldownController : MonoBehaviour
    {
        public void Awake()
        {
            this._abilityCooldown = AbilityManager.GetAbilities.ToDictionary((IAbility a) => a.Name, (IAbility _) => 0f);
            base.InvokeRepeating("CheckCooldown", 0f, 1f);
            LabApi.Features.Console.Logger.Debug("[CooldownController] Invoke the cooldown cycle");
        }

        private void CheckCooldown()
        {
            foreach (string text in this._abilityCooldown.Keys.ToList<string>())
            {
                if (this._abilityCooldown[text] > 0f)
                {
                    Dictionary<string, float> abilityCooldown = this._abilityCooldown;
                    string key = text;
                    float num = abilityCooldown[key];
                    abilityCooldown[key] = num - 1f;
                }
                else
                {
                    this._abilityCooldown[text] = 0f;
                }
            }
        }

        private void OnDestroy()
        {
            base.CancelInvoke("CheckCooldown");
            Log.Debug("[CooldownController] Cancel the cooldown cycle");
        }

        public bool IsAbilityAvailable(string ability)
        {
            return this._abilityCooldown[ability] <= 0f;
        }

        public void SetCooldownForAbility(string ability, float time)
        {
            this._abilityCooldown[ability] = time;
        }

        private Dictionary<string, float> _abilityCooldown;
    }
}
