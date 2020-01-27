using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MapLoader
{
    public class LevelButton : MonoBehaviour
    {
        public LevelInfo info;

        private Button _button;
        private Image _levelCover;
        private TextMeshProUGUI _levelNameText;
        private TextMeshProUGUI _levelTimeText;

        public void Init()
        {
            _button = GetComponent<Button>();
            _button.onClick = new Button.ButtonClickedEvent();

            _levelCover = GetComponent<Image>();

            _levelNameText = GetComponentsInChildren<TextMeshProUGUI>().First(x => !x.name.Contains("(1)"));
            _levelTimeText = GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name.Contains("(1)"));
        }

        public void UpdateInfo(LevelInfo info)
        {
            this.info = info;

            _button.onClick = new Button.ButtonClickedEvent();
            _button.onClick.AddListener(() => { GameObject.FindObjectOfType<Lobby>().ButtonSound(); });
            _button.onClick.AddListener(() => { LevelAssetsLoader.Instance.LoadLevel(info); });

            _levelNameText.text = info.metadata.levelName;

            var save = SaveSystem.GetDataForLevel(info);
            var time = save.timeForStages.Sum(x => x.Value == float.NaN ? 0f : x.Value);

            _levelTimeText.text = Timer.Instance.GetFormattedTime(time).ToString();
            _levelTimeText.color = (save.timeForStages.Count == info.metadata.stages.Count || time == float.NaN || time <= 0f) ? Color.white : Color.red;

            if (info.cover != null)
            {
                _levelCover.sprite = Sprite.Create(info.cover, new Rect(0, 0, info.cover.width, info.cover.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                _levelCover.sprite = null;
            }
         }
    }
}
