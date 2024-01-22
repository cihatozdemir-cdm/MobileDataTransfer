using System;
using UnityEngine;

namespace MobileDataTransfer.Unity
{
    [DisallowMultipleComponent]
    public class GameObjectEventTrigger : MonoBehaviour
    {
        public Action updateCallback { get; set; }

        private void OnDestroy()
        {
            updateCallback = null;
        }

        private void Update()
        {
            updateCallback?.Invoke();
        }
    }
}