// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MagicLeapTools
{
    public abstract class VirtualButton : MonoBehaviour
    {
        //Public Variables:
        [Tooltip("The radius around the transform that will be evaluated for a hit.")]
        public float radius = 0.025f;
        [Tooltip("How far a user needs to press in to trigger a pressed event - the physical movement distance of the button.")]
        public float touchDistance = 0.02f;
        [Tooltip("How far away is a hover considered.")]
        public float hoverDistance = 0.1f;
        [Tooltip("Is the visual forward opposite of transform forward?")]
        public bool flippedForward;

        //Events:
        /// <summary>
        /// Thrown when completely pressed.
        /// </summary>
        public UnityEvent OnPressed = new UnityEvent();
        /// <summary>
        /// Thrown when a press is lifted.
        /// </summary>
        public UnityEvent OnReleased = new UnityEvent();
        /// <summary>
        /// Thrown when interaction enters the hover zone.
        /// </summary>
        public UnityEvent OnHoverBegin = new UnityEvent();
        /// <summary>
        /// Thrown when an interaction moves within the hover zone.
        /// </summary>
        public FloatEvent OnHoverUpdated = new FloatEvent();
        /// <summary>
        /// Thrown when an interaction exits the hover zone.
        /// </summary>
        public UnityEvent OnHoverEnd = new UnityEvent();
        /// <summary>
        /// Thrown when an interaction enters the touch zone.
        /// </summary>
        public UnityEvent OnTouchBegin = new UnityEvent();
        /// <summary>
        /// Thrown when an interaction moves within the touch zone.
        /// </summary>
        public FloatEvent OnPressUpdated = new FloatEvent();
        /// <summary>
        /// Thrown when an interaction exits the touch zone.
        /// </summary>
        public UnityEvent OnTouchEnd = new UnityEvent();
        /// <summary>
        /// Thrown when an interaction exits early.
        /// </summary>
        public UnityEvent OnCanceled = new UnityEvent();
        /// <summary>
        /// Thrown when value changes - primarily used for variations that store a value such as a toggle.
        /// </summary>
        public BoolEvent OnValueChanged = new BoolEvent();

        private class EvaluationStatus
        {
            //Public Variables:
            public Vector3 current;
            public Vector3 previous;

            //Public Variables:
            public void Set(Vector3 current)
            {
                previous = this.current;
                this.current = current;
            }

            //Constructors:
            public EvaluationStatus(Vector3 initial)
            {
                current = initial;
                previous = initial;
            }
        }

        //Public Properties:
        /// <summary>
        /// Used internally to properly scale with the transform.
        /// </summary>
        public float ScaledTouchDistance
        {
            get
            {
                return touchDistance * transform.localScale.z;
            }
        }

        /// <summary>
        /// Used internally to properly scale with the transform.
        /// </summary>
        public float ScaledRadius
        {
            get
            {
                return radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
            }
        }

        /// <summary>
        /// Is this button being interacted with?
        /// </summary>
        public bool InteractionActive
        {
            get;
            private set;
        }

        /// <summary>
        /// Provides the world space location of the touch zone's enter location.
        /// </summary>
        public Vector3 TouchPlaneLocation
        {
            get
            {
                return InputNormal * ScaledTouchDistance + transform.position;
            }
        }

        /// <summary>
        /// Provides the world space location of the hover zone's enter location.
        /// </summary>
        public Vector3 HoverPlaneLocation
        {
            get
            {
                return InputNormal * hoverDistance + transform.position;
            }
        }

        /// <summary>
        /// Provides the world space normal that input is analyzed along.
        /// </summary>
        public Vector3 InputNormal
        {
            get
            {
                if (flippedForward)
                {
                    return -transform.forward;
                }
                else
                {
                    return transform.forward;
                }
            }
        }

        /// <summary>
        /// Pressed?
        /// </summary>
        public bool Pressed
        {
            get;
            private set;
        }

        /// <summary>
        /// How far into the hover zone is the interaction?
        /// </summary>
        public float HoverPercentage
        {
            get;
            private set;
        }

        /// <summary>
        /// How far into the touch zone is the interaction?
        /// </summary>
        public float TouchPercentage
        {
            get;
            private set;
        }

        //Private Variables:
        private Dictionary<string, EvaluationStatus> _evaluations = new Dictionary<string, EvaluationStatus>();
        private EvaluationStatus _activeStatus;

        //Public Methods:
        public void Evaluate()
        {
            if (!InteractionActive)
            {
                DetectBegin();
            }
            else
            {
                DetectEnd();
                UpdateProgress();
            }
        }

        public void Register(string id)
        {
            _evaluations.Add(id, new EvaluationStatus(Vector3.negativeInfinity));
        }

        public void Register(string id, Vector3 initial)
        {
            _evaluations.Add(id, new EvaluationStatus(initial));
        }

        public void Set(string id, Vector3 current)
        {
            _evaluations[id].Set(current);
        }
        
        //Private Methods:
        private void DetectBegin()
        {
            //sets:
            Plane backPlane = new Plane(InputNormal, transform.position);

            //entered button volume?
            foreach (var item in _evaluations)
            {
                //skip inactive items:
                if (float.IsNegativeInfinity(item.Value.current.x))
                {
                    continue;
                }

                //status:
                Vector3 onPlane = backPlane.ClosestPointOnPlane(item.Value.current);
                float onPlaneDistance = Vector3.Distance(transform.position, onPlane);

                //within radius?
                if (onPlaneDistance * 2 <= ScaledRadius)
                {
                    //status:
                    Vector3 hoverPlanePosition = InputNormal * (hoverDistance + ScaledTouchDistance) + transform.position;
                    float zonePercentage = MathUtilities.TraveledPercentage(transform.position, hoverPlanePosition, item.Value.current);

                    //within zones?
                    if (zonePercentage >= 0 && zonePercentage <= 1)
                    {
                        _activeStatus = item.Value;
                        InteractionActive = true;
                        OnHoverBegin?.Invoke();
                        break;
                    }
                }
            }
        }

        private void UpdateProgress()
        {
            //interaction ended?
            if (!InteractionActive)
            {
                HoverPercentage = 0;
                TouchPercentage = 0;
                return;
            }

            //changed?
            if (_activeStatus.previous == _activeStatus.current)
            {
                return;
            }

            //sets:
            Vector3 hoverLocation = InputNormal * hoverDistance + transform.position;
            Vector3 touchLocation = InputNormal * ScaledTouchDistance + transform.position;
            Plane touchPlane = new Plane(InputNormal, touchLocation);
            Plane backPlane = new Plane(InputNormal, transform.position);

            //status:
            bool touchPrevious = touchPlane.GetSide(_activeStatus.previous);
            bool touchCurrent = touchPlane.GetSide(_activeStatus.current);
            bool pressPrevious = backPlane.GetSide(_activeStatus.previous);
            bool pressCurrent = backPlane.GetSide(_activeStatus.current);

            //touch began?
            if (touchPrevious && !touchCurrent)
            {
                OnTouchBegin?.Invoke();
            }

            //touch end?
            if (!touchPrevious && touchCurrent)
            {
                OnTouchEnd?.Invoke();
            }

            //pressed?
            if (pressPrevious && !pressCurrent)
            {
                Pressed = transform;
                OnPressed?.Invoke();
            }

            if (!pressPrevious && pressCurrent)
            {
                Pressed = false;
                OnReleased?.Invoke();
            }

            //progress:
            HoverPercentage = MathUtilities.TraveledPercentage(hoverLocation, touchLocation, _activeStatus.current);
            TouchPercentage = MathUtilities.TraveledPercentage(touchLocation, transform.position, _activeStatus.current);
            OnHoverUpdated?.Invoke(HoverPercentage);
            if (TouchPercentage >= 0)
            {
                OnPressUpdated?.Invoke(TouchPercentage);
            }
        }

        private void DetectEnd()
        {
            //sets:
            Plane hoverPlane = new Plane(InputNormal, InputNormal * hoverDistance + transform.position);
            bool previous = hoverPlane.GetSide(_activeStatus.previous);
            bool current = hoverPlane.GetSide(_activeStatus.current);

            //hover breached?
            if (!previous && current)
            {
                EndInteraction();
                OnHoverEnd?.Invoke();
            }
            else
            {
                //sets:
                Plane radiusPlane = new Plane(InputNormal, transform.position);
                Vector3 onPlane = radiusPlane.ClosestPointOnPlane(_activeStatus.current);
                float onPlaneDistance = Vector3.Distance(transform.position, onPlane);

                //left radius?
                if (onPlaneDistance * 2 > ScaledRadius)
                {
                    EndInteraction();
                    OnCanceled?.Invoke();
                    if (Pressed)
                    {
                        Pressed = false;
                        OnReleased?.Invoke();
                    }
                }
            }
        }

        private void EndInteraction()
        {
            InteractionActive = false;
        }
    }
}