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

        public override string ToString()
        {
            return $"{name}@{version}";
        }
    }

    public struct ModsCheckResult
    {
        public bool satisfied;

        public List<ModEntry> missingMods;
        public List<ModEntry> oudatedMods;
        public List<ModEntry> satisfiedMods;
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

        public ModsCheckResult CheckRequiredMods()
        {
            if (requiredMods == null || requiredMods.Count == 0)
            {
                return new ModsCheckResult() { satisfied = true };
            }

            ModsCheckResult result = new ModsCheckResult();
            result.missingMods = new List<ModEntry>();
            result.oudatedMods = new List<ModEntry>();
            result.satisfiedMods = new List<ModEntry>();
            result.satisfied = true;

            foreach (var reqMod in requiredMods)
            {
                var mod = IllusionInjector.PluginManager.Plugins.FirstOrDefault(x => x.Name == reqMod.name);
                if (mod != null)
                {
                    SemVer.Version version = new SemVer.Version(mod.Version);

                    SemVer.Range range = new SemVer.Range(reqMod.version);

                    if (range.IsSatisfied(version))
                    {
                        result.satisfiedMods.Add(reqMod);
                    }
                    else
                    {
                        result.oudatedMods.Add(reqMod);
                        result.satisfied = false;
                    }
                }
                else
                {
                    result.missingMods.Add(reqMod);
                    result.satisfied = false;
                }
            }

            return result;
        }
    }
}
