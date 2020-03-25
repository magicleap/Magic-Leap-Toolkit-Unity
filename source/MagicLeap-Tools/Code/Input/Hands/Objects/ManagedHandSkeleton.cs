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

namespace MagicLeapTools
{
    public class ManagedHandSkeleton
    {
#if PLATFORM_LUMIN
        //Public Properties:
        public bool InsideClipPlane
        {
            get;
            private set;
        }

        public Quaternion Rotation
        {
            get;
            private set;
        }

        public Vector3 Position
        {
            get;
            private set;
        }

        public ManagedFinger Thumb
        {
            get;
            private set;
        }

        public ManagedFinger Index
        {
            get;
            private set;
        }

        public ManagedFinger Middle
        {
            get;
            private set;
        }

        public ManagedFinger Ring
        {
            get;
            private set;
        }

        public ManagedFinger Pinky
        {
            get;
            private set;
        }

        /// <summary>
        /// The one supplied by the SDK which rides along the surface of the skin.
        /// </summary>
        public ManagedKeypoint HandCenter
        {
            get;
            private set;
        }

        public ManagedKeypoint WristCenter
        {
            get;
            private set;
        }

        public ManagedFinger[] Fingers
        {
            get;
            private set;
        }

        //Private Variables:
        private ManagedHand _managedHand;
        private Camera _mainCamera;
        private List<Vector3> _rotationOffsets;
        private ManagedKeypoint _thumbMCP = new ManagedKeypoint();
        private ManagedKeypoint _thumbPIP = new ManagedKeypoint();
        private ManagedKeypoint _thumbTip = new ManagedKeypoint();
        private ManagedKeypoint _indexMCP = new ManagedKeypoint();
        private ManagedKeypoint _indexPIP = new ManagedKeypoint();
        private ManagedKeypoint _indexTip = new ManagedKeypoint();
        private ManagedKeypoint _middleMCP = new ManagedKeypoint();
        private ManagedKeypoint _middlePIP = new ManagedKeypoint();
        private ManagedKeypoint _middleTip = new ManagedKeypoint();
        private ManagedKeypoint _ringMCP = new ManagedKeypoint();
        private ManagedKeypoint _ringTip = new ManagedKeypoint();
        private ManagedKeypoint _pinkyMCP = new ManagedKeypoint();
        private ManagedKeypoint _pinkyTip = new ManagedKeypoint();

        //Constructors:
        public ManagedHandSkeleton(ManagedHand managedHand)
        {
            //refs:
            _mainCamera = Camera.main;

            //sets:
            _managedHand = managedHand;

            //establish rotation offsets:
            _rotationOffsets = new List<Vector3>();
            _rotationOffsets.Add(Vector3.zero);
            _rotationOffsets.Add(new Vector3(-0.3004f, -0.9466f, 0.1174f));
            _rotationOffsets.Add(new Vector3(-0.8816f, 0.3225f, 0.3447f));

            //keypoints:
            HandCenter = new ManagedKeypoint();
            WristCenter = new ManagedKeypoint();

            //fingers:
            Thumb = new ManagedFinger(_managedHand.Hand,FingerType.Thumb, _thumbMCP, _thumbPIP, _thumbTip);
            Index = new ManagedFinger(_managedHand.Hand, FingerType.Index, _indexMCP, _indexPIP, _indexTip);
            Middle = new ManagedFinger(_managedHand.Hand, FingerType.Middle, _middleMCP, _middlePIP, _middleTip);
            Ring = new ManagedFinger(_managedHand.Hand, FingerType.Ring, _ringMCP, _ringTip);
            Pinky = new ManagedFinger(_managedHand.Hand, FingerType.Pinky, _pinkyMCP, _pinkyTip);
            Fingers = new ManagedFinger[5] { Thumb, Index, Middle, Ring, Pinky };
        }

        //Public Methods:
        public void Update()
        {
            //update keypoints and fingers:
            WristCenter.Update(_managedHand, _managedHand.Hand.Wrist.Center.Position, _managedHand.Hand.Center);
            HandCenter.Update(_managedHand, _managedHand.Hand.Center);
            _thumbMCP.Update(_managedHand, _managedHand.Hand.Thumb.MCP.Position, _managedHand.Hand.Index.MCP.Position, _managedHand.Hand.Center);
            _thumbPIP.Update(_managedHand, _managedHand.Hand.Thumb.IP.Position, _managedHand.Hand.Thumb.MCP.Position, _managedHand.Hand.Center);
            _thumbTip.Update(_managedHand, _managedHand.Hand.Thumb.Tip.Position, _managedHand.Hand.Thumb.IP.Position, _managedHand.Hand.Middle.Tip.Position, _managedHand.Hand.Thumb.MCP.Position, _managedHand.Hand.Center);
            Thumb.Update();
            _indexMCP.Update(_managedHand, _managedHand.Hand.Index.MCP.Position, _managedHand.Hand.Center);
            _indexPIP.Update(_managedHand, _managedHand.Hand.Index.PIP.Position, _managedHand.Hand.Index.MCP.Position, _managedHand.Hand.Center);
            _indexTip.Update(_managedHand, _managedHand.Hand.Index.Tip.Position, _managedHand.Hand.Index.PIP.Position, _managedHand.Hand.Index.MCP.Position, _managedHand.Hand.Center, _managedHand.Hand.Thumb.IP.Position, _managedHand.Hand.Middle.Tip.Position);
            Index.Update();
            _middleMCP.Update(_managedHand, _managedHand.Hand.Middle.MCP.Position, _managedHand.Hand.Center);
            _middlePIP.Update(_managedHand, _managedHand.Hand.Middle.PIP.Position, _managedHand.Hand.Middle.MCP.Position, _managedHand.Hand.Center);
            _middleTip.Update(_managedHand, _managedHand.Hand.Middle.Tip.Position, _managedHand.Hand.Middle.PIP.Position, _managedHand.Hand.Middle.MCP.Position, _managedHand.Hand.Ring.Tip.Position, _managedHand.Hand.Center);
            Middle.Update();
            _ringMCP.Update(_managedHand, _managedHand.Hand.Ring.MCP.Position, _managedHand.Hand.Center);
            _ringTip.Update(_managedHand, _managedHand.Hand.Ring.Tip.Position, _managedHand.Hand.Ring.MCP.Position, _managedHand.Hand.Pinky.Tip.Position, _managedHand.Hand.Middle.Tip.Position, _managedHand.Hand.Center);
            Ring.Update();
            _pinkyMCP.Update(_managedHand, _managedHand.Hand.Pinky.MCP.Position, _managedHand.Hand.Center);
            _pinkyTip.Update(_managedHand, _managedHand.Hand.Pinky.Tip.Position, _managedHand.Hand.Pinky.MCP.Position, _managedHand.Hand.Ring.Tip.Position, _managedHand.Hand.Center);
            Pinky.Update();

            //we need a hand to continue:
            if (!_managedHand.Visible)
            {
                return;
            }

            //correct distances:
            float thumbMcpToWristDistance = Vector3.Distance(_thumbMCP.positionFiltered, WristCenter.positionFiltered) * .5f;
            //fix the distance between the wrist and thumbMcp as it incorrectly expands as the hand gets further from the camera:
            float distancePercentage = Mathf.Clamp01(Vector3.Distance(_mainCamera.transform.position, WristCenter.positionFiltered) / .5f);
            distancePercentage = 1 - Percentage(distancePercentage, .90f, 1) * .4f;
            thumbMcpToWristDistance *= distancePercentage;
            Vector3 wristToPalmDirection = Vector3.Normalize(Vector3.Normalize(HandCenter.positionFiltered - WristCenter.positionFiltered));
            Vector3 center = WristCenter.positionFiltered + (wristToPalmDirection * thumbMcpToWristDistance);
            Vector3 camToWristDirection = Vector3.Normalize(WristCenter.positionFiltered - _mainCamera.transform.position);

            //rays needed for planarity discovery for in/out palm facing direction:
            Vector3 camToWrist = new Ray(WristCenter.positionFiltered, camToWristDirection).GetPoint(1);
            Vector3 camToThumbMcp = new Ray(_thumbMCP.positionFiltered, Vector3.Normalize(_thumbMCP.positionFiltered - _mainCamera.transform.position)).GetPoint(1);
            Vector3 camToPalm = new Ray(center, Vector3.Normalize(center - _mainCamera.transform.position)).GetPoint(1);

            //discover palm facing direction to camera:
            Plane palmFacingPlane = new Plane(camToWrist, camToPalm, camToThumbMcp);
            if (_managedHand.Hand.Type == MLHandTracking.HandType.Left)
            {
                palmFacingPlane.Flip();
            }
            float palmForwardFacing = Mathf.Sign(Vector3.Dot(palmFacingPlane.normal, _mainCamera.transform.forward));

            //use thumb/palm/wrist alignment to determine amount of roll in the hand:
            Vector3 toThumbMcp = Vector3.Normalize(_thumbMCP.positionFiltered - center);
            Vector3 toPalm = Vector3.Normalize(center - WristCenter.positionFiltered);
            float handRollAmount = (1 - Vector3.Dot(toThumbMcp, toPalm)) * palmForwardFacing;

            //where between the wrist and thumbMcp should we slide inwards to get the palm in the center:
            Vector3 toPalmOrigin = Vector3.Lerp(WristCenter.positionFiltered, _thumbMCP.positionFiltered, .35f);

            //get a direction from the camera to toPalmOrigin as psuedo up for use in quaternion construction:
            Vector3 toCam = Vector3.Normalize(_mainCamera.transform.position - toPalmOrigin);

            //construct a quaternion that helps get angles needed between the wrist and thumbMCP to point towards the palm center:
            Vector3 wristToThumbMcp = Vector3.Normalize(_thumbMCP.positionFiltered - WristCenter.positionFiltered);
            Quaternion towardsCamUpReference = Quaternion.identity;
            if (wristToThumbMcp != Vector3.zero && toCam != Vector3.zero)
            {
                towardsCamUpReference = Quaternion.LookRotation(wristToThumbMcp, toCam);
            }

            //rotate the inwards vector depending on hand roll to know where to push the palm back:
            float inwardsVectorRotation = 90;
            if (_managedHand.Hand.Type == MLHandTracking.HandType.Left)
            {
                inwardsVectorRotation = -90;
            }
            towardsCamUpReference = Quaternion.AngleAxis(handRollAmount * inwardsVectorRotation, towardsCamUpReference * Vector3.forward) * towardsCamUpReference;
            Vector3 inwardsVector = towardsCamUpReference * Vector3.up;

            //slide palm location along inwards vector to get it into proper physical location in the center of the hand:
            center = toPalmOrigin - inwardsVector * thumbMcpToWristDistance;
            Vector3 deadCenter = center;

            //as the hand flattens back out balance corrected location with originally provided location for better forward origin:
            center = Vector3.Lerp(center, HandCenter.positionFiltered, Mathf.Abs(handRollAmount));

            //get a forward using the corrected palm location:
            Vector3 forward = Vector3.Normalize(center - WristCenter.positionFiltered);

            //switch back to physical center of hand - this reduces surface-to-surface movement of the center between back and palm:
            center = deadCenter;

            //get an initial hand up:
            Plane handPlane = new Plane(WristCenter.positionFiltered, _thumbMCP.positionFiltered, center);
            if (_managedHand.Hand.Type == MLHandTracking.HandType.Left)
            {
                handPlane.Flip();
            }
            Vector3 up = handPlane.normal;

            //find out how much the back of the hand is facing the camera so we have a safe set of features for a stronger forward: 
            Vector3 centerToCam = Vector3.Normalize(_mainCamera.transform.position - WristCenter.positionFiltered);
            float facingDot = Vector3.Dot(centerToCam, up);

            if (facingDot > .5f)
            {
                float handBackFacingCamAmount = Percentage(facingDot, .5f, 1);

                //steer forward for more accuracy based on the visibility of the back of the hand:
                if (_middleMCP.Visible)
                {
                    Vector3 toMiddleMcp = Vector3.Normalize(_middleMCP.positionFiltered - center);
                    forward = Vector3.Lerp(forward, toMiddleMcp, handBackFacingCamAmount);
                }
                else if (_indexMCP.Visible)
                {
                    Vector3 inIndexMcp = Vector3.Normalize(_indexMCP.positionFiltered - center);
                    forward = Vector3.Lerp(forward, inIndexMcp, handBackFacingCamAmount);
                }
            }

            //make sure palm distance from wrist is consistant while also leveraging steered forward:
            center = WristCenter.positionFiltered + (forward * thumbMcpToWristDistance);

            //an initial rotation of the hand:
            Quaternion orientation = Quaternion.identity;
            if (forward != Vector3.zero && up != Vector3.zero)
            {
                orientation = Quaternion.LookRotation(forward, up);
            }

            //as the hand rolls counter-clockwise the thumbMcp loses accuracy so we need to interpolate to the back of the hand's features:
            if (_indexMCP.Visible && _middleMCP.Visible)
            {
                Vector3 knucklesVector = Vector3.Normalize(_middleMCP.positionFiltered - _indexMCP.positionFiltered);
                float knucklesDot = Vector3.Dot(knucklesVector, Vector3.up);
                if (knucklesDot > .5f)
                {
                    float counterClockwiseRoll = Percentage(Vector3.Dot(knucklesVector, Vector3.up), .35f, .7f);
                    center = Vector3.Lerp(center, HandCenter.positionFiltered, counterClockwiseRoll);
                    forward = Vector3.Lerp(forward, Vector3.Normalize(_middleMCP.positionFiltered - HandCenter.positionFiltered), counterClockwiseRoll);
                    Plane backHandPlane = new Plane(HandCenter.positionFiltered, _indexMCP.positionFiltered, _middleMCP.positionFiltered);
                    if (_managedHand.Hand.Type == MLHandTracking.HandType.Left)
                    {
                        backHandPlane.Flip();
                    }
                    up = Vector3.Lerp(up, backHandPlane.normal, counterClockwiseRoll);
                    orientation = Quaternion.LookRotation(forward, up);
                }
            }

            //as the wrist tilts away from the camera (with the thumb down) at extreme angles the hand center will move toward the thumb:
            float handTiltAwayAmount = 1 - Percentage(Vector3.Distance(HandCenter.positionFiltered, WristCenter.positionFiltered), .025f, .04f);
            Vector3 handTiltAwayCorrectionPoint = WristCenter.positionFiltered + camToWristDirection * thumbMcpToWristDistance;
            center = Vector3.Lerp(center, handTiltAwayCorrectionPoint, handTiltAwayAmount);
            forward = Vector3.Lerp(forward, Vector3.Normalize(handTiltAwayCorrectionPoint - WristCenter.positionFiltered), handTiltAwayAmount);
            Plane wristPlane = new Plane(WristCenter.positionFiltered, _thumbMCP.positionFiltered, center);
            if (_managedHand.Hand.Type == MLHandTracking.HandType.Left)
            {
                wristPlane.Flip();
            }
            up = Vector3.Lerp(up, wristPlane.normal, handTiltAwayAmount);
            if (forward != Vector3.zero && up != Vector3.zero)
            {
                orientation = Quaternion.LookRotation(forward, up);
            }

            //steering for if thumb/index are not available from self-occlusion to help rotate the hand better outwards better:
            float forwardUpAmount = Vector3.Dot(forward, Vector3.up);
            if (forwardUpAmount > .7f && _indexMCP.Visible && _ringMCP.Visible)
            {
                float angle = 0;
                if (_managedHand.Hand.Type == MLHandTracking.HandType.Right)
                {
                    Vector3 knucklesVector = Vector3.Normalize(_ringMCP.positionFiltered - _indexMCP.positionFiltered);
                    angle = Vector3.Angle(knucklesVector, orientation * Vector3.right);
                    angle *= -1;
                }
                else
                {
                    Vector3 knucklesVector = Vector3.Normalize(_indexMCP.positionFiltered - _ringMCP.positionFiltered);
                    angle = Vector3.Angle(knucklesVector, orientation * Vector3.right);
                }
                Quaternion selfOcclusionSteering = Quaternion.AngleAxis(angle, forward);
                orientation = selfOcclusionSteering * orientation;
            }
            else
            {
                //when palm is facing down we need to rotate some to compensate for an offset:
                float rollCorrection = Mathf.Clamp01(Vector3.Dot(orientation * Vector3.up, Vector3.up));
                float rollCorrectionAmount = -30;
                if (_managedHand.Hand.Type == MLHandTracking.HandType.Left)
                {
                    rollCorrectionAmount = 30;
                }
                orientation = Quaternion.AngleAxis(rollCorrectionAmount * rollCorrection, forward) * orientation;
            }

            //inside the camera plane:
            InsideClipPlane = TransformUtilities.InsideClipPlane(center);

            //set pose:
            Position = center;
            Rotation = orientation;

            UpdateKeypointRotations();
        }

        //Private Methods:
        private float Percentage(float value, float minimum, float maximum)
        {
            value -= minimum;
            value = Mathf.Max(0, value);
            return Mathf.Clamp01(value / (maximum - minimum));
        }

        public ManagedKeypoint GetKeypoint(FingerType fingerType, KeypointType keypointType)
        {
            if (keypointType == KeypointType.HandCenter)
            {
                return HandCenter;
            }
            if (keypointType == KeypointType.Wrist)
            {
                return WristCenter;
            }

            return GetFinger(fingerType).GetKeypoint(keypointType);
        }

        public ManagedFinger GetFinger(FingerType fingerType)
        {
            ManagedFinger finger = null;
            switch (fingerType)
            {
                case FingerType.Thumb:
                    finger = Thumb;
                    break;
                case FingerType.Index:
                    finger = Index;
                    break;
                case FingerType.Middle:
                    finger = Middle;
                    break;
                case FingerType.Ring:
                    finger = Ring;
                    break;
                case FingerType.Pinky:
                    finger = Pinky;
                    break;
            }

            return finger;
        }


        void UpdateKeypointRotations()
        {
            Vector3 up;
            Vector3 forward;
            int sign = _managedHand.Hand.Type == MLHandTracking.HandType.Left ? 1 : -1;

            for (int f = 0; f < Fingers.Length; f++)
            {
                for(int i = 0; i < Fingers[f].points.Length-1; i++)
                {
                    forward = Fingers[f].points[i+1].positionFiltered - Fingers[f].points[i].positionFiltered;

                    if (f == 0)
                    {
                        up = _managedHand.Hand.Type == MLHandTracking.HandType.Left ? Rotation * Vector3.right : -(Rotation * Vector3.right);
                    }
                    else
                    {
                        up = sign * Vector3.Cross(forward, Rotation*Vector3.right);
                    }

                    if(forward != Vector3.zero)
                    {
                        Fingers[f].points[i].Rotation = Quaternion.LookRotation(forward, up);
                    }
                }

                int last = Fingers[f].points.Length - 1;
                Fingers[f].points[last].Rotation = Fingers[f].points[last-1].Rotation;
            }
        }
#endif
    }
}