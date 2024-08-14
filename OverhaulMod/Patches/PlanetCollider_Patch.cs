using HarmonyLib;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(PlanetCollider))]
    internal static class PlanetCollider_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetCollider.OnEnable))]
        private static void OnEnable_Postfix(PlanetCollider __instance)
        {
            if (!__instance.name.Contains("Earth"))
                return;

            Rotator rotator = __instance.GetComponent<Rotator>();
            if (rotator)
            {
                rotator.enabled = !GameModeManager.IsInLevelEditor();
                rotator.SetRotationSpeed(new Vector3(0f, 0.25f, 0f));
            }

            Transform transform = __instance.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform t = transform.GetChild(i);
                t.gameObject.SetActive(true);
                t.localPosition = new Vector3(0f, 0f, 0f);
            }

            for (int j = 0; j < transform.childCount; j++)
            {
                Transform t = transform.GetChild(j);
                string tName = t.name;

                if (tName == "Clouds_Pixel_Original" || tName == "Atmosphere (1)" || tName == "Clouds_Pixel (1)")
                    t.gameObject.SetActive(false);
                else if (tName == "Atmosphere_original")
                    t.localPosition = new Vector3(0f, 0.01f, 0f);
                else if (tName == "OceanSphere")
                    t.localScale = new Vector3(29.5f, 29.5f, 29.5f);
                else if (tName == "Atmosphere_Clouds")
                    t.localScale = new Vector3(32.2f, 32.2f, 32.2f);
                else if (tName == "Continents")
                    t.localScale = new Vector3(0.95f, 0.99f, 0.95f);
                else if (tName == "SphereClouds")
                {
                    t.localScale = new Vector3(0.045f, 0.045f, 0.21f);
                    t.localPosition = new Vector3(-0.005f, 0.005f, -0.005f);
                }
            }
        }
    }
}
