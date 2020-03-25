// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeapTools
{
    public class SimpleHandInputDriver : InputDriver
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public MLHandTracking.HandType handedness;
        public float shoulderWidth = 0.37465f;
        public float shoulderDistanceBelowHead = 0.2159f;

        //Private Variables:
        private Transform _mainCamera;

        //Private Properties:
        private ManagedHand Hand
        {
            get
            {
                if (handedness == MLHandTracking.HandType.Left)
                {
                    return HandInput.Left;
                }
                else
                {
                    return HandInput.Right;
                }
            }
        }

        //Init:
        private void Awake()
        {
            //refs:
            _mainCamera = Camera.main.transform;

            //hooks:
            HandInput.OnReady += HandleReady;
        }

        //Event Handlers:
        private void HandleReady()
        {
            //hooks:
            Hand.OnVisibilityChanged += HandleVisibilityChanged;
            Hand.Gesture.OnIntentChanged += HandleIntent;
        }

        private void HandleIntent(ManagedHand hand, IntentPose pose)
        {
            //grab:
            if (pose == IntentPose.Grasping || pose == IntentPose.Pinching)
            {
                Fire0Down();
            }

            //release:
            if (pose == IntentPose.Relaxed)
            {
                Fire0Up();
            }
        }

        private void HandleVisibilityChanged(ManagedHand hand, bool active)
        {
            //for hiding and showing the pointer:
            if (active)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }

        //Loops:
        private void Update()
        {
            //wait for hand input to come online:
            if (!HandInput.Ready)
            {
                return;
            }

            //shoulder:
            float shoulderDistance = shoulderWidth * .5f;

            //swap for the left shoulder:
            if (handedness == MLHandTracking.HandType.Left)
            {
                shoulderDistance *= -1;
            }

            //source locations:
            Vector3 flatForward = Vector3.ProjectOnPlane(_mainCamera.forward, Vector3.up);
            Vector3 shoulder = TransformUtilities.WorldPosition(_mainCamera.position, Quaternion.LookRotation(flatForward), new Vector2(shoulderDistance, Mathf.Abs(shoulderDistanceBelowHead) * -1));
            Vector3 pointerOrigin = Vector3.Lerp(Hand.Skeleton.Thumb.Knuckle.positionFiltered, Hand.Skeleton.Position, .5f);

            //direction:
            Quaternion orientation = Quaternion.LookRotation(Vector3.Normalize(pointerOrigin - shoulder), Hand.Skeleton.Rotation * Vector3.up);

            //application:
            transform.position = pointerOrigin;
            transform.rotation = orientation;
        }
#endif
    }
}