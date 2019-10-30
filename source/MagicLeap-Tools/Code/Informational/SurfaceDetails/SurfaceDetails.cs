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
    //Public Enums:
    public enum SurfaceType { None, Floor, Seat, Table, Underside, Wall, Ceiling }

    public class SurfaceDetails : MonoBehaviour
    {
        //Public Properties:
        public static float FloorHeight
        {
            get;
            private set;
        }
        
        //Private Variables:
        private static Transform _mainCamera;
        private static float _wallThreshold = .65f;
        private static float _minimumSeatHeight = 0.4064f;
        private static float _minimumTableHeight = 0.6604f;
        private static float _undersideHeight = 1.2192f;
        private static float _roomHeightThreshold = 3.048f;
        private static bool _initialized;

        //Init:
        private void Awake()
        {
            Initialize();
        }

        //Loops:
        private void Update()
        {
            //cast to help update floor location faster:
            RaycastHit hit;

            //headpose targeted:
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit))
            {
                UpdateFloor(hit);
            }

            //straight down:
            if (Physics.Raycast(_mainCamera.position, Vector3.down, out hit))
            {
                UpdateFloor(hit);
            }

            //halfway between headpose targeted and straight down:
            Quaternion between = Quaternion.Lerp(_mainCamera.rotation, Quaternion.LookRotation(Vector3.down), .5f);
            if (Physics.Raycast(_mainCamera.position, between * Vector3.forward, out hit))
            {
                UpdateFloor(hit);
            }

        }

        //Public Methods:
        public static SurfaceType Analyze(RaycastHit hit)
        {
            Initialize();

            UpdateFloor(hit);

            //determine surface:
            float dot = Vector3.Dot(Vector3.up, hit.normal);
            if (Mathf.Abs(dot) <= _wallThreshold)
            {
                return SurfaceType.Wall;
            }
            else
            {
                if (Mathf.Sign(dot) == 1)
                {
                    //status:
                    float floorDistance = Mathf.Abs(hit.point.y - FloorHeight);
                    float headDistance = Mathf.Abs(_mainCamera.position.y - FloorHeight);

                    if (headDistance < _minimumTableHeight)
                    {
                        return SurfaceType.Table;
                    }

                    if (hit.point.y >= FloorHeight + _minimumTableHeight)
                    {
                        return SurfaceType.Table;
                    }

                    if (hit.point.y >= FloorHeight + _minimumSeatHeight)
                    {
                        return SurfaceType.Seat;
                    }

                    return SurfaceType.Floor;

                }
                else
                {
                    if (hit.point.y <= FloorHeight + _undersideHeight)
                    {
                        return SurfaceType.Underside;
                    }

                    return SurfaceType.Ceiling;
                }
            }
        }

        //Private Methods:
        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            //sets:
            FloorHeight = float.MaxValue;

            //refs:
            _mainCamera = Camera.main.transform;

            _initialized = true;
        }

        private static void UpdateFloor(RaycastHit hit)
        {
            //solve for going to another floor in a building:
            if (_mainCamera.position.y - FloorHeight > _roomHeightThreshold)
            {
                FloorHeight = float.MaxValue;
            }
            
            if (hit.point.y < FloorHeight)
            {
                FloorHeight = hit.point.y;
            }
        }
    }
}