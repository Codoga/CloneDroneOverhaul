﻿using System.Collections;
using TMPro;
using UnityEngine;

namespace CDOverhaul.ArenaRemaster
{
    public class ArenaRemasterEnemyCounter : MonoBehaviour
    {
        // Todo: Make a lvl editor objct that allows to move this counter anywhere you want

        private TextMeshPro _header;
        private TextMeshPro _label;

        private bool _isRefreshingText;

        public void Initialize(in ModdedObject moddedObject)
        {
            _label = moddedObject.GetObject<TextMeshPro>(0);
            _label.text = "-";
            _header = moddedObject.GetObject<TextMeshPro>(1);

            _ = OverhaulEventManager.AddEventListener<Character>(GlobalEvents.CharacterKilled, refreshEnemiesLeft, true);
            _ = OverhaulEventManager.AddEventListener<Character>(GlobalEvents.CharacterAdded, refreshEnemiesLeft, true);
            _ = OverhaulEventManager.AddEventListener(GlobalEvents.LevelEditorEnemyDeleted, refreshEnemiesLeft, true);
        }

        private void refreshEnemiesLeft()
        {
            refreshEnemiesLeft(null);
        }

        private void refreshEnemiesLeft(Character c)
        {
            if (_isRefreshingText)
            {
                _isRefreshingText = false;
                StopAllCoroutines();
            }
            _ = StartCoroutine(refreshText(CharacterTracker.Instance.GetNumEnemyCharactersAlive()));
        }

        private IEnumerator refreshText(int count)
        {
            _isRefreshingText = true;
            yield return new WaitForSeconds(0.2f);

            _label.text = count == 0 ? "-" : count.ToString();

            _isRefreshingText = false;
            yield break;
        }

        private void OnDestroy()
        {
            OverhaulEventManager.RemoveEventListener<Character>(GlobalEvents.CharacterKilled, refreshEnemiesLeft, true);
        }
    }
}
