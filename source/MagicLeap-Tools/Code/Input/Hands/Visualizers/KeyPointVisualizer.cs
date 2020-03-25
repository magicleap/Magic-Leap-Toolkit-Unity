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
    public class KeyPointVisualizer : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Enums:
        public enum KeyPoint { None, RightHandCenter, RightWristCenter, RightThumbKnuckle, RightThumbJoint, RightThumbTip, RightIndexKnuckle, RightIndexJoint, RightIndexTip, RightMiddleKnuckle, RightMiddleJoint, RightMiddleTip, RightRingKnuckle, RightRingTip, RightPinkyKnuckle, RightPinkyTip, LeftHandCenter, LeftWristCenter, LeftThumbKnuckle, LeftThumbJoint, LeftThumbTip, LeftIndexKnuckle, LeftIndexJoint, LeftIndexTip, LeftMiddleKnuckle, LeftMiddleJoint, LeftMiddleTip, LeftRingKnuckle, LeftRingTip, LeftPinkyKnuckle, LeftPinkyTip }

        //Public Variables:
        public KeyPoint keyPoint;
        public Renderer filtered;
        public Renderer filteredBlur;
        public Renderer raw;

        //Private Variables:
        private ManagedKeypoint _status;
        private Camera _mainCamera;
        private Plane _cameraPlane;
        private Color _color;

        //Init:
        private void Awake()
        {
            //refs:
            _mainCamera = Camera.main;

            //color:
            _color = Random.ColorHSV(0, 1, .5f, 1, 1, 1);
            filtered.material.color = _color;
            raw.material.color = _color;
        }

        //Loops:
        private void Update()
        {
            if (!HandInput.Ready)
            {
                return;
            }

            //get:
            switch (keyPoint)
            {
                case KeyPoint.RightHandCenter:
                    _status = HandInput.Right.Skeleton.HandCenter;
                    break;

                case KeyPoint.RightWristCenter:
                    _status = HandInput.Right.Skeleton.WristCenter;
                    break;

                case KeyPoint.RightThumbKnuckle:
                    _status = HandInput.Right.Skeleton.Thumb.Knuckle;
                    break;

                case KeyPoint.RightThumbJoint:
                    _status = HandInput.Right.Skeleton.Thumb.Joint;
                    break;

                case KeyPoint.RightThumbTip:
                    _status = HandInput.Right.Skeleton.Thumb.Tip;
                    break;

                case KeyPoint.RightIndexKnuckle:
                    _status = HandInput.Right.Skeleton.Index.Knuckle;
                    break;

                case KeyPoint.RightIndexJoint:
                    _status = HandInput.Right.Skeleton.Index.Joint;
                    break;

                case KeyPoint.RightIndexTip:
                    _status = HandInput.Right.Skeleton.Index.Tip;
                    break;

                case KeyPoint.RightMiddleKnuckle:
                    _status = HandInput.Right.Skeleton.Middle.Knuckle;
                    break;

                case KeyPoint.RightMiddleJoint:
                    _status = HandInput.Right.Skeleton.Middle.Joint;
                    break;

                case KeyPoint.RightMiddleTip:
                    _status = HandInput.Right.Skeleton.Middle.Tip;
                    break;

                case KeyPoint.RightRingKnuckle:
                    _status = HandInput.Right.Skeleton.Ring.Knuckle;
                    break;

                case KeyPoint.RightRingTip:
                    _status = HandInput.Right.Skeleton.Ring.Tip;
                    break;

                case KeyPoint.RightPinkyKnuckle:
                    _status = HandInput.Right.Skeleton.Pinky.Knuckle;
                    break;

                case KeyPoint.RightPinkyTip:
                    _status = HandInput.Right.Skeleton.Pinky.Tip;
                    break;

                case KeyPoint.LeftHandCenter:
                    _status = HandInput.Left.Skeleton.HandCenter;
                    break;

                case KeyPoint.LeftWristCenter:
                    _status = HandInput.Left.Skeleton.WristCenter;
                    break;

                case KeyPoint.LeftThumbKnuckle:
                    _status = HandInput.Left.Skeleton.Thumb.Knuckle;
                    break;

                case KeyPoint.LeftThumbJoint:
                    _status = HandInput.Left.Skeleton.Thumb.Joint;
                    break;

                case KeyPoint.LeftThumbTip:
                    _status = HandInput.Left.Skeleton.Thumb.Tip;
                    break;

                case KeyPoint.LeftIndexKnuckle:
                    _status = HandInput.Left.Skeleton.Index.Knuckle;
                    break;

                case KeyPoint.LeftIndexJoint:
                    _status = HandInput.Left.Skeleton.Index.Joint;
                    break;

                case KeyPoint.LeftIndexTip:
                    _status = HandInput.Left.Skeleton.Index.Tip;
                    break;

                case KeyPoint.LeftMiddleKnuckle:
                    _status = HandInput.Left.Skeleton.Middle.Knuckle;
                    break;

                case KeyPoint.LeftMiddleJoint:
                    _status = HandInput.Left.Skeleton.Middle.Joint;
                    break;

                case KeyPoint.LeftMiddleTip:
                    _status = HandInput.Left.Skeleton.Middle.Tip;
                    break;

                case KeyPoint.LeftRingKnuckle:
                    _status = HandInput.Left.Skeleton.Ring.Knuckle;
                    break;

                case KeyPoint.LeftRingTip:
                    _status = HandInput.Left.Skeleton.Ring.Tip;
                    break;

                case KeyPoint.LeftPinkyKnuckle:
                    _status = HandInput.Left.Skeleton.Pinky.Knuckle;
                    break;

                case KeyPoint.LeftPinkyTip:
                    _status = HandInput.Left.Skeleton.Pinky.Tip;
                    break;
            }

            //status:
            if (_status != null)
            {
                //status:
                filtered.enabled = _status.Visible;
                raw.enabled = _status.Visible;
                filteredBlur.enabled = _status.Visible;

                //location:
                filtered.transform.position = _status.positionFiltered;
                filteredBlur.transform.position = _status.positionFiltered;
                raw.transform.position = _status.positionRaw;

                //color if inside clip plane:
                if (_status.Visible)
                {
                    if (_status.InsideClipPlane)
                    {
                        raw.enabled = false;
                        filtered.enabled = false;
                        filteredBlur.enabled = true;
                    }
                    else
                    {
                        raw.enabled = true;
                        filtered.enabled = true;
                        filteredBlur.enabled = false;
                    }
                }
            }
        }
#endif
    }
}