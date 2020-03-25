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
    public class PointingVisualizer : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public MLHandTracking.HandType hand;
        public Transform point;
        public Transform axis;
        
        //Private Variables:
        private ManagedHand _managedHand;

        //Private Properties:
        private float FastLerp
        {
            get
            {
                return Time.deltaTime * 20;
            }
        }

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

            if (_managedHand.Skeleton.Index.IsVisible && _managedHand.Gesture.Intent == IntentPose.Pointing)
            {
                point.position = _managedHand.Skeleton.Index.End;
                point.rotation = Quaternion.LookRotation(_managedHand.Skeleton.Index.DirectionFiltered, _managedHand.Skeleton.Rotation * Vector3.up);
                axis.position = point.position;
                axis.rotation = point.rotation;
                point.gameObject.SetActive(true);
                axis.gameObject.SetActive(true);
            }
            else
            {
                point.gameObject.SetActive(false);
                axis.gameObject.SetActive(false);
            }
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

            _managedHand.OnVisibilityChanged += HandleVisibilityChanged;
            _managedHand.Gesture.OnIntentChanged += HandleIntentChanged;
        }

        private void HandleIntentChanged(ManagedHand hand, IntentPose intent)
        {
            if (intent == IntentPose.Pointing)
            {
                point.gameObject.SetActive(true);
            }
            else
            {
                point.gameObject.SetActive(false);
            }
        }

        private void HandleVisibilityChanged(ManagedHand managedHand, bool visible)
        {
            if (visible && _managedHand.Gesture.Intent == IntentPose.Pointing)
            {
                point.gameObject.SetActive(true);
            }
        }
#endif
    }
}