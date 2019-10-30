// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeapTools
{
    public class TransmissionRoot : MonoBehaviour
    {
        //Public Variables:
        [HideInInspector] public string owner;

        //Public Properties:
        public static TransmissionRoot MyRoot
        {
            get
            {
                if (_myRoot == null)
                {
                    _myRoot = Get(NetworkUtilities.MyAddress);
                }

                return _myRoot;
            }
        }

        //Private Variables:
        private static Dictionary<string, TransmissionRoot> _all = new Dictionary<string, TransmissionRoot>();
        private static TransmissionRoot _myRoot;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private float _smoothTime = 0.3F;
        private Vector3 _positionalVelocity;
        private Quaternion _rotationalVelocity;

        //Deinit:
        private void OnDestroy()
        {
            _all.Remove(owner);
        }

        //Loops:
        private void Update()
        {
            //lerp us to the world-aligned location:
            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _positionalVelocity, _smoothTime);
            transform.rotation = MotionUtilities.SmoothDamp(transform.rotation, _targetRotation, ref _rotationalVelocity, _smoothTime); 
        }

        //Public Methods:
        public void Initialize()
        {
            if (!_all.ContainsKey(owner))
            {
                _all.Add(owner, this);
            }
        }

        public void SetPositionAndRotationTargets(Vector3 position, Quaternion rotation)
        {
            _targetPosition = position;
            _targetRotation = rotation;

            StopCoroutine("TweenPose");
            StartCoroutine("TweenPose");
        }

        public static TransmissionRoot Add(string ip)
        {
            TransmissionRoot transmissionRoot = Get(ip);
            if (transmissionRoot != null)
            {
                return transmissionRoot;
            }
            else
            {

                GameObject rootGameObject = new GameObject($"(Root - {ip})");
                transmissionRoot = rootGameObject.AddComponent<TransmissionRoot>();
                transmissionRoot.owner = ip;
                transmissionRoot.Initialize();
                return transmissionRoot;
            }
        }

        public static TransmissionRoot Get(string ip)
        {
            if (_all.ContainsKey(ip))
            {
                return (_all[ip]);
            }
            else
            {
                return null;
            }
        }

        //Coroutines:
        private IEnumerator TweenPose()
        {
            while (Quaternion.Angle(transform.rotation, _targetRotation) > 0)
            {
                transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _positionalVelocity, _smoothTime);
                transform.rotation = MotionUtilities.SmoothDamp(transform.rotation, _targetRotation, ref _rotationalVelocity, _smoothTime);

                yield return null;
            }
        }
    }
}