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
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeapTools
{
    [ExecuteAlways]
    public class KeepInFront : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        [Tooltip("The offset from the camera we should try to maintain.")]
        public Vector3 offset = new Vector3(0, 0, 1);
        [Tooltip("How far should we push away from a surface?")]
        public float surfaceOffset = .07f;
        [Tooltip("At 0 this will always stay in front, at a higher value it will let the user move around more before reorienting.")]
        public float allowedDistance = 0;
        [Tooltip("What's the smallest we can scale down from the initial scale while pressing against a surface? 1 will turn this feature off.")]
        public float minimumScale = .5f;
        [Tooltip("How fast should we move into place?")]
        public float arrivalSpeed = 7;
        [Tooltip("Is the content's forward different than the transform's forward?")]
        public bool flipForward;
        [Tooltip("Should content's up match gravity at all times?")]
        public bool matchGravity = true;
        [Tooltip("Should we try to stay straight out in front of the user?")]
        public bool flatLocation;

        //Private Variables:
        private readonly float _maxWallDot = .3f;
        private Camera _mainCamera;
        private Vector3 _targetLocation;
        private Vector3 _updatedDestination;
        private Quaternion _updatedRotation;
        private bool _onSurface;
        private Vector3 _initialScale;

        //Init:
        private void Awake()
        {
            //refs:
            _mainCamera = Camera.main;

            if (!Application.isPlaying)
            {
                return;
            }

            //sets:
            _initialScale = transform.localScale;
            _updatedDestination = transform.position;
            OrientForward(Vector3.Normalize(_mainCamera.transform.position - transform.position));
        }

        //Flow:
        private void OnEnable()
        {
            //refs:
            _mainCamera = Camera.main;
        }

        //Deinit:
        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                return;
            }
        }

        //Loops:
        private void Update()
        {
            if (!Application.isPlaying)
            {
                //in-editor placement visualizer:
                transform.position = _mainCamera.transform.TransformPoint(offset);
                Vector3 to = Vector3.Normalize(transform.position - _mainCamera.transform.position);
                if (flipForward) to *= -1;
                if (matchGravity)
                {
                    to = Vector3.ProjectOnPlane(to, Vector3.up).normalized;
                    transform.rotation = Quaternion.LookRotation(to);
                }
                else
                {
                    transform.rotation = Quaternion.LookRotation(to);
                }
                return;
            }

            //get casting direction:
            Vector3 offsetPoint = _mainCamera.transform.TransformPoint(offset);
            Vector3 castVector = Vector3.Normalize(offsetPoint - _mainCamera.transform.position);
            if (flatLocation)
            {
                castVector = Vector3.ProjectOnPlane(castVector, Vector3.up).normalized;
            }

            //cast into world:
            RaycastHit[] hits = Physics.RaycastAll(_mainCamera.transform.position, castVector, offset.z);

            //reset to keep this current:
            _onSurface = false;

            if (hits.Length > 0)
            {
                //closest params:
                float closest = float.MaxValue;
                RaycastHit closestHit = new RaycastHit();

                //find closest that isn't within us:
                foreach (var item in hits)
                {
                    if (item.transform.GetComponentInParent<KeepInFront>() != null)
                    {
                        continue;
                    }

                    if (item.distance < closest)
                    {
                        closest = item.distance;
                        closestHit = item;
                    }
                }

                if (closestHit.collider != null)
                {
                    //status changed:
                    _onSurface = true;

                    //pull us off the surface based on surfaceOffset:
                    _targetLocation = closestHit.point + (castVector * -surfaceOffset);

                    //hug the wall:
                    _updatedDestination = _targetLocation;

                    //face out from wall:
                    OrientForward(closestHit.normal);
                }
                else
                {
                    //nothing was hit so just use offset:
                    _targetLocation = _mainCamera.transform.position + (castVector * offset.z);
                }
            }
            else
            {
                //nothing was hit so just use offset:
                _targetLocation = _mainCamera.transform.position + (castVector * offset.z);
            }

            //how far are we from the target?
            float distance = Vector3.Distance(_targetLocation, transform.position);


            if (!_onSurface)
            {
                //moved far enough to need reorientation?
                if (distance > allowedDistance)
                {
                    //new location:
                    _updatedDestination = _targetLocation;

                    //new rotation:
                    OrientForward(Vector3.Normalize(_mainCamera.transform.position - _updatedDestination));
                }
            }

            //update rotation:
            transform.rotation = Quaternion.Lerp(transform.rotation, _updatedRotation, Time.deltaTime * arrivalSpeed);

            //update position:
           // _updatedDestination += transform.TransformDirection(new Vector3(offset.x, offset.y, 0));
            transform.position = Vector3.Lerp(transform.position, _updatedDestination, Time.deltaTime * arrivalSpeed);

            //how far have we moved?
            float currentDistance = Vector3.Distance(transform.position, _targetLocation);

            //distance scaling:
            float distanceFrom = Vector3.Distance(_updatedDestination, _mainCamera.transform.position + _mainCamera.transform.forward * _mainCamera.nearClipPlane);
            float totalDistance = offset.z - _mainCamera.nearClipPlane - (_onSurface ? surfaceOffset : 0);
            float distancePercentage = Mathf.Clamp(distanceFrom / totalDistance, minimumScale, 1);
            transform.localScale = Vector3.Lerp(transform.localScale, _initialScale * distancePercentage, Time.deltaTime * arrivalSpeed);
        }

        //Public Methods:
        public void Recenter()
        {
            _updatedDestination = _targetLocation;
            OrientForward(Vector3.Normalize(_mainCamera.transform.position - _updatedDestination));
        }

        //Private Methods:
        private void OrientForward(Vector3 direction)
        {
            Vector3 up = Vector3.up;

            //are we on a horizontal surface?
            bool onHorizontal = Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > _maxWallDot;

            //adjust for up:
            if (flatLocation || matchGravity)
            {
                direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            }

            //handle horizontal:
            if (_onSurface && onHorizontal)
            {
                if (matchGravity)
                {
                    Vector3 to = Vector3.Normalize(_mainCamera.transform.position - transform.position);
                    direction = Vector3.ProjectOnPlane(to, Vector3.up).normalized;
                }
                else
                {
                    direction = Vector3.up;
                    up = Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized;
                }
            }

            //match content direction:
            if (flipForward)
            {
                _updatedRotation = Quaternion.LookRotation(direction, up);
            }
            else
            {
                _updatedRotation = Quaternion.LookRotation(direction * -1, up);
            }
        }
#endif
    }
}