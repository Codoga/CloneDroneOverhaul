﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace CDOverhaul.Gameplay.Editors.Personalization
{
    public class PersonalizationEditorFloatFieldDisplay : PersonalizationEditorFieldDisplay
    {
        [ActionReference(nameof(onChangedValue))]
        [ObjectReference("InputField")]
        private InputField m_InputField;

        public override void Initialize(FieldInfo fieldToEdit, object targetObject)
        {
            base.Initialize(fieldToEdit, targetObject);
            m_InputField.text = ((float)FieldValue).ToString().Replace(',', '.');
        }

        private void onChangedValue(string newValue)
        {
            newValue = newValue.Replace('.', ',').Replace(" ", string.Empty);
            if (!float.TryParse(newValue, out float result))
            {
                string initialText = ((float)InitialFieldValue).ToString().Replace(',', '.');
                FieldValue = initialText;
                m_InputField.text = initialText;
                return;
            }

            FieldValue = result;
            OnFieldValueChanged();
        }
    }
}
