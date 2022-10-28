﻿using UnityEngine.UI;

namespace CloneDroneOverhaul.UI
{
    public class Watermark : ModGUIBase
    {
        private Text WatermarkText;
        private Button ChangelogButton;
        private Button LEButton;
        private Button editorsButton;

        private Text ChangelogText;
        private Text LEText;
        private Text editText;

        public override void OnInstanceStart()
        {
            base.MyModdedObject = base.GetComponent<ModdedObject>();
            WatermarkText = MyModdedObject.GetObjectFromList<Text>(0);
            ChangelogButton = MyModdedObject.GetObjectFromList<Button>(1);
            ChangelogButton.onClick.AddListener(new UnityEngine.Events.UnityAction(OnUpdateNotesClicked));
            ChangelogText = MyModdedObject.GetObjectFromList<Text>(2);
            LEButton = MyModdedObject.GetObjectFromList<Button>(3);
            LEButton.onClick.AddListener(new UnityEngine.Events.UnityAction(TryShowLocalEditor));
            LEText = MyModdedObject.GetObjectFromList<Text>(4);
            editText = MyModdedObject.GetObjectFromList<Text>(6);
            editorsButton = MyModdedObject.GetObjectFromList<Button>(5);

            OverhaulMain.Timer.AddNoArgActionToCompleteNextFrame(AddListeners);
        }

        public override void OnManagedUpdate()
        {
            ChangelogButton.gameObject.SetActive(GameModeManager.IsOnTitleScreen());
            LEButton.gameObject.SetActive(GameModeManager.IsOnTitleScreen() && !OverhaulDescription.IsPublicBuild());
            editorsButton.gameObject.SetActive(false && GameModeManager.IsOnTitleScreen()); //false is a temp thing there
            WatermarkText.text = OverhaulDescription.GetModName(true, !GameModeManager.IsOnTitleScreen());
        }

        public override void OnNewFrame()
        {
            WatermarkText.gameObject.SetActive(!PhotoManager.Instance.IsInPhotoMode() && !CutSceneManager.Instance.IsInCutscene() && !Modules.MiscEffectsManager.IsUIHidden);
        }

        private void OnLangChanged()
        {
            ChangelogText.text = OverhaulMain.GetTranslatedString("UpdateNotes");
        }

        private void TryShowLocalEditor()
        {
            GUIModule.GetGUI<Localization.OverhaulLocalizationEditor>().TryShow();
        }
        private void AddListeners()
        {
            GlobalEventManager.Instance.AddEventListener(GlobalEvents.UILanguageChanged, OnLangChanged);
            OnLangChanged();
        }
        private void OnUpdateNotesClicked()
        {
            BaseUtils.OpenURL("https://github.com/aTVCat/CloneDroneOverhaul/releases/tag/" + OverhaulDescription.GetModVersion(false));
        }
    }
}
