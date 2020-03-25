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
using UnityEngine.XR.MagicLeap;

namespace MagicLeapTools
{
    public class GraspVisualizer : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public MLHandTracking.HandType hand;
        public Transform volume;
        public Transform point;

        //Private Variables:
        private ManagedHand _managedHand;
        private Vector3 _relativeOffset = new Vector3(-0.0508f, 0, 0.0508f);
        
        //Init:
        private void Awake()
        {
            //hooks:
            HandInput.OnReady += HandleHandsReady;
        }

        //Loops:
        private void Update()
        {
            if (!HandInput.Ready)
            {
                return;
            }

            volume.gameObject.SetActive(_managedHand.Visible);

            if (!_managedHand.Visible)
            {
                return;
            }
            
            volume.gameObject.SetActive(!_managedHand.Skeleton.InsideClipPlane);
            volume.position = _managedHand.Gesture.Grasp.position;
            volume.localScale = Vector3.one * _managedHand.Gesture.Grasp.radius;

            point.position = _managedHand.Skeleton.Position;
            point.rotation = _managedHand.Skeleton.Rotation;
            point.gameObject.SetActive(_managedHand.Gesture.Intent == IntentPose.Grasping && !_managedHand.Skeleton.InsideClipPlane);
        }

        //Event Handlers:
        private void HandleHandsReady()
        {
            //get hand:
            if (hand == MLHandTracking.HandType.Right)
            {
                _managedHand = HandInput.Right;
            }
            else
            {
                _managedHand = HandInput.Left;
            }
        }
#endif
    }
}