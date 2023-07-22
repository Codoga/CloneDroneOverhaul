﻿using ModLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CDOverhaul.Gameplay
{
    public class OverhaulAchievementsController : OverhaulGameplayController
    {
        private List<string> m_AddedAchievements = new List<string>();

        public override void Initialize()
        {
            base.Initialize();
        }

        public T CreateAchievement<T>(string name, string description, Sprite previewImage) where T : OverhaulAchievement
        {
            if (m_AddedAchievements.Contains(name))
                return null;

            T achievement = ScriptableObject.CreateInstance<T>();
            achievement.Name = name;
            achievement.Description = description;
            achievement.SteamAchievementID = "none";
            achievement.AchievementID = "Overhaul_" + name;
            achievement.Image = previewImage;

            GameplayAchievementManager manager = GameplayAchievementManager.Instance;
            List<GameplayAchievement> list = manager.Achievements.ToList();
            list.Add(achievement);
            manager.Achievements = list.ToArray();

            m_AddedAchievements.Add(name);
            return achievement;
        }
    }
}