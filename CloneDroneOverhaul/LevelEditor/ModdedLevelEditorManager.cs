﻿using ModLibrary;
using UnityEngine;
using System.Collections.Generic;

namespace CloneDroneOverhaul.LevelEditor
{
    public class ModdedLevelEditorManager : Modules.ModuleBase
    {
        public static ModdedLevelEditorManager Instance;

        private bool _hasSpawnedSpecialObjects;
        private GameObject Grid;

        public override bool IsEnabled()
        {
            return true;
        }

        public override void OnActivated()
        {
            Instance = this;
            Functions.Add("onLevelEditorStarted");
        }

        public override void OnNewFrame()
        {
            if (!_hasSpawnedSpecialObjects)
            {
                return;
            }
            List<ObjectPlacedInLevel> transforms = LevelEditorObjectPlacementManager.Instance.GetSelectedSceneObjects();
            if(transforms.Count == 0)
            {
                return;
            }
            var bound = new Bounds(transforms[0].transform.position, Vector3.zero);
            for (int i = 1; i < transforms.Count; i++)
            {
                bound.Encapsulate(transforms[i].transform.position);
            }
            Vector3 Center = bound.center;
            Grid.transform.position = Center;
        }

        public void SpawnSpecialObjects()
        {
            Grid = UnityEngine.GameObject.Instantiate(OverhaulCacheManager.GetCached<GameObject>("LevelEditor_Grid"));
            Grid.transform.position = Vector3.zero;
            _hasSpawnedSpecialObjects = true;
        }

        public override void RunFunction(string name, object[] arguments)
        {
            if (!OverhaulMain.GetSetting<bool>("Levels.Editor.New Level Editor"))
            {
                return;
            }
            if (name == "onLevelEditorStarted")
            {
                SpawnSpecialObjects();
            }
        }

        public ObjectPlacedInLevel PlaceObject(LevelObjectEntry objectPlacedLevelObjectEntry, Transform levelRoot)
        {
            Transform transform = OverhaulCacheManager.GetAndCacheLevelEditorObject(objectPlacedLevelObjectEntry.PathUnderResources);

            if (transform == null)
            {
                transform = Resources.Load<Transform>(Singleton<LevelObjectsLibraryManager>.Instance.GetRenamedPath(objectPlacedLevelObjectEntry.PathUnderResources));
                if (transform == null)
                {
                    if (objectPlacedLevelObjectEntry.PathUnderResources.StartsWith("modded/"))
                    {
                        Debug.LogError("Looks like this level requires a mod called " + objectPlacedLevelObjectEntry.PathUnderResources.Split(new char[]
                        {
                        '/'
                        })[1] + "ask around in the discord on how you fix this :)");
                    }
                    else
                    {
                        Debug.LogError("PlaceObjectInLevelRoot, Can't find asset: " + objectPlacedLevelObjectEntry.PathUnderResources);
                    }
                    return null;
                }
            }

            Transform transform2 = UnityEngine.Object.Instantiate<Transform>(transform);
            transform2.SetParent(levelRoot, false);

            if (!objectPlacedLevelObjectEntry.IsSection())
            {
                transform2.gameObject.AddComponent<SectionMember>();
            }

            ObjectPlacedInLevel objectPlacedInLevel = transform2.gameObject.AddComponent<ObjectPlacedInLevel>();
            objectPlacedInLevel.LevelObjectEntry = objectPlacedLevelObjectEntry;
            objectPlacedInLevel.Initialize(levelRoot);

            object[] args = new object[]
            {
                objectPlacedInLevel
            };
            Singleton<LevelEditorObjectPlacementManager>.Instance.CallPrivateMethod("registerObjectInAllObjectList", args);

            return objectPlacedInLevel;
        }

    }
}