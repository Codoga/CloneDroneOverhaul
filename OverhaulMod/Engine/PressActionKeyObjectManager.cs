﻿using OverhaulMod.UI;
using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OverhaulMod.Engine
{
    public class PressActionKeyObjectManager : Singleton<PressActionKeyObjectManager>
    {
        [ModSetting(ModSettingsConstants.ENABLE_PRESS_BUTTON_TRIGGER_DESCRIPTION_REWORK, true)]
        public static bool EnablePressButtonTriggerDescriptionRework;

        public static readonly Color BGGlowColor = new Color(0.65f, 0.75f, 1f, 0.3f);

        private List<LevelEditorUseButtonTrigger> m_triggers;

        private UIPressActionKeyDescription m_pressActionKeyDescription;

        private LevelEditorUseButtonTrigger m_prevNearestTrigger;

        private float m_timeToHideText;

        public override void Awake()
        {
            base.Awake();

            m_triggers = new List<LevelEditorUseButtonTrigger>();
            m_timeToHideText = -1f;
        }

        private void Update()
        {
            if(m_timeToHideText != -1f && Time.unscaledTime >= m_timeToHideText)
            {
                m_timeToHideText = -1f;
                HideDescription();
            }

            if (EnablePressButtonTriggerDescriptionRework && ModTime.hasFixedUpdated)
            {
                CharacterTracker characterTracker = CharacterTracker.Instance;

                float dist = float.MaxValue;
                LevelEditorUseButtonTrigger nearestTrigger = null;
                foreach (var trigger in m_triggers)
                {
                    if (!trigger || !trigger.CanBeActivated())
                        continue;

                    float newDist = characterTracker.GetDistanceToPlayer(trigger.transform.position);
                    if(newDist < dist)
                    {
                        dist = newDist;
                        nearestTrigger = trigger;
                    }
                }

                if(m_prevNearestTrigger != nearestTrigger)
                {
                    m_prevNearestTrigger = nearestTrigger;
                    if (nearestTrigger != null)
                    {
                        string description = nearestTrigger.Description;
                        LocalizationManager localizationManager = LocalizationManager.Instance;
                        if (localizationManager && localizationManager.HasTranslatedString(description))
                            description = localizationManager.GetTranslatedString(description);

                        ShowDescription(description);
                    }
                    else
                    {
                        HideDescription();
                    }
                }
            }
        }

        public void SetTriggerRegistered(LevelEditorUseButtonTrigger trigger, bool value)
        {
            if (trigger == null)
                return;

            if (value && !m_triggers.Contains(trigger))
                m_triggers.Add(trigger);
            else if(!value)
                m_triggers.Remove(trigger);
        }

        public void ShowThenHideDescription(string description, float time)
        {
            m_timeToHideText = Time.unscaledTime + time;
            ShowDescription(description);
        }

        public void ShowDescription(string description)
        {
            UIPressActionKeyDescription actionDescription = m_pressActionKeyDescription;
            if (!actionDescription)
            {
                actionDescription = ModUIConstants.ShowPressActionKeyDescription();
                m_pressActionKeyDescription = actionDescription;
            }

            if (actionDescription)
                actionDescription.ShowText(description);
        }

        public void HideDescription()
        {
            UIPressActionKeyDescription actionDescription = m_pressActionKeyDescription;
            if (actionDescription)
                actionDescription.HideText();
        }
    }
}
