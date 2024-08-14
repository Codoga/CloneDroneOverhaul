using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Patches.Behaviours
{
    internal class MenuButtonsPatchBehaviour : GamePatchBehaviour
    {
        public override void Patch()
        {
            GameUIRoot gameUIRoot = ModCache.gameUIRoot;
            if (gameUIRoot)
            {
                SettingsMenu settingsMenu = gameUIRoot.SettingsMenu;
                if (settingsMenu)
                {
                    GameObject buttonObject = Instantiate(ModResources.Prefab(AssetBundleConstants.UI, "NewUIButtonPrefab"), settingsMenu.RootContainer.transform);
                    buttonObject.SetActive(true);
                    RectTransform buttonTransform = buttonObject.transform as RectTransform;
                    buttonTransform.localEulerAngles = Vector3.zero;
                    buttonTransform.localScale = Vector3.one;
                    buttonTransform.anchorMax = Vector3.one;
                    buttonTransform.anchorMin = Vector3.one;
                    buttonTransform.pivot = Vector3.one;
                    buttonTransform.anchoredPosition = new Vector2(-40f, -7.5f);

                    Button button = buttonObject.GetComponent<Button>();
                    button.onClick.AddListener(delegate
                    {
                        settingsMenu.Hide();
                        ModActionUtils.DoInFrame(delegate
                        {
                            _ = ModUIConstants.ShowSettingsMenuRework(false);
                        });
                    });
                }
            }
        }
    }
}
