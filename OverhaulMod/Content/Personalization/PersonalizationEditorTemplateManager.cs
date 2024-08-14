using OverhaulMod.Utils;
using System.IO;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationEditorTemplateManager : Singleton<PersonalizationEditorTemplateManager>
    {
        private static string s_templatesFolder;
        public static string templatesFolder
        {
            get
            {
                if (s_templatesFolder == null)
                {
                    s_templatesFolder = Path.Combine(ModCore.dataFolder, "customizationTemplates");
                }
                return s_templatesFolder;
            }
        }

        private bool m_hasLoadedTemplates;

        private bool m_noTemplates;

        private PersonalizationItemInfo[] m_templates;

        public void LoadTemplates()
        {
            m_hasLoadedTemplates = true;

            string path = templatesFolder;
            if (!Directory.Exists(path))
            {
                m_noTemplates = true;
                return;
            }

            string[] files = Directory.GetFiles(path);
            if (files.IsNullOrEmpty())
            {
                m_noTemplates = true;
                return;
            }

            PersonalizationItemInfo[] array = new PersonalizationItemInfo[files.Length];

            int i = 0;
            foreach (string file in files)
            {
                PersonalizationItemInfo itemInfo;
                try
                {
                    itemInfo = ModJsonUtils.DeserializeStream<PersonalizationItemInfo>(file);
                }
                catch
                {
                    itemInfo = new PersonalizationItemInfo()
                    {
                        Corrupted = true
                    };
                }

                array[i] = itemInfo;

                i++;
            }

            m_templates = array;
        }

        public PersonalizationItemInfo[] GetTemplates()
        {
            if (!m_hasLoadedTemplates || m_noTemplates)
                return null;

            return m_templates;
        }
    }
}
