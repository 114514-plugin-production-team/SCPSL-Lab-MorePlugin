using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabMorePlugins
{
    public class Plugin : Plugin<Class1>
    {
        public override string Name => "Lab综合插件";

        public override string Description => "Lab综合插件";

        public override string Author => "HUI(3145186196)";

        public override Version Version => new Version(1,0);
        public static Class1 config;
        public static CoroutineHandle CoroutineHandle;
        public override Version RequiredApiVersion => new Version(LabApi.Features.LabApiProperties.CompiledVersion);

        public override void Disable()
        {
            ServerEvents.RoundStarted -= OnStart;
            PlayerEvents.Spawned -= OnSpawned;
            PlayerEvents.Joined -= OnJoined;
        }

        public override void Enable()
        {
            ServerEvents.RoundStarted += OnStart;
            PlayerEvents.Spawned += OnSpawned;
            PlayerEvents.Joined += OnJoined;
        }
        public void OnStart()
        {
            if (!Round.IsRoundEnded)
            {
                
            }
        }
        public void OnSpawned(PlayerSpawnedEventArgs ev)
        {
            if(ev.Player!=null)
            {
                if (ev.Player.Role == PlayerRoles.RoleTypeId.ClassD)
                {
                    ev.Player.AddItem(ItemType.KeycardJanitor);
                }
            }
        }
        public void OnJoined(PlayerJoinedEventArgs ev)
        {
            if (ev.Player!=null)
            {
                Logger.Info($"玩家{ev.Player.Nickname}加入服务器|Steam64ID为{ev.Player.UserId}");

            }
        }
    }
}
