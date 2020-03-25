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
using System;

namespace MagicLeapTools
{
    public class TransmissionObject : MonoBehaviour
    {
        //Public Variables:
        [Tooltip("Optional transform to observe for automatic synchronization to all known peers.")]
        public Transform motionSource;
        [Tooltip("Duration of lerp to new updates from the network.")]
        public float smoothTime = .04f;
        [Tooltip("How smooth should network sends be.")]
        public float sendFrameRate = 24;
        [Tooltip("An optional, uniquely named prefab residing in a Resources folder for spawning on remote peers.")]
        public TransmissionObject remotePrefab;
        [Tooltip("Determines if ownership transfer requests should be denied. An ownership request occurs when IsMine is changed to true.")]
        public bool ownershipLocked;
        [HideInInspector] public string resourceFileName;
        [HideInInspector] public string creator;
        [HideInInspector] public string guid;
        [HideInInspector] public Vector3 localPosition;
        [HideInInspector] public Quaternion rotationOffset;
        [HideInInspector] public Vector3 targetScale;

        //Public Properties:
        public bool IsMine
        {
            get
            {
                return _isMine;
            }

            set
            {
                if (!_initialOwnershipSet)
                {
                    _initialOwnershipSet = true;
                    _isMine = value;
                }
                else
                {
                    //let go of ownership:
                    if (_isMine && !value)
                    {
                        _isMine = false;
                    }

                    //request ownership:
                    if (!_isMine && value)
                    {
                        OwnershipTransferenceRequestMessage ownershipTransferenceRequestMessage = new OwnershipTransferenceRequestMessage(guid);
                        Transmission.Send(ownershipTransferenceRequestMessage);
                    }
                }
            }
        }

        //Private Variables:
        private Vector3 _previousPosition;
        private Quaternion _previousRotation;
        private Vector3 _previousScale;
        private static Dictionary<string, TransmissionObject> _all = new Dictionary<string, TransmissionObject>();
        private bool _isMine;
        private Vector3 _positionVelocity;
        private Quaternion _rotationVelocity;
        private Vector3 _scaleVelocity;
        private bool _initialOwnershipSet;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        //Init:
        private void Awake()
        {
            //sets:
            localPosition = TransformUtilities.LocalPosition(Transmission.Instance.sharedOrigin.position, Transmission.Instance.sharedOrigin.rotation, transform.position);
            rotationOffset = TransformUtilities.GetRotationOffset(Transmission.Instance.sharedOrigin.rotation, transform.rotation);
            targetScale = transform.localScale;

            //catalog:
            _all.Add(guid, this);

            //hooks:
            Transmission.Instance.OnPeerFound.AddListener(HandlePeerFound);
            Transmission.Instance.OnTransformSync.AddListener(HandleTransformSync);
            Transmission.Instance.OnOwnershipGained.AddListener(HandleOwnershipGained);
            Transmission.Instance.OnOwnershipTransferDenied.AddListener(HandleOwnershipTransferDenied);

            StartCoroutine("ShareTransformStatus");
        }

        //Deint:
        private void OnDestroy()
        {
            StopAllCoroutines();
            
            //remove from catalog:
            _all.Remove(guid);

            //unhooks:
            if (Transmission.Instance != null)
            {
                Transmission.Instance.OnPeerFound.RemoveListener(HandlePeerFound);
                Transmission.Instance.OnTransformSync.RemoveListener(HandleTransformSync);
                Transmission.Instance.OnOwnershipGained.RemoveListener(HandleOwnershipGained);
                Transmission.Instance.OnOwnershipTransferDenied.RemoveListener(HandleOwnershipTransferDenied);
            }
        }

        //Loops:
        private void Update()
        {
            if (!_isMine)
            {
                transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _positionVelocity, smoothTime);
                transform.rotation = MotionUtilities.SmoothDamp(transform.rotation, _targetRotation, ref _rotationVelocity, smoothTime);
                transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref _scaleVelocity, smoothTime);
            }
        }

        private void LateUpdate()
        {
            if (_isMine)
            {
                //automatically follow motion source if set:
                if (motionSource != null)
                {
                    transform.position = motionSource.position;
                    transform.rotation = motionSource.rotation;
                    transform.localScale = motionSource.localScale;
                }
            }
        }

        //Public Methods:
        /// <summary>
        /// Updates every TransmissionObject that we own to all peers to synchonize position, scale, and rotation.
        /// </summary>
        public static void SynchronizeAll()
        {
            foreach (var item in _all)
            {
                if (item.Value._isMine)
                {
                    item.Value.Synchronize();
                }
            }
        }

        /// <summary>
        /// Updates peers on the position, scale, and rotation of this transmission object.
        /// </summary>
        public void Synchronize()
        {
            if (_isMine)
            {
                //update relative:
                localPosition = TransformUtilities.LocalPosition(Transmission.Instance.sharedOrigin.position, Transmission.Instance.sharedOrigin.rotation, transform.position);
                rotationOffset = TransformUtilities.GetRotationOffset(Transmission.Instance.sharedOrigin.rotation, transform.rotation);

                //send out the change to this transform:
                Transmission.Send(new TransformSyncMessage(this));
            }
        }

        /// <summary>
        /// Enables this GameObject and sychronizes this across all peers.
        /// </summary>
        public void Enable()
        {
            if (_isMine && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Transmission.Send(new OnEnabledMessage(guid));
            }

            StartCoroutine("ShareTransformStatus");
        }


        /// <summary>
        /// Disables this GameObject and sychronizes this across all peers.
        /// </summary>
        public void Disable()
        {
            if (_isMine && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                Transmission.Send(new OnDisabledMessage(guid));
            }

            StopCoroutine("ShareTransformStatus");
        }

        /// <summary>
        /// Destroys this GameObject and sychronizes this across all peers.
        /// </summary>
        public void Despawn()
        {
            Transmission.Send(new DespawnMessage(guid));
            StopAllCoroutines();
            Destroy(gameObject);
        }

        public static TransmissionObject Get(string guid)
        {
            if (_all.ContainsKey(guid))
            {
                return (_all[guid]);
            }
            else
            {
                return null;
            }
        }

        public static bool Exists(string guid)
        {
            return _all.ContainsKey(guid);
        }

        //Coroutines:
        private IEnumerator ShareTransformStatus()
        {
            while (true)
            {
                if (_isMine)
                {
                    bool dirty = false;

                    //position changed?
                    if (_previousPosition != transform.position)
                    {
                        _previousPosition = transform.position;
                        dirty = true;
                    }

                    //rotation changed?
                    if (_previousRotation != transform.rotation)
                    {
                        _previousRotation = transform.rotation;
                        dirty = true;
                    }

                    //scale changed?
                    if (_previousScale != transform.localScale)
                    {
                        _previousScale = transform.localScale;
                        dirty = true;
                    }

                    //something changed!
                    if (dirty)
                    {
                        Synchronize();
                    }

                    //pace the looping based on frame rate chosen:
                    yield return new WaitForSeconds(1 / sendFrameRate);
                }

                yield return null;
            }
        }

        //Event Handlers:
        private void HandleOwnershipTransferDenied(TransmissionObject transmissionObject)
        {
            if (transmissionObject == this)
            {
                //set just in case:
                _isMine = false;
            }
        }

        private void HandleOwnershipGained(TransmissionObject transmissionObject)
        {
            if (transmissionObject == this)
            {
                _isMine = true;
            }
        }

        private void HandlePeerFound(string peer, long age)
        {
            //if we own this then share existence with new peer:
            if (_isMine)
            {
                Transmission.Instance.SpawnRecap(this, peer);
            }
        }

        private void HandleTransformSync(TransformSyncMessage transformSyncMessage)
        {
            //is this a pose message for us?
            if (transformSyncMessage.ig != guid)
            {
                return;
            }

            //unpack:
            Vector3 receivedLocalPosition = new Vector3((float)transformSyncMessage.px, (float)transformSyncMessage.py, (float)transformSyncMessage.pz);
            Quaternion receivedRotationOffset = new Quaternion((float)transformSyncMessage.rx, (float)transformSyncMessage.ry, (float)transformSyncMessage.rz, (float)transformSyncMessage.rw);
            _targetPosition = TransformUtilities.WorldPosition(Transmission.Instance.sharedOrigin.position, Transmission.Instance.sharedOrigin.rotation, receivedLocalPosition);
            _targetRotation = TransformUtilities.ApplyRotationOffset(Transmission.Instance.sharedOrigin.rotation, receivedRotationOffset);
            targetScale = new Vector3((float)transformSyncMessage.sx, (float)transformSyncMessage.sy, (float)transformSyncMessage.sz);
        }
    }
}