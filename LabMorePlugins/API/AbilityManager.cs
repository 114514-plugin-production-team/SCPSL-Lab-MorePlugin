using GameCore;
using LabMorePlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Log = LabApi.Features.Console.Logger;

namespace LabMorePlugins.API
{
    public static class AbilityManager
    {
        public static void RegisterAbilities()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                try
                {
                    if (!type.IsInterface && !type.IsAbstract && type.GetInterfaces().Contains(typeof(IAbility)))
                    {
                        IAbility ability = Activator.CreateInstance(type) as IAbility;
                        if (ability != null)
                        {
                            AbilityManager._abilityList.Add(ability);
                            Log.Debug("Register the " + ability.Name + " ability.");
                            ability.Register();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error in RegisterAbilities:" + ex.Message);
                }
            }
        }

        public static void UnregisterAbilities()
        {
            foreach (IAbility ability in AbilityManager._abilityList)
            {
                try
                {
                    ability.Unregister();
                }
                catch (Exception ex)
                {
                    Log.Error("Error in UnregisterAbilities:" + ex.Message);
                }
            }
        }

        public static List<IAbility> GetAbilities
        {
            get
            {
                return AbilityManager._abilityList;
            }
        }

        private static List<IAbility> _abilityList = new List<IAbility>();
    }
}
