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
    public class ControlSmoother : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public ControlInput controlInput;
        [Tooltip("Optional slot for a pointer to use the trigger and collider detection to further stabalize from user input.")]
        public Pointer pointer;

        //Private Variables:
        private readonly float _stablePositionDelta = 0.0002f; //based on averages this velocity is seen as stable
        private readonly float _unstablePositionDelta = 0.0008f; //based on averages this velocity is seen as intentional movement
        private readonly float _triggerPulledMovedThreshold = 0.0025f; //while the trigger is pulled below this amount of movement is seen as stable 
        private readonly float _stablePositionLerpSpeed = .25f; //how much to lerp when stable
        private readonly float _unstablePositionLerpSpeed = 40; //how much to lerp when intentionally moving
        private List<Vector3> _positionHistory = new List<Vector3>();
        private readonly int _positionHistoryCount = 5;
        private bool _pinned;

        //Init:
        private void Reset()
        {
            //refs:
            controlInput = GetComponent<ControlInput>();
            pointer = GetComponent<Pointer>();
        }

        //Flow:
        private void OnEnable()
        {
            //override incase follow control was turned on:
            controlInput.followControl = false;

            //hooks:
            controlInput.OnTriggerPressBegan.AddListener(HandleTriggerPressBegan);
            controlInput.OnTriggerPressEnded.AddListener(HandleTriggerPressEnded);
        }

        private void OnDisable()
        {
            //unhooks:
            controlInput.OnTriggerPressBegan.RemoveListener(HandleTriggerPressBegan);
            controlInput.OnTriggerPressEnded.RemoveListener(HandleTriggerPressEnded);
        }

        //Loops:
        private void Update()
        {
            //enough history to stabalize control?
            if (_positionHistory.Count == _positionHistoryCount)
            {
                //calculate average delta/velocity of control's movement from history:
                float averageLocationDelta = 0;
                for (int i = 1; i < _positionHistory.Count; i++)
                {
                    averageLocationDelta += Vector3.Distance(_positionHistory[i], _positionHistory[i - 1]);
                }
                averageLocationDelta /= _positionHistory.Count;

                //calculate movement speed percentage based on velocity from averages:
                float clampedDelta = Mathf.Clamp(averageLocationDelta, _stablePositionDelta, _unstablePositionDelta);
                float percentage = (clampedDelta - _stablePositionDelta) / (_unstablePositionDelta - _stablePositionDelta);

                //get position lerp value:
                float lerp = Mathf.Lerp(_stablePositionLerpSpeed, _unstablePositionLerpSpeed, percentage);

                //still pinned?
                if (_pinned)
                {
                    //has the control moved far enough?
                    if (Vector3.Distance(controlInput.Position, _positionHistory[_positionHistory.Count - 1]) > _triggerPulledMovedThreshold)
                    {
                        _pinned = false;
                    }
                }

                //apply:
                if (!_pinned)
                {
                    transform.position = Vector3.Lerp(transform.position, controlInput.Position, Time.deltaTime * lerp);
                    transform.rotation = Quaternion.Lerp(transform.rotation, controlInput.Orientation, Time.deltaTime * lerp);
                }
            }

            //cache:
            if (!_pinned)
            {
                //log control location for smoothing history:
                _positionHistory.Add(controlInput.Position);
                if (_positionHistory.Count > _positionHistoryCount)
                {
                    _positionHistory.RemoveAt(0);
                }
            }
        }

        //Event Handlers:
        private void HandleTriggerPressEnded()
        {
            //we can safely remove the pinning:
            if (_pinned)
            {
                _pinned = false;
            }
        }

        private void HandleTriggerPressBegan()
        {
            if (pointer == null)
            {
                return;
            }

            //if we have a pointer and it has a target let's pin so aiming is more stable during trigger pulls:
            if (pointer.Target != null)
            {
                _pinned = true;
            }
        }
#endif
    }
}