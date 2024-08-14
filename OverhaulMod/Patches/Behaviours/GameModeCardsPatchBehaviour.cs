using OverhaulMod.Content;
using OverhaulMod.Utils;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OverhaulMod.Patches.Behaviours
{
    internal class GameModeCardsPatchBehaviour : GamePatchBehaviour
    {
        private UnityEvent m_storyModeEvent, m_endlessModeEvent, m_soloChallengesEvent, m_coopEndlessEvent, m_coopChallengesEvent, m_lastBotStandingEvent, m_duelEvent;

        public override void Patch()
        {
            patchGameModeCardPrefab(ModCache.titleScreenUI.MultiplayerModeSelectScreen);
            patchGameModeCardPrefab(ModCache.titleScreenUI.SingleplayerModeSelectScreen);

            GameModeCardData[] multiplayerDatas = ModCache.titleScreenUI.MultiplayerModeSelectScreen.GameModeData;
            GameModeCardData[] singleplayerDatas = ModCache.titleScreenUI.SingleplayerModeSelectScreen.GameModeData;

            m_storyModeEvent = singleplayerDatas[0].ClickedCallback;
            m_endlessModeEvent = singleplayerDatas[1].ClickedCallback;
            m_soloChallengesEvent = singleplayerDatas[2].ClickedCallback;
            m_coopEndlessEvent = multiplayerDatas[0].ClickedCallback;
            m_coopChallengesEvent = multiplayerDatas[1].ClickedCallback;
            m_lastBotStandingEvent = multiplayerDatas[2].ClickedCallback;
            m_duelEvent = multiplayerDatas[3].ClickedCallback;

            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.ChapterSelectMenuRework))
            {
                UnityEvent storyModeEvent = new UnityEvent();
                storyModeEvent.AddListener(delegate
                {
                    if (!ModUIManager.ShowChapterSelectionMenuRework)
                    {
                        ModCache.titleScreenUI.OnPlayStoryButtonClicked();
                        return;
                    }
                    _ = ModUIConstants.ShowChapterSelectMenu();
                });
                singleplayerDatas[0].ClickedCallback = storyModeEvent;
            }
            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.EndlessModeMenu))
            {
                UnityEvent endlessModeEvent = new UnityEvent();
                endlessModeEvent.AddListener(delegate
                {
                    if (!ModUIManager.ShowEndlessModeMenu)
                    {
                        ModCache.titleScreenUI.OnPlayEndlessButtonClicked();
                        return;
                    }
                    _ = ModUIConstants.ShowEndlessModeMenu();
                });
                singleplayerDatas[1].ClickedCallback = endlessModeEvent;
            }

            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.DuelInviteMenuRework))
            {
                UnityEvent coopEndlessModeEvent = new UnityEvent();
                coopEndlessModeEvent.AddListener(delegate
                {
                    if (!ModUIManager.ShowDuelInviteMenuRework)
                    {
                        ModCache.titleScreenUI.OnPlayCoopButtonClicked();
                        return;
                    }
                    _ = ModUIConstants.ShowDuelInviteMenuRework(GameMode.EndlessCoop);
                });
                multiplayerDatas[0].ClickedCallback = coopEndlessModeEvent;

                UnityEvent coopChallengesModeEvent = new UnityEvent();
                coopChallengesModeEvent.AddListener(delegate
                {
                    if (!ModUIManager.ShowDuelInviteMenuRework)
                    {
                        ModCache.titleScreenUI.OnPlayCoopChallengesButtonClicked();
                        return;
                    }
                    _ = ModUIConstants.ShowDuelInviteMenuRework(GameMode.CoopChallenge);
                });
                multiplayerDatas[1].ClickedCallback = coopChallengesModeEvent;

                UnityEvent battleRoyaleModeEvent = new UnityEvent();
                battleRoyaleModeEvent.AddListener(delegate
                {
                    if (!ModUIManager.ShowDuelInviteMenuRework)
                    {
                        ModCache.titleScreenUI.OnPlayBattleRoyaleButtonClicked();
                        return;
                    }
                    _ = ModUIConstants.ShowDuelInviteMenuRework(GameMode.BattleRoyale);
                });
                multiplayerDatas[2].ClickedCallback = battleRoyaleModeEvent;

                UnityEvent duelModeEvent = new UnityEvent();
                duelModeEvent.AddListener(delegate
                {
                    if (!ModUIManager.ShowDuelInviteMenuRework)
                    {
                        ModCache.titleScreenUI.OnDuelInviteMenuClicked();
                        return;
                    }
                    _ = ModUIConstants.ShowDuelInviteMenuRework(GameMode.MultiplayerDuel);
                });
                multiplayerDatas[3].ClickedCallback = duelModeEvent;
            }

            UnityEvent spChallengesEvent = new UnityEvent();
            spChallengesEvent.AddListener(delegate
            {
                if (!ModUIManager.ShowChallengesMenuRework)
                {
                    ModCache.titleScreenUI.OnChallengeButtonClicked();
                    return;
                }
                _ = ModUIConstants.ShowChallengesMenuRework(false, false);
            });
            singleplayerDatas[2].ClickedCallback = spChallengesEvent;

            ReplaceSprite(singleplayerDatas, 1, "Endless");
            ReplaceSprite(singleplayerDatas, 2, "Bot");
            ReplaceSprite(multiplayerDatas, 0, "Humans");
            ReplaceSprite(multiplayerDatas, 1, "Bot");
            ReplaceSprite(multiplayerDatas, 2, "Raptor");
            ReplaceSprite(multiplayerDatas, 3, "DuelHumans");
        }

        public override void UnPatch()
        {
            GameModeCardData[] multiplayerDatas = ModCache.titleScreenUI.MultiplayerModeSelectScreen.GameModeData;
            GameModeCardData[] singleplayerDatas = ModCache.titleScreenUI.SingleplayerModeSelectScreen.GameModeData;

            singleplayerDatas[0].ClickedCallback = m_storyModeEvent;
            singleplayerDatas[1].ClickedCallback = m_endlessModeEvent;
            singleplayerDatas[2].ClickedCallback = m_soloChallengesEvent;
            multiplayerDatas[0].ClickedCallback = m_coopEndlessEvent;
            multiplayerDatas[1].ClickedCallback = m_coopChallengesEvent;
            multiplayerDatas[2].ClickedCallback = m_lastBotStandingEvent;
            multiplayerDatas[3].ClickedCallback = m_duelEvent;
        }

        public void ReplaceSprite(GameModeCardData[] array, int index, string imageName)
        {
            string key = $"OverhaulGameModeImage_{imageName}";
            if (ModAdvancedCache.TryGet(key, out Sprite item))
            {
                array[index].ThumbnailSprite = item;
                return;
            }

            RepositoryManager.Instance.GetLocalTexture(Path.Combine(ModCore.texturesFolder, "gamemodes", $"{imageName}.jpg"), delegate (Texture2D texture2D)
            {
                Sprite sprite = texture2D.ToSprite();
                array[index].ThumbnailSprite = sprite;
                ModAdvancedCache.Add(key, sprite);
            }, null, out _);
        }

        private void patchGameModeCardPrefab(GameModeSelectScreen gameModeSelectScreen)
        {
            RectTransform cardTransform = gameModeSelectScreen.ButtonPrefab.transform as RectTransform;
            RectTransform cardBG = cardTransform.FindChildRecursive("BG") as RectTransform;
            cardBG.anchoredPosition = Vector2.zero;
            cardBG.sizeDelta = Vector2.zero;
            cardBG.GetComponent<Image>().color = new Color(0.5f, 0.7f, 1f, 0.3f);
            UIColorSwapper cardBGColors = cardBG.GetComponent<UIColorSwapper>();
            cardBGColors.ColorVariants[0] = new Color(0.5f, 0.7f, 1f, 0.3f);
            cardBGColors.ColorVariants[1] = new Color(0f, 1f, 0.25f, 0.65f);
            RectTransform cardImage = cardBG.FindChildRecursive("GameModeImage") as RectTransform;
            cardImage.sizeDelta = new Vector2(170f, 200f);

            Transform cardButtonBG = cardTransform.FindChildRecursive("buttonBG");
            cardButtonBG.gameObject.SetActive(false);

            Transform cardGamemodeButton = cardTransform.FindChildRecursive("GameModeButton");
            cardGamemodeButton.GetComponent<Image>().enabled = false;
            cardGamemodeButton.FindChildRecursive("Image (2)").gameObject.SetActive(false);

            Transform cardHeadingLabel = cardGamemodeButton.FindChildRecursive("headingLabel");
            if (!cardHeadingLabel.GetComponent<Outline>())
            {
                _ = cardHeadingLabel.gameObject.AddComponent<Outline>();
                cardHeadingLabel.gameObject.AddComponent<Shadow>().effectDistance = Vector2.one * -1.5f;
            }
            cardHeadingLabel.GetComponent<Text>().color = Color.white;
            cardHeadingLabel.localPosition = new Vector3(0, 5f);
            UIColorSwapper cardGamemodeButtonColors = cardHeadingLabel.GetComponent<UIColorSwapper>();
            cardGamemodeButtonColors.ColorVariants[0] = Color.white;
            cardGamemodeButtonColors.ColorVariants[1] = new Color(0.3f, 1f, 0.35f, 1f);
        }
    }
}
