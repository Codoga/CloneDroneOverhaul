﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CDOverhaul.Gameplay.Pets
{
    public class PetItem // Sounds weird..?
    {
        private static PetItem s_RecentlyCreatedItem;

        [NonSerialized]
        public GameObject Model;
        [NonSerialized]
        public string ModelName;

        public string Name;
        public string Description;
        public string Author;

        public PetBehaviourSettings BehaviourSettings;

        public string[] UserSettings; // ex: bool:Follow player, int:Position type

        public string UnlockedFor;
        public bool IsUnlocked(FirstPersonMover firstPersonMover)
        {
            if (string.IsNullOrEmpty(UnlockedFor))
                return true;

            if (!firstPersonMover)
                return false;

            if (firstPersonMover.IsForcedToUseLockedStuff())
                return true;

            return GameModeManager.IsSinglePlayer()
                ? UnlockedFor.Contains(PlayFabDataController.GetLocalPlayFabID()) : UnlockedFor.Contains(firstPersonMover.GetPlayFabID());
        }
        public bool IsUnlocked() => IsUnlocked(CharacterTracker.Instance ? CharacterTracker.Instance.GetPlayerRobot() : null);

        public static PetItem CreateNew(string name, string author, string description, string unlockedFor, string[] userSettings)
        {
            PetItem newItem = new PetItem()
            {
                Name = name,
                Author = author,
                Description = description,
                UnlockedFor = unlockedFor,
                BehaviourSettings = new PetBehaviourSettings(),
                UserSettings = userSettings
            };
            s_RecentlyCreatedItem = newItem;
            return newItem;
        }

        public static void AttachModelQuick(GameObject prefab)
        {
            if (s_RecentlyCreatedItem == null)
                return;

            s_RecentlyCreatedItem.Model = prefab;
            s_RecentlyCreatedItem.ModelName = prefab.name;
        }

        public static PetBehaviourSettings GetBehaviourSettingsQuick()
        {
            if (s_RecentlyCreatedItem == null)
                return null;

            return s_RecentlyCreatedItem.BehaviourSettings;
        }
    }
}