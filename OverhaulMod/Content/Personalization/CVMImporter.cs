using Bolt;
using OverhaulMod.Engine;
using PicaVoxel;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OverhaulMod.Content.Personalization
{
    // most of the code is from Custom robot model editor mod by X606
    public static class CVMImporter
    {
        public static bool InstantiateModel(SaveClass saveClass, WeaponType type, WeaponVariant variant, Transform parent, out string error)
        {
            WeaponSpecialType weaponSpecialType = GetWeaponSpecialType(type, variant);
            if(weaponSpecialType != (WeaponSpecialType)(-1))
            {
                if (!saveClass.CustomSpecialWeaponModels.ContainsKey(weaponSpecialType))
                {
                    error = $"The model doesn't have the edited model of {weaponSpecialType}";
                    return false;
                }

                Transform t;
                switch (weaponSpecialType)
                {
                    case WeaponSpecialType.FireHammer:
                        t = WeaponManager.Instance.FireHammerModelPrefab;
                        break;
                    case WeaponSpecialType.FireSword:
                        t = WeaponManager.Instance.FireSwordModelPrefab;
                        break;
                    case WeaponSpecialType.GreatSword:
                        t = WeaponManager.Instance.GreatSwordModelPrefab;
                        break;
                    case WeaponSpecialType.GreatFireSword:
                        t = WeaponManager.Instance.FireGreatSwordModelPrefab;
                        break;
                    default:
                        t = null;
                        break;
                }

                if (!t)
                {
                    error = $"Could not find the base model for {weaponSpecialType}";
                    return false;
                }

                Volume componentInChildren = t.GetComponentInChildren<Volume>(true);
                if (!componentInChildren)
                {
                    error = $"Could not find the base model's Volume for {weaponSpecialType}";
                    return false;
                }

                Volume volume1 = UnityEngine.Object.Instantiate(componentInChildren, parent);
                volume1.gameObject.SetActive(true);
                foreach (KeyValuePair<PicaVoxelPoint, Voxel> kv in saveClass.CustomSpecialWeaponModels[weaponSpecialType])
                {
                    volume1.GetCurrentFrame().SetVoxelAtArrayPosition(kv.Key, kv.Value);
                }
                foreach (var replaceColor in volume1.GetComponents<ReplaceVoxelColor>())
                {
                    ReplaceVoxelColor.ReplaceColors(volume1, replaceColor.Old, replaceColor.New);
                }
                volume1.UpdateAllChunks();

                error = null;
                return true;
            }

            FirstPersonMover firstPersonMover = null;
            if (saveClass.Base == EnemyType.None)
            {
                firstPersonMover = PrefabDatabase.Find(new PrefabId() { Value = 59 }).GetComponent<FirstPersonMover>();
            }
            else
            {
                EnemyConfiguration configuration = EnemyFactory.Instance.GetEnemyConfiguration(saveClass.Base);
                if (configuration != null && configuration.EnemyPrefab)
                {
                    firstPersonMover = configuration.EnemyPrefab.GetComponent<FirstPersonMover>();
                }
            }

            if (!firstPersonMover)
            {
                error = $"Could not find base enemy {saveClass.Base}";
                ModDebug.Log(error);
                return false;
            }

            if (!firstPersonMover.CharacterModelPrefab)
            {
                error = $"The base enemy {saveClass.Base} doesn't have a model prefab";
                ModDebug.Log(error);
                return false;
            }

            GameObject gameObject = firstPersonMover.CharacterModelPrefab.gameObject;
            Volume[] componentsInChildren = gameObject.GetComponentsInChildren<Volume>(true);
            if (componentsInChildren.Length != saveClass.ModifiedVoxels.Length)
            {
                error = $"The model is corrupted";
                ModDebug.Log(error);
                return false;
            }

            int i = -1;
            foreach (Volume component in componentsInChildren)
            {
                i++;

                WeaponModel weaponModel = component.transform.GetComponentInParents<WeaponModel>();
                if (weaponModel && weaponModel.WeaponType == type)
                {
                    Volume volume = UnityEngine.Object.Instantiate(component, parent);
                    volume.gameObject.SetActive(true);
                    foreach (KeyValuePair<PicaVoxelPoint, Voxel> kv in saveClass.ModifiedVoxels[i])
                    {
                        volume.GetCurrentFrame().SetVoxelAtArrayPosition(kv.Key, kv.Value);
                    }                    
                    foreach(var replaceColor in volume.GetComponents<ReplaceVoxelColor>())
                    {
                        ReplaceVoxelColor.ReplaceColors(volume, replaceColor.Old, replaceColor.New);
                    }
                    volume.UpdateAllChunks();

                    error = null;
                    return true;
                }
            }

            error = $"The base enemy {saveClass.Base} model doesn't have required weapon {type}";
            ModDebug.Log(error);
            return false;
        }

        public static SaveClass LoadModel(string path)
        {
            if (!File.Exists(path))
                return null;

            SaveClass saveClass = new SaveClass();
            saveClass.LoadData(Utils.ModIOUtils.ReadBytes(path));
            return saveClass;
        }

        public static T GetComponentInParents<T>(this Transform transform) where T : Component
        {
            T t = default;
            Transform transform2 = transform;
            while (!t && transform2)
            {
                t = transform2.GetComponent<T>();
                transform2 = transform2.parent;
            }
            return t;
        }

        public static WeaponSpecialType GetWeaponSpecialType(WeaponType weaponType, WeaponVariant weaponVariant)
        {
            if(weaponType == WeaponType.Sword)
            {
                if (weaponVariant == WeaponVariant.NormalMultiplayer)
                    return WeaponSpecialType.GreatSword;
                else if (weaponVariant == WeaponVariant.OnFireMultiplayer)
                    return WeaponSpecialType.GreatFireSword;
                else if (weaponVariant == WeaponVariant.OnFire)
                    return WeaponSpecialType.FireSword;
            }
            else if (weaponType == WeaponType.Hammer)
            {
                if (weaponVariant == WeaponVariant.OnFire)
                    return WeaponSpecialType.FireHammer;
            }
            return (WeaponSpecialType)(-1);
        }

        public enum WeaponSpecialType
        {
            EMPHammer,
            FireHammer,
            FireSword,
            GreatSword,
            GreatFireSword
        }

        public class SaveClass
        {
            public EnemyType Base;

            public Dictionary<PicaVoxelPoint, Voxel>[] ModifiedVoxels;

            public Dictionary<WeaponSpecialType, Dictionary<PicaVoxelPoint, Voxel>> CustomSpecialWeaponModels;

            public void LoadData(byte[] data)
            {
                int num = 0;
                Base = (EnemyType)BitConverter.ToInt32(ReadData(data, 4, ref num), 0);
                int num2 = BitConverter.ToInt32(ReadData(data, 4, ref num), 0);
                ModifiedVoxels = new Dictionary<PicaVoxelPoint, Voxel>[num2];
                for (int i = 0; i < num2; i++)
                {
                    ModifiedVoxels[i] = new Dictionary<PicaVoxelPoint, Voxel>();
                    int num3 = BitConverter.ToInt32(ReadData(data, 4, ref num), 0);
                    for (int j = 0; j < num3; j++)
                    {
                        KeyValuePair<PicaVoxelPoint, Voxel> keyValuePair = ReadVoxelInfoFromList(data, ref num);
                        ModifiedVoxels[i].Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }
                CustomSpecialWeaponModels = new Dictionary<WeaponSpecialType, Dictionary<PicaVoxelPoint, Voxel>>();
                bool flag = num >= data.Length;
                if (!flag)
                {
                    int num4 = BitConverter.ToInt32(ReadData(data, 4, ref num), 0);
                    for (int k = 0; k < num4; k++)
                    {
                        WeaponSpecialType key = (WeaponSpecialType)BitConverter.ToInt32(ReadData(data, 4, ref num), 0);
                        int num5 = BitConverter.ToInt32(ReadData(data, 4, ref num), 0);
                        Dictionary<PicaVoxelPoint, Voxel> dictionary = new Dictionary<PicaVoxelPoint, Voxel>();
                        for (int l = 0; l < num5; l++)
                        {
                            KeyValuePair<PicaVoxelPoint, Voxel> keyValuePair2 = ReadVoxelInfoFromList(data, ref num);
                            dictionary.Add(keyValuePair2.Key, keyValuePair2.Value);
                        }
                        CustomSpecialWeaponModels.Add(key, dictionary);
                    }
                }
            }

            private byte[] ReadData(byte[] data, int length, ref int index)
            {
                byte[] array = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    array[i] = data[index + i];
                }
                index += length;
                return array;
            }

            private KeyValuePair<PicaVoxelPoint, Voxel> ReadVoxelInfoFromList(byte[] data, ref int index)
            {
                PicaVoxelPoint picaVoxelPoint = new PicaVoxelPoint(0, 0, 0)
                {
                    X = BitConverter.ToInt32(ReadData(data, 4, ref index), 0),
                    Y = BitConverter.ToInt32(ReadData(data, 4, ref index), 0),
                    Z = BitConverter.ToInt32(ReadData(data, 4, ref index), 0)
                };
                Voxel value = default;
                byte[] array = ReadData(data, 4, ref index);
                value.Color = new Color32(array[0], array[1], array[2], array[3]);
                value.State = (VoxelState)ReadData(data, 1, ref index)[0];
                value.Value = ReadData(data, 1, ref index)[0];
                return new KeyValuePair<PicaVoxelPoint, Voxel>(picaVoxelPoint, value);
            }
        }
    }
}
