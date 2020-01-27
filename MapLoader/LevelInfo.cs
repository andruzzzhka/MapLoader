using System;
using System.IO;
using UnityEngine;

namespace MapLoader
{
    public struct LevelLoadResult
    {
        public bool success;
        public LevelInfo info;
        public Stream fileStream;
    }

    public class LevelInfo
    {
        public string path;

        public LevelMetadata metadata;
        public Texture2D cover;
        public long assetsArchiveOffset;

        public string hash;

        public string GetRelativePath()
        {
            Uri pathUri = new Uri(path);

            var levelsPath = LevelInfoLoader.customLevelsPath;

            if (!levelsPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                levelsPath += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(levelsPath);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
