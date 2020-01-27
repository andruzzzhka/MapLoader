using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MapLoader
{
    public class ExporterWindow : EditorWindow
    {
        SerializedObject serializedObject;
        bool isBuildingLevel;

        LevelMetadata levelMetadata = new LevelMetadata();

        [SerializeField]
        Texture2D levelCover;
        SerializedProperty levelCoverProperty;

        [SerializeField]
        List<ModEntry> requiredModsList;
        SerializedProperty requiredModsProperty;

        [SerializeField]
        SceneAsset mainStage;
        SerializedProperty mainSceneProperty;

        [SerializeField]
        List<SceneAsset> packStages;
        SerializedProperty packStagesProperty;

        ReorderableList requiredModsReorderableList;
        ReorderableList packStagesReorderableList;

        string status = "Ready!";

        [MenuItem("MapExporter/Show exporter")]
        public static void ShowWindow()
        {
            GetWindow<ExporterWindow>("Exporter", true);
        }

        public void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            requiredModsList = new List<ModEntry>(); 
            packStages = new List<SceneAsset>();

            levelCoverProperty = serializedObject.FindProperty("levelCover");
            requiredModsProperty = serializedObject.FindProperty("requiredModsList");
            packStagesProperty = serializedObject.FindProperty("packStages");
            mainSceneProperty = serializedObject.FindProperty("mainStage");

            requiredModsReorderableList = new ReorderableList(serializedObject, requiredModsProperty, true, true, true, true);

            requiredModsReorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Required mods");
            requiredModsReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                ModEntry mod = requiredModsList[index];

                mod = DrawModEntry(rect, mod);

                requiredModsList[index] = mod;
            };

            packStagesReorderableList = new ReorderableList(serializedObject, packStagesProperty, true, true, true, true);

            packStagesReorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Stages");
            packStagesReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, packStagesReorderableList.serializedProperty.GetArrayElementAtIndex(index));
            };
        }

        public void OnGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Karlson custom level exporter", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            levelMetadata.levelId = EditorGUILayout.TextField("Level ID", levelMetadata.levelId);

            EditorGUILayout.Space();

            levelMetadata.levelName = EditorGUILayout.TextField("Level name", levelMetadata.levelName);
            levelMetadata.levelVersion = EditorGUILayout.TextField("Level version", levelMetadata.levelVersion);
            levelMetadata.authorName = EditorGUILayout.TextField("Author name", levelMetadata.authorName);
            EditorGUILayout.PropertyField(levelCoverProperty);

            EditorGUILayout.Space();

            levelMetadata.isLevelPack = EditorGUILayout.Toggle("Is level pack", levelMetadata.isLevelPack);

            EditorGUILayout.Space();

            if(!levelMetadata.isLevelPack)
            {
                EditorGUILayout.PropertyField(mainSceneProperty);
            }
            else
            {
                packStagesReorderableList.DoLayoutList();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Level description");
            levelMetadata.levelDescription = EditorGUILayout.TextArea(levelMetadata.levelDescription);

            EditorGUILayout.Space();

            requiredModsReorderableList.DoLayoutList();

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            bool canBuildLevel = ((!levelMetadata.isLevelPack && mainStage != null) || (levelMetadata.isLevelPack && packStages.Count(x => x != default) > 0)) && !string.IsNullOrEmpty(levelMetadata.levelId);

            if (!canBuildLevel)
            {
                if (string.IsNullOrEmpty(levelMetadata.levelId))
                    status = "Level ID is empty!";
                else if (!((!levelMetadata.isLevelPack && mainStage != null) || (levelMetadata.isLevelPack && packStages.Count(x => x != default) > 0)))
                    status = "No stage(s)!";
            }
            else
            {
                if(!isBuildingLevel)
                    status = "Ready!";
            }

            GUI.enabled = canBuildLevel;

            if (GUILayout.Button("Build level", GUILayout.Height(30f)))
            {
                levelMetadata.requiredMods = requiredModsList;

                BuildLevel(levelMetadata, levelCover, levelMetadata.isLevelPack ? packStages : new List<SceneAsset>() { mainStage });
            }

            GUI.enabled = true;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save preset"))
            {
                levelMetadata.stages = levelMetadata.isLevelPack ? packStages.Select(x => AssetDatabase.GetAssetPath(x)).ToList() : new List<string>() { AssetDatabase.GetAssetPath(mainStage) };
                levelMetadata.requiredMods = requiredModsList;

                string serializedMetadata = JsonUtility.ToJson(levelMetadata);

                string savePath = EditorUtility.SaveFilePanel("Save level preset", "", "", "json");

                if(savePath.Length > 0)
                {
                    File.WriteAllText(savePath, serializedMetadata);
                }
            }

            if (GUILayout.Button("Load preset"))
            {
                string loadPath = EditorUtility.OpenFilePanel("Load level preset", "", "json");

                if (loadPath.Length > 0)
                {
                    try
                    {
                        string serializedMetadata = File.ReadAllText(loadPath);

                        levelMetadata = JsonUtility.FromJson<LevelMetadata>(serializedMetadata);

                        if (levelMetadata.stages != null && levelMetadata.stages.Count > 0)
                        {
                            packStages = levelMetadata.stages.Select(x => AssetDatabase.LoadAssetAtPath<SceneAsset>(x)).ToList();
                            mainStage = packStages[0];
                        }
                        else
                        {
                            packStages = new List<SceneAsset>();
                        }

                        if (levelMetadata.requiredMods != null && levelMetadata.requiredMods.Count > 0)
                        {
                            requiredModsList = levelMetadata.requiredMods;
                        }
                        else
                        {
                            requiredModsList = new List<ModEntry>();
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                    }
                }                
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Status: "+status);
        }

        public ModEntry DrawModEntry(Rect rect, ModEntry entry)
        {
            Rect leftRect = rect;
            leftRect.width *= 3f/4f;

            Rect rightRect = rect;
            rightRect.width *= 1f / 4f;
            rightRect.x += leftRect.width;

            entry.name = EditorGUI.TextField(leftRect, entry.name);
            entry.version = EditorGUI.TextField(rightRect, entry.version);

            return entry;
        }

        private byte[] fileFormatVersion = new byte[3] { 1, 0, 0};

        private void BuildLevel(LevelMetadata levelMetadata, Texture2D levelCover, List<SceneAsset> sceneAsset)
        {
            isBuildingLevel = true;

            string assetBundleDirectory = "Assets/Output";

            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }
            else if(Directory.GetFileSystemEntries(assetBundleDirectory).Length != 0)
            {
                DirectoryInfo deleteFilesInfo = new DirectoryInfo(assetBundleDirectory);

                foreach (FileInfo file in deleteFilesInfo.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in deleteFilesInfo.EnumerateDirectories())
                {
                    dir.Delete(true);
                }

            }

            status = "Collecting assets...";

            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = "Stages_" + levelMetadata.levelId;
            buildMap[0].assetNames = sceneAsset.Distinct().Select(x => AssetDatabase.GetAssetPath(x)).ToArray();

            //Debug.Log($"Scene assets: {sceneAsset.Count}");
            //Debug.Log($"Path: {AssetDatabase.GetAssetPath(sceneAsset[0])}");

            foreach (var bundle in buildMap)
            {
                Debug.Log($"Building AssetBundle with name \"{bundle.assetBundleName}\", assets count: {bundle.assetNames.Length}");
            }

            status = "Building assets...";

            BuildPipeline.BuildAssetBundles(assetBundleDirectory, 
                                            buildMap,
                                            BuildAssetBundleOptions.None,
                                            BuildTarget.StandaloneWindows);

            Debug.Log($"Finished building AssetBundles! Packing level...");
            status = "Packing level...";

            File.Move(Path.Combine(assetBundleDirectory, "Output"), Path.Combine(assetBundleDirectory, levelMetadata.levelId));

            var fileStream = File.Open(Path.Combine(assetBundleDirectory, levelMetadata.levelId + ".lvl"), FileMode.Create);

            levelMetadata.stages = sceneAsset.Select(x => x.name).ToList();

            byte[] levelMetadataBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(levelMetadata));
            byte[] metadataSizeBuffer = BitConverter.GetBytes(levelMetadataBytes.Length);

            if (levelCover == null)
                levelCover = new Texture2D(1,1);

            if (!levelCover.isReadable)
                SetTextureImporterFormat(levelCover, true);
            var decompressed = DeCompress(levelCover);

            byte[] coverBytes = decompressed.EncodeToPNG();
            byte[] coverSizeBuffer = BitConverter.GetBytes(coverBytes.Length);
            
            byte[] buffer = new byte[5 + 3 + 4 + 4 + levelMetadataBytes.Length + coverBytes.Length];

            buffer[0] = (byte)'K';
            buffer[1] = (byte)'R';
            buffer[2] = (byte)'L';
            buffer[3] = (byte)'S';
            buffer[4] = (byte)'N';

            Array.Copy(fileFormatVersion, 0, buffer, 5, 3);
            Array.Copy(metadataSizeBuffer, 0, buffer, 8, 4);
            Array.Copy(coverSizeBuffer, 0, buffer, 12, 4);

            Array.Copy(levelMetadataBytes, 0, buffer, 16, levelMetadataBytes.Length);
            Array.Copy(coverBytes,      0, buffer, 16 + levelMetadataBytes.Length, coverBytes.Length);

            fileStream.Write(buffer, 0, buffer.Length);

            ZipArchive assetsArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);

            assetsArchive.CreateEntryFromFile(Path.Combine(assetBundleDirectory, levelMetadata.levelId), levelMetadata.levelId);

            foreach(var bundle in buildMap)
            {
                assetsArchive.CreateEntryFromFile(Path.Combine(assetBundleDirectory, bundle.assetBundleName), bundle.assetBundleName);
            }

            assetsArchive.Dispose();

            DirectoryInfo dirInfo = new DirectoryInfo(assetBundleDirectory);

            foreach (FileInfo file in dirInfo.EnumerateFiles())
            {
                if(!file.Name.EndsWith(".lvl"))
                    file.Delete();
            }

            Debug.Log($"Done!");
            isBuildingLevel = false;

        }

        public static void SetTextureImporterFormat(Texture2D texture, bool isReadable)
        {
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.textureType = TextureImporterType.Default;

                tImporter.isReadable = isReadable;
                tImporter.textureCompression = TextureImporterCompression.Uncompressed;
                tImporter.mipmapEnabled = false;
                tImporter.npotScale = TextureImporterNPOTScale.None;

                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }

        public static Texture2D DeCompress(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }
}
