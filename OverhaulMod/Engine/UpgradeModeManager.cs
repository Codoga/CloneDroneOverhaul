﻿using OverhaulMod.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Engine
{
    public class UpgradeModeManager : Singleton<UpgradeModeManager>, IGameLoadListener
    {
        public static readonly Tuple<UpgradeType, int>[] UnrevertableUpgrades = new Tuple<UpgradeType, int>[]
        {
            new Tuple<UpgradeType, int>(UpgradeType.SwordUnlock, 1),
            new Tuple<UpgradeType, int>(UpgradeType.Hammer, 1),
            new Tuple<UpgradeType, int>(UpgradeType.SpearUnlock, 1),
            new Tuple<UpgradeType, int>(UpgradeType.BowUnlock, 1),
            new Tuple<UpgradeType, int>(UpgradeType.Armor, 0),
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

        public static UpgradeMode Mode
        {
            get;
            private set;
        }

        private Image m_ButtonGraphic;

        public void OnGameLoaded()
        {
            PlaceButton();
        }

        public void PlaceButton()
        {
            if (!m_ButtonGraphic)
            {
                RectTransform upgradeUITransform = GameUIRoot.Instance.UpgradeUI.transform as RectTransform;
                RectTransform centerHolderTransform = TransformUtils.FindChildRecursive(upgradeUITransform, "CenterHolder") as RectTransform;
                RectTransform iconContainerTransform = TransformUtils.FindChildRecursive(upgradeUITransform, "IconContainer") as RectTransform;

                RectTransform spawnedButton = Instantiate(ModResources.Load<GameObject>(AssetBundleConstants.UI, "RevertUpgradesButtonPrefab"), centerHolderTransform).GetComponent<RectTransform>();
                spawnedButton.anchoredPosition = new Vector2(250f, 133.5f);
                spawnedButton.sizeDelta = Vector2.one * 50f;
                spawnedButton.localEulerAngles = Vector3.zero;
                spawnedButton.localScale = Vector3.one;

                Button button = spawnedButton.GetComponent<Button>();
                button.onClick.AddListener(ToggleMode);
                m_ButtonGraphic = spawnedButton.GetComponent<Image>();
            }
            SetMode(UpgradeMode.Upgrade);
        }

        public void ToggleMode()
        {
            SetMode(Mode == UpgradeMode.Upgrade ? UpgradeMode.RevertUpgrade : UpgradeMode.Upgrade);
        }

        public void SetMode(UpgradeMode upgradeMode)
        {
            Mode = upgradeMode;

            if (GameUIRoot.Instance && GameUIRoot.Instance.UpgradeUI && GameUIRoot.Instance.UpgradeUI.gameObject.activeSelf)
                GameUIRoot.Instance.UpgradeUI.PopulateIcons();

            if (!m_ButtonGraphic)
                return;

            if (upgradeMode == UpgradeMode.Upgrade)
            {
                m_ButtonGraphic.sprite = ModResources.Load<Sprite>(AssetBundleConstants.UI, "RevertUpgradesButton");
                return;
            }
            m_ButtonGraphic.sprite = ModResources.Load<Sprite>(AssetBundleConstants.UI, "GetUpgradesButton");
        }
    }
}
