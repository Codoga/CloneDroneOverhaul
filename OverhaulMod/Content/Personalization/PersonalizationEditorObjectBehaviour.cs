﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationEditorObjectBehaviour : MonoBehaviour
    {
        public string Name, Path;

        public bool IsRoot;

        public int UniqueIndex, NextUniqueIndex;

        public Dictionary<string, object> PropertyValues;

        private List<PersonalizationEditorObjectBehaviour> m_children;
        public List<PersonalizationEditorObjectBehaviour> children
        {
            get
            {
                List<PersonalizationEditorObjectBehaviour> list = m_children;
                if (list == null)
                {
                    list = new List<PersonalizationEditorObjectBehaviour>();
                    m_children = list;
                }
                else
                {
                    list.Clear();
                }

                Transform transform = base.transform;
                if (transform.childCount > 0)
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        if (child)
                        {
                            PersonalizationEditorObjectBehaviour personalizationEditorObjectBehaviour = child.GetComponent<PersonalizationEditorObjectBehaviour>();
                            if (personalizationEditorObjectBehaviour)
                            {
                                list.Add(personalizationEditorObjectBehaviour);
                            }
                        }
                    }

                return list;
            }
        }

        public int childrenCount
        {
            get
            {
                Transform transform = base.transform;

                int result = transform.childCount;
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    if (child)
                    {
                        PersonalizationEditorObjectBehaviour personalizationEditorObjectBehaviour = child.GetComponent<PersonalizationEditorObjectBehaviour>();
                        if (personalizationEditorObjectBehaviour)
                        {
                            result += personalizationEditorObjectBehaviour.childrenCount;
                        }
                    }
                }
                return result;
            }
        }

        public PersonalizationItemInfo ItemInfo;

        private void Awake()
        {
            m_children = new List<PersonalizationEditorObjectBehaviour>();
            if (PersonalizationEditorManager.IsInEditor())
                PersonalizationEditorObjectManager.Instance.AddInstantiatedObject(this);
        }

        private void OnDestroy()
        {
            PersonalizationEditorObjectManager.Instance.RemoveInstantiatedObject(this);
        }

        public T GetPropertyValue<T>(string name, T defaultValue)
        {
            if (!PropertyValues.TryGetValue(name, out object obj))
                return defaultValue;

            if (typeof(T) == typeof(float) && obj is double)
                return (T)(object)Convert.ToSingle(obj);

            if (typeof(T) == typeof(int) && obj is long)
                return (T)(object)Convert.ToInt32(obj);

            return (T)obj;
        }

        public void SetPropertyValue(string name, object value)
        {
            if (!PropertyValues.ContainsKey(name))
            {
                PropertyValues.Add(name, value);
            }
            else
            {
                PropertyValues[name] = value;
            }
        }

        public List<PersonalizationEditorObjectPropertyAttribute> GetProperties()
        {
            return PersonalizationEditorObjectManager.Instance.GetProperties(this);
        }

        public PersonalizationEditorObjectInfo Serialize()
        {
            if (IsRoot)
                NextUniqueIndex = PersonalizationEditorObjectManager.Instance.GetCurrentUniqueIndex();

            PersonalizationEditorObjectInfo objectInfo = new PersonalizationEditorObjectInfo()
            {
                Name = Name,
                Path = Path,
                IsRoot = IsRoot,
                UniqueIndex = UniqueIndex,
                NextUniqueIndex = NextUniqueIndex,
                PropertyValues = PropertyValues ?? new Dictionary<string, object>(),
                Children = new List<PersonalizationEditorObjectInfo>()
            };
            objectInfo.SetPosition(base.transform.localPosition);
            objectInfo.SetEulerAngles(base.transform.localEulerAngles);
            objectInfo.SetScale(base.transform.localScale);

            List<PersonalizationEditorObjectBehaviour> list = children;
            if (list == null)
                return objectInfo;

            foreach (PersonalizationEditorObjectBehaviour c in list)
            {
                if (!c || !c.gameObject)
                    continue;

                objectInfo.Children.Add(c.Serialize());
            }
            return objectInfo;
        }
    }
}
