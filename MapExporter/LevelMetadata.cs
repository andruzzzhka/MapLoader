using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MapLoader
{
    [Serializable]
    public struct ModEntry
    {
        public string name;
        public string version;
    }

    [Serializable]
    public struct LevelMetadata
    {
        public string levelId;

        public string levelName;
        public string levelVersion;
        public string authorName;

        public bool isLevelPack;

        public string levelDescription;
        public List<string> stages;

        public List<ModEntry> requiredMods;

    }
}
