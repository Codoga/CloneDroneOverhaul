﻿using OverhaulAPI.SharedMonoBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace CDOverhaul.Gameplay.Editors.Personalization
{
    public class PersonalizationEditorPropertiesWindow : PersonalizationEditorUIElement
    {
        [UIElementDefaultVisibilityStateAttribute(false)]
        [UIElementComponents(new System.Type[] { typeof(PersonalizationEditorPropertyCategoryDisplay) })]
        [UIElementReferenceAttribute("CategoryPrefab")]
        public readonly PersonalizationEditorPropertyCategoryDisplay PropertyCategoryPrefab;

        [UIElementReferenceAttribute("Content")]
        private readonly Transform m_Container;

        private bool m_HasInitialized;

        protected override bool AssignVariablesAutomatically() => false;

        public void Populate(EPersonalizationCategory category)
        {
            if (!m_HasInitialized)
            {
                UIController.AssignValues(this);
                _ = base.gameObject.AddComponent<OverhaulDraggablePanel>();
                m_HasInitialized = true;
            }

            if (PersonalizationEditor.EditingItem == null)
                return;

            TransformUtils.DestroyAllChildren(m_Container);
            List<PersonalizationEditorPropertyAttribute> properties;
            switch (category)
            {
                case EPersonalizationCategory.WeaponSkins:
                    properties = PersonalizationEditor.WeaponSkinItemFields;
                    break;
                case EPersonalizationCategory.Outfits:
                    properties = PersonalizationEditor.AccessoryItemFields;
                    break;
                case EPersonalizationCategory.Pets:
                    properties = PersonalizationEditor.PetItemFields;
                    break;
                default:
                    throw new System.NotImplementedException(category + " is not supported!");
            }

            if (properties.IsNullOrEmpty())
                throw new System.NullReferenceException("The attributes list of " + category + " is null/empty");

            List<string> categories = new List<string>();
            foreach (PersonalizationEditorPropertyAttribute attribute in properties)
            {
                if (!string.IsNullOrEmpty(attribute.Category) && !categories.Contains(attribute.Category))
                    categories.Add(attribute.Category);
            }

            foreach (string categoryToInstantiate in categories)
            {
                List<PersonalizationEditorPropertyAttribute> categoryFields = new List<PersonalizationEditorPropertyAttribute>();
                foreach (PersonalizationEditorPropertyAttribute attribute in properties)
                {
                    if (attribute.Category == categoryToInstantiate)
                        categoryFields.Add(attribute);
                }

                PersonalizationEditorPropertyCategoryDisplay categoryPrefab = Instantiate(PropertyCategoryPrefab, m_Container);
                categoryPrefab.IsInstantiated = true;
                categoryPrefab.Populate(categoryToInstantiate, categoryFields);
                categoryPrefab.gameObject.SetActive(true);
            }
        }
    }
}
