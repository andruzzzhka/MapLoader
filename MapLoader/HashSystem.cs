using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MapLoader
{
    public struct HashData
    {
        public DateTime lastWriteTime;
        public string hash;
    }

    public static class HashSystem
    {
        public static string hashesFilePath { get; private set; } = Path.Combine(Application.persistentDataPath, "customLevelsHashes.json");

        private static Dictionary<string, HashData> _hashes = new Dictionary<string, HashData>();

        public static void Save()
        {
            if (File.Exists(hashesFilePath))
            {
                if (File.Exists(hashesFilePath + ".bak"))
                    File.Delete(hashesFilePath + ".bak");

                File.Move(hashesFilePath, hashesFilePath + ".bak");
            }

            File.WriteAllText(hashesFilePath, JsonConvert.SerializeObject(_hashes, Formatting.Indented));
        }

        public static void Load()
        {
            if (File.Exists(hashesFilePath))
            {
                _hashes = JsonConvert.DeserializeObject<Dictionary<string, HashData>>(File.ReadAllText(hashesFilePath));
            }
        }

        public static async Task GetHashForAllLevels()
        {
            foreach(var level in LevelInfoLoader.Instance.loadedLevels)
            {
                await GetHashForLevel(level, false);
            }

            Save();
        }

        public static async Task<string> GetHashForLevel(LevelInfo info, bool save = true)
        {
            if(_hashes.TryGetValue(info.GetRelativePath(), out var data))
            {
                FileInfo file = new FileInfo(info.path);

                if(file.LastWriteTimeUtc != data.lastWriteTime)
                {
                    var newHash = await CalculateHash(info.path);

                    data.lastWriteTime = file.LastWriteTimeUtc;
                    data.hash = newHash;
                    _hashes[info.GetRelativePath()] = data;
                    info.hash = newHash;

                    if (save)
                        Save();

                    return newHash;
                }
                else
                {
                    info.hash = data.hash;
                    return data.hash;
                }
            }
            else
            {
                FileInfo file = new FileInfo(info.path);

                var newHash = await CalculateHash(info.path);

                data.lastWriteTime = file.LastWriteTimeUtc;
                data.hash = newHash;
                _hashes[info.GetRelativePath()] = data;
                info.hash = newHash;

                if (save)
                    Save();

                return newHash;
            }
        }

        public static async Task<string> RecalculateHashForLevel(LevelInfo info)
        {
            FileInfo file = new FileInfo(info.path);
            var newHash = await CalculateHash(info.path);
            HashData data = new HashData() { lastWriteTime = file.LastWriteTimeUtc, hash = newHash };
            info.hash = newHash;

            if (_hashes.ContainsKey(info.GetRelativePath()))
                _hashes[info.GetRelativePath()] = data;
            else
                _hashes.Add(info.GetRelativePath(), data);

            Save();

            return newHash;
        }

        private static Task<string> CalculateHash(string path)
        {
            return Task.Run(() => {
                return BitConverter.ToString(new SHA1Managed().ComputeHash(File.ReadAllBytes(path))).Replace("-", "");
            });
        }
    }
}
