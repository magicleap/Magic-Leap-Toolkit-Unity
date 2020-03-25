// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeapTools
{
    public class RotationManipulator : MonoBehaviour
    {
        //Public Variables:
        [Tooltip("Exaggerate or diminish the rotation.")]
        public float multiplier = 1;
        [Tooltip("Is visual forward of the GameObject facing backwards? Used for resetting so we can face the camera.")]
        public bool flippedForward;

        //Private Methods:
        private Camera _mainCamera;

        //Init:
        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        //Public Methods:
        public void Rotate(float angleDelta)
        {
            transform.Rotate(Vector3.up * (angleDelta * multiplier));
        }

        public void ResetManipulation()
        {
            //look at camera flattened:
            Vector3 toCamera = _mainCamera.transform.position - transform.position;
            if (flippedForward)
            {
                toCamera *= -1;
            }
            Vector3 toCameraFlat = Vector3.ProjectOnPlane(toCamera, Vector3.up);
            transform.rotation = Quaternion.LookRotation(toCameraFlat);
        }
    }
}