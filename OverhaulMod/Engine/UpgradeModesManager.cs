using OverhaulMod.Combat;
using OverhaulMod.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Engine
{
    public class UpgradeModesManager : Singleton<UpgradeModesManager>, IGameLoadListener
    {
        public static readonly Tuple<UpgradeType, int>[] UnrevertableUpgrades = new Tuple<UpgradeType, int>[]
        {
            new Tuple<UpgradeType, int>(UpgradeType.SwordUnlock, 1),
            new Tuple<UpgradeType, int>(UpgradeType.Hammer, 1),
            new Tuple<UpgradeType, int>(UpgradeType.SpearUnlock, 1),
            new Tuple<UpgradeType, int>(UpgradeType.BowUnlock, 1),
            new Tuple<UpgradeType, int>(UpgradeType.Armor, 0),
            new Tuple<UpgradeType, int>(ModUpgradesManager.SCYTHE_UNLOCK_UPGRADE, 1),
        };

        public static bool IsUnrevertableUpgrade(UpgradeType type, int level)
        {
            foreach (Tuple<UpgradeType, int> tuple in UnrevertableUpgrades)
            {
                if (tuple.Item1 == type && (level <= tuple.Item2))
                    return true;
            }
            return false;
        }

        public static UpgradeModes Mode
        {
            get;
            private set;
        }

        private UpgradeModeButtonController m_buttonController;

        public void OnGameLoaded()
        {
            PlaceButton();
        }

        public void PlaceButton()
        {
            if (!m_buttonController)
            {
                RectTransform upgradeUITransform = GameUIRoot.Instance.UpgradeUI.transform as RectTransform;
                RectTransform centerHolderTransform = TransformUtils.FindChildRecursive(upgradeUITransform, "CenterHolder") as RectTransform;
                RectTransform iconContainerTransform = TransformUtils.FindChildRecursive(upgradeUITransform, "IconContainer") as RectTransform;

                RectTransform spawnedButton = Instantiate(ModResources.Prefab(AssetBundleConstants.UI, "RevertUpgradesButtonPrefab"), centerHolderTransform).GetComponent<RectTransform>();
                spawnedButton.anchoredPosition = new Vector2(250f, 133.5f);
                spawnedButton.sizeDelta = Vector2.one * 50f;
                spawnedButton.localEulerAngles = Vector3.zero;
                spawnedButton.localScale = Vector3.one;

                Button button = spawnedButton.GetComponent<Button>();
                button.onClick.AddListener(ToggleMode);
                UpgradeModeButtonController upgradeModeButtonController = spawnedButton.gameObject.AddComponent<UpgradeModeButtonController>();
                upgradeModeButtonController.InitializeElement();
                m_buttonController = upgradeModeButtonController;
            }
            SetMode(UpgradeModes.Upgrade);
        }

        public void ToggleMode()
        {
            SetMode(Mode == UpgradeModes.Upgrade ? UpgradeModes.RevertUpgrade : UpgradeModes.Upgrade);
        }

        public void SetMode(UpgradeModes upgradeMode)
        {
            Mode = upgradeMode;

            if (GameUIRoot.Instance && GameUIRoot.Instance.UpgradeUI && GameUIRoot.Instance.UpgradeUI.gameObject.activeSelf)
                GameUIRoot.Instance.UpgradeUI.PopulateIcons();

            UpgradeModeButtonController controller = m_buttonController;
            if (!controller)
                return;

            if (upgradeMode == UpgradeModes.Upgrade)
            {
                controller.SetText(false);
                controller.SetSprite(ModResources.Sprite(AssetBundleConstants.UI, "RevertUpgradesButton"));
                return;
            }
            controller.SetText(true);
            controller.SetSprite(ModResources.Sprite(AssetBundleConstants.UI, "GetUpgradesButton"));
        }
    }
}
