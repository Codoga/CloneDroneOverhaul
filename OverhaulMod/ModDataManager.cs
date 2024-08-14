using OverhaulMod.Utils;
using System.IO;

namespace OverhaulMod
{
    public class ModDataManager : Singleton<ModDataManager>
    {
        /// <summary>
        /// A folder where all game data is stored
        /// </summary>
        public static string userDataFolder
        {
            get
            {
                return ModCore.modDataFolder;
            }
        }

        /// <summary>
        /// "saves" folder located under mod folder
        /// </summary>
        public static string savesFolder
        {
            get
            {
                return ModCore.savesFolder;
            }
        }

        public void WriteFile(string name, string content, bool useSavesFolder)
        {
            ModIOUtils.WriteText(content, Path.Combine(useSavesFolder ? savesFolder : userDataFolder, name));
        }

        public void SerializeToFile(string name, object obj, bool useSavesFolder)
        {
            ModJsonUtils.WriteStream(Path.Combine(useSavesFolder ? savesFolder : userDataFolder, name), obj);
        }

        public string ReadFile(string name, bool useSavesFolder)
        {
            return ModIOUtils.ReadText(Path.Combine(useSavesFolder ? savesFolder : userDataFolder, name));
        }

        public T DeserializeFile<T>(string name, bool useSavesFolder)
        {
            return ModJsonUtils.DeserializeStream<T>(Path.Combine(useSavesFolder ? savesFolder : userDataFolder, name));
        }

        public bool FileExists(string name, bool useSavesFolder)
        {
            return File.Exists(Path.Combine(useSavesFolder ? savesFolder : userDataFolder, name));
        }
    }
}
