﻿using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIAutoBuildSelectionMenu : OverhaulUIBehaviour
    {
        [UIElement("BuildDisplayPrefab", false)]
        private readonly ModdedObject m_buildDisplayPrefab;

        [UIElement("Holder", false)]
        private readonly GameObject m_holder;

        [UIElement("Container")]
        private readonly Transform m_buildDisplayContainer;

        private CanvasGroup m_canvasGroup;

        private UIElementAutoBuildSelectionEntry m_prevSelectedEntry, m_currentSelectedEntry;

        private bool m_show;

        public override bool closeOnEscapeButtonPress => false;

        protected override void OnInitialized()
        {
            m_canvasGroup = base.GetComponent<CanvasGroup>();
            PopulateBuilds();
        }

        public override void Show()
        {
            base.Show();
            if (m_currentSelectedEntry)
                m_currentSelectedEntry.SetBGActive(false);

            m_show = true;
            m_canvasGroup.alpha = 0f;
            m_canvasGroup.blocksRaycasts = true;
            m_holder.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            ModActionUtils.DoInFrame(delegate
            {
                Cursor.lockState = CursorLockMode.None;
            });
        }

        public override void Hide()
        {
            m_show = false;
            m_canvasGroup.blocksRaycasts = false;

            if (m_currentSelectedEntry)
            {
                AutoBuildManager.Instance.ApplyBuild(m_currentSelectedEntry.transform.GetSiblingIndex());
                SelectEntry(null);
            }
        }

        public override void Update()
        {
            CanvasGroup canvasGroup = m_canvasGroup;

            float alpha = m_canvasGroup.alpha;
            alpha += Time.unscaledDeltaTime * 10f * (m_show ? 1f : -1f);
            m_canvasGroup.alpha = alpha;

            if (!m_show && alpha <= 0.01f)
                m_holder.SetActive(false);
        }

        public void PopulateBuilds()
        {
            if (m_buildDisplayContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_buildDisplayContainer);

            UpgradeManager upgradeManager = UpgradeManager.Instance;
            AutoBuildManager autoBuildManager = AutoBuildManager.Instance;
            foreach (var build in autoBuildManager.buildList.Builds)
            {
                ModdedObject moddedObject = Instantiate(m_buildDisplayPrefab, m_buildDisplayContainer);
                moddedObject.gameObject.SetActive(true);

                Text buildNameText = moddedObject.GetObject<Text>(0);
                buildNameText.text = AutoBuildManager.GetBuildDisplayName(build.Name);

                Image upgradeIconPrefab = moddedObject.GetObject<Image>(1);
                upgradeIconPrefab.gameObject.SetActive(true);
                Transform upgradeIconContainer = moddedObject.GetObject<Transform>(2);
                foreach (var upgrade in build.Upgrades)
                {
                    Sprite sprite = null;

                    UpgradeDescription upgradeDescription = upgradeManager.GetUpgrade(upgrade.UpgradeType, upgrade.Level);
                    if (upgradeDescription && upgradeDescription.Icon)
                    {
                        sprite = upgradeDescription.Icon;
                    }
                    else
                    {
                        sprite = ModResources.Sprite(AssetBundleConstants.UI, "NA-HQ-128x128");
                    }

                    Image upgradeIcon = Instantiate(upgradeIconPrefab, upgradeIconContainer);
                    upgradeIcon.sprite = sprite;
                }
                upgradeIconPrefab.gameObject.SetActive(false);

                UIElementAutoBuildSelectionEntry autoBuildSelectionEntry = moddedObject.gameObject.AddComponent<UIElementAutoBuildSelectionEntry>();
                autoBuildSelectionEntry.BuildInfo = build;
                autoBuildSelectionEntry.Menu = this;
                autoBuildSelectionEntry.InitializeElement();
            }
        }

        public void SelectEntry(UIElementAutoBuildSelectionEntry entry)
        {
            if(entry)
                entry.SetBGActive(true);

            if (m_currentSelectedEntry)
                m_currentSelectedEntry.SetBGActive(false);

            m_prevSelectedEntry = m_currentSelectedEntry;
            m_currentSelectedEntry = entry;
        }
    }
}
