// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeapTools
{
    public class RuntimeConsole : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        [Tooltip("Will only become visible in builds that have 'Development Build' on in Build Settings.")]
        public bool debugBuildsOnly;
        public bool showFPS = true;
        public bool showStackTrace;
        public bool logs = true;
        public bool warnings = true;
        public bool errors = true;
        public int maxVisible = 5;
        public Text logText;
		public Canvas logCanvas;
        public ControlInput controlInput;
        public Transform scrollMin;
        public Transform scrollMax;
        public Transform scrollHandle;

        //Private Variables:
        private List<string> _conditions = new List<string>();
        private List<string> _stackTrace = new List<string>();
        private int _index;
        private int _bottomIndex;
        private bool _manualScroll;
        private float _deltaTime;
        private List<float> _fpsHistory = new List<float>();
        private List<float> _msecHistory = new List<float>();
        private float _fpsDisplayInterval = 4;
        private float _lastFPSDisplayTime;

        //Init:
        private void Reset()
        {
            controlInput = GetComponent<ControlInput>();
        }

        private void Awake()
        {
            //only operate if not in editor:
            if (Application.isEditor)
            {
                Debug.LogWarning("RuntimeConsole is intended easy debugging on device and will be disabled while playing in the editor.");
                gameObject.SetActive(false);
            }

            //only operate if debug build:
            if (debugBuildsOnly && !Debug.isDebugBuild)
            {
                gameObject.SetActive(false);
            }

        }

        //Flow:
        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
            controlInput.OnTapped.AddListener(HandleTap);
            controlInput.OnTouchHold.AddListener(JumpToBottom);
            controlInput.OnBumperHold.AddListener(ToggleVisibility);
            StartCoroutine("FPSCounter");
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
            controlInput.OnTapped.RemoveListener(HandleTap);
            controlInput.OnTouchHold.RemoveListener(JumpToBottom);
			controlInput.OnBumperHold.RemoveListener(ToggleVisibility);
            StopAllCoroutines();
        }

        //Coroutines:
        private IEnumerator FPSCounter()
        {
            _lastFPSDisplayTime = Time.realtimeSinceStartup;

            while (true)
            {
                //values:
                _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
                _fpsHistory.Add(1.0f / _deltaTime);
                _msecHistory.Add(_deltaTime * 1000.0f);

                //interval:
                if (Time.realtimeSinceStartup - _lastFPSDisplayTime > _fpsDisplayInterval)
                {
                    _lastFPSDisplayTime = Time.realtimeSinceStartup;
                    if (showFPS)
                    {
                        Debug.Log($"Average fps {_fpsHistory.Average()}, ms / f {_msecHistory.Average()}");
                    }

                    _fpsHistory.Clear();
                    _msecHistory.Clear();
                }

                yield return null;
            }
        }

        //Event Handlers:
        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            //color code:
            switch (type)
            {
                case LogType.Log:
                    if (!logs)
                    {
                        return;
                    }

                    condition = $"<color=#ffffffff>{condition}</color>";
                    break;

                case LogType.Warning:
                    if (!warnings)
                    {
                        return;
                    }

                    condition = $"<color=#ffff00ff>{condition}</color>";
                    break;

                case LogType.Error:
                    if (!errors)
                    {
                        return;
                    }

                    condition = $"<color=#ff0000ff>{condition}</color>";
                    break;
            }

            stackTrace = $"<color=#c0c0c0ff>{stackTrace}</color>";

            //catalog:
            _conditions.Add(condition);
            _stackTrace.Add(stackTrace);

            //index updates:
            _bottomIndex = _conditions.Count - maxVisible;
            _bottomIndex = Mathf.Clamp(_bottomIndex, 0, _bottomIndex);
            if (!_manualScroll)
            {
                _index = _bottomIndex;
            }

            UpdateConsole();
        }

        //Event Handlers:
        private void HandleTap(MLInput.Controller.TouchpadGesture.GestureDirection direction)
        {
            switch (direction)
            {
                case MLInput.Controller.TouchpadGesture.GestureDirection.Up:
                    ScrollUp();
                    break;

                case MLInput.Controller.TouchpadGesture.GestureDirection.Down:
                    ScrollDown();
                    break;
            }
        }

        //Private Methods:
        private void UpdateConsole()
        {
            string output = "";

            int length = _index + maxVisible;
            length = Mathf.Clamp(length, length, _conditions.Count);

            for (int i = _index; i < length; i++)
            {
                output += _conditions[i];

                if (showStackTrace)
                {
                    output += _stackTrace[i];
                }

                //trailing new line:
                if (i != _index + maxVisible)
                {
                    output += "\n";
                }
            }
            logText.text = output;
   
            //update rects:
            foreach (var item in GetComponentsInChildren<RectTransform>())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            }

            //caculate scroller:
            Vector3 position = Vector3.Lerp(scrollMin.position, scrollMax.position, (float)_index / _bottomIndex);

            //nan protection:
            if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
            {
                position = scrollMin.position;
            }

            //update scroller:
            scrollHandle.position = Vector3.Lerp(scrollMin.position, scrollMax.position, (float)_index / _bottomIndex);
        }

        private void JumpToBottom()
        {
            _index = _bottomIndex;
            _manualScroll = false;
            UpdateConsole();
        }

        private void ScrollUp()
        {
            _manualScroll = true;
            _index -= maxVisible;
            _index = Mathf.Clamp(_index, 0, _index);
            UpdateConsole();
        }

        private void ScrollDown()
        {
            _manualScroll = true;
            _index += maxVisible;
            _index = Mathf.Clamp(_index, _index, _bottomIndex);

            if (_index == _bottomIndex)
            {
                _manualScroll = false;
            }

            UpdateConsole();
        }
		
        private void ToggleVisibility()
        {
           logCanvas.gameObject.SetActive(!logCanvas.gameObject.activeInHierarchy);
        }
#endif
    }
}