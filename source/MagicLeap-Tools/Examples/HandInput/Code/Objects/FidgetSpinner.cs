// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.Events;

namespace MagicLeapTools
{
    [RequireComponent(typeof(AudioSource))]
    public class FidgetSpinner : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public Transform housing;
        public float friction = .995f;
        public float minVelocity = .0005f;
        public float maxVelocity = 20;
        public AudioClip startSound;
        public AudioClip loopSound;

        //Events:
        /// <summary>
        /// Thrown as soon as we start spinning.
        /// </summary>
        public UnityEvent OnSpinningStarted;
        /// <summary>
        /// Thrown once we slow down and stop spinning.
        /// </summary>
        public UnityEvent OnSpinningCompleted;

        //Public Properties:
        /// <summary>
        /// Are we spinning?
        /// </summary>
        public bool Spinning
        {
            get;
            private set;
        }

        /// <summary>
        /// How fast are we spinning?
        /// </summary>
        public float Velocity
        {
            get;
            private set;
        }

        //Private Variables:
        private DirectManipulation _directManipulation;
        private ManagedHand _oppositeHand;
        private float _swipeRadius = 0.0508f;
        private float _swipeRadiusBuffer = 0.003175f;
        private float _swipeMagnifier = 1000;
        private bool _inSwipeZone;
        private AudioSource _audioSource;
        private InteractionPoint _interactionPoint;
        private float _maxVolumeVelocity = 20;

        //Init:
        private void Awake()
        {
            //refs:
            //if we have 2 audiosources we can fade one with velocity reduction:
            _audioSource = GetComponents<AudioSource>()[1];
            if (_audioSource == null)
            {
                _audioSource = GetComponents<AudioSource>()[0];
            }

            //setups:
            _audioSource.loop = true;
            _audioSource.clip = loopSound;

            //hooks:
            _directManipulation = GetComponent<DirectManipulation>();
            _directManipulation.OnGrabBegin.AddListener(HandleTouchBegan);
            _directManipulation.OnDragUpdate.AddListener(HandleDragUpdate);
        }

        //Event Handlers:
        private void HandleTouchBegan(InteractionPoint point)
        {
            if (_directManipulation.activeInteractionPoints.Count == 1)
            {
                _interactionPoint = point;
                switch (_directManipulation.activeInteractionPoints[0].managedHand.Hand.Type)
                {
                    case MLHandTracking.HandType.Left:
                        _oppositeHand = HandInput.Right;
                        break;

                    case MLHandTracking.HandType.Right:
                        _oppositeHand = HandInput.Left;
                        break;
                }
            }
        }

        private void HandleDragUpdate(InteractionPoint[] arg0, Vector3 arg1, Quaternion arg2, float arg3)
        {
            if (_oppositeHand.Skeleton.Index.Tip.Visible)
            {
                float distance = Vector3.Distance(_interactionPoint.position, _oppositeHand.Skeleton.Index.Tip.positionFiltered);

                //detect swipe:
                if (!_inSwipeZone)
                {
                    if (distance < _swipeRadius)
                    {
                        Velocity = 0;
                        _inSwipeZone = true;
                        _audioSource.Stop();
                        Spinning = false;

                        OnSpinningCompleted?.Invoke();
                    }
                }
                else
                {
                    if (distance > _swipeRadius + _swipeRadiusBuffer)
                    {
                        //discover swipe details:
                        Vector3 swipeExitPoint = Vector3.Normalize(_oppositeHand.Skeleton.Index.Tip.positionFiltered - _interactionPoint.position) * _swipeRadius + _interactionPoint.position;
                        Vector3 swipeDirection = _oppositeHand.Skeleton.Index.Tip.positionFiltered - swipeExitPoint;
                        float swipeSpeed = swipeDirection.magnitude;

                        //discover spin direction:
                        Vector3 interactionPointRight = _interactionPoint.rotation * Vector3.right;
                        float dot = Vector3.Dot(interactionPointRight, swipeDirection.normalized);
                        float spinDirection = Mathf.Sign(dot);

                        //apply velocity:
                        Velocity = swipeSpeed * _swipeMagnifier * spinDirection;
                        Velocity = Mathf.Clamp(Velocity, -maxVelocity, maxVelocity);
                        _inSwipeZone = false;
                    }
                }
            }
        }

        //Loops:
        private void Update()
        {
            //started spinning:
            if (!Spinning && Mathf.Abs(Velocity) != 0)
            {
                _audioSource.PlayOneShot(startSound);
                _audioSource.volume = 1;
                _audioSource.Play();
                Spinning = true;

                OnSpinningStarted?.Invoke();
            }

            //spin application:
            if (Spinning)
            {
                //cache:
                float currentVelocity = Velocity;

                //apply friction:
                Velocity *= friction;

                //apply velocty:
                housing.Rotate(Vector3.up * Velocity);

                //apply volume:
                _audioSource.volume = Mathf.Clamp01(Mathf.Abs(Velocity) / _maxVolumeVelocity);

                //finalize spinning:
                float delta = Mathf.Abs(currentVelocity) - Mathf.Abs(Velocity);
                if (delta <= minVelocity)
                {
                    Spinning = false;
                    _audioSource.Stop();
                    Velocity = 0;

                    OnSpinningCompleted?.Invoke();
                }
            }
        }
#endif
    }
}