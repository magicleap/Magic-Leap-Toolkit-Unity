// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

public class RuntimeConsoleExample : MonoBehaviour
{
#if PLATFORM_LUMIN
    //Private Variables:
    private bool _leftEyeBlinking;
    private bool _rightEyeBlinking;
    private bool _headsetOn;

    //Init:
    private void Awake()
    {
        MLEyes.Start();
    }

    //Deinit:
    private void OnDestroy()
    {
        if (MLEyes.IsStarted)
        {
            MLEyes.Stop();
        }
    }

    //Loops:
    private void Update()
    {
        if (!MLEyes.IsStarted)
        {
            return;
        }

        //was the headset put on?
        if (MLEyes.FixationConfidence > 0)
        {
            _headsetOn = true;
        }

        //headset not on yet:
        if (!_headsetOn)
        {
            return;
        }

        //status change tracker:
        bool eyesStatusChanged = false;

        //left eye status changed?
        if (_leftEyeBlinking != MLEyes.LeftEye.IsBlinking)
        {
            eyesStatusChanged = true;
            _leftEyeBlinking = MLEyes.LeftEye.IsBlinking;
        }

        //right eye status changed?
        if (_rightEyeBlinking != MLEyes.RightEye.IsBlinking)
        {
            eyesStatusChanged = true;
            _rightEyeBlinking = MLEyes.RightEye.IsBlinking;
        }

        //respond to changes:
        if (eyesStatusChanged)
        {
            //eyes closed?
            if (_leftEyeBlinking && _rightEyeBlinking)
            {
                Debug.LogError($"Error: Both eyes are closed! You can't see anything! Time: {Time.realtimeSinceStartup}");
                return;
            }

            //left eye wink?
            if (MLEyes.LeftEye.IsBlinking)
            {
                Debug.LogWarning($"Warning: Left eye is closed! Time: {Time.realtimeSinceStartup}");
                return;
            }

            //right eye wink?
            if (MLEyes.RightEye.IsBlinking)
            {
                Debug.Log($"Log: Right eye is closed! Time: {Time.realtimeSinceStartup}");
            }
        }
    }
#endif
}