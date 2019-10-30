// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeapTools
{
    public class ControlHaptics : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public ControlInput controlInput;
        public HapticSetting triggerDown;
        public HapticSetting touchDown;
        public HapticSetting touchUp;
        public HapticSetting forceTouchDown;
        public HapticSetting forceTouchUp;
        public HapticSetting radialMove;
        public HapticSetting triggerHold;
        public HapticSetting bumperHold;
        public HapticSetting touchHold;

        //Private Variables:
        private readonly float _radialAngleAmount = 15;
        private float _angleAccumulation;

        //Init:
        private void Reset()
        {
            //refs:
            controlInput = GetComponent<ControlInput>();

            triggerDown = new HapticSetting(true, MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.High);
            touchDown = new HapticSetting(true, MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
            touchUp = new HapticSetting(true, MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
            forceTouchDown = new HapticSetting(true, MLInputControllerFeedbackPatternVibe.ForceDown, MLInputControllerFeedbackIntensity.High);
            forceTouchUp = new HapticSetting(true, MLInputControllerFeedbackPatternVibe.ForceUp, MLInputControllerFeedbackIntensity.High);
            radialMove = new HapticSetting(true, MLInputControllerFeedbackPatternVibe.Tick, MLInputControllerFeedbackIntensity.Low);
            triggerHold = new HapticSetting(true, MLInputControllerFeedbackPatternVibe.ForceDown, MLInputControllerFeedbackIntensity.High);
            bumperHold = new HapticSetting(true, MLInputControllerFeedbackPatternVibe.ForceDown, MLInputControllerFeedbackIntensity.High);
            touchHold = new HapticSetting(true, MLInputControllerFeedbackPatternVibe.ForceDown, MLInputControllerFeedbackIntensity.High);
        }

        //Flow:
        private void OnEnable()
        {
            //hooks:
            controlInput.OnTriggerDown.AddListener(HandleTriggerDown);
            controlInput.OnTouchDown.AddListener(HandleTouchDown);
            controlInput.OnForceTouchDown.AddListener(HandleForceTouchDown);
            controlInput.OnForceTouchUp.AddListener(HandleForceTouchUp);
            controlInput.OnTouchRadialMove.AddListener(HandleRadialMove);
            controlInput.OnTouchUp.AddListener(HandleTouchUp);
            controlInput.OnTriggerHold.AddListener(HandleTriggerHold);
            controlInput.OnBumperHold.AddListener(HandleBumperHold);
            controlInput.OnTouchHold.AddListener(HandleOnTouchHold);
        }

        private void OnDisable()
        {
            //unhooks:
            controlInput.OnTriggerDown.RemoveListener(HandleTriggerDown);
            controlInput.OnTouchDown.RemoveListener(HandleTouchDown);
            controlInput.OnForceTouchDown.RemoveListener(HandleForceTouchDown);
            controlInput.OnForceTouchUp.RemoveListener(HandleForceTouchUp);
            controlInput.OnTouchRadialMove.RemoveListener(HandleRadialMove);
            controlInput.OnTouchUp.RemoveListener(HandleTouchUp);
            controlInput.OnTriggerHold.RemoveListener(HandleTriggerHold);
            controlInput.OnBumperHold.RemoveListener(HandleBumperHold);
            controlInput.OnTouchHold.RemoveListener(HandleOnTouchHold);
        }

        //Event Handlers:
        private void HandleTriggerDown()
        {
            PerformHaptic(triggerDown);
        }

        private void HandleTouchDown(Vector4 touch)
        {
            PerformHaptic(touchDown);
        }

        private void HandleForceTouchDown()
        {
            PerformHaptic(forceTouchDown);
        }

        private void HandleForceTouchUp()
        {
            PerformHaptic(forceTouchUp);
        }

        private void HandleRadialMove(float angleDelta)
        {
            _angleAccumulation += Mathf.Abs(angleDelta);
            if (_angleAccumulation > _radialAngleAmount)
            {
                PerformHaptic(radialMove);
                _angleAccumulation = 0;
            }
        }

        private void HandleTouchUp(Vector4 touch)
        {
            PerformHaptic(touchUp);
        }

        private void HandleTriggerHold()
        {
            PerformHaptic(triggerHold);
        }

        private void HandleBumperHold()
        {
            PerformHaptic(bumperHold);
        }

        private void HandleOnTouchHold()
        {
            PerformHaptic(touchHold);
        }

        //Public Methods:
        public void StartHaptic(MLInputControllerFeedbackPatternVibe vibe, MLInputControllerFeedbackIntensity intensity)
        {
            controlInput.Control.StartFeedbackPatternVibe(vibe, intensity);
        }

        public void StopHaptic()
        {
            controlInput.Control.StopFeedbackPatternVibe();
        }

        //Private Methods:
        private void PerformHaptic(HapticSetting settings)
        {
            if (!settings.enabled)
            {
                return;
            }

            controlInput.Control.StartFeedbackPatternVibe(settings.pattern, settings.instensity);
        }
#endif
    }
}