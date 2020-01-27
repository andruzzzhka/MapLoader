using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MapLoader
{
    public struct LevelSaveData
    {
        public Dictionary<string, float> timeForStages;
    }

    public static class SaveSystem
    {
        public static string saveFilePath { get; private set; } = Path.Combine(Application.persistentDataPath, "customLevelsSaveData.json");

        private static Dictionary<string, LevelSaveData> _saveData = new Dictionary<string, LevelSaveData>();

        private static bool _dirty;

        public static void Load()
        {
            if (File.Exists(saveFilePath))
            {
                _saveData = JsonConvert.DeserializeObject<Dictionary<string, LevelSaveData>>(File.ReadAllText(saveFilePath));
            }
        }

        public static void Save(bool force = false)
        {
            if(_dirty || force)
            {
                if (File.Exists(saveFilePath))
                {
                    if (File.Exists(saveFilePath + ".bak"))
                        File.Delete(saveFilePath + ".bak");

                    File.Move(saveFilePath, saveFilePath + ".bak");
                }

                File.WriteAllText(saveFilePath, JsonConvert.SerializeObject(_saveData, Formatting.Indented));

                _dirty = false;
            }
        }

        public static LevelSaveData GetDataForLevel(LevelInfo info)
        {
            if (string.IsNullOrEmpty(info.hash))
            {
                Console.WriteLine("Level hash is empty!");

                return new LevelSaveData() { timeForStages = new Dictionary<string, float>() };
            }

            if(_saveData.TryGetValue(info.hash, out var save))
            {
                return save;
            }

            return new LevelSaveData() { timeForStages = new Dictionary<string, float>() };
        }

        public static void SetDataForLevel(LevelInfo info, LevelSaveData data, bool save = true)
        {
            if (string.IsNullOrEmpty(info.hash))
            {
                Console.WriteLine("Level hash is empty!");

                return;
            }

            if (_saveData.ContainsKey(info.hash))
                _saveData[info.hash] = data;
            else
                _saveData.Add(info.hash, data);

            _dirty = true;

            if (save)
                Save();
        }
    }
}
