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

namespace MagicLeapTools
{
    [RequireComponent(typeof(ControlInput))]
    public class ControlInputDriver : InputDriver
    {
#if PLATFORM_LUMIN
        //Private Variables:
        ControlInput _controlInput;

        //Init:
        private void Awake()
        {
            _controlInput = GetComponent<ControlInput>();
            Active = _controlInput.Control != null;
        }

        //Flow:
        private void OnEnable()
        {
            //hooks:
            _controlInput.OnTapped.AddListener(HandleTouchPad);
            _controlInput.OnSwipe.AddListener(HandleTouchPad);
            _controlInput.OnTriggerDown.AddListener(HandleTriggerDown);
            _controlInput.OnTriggerUp.AddListener(HandleTriggerUp);
            _controlInput.OnBumperDown.AddListener(HandleBumperDown);
            _controlInput.OnBumperUp.AddListener(HandleBumperUp);
            _controlInput.OnForceTouchDown.AddListener(HandleForceTouchDown);
            _controlInput.OnForceTouchUp.AddListener(HandleForceTouchUp);
            _controlInput.OnTouchRadialMove.AddListener(HandleTouchRadialMove);
            _controlInput.OnControlConnected.AddListener(HandleControlConnected);
            _controlInput.OnControlDisconnected.AddListener(HandleControlDisconnected);
        }

        private void OnDisable()
        {
            //unhook:
            _controlInput.OnTapped.RemoveListener(HandleTouchPad);
            _controlInput.OnSwipe.RemoveListener(HandleTouchPad);
            _controlInput.OnTriggerDown.RemoveListener(HandleTriggerDown);
            _controlInput.OnTriggerUp.RemoveListener(HandleTriggerUp);
            _controlInput.OnBumperDown.RemoveListener(HandleBumperDown);
            _controlInput.OnBumperUp.RemoveListener(HandleBumperUp);
            _controlInput.OnForceTouchDown.RemoveListener(HandleForceTouchDown);
            _controlInput.OnForceTouchUp.RemoveListener(HandleForceTouchUp);
            _controlInput.OnTouchRadialMove.RemoveListener(HandleTouchRadialMove);
            _controlInput.OnControlConnected.RemoveListener(HandleControlConnected);
            _controlInput.OnControlDisconnected.RemoveListener(HandleControlDisconnected);
        }

        //Event Handlers:
        private void HandleTriggerDown()
        {
            Fire0Down();
        }

        private void HandleTriggerUp()
        {
            Fire0Up();
        }

        private void HandleBumperDown()
        {
            Fire1Down();
        }

        private void HandleBumperUp()
        {
            Fire1Up();
        }

        private void HandleForceTouchDown()
        {
            Fire2Down();
        }

        private void HandleForceTouchUp()
        {
            Fire2Up();
        }

        private void HandleTouchRadialMove(float angleDelta)
        {
            RadialDrag(angleDelta);
        }

        private void HandleTouchPad(MLInput.Controller.TouchpadGesture.GestureDirection direction)
        {
            switch (direction)
            {
                case MLInput.Controller.TouchpadGesture.GestureDirection.Left:
                    Left();
                    break;

                case MLInput.Controller.TouchpadGesture.GestureDirection.Right:
                    Right();
                    break;

                case MLInput.Controller.TouchpadGesture.GestureDirection.Up:
                    Up();
                    break;

                case MLInput.Controller.TouchpadGesture.GestureDirection.Down:
                    Down();
                    break;
            }
        }

        private void HandleControlConnected()
        {
            Activate();
        }

        private void HandleControlDisconnected()
        {
            Deactivate();
        }
#endif
    }
}