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
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeapTools
{
    [RequireComponent(typeof(SpatialMapperThrottle))]
    public class PlaceOnFloor : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Properties:
        public Vector3 Location
        {
            get;
            private set;
        }

        public bool Placed
        {
            get;
            private set;
        }

        //Public Variables:
        [Tooltip("Instructions that will be turned on until placement is complete.")]
        public GameObject instructions;
        [Tooltip("Content that will be turned on and placed when a suitable location is found.")]
        public GameObject content;
        [Tooltip("Does content's content match it's transform forward?")]
        public bool flippedForward;

        //Events:
        /// <summary>
        /// Fired when we have a clear area of the floor and the user's head is relatively stable:
        /// </summary>
        public Vector3Event OnPlaced = new Vector3Event();

        //Private Variables:
        private readonly float _headLocationIdleThreshold = 0.003f;
        private readonly float _headRotationIdleThreshold = .3f;
        private readonly int _historyCount = 5;
        private readonly float _headIdleRequiredDuration = .2f;
        private List<Vector3> _headLocationHistory;
        private List<Quaternion> _headRotationHistory;
        private float _headLocationVelocity;
        private float _headRotationVelocity;
        private Transform _mainCamera;
        private bool _headLocationIdle;
        private bool _headRotationIdle;
        private bool _headTemporarilyIdle;
        private bool _headIdle;
        private bool _placementValid;

        //Init:
        private void Awake()
        {
            //refs:
            _mainCamera = Camera.main.transform;

            //requirements:
            if (FindObjectOfType<MLSpatialMapper>() == null)
            {
                Debug.LogError("PlaceOnFloor requires and instance of the MLSpatialMapper in your scene.");
            }
        }

        //Flow:
        private void OnEnable()
        {
            //we can't operate on own content since it disables the content:
            if (content == gameObject)
            {
                Debug.LogError("StartInOpenArea can not be used on it's own content.  Move the StartInOpenArea to a separate object.", this);
                enabled = false;
            }

            //hide content:
            if (content != null)
            {
                content.SetActive(false);
            }

            if (instructions != null)
            {
                instructions.SetActive(true);
            }

            //sets:
            _headLocationHistory = new List<Vector3>();
            _headRotationHistory = new List<Quaternion>();

            //starts:
            Placed = false;
        }

        //Loops:
        private void Update()
        {
            //let headpose warmup a little:
            if (Time.frameCount < 3)
            {
                return;
            }

            HeadActivityDetermination();
            LookingAtFloorDetermination();
            
            //are we good to go?
            if (_headIdle && _placementValid)
            {
                //place content:
                if (content != null)
                {
                    content.SetActive(true);
                    content.transform.position = Location;

                    //face user after placement:
                    Vector3 to = Vector3.Normalize(Location - _mainCamera.position);
                    Vector3 flat = Vector3.ProjectOnPlane(to, Vector3.up);
                    if (!flippedForward)
                    {
                        flat *= -1;
                    }
                    content.transform.rotation = Quaternion.LookRotation(flat);
                }

                //instructions:
                if (instructions != null)
                {
                    instructions.SetActive(false);
                }

                Placed = true;
                OnPlaced?.Invoke(Location);
                enabled = false;
            }
        }

        //Coroutines:
        private IEnumerator HeadIdleTimeout()
        {
            yield return new WaitForSeconds(_headIdleRequiredDuration);
            _headIdle = true;
        }

        //Private Methods:
        private bool LookingAtFloorDetermination()
        {
            //cast to see if we are looking at the floor:
            RaycastHit hit;
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit))
            {
                SurfaceType surface = SurfaceDetails.Analyze(hit);
                
                if (surface == SurfaceType.Floor)
                {
                    Location = hit.point;
                    _placementValid = true;
                    return true;
                }
                else
                {
                    _placementValid = false;
                    return false;
                }
            }
            else
            {
                _placementValid = false;
                return false;
            }
        }

        private void HeadActivityDetermination()
        {
            //history:
            _headLocationHistory.Add(_mainCamera.position);
            if (_headLocationHistory.Count > _historyCount)
            {
                _headLocationHistory.RemoveAt(0);
            }

            _headRotationHistory.Add(_mainCamera.rotation);
            if (_headRotationHistory.Count > _historyCount)
            {
                _headRotationHistory.RemoveAt(0);
            }

            //location velocity:
            if (_headLocationHistory.Count == _historyCount)
            {
                _headLocationVelocity = 0;
                for (int i = 1; i < _headLocationHistory.Count; i++)
                {
                    _headLocationVelocity += Vector3.Distance(_headLocationHistory[i], _headLocationHistory[i - 1]);
                }
                _headLocationVelocity /= _headLocationHistory.Count;

                //idle detection:
                if (_headLocationVelocity <= _headLocationIdleThreshold)
                {
                    if (!_headLocationIdle)
                    {
                        _headLocationIdle = true;
                    }
                }
                else
                {
                    if (_headLocationIdle)
                    {
                        _headLocationIdle = false;
                    }
                }
            }

            //rotation velocity:
            if (_headRotationHistory.Count == _historyCount)
            {
                _headRotationVelocity = 0;
                for (int i = 1; i < _headRotationHistory.Count; i++)
                {
                    _headRotationVelocity += Quaternion.Angle(_headRotationHistory[i], _headRotationHistory[i - 1]);
                }
                _headRotationVelocity /= _headRotationHistory.Count;

                //idle detection:
                if (_headRotationVelocity <= _headRotationIdleThreshold)
                {
                    if (!_headRotationIdle)
                    {
                        _headRotationIdle = true;
                    }
                }
                else
                {
                    if (_headRotationIdle)
                    {
                        _headRotationIdle = false;
                    }
                }
            }

            //absolute idle head determination:
            if (_headLocationIdle && _headRotationIdle)
            {
                if (!_headTemporarilyIdle)
                {
                    _headTemporarilyIdle = true;
                    StartCoroutine("HeadIdleTimeout");
                }
            }
            else
            {
                if (_headTemporarilyIdle)
                {
                    _headIdle = false;
                    _headTemporarilyIdle = false;
                    StopCoroutine("HeadIdleTimeout");
                }
            }
        }
#endif
    }
}