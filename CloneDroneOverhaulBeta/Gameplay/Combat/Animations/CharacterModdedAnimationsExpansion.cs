﻿using UnityEngine;

namespace CDOverhaul.Gameplay.Combat
{
    public class CharacterModdedAnimationsExpansion : CombatOverhaulMechanic
    {
        private Animation m_UpperAnimation;
        private bool m_HasUpperAnimator;
        private Animation m_LowerAnimation;
        private bool m_HasLowerAnimator;
        public void SetAnimationReferences(Animation upper, Animation lower)
        {
            m_UpperAnimation = upper;
            m_LowerAnimation = lower;
        }

        public bool IsPlayingCustomAnimation => IsPlayingCustomLowerAnimation || IsPlayingCustomUpperAnimation;
        public bool IsPlayingCustomUpperAnimation => !IsDisposedOrDestroyed() && (ForceSetIsPlayingUpperAnimation || (m_HasUpperAnimator && m_UpperAnimation != null && m_UpperAnimation.isPlaying));
        public bool IsPlayingCustomLowerAnimation => !IsDisposedOrDestroyed() && (ForceSetIsPlayingLowerAnimation || (m_HasLowerAnimator && m_LowerAnimation != null && m_LowerAnimation.isPlaying));

        public bool ForceSetIsPlayingUpperAnimation;
        public bool ForceSetIsPlayingLowerAnimation;

        public override void Start()
        {
            base.Start();
            m_HasUpperAnimator = HasAnimator(CombatOverhaulAnimatorType.Upper);
            m_HasLowerAnimator = HasAnimator(CombatOverhaulAnimatorType.Legs);
        }

        protected override void OnDisposed()
        {
            m_UpperAnimation = null;
            m_LowerAnimation = null;
        }

        public void PlayCustomAnimaton(string animationName)
        {
            if (IsDisposedOrDestroyed())
            {
                return;
            }

            if (m_HasUpperAnimator && m_UpperAnimation != null)
            {
                AnimationClip clip = m_UpperAnimation.GetClip(animationName);
                if(clip != null)
                {
                    m_UpperAnimation.clip = clip;
                    m_UpperAnimation.Play();
                }
            }
            if (m_HasLowerAnimator && m_LowerAnimation != null)
            {
                AnimationClip clip = m_LowerAnimation.GetClip(animationName);
                if (clip != null)
                {
                    m_LowerAnimation.clip = clip;
                    m_LowerAnimation.Play();
                }
            }
        }

        public void StopPlayingCustomAnimations()
        {
            if (IsDisposedOrDestroyed())
            {
                return;
            }

            if (m_HasUpperAnimator && m_UpperAnimation != null)
            {
                m_UpperAnimation.Stop();
            }
            if (m_HasLowerAnimator && m_LowerAnimation != null)
            {
                m_LowerAnimation.Stop();
            }
        }

        public string GetPlayingCustomAnimationName(CombatOverhaulAnimatorType animationType)
        {
            if (!HasAnimator(animationType))
            {
                return null;
            }

            switch (animationType)
            {
                case CombatOverhaulAnimatorType.Upper:
                    return m_UpperAnimation.clip.name;
                case CombatOverhaulAnimatorType.Legs:
                    return m_LowerAnimation.clip.name;
            }
            return null;
        }

        private void LateUpdate()
        {
            if(IsDisposedOrDestroyed() || CharacterModel == null || !FirstPersonMover.IsAlive())
            {
                return;
            }

            if(m_HasUpperAnimator) CharacterModel.UpperAnimator.enabled = !IsPlayingCustomUpperAnimation && !CharacterModel.IsManualUpperAnimationEnabled();
            if (m_HasLowerAnimator) CharacterModel.LegsAnimator.enabled = !IsPlayingCustomLowerAnimation && !CharacterModel.IsManualLegsAnimationEnabled();
        }
    }
}