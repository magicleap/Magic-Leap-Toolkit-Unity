// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System;

namespace MagicLeapTools
{
    public class ManagedHand
    {
#if PLATFORM_LUMIN
        //Events:
        public event Action<ManagedHand, bool> OnVisibilityChanged;

        //Public Properties:
        public ManagedHandSkeleton Skeleton
        {
            get;
            private set;
        }

        public ManagedHandGesture Gesture
        {
            get;
            private set;
        }

        public ManagedHandCollider Collider
        {
            get;
            private set;
        }
        
        public MLHandTracking.Hand Hand
        {
            get;
            private set;
        }

        public bool Visible
        {
            get;
            private set;
        }
        
        //Private Variables:
        private HandInput _handInput;

        //Constructors:
        public ManagedHand(MLHandTracking.Hand hand, HandInput handInput)
        {
            _handInput = handInput;
            Hand = hand;
            Skeleton = new ManagedHandSkeleton(this);
            Gesture = new ManagedHandGesture(this);
            Collider = new ManagedHandCollider(this);
        }

        //Public Methods:
        public void Update()
        {
            //set visibility:
            if (Hand.HandConfidence > .85f && !Visible)
            {
                Visible = true;
                OnVisibilityChanged?.Invoke(this, true);
            }
            else if (Hand.HandConfidence <= .85f && Visible)
            {
                Visible = false;
                OnVisibilityChanged?.Invoke(this, false);
            }

            Skeleton.Update();
            Gesture.Update();
            Collider.Update(_handInput.palmCollisions);
        }
#endif
    }
}