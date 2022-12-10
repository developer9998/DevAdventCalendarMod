using HarmonyLib;
using GorillaNetworking;

namespace DevAdventCalendarMod.Patches
{
    [HarmonyPatch(typeof(GorillaNetworkJoinTrigger), "OnBoxTriggered")]
    internal class NetworkPatch
    {
        internal static void Postfix(GorillaNetworkJoinTrigger __instance)
        {
            Plugin.Instance.OnNetworkSwitched(__instance);
        }
    }
}
