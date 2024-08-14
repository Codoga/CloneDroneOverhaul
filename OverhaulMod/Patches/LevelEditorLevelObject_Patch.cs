using HarmonyLib;
using OverhaulMod.Engine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(LevelEditorLevelObject))]
    internal static class LevelEditorLevelObject_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LevelEditorLevelObject.DeserializeInto))]
        private static void DeserializeInto_Postfix(LevelEditorLevelObject __instance, ObjectPlacedInLevel objectPlacedInLevel)
        {
            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.QuickReset) && objectPlacedInLevel)
            {
                LevelEditorObjectAdvancedBehaviour objectAdvancedBehaviour = objectPlacedInLevel.GetComponent<LevelEditorObjectAdvancedBehaviour>();
                if (!objectAdvancedBehaviour)
                    objectAdvancedBehaviour = objectPlacedInLevel.gameObject.AddComponent<LevelEditorObjectAdvancedBehaviour>();

                objectAdvancedBehaviour.SerializedObject = __instance;
            }
        }
    }
}
