// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicLeapTools
{
    [ExecuteAlways]
    public class Billboard : MonoBehaviour
    {
        //Public Variables:
        [Tooltip("Is this object's visual forward opposite of its transform forward?")]
        public bool flippedForward;
        [Tooltip("Should the Y axis of this transform always match gravity?")]
        public bool matchGravity;
        [Tooltip("0 will instantly face the camera any other value (greater than 0) will employ easing.")]
        public float arrivalDuration;

        //Private Variables:
        private Transform _mainCamera;
        private Vector3 _direction;
        private Quaternion _rotation;
        private Quaternion _velocity;

        //Init:
        private void Awake()
        {
            _mainCamera = Camera.main.transform;

            //no camera?
            if (_mainCamera == null)
            {
                Debug.LogWarning("You must have a camera in your scene tagged as 'MainCamera' to use Billboard.");
                enabled = false;
            }
        }

        //Loops:
        private void LateUpdate()
        {
            //remove any negative durations:
            arrivalDuration = Mathf.Max(0, arrivalDuration);

            //direction:
            _direction = Vector3.Normalize(transform.position - _mainCamera.position);
            if (_direction == Vector3.zero)
            {
                return;
            }

            //match gravity?
            if (matchGravity)
            {
                _direction = Vector3.ProjectOnPlane(_direction, Vector3.up);
            }

            //flipped?
            if (flippedForward)
            {
                _direction *= -1;
            }

            //get rotation:
            if (_direction != Vector3.zero)
            {
                _rotation = Quaternion.LookRotation(_direction);
            }

            //apply:
            if (Application.isPlaying)
            {
                transform.rotation = MotionUtilities.SmoothDamp(transform.rotation, _rotation, ref _velocity, arrivalDuration);
            }
            else
            {
                //time doesn't update smoothly while not playing in the editor so let's just force snapping:
                transform.rotation = _rotation;
            }
        }
    }
}