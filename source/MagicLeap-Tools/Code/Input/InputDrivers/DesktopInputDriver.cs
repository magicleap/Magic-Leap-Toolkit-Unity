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
    public class DesktopInputDriver : InputDriver
    {
        //Public Variables:
        public KeyCode fire0 = KeyCode.Alpha1;
        public KeyCode fire1 = KeyCode.Alpha2;
        public KeyCode fire2 = KeyCode.Alpha3;
        public KeyCode up = KeyCode.W;
        public KeyCode down = KeyCode.S;
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        public KeyCode radialDragPositive = KeyCode.E;
        public KeyCode radialDragNegative = KeyCode.Q;
        public bool followInput = true;

        //Private Variables:
        private readonly float _radialDragIncrement = 10;
        private Camera _mainCamera;
        private InputSender _inputSender;

        //Init:
        private void Reset()
        {
            motionSource = transform;
        }

        private void Awake()
        {
            //refs:
            _mainCamera = Camera.main;
            _inputSender = GetComponent<InputSender>();

            //turn on sisnce we are on the actul desktop:
            if (Application.isEditor)
            {
                Active = true;
            }
        }

        //Loops:
        private void Update()
        {
            if (followInput)
            {
                //orientation from mouse:
                Vector3 worldPoint = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
                Vector3 to = Vector3.Normalize(worldPoint - _mainCamera.transform.position);
                transform.rotation = Quaternion.LookRotation(to);

                //position:
                transform.position = _mainCamera.transform.position;
            }

            //input events:
            if (Input.GetKeyDown(radialDragPositive))
            {
                RadialDrag(_radialDragIncrement);
            }

            if (Input.GetKeyDown(radialDragNegative))
            {
                RadialDrag(-_radialDragIncrement);
            }

            if (Input.GetMouseButtonDown(0))
            {
                Fire0Down();
            }

            if (Input.GetMouseButtonUp(0))
            {
                Fire0Up();
            }

            if (Input.GetKeyDown(fire0))
            {
                Fire0Down();
            }

            if (Input.GetKeyUp(fire0))
            {
                Fire0Up();
            }

            if (Input.GetKeyDown(fire1))
            {
                Fire1Down();
            }

            if (Input.GetKeyUp(fire1))
            {
                Fire1Up();
            }

            if (Input.GetKeyDown(fire2))
            {
                Fire2Down();
            }

            if (Input.GetKeyUp(fire2))
            {
                Fire2Up();
            }

            if (Input.GetKeyUp(up))
            {
                Up();
            }

            if (Input.GetKeyUp(down))
            {
                Down();
            }

            if (Input.GetKeyUp(left))
            {
                Left();
            }

            if (Input.GetKeyUp(right))
            {
                Right();
            }
        }
    }
}