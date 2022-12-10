using HarmonyLib;
using System.Collections;
using GorillaLocomotion;

namespace DevAdventCalendarMod.Patches
{
    [HarmonyPatch(typeof(Player), "Awake")]
    internal class InitializationPatch
    {
        internal static void Postfix(Player __instance)
        {
            __instance.StartCoroutine(Delay());
        }

        internal static IEnumerator Delay()
        {
            yield return 0;

            Plugin.Instance.OnInitialized();
        }
    }
}
