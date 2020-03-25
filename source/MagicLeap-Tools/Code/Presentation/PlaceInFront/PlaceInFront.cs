// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using UnityEngine;

namespace MagicLeapTools
{
    public class PlaceInFront : MonoBehaviour
    {
        //Public Variables:
        [Tooltip("Where should we place this transform in relation to the user's view?")]
        public Vector3 offset = new Vector3(0, -0.2f, .508f);
        [Tooltip("Should we put in front of the user at application Start?")]
        public bool runAtStart = true;
        [Tooltip("If run at start is true this will delay placement.")]
        public float delay = .25f;
        [Tooltip("After placement, should we face the camera?")]
        public bool faceCamera = true;
        [Tooltip("Is the content's forward different than the transform's forward?")]
        public bool flipForward;
        
        //Private Variables:
        private Transform _camera;

        //Init:
        private IEnumerator Start()
        {
            if (runAtStart)
            {
                //hide:
                foreach (var item in GetComponentsInChildren<Renderer>())
                {
                    item.enabled = false;
                }

                //wait:
                yield return new WaitForSeconds(delay);

                //place:
                Place();

                //show:
                foreach (var item in GetComponentsInChildren<Renderer>())
                {
                    item.enabled = true;
                }
            }
        }

        //Public Methods:
        public void Place()
        {
            //refs:
            if (_camera == null)
            {
                _camera = Camera.main.transform;
            }

            //hault physics:
            foreach (var item in GetComponentsInChildren<Rigidbody>())
            {
                item.velocity = Vector3.zero;
                item.angularVelocity = Vector3.zero;
            }

            //place:
            Vector3 flatForward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
            Matrix4x4 matrix = Matrix4x4.TRS(_camera.position, Quaternion.LookRotation(flatForward), Vector3.one);
            transform.position = matrix.MultiplyPoint3x4(offset);

            //face:
            if (faceCamera)
            {
                Vector3 to = Vector3.ProjectOnPlane(_camera.position - transform.position, Vector3.up).normalized;
                if (flipForward)
                {
                    to *= -1;
                }
                transform.rotation = Quaternion.LookRotation(to);
            }
        }
    }
}