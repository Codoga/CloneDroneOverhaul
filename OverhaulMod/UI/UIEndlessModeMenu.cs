﻿using OverhaulMod.Engine;
using OverhaulMod.UI.Attributes;
using OverhaulMod.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIEndlessModeMenu : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(OnExitButtonClicked))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnPlayButtonClicked))]
        [UIElement("PlayButton")]
        private readonly Button m_playButton;

        [UIElementAction(nameof(OnLeaderboardButtonClicked))]
        [UIElement("ViewLeaderBoardButton")]
        private readonly Button m_leaderboardButton;

        [ShowTooltipHighLight("Erase progress", 1.5f)]
        [UIElementAction(nameof(OnResetProgressButtonClicked))]
        [UIElement("DeleteProgressButton")]
        private readonly Button m_resetProgressButton;

        [UIElement("CurrentGameplayProgress")]
        private readonly Text m_progressText;

        public override bool dontRefreshUI => true;

        protected override void OnInitialized()
        {
            RefreshProgressDisplays();
        }

        public void RefreshProgressDisplays()
        {
            GameData gameData = GameDataManager.Instance?._endlessModeData;
            if (gameData != null)
            {
                EndlessTierDescription tierDesc = EndlessModeManager.Instance?.GetNextLevelDifficultyTierDescription(gameData.LevelIDsBeatenThisPlaythrough.Count);
                if (tierDesc == null)
                {
                    m_progressText.text = "Error: difficulty";
                }
                else
                {
                    string name = gameData.HumanFacts?.GetFullName();
                    string difficultyText = $" <color={tierDesc.TextColor.ToHex()}>{tierDesc.Tier.GetTierString()}</color>";
                    m_progressText.text = $"{name} - Level {gameData.LevelIDsBeatenThisPlaythrough.Count + 1}{difficultyText}";
                }
            }
            else
            {
                m_progressText.text = "N/A";
            }
        }

        public override void Show()
        {
            base.Show();
            ModCache.titleScreenUI.SetSinglePlayerModeSelectButtonsVisibile(false);
        }

        public override void Hide()
        {
            base.Hide();
            ModCache.titleScreenUI.SetSinglePlayerModeSelectButtonsVisibile(true);
        }

        public void OnExitButtonClicked()
        {
            Hide();
        }

        public void OnPlayButtonClicked()
        {
            TransitionManager.Instance.DoNonSceneTransition(transitionCoroutine());
        }

        public void OnLeaderboardButtonClicked()
        {
            ModUIConstants.ShowLeaderboard(base.transform, GameDataManager.Instance._endlessHighScores, "EndlessHighScores.json");
        }

        public void OnResetProgressButtonClicked()
        {
            ModUIUtils.MessagePopup(true, "Do you want to erase the progress?", "This action will reset your current progress in endless mode leaving the leaderboard untouched.\nThis action cannot be undone.", 150f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
            {
                GameData gameData = new GameData();
                gameData.HumanFacts = HumanFactsManager.Instance.GetRandomFactSet();
                gameData.RepairAnyMissingFields(true);
                gameData.SetDirty(true);
                GameDataManager.Instance._endlessModeData = gameData;

                RefreshProgressDisplays();
            });
        }

        private IEnumerator transitionCoroutine()
        {
            yield return new WaitForSecondsRealtime(0.25f);
            Hide();
            ModCache.titleScreenUI.OnPlayEndlessButtonClicked();
            yield return new WaitUntil(() => CharacterTracker.Instance._player);
            yield return new WaitForSecondsRealtime(0.1f);
            TransitionManager.Instance.EndTransition();
            yield break;
        }
    }
}
