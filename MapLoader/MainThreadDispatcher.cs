using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MapLoader
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        public static MainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("MainThreadDispatcher").AddComponent<MainThreadDispatcher>();
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }
        private static MainThreadDispatcher _instance;

        private static Queue<Action> _queuedActions = new Queue<Action>();

        public void Update()
        {
            while(_queuedActions.Count > 0)
            {
                _queuedActions.Dequeue()?.Invoke();
            }
        }

        public static void Enqueue(Action action)
        {
            _queuedActions.Enqueue(action);
        }
    }

}
