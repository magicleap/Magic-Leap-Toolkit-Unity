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
    public class HandSkeletonVisualizer : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public MLHandTracking.HandType hand;
        public Color boneColor;

        //Private Variables:
        private ManagedHand _managedHand;
        private AxisVisualizer _axisVisualizer;
        private Color _insideClipColor = new Color(.3f, .3f, .3f, 1);

        //Init:
        private void Reset()
        {
            boneColor = new Color(0, 0.6784f, 1, 1);
        }

        private void Awake()
        {
            //hooks:
            HandInput.OnReady += HandleHandsReady;

            //refs:
            _axisVisualizer = GetComponent<AxisVisualizer>();
        }

        //Flow:
        private void OnEnable()
        {
            if (_managedHand == null)
            {
                return;
            }
            HandleHandVisibility(_managedHand, gameObject.activeSelf);
        }

        private void OnDisable()
        {
            if (_managedHand == null)
            {
                return;
            }
            HandleHandVisibility(_managedHand, false);
        }

        //Loops:
        private void Update()
        {
            if (!HandInput.Ready)
            {
                return;
            }

            //place:
            transform.SetPositionAndRotation(_managedHand.Skeleton.Position, _managedHand.Skeleton.Rotation);

            //hide axis if inside clip plane:
            _axisVisualizer.enabled = !_managedHand.Skeleton.InsideClipPlane;

            //wrist:
            Lines.DrawLine(Name("wrist"), GetColor(_managedHand.Skeleton.WristCenter.InsideClipPlane), GetColor(_managedHand.Skeleton.InsideClipPlane), .0005f, _managedHand.Skeleton.WristCenter.positionFiltered, _managedHand.Skeleton.Position);

            //fingers:
            DrawDigit("thumb", _managedHand.Skeleton.Thumb, GetColor(_managedHand.Skeleton.Thumb.Knuckle.InsideClipPlane), GetColor(_managedHand.Skeleton.Thumb.Tip.InsideClipPlane));
            DrawDigit("index", _managedHand.Skeleton.Index, GetColor(_managedHand.Skeleton.Index.Knuckle.InsideClipPlane), GetColor(_managedHand.Skeleton.Index.Tip.InsideClipPlane));
            DrawDigit("middle", _managedHand.Skeleton.Middle, GetColor(_managedHand.Skeleton.Middle.Knuckle.InsideClipPlane), GetColor(_managedHand.Skeleton.Middle.Tip.InsideClipPlane));
            DrawDigit("ring", _managedHand.Skeleton.Ring, GetColor(_managedHand.Skeleton.Ring.Knuckle.InsideClipPlane), GetColor(_managedHand.Skeleton.Ring.Tip.InsideClipPlane));
            DrawDigit("pinky", _managedHand.Skeleton.Pinky, GetColor(_managedHand.Skeleton.Pinky.Knuckle.InsideClipPlane), GetColor(_managedHand.Skeleton.Pinky.Tip.InsideClipPlane));

            //finger connections:
            DrawBone("thumbConnection", _managedHand.Skeleton.Position, _managedHand.Skeleton.Thumb.Knuckle, GetColor(_managedHand.Skeleton.InsideClipPlane), GetColor(_managedHand.Skeleton.Thumb.Knuckle.InsideClipPlane));
            DrawBone("indexConnection", _managedHand.Skeleton.Position, _managedHand.Skeleton.Index.Knuckle, GetColor(_managedHand.Skeleton.InsideClipPlane), GetColor(_managedHand.Skeleton.Index.Knuckle.InsideClipPlane));
            DrawBone("middleConnection", _managedHand.Skeleton.Position, _managedHand.Skeleton.Middle.Knuckle, GetColor(_managedHand.Skeleton.InsideClipPlane), GetColor(_managedHand.Skeleton.Middle.Knuckle.InsideClipPlane));
            DrawBone("ringConnection", _managedHand.Skeleton.Position, _managedHand.Skeleton.Ring.Knuckle, GetColor(_managedHand.Skeleton.InsideClipPlane), GetColor(_managedHand.Skeleton.Ring.Knuckle.InsideClipPlane));
            DrawBone("pinkyConnection", _managedHand.Skeleton.Position, _managedHand.Skeleton.Pinky.Knuckle, GetColor(_managedHand.Skeleton.InsideClipPlane), GetColor(_managedHand.Skeleton.Pinky.Knuckle.InsideClipPlane));
        }

        //Event Handlers:
        private void HandleFingerVisibilityChanged(ManagedFinger finger, bool visible)
        {
            Lines.SetVisibility(Name(finger), visible);
        }


        private void HandleHandVisibility(ManagedHand managedHand, bool visible)
        {
            HandleFingerVisibilityChanged(managedHand.Skeleton.Thumb, visible);
            HandleFingerVisibilityChanged(managedHand.Skeleton.Index, visible);
            HandleFingerVisibilityChanged(managedHand.Skeleton.Middle, visible);
            HandleFingerVisibilityChanged(managedHand.Skeleton.Ring, visible);
            HandleFingerVisibilityChanged(managedHand.Skeleton.Pinky, visible);
            Lines.SetVisibility(Name("thumbConnection"), visible);
            Lines.SetVisibility(Name("indexConnection"), visible);
            Lines.SetVisibility(Name("middleConnection"), visible);
            Lines.SetVisibility(Name("ringConnection"), visible);
            Lines.SetVisibility(Name("pinkyConnection"), visible);
            gameObject.SetActive(visible);
            Lines.SetVisibility(Name("wrist"), visible);
        }

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

            //hooks:
            _managedHand.OnVisibilityChanged += HandleHandVisibility;
            _managedHand.Skeleton.Thumb.OnVisibilityChanged += HandleFingerVisibilityChanged;
            _managedHand.Skeleton.Index.OnVisibilityChanged += HandleFingerVisibilityChanged;
            _managedHand.Skeleton.Middle.OnVisibilityChanged += HandleFingerVisibilityChanged;
            _managedHand.Skeleton.Ring.OnVisibilityChanged += HandleFingerVisibilityChanged;
            _managedHand.Skeleton.Pinky.OnVisibilityChanged += HandleFingerVisibilityChanged;

            HandleHandVisibility(_managedHand, gameObject.activeSelf);
        }

        //Private Methods:
        private Color GetColor(bool insideClipPlane)
        {
            return insideClipPlane ? _insideClipColor : boneColor;
        }

        private void DrawBone(string fingerName, Vector3 from, ManagedKeypoint to, Color colorA, Color colorB)
        {
            fingerName = Name(fingerName);
            Lines.DrawLine(fingerName, colorA, colorB, .0005f, from, to.positionFiltered);

            if (_managedHand.Visible && to.Visible)
            {
                Lines.SetVisibility(fingerName, true);
            }
            else
            {
                Lines.SetVisibility(fingerName, false);
            }
        }

        private void DrawDigit(string fingerName, ManagedFinger finger, Color colorA, Color colorB)
        {
            fingerName = Name(fingerName);
            Lines.DrawLine(fingerName, colorA, colorB, .0005f, finger.PointLocationsFiltered);

            if (_managedHand.Visible && finger.IsVisible)
            {
                Lines.SetVisibility(fingerName, true);
            }
            else
            {
                Lines.SetVisibility(fingerName, false);
            }
        }

        private string Name(ManagedFinger finger)
        {
            switch (finger.FingerType)
            {
                case FingerType.Thumb:
                    return Name("thumb");
                case FingerType.Index:
                    return Name("index");
                case FingerType.Middle:
                    return Name("middle");
                case FingerType.Ring:
                    return Name("ring");
                case FingerType.Pinky:
                    return Name("pinky");
            }
            return "";
        }

        private string Name(string part)
        {
            return $"{part}_{hand}";
        }
#endif
    }
}