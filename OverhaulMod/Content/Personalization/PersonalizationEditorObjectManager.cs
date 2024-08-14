using OverhaulMod.Utils;
using PicaVoxel;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationEditorObjectManager : Singleton<PersonalizationEditorObjectManager>
    {
        public const string OBJECT_SELECTION_CHANGED_EVENT = "PersonalizationEditorObjectSelectionChanged";

        private List<PersonalizationEditorObjectSpawnInfo> m_objectInfos;

        private Material m_volumeMaterial;

        private List<PersonalizationEditorObjectBehaviour> m_instantiatedObjects;

        private Dictionary<string, List<PersonalizationEditorObjectPropertyAttribute>> m_cachedProperties;

        private int m_nextUniqueIndex;

        public override void Awake()
        {
            base.Awake();

            m_cachedProperties = new Dictionary<string, List<PersonalizationEditorObjectPropertyAttribute>>();
            m_instantiatedObjects = new List<PersonalizationEditorObjectBehaviour>();
            m_objectInfos = new List<PersonalizationEditorObjectSpawnInfo>();
            addObjectInfo("Empty object", "Empty", instantiateEmpty);
            addObjectInfo("Model Renderer (.vox)", "Volume", instantiateVolume);
            addObjectInfo("Model Renderer (.cvm) [BETA]", "CvmModel", instantiateCvmModel);
            addObjectInfo("Fire particles (Sword)", "FireParticles_Sword", instantiateSwordFireParticles);
            addObjectInfo("Fire particles (Hammer)", "FireParticles_Hammer", instantiateHammerFireParticles);
            addObjectInfo("Fire particles (Spear)", "FireParticles_Spear", instantiateSpearFireParticles);
        }

        private void addObjectInfo(string name, string path, Func<Transform, GameObject> func)
        {
            PersonalizationEditorObjectSpawnInfo personalizationEditorObjectInfo = new PersonalizationEditorObjectSpawnInfo()
            {
                Name = name,
                Path = path,
                InstantiateFunction = func
            };
            m_objectInfos.Add(personalizationEditorObjectInfo);
        }

        public List<PersonalizationEditorObjectSpawnInfo> GetObjectInfos()
        {
            return m_objectInfos;
        }

        public void AddInstantiatedObject(PersonalizationEditorObjectBehaviour behaviour)
        {
            if (!m_instantiatedObjects.Contains(behaviour))
                m_instantiatedObjects.Add(behaviour);
        }

        public void RemoveInstantiatedObject(PersonalizationEditorObjectBehaviour behaviour)
        {
            _ = m_instantiatedObjects.Remove(behaviour);
        }

        public PersonalizationEditorObjectBehaviour GetInstantiatedObject(int uniqueIndex)
        {
            List<PersonalizationEditorObjectBehaviour> list = m_instantiatedObjects;
            if (list.IsNullOrEmpty())
                return null;

            int i = 0;
            do
            {
                PersonalizationEditorObjectBehaviour obj = list[i];
                if (obj && obj.UniqueIndex == uniqueIndex)
                    return obj;

                i++;
            } while (i < list.Count);

            return null;
        }

        public void SetCurrentRootNextUniqueIndex(int value)
        {
            m_nextUniqueIndex = value;
        }

        public int GetNextUniqueIndex()
        {
            m_nextUniqueIndex++;
            return m_nextUniqueIndex;
        }

        public int GetCurrentUniqueIndex()
        {
            return m_nextUniqueIndex;
        }

        public List<PersonalizationEditorObjectPropertyAttribute> GetProperties(PersonalizationEditorObjectBehaviour objectBehaviour)
        {
            if (m_cachedProperties.TryGetValue(objectBehaviour.Path, out List<PersonalizationEditorObjectPropertyAttribute> result))
                return result;

            result = new List<PersonalizationEditorObjectPropertyAttribute>();
            foreach (PersonalizationEditorObjectComponentBase c in objectBehaviour.GetComponents<PersonalizationEditorObjectComponentBase>())
            {
                PropertyInfo[] properties = c.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                if (!properties.IsNullOrEmpty())
                {
                    foreach (PropertyInfo property in properties)
                    {
                        PersonalizationEditorObjectPropertyAttribute attribute = property.GetCustomAttribute<PersonalizationEditorObjectPropertyAttribute>();
                        if (attribute != null)
                        {
                            attribute.propertyInfo = property;
                            result.Add(attribute);
                        }
                    }
                }
            }
            m_cachedProperties.Add(objectBehaviour.Path, result);
            return result;
        }

        private Material getVolumeMaterial()
        {
            Material material = m_volumeMaterial;
            if (material)
                return material;

            foreach (Material m in Resources.FindObjectsOfTypeAll<Material>())
                if (m && m.shader && m.name == "PicaVoxel PBR OneMinus Alpha Emissive" && m.shader.name == "PicaVoxel/PicaVoxel PBR OneMinus Alpha Emissive")
                {
                    material = m;
                    break;
                }

            m_volumeMaterial = material;
            return material;
        }

        private GameObject instantiateVolume(Transform parent)
        {
            SeveredVolumeGenerator severedVolumeGenerator = SeveredVolumeGenerator.Instance;
            MechBodyPart prefab = severedVolumeGenerator.EmptyBodyPartPrefab;
            prefab.enabled = false;
            MechBodyPart bodyPart = Instantiate(prefab, parent);
            prefab.enabled = true;
            GameObject gameObject = bodyPart.gameObject;
            Transform t = gameObject.transform;
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            t.localScale = Vector3.one;
            _ = gameObject.AddComponent<PersonalizationEditorObjectVolume>();
            _ = gameObject.AddComponent<PersonalizationEditorObjectVisibilityController>();
            Volume volume = gameObject.GetComponent<Volume>();
            volume.Material = getVolumeMaterial();
            Destroy(bodyPart);
            return gameObject;
        }

        private GameObject instantiateCvmModel(Transform parent)
        {
            GameObject obj = new GameObject();
            Transform t = obj.transform;
            t.SetParent(parent);
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            t.localScale = Vector3.one;
            obj.AddComponent<PersonalizationEditorObjectCVMModel>();
            _ = obj.AddComponent<PersonalizationEditorObjectVisibilityController>();
            return obj;
        }

        private GameObject instantiateSwordFireParticles(Transform parent)
        {
            Transform fireParticles = Instantiate(TransformUtils.FindChildRecursive(WeaponManager.Instance.FireSwordModelPrefab, "SwordFireVFX"), parent);
            Transform t = fireParticles.transform;
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            t.localScale = Vector3.one * 0.01f;
            _ = fireParticles.gameObject.AddComponent<PersonalizationEditorObjectVisibilityController>();
            _ = fireParticles.gameObject.AddComponent<PersonalizationEditorObjectFireParticles>();
            return fireParticles.gameObject;
        }

        private GameObject instantiateSpearFireParticles(Transform parent)
        {
            Transform fireParticles = Instantiate(TransformUtils.FindChildRecursive(WeaponManager.Instance.FireSpearModelPrefab, "SwordFireVFX (1)"), parent);
            Transform t = fireParticles.transform;
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            t.localScale = Vector3.one * 0.01f;
            _ = fireParticles.gameObject.AddComponent<PersonalizationEditorObjectVisibilityController>();
            _ = fireParticles.gameObject.AddComponent<PersonalizationEditorObjectFireParticles>();
            return fireParticles.gameObject;
        }

        private GameObject instantiateHammerFireParticles(Transform parent)
        {
            Transform fireParticles = Instantiate(TransformUtils.FindChildRecursive(WeaponManager.Instance.FireHammerModelPrefab, "FireVFX (1)"), parent);
            Transform t = fireParticles.transform;
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            t.localScale = Vector3.one * 0.01f;
            _ = fireParticles.gameObject.AddComponent<PersonalizationEditorObjectVisibilityController>();
            _ = fireParticles.gameObject.AddComponent<PersonalizationEditorObjectFireParticles>();
            return fireParticles.gameObject;
        }

        private GameObject instantiateEmpty(Transform parent)
        {
            GameObject obj = new GameObject();
            Transform t = obj.transform;
            t.SetParent(parent);
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            t.localScale = Vector3.one;
            return obj;
        }

        public PersonalizationEditorObjectSpawnInfo GetObjectInfo(string path)
        {
            foreach (PersonalizationEditorObjectSpawnInfo info in m_objectInfos)
                if (info.Path == path)
                    return info;

            return null;
        }

        public PersonalizationEditorObjectBehaviour PlaceObject(string path, Transform parent = null, bool assignEditableValues = true)
        {
            PersonalizationEditorObjectSpawnInfo objectInfo = GetObjectInfo(path);
            if (objectInfo == null)
                return null;

            GameObject gameObject = objectInfo.Instantiate(parent);
            gameObject.name = gameObject.name.Replace("(Clone)", string.Empty);
            PersonalizationEditorObjectBehaviour personalizationEditorObject = gameObject.AddComponent<PersonalizationEditorObjectBehaviour>();
            personalizationEditorObject.Path = objectInfo.Path;

            if (assignEditableValues)
            {
                personalizationEditorObject.Name = objectInfo.Name;
                personalizationEditorObject.PropertyValues = new Dictionary<string, object>();
                if (!objectInfo.Properties.IsNullOrEmpty())
                    foreach (PersonalizationEditorObjectPropertyValueInfo p in objectInfo.Properties)
                        personalizationEditorObject.PropertyValues.Add(p.PropertyName, default);
            }

            return personalizationEditorObject;
        }
    }
}
