// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System;

namespace MagicLeapTools
{
    public class HandInput : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Events:
        public static event Action OnReady;

        //Public Properties:
        public static bool Ready
        {
            get;
            private set;
        }

        public static ManagedHand Right
        {
            get
            {
                if (_right == null)
                {
                    NotReadyError();
                }
                return _right;
            }
        }

        public static ManagedHand Left
        {
            get
            {
                if (_left == null)
                {
                    NotReadyError();
                }
                return _left;
            }
        }

        //Public Variables:
        [Header("Experimental")]
        [Tooltip("Allows a palm collider for pushing content around when the hand is fully open. Experimental for now since grabbing for something can incur a hit from the palm collider which knocks it away.")]
        public bool palmCollisions;

        //Private Variables:
        private static ManagedHand _right;
        private static ManagedHand _left;

        //Init:
        private void Start()
        {
            //turn on inputs:
            if (!MLHandTracking.IsStarted)
            {
                if (!MLHandTracking.Start().IsOk)
                {
                    enabled = false;
                }
                else
                {
                    MLHandTracking.KeyPoseManager.SetKeyPointsFilterLevel(MLHandTracking.KeyPointFilterLevel.Smoothed);
                }
            }

            //setup hand tracking:
            List<MLHandTracking.HandKeyPose> handPoses = new List<MLHandTracking.HandKeyPose>();
            handPoses.Add(MLHandTracking.HandKeyPose.Finger);
            handPoses.Add(MLHandTracking.HandKeyPose.Pinch);
            handPoses.Add(MLHandTracking.HandKeyPose.Fist);
            handPoses.Add(MLHandTracking.HandKeyPose.Thumb);
            handPoses.Add(MLHandTracking.HandKeyPose.L);
            handPoses.Add(MLHandTracking.HandKeyPose.OpenHand);
            handPoses.Add(MLHandTracking.HandKeyPose.Ok);
            handPoses.Add(MLHandTracking.HandKeyPose.C);
            handPoses.Add(MLHandTracking.HandKeyPose.NoPose);
            MLHandTracking.KeyPoseManager.EnableKeyPoses(handPoses.ToArray(), true, false);

            _right = new ManagedHand(MLHandTracking.Right, this);
            _left = new ManagedHand(MLHandTracking.Left, this);

            //ready:
            Ready = true;
            OnReady?.Invoke();
        }

        private void OnDestroy()
        {
            //turn off hand tracking:
            if (MLHandTracking.IsStarted)
            {
                MLHandTracking.Stop();
            }
        }
        
        //Loops:
        private void Update()
        {
            //avoid MLHands start failures:
            if (!Ready)
            {
                return;
            }

            //process hands:
            _right.Update();
            _left.Update();
        }

        //Private Methods:
        private static void NotReadyError()
        {
            Debug.LogError("Hand input not ready. Check 'Ready' property or subscribe to OnReady event before accessing.");
        }
#endif
    }
}