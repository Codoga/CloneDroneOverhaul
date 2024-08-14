using HarmonyLib;
using OverhaulMod.Visuals.Environment;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(SeveredVolumeGenerator))]
    internal static class SeveredVolumeGenerator_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SeveredVolumeGenerator.CreateVolumeCopy))]
        private static void CreateVolumeCopy_Postfix(SeveredBodyPart __result)
        {
            if (SeveredBodyPartSparks.EnableGarbageParticles && Random.value <= 0.7f)
                _ = __result.gameObject.AddComponent<SeveredBodyPartSparks>();

            Rigidbody rigidBody = __result.gameObject.GetComponent<Rigidbody>();
            if (rigidBody)
            {
                rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
    }
}
