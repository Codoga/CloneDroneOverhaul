using OverhaulMod.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationEditorObjectInfo
    {
        public float[] PositionArray, EulerAnglesArray, ScaleArray;

        public string Name, Path;

        public bool IsRoot;

        public List<PersonalizationEditorObjectInfo> Children;

        public Dictionary<string, object> PropertyValues;

        public int UniqueIndex, NextUniqueIndex;

        public Vector3 GetPosition()
        {
            return new Vector3(roundValue(PositionArray[0]), roundValue(PositionArray[1]), roundValue(PositionArray[2]));
        }

        public void SetPosition(Vector3 vector)
        {
            PositionArray[0] = vector.x;
            PositionArray[1] = vector.y;
            PositionArray[2] = vector.z;
        }

        public Vector3 GetEulerAngles()
        {
            return new Vector3(roundValue(EulerAnglesArray[0]), roundValue(EulerAnglesArray[1]), roundValue(EulerAnglesArray[2]));
        }

        public void SetEulerAngles(Vector3 vector)
        {
            EulerAnglesArray[0] = vector.x;
            EulerAnglesArray[1] = vector.y;
            EulerAnglesArray[2] = vector.z;
        }

        public Vector3 GetScale()
        {
            return new Vector3(roundValue(ScaleArray[0]), roundValue(ScaleArray[1]), roundValue(ScaleArray[2]));
        }

        public void SetScale(Vector3 vector)
        {
            ScaleArray[0] = vector.x;
            ScaleArray[1] = vector.y;
            ScaleArray[2] = vector.z;
        }

        private float roundValue(float value)
        {
            return Mathf.Round(value * 1000f) / 1000f;
        }

        public PersonalizationEditorObjectInfo()
        {
            if (PositionArray == null)
                PositionArray = new float[3];

            if (EulerAnglesArray == null)
                EulerAnglesArray = new float[3];

            if (ScaleArray == null)
                ScaleArray = new float[3];
        }

        public PersonalizationEditorObjectBehaviour Deserialize(Transform parent, PersonalizationControllerInfo personalizationControllerInfo)
        {
            PersonalizationEditorObjectManager personalizationEditorObjectManager = PersonalizationEditorObjectManager.Instance;
            if (personalizationEditorObjectManager.GetObjectInfo(Path) == null)
                return null;

            if (!parent)
            {
                GameObject gameObject = new GameObject(Name);
                parent = gameObject.transform;
            }

            PersonalizationEditorObjectBehaviour behaviour = personalizationEditorObjectManager.PlaceObject(Path, parent, false);
            behaviour.IsRoot = IsRoot;
            behaviour.UniqueIndex = UniqueIndex;
            behaviour.ControllerInfo = personalizationControllerInfo;
            behaviour.Name = Name;
            behaviour.PropertyValues = PropertyValues;
            behaviour.SerializedScale = GetScale();
            if (PersonalizationEditorManager.IsInEditor())
                PersonalizationEditorObjectManager.Instance.AddInstantiatedObject(behaviour);
            Transform transform = behaviour.transform;
            transform.localPosition = GetPosition();
            transform.localEulerAngles = GetEulerAngles();
            transform.localScale = behaviour.SerializedScale;
            if (!Children.IsNullOrEmpty())
            {
                foreach (PersonalizationEditorObjectInfo info in Children)
                    _ = info.Deserialize(behaviour.transform, personalizationControllerInfo);
            }
            return behaviour;
        }
    }
}
