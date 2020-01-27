using Harmony;
using IllusionPlugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MapLoader
{
    public class Plugin : IPlugin
    {
        public string Name => "MapLoader";
        public string Version => "0.1.0";

        public static event Action<string, float> newTimeAvailable;
        public static event Action<string> levelLoaded;

        public void OnApplicationQuit()
        {
        }

        public void OnApplicationStart()
        {
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            var harmony = HarmonyInstance.Create("com.andruzzzhka.MapLoader");
            harmony.PatchAll();

            HashSystem.Load();
            SaveSystem.Load();
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name == "Initialize")
            {
                MainThreadDispatcher.Instance.Update();
                LevelInfoLoader.Instance.PreLoadLevels();
            }
            else if (arg1.name == "MainMenu")
            {
                PluginUI.Instance.OnLoad();
            }
        }

        public void OnFixedUpdate()
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                LevelAssetsLoader.Instance.LoadLevel(LevelInfoLoader.Instance.loadedLevels.First());
            }
        }

        internal static void NewTime(string sceneName, float time)
        {
            newTimeAvailable?.Invoke(sceneName, time);
        }

        internal static void LevelLoaded(string sceneName)
        {
            levelLoaded?.Invoke(sceneName);
        }
    }
}
