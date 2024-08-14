using HarmonyLib;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(UpgradeUIIcon))]
    internal static class UpgradeUIIcon_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UpgradeUIIcon.OnClickToUpgradeAbility))]
        private static bool OnClickToUpgradeAbility_Prefix(UpgradeUIIcon __instance, ref bool __result, bool isRandomSelectionInput)
        {
            if (isRandomSelectionInput)
                return true;

            if (UpgradeModesManager.Mode == UpgradeModes.Upgrade)
                return true;

            __result = UpgradeManager.Instance.RevertUpgrade(__instance.GetDescription());
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UpgradeUIIcon.refreshDisplay))]
        private static void refreshDisplay_Postfix(UpgradeUIIcon __instance)
        {
            CanvasGroup canvasGroup = __instance.GetComponent<CanvasGroup>();
            if (!canvasGroup)
                canvasGroup = __instance.gameObject.AddComponent<CanvasGroup>();

            bool isUpgradeMode = UpgradeModesManager.Mode == UpgradeModes.Upgrade;
            canvasGroup.alpha = !isUpgradeMode && !__instance.GetDescription().CanBeReverted() ? 0.3f : 1f;

            RectTransform content = __instance.transform.FindChildRecursive("Content") as RectTransform;
            if (content)
            {
                UpgradeDescription upgradeDescription = __instance._upgradeDescription;
                if (!upgradeDescription || !upgradeDescription.Icon)
                {
                    Image image = content.GetComponent<Image>();
                    if (image)
                    {
                        image.sprite = ModResources.Sprite(AssetBundleConstants.UI, "NA-HQ-128x128");
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UpgradeUIIcon.Awake))]
        private static void Awake_Postfix(UpgradeUIIcon __instance)
        {
            Transform selectableFrame = __instance.transform.FindChildRecursive("SelectableFrame");
            if (selectableFrame)
            {
                selectableFrame.localPosition = new Vector3(1f, 2f, 0f);
                selectableFrame.localScale = new Vector3(1.2f, 1f, 1f);
            }

            Transform bgFillTransform = __instance.transform.FindChildRecursive("BGFill");
            if (bgFillTransform)
            {
                UIColorSwapper bgFill = bgFillTransform.GetComponent<UIColorSwapper>();
                if (bgFill)
                {
                    bgFill.ColorVariants[0] = new Color(0f, 0.33f, 0.475f, 0.7f);
                }
            }

            RectTransform content = __instance.transform.FindChildRecursive("Content") as RectTransform;
            if (content)
            {
                UIColorSwapper contentColors = content.GetComponent<UIColorSwapper>();
                if (contentColors)
                {
                    contentColors.ColorVariants[0] = new Color(0f, 1f, 1f, 1f);
                }

                Shadow shadow = content.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.225f);
                shadow.effectDistance = Vector2.one * -1.5f;
            }
        }
    }
}
