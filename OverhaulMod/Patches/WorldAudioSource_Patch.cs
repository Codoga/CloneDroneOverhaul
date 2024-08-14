using HarmonyLib;
using OverhaulMod.Engine;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(WorldAudioSource))]
    internal static class WorldAudioSource_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(WorldAudioSource.Initialize))]
        private static void Initialize_Postfix(WorldAudioSource __instance)
        {
            if (__instance.gameObject.name.Contains("Global"))
                return;

            AudioReverbFilter filter = __instance.GetComponent<AudioReverbFilter>();
            if (filter)
            {
                if (ModAudioManager.EnableReverbFilter)
                {
                    Ray ray = ModPhysicsManager.GetRay(__instance.transform.position, Vector3.up);
                    RaycastHit[] rayCastHits = ModPhysicsManager.GetRayCastHitArray(false);
                    int hits = Physics.RaycastNonAlloc(ray, rayCastHits, 75f, PhysicsManager.GetEnvironmentLayerMask());

                    filter.decayTime = hits != 0 ? 0.375f * ModAudioManager.ReverbIntensity : 0f;
                }
                else
                    filter.decayTime = 0f;
            }
        }
    }
}
