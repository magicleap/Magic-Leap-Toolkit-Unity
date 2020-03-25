// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;

namespace MagicLeapTools
{
    public class Slider : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        [Tooltip("The visual that a user can grab and move.")]
        public DirectManipulation handle;
        [Tooltip("The minimum position of the slider.")]
        public Transform startPosition;
        [Tooltip("The maximum position of the slider.")]
        public Transform endPosition;
        [Tooltip("The value associated with the minimum position of the slider.")]
        public float minValue = 0;
        [Tooltip("The value associated with the maximum position of the slider.")]
        public float maxValue = 1;
        [Tooltip("Should the value only provide whole numbers?")]
        public bool wholeNumbers;
        [Tooltip("An initial value at Start.")]
        public float startingValue;
        [Tooltip("Dragging further than this distance away from the handle will end the drag operation.")]
        public float dragBreakDistance = 0.2032f;

        //Events:
        /// <summary>
        /// Thrown when the handle moves or the Value property is changed.
        /// </summary>
        public FloatEvent OnChanged = new FloatEvent();
        /// <summary>
        /// Thrown when the handle is grabbed.
        /// </summary>
        public UnityEvent OnGrabbed = new UnityEvent();
        /// <summary>
        /// Thrown when the handle is released.
        /// </summary>
        public UnityEvent OnReleased = new UnityEvent();

        //Public Propertes:
        public float Value
        {
            get
            {
                return (float)System.Math.Round(_value, 1);
            }

            set
            {
                value = Mathf.Clamp(value, minValue, maxValue);
                float percentage = Mathf.InverseLerp(minValue, maxValue, value);
                handle.transform.position = Vector3.Lerp(startPosition.position, endPosition.position, percentage);
                _value = value;
                OnChanged?.Invoke(Value);
            }
        }

        //Private Variables:
        private string _lineID;
        private float _value;

        //Init:
        private void Start()
        {
            //hooks:
            handle.OnGrabBegin.AddListener(HandleGrabBegin);
            handle.OnDragBegin.AddListener(HandleDragBegin);
            handle.OnDragUpdate.AddListener(HandleDragUpdate);
            handle.OnDragEnd.AddListener(HandleDragEnd);

            //setup:
            _lineID = gameObject.GetInstanceID().ToString();
            handle.rotatable = false;
            handle.scalable = false;
            handle.throwable = false;

            //set:
            Value = startingValue;
        }

        //Event Handlers:
        private void HandleGrabBegin(InteractionPoint interactionPoint)
        {
            OnGrabbed?.Invoke();
        }

        private void HandleDragBegin(InteractionPoint interactionPoint)
        {
            Lines.SetVisibility(_lineID, true);
        }

        private void HandleDragUpdate(InteractionPoint[] interactionPoint, Vector3 position, Quaternion rotation, float scale)
        {
            float percentage = MathUtilities.TraveledPercentage(startPosition.position, endPosition.position, position);
            handle.transform.position = Vector3.Lerp(startPosition.position, endPosition.position, Mathf.Clamp01(percentage));
            _value = Mathf.Lerp(minValue, maxValue, percentage);
            if (wholeNumbers)
            {
                _value = Mathf.Round(_value);
            }
            OnChanged?.Invoke(Value);

            //draw connection line:
            Lines.DrawLine(_lineID, Color.green, Color.green, .0025f, interactionPoint[0].position, handle.transform.position);

            //dragged too far?
            if (Vector3.Distance(interactionPoint[0].position, handle.transform.position) > dragBreakDistance)
            {
                handle.StopGrab();
            }
        }

        private void HandleDragEnd(InteractionPoint interactionPoint)
        {
            Lines.SetVisibility(_lineID, false);
            OnReleased?.Invoke();
        }

        //Gizmos:
        private void OnDrawGizmos()
        {
            if (startPosition != null && endPosition != null)
            {
                Gizmos.DrawLine(startPosition.position, endPosition.position);
            }
        }
#endif
    }
}