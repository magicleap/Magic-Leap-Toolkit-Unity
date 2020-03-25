// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace MagicLeapTools
{
    /// <summary>
    /// Sends input to InputReceivers that have been identifed through raycasting.
    /// </summary>
    public class InputSender : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public InputDriver inputDriver;
        [Tooltip("How far to raycast.")]
        public float distance = 3;
        public LayerMask layermask;
        [Tooltip("While selecting and then moving, an inputDriver's motionSource must exceed this much distance to begin a drag.")]
        public float dragPositionThreshold = .01f;
        [Tooltip("While selecting and then moving, an inputDriver's motionSource must exceed this much rotation to begin a drag.")]
        public float dragRotationThreshold = 1f;
        public bool raycastAll;

        //Public Properties:
        public Vector3 DragStartedLocation
        {
            get;
            private set;
        }

        //Private Variables:
        private List<InputReceiver> _scannedReceivers = new List<InputReceiver>();
        private List<InputReceiver> _targetedReceivers = new List<InputReceiver>();
        private List<InputReceiver> _selectedReceivers = new List<InputReceiver>();
        private List<InputReceiver> _draggedReceivers = new List<InputReceiver>();
        private Vector3 _selectedPosition;
        private Quaternion _selectedRotation;

        //Init:
        private void Reset()
        {
            //sets:
            layermask = -1;

            //refs:
            inputDriver = GetComponent<InputDriver>();
        }

        //Flow:
        private void OnEnable()
        {
            //hooks:
            inputDriver.OnUp += HandleUp;
            inputDriver.OnDown += HandleDown;
            inputDriver.OnLeft += HandleLeft;
            inputDriver.OnRight += HandleRight;
            inputDriver.OnFire0Down += HandleFire0Down;
            inputDriver.OnFire0Up += HandleFire0Up;
            inputDriver.OnFire1Down += HandleFire1Down;
            inputDriver.OnFire1Up += HandleFire1Up;
            inputDriver.OnFire2Down += HandleFire2Down;
            inputDriver.OnFire2Up += HandleFire2Up;
            inputDriver.OnRadialDrag += HandleRotate;
        }

        private void OnDisable()
        {
            //unhooks:
            inputDriver.OnUp -= HandleUp;
            inputDriver.OnDown -= HandleDown;
            inputDriver.OnLeft -= HandleLeft;
            inputDriver.OnRight -= HandleRight;
            inputDriver.OnFire0Down -= HandleFire0Down;
            inputDriver.OnFire0Up -= HandleFire0Up;
            inputDriver.OnFire1Down -= HandleFire1Down;
            inputDriver.OnFire1Up -= HandleFire1Up;
            inputDriver.OnFire2Down -= HandleFire2Down;
            inputDriver.OnFire2Up -= HandleFire2Up;
            inputDriver.OnRadialDrag -= HandleRotate;
        }

        //Loops:
        private void LateUpdate()
        {
            Scan();
            DragDetection();
        }

        //Private Methods:
        private void DragDetection()
        {
            if (_draggedReceivers.Count == 0 && _selectedReceivers.Count > 0)
            {
                //currents:
                float positionDistance = Vector3.Distance(_selectedPosition, inputDriver.motionSource.position);
                float rotationDistance = Quaternion.Angle(_selectedRotation, inputDriver.motionSource.rotation);

                //did we start dragging?
                if (positionDistance > dragPositionThreshold || rotationDistance > dragRotationThreshold)
                {
                    DragStartedLocation = inputDriver.motionSource.position;

                    foreach (var item in _selectedReceivers)
                    {
                        _draggedReceivers.Add(item);
                        item.DragBegin(gameObject);
                    }
                }
            }
        }

        private void Scan()
        {
            RaycastHit[] hits = new RaycastHit[0];

            if (raycastAll)
            {
                //cast:
                hits = Physics.RaycastAll(inputDriver.motionSource.position, inputDriver.motionSource.forward, distance, layermask);
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(inputDriver.motionSource.position, inputDriver.motionSource.forward, out hit, distance, layermask))
                {
                    hits = new RaycastHit[] { hit };
                }
            }

            if (hits.Length == 0)
            {
                if (_targetedReceivers.Count > 0)
                {
                    foreach (var item in _targetedReceivers)
                    {
                        if (!_selectedReceivers.Contains(item))
                        {
                            item.TargetExit(gameObject);
                        }
                    }

                    _scannedReceivers.Clear();
                    _targetedReceivers.Clear();
                }

                return;
            }

            //catalog zones:
            _scannedReceivers.Clear();
            foreach (var hit in hits)
            {
                InputReceiver zone = hit.transform.GetComponent<InputReceiver>();
                if (zone != null)
                {
                    zone.TargetEnter(gameObject);
                    _scannedReceivers.Add(zone);
                }
            }

            //target exits:
            foreach (var item in _targetedReceivers)
            {
                if (!_selectedReceivers.Contains(item) && !_scannedReceivers.Contains(item))
                {
                    item.TargetExit(gameObject);
                }
            }

            //update active zones:
            _targetedReceivers = new List<InputReceiver>(_scannedReceivers);
        }

        //Event Handlers:
        private void HandleLeft(InputDriver input)
        {
            if (_targetedReceivers.Count > 0)
            {
                foreach (var item in _targetedReceivers)
                {
                    item.LeftReceived(gameObject);
                }
            }
        }

        private void HandleRight(InputDriver input)
        {
            if (_targetedReceivers.Count > 0)
            {
                foreach (var item in _targetedReceivers)
                {
                    item.RightReceived(gameObject);
                }
            }
        }

        private void HandleUp(InputDriver input)
        {
            if (_targetedReceivers.Count > 0)
            {
                foreach (var item in _targetedReceivers)
                {
                    item.UpReceived(gameObject);
                }
            }
        }

        private void HandleDown(InputDriver input)
        {
            if (_targetedReceivers.Count > 0)
            {
                foreach (var item in _targetedReceivers)
                {
                    item.DownReceived(gameObject);
                }
            }
        }

        private void HandleRotate(float delta, InputDriver input)
        {
            if (_targetedReceivers.Count > 0)
            {
                foreach (var item in _targetedReceivers)
                {
                    item.RadialDragReceived(delta, gameObject);
                }
            }
        }

        private void HandleFire0Down(InputDriver input)
        {
            if (_targetedReceivers.Count > 0)
            {
                foreach (var item in _targetedReceivers)
                {
                    _selectedReceivers.Add(item);
                    item.Fire0DownReceived(gameObject);
                }

                _selectedPosition = inputDriver.motionSource.position;
                _selectedRotation = inputDriver.motionSource.rotation;
            }
        }

        private void HandleFire0Up(InputDriver input)
        {
            if (_draggedReceivers.Count > 0)
            {
                foreach (var item in _draggedReceivers)
                {
                    item.DragEnd(gameObject);
                }

                _draggedReceivers.Clear();
            }
            else
            {
                foreach (var item in _selectedReceivers)
                {
                    item.Clicked(gameObject);
                }
            }

            foreach (var item in _selectedReceivers)
            {
                item.Fire0UpReceived(gameObject);

                if (!_targetedReceivers.Contains(item))
                {
                    item.TargetExit(gameObject);
                }
            }

            _selectedReceivers.Clear();
        }

        private void HandleFire1Down(InputDriver input)
        {
            if (_targetedReceivers.Count > 0)
            {
                foreach (var item in _targetedReceivers)
                {
                    item.Fire1DownReceived(gameObject);
                }
            }
        }

        private void HandleFire1Up(InputDriver input)
        {
            if (_targetedReceivers.Count > 0)
            {
                foreach (var item in _targetedReceivers)
                {
                    item.Fire1UpReceived(gameObject);
                }
            }
        }

        private void HandleFire2Down(InputDriver input)
        {
            if (_targetedReceivers.Count > 0)
            {
                foreach (var item in _targetedReceivers)
                {
                    item.Fire2DownReceived(gameObject);
                }
            }
        }

        private void HandleFire2Up(InputDriver input)
        {
            if (_targetedReceivers.Count > 0)
            {
                foreach (var item in _targetedReceivers)
                {
                    item.Fire2UpReceived(gameObject);
                }
            }
        }
#endif
    }
}