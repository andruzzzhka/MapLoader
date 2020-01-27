using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MapLoader
{
    public class LevelAssetsLoader : MonoBehaviour
    {
        public static LevelAssetsLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("LevelAssetsLoader").AddComponent<LevelAssetsLoader>();
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }
        private static LevelAssetsLoader _instance;
        private static List<AssetBundle> _loadedAssetBundles = new List<AssetBundle>();

        public bool isLoadingLevel;
        public float loadingProgress;
        public LevelInfo currentLevel;
        public int currentStage;

        private Camera _loadingCamera;

        public void Awake()
        {
            SceneManager.activeSceneChanged += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene arg0, Scene arg1)
        {
        }

        public void LoadLevel(LevelInfo info)
        {
            if (!isLoadingLevel)
                StartCoroutine(LoadLevelCoroutine(info));
        }

        private IEnumerator LoadLevelCoroutine(LevelInfo info)
        {
            isLoadingLevel = true;
            loadingProgress = 0f;
            currentLevel = info;
            currentStage = 0;

            if (_loadedAssetBundles.Count > 0)
                UnloadAssets();

            ModsCheckResult check = info.metadata.CheckRequiredMods();
            if (!check.satisfied)
            {
                Console.WriteLine("Unable to load level! Mods requirements not satisfied:");
                if (check.missingMods.Count > 0)
                {
                    Console.WriteLine("Missing mods:");
                    foreach (var missingMod in check.missingMods)
                        Console.WriteLine(missingMod);
                }
                if (check.oudatedMods.Count > 0)
                {
                    Console.WriteLine("Outdated mods:");
                    foreach (var outdatedMod in check.oudatedMods)
                        Console.WriteLine(outdatedMod);
                }

                loadingProgress = 0f;
                isLoadingLevel = false;

                yield break;
            }

            if (_loadingCamera == null)
                CreateLoadingCamera(currentLevel.cover);

            var task = HashSystem.RecalculateHashForLevel(info);

            yield return new WaitUntil(() => task.IsCompleted);
            loadingProgress = 0.05f;

            FileStream stream = File.OpenRead(info.path);

            stream.Position = info.assetsArchiveOffset;

            ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var assetsManifestRequest = AssetBundle.LoadFromStreamAsync(archive.GetEntry(info.metadata.levelId).ExtractToStream());

            while(!assetsManifestRequest.isDone)
            {
                loadingProgress = 0.05f + assetsManifestRequest.progress*0.05f;

                yield return null;
            }
            loadingProgress = 0.1f;

            if (assetsManifestRequest.assetBundle == null)
            {
                Console.WriteLine("Failed to load AssetBundle!");
                loadingProgress = 0f;
                isLoadingLevel = false;
                if (_loadingCamera != null)
                    Destroy(_loadingCamera.gameObject);
                yield break;
            }
            _loadedAssetBundles.Add(assetsManifestRequest.assetBundle);

            var manifest = assetsManifestRequest.assetBundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");

            List<string> foundScenes = new List<string>();

            int totalBundles = manifest.GetAllAssetBundles().Length;
            int currentBundle = 0;

            foreach (var assetBundleName in manifest.GetAllAssetBundles())
            {
                var assetBundleRequest = AssetBundle.LoadFromStreamAsync(archive.Entries.First(x => x.Name.ToLower() == assetBundleName).ExtractToStream());

                while (!assetBundleRequest.isDone)
                {
                    loadingProgress = 0.1f + ((currentBundle + assetBundleRequest.progress) / totalBundles * 0.15f);

                    yield return null;
                }

                if (assetBundleRequest.assetBundle != null)
                {
                    _loadedAssetBundles.Add(assetBundleRequest.assetBundle);

                    var scenes = assetBundleRequest.assetBundle.GetAllScenePaths();

                    if (scenes.Length != 0)
                    {
                        foundScenes.AddRange(scenes);
                        Console.WriteLine($"Found {scenes.Length} scenes!");
                    }
                }
            }

            if (foundScenes.Count == 0)
            {
                Console.WriteLine("No scenes found!");
                loadingProgress = 0f;
                isLoadingLevel = false;
                if (_loadingCamera != null)
                    Destroy(_loadingCamera.gameObject);
                yield break;
            }

            Console.WriteLine("Loading scenes...");

            var sceneLoadingOperation =  SceneManager.LoadSceneAsync(info.metadata.stages[0], LoadSceneMode.Single);

            while (!sceneLoadingOperation.isDone)
            {
                loadingProgress = 0.25f + sceneLoadingOperation.progress * 0.5f;
                yield return null;
            }

            Console.WriteLine("Loaded custom scene!");

            SpawnPrefabs();
        }

        public void LoadScene(string sceneName, Action callback = null)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName, callback));
        }

        public IEnumerator LoadSceneCoroutine(string sceneName, Action callback = null)
        {
            CreateLoadingCamera(currentLevel.cover);

            var sceneLoadingOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            while (!sceneLoadingOperation.isDone)
            {
                loadingProgress = sceneLoadingOperation.progress * 0.75f;
                yield return null;
            }

            SpawnPrefabs();

            callback?.Invoke();

        }

        public void SpawnPrefabs()
        {
            StartCoroutine(SpawnPrefabsCoroutine());
        }

        public IEnumerator SpawnPrefabsCoroutine()
        {
            loadingProgress = 0.75f;
            isLoadingLevel = true;

            if (_loadingCamera == null)
                CreateLoadingCamera(currentLevel.cover);

            Console.WriteLine("Loading game scene!");

            var sceneLoadingOperation = SceneManager.LoadSceneAsync("2Sandbox1", LoadSceneMode.Additive);

            while (sceneLoadingOperation.progress < 0.9f) //For some reason, using isDone or values over 0.9 breaks ragdolls ¯\_(ツ)_/¯
            {
                loadingProgress = 0.75f + sceneLoadingOperation.progress * 0.20f;
                yield return null;
            }

            yield return sceneLoadingOperation;

            Console.WriteLine("Scene ready!");

            Prefabs.player = MoveToActiveScene<PlayerMovement>();
            Prefabs.camera = MoveToActiveScene<MoveCamera>();
            Prefabs.milk = MoveToActiveScene<Milk>();
            Prefabs.enemy = MoveToActiveScene<Enemy>();
            Prefabs.barrel = MoveToActiveScene<Barrel>();

            Prefabs.grapplingGun = MoveToActiveScene<Grappler>("Grappler");
            Prefabs.shotgun = MoveToActiveScene<RangedWeapon>("Shotgun");
            Prefabs.boomer = MoveToActiveScene<RangedWeapon>("Boomer");
            Prefabs.pistol = MoveToActiveScene<RangedWeapon>("Pistol");
            Prefabs.microUZI = MoveToActiveScene<RangedWeapon>("Uzi");

            Console.WriteLine("Grabbed prefabs!");
            
            sceneLoadingOperation = SceneManager.UnloadSceneAsync("2Sandbox1", UnloadSceneOptions.None); 
            
            while (!sceneLoadingOperation.isDone)
            {
                loadingProgress = 0.95f + sceneLoadingOperation.progress * 0.05f;
                yield return null;
            }

            Console.WriteLine("Unloaded scene!");
            loadingProgress = 1f;
            isLoadingLevel = false;

            yield return null;

            if (_loadingCamera != null)
                Destroy(_loadingCamera.gameObject);

            Game.Instance.StartGame();

            Plugin.LevelLoaded(SceneManager.GetActiveScene().name);
        }

        internal GameObject MoveToActiveScene<T>(string filter = "") where T : MonoBehaviour
        {
            GameObject temp = Resources.FindObjectsOfTypeAll<T>().First(x => string.IsNullOrEmpty(filter) ? true : x.name == filter).gameObject;
            if (temp.transform.parent != null)
                temp.transform.parent = null;
            SceneManager.MoveGameObjectToScene(temp, SceneManager.GetActiveScene());
            temp.SetActive(false);

            return temp;
        }

        internal void CreateLoadingCamera(Texture2D background = null)
        {
            if (_loadingCamera != null)
            {
                var img = _loadingCamera.GetComponentInChildren<RawImage>();

                if (background == null)
                {
                    img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                }
                else
                {
                    img.texture = background;
                    img.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                }

                return;
            }

            _loadingCamera = new GameObject("Loading Camera").AddComponent<Camera>();
            DontDestroyOnLoad(_loadingCamera);
            _loadingCamera.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            _loadingCamera.clearFlags = CameraClearFlags.Color;

            var canvas = new GameObject("BG").AddComponent<Canvas>();
            canvas.transform.SetParent(_loadingCamera.transform, false);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var image = new GameObject("BG").AddComponent<RawImage>();
            image.transform.SetParent(canvas.transform, false);

            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.sizeDelta = Vector2.zero;
            image.rectTransform.anchoredPosition = Vector2.zero;

            if (background == null)
            {
                image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            }
            else
            {
                image.texture = background;
                image.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            }

            var text = PluginUI.CreateText(image.transform as RectTransform, "Loading...", Vector2.zero, Vector2.zero);

            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.sizeDelta = Vector2.zero;
            text.rectTransform.anchoredPosition = Vector2.zero;
            text.fontSize = 72f;
            text.alignment = TextAlignmentOptions.Center;

            text.gameObject.AddComponent<UpdateProgressText>();
        }


        private void UnloadAssets()
        {
            foreach (var bundle in _loadedAssetBundles)
            {
                if (bundle != null)
                    try
                    {
                        bundle.Unload(false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unable to unload assets! Exception: " + e);
                    }
                else
                    Console.WriteLine("Unable to unload assets! AssetBundle is null");
            }
            _loadedAssetBundles.Clear();
        }


    }
}
