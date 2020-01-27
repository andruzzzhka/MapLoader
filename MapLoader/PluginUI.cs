using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MapLoader
{
    public class PluginUI : MonoBehaviour
    {
        public static PluginUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("MapLoaderUI").AddComponent<PluginUI>();
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }
        private static PluginUI _instance;


        private RectTransform _customLevelsPanel;
        private Button _customLevelsButton;
        private TextMeshProUGUI _loadingText;
        private TextMeshProUGUI _pageText;

        private Lobby _lobbyScript;

        private LevelButton[] _levelButtons;
        private int _currentPage;

        private Vector2 _lvlBtnStartPos = new Vector2(-401, 101);
        private Vector2 _lvlBtnOffset = new Vector2(168.3f, -114.36f);

        public void OnLoad()
        {
            CreateUI();
        }


        public void CreateUI()
        {
            _lobbyScript = GameObject.FindObjectOfType<Lobby>();
            var playUI = _lobbyScript.GetComponentInChildren<PlayUI>(true);

            _customLevelsButton = CreateTextButton(playUI.transform as RectTransform, "CUSTOM LEVELS", new Vector2(-10, 196), new Vector2(350, 50));
            _customLevelsButton.onClick.AddListener(() => { CustomLevelsPressed(); });

            #region Create custom levels UI

            _customLevelsPanel = new GameObject("CustomLevels").AddComponent<RectTransform>();
            _customLevelsPanel.gameObject.layer = 5;
            _customLevelsPanel.SetParent(_lobbyScript.transform, false);
            _customLevelsPanel.sizeDelta = new Vector2(100f, 100f);
            _customLevelsPanel.anchoredPosition3D = new Vector3(-202f, 107.8f, 72.5f);
            _customLevelsPanel.localRotation = Quaternion.Euler(0f, 270f, 0f);
            _customLevelsPanel.localScale = new Vector3(0.7636f, 0.7636f, 0.7636f);

            CreateTextButton(_customLevelsPanel, "BACK", new Vector2(-383, 196), new Vector2(160, 30), BackPressed);
            _loadingText = CreateText(_customLevelsPanel, "Loading levels...\n0.00%", new Vector2(-150f, 0f), new Vector2(10f, 10f));

            CreateTextButton(_customLevelsPanel, "<<", new Vector2(-75, 196), new Vector2(60, 30), () => UpdateLevelsList(_currentPage - 1));
            _pageText = CreateText(_customLevelsPanel, "1", new Vector2(0, 196), new Vector2(10f, 10f));
            _pageText.fontSize = 55f;
            CreateTextButton(_customLevelsPanel, ">>", new Vector2(75, 196), new Vector2(60, 30), () => UpdateLevelsList(_currentPage + 1));

            #endregion

            _customLevelsPanel.gameObject.SetActive(false);


            if (LevelInfoLoader.Instance.isPreloadingLevels)
            {
                StartCoroutine(WaitWhileLevelsLoading());
            }
            else
            {
                _loadingText.gameObject.SetActive(false);
                UpdateLevelsList();
            }
        }

        public IEnumerator WaitWhileLevelsLoading()
        {
            while (LevelInfoLoader.Instance.isPreloadingLevels)
            {
                if (_loadingText != null)
                    _loadingText.text = $"Loading levels...\n{LevelInfoLoader.Instance.preloadingProgress:P}";
                yield return null;
            }

            _loadingText.gameObject.SetActive(false);
            UpdateLevelsList();
        }

        public void UpdateLevelsList(int page = 0)
        {
            if (page >= 0 && page < Mathf.CeilToInt(LevelInfoLoader.Instance.loadedLevels.Count / 12f))
                _currentPage = page;

            _pageText.text = (_currentPage + 1).ToString();

            if (_levelButtons == null || _levelButtons.Any(x => x == null))
            {
                _levelButtons = new LevelButton[12];
                for (int y = 0; y < 3; y++)
                    for (int x = 0; x < 4; x++)
                    {
                        _levelButtons[y * 4 + x] = CreateLevelButton(_customLevelsPanel, _lvlBtnStartPos + _lvlBtnOffset * new Vector2(x, y));
                    }
            }

            for (int i = 0; i < 12; i++)
            {
                if (LevelInfoLoader.Instance.loadedLevels.Count > (i + 12 * _currentPage))
                {
                    _levelButtons[i].gameObject.SetActive(true);
                    _levelButtons[i].UpdateInfo(LevelInfoLoader.Instance.loadedLevels[i + 12 * _currentPage]);
                }
                else
                {
                    _levelButtons[i].gameObject.SetActive(false);
                }
            }
        }

        #region Button callbacks

        public void CustomLevelsPressed()
        {
            MenuCamera menuCamera = GameObject.FindObjectOfType<MenuCamera>();

            menuCamera.SetPrivateField("desiredPos", new Vector3(-1f, 4.6f, 5.5f) + menuCamera.GetPrivateField<Vector3>("startPos"));
            menuCamera.SetPrivateField("desiredRot", Quaternion.Euler(0f, 270f, 0f));

            ChangeUIState(true);
        }

        public void BackPressed()
        {
            MenuCamera menuCamera = GameObject.FindObjectOfType<MenuCamera>();

            menuCamera.SetPrivateField("desiredPos", new Vector3(1f, 4.6f, 5.5f) + menuCamera.GetPrivateField<Vector3>("startPos"));
            menuCamera.SetPrivateField("desiredRot", Quaternion.Euler(0f, 90f, 0f));

            ChangeUIState(false);
        }

        private void ChangeUIState(bool showCustomLevels)
        {
            _customLevelsPanel.gameObject.SetActive(showCustomLevels);
            _lobbyScript.GetComponentInChildren<PlayUI>(true).gameObject.SetActive(!showCustomLevels);
        }

        #endregion

        #region UI Helpers

        private static TextMeshProUGUI _backBtnText;

        public static LevelButton CreateLevelButton(RectTransform parent, Vector2 position)
        {
            var levelButton = Instantiate(GameObject.FindObjectOfType<Lobby>().GetComponentInChildren<PlayUI>(true).GetComponentsInChildren<Button>().First(x => x.name == "Escape0").gameObject, parent);
            (levelButton.transform as RectTransform).anchoredPosition = position;

            var result = levelButton.gameObject.AddComponent<LevelButton>();
            result.Init();

            return result;
        }

        public static Button CreateTextButton(RectTransform parent, string text, Vector2 position, Vector2 sizeDelta, Action onClick = null)
        {
            Button backBtn = GameObject.FindObjectOfType<Lobby>().GetComponentInChildren<PlayUI>(true).GetComponentsInChildren<Button>(true).First(x => x.name == "Back");

            Button result = GameObject.Instantiate(backBtn.gameObject, parent).GetComponent<Button>();
            (result.transform as RectTransform).anchoredPosition = position;
            (result.transform as RectTransform).sizeDelta = sizeDelta;
            //result.GetComponent<Image>().enabled = true;

            TextMeshProUGUI buttonText = result.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.overflowMode = TextOverflowModes.Overflow;
            buttonText.enableWordWrapping = false;
            buttonText.text = text;

            result.onClick = new Button.ButtonClickedEvent();
            result.onClick.AddListener(() => { GameObject.FindObjectOfType<Lobby>().ButtonSound(); });

            if (onClick != null)
                result.onClick.AddListener(() => { onClick.Invoke(); });

            return result;
        }

        public static TextMeshProUGUI CreateText(RectTransform parent, string text, Vector2 position, Vector2 sizeDelta)
        {
            if (_backBtnText == null)
            {
                _backBtnText = Instantiate(GameObject.FindObjectOfType<Lobby>().GetComponentInChildren<PlayUI>(true).GetComponentsInChildren<TextMeshProUGUI>(true).First(x => x.transform.parent.name == "Back"));
                DontDestroyOnLoad(_backBtnText.gameObject);
                _backBtnText.gameObject.SetActive(false);
            }

            TextMeshProUGUI result = GameObject.Instantiate(_backBtnText.gameObject, parent).GetComponent<TextMeshProUGUI>();
            (result.transform as RectTransform).anchoredPosition = position;
            (result.transform as RectTransform).sizeDelta = sizeDelta;
            result.overflowMode = TextOverflowModes.Overflow;
            result.enableWordWrapping = false;
            result.text = text;

            result.gameObject.SetActive(true);

            return result;
        }

        #endregion
    }
}
