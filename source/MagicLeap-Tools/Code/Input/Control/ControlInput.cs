// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.MagicLeap;

namespace MagicLeapTools
{
    public class ControlInput : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Enum:
        public enum ControlHandedness {Any, Left, Right };

        //Public Variables:
        [Tooltip("Which hand?")]
        public ControlHandedness handedness;
        [Tooltip("Match transform to control?")]
        public bool followControl;

        //Events
        /// <summary>
        /// Fired when a control is connected.
        /// </summary>
        public UnityEvent OnControlConnected = new UnityEvent();
        /// <summary>
        /// Fired when a control is disconnected.
        /// </summary>
        public UnityEvent OnControlDisconnected = new UnityEvent();
        /// <summary>
        /// Fired as soon as the trigger has been depressed at all.
        /// </summary>
        public UnityEvent OnTriggerPressBegan = new UnityEvent();
        /// <summary>
        /// Fired when the trigger has passed its TriggerDownThreshold.
        /// </summary>
        public UnityEvent OnTriggerDown = new UnityEvent();
        /// <summary>
        /// Fired when the trigger has been rapidly squeezed twice.
        /// </summary>
        public UnityEvent OnDoubleTrigger = new UnityEvent();
        /// <summary>
        /// Fired when the trigger has passed its TriggerUpThreshold.
        /// </summary>
        public UnityEvent OnTriggerUp = new UnityEvent();
        /// <summary>
        /// Fired when the trigger has been completely released.
        /// </summary>
        public UnityEvent OnTriggerPressEnded = new UnityEvent();
        /// <summary>
        /// Fired when the trigger has been held for longer than _triggerHoldDuration.
        /// </summary>
        public UnityEvent OnTriggerHold = new UnityEvent();
        /// <summary>
        /// Fired when the trigger has been released or pulled more since the last frame.
        /// </summary>
        public FloatEvent OnTriggerMove = new FloatEvent();
        /// <summary>
        /// Fired when the home button is pressed.
        /// </summary>
        public UnityEvent OnHomeButtonTap = new UnityEvent();
        /// <summary>
        /// Fired when the home button has been rapidly pressed twice.
        /// </summary>
        public UnityEvent OnDoubleHome = new UnityEvent();
        /// <summary>
        /// Fired when the bumper is pressed.
        /// </summary>
        public UnityEvent OnBumperDown = new UnityEvent();
        /// <summary>
        /// Fired when the bumper has been rapidly pressed twice.
        /// </summary>
        public UnityEvent OnDoubleBumper = new UnityEvent();
        /// <summary>
        /// Fired when the bumper is released.
        /// </summary>
        public UnityEvent OnBumperUp = new UnityEvent();
        /// <summary>
        /// Fired when the bumper has been held for longer than _bumperHoldDuration.
        /// </summary>
        public UnityEvent OnBumperHold = new UnityEvent();
        /// <summary>
        /// Fired when the touch pad is touched. X, Y, Z and W is for angle.
        /// </summary>
        public Vector4Event OnTouchDown = new Vector4Event();
        /// <summary>
        /// Fired when the touch pad has been rapidly pressed twice. X, Y, Z and W is for angle.
        /// </summary>
        public Vector4Event OnDoubleTap = new Vector4Event();
        /// <summary>
        /// Fired when a touch has been lifted from the touch pad. X, Y, Z and W is for angle.
        /// </summary>
        public Vector4Event OnTouchUp = new Vector4Event();
        /// <summary>
        /// Fired when a touch has moved from the last update. X, Y, Z and W is for angle.
        /// </summary>
        public Vector4Event OnTouchMove = new Vector4Event();
        /// <summary>
        /// Fired when a touch moves on the touch pad and provides and angle change in degrees.
        /// </summary>
        public FloatEvent OnTouchRadialMove = new FloatEvent();
        /// <summary>
        /// Fired when a touch has been held for longer than _touchHoldDuration.
        /// </summary>
        public UnityEvent OnTouchHold = new UnityEvent();
        /// <summary>
        /// Fired when a touch force passes _forceTouchDownThreshold.
        /// </summary>
        public UnityEvent OnForceTouchDown = new UnityEvent();
        /// <summary>
        /// Fired when a touch force passes _forceTouchUpThreshold.
        /// </summary>
        public UnityEvent OnForceTouchUp = new UnityEvent();
        /// <summary>
        /// Fired when a touch initially moves far enough to designated an intentional move.
        /// </summary>
        public UnityEvent OnTouchBeganMoving = new UnityEvent();
        /// <summary>
        /// Fired when a swipe gesture occurs.
        /// </summary>
        public TouchpadGestureDirectionEvent OnSwipe = new TouchpadGestureDirectionEvent();
        /// <summary>
        /// Fired when a tap begins and ends quickly and in a similar location.
        /// </summary>
        public TouchpadGestureDirectionEvent OnTapped = new TouchpadGestureDirectionEvent();
        
        //Public Properties:
        /// <summary>
        /// Location of the control.
        /// </summary>
        public Vector3 Position
        {
            get;
            private set;
        }

        /// <summary>
        /// A reference to the control.
        /// </summary>
        public MLInput.Controller Control
        {
            get;
            private set;
        }

        /// <summary>
        /// Orientation of the control.
        /// </summary>
        public Quaternion Orientation
        {
            get;
            private set;
        }

        /// <summary>
        /// Is the trigger pulled?
        /// </summary>
        public bool Trigger
        {
            get;
            private set;
        }

        /// <summary>
        /// Current depressed value of the trigger.
        /// </summary>
        public float TriggerValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Is the bumper down?
        /// </summary>
        public bool Bumper
        {
            get;
            private set;
        }

        /// <summary>
        /// Is a thumb on the control touch surface?
        /// </summary>
        public bool Touch
        {
            get;
            private set;
        }

        /// <summary>
        /// Was the thumb intentionally moved on the touch surface?
        /// </summary>
        public bool TouchMoved
        {
            get;
            private set;
        }

        /// <summary>
        /// What is the status of the touch? X, Y, Z and W is for angle.
        /// </summary>
        public Vector4 TouchValue
        {
            get;
            private set;
        }

        /// <summary>
        /// What was the angle change of the thumb on the touch surface - useful for rotating with the touch surface.
        /// </summary>
        public float TouchRadialDelta
        {
            get;
            private set;
        }

        /// <summary>
        /// Is the touch surface being pressed hard?
        /// </summary>
        public bool ForceTouch
        {
            get;
            private set;
        }

        /// <summary>
        /// Is the device connected?
        /// </summary>
        public bool Connected
        {
            get;
            private set;
        }

        //Private Variables:
        private readonly float _triggerDoubleDuration = .5f;
        private float _triggerLastTime;
        private readonly float _touchDoubleDuration = .5f;
        private float _lastTouchTime;
        private readonly float _bumperDoubleTapDuration = .5f;
        private float _lastBumperTime;
        private readonly float _homeDoubleDuration = .5f;
        private float _lastHomeTime;
        private Vector4 _touchBeganValue;
        private float _touchBeganTime;
        private readonly float _maxTapDuration = .5f;
        private readonly float _maxSwipeDuration = .5f;
        private readonly float _minSwipeDistance = .2f;
        private readonly float _minTouchMove = 0.004f;
        private readonly float _maxDoubleTouchDistance = .2f;
        private readonly float _forceTouchDownThreshold = .8f;
        private readonly float _forceTouchUpThreshold = .2f;
        private readonly float _touchBeganMovingThreshold = 0.04f;
        private readonly float _triggerHoldDuration = 2;
        private readonly float _bumperHoldDuration = 2;
        private readonly float _touchHoldDuration = 2;
        private readonly float _minForceDelta = 0.01f;
        private bool _wasForceTouched;
        private float _angleAccumulation;
        private ControlHandedness _previousHand;
        private Vector4 _activeTouch;

        //Init:
        private void Start()
        {
            //sets:
            _previousHand = handedness;

            //startup:
            if (!MLInput.IsStarted)
            {
                MLResult result = MLInput.Start();
                if (!result.IsOk)
                {
                    enabled = false;
                }
            }

            GetControl();

            //hooks:
            MLInput.OnControllerConnected += HandleControlConnected;
            MLInput.OnControllerDisconnected += HandleControlDisconnected;
        }

        //Deinit:
        private void OnDestroy()
        {
            //look for activeControl Inputs:
            bool anyControlInputsStillActive = false;
            foreach (var item in FindObjectsOfType<ControlInput>())
            {
                if (item.enabled)
                {
                    anyControlInputsStillActive = true;
                    break;
                }
            }

            //if no ControlInputs are active lets go ahead and turn off MLInput:
            if (!anyControlInputsStillActive)
            {
                if (MLInput.IsStarted) MLInput.Stop();
            }

            //steps:
            StopAllCoroutines();

            //unhooks:
            MLInput.OnControllerConnected -= HandleControlConnected;
            MLInput.OnControllerDisconnected -= HandleControlDisconnected;
            MLInput.OnControllerButtonDown -= HandleControlButtonDown;
            MLInput.OnControllerButtonUp -= HandleControlButtonUp;
            MLInput.OnTriggerDown -= HandleOnTriggerDown;
            MLInput.OnTriggerUp -= HandleOnTriggerUp;

            //clear:
            Control = null;
        }

        //Private Methods:
        private void GetControl()
        {
            for (int i = 0; i < 2; ++i)
            {
                MLInput.Controller control = MLInput.GetController(i);
                if (control.Type == MLInput.Controller.ControlType.Control)
                {
                    switch (handedness)
                    {
                        case ControlHandedness.Any:
                            Initialize(control);
                            break;

                        case ControlHandedness.Left:
                            if (control.Hand == MLInput.Hand.Left)
                            {
                                Initialize(control);
                            }
                            break;

                        case ControlHandedness.Right:
                            if (control.Hand == MLInput.Hand.Right)
                            {
                                Initialize(control);
                            }
                            break;
                    }
                }
            }
        }

        private void Initialize(MLInput.Controller control)
        {
            //status:
            Control = control;
            Connected = true;
            OnControlConnected?.Invoke();

            //hooks:
            MLInput.OnControllerButtonDown += HandleControlButtonDown;
            MLInput.OnControllerButtonUp += HandleControlButtonUp;
            MLInput.OnTriggerDown += HandleOnTriggerDown;
            MLInput.OnTriggerUp += HandleOnTriggerUp;
        }

        private Vector4 GetTouch1Info()
        {
            //build info:
            Vector4 touchInfo = Control.Touch1PosAndForce;

            //get angle:
            float angle = Vector2.Angle(Vector2.up, Control.Touch1PosAndForce);
            float dot = Vector2.Dot(Control.Touch1PosAndForce.normalized, Vector2.right);

            //on the left side of the touchpad?
            if (Mathf.Sign(dot) == -1)
            {
                angle = 360 - angle;
            }

            //set angle:
            touchInfo.w = angle;
            return touchInfo;
        }

        //Loops:
        private void Update()
        {
            if (_previousHand != handedness)
            {
                _previousHand = handedness;
                GetControl();
            }
            
            //no control?
            if (Control == null)
            {
                return;
            }

            //control pose:
            Position = Control.Position;
            Orientation = Control.Orientation;

            if (followControl)
            {
                transform.position = Position;
                transform.rotation = Orientation;
            }

            //touch cache:
            if (Control.Touch1Active)
            {
                _activeTouch = GetTouch1Info();
            }

            //touch down:
            if (!Touch && Control.Touch1Active)
            {
                Touch = true;
                StartCoroutine("TouchHold");

                //resets:
                TouchMoved = false;
                TouchRadialDelta = 0;
                
                //double - must be close to last touch and quick enough:
                float distanceFromLastTouchDown = Vector2.Distance(_activeTouch, TouchValue);
                float durationSinceLastTouch = Time.realtimeSinceStartup - _lastTouchTime;
                if (distanceFromLastTouchDown <= _maxDoubleTouchDistance && durationSinceLastTouch < _touchDoubleDuration)
                {
                    OnDoubleTap?.Invoke(TouchValue);
                }

                //cache:
                TouchValue = _activeTouch;
                _touchBeganValue = TouchValue;
                _touchBeganTime = Time.realtimeSinceStartup;
                _lastTouchTime = Time.realtimeSinceStartup;

                OnTouchDown?.Invoke(TouchValue);
            }

            //touch movement:
            if (Touch)
            {
                //touch force delta tracking:
                if (_activeTouch.z != TouchValue.z)
                {
                    //pressed enough to be a change?
                    float delta = Mathf.Abs(_activeTouch.z - TouchValue.z);
                    if (delta > _minForceDelta)
                    {
                        if (_activeTouch.z > TouchValue.z)
                        {
                            //touch is getting stronger:
                            if (!ForceTouch && _activeTouch.z >= _forceTouchDownThreshold)
                            {
                                ForceTouch = true;
                                _wasForceTouched = true;
                                OnForceTouchDown?.Invoke();
                            }
                        }
                        else
                        {
                            //touch is getting weaker:
                            if (ForceTouch && _activeTouch.z <= _forceTouchUpThreshold + _minForceDelta)
                            {
                                ForceTouch = false;
                                OnForceTouchUp?.Invoke();
                            }
                        }
                    }
                }

                //since force touch can make values go crazy we ignore everything if it happened:
                if (!_wasForceTouched)
                {
                    //did we have an intentional initial move?
                    if (!TouchMoved)
                    {
                        //did we initially move far enough?
                        float movedFromInitialTouchDistance = Vector2.Distance(_activeTouch, _touchBeganValue);

                        if (movedFromInitialTouchDistance > _touchBeganMovingThreshold)
                        {
                            TouchMoved = true;

                            OnTouchBeganMoving?.Invoke();
                        }
                    }

                    //only track subsequent moves if we initially began moving:
                    if (TouchMoved)
                    {
                        //did we have an intentional move?
                        float movedDistance = Vector2.Distance(TouchValue, _activeTouch);
                        if (TouchValue != _activeTouch && movedDistance > 0 && movedDistance > _minTouchMove)
                        {
                            //moved:
                            OnTouchMove?.Invoke(TouchValue);

                            //radial move:
                            float angleDelta = _activeTouch.w - TouchValue.w;
                            if (OnTouchRadialMove != null)
                            {
                                TouchRadialDelta = angleDelta;
                                OnTouchRadialMove?.Invoke(angleDelta);
                            }

                            //cache:
                            TouchValue = _activeTouch;
                        }
                    }
                }
            }

            //touch up:
            if (!Control.Touch1Active && Touch)
            {
                //status:
                Touch = false;
                TouchMoved = false;
                TouchValue = _activeTouch;
                StopCoroutine("TouchHold");

                //meta on touch sequence:
                Vector2 start = _touchBeganValue;
                Vector2 end = TouchValue;
                float distanceFromTouchStart = Vector2.Distance(start, end);
                float durationFromTouchStart = Time.realtimeSinceStartup - _touchBeganTime;

                //since force touch can make values go crazy we ignore everything if it happened:
                if (!_wasForceTouched)
                {
                    //swipe determinations:
                    if (distanceFromTouchStart >= _minSwipeDistance)
                    {
                        //swiped - we only calculate if the event is registered:
                        if (OnSwipe != null)
                        {
                            //swipes must be quicker than _maxSwipeDuration:
                            if (durationFromTouchStart < _maxSwipeDuration)
                            {
                                //get angle:
                                Vector2 swipe = (end - start).normalized;
                                float swipeAngle = Vector2.Angle(Vector2.up, swipe);

                                //swiped to the left? then we need to continue to 360 degrees:
                                if (end.x < start.x)
                                {
                                    swipeAngle = 360 - swipeAngle;
                                }

                                //determine swipe direction:
                                MLInput.Controller.TouchpadGesture.GestureDirection direction = MLInput.Controller.TouchpadGesture.GestureDirection.Left;
                                if (swipeAngle > 315 || swipeAngle <= 45)
                                {
                                    direction = MLInput.Controller.TouchpadGesture.GestureDirection.Up;
                                }
                                else if (swipeAngle > 45 && swipeAngle <= 135)
                                {
                                    direction = MLInput.Controller.TouchpadGesture.GestureDirection.Right;
                                }
                                else if (swipeAngle > 135 && swipeAngle <= 225)
                                {
                                    direction = MLInput.Controller.TouchpadGesture.GestureDirection.Down;
                                }

                                //radial swipe?
                                if (Control.CurrentTouchpadGesture.Type == MLInput.Controller.TouchpadGesture.GestureType.RadialScroll)
                                {
                                    direction = Control.CurrentTouchpadGesture.Direction;
                                }

                                OnSwipe?.Invoke(direction);
                            }
                        }
                    }
                    else
                    {
                        //tapped - we only calculate if the event is registered:
                        if (OnTapped != null)
                        {
                            //taps must be quicker than _maxTapDuration:
                            if (durationFromTouchStart < _maxTapDuration)
                            {
                                //determine tap location:
                                MLInput.Controller.TouchpadGesture.GestureDirection direction = MLInput.Controller.TouchpadGesture.GestureDirection.Left;
                                if (TouchValue.w > 315 || TouchValue.w <= 45)
                                {
                                    direction = MLInput.Controller.TouchpadGesture.GestureDirection.Up;
                                }
                                else if (TouchValue.w > 45 && TouchValue.w <= 135)
                                {
                                    direction = MLInput.Controller.TouchpadGesture.GestureDirection.Right;
                                }
                                else if (TouchValue.w > 135 && TouchValue.w <= 225)
                                {
                                    direction = MLInput.Controller.TouchpadGesture.GestureDirection.Down;
                                }

                                OnTapped?.Invoke(direction);
                            }
                        }
                    }
                }

                //we ultimately released so fire that event:
                OnTouchUp?.Invoke(TouchValue);

                //reset force touch activity on full release only to avoid any slight swipes at the end of release:
                _wasForceTouched = false;

                //if a user releases rapidly after a force press this will catch the release:
                if (ForceTouch)
                {
                    ForceTouch = false;
                    OnForceTouchUp?.Invoke();
                }
            }

            //trigger:
            if (TriggerValue != Control.TriggerValue)
            {
                //trigger began moving:
                if (TriggerValue == 0)
                {
                    OnTriggerPressBegan?.Invoke();
                }

                //trigger moved:
                OnTriggerMove?.Invoke(Control.TriggerValue - TriggerValue);

                //trigger released:
                if (Control.TriggerValue == 0)
                {
                    OnTriggerPressEnded?.Invoke();
                }

                TriggerValue = Control.TriggerValue;
            }
        }

        private IEnumerator TriggerHold()
        {
            yield return new WaitForSeconds(_triggerHoldDuration);
            OnTriggerHold?.Invoke();
        }

        private IEnumerator BumperHold()
        {
            yield return new WaitForSeconds(_bumperHoldDuration);
            OnBumperHold?.Invoke();
        }

        private IEnumerator TouchHold()
        {
            yield return new WaitForSeconds(_touchHoldDuration);
            OnTouchHold?.Invoke();
        }

        //Event Handlers:
        private void HandleControlDisconnected(byte controlId)
        {
            //wrong or no control?
            if (Control == null || controlId != Control.Id)
            {
                return;
            }

            //handle disconnect:
            Connected = false;
            Control = null;
            OnControlDisconnected?.Invoke();

            //unhook:
            MLInput.OnControllerButtonDown -= HandleControlButtonDown;
            MLInput.OnControllerButtonUp -= HandleControlButtonUp;
            MLInput.OnTriggerDown -= HandleOnTriggerDown;
            MLInput.OnTriggerUp -= HandleOnTriggerUp;

            StopAllCoroutines();
        }

        private void HandleControlConnected(byte controlId)
        {
            //we just want to work with the control:
            MLInput.Controller connectedControl = MLInput.GetController(controlId);

            switch (handedness)
            {
                case ControlHandedness.Any:
                    Initialize(MLInput.GetController(controlId));
                    break;

                case ControlHandedness.Left:
                    if (connectedControl.Hand == MLInput.Hand.Left)
                    {
                        Initialize(MLInput.GetController(controlId));
                    }
                    break;

                case ControlHandedness.Right:
                    if (connectedControl.Hand == MLInput.Hand.Right)
                    {
                        Initialize(MLInput.GetController(controlId));
                    }
                    break;
            }
        }

        private void HandleOnTriggerDown(byte controlId, float triggerValue)
        {
            //wrong or no control?
            if (Control == null || controlId != Control.Id)
            {
                return;
            }

            Trigger = true;
            StartCoroutine("TriggerHold");
            OnTriggerDown?.Invoke();

            //double?
            if (Time.realtimeSinceStartup - _triggerLastTime < _triggerDoubleDuration)
            {
                OnDoubleTrigger?.Invoke();
            }

            _triggerLastTime = Time.realtimeSinceStartup;
        }

        private void HandleOnTriggerUp(byte controlId, float triggerValue)
        {
            //wrong or no control?
            if (Control == null || controlId != Control.Id)
            {
                return;
            }

            Trigger = false;
            StopCoroutine("TriggerHold");
            OnTriggerUp?.Invoke();
        }

        private void HandleControlButtonDown(byte controlId, MLInput.Controller.Button button)
        {
            //wrong or no control?
            if (Control == null || controlId != Control.Id)
            {
                return;
            }

            switch (button)
            {
                case MLInput.Controller.Button.Bumper:
                    StartCoroutine("BumperHold");
                    Bumper = true;
                    OnBumperDown?.Invoke();

                    //double?
                    if (Time.realtimeSinceStartup - _lastBumperTime < _bumperDoubleTapDuration)
                    {
                        OnDoubleBumper?.Invoke();
                    }

                    _lastBumperTime = Time.realtimeSinceStartup;
                    break;

                case MLInput.Controller.Button.HomeTap:
                    OnHomeButtonTap?.Invoke();

                    //double?
                    if (Time.realtimeSinceStartup - _lastHomeTime < _homeDoubleDuration)
                    {
                        OnDoubleHome?.Invoke();
                    }

                    _lastHomeTime = Time.realtimeSinceStartup;
                    break;
            }
        }

        private void HandleControlButtonUp(byte controlId, MLInput.Controller.Button button)
        {
            //wrong or no control?
            if (Control == null || controlId != Control.Id)
            {
                return;
            }

            switch (button)
            {
                case MLInput.Controller.Button.Bumper:
                    StopCoroutine("BumperHold");
                    Bumper = false;
                    OnBumperUp?.Invoke();
                    break;
            }
        }
#endif
    }
}