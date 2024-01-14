﻿using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationEditorManager : Singleton<PersonalizationEditorManager>
    {
        public void StartEditorGameMode()
        {
            GameFlowManager.Instance._gameMode = (GameMode)2500;
            LevelManager.Instance._currentLevelHidesTheArena = true;

            LevelManager.Instance.CleanUpLevelThisFrame();
            GameFlowManager.Instance.HideTitleScreen(false);

            GameUIRoot.Instance.LoadingScreen.Show();

            GameDataManager.Instance.SaveHighScoreDataWithoutModifyingIt();
            CacheManager.Instance.CreateOrClearInstance();
            SingleplayerServerStarter.Instance.StartServerThenCall(delegate
            {
                GameUIRoot.Instance.LoadingScreen.Hide();
                GlobalEventManager.Instance.Dispatch(GlobalEvents.LevelSpawned);

                new GameObject("Camera", new Type[] { typeof(Camera) });

                ModUIConstants.ShowPersonalizationEditorUI();
            });
        }
    }
}
