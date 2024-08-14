using HarmonyLib;
using OverhaulMod.Visuals;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(GlobalFireParticleSystem))]
    internal static class GlobalFireParticleSystem_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GlobalFireParticleSystem.SpawnSingleBig), new System.Type[] { typeof(Vector3), typeof(Vector3), typeof(float) })]
        private static void SpawnSingleBig_Postfix(Vector3 worldPos, Vector3 startVelocity, float lifeTime)
        {
            if (ParticleManager.EnableParticles && UnityEngine.Random.value >= 0.95f)
                ParticleManager.Instance.SpawnFireParticles(worldPos);
        }
    }
}
