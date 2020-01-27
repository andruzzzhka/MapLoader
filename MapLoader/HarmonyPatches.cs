using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MapLoader
{
    [HarmonyPatch(typeof(Game), "Win")]
    class GameSaveFix
    {
        public static bool Prefix(ref bool ___playing, ref bool ___done)
        {

            ___playing = false;
            Timer.Instance.Stop();
            Time.timeScale = 0.05f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UIManger.Instance.WinUI(true);
            float timer = Timer.Instance.GetTimer();

            if (SceneManager.GetActiveScene().buildIndex != -1)
            {
                int num = int.Parse(SceneManager.GetActiveScene().name[0].ToString() ?? "");
                int num2;
                if (int.TryParse(SceneManager.GetActiveScene().name.Substring(0, 2) ?? "", out num2))
                {
                    num = num2;
                }
                float num3 = SaveManager.Instance.state.times[num];
                if (timer < num3 || num3 == 0f)
                {
                    SaveManager.Instance.state.times[num] = timer;
                    SaveManager.Instance.Save();
                }
                MonoBehaviour.print("time has been saved as: " + Timer.Instance.GetFormattedTime(timer));
            }
            else
            {
                var save = SaveSystem.GetDataForLevel(LevelAssetsLoader.Instance.currentLevel);

                var sceneName = LevelAssetsLoader.Instance.currentStage+"_"+SceneManager.GetActiveScene().name;

                if (save.timeForStages.ContainsKey(sceneName))
                {
                    if(timer < save.timeForStages[sceneName])
                        save.timeForStages[sceneName] = timer;
                }
                else
                    save.timeForStages.Add(sceneName, timer);

                SaveSystem.SetDataForLevel(LevelAssetsLoader.Instance.currentLevel, save);
            }

            ___done = true;

            Plugin.NewTime(SceneManager.GetActiveScene().name, timer);

            return false;
        }
    }

    [HarmonyPatch(typeof(Game), "NextMap")]
    class NextMapFix
    {
        public static bool Prefix(Game __instance)
        {
            Time.timeScale = 1f;
            int buildIndex = SceneManager.GetActiveScene().buildIndex;

            if (buildIndex == -1)
            {
                if (LevelAssetsLoader.Instance.currentLevel.metadata.isLevelPack)
                {
                    if (LevelAssetsLoader.Instance.currentStage == -1)
                    {
                        Console.WriteLine("Something went wrong... Unable to find current stage index!");
                        __instance.MainMenu();
                        return false;
                    }
                    else if (LevelAssetsLoader.Instance.currentLevel.metadata.stages.Count - 1 > LevelAssetsLoader.Instance.currentStage)
                    {
                        LevelAssetsLoader.Instance.currentStage++;
                        LevelAssetsLoader.Instance.LoadScene(LevelAssetsLoader.Instance.currentLevel.metadata.stages[LevelAssetsLoader.Instance.currentStage]);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("No more stages!");
                        __instance.MainMenu();
                        return false;
                    }
                }
                else
                {
                    __instance.MainMenu();
                    return false;
                }
            }
            else if (buildIndex + 1 >= SceneManager.sceneCountInBuildSettings)
            {
                __instance.MainMenu();
                return false;
            }

            SceneManager.LoadScene(buildIndex + 1);
            __instance.StartGame();

            return false;
        }

    }

    [HarmonyPatch(typeof(Game), "RestartGame")]
    class RestartFix
    {
        public static bool Prefix(Game __instance)
        {
            LevelAssetsLoader.Instance.LoadScene(SceneManager.GetActiveScene().name, () =>
            {
                Time.timeScale = 1f;
            });

            return false;
        }

    }
}
