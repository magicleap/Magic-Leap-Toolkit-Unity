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
    public class ManagedHandCollider
    {
#if PLATFORM_LUMIN
        //Events:
        public TriggerEvent OnTriggerEntered = new TriggerEvent();
        public TriggerEvent OnTriggerStayed = new TriggerEvent();
        public TriggerEvent OnTriggerExited = new TriggerEvent();
        public CollisionEvent OnCollisionEntered = new CollisionEvent();
        public CollisionEvent OnCollisionStayed = new CollisionEvent();
        public CollisionEvent OnCollisionExited = new CollisionEvent();

        //Private Variables:
        private ManagedHand _managedHand;
        private HandCollider _handCollider;
        private float _pointColliderSize = 0.0381f;
        private float _openHandColliderWidth = 0.0127f;
        private BoxCollider _boxCollider;
        private SphereCollider _sphereCollider;
        private float _touchEndTime;
        private float _touchEndTimeoutDelay = .5f;

        //Constructors:
        public ManagedHandCollider(ManagedHand managedHand)
        {
            //sets:
            _managedHand = managedHand;

            //creates:
            GameObject collider = new GameObject($"({managedHand.Hand.Type}Collider)", typeof(SphereCollider), typeof(BoxCollider), typeof(Rigidbody));
            collider.GetComponent<Rigidbody>().isKinematic = true;
            _handCollider = collider.AddComponent<HandCollider>();
            _boxCollider = _handCollider.GetComponent<BoxCollider>();
            _boxCollider.enabled = false;
            _sphereCollider = _handCollider.GetComponent<SphereCollider>();
            _sphereCollider.enabled = false;

            //hooks (relay the events coming from the relay to simplify access):
            _handCollider.OnTriggerEntered.AddListener(HandleTriggerEnter);
            _handCollider.OnTriggerStayed.AddListener(HandleTriggerStay);
            _handCollider.OnTriggerExited.AddListener(HandleTriggerExit);
            _handCollider.OnCollisionEntered.AddListener(HandleCollisionEnter);
            _handCollider.OnCollisionStayed.AddListener(HandleCollisionStay);
            _handCollider.OnCollisionExited.AddListener(HandleCollisionExit);
            _managedHand.Gesture.OnVerifiedGestureChanged += HandleGestureChanged;
            _managedHand.OnVisibilityChanged += HandleVisibility;
            _managedHand.Gesture.Pinch.Touch.OnEnd += HandleTouchEnd;
            _managedHand.Gesture.Grasp.Touch.OnEnd += HandleTouchEnd;
            _managedHand.OnVisibilityChanged += HandleVisibilityChanged;
        }

        //Public Methods:
        public void Update(bool palmCollisionsActive)
        {
            if (!_managedHand.Visible)
            {
                return;
            }

            //hand collider activity:
            switch (_managedHand.Gesture.VerifiedGesture)
            {
                case MLHandTracking.HandKeyPose.Fist:
                    //only enable if we aren't directly manipulating anything:
                    if (_managedHand.Gesture.Grasp.DirectManipulations.Count == 0)
                    {
                        _boxCollider.enabled = false;
                        _sphereCollider.enabled = true;
                        _handCollider.Rigidbody.MovePosition(_managedHand.Skeleton.Position);
                        _handCollider.transform.localScale = Vector3.one * _managedHand.Gesture.Grasp.radius;
                    }
                    else
                    {
                        _boxCollider.enabled = false;
                        _sphereCollider.enabled = false;
                    }
                    break;

                case MLHandTracking.HandKeyPose.L:
                case MLHandTracking.HandKeyPose.Finger:
                    if (_managedHand.Skeleton.Index.DotProduct > .92f)
                    {
                        _boxCollider.enabled = false;
                        _sphereCollider.enabled = true;
                        _handCollider.transform.localScale = Vector3.one * _pointColliderSize;
                        _handCollider.Rigidbody.MovePosition(_managedHand.Skeleton.Index.Tip.positionFiltered);
                    }
                    else
                    {
                        _boxCollider.enabled = false;
                        _sphereCollider.enabled = false;
                    }
                    break;

                case MLHandTracking.HandKeyPose.OpenHand:
                    if (palmCollisionsActive) //experimental since sometimes grabbing will incur a hit from the palm collider
                    {
                        if (_managedHand.Skeleton.Index.DotProduct > .92f)
                        {
                            _boxCollider.enabled = true;
                            _sphereCollider.enabled = false;
                            _handCollider.transform.localScale = new Vector3(_managedHand.Gesture.Grasp.radius, _openHandColliderWidth, _managedHand.Gesture.Grasp.radius);
                            _handCollider.Rigidbody.MovePosition(_managedHand.Skeleton.Position);
                            _handCollider.Rigidbody.MoveRotation(_managedHand.Skeleton.Rotation);
                        }
                        else
                        {
                            _boxCollider.enabled = false;
                            _sphereCollider.enabled = false;
                        }
                    }
                    else
                    {
                        _boxCollider.enabled = false;
                        _sphereCollider.enabled = false;
                    }

                    break;

                default:
                    _boxCollider.enabled = false;
                    _sphereCollider.enabled = false;
                    break;
            }

            //disabled timeout?
            if (Time.realtimeSinceStartup - _touchEndTime < _touchEndTimeoutDelay)
            {
                _boxCollider.enabled = false;
                _sphereCollider.enabled = false;
            }
        }

        //Event Handlers:
        private void HandleVisibilityChanged(ManagedHand hand, bool visible)
        {
            //we don't want an orphaned collider to cause collisions if the hand is not tracked:
            _handCollider.Rigidbody.detectCollisions = visible;

            //snap on hand found to avoid collider impacts that attempt to move toward the new hand location 
            if (visible)
            {
                _handCollider.transform.position = hand.Skeleton.Position;
            }
        }

        private void HandleTouchEnd()
        {
            _touchEndTime = Time.realtimeSinceStartup;
        }

        private void HandleGestureChanged(ManagedHand hand, MLHandTracking.HandKeyPose pose)
        {
            //snap transform changes to avoid force pops:
            switch (pose)
            {
                case MLHandTracking.HandKeyPose.Fist:
                    _handCollider.transform.position = _managedHand.Skeleton.Position;
                    _handCollider.transform.localScale = Vector3.one * _managedHand.Gesture.Grasp.radius;
                    break;

                case MLHandTracking.HandKeyPose.L:
                case MLHandTracking.HandKeyPose.Finger:
                    _handCollider.transform.localScale = Vector3.one * _pointColliderSize;
                    _handCollider.transform.position = _managedHand.Skeleton.Index.Tip.positionFiltered;
                    break;

                case MLHandTracking.HandKeyPose.OpenHand:
                    _handCollider.transform.localScale = new Vector3(_managedHand.Gesture.Grasp.radius, _openHandColliderWidth, _managedHand.Gesture.Grasp.radius);
                    _handCollider.transform.position = _managedHand.Skeleton.Position;
                    _handCollider.transform.rotation = _managedHand.Skeleton.Rotation;
                    break;
            }
        }

        private void HandleVisibility(ManagedHand hand, bool visible)
        {
            _handCollider.gameObject.SetActive(visible);
        }

        private void HandleTriggerEnter(Collider other)
        {
            OnTriggerEntered?.Invoke(other);
        }

        private void HandleTriggerStay(Collider other)
        {
            OnTriggerStayed?.Invoke(other);
        }

        private void HandleTriggerExit(Collider other)
        {
            OnTriggerExited?.Invoke(other);
        }

        private void HandleCollisionEnter(Collision collision)
        {
            OnCollisionEntered?.Invoke(collision);
        }

        private void HandleCollisionStay(Collision collision)
        {
            OnCollisionStayed?.Invoke(collision);
        }

        private void HandleCollisionExit(Collision collision)
        {
            OnCollisionExited?.Invoke(collision);
        }
#endif
    }
}