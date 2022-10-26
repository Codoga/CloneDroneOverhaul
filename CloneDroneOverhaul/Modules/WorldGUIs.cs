﻿using CloneDroneOverhaul.PooledPrefabs;
using CloneDroneOverhaul.Utilities;
using ModLibrary;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;
using System.Collections.Generic;

namespace CloneDroneOverhaul.Modules
{
    public class WorldGUIs : ModuleBase
    {
        Transform MindTransfers;
        Transform MindTransferIcon;
        Transform MindTransferContainer;
        Transform MindTransferToFollow;
        float TimeToHideTransfers;
        float YOffset;
        Bounds _combinedBounds;

        RobotShortInformation CurrentPlayerInfo;
        RobotShortInformation OldPlayerInfo;

        static bool TransferedBefore;

        public override void Start()
        {
            Functions = new string[]
            {
                "onPlayerSet"
            };

            MindTransfers = UnityEngine.GameObject.Instantiate(AssetLoader.GetObjectFromFile("cdo_rw_stuff", "MindTransfersLeft")).transform;
            MindTransfers.gameObject.SetActive(false);
            MindTransferContainer = MindTransfers.GetComponent<ModdedObject>().GetObjectFromList<RectTransform>(1);
            MindTransferIcon = MindTransfers.GetComponent<ModdedObject>().GetObjectFromList<RectTransform>(0);
            MindTransfers.gameObject.AddComponent<FaceTowardsCamera>();
        }

        public override void RunFunction(string name, object[] arguments)
        {
            if (name == Functions[0])
            {
                CurrentPlayerInfo = arguments[0] as RobotShortInformation;
                OldPlayerInfo = arguments[1] as RobotShortInformation;
                OverhaulMain.Timer.AddNoArgActionToCompleteNextFrame(OnTransfer);
            }
        }

        private void OnTransfer()
        {
            // Show mind transfers using top of the robot's head
            if (GameModeManager.ConsciousnessTransferToKillerEnabled() && !CurrentPlayerInfo.IsNull && CurrentPlayerInfo.Instance is FirstPersonMover)
            {
                if (!TransferedBefore)
                {
                    CloneDroneOverhaul.UI.Notifications.Notification notif = new UI.Notifications.Notification();
                    notif.SetUp("Press 'TAB' to view your mind transfers", "", 10, new Vector2(450, 52), Color.clear, new UI.Notifications.Notification.NotificationButton[] { });
                    TransferedBefore = true;
                }
                FirstPersonMover mover = (FirstPersonMover)CurrentPlayerInfo.Instance;
                Transform head = TransformUtils.FindChildRecursive(CurrentPlayerInfo.Instance.transform, "Head");
                MindTransferToFollow = head;
                MindTransferContainer.transform.localScale = Vector3.zero;
                iTween.ScaleTo(MindTransferContainer.gameObject, Vector3.one, Time.timeScale * 0.5f);
                TimeToHideTransfers = Time.time + 5f;
                YOffset = 0;
                _combinedBounds = new Bounds();
                RefreshMindTransfers();

                if (head == null || mover.CharacterType == EnemyType.EmperorCombat || mover.CharacterType == EnemyType.EmperorNonCombat)
                {
                    OverhaulMain.GUI.GetGUI<UI.BackupMindTransfersUI>().RefreshMindTransfers();
                    MindTransferToFollow = null;
                    return;
                }
                Renderer[] renderers = head.gameObject.GetComponentsInChildren<Renderer>();
                if (renderers.Length != 0)
                {
                    this._combinedBounds = renderers[0].bounds;
                    int i = 1;
                    int len = renderers.Length;
                    while (i < len)
                    {
                        this._combinedBounds.Encapsulate(renderers[i].bounds);
                        int num = i;
                        i = num + 1;
                    }
                }
                YOffset = Mathf.Clamp((_combinedBounds.max.y - 1.25f), 1.25f, 3f);
            }
        }

        private void RefreshMindTransfers()
        {
            TransformUtils.DestroyAllChildren(MindTransferContainer);
            for(int i = 0; i < GameDataManager.Instance.GetNumConsciousnessTransfersLeft(); i++)
            {
                Transform trans = GameObject.Instantiate(MindTransferIcon, MindTransferContainer);
                trans.gameObject.SetActive(true);
            }
        }

        public override void OnNewFrame()
        {
            bool work = MindTransferToFollow != null && GameModeManager.ConsciousnessTransferToKillerEnabled();
            MindTransfers.gameObject.SetActive(work);
            if (work)
            {
                MindTransfers.position = new Vector3(MindTransferToFollow.position.x, MindTransferToFollow.position.y + YOffset, MindTransferToFollow.position.z);
                if (TimeToHideTransfers != -1 && Time.time >= TimeToHideTransfers)
                {
                    TimeToHideTransfers = -1;
                    iTween.ScaleTo(MindTransferContainer.gameObject, Vector3.zero, Time.timeScale * 0.5f);
                }
            }
            if (Input.GetKeyDown(KeyCode.Tab) && GameModeManager.ConsciousnessTransferToKillerEnabled())
            {
                OnTransfer();
            }
        }
    }
}
