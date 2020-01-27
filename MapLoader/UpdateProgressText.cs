using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MapLoader
{
    public class UpdateProgressText : MonoBehaviour
    {
        private TextMeshProUGUI _progressText;

        public void Update()
        {
            if(_progressText == null)
            {
                _progressText = GetComponent<TextMeshProUGUI>();
            }

            _progressText.text = $"Loading...\n{LevelAssetsLoader.Instance.loadingProgress:P}";
        }

    }
}
