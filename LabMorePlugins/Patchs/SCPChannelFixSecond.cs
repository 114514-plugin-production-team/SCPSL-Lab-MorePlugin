using HarmonyLib;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles;
using PlayerRoles.Voice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceChat;
using VoiceChat.Networking;

namespace LabMorePlugins.Patchs
{
    [HarmonyPatch(typeof(VoiceTransceiver), "ServerReceiveMessage")]
    public class SCPChannelFixSecond
    {
        public static void Postfix(NetworkConnection conn, VoiceMessage msg)
        {
            Player player = Player.Get(conn.identity);
            if (player != null && player!=null && player.Team == Team.SCPs)
            {
                foreach (ReferenceHub referenceHub in ReferenceHub.AllHubs)
                {
                    if (referenceHub.roleManager.CurrentRole is IVoiceRole && scpChannel.Contains(referenceHub.PlayerId))
                    {
                        msg.Channel = VoiceChatChannel.Intercom;
                        referenceHub.connectionToClient.Send<VoiceMessage>(msg, 0);
                    }
                }
            }
        }
        public static List<int> scpChannel = new List<int>();
    }
}
