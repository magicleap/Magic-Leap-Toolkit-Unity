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
    public class HandAxisVisualizer : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public MLHandTracking.HandType hand;

        //Private Variables:
        private ManagedHand _managedHand;
        private Color boneColor = Color.white;

        //Init:
        private void Awake()
        {
            HandInput.OnReady += HandleHandsReady;
        }

        //Loops:
        private void Update()
        {
            if (!HandInput.Ready)
            {
                return;
            }

            transform.SetPositionAndRotation(_managedHand.Skeleton.Position, _managedHand.Skeleton.Rotation);
        }

        //Event Handlers:
        private void HandleHandsReady()
        {
            //get skeleton:
            if (hand == MLHandTracking.HandType.Right)
            {
                _managedHand = HandInput.Right;
            }
            else
            {
                _managedHand = HandInput.Left;
            }

            _managedHand.OnVisibilityChanged += HandleHandVisibility;
        }

        private void HandleHandVisibility(ManagedHand managedHand, bool visible)
        {
            gameObject.SetActive(visible);
        }
#endif
    }
}