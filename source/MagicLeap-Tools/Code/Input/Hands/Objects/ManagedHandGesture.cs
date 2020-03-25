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
    //Enums:
    public enum IntentPose { Relaxed, Pinching, Grasping, Pointing }

    [System.Serializable]
    public class ManagedHandGesture
    {
    #if PLATFORM_LUMIN
        //Events:
        /// <summary>
        /// A verified keypose change that will not always match the KeyPose of the hand.  This is useful for maintaining the action of the user's hand and will strive to maintain grasping poses regardless of variations.
        /// </summary>
        public event Action<ManagedHand, MLHandTracking.HandKeyPose> OnVerifiedGestureChanged;
        /// <summary>
        /// A change in intent of the hand.  This buckets a few keyposes together so an overall intent can be identified.
        /// </summary>
        public event Action<ManagedHand, IntentPose> OnIntentChanged;
        /// <summary>
        /// The raw keypose change.
        /// </summary>
        public event Action<ManagedHand, MLHandTracking.HandKeyPose> OnKeyPoseChanged;
        /// <summary>
        /// Event when palm is facing forwards (away from head).
        /// </summary>
        public event Action OnPalmFront;
        /// <summary>
        /// Event when palm is facing backwards (towards head).
        /// </summary>
        public event Action OnPalmBack;

        //Public Properties:
        public IntentPose Intent
        {
            get;
            private set;
        }

        public IntentPose LastIntent
        {
            get;
            private set;
        }

        public InteractionPoint Pinch
        {
            get;
            private set;
        }

        public InteractionPoint Grasp
        {
            get;
            private set;
        }

        public InteractionPoint Point
        {
            get;
            private set;
        }

        public InteractionPoint Current
        {
            get;
            private set;
        }
    
        public bool PalmFacingForward
        {
            get;
            private set;
        }

        public bool PalmFacingBack
        {
            get;
            private set;
        }

        /// <summary>
        /// A verified keypose that will not always match the KeyPose of the hand.  This is useful for maintaining the action of the user's hand and will strive to maintain grasping poses regardless of variations.
        /// </summary>
        public MLHandTracking.HandKeyPose VerifiedGesture
        {
            get;
            private set;
        }

        //Private Methods:
        private ManagedHand _managedHand;
        private MLHandTracking.HandKeyPose _proposedKeyPose = MLHandTracking.HandKeyPose.NoPose;
        private float _keyPoseChangedTime;
        private float _keyPoseStabailityDuration = .08f;
        private bool _collapsed;
        private float _dynamicReleaseDistance = 0.00762f;
        private Pose _interactionPointOffset;
        private InteractionState _currentInteractionState;
        private Vector3 _pinchAbsolutePositionOffset = new Vector3(-0.03f, -0.1f, 0.04f);
        private Vector3 _pinchAbsoluteRotationOffset = new Vector3(57.2f, -44.6f, -7.9f);
        private float _pinchRelativePositionDistance = 0.0889f;
        private Vector3 _pinchRelativeRotationOffset = new Vector3(57.2f, 0, -7.9f);
        private bool _pinchIsRelative;
        private bool _pinchTransitioning;
        private float _pinchTransitionStartTime;
        private float _pinchTransitionMaxDuration = .5f;
        private Vector3 _pinchArrivalPositionVelocity;
        private Quaternion _pinchArrivalRotationVelocity;
        private float _pinchTransitionTime = .1f;
        private float _maxGraspRadius = 0.1143f;
        private Vector3 _handToHead;
        private Transform _headpose;
        private float _palmDotValidThreshold = 0.7f;
        private float _palmDotInValidThreshold = 0.5f;
        private bool _lastPalmForward;
        private bool _lastPalmBack;

        //Constructors:
        public ManagedHandGesture(ManagedHand managedHand)
        {
            LastIntent = IntentPose.Relaxed;
            Intent = IntentPose.Relaxed;
        
            _headpose = Camera.main.transform;
            //sets:
            _managedHand = managedHand;
            Pinch = new InteractionPoint(_managedHand);
            Grasp = new InteractionPoint(_managedHand);
            Point = new InteractionPoint(_managedHand);
            Current = new InteractionPoint(_managedHand);
        
            _managedHand.Hand.OnHandKeyPoseBegin += HandleKeyposeChanged;
        }

        //Public Methods:
        public void Update()
        {
            if (!_managedHand.Visible)
            {
                return;
            }

            //pinch rotation offset mirror:
            Vector3 rotationOffset = _pinchAbsoluteRotationOffset;
            if (_managedHand.Hand.Type == MLHandTracking.HandType.Left)
            {
                rotationOffset.y *= -1;
            }

            //holders:
            Vector3 pinchPosition = Vector3.zero;
            Quaternion pinchRotation = Quaternion.identity;

            //pinch interaction point radius:
            if (_managedHand.Skeleton.Thumb.Tip.Visible && _managedHand.Skeleton.Index.Tip.Visible)
            {
                Pinch.radius = Vector3.Distance(_managedHand.Skeleton.Thumb.Tip.positionFiltered, _managedHand.Skeleton.Index.Tip.positionFiltered);
            }

            if (_managedHand.Skeleton.Thumb.Tip.Visible) //absolute placement:
            {
                //are we swapping modes?
                if (_pinchIsRelative)
                {
                    _pinchIsRelative = false;
                    _pinchTransitioning = true;
                    _pinchTransitionStartTime = Time.realtimeSinceStartup;
                }

                pinchPosition = _managedHand.Skeleton.Thumb.Tip.positionFiltered;
                pinchRotation = TransformUtilities.RotateQuaternion(_managedHand.Skeleton.Rotation, rotationOffset);

                //gather offset distance:
                if (_managedHand.Skeleton.Index.Knuckle.Visible && _managedHand.Skeleton.Thumb.Knuckle.Visible)
                {
                    Vector3 mcpMidpoint = Vector3.Lerp(_managedHand.Skeleton.Index.Knuckle.positionFiltered, _managedHand.Skeleton.Thumb.Knuckle.positionFiltered, .5f);
                    _pinchRelativePositionDistance = Vector3.Distance(mcpMidpoint, pinchPosition);
                }
            }
            else //relative placement:
            {
                //are we swapping modes?
                if (!_pinchIsRelative)
                {
                    _pinchIsRelative = true;
                    _pinchTransitioning = true;
                    _pinchTransitionStartTime = Time.realtimeSinceStartup;
                }

                //place between available mcps:
                if (_managedHand.Skeleton.Index.Knuckle.Visible && _managedHand.Skeleton.Thumb.Knuckle.Visible)
                {
                    pinchPosition = Vector3.Lerp(_managedHand.Skeleton.Index.Knuckle.positionFiltered, _managedHand.Skeleton.Thumb.Knuckle.positionFiltered, .5f);

                    //rotate:
                    pinchRotation = TransformUtilities.RotateQuaternion(_managedHand.Skeleton.Rotation, _pinchRelativeRotationOffset);

                    //move out along rotation forward:
                    pinchPosition += pinchRotation * Vector3.forward * _pinchRelativePositionDistance;
                }
                else
                {
                    //just use previous:
                    pinchPosition = Pinch.position;
                    pinchRotation = Pinch.rotation;
                }
            }

            //sticky release reduction:
            if (_collapsed)
            {
                if (_managedHand.Skeleton.Thumb.Tip.Visible && _managedHand.Skeleton.Index.Tip.Visible)
                {
                    //if starting to release, start using a point between the thumb and index tips:
                    if (Vector3.Distance(_managedHand.Skeleton.Thumb.Tip.positionFiltered, _managedHand.Skeleton.Index.Tip.positionFiltered) > _dynamicReleaseDistance)
                    {
                        pinchPosition = Vector3.Lerp(_managedHand.Skeleton.Thumb.Tip.positionFiltered, _managedHand.Skeleton.Index.Tip.positionFiltered, .3f);
                    }
                }
            }

            //apply pinch pose - to avoid jumps when relative placement is used we smooth until close enough:
            if (_pinchTransitioning)
            {
                //position:
                Pinch.position = Vector3.SmoothDamp(Pinch.position, pinchPosition, ref _pinchArrivalPositionVelocity, _pinchTransitionTime);
                float positionDelta = Vector3.Distance(Pinch.position, pinchPosition);

                //rotation:
                Pinch.rotation = MotionUtilities.SmoothDamp(Pinch.rotation, pinchRotation, ref _pinchArrivalRotationVelocity, _pinchTransitionTime);
                float rotationDelta = Quaternion.Angle(Pinch.rotation, pinchRotation);

                //close enough to hand off?
                if (positionDelta < .001f && rotationDelta < 5)
                {
                    _pinchTransitioning = false;
                }

                //taking too long?
                if (Time.realtimeSinceStartup - _pinchTransitionStartTime > _pinchTransitionMaxDuration)
                {
                    _pinchTransitioning = false;
                }
            }
            else
            {
                Pinch.position = pinchPosition;
                Pinch.rotation = pinchRotation;
            }

            //grasp interaction point:
            Bounds graspBounds = CalculateGraspBounds
                (
                _managedHand.Skeleton.Thumb.Knuckle,
                _managedHand.Skeleton.Thumb.Joint,
                _managedHand.Skeleton.Thumb.Tip,
                _managedHand.Skeleton.Index.Knuckle,
                _managedHand.Skeleton.Index.Joint,
                _managedHand.Skeleton.Index.Tip,
                _managedHand.Skeleton.Middle.Knuckle,
                _managedHand.Skeleton.Middle.Joint,
                _managedHand.Skeleton.Middle.Tip
                );
            Grasp.position = _managedHand.Skeleton.Position;
            //when points are being initially found they can be wildly off and this could cause a massively large volume:
            Grasp.radius = Mathf.Min(graspBounds.size.magnitude, _maxGraspRadius); 
            Grasp.rotation = _managedHand.Skeleton.Rotation;

            //intent updated:
            if (_currentInteractionState != null)
            {
                _currentInteractionState.FireUpdate();
            }

            //keypose change proposed:
            if (_managedHand.Hand.KeyPose != VerifiedGesture && _managedHand.Hand.KeyPose != _proposedKeyPose)
            {
                //queue a new proposed change to keypose:
                _proposedKeyPose = _managedHand.Hand.KeyPose;
                _keyPoseChangedTime = Time.realtimeSinceStartup;
            }
        
            //keypose change acceptance:
            if (_managedHand.Hand.KeyPose != VerifiedGesture && Time.realtimeSinceStartup - _keyPoseChangedTime > _keyPoseStabailityDuration)
            {
                //reset:
                Point.active = false;
                Pinch.active = false;
                Grasp.active = false;
            
                if (_collapsed)
                {
                    //intent end:
                    if (_managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.C || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.OpenHand || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.L || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Finger)
                    {
                        if (_managedHand.Skeleton.Thumb.Tip.Visible && _managedHand.Skeleton.Index.Tip.Visible)
                        {
                            //dynamic release:
                            if (Vector3.Distance(_managedHand.Skeleton.Thumb.Tip.positionFiltered, _managedHand.Skeleton.Index.Tip.positionFiltered) > _dynamicReleaseDistance)
                            {
                                //end intent:
                                _collapsed = false;
                                _currentInteractionState.FireEnd();
                                _currentInteractionState = null;

                                //accept keypose change:
                                VerifiedGesture = _managedHand.Hand.KeyPose;
                                _proposedKeyPose = _managedHand.Hand.KeyPose;
                                OnVerifiedGestureChanged?.Invoke(_managedHand, VerifiedGesture);

                                if (_managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Finger || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.L)
                                {
                                    Intent = IntentPose.Pointing;
                                    OnIntentChanged?.Invoke(_managedHand, Intent);
                                }
                                else if (_managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.C || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.OpenHand || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Thumb)
                                {
                                    Intent = IntentPose.Relaxed;
                                    OnIntentChanged?.Invoke(_managedHand, Intent);
                                }
                            }
                        }
                    }
                }
                else
                {
                    //intent begin:
                    if (_managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Pinch || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Ok || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Fist)
                    {
                        _collapsed = true;
                    
                        if (_managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Pinch || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Ok)
                        {
                            Intent = IntentPose.Pinching;
                            Pinch.active = true;
                            _currentInteractionState = Pinch.Touch;
                            _currentInteractionState.FireBegin();
                            OnIntentChanged?.Invoke(_managedHand, Intent);
                        }
                        else if (_managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Fist)
                        {
                            Intent = IntentPose.Grasping;
                            Grasp.active = true;
                            _currentInteractionState = Grasp.Touch;
                            _currentInteractionState.FireBegin();
                            OnIntentChanged?.Invoke(_managedHand, Intent);
                        }
                    }

                    if (_managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Finger || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.L)
                    {
                        Intent = IntentPose.Pointing;
                        Point.active = true;
                        OnIntentChanged?.Invoke(_managedHand, Intent);
                    }
                    else if (_managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.C || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.OpenHand || _managedHand.Hand.KeyPose == MLHandTracking.HandKeyPose.Thumb)
                    {
                        Intent = IntentPose.Relaxed;
                        OnIntentChanged?.Invoke(_managedHand, Intent);
                    }

                    //accept keypose change:
                    VerifiedGesture = _managedHand.Hand.KeyPose;
                    _proposedKeyPose = _managedHand.Hand.KeyPose;
                    OnVerifiedGestureChanged?.Invoke(_managedHand, VerifiedGesture);
                }
            }

            UpdateCurrentInteractionPoint();
            UpdatePalmEvents();
        }

        void UpdatePalmEvents()
        {
            _lastPalmBack = PalmFacingBack;
            _lastPalmForward = PalmFacingForward;
            _handToHead = _headpose.position - _managedHand.Skeleton.Position;


            Vector3 up = _managedHand.Skeleton.Rotation*Vector3.up;
            if (Vector3.Dot(up, _handToHead.normalized) > _palmDotValidThreshold)
            {
                PalmFacingForward = true;
            }
            if (Vector3.Dot(up, _handToHead.normalized) < _palmDotInValidThreshold)
            {
                PalmFacingForward = false;
            }

            if (Vector3.Dot(up, _handToHead.normalized) < -_palmDotValidThreshold)
            {
                PalmFacingBack = true;
            }
            if (Vector3.Dot(up, _handToHead.normalized) > -_palmDotInValidThreshold)
            {
                PalmFacingBack = false;
            }


            if (PalmFacingForward && !_lastPalmForward)
            {
                OnPalmFront?.Invoke();
            }

            if (PalmFacingBack && !_lastPalmBack)
            {
                OnPalmBack?.Invoke();
            }
        }

        void UpdateCurrentInteractionPoint()
        {
            InteractionPoint referencePoint;
            if(Intent == IntentPose.Relaxed)
            {
                referencePoint = GetInteractionPoint(LastIntent);
            }
            else
            {
                referencePoint = GetInteractionPoint(Intent);
            }

            if(referencePoint != null)
            {
                Current.position = referencePoint.position;
                Current.radius = referencePoint.radius;
                Current.rotation = referencePoint.rotation;
                Current.active = true;
            }
        }

        public InteractionPoint GetInteractionPoint(IntentPose type)
        {
            switch (type)
            {
                case IntentPose.Pointing:
                    return Point;
                case IntentPose.Grasping:
                    return Grasp;
                case IntentPose.Pinching:
                    return Pinch;
                default:
                    break;
            }

            return null;
        }

        public InteractionPoint GetInteractionPoint(InteractionPointType type)
        {
            switch (type)
            {
                case InteractionPointType.Point:
                    return Point;
                case InteractionPointType.Grasp:
                    return Grasp;
                case InteractionPointType.Pinch:
                    return Pinch;
                case InteractionPointType.Current:
                    return Current;
                default:
                    break;
            }

            return null;
        }

        //Event Handlers:
        private void HandleKeyposeChanged(MLHandTracking.HandKeyPose keyPose)
        {
            OnKeyPoseChanged?.Invoke(_managedHand, keyPose);
        }

        //Private Methods:
        private Bounds CalculateGraspBounds(params ManagedKeypoint[] points)
        {
            Bounds graspBounds = new Bounds();
            graspBounds.center = _managedHand.Skeleton.Position;

            foreach (var item in points)
            {
                if (item.Visible)
                {
                    graspBounds.Encapsulate(item.positionFiltered);
                }
            }

            return graspBounds;
        }
#endif
    }
}