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
    public class ScaleManipulator : MonoBehaviour
    {
        //Public Variables:
        [Tooltip("This is multiplied against the initial scale and defines how small we can get.")]
        public float minimumScaleOfInitial = .5f;
        [Tooltip("This is multiplied against the initial scale and defines how large we can get.")]
        public float maximumScaleOfInitial = 2f;
        [Tooltip("How many calls are required to go from minimum to maximum.")]
        public int steps = 5;
        [Tooltip("How fast do we animate to the next change in scale.")]
        public float animationSpeed = 10;

        //Private Variables:
        private Vector3 _initialScale;
        private Vector3 _minScale;
        private Vector3 _maxScale;
        private Vector3 _targetScale;

        //Init:
        private void Awake()
        {
            //sets:
            _initialScale = transform.localScale;
            _targetScale = transform.localScale;
            _minScale = _initialScale * minimumScaleOfInitial;
            _maxScale = _initialScale * maximumScaleOfInitial;
        }

        //Public Methods:
        public void ScaleUp()
        {
            Vector3 increment = (_maxScale - _minScale) / steps;
            _targetScale += increment;

            //clamp:
            if (_targetScale.magnitude > _maxScale.magnitude)
            {
                _targetScale = _maxScale;
            }
        }

        public void ScaleDown()
        {
            Vector3 increment = (_maxScale - _minScale) / steps;
            _targetScale -= increment;

            //clamp:
            if (_targetScale.magnitude < _minScale.magnitude)
            {
                _targetScale = _minScale;
            }
        }

        public void ResetManipulation()
        {
            _targetScale = _initialScale;
        }

        //Loops:
        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * animationSpeed);
        }
    }
}