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
    public class LevelInfoLoader : MonoBehaviour
    {
        public static LevelInfoLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("LevelInfoLoader").AddComponent<LevelInfoLoader>();
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }
        private static LevelInfoLoader _instance;

        public static string customLevelsPath { get; private set; } = Path.Combine(Application.dataPath, "..", "CustomLevels");

        public bool isPreloadingLevels;
        public float preloadingProgress;

        public List<LevelInfo> loadedLevels = new List<LevelInfo>();

        private SemVer.Range compatibleFileVersions = new SemVer.Range("^1.0.0");

        public async void PreLoadLevels()
        {
            if (!isPreloadingLevels)
            {
                isPreloadingLevels = true;
                preloadingProgress = 0f;

                loadedLevels.Clear();

                Directory.CreateDirectory(customLevelsPath);

                var allLevels = GetAllLevels(customLevelsPath);

                Console.WriteLine($"Found {allLevels.Count} levels!");

                int counter = 0;
                int totalLevels = allLevels.Count;

                foreach (var path in allLevels)
                {
                    var result = await LoadLevelInfoAsync(path);
                    if (result.success)
                    {
                        await HashSystem.GetHashForLevel(result.info);
                        loadedLevels.Add(result.info);
                    }

                    counter++;
                    preloadingProgress = ((float)counter) / totalLevels;
                }

                isPreloadingLevels = false;
            }
        }

        public List<string> GetAllLevels(string path)
        {
            List<string> levels = new List<string>();

            DirectoryInfo levelsDirInfo = new DirectoryInfo(path);
            Console.WriteLine($"Searching for levels in \"{levelsDirInfo.FullName}\"...");

            foreach (var entry in levelsDirInfo.EnumerateFileSystemInfos())
            {
                if (entry.Attributes.HasFlag(FileAttributes.Directory))
                {
                    levels.AddRange(GetAllLevels(entry.FullName));
                }
                else if (entry.Extension == ".lvl")
                {
                    levels.Add(entry.FullName);
                }
            }

            return levels;
        }

        public Task<LevelLoadResult> LoadLevelInfoAsync(string levelPath)
        {
            return Task.Run(() => LoadLevelInfo(levelPath, false, true));
        }

        public LevelLoadResult LoadLevelInfo(string levelPath, bool leaveOpen, bool isAsync = false)
        {
            LevelLoadResult result = new LevelLoadResult();

            FileStream stream = File.OpenRead(levelPath);

            result.fileStream = stream;

            try
            {
                bool headerOK = stream.ReadByte() == (byte)'K'
                            && stream.ReadByte() == (byte)'R'
                            && stream.ReadByte() == (byte)'L'
                            && stream.ReadByte() == (byte)'S'
                            && stream.ReadByte() == (byte)'N';

                SemVer.Version fileVersion = new SemVer.Version(stream.ReadByte(), stream.ReadByte(), stream.ReadByte());

                if (headerOK && compatibleFileVersions.IsSatisfied(fileVersion))
                {
                    byte[] metadataSizeBuffer = new byte[4];
                    stream.Read(metadataSizeBuffer, 0, 4);

                    int metadataSize = BitConverter.ToInt32(metadataSizeBuffer, 0);

                    byte[] coverSizeBuffer = new byte[4];
                    stream.Read(coverSizeBuffer, 0, 4);

                    int coverSize = BitConverter.ToInt32(coverSizeBuffer, 0);

                    byte[] metadataBuffer = new byte[metadataSize];
                    stream.Read(metadataBuffer, 0, metadataSize);

                    LevelMetadata levelMetadata = JsonConvert.DeserializeObject<LevelMetadata>(Encoding.UTF8.GetString(metadataBuffer));

                    byte[] coverBuffer = new byte[coverSize];
                    stream.Read(coverBuffer, 0, coverSize);


                    long assetsArchiveOffset = stream.Position;

                    LevelInfo info = new LevelInfo() { path = levelPath, metadata = levelMetadata, cover = null, assetsArchiveOffset = assetsArchiveOffset };

                    if (isAsync)
                    {
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            if (coverBuffer != null && coverBuffer.Length > 0)
                            {
                                Texture2D levelCover = new Texture2D(1, 1);
                                levelCover.LoadImage(coverBuffer);
                                info.cover = levelCover;
                            }
                        });
                    }
                    else
                    {
                        if (coverBuffer != null && coverBuffer.Length > 0)
                        {
                            Texture2D levelCover = new Texture2D(1, 1);
                            levelCover.LoadImage(coverBuffer);
                            info.cover = levelCover;
                        }
                    }

                    result.info = info;
                    result.success = true;

                    if (!leaveOpen)
                    {
                        stream.Close();
                        result.fileStream = null;
                    }

                    return result;
                }
                else
                {
                    if (headerOK)
                    {
                        Console.WriteLine($"File at \"{levelPath}\" is not a Karlson level!");
                    }
                    else
                    {
                        Console.WriteLine($"Level at \"{levelPath}\" is not compatible with this MapLoader version! Level file version: {fileVersion}, Compatible file versions: {compatibleFileVersions}");
                    }

                    stream.Close();
                    result.fileStream = null;
                    result.success = false;

                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to load level info for level at \"{levelPath}\"! Exception: " + e);

                stream.Close();
                result.fileStream = null;
                result.success = false;

                return result;
            }
        }
    }
}
