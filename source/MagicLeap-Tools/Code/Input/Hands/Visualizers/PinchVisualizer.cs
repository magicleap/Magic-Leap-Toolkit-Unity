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
    public class PinchVisualizer : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public MLHandTracking.HandType hand;
        public Transform volume;
        public Transform point;

        //Private Variables:
        private ManagedHand _managedHand;

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
            volume.gameObject.SetActive(_managedHand.Gesture.Intent == IntentPose.Relaxed || _managedHand.Gesture.Intent == IntentPose.Pinching);
            volume.position = _managedHand.Gesture.Pinch.position;
            volume.localScale = Vector3.one * _managedHand.Gesture.Pinch.radius;

            point.position = _managedHand.Gesture.Pinch.position;
            point.rotation = _managedHand.Gesture.Pinch.rotation;
            point.gameObject.SetActive(_managedHand.Gesture.Intent == IntentPose.Pinching);
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