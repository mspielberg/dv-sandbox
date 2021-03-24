using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace DvMod.Sandbox
{
    public static class GrabberDump
    {
        [HarmonyPatch(typeof(Grabber), nameof(Grabber.TryRaycastInteractable))]
        public static class TryRaycastPatch
        {
            private static AGrabHandler? Original(Grabber __instance)
            {
                if (!Physics.SphereCast(__instance.ray, __instance.sphereCastRadius, out __instance.hit, __instance.sphereCastMaxDistance, __instance.sphereCastMask.value, QueryTriggerInteraction.Collide))
                {
                    Main.DebugLog("Found no colliders");
                    return null;
                }
                Main.DebugLog($"hit.collider={__instance.hit.collider}");
                AGrabHandler? aGrabHandler = __instance.hit.collider.GetComponentInParent<StaticInteractionArea>()?.grabHandler;
                if ((bool)aGrabHandler)
                {
                    Main.DebugLog("Found StaticInteractionArea");
                    return aGrabHandler;
                }
                aGrabHandler = __instance.hit.collider.GetComponentInParent<AGrabHandler>();
                if ((bool)aGrabHandler)
                {
                    Main.DebugLog("Found AGrabHandler");
                    return aGrabHandler;
                }
                if (!Physics.SphereCast(__instance.ray, __instance.sphereCastRadius, out __instance.hit, __instance.sphereCastMaxDistance, __instance.sphereCastMask.value, QueryTriggerInteraction.Ignore))
                {
                    Main.DebugLog("Found no non-trigger colliders");
                    return null;
                }
                return __instance.hit.collider.GetComponentInParent<AGrabHandler>();
            }

            private static string MaskToString(LayerMask mask)
            {
                return string.Join(",", Enumerable.Range(0, 32).Where(id => (mask.value & (1 << id)) != 0).Select(LayerMask.LayerToName));
            }

            // public static bool Prefix(Grabber __instance, ref AGrabHandler? __result)
            // {
            //     __result = Original(__instance);
            //     Main.DebugLog($"maxDistance={__instance.sphereCastMaxDistance},mask={MaskToString(__instance.sphereCastMask)},result={__result}");
            //     return false;
            // }
        }
    }
}