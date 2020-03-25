// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicLeapTools
{
    /// <summary>
    /// Attempts to reach a location or position similar to a spring joint with less overhead.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class PursuitJoint : MonoBehaviour
    {
        //Public Properties:
        [Tooltip("What should we pursue?")]
        public Transform target;
        [Tooltip("If target is not provided this location, in world coordiantes, is where we pursue instead.")]
        public Vector3 targetLocation;
        [Tooltip("When target is set/changed should we maintain the offset?")]
        public bool maintainOffset = true;
        [Tooltip("How much pull does the target have?  Reducing this will simulate increased weight.")]
        public float strength = 120;
        [Tooltip("Will slow down at this rate as object gets closer to the target.")]
        public float dampening = 15;

        //Private Variables:
        private Vector3 _offset;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private Transform _previousTarget;
        private float _initialDrag;
        private RigidbodyInterpolation _initialInterpolation;
        private PhysicMaterial _activePhysicMaterial;
        private PhysicMaterial _initialPhysicMaterial;

        //Init:
        private void Reset()
        {
            //setup rigidbody:
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.useGravity = false;
        }

        private void Awake()
        {
            //loads:
            _activePhysicMaterial = Resources.Load<PhysicMaterial>("PointerInteractablePhysicMaterial");

            //refs:
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();

            //sets:
            _initialDrag = _rigidbody.drag;
        }

        //Flow:
        private void OnEnable()
        {
            //toggle interpolation so movement is smooth:
            _initialInterpolation = _rigidbody.interpolation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            //swap physic material so collisions just slide as we drag:
            _initialPhysicMaterial = _collider.material;
            _collider.material = _activePhysicMaterial;

            //update offset:
            if (target != null)
            {
                _offset = transform.position - target.position;
                Debug.Log(_offset + " " + transform.position + " " + target.position);
            }

            //sets:
            targetLocation = transform.position;
        }

        private void OnDisable()
        {
            //toggle interpolation back:
            _rigidbody.interpolation = _initialInterpolation;

            //reset physic material:
            _collider.material = _initialPhysicMaterial;

            //reset drag value - this is important for throwing situaions where the body should go back to initial values:
            _rigidbody.drag = _initialDrag;
        }

        //Loops:
        private void FixedUpdate()
        {
            //apply dampening which overrides drag:
            _rigidbody.drag = dampening;
            
            //new target:
            if (_previousTarget != target)
            {
                if (target != null)
                {
                    if (target.parent == transform)
                    {
                        Debug.LogError("A PursuitJoint can not be the parent of it's target.");
                        enabled = false;
                    }
                    _offset = transform.position - target.position;
                }
                else
                {
                    targetLocation = transform.position;
                }
                _previousTarget = target;
            }
            
            //pursuit params:
            Vector3 destination = transform.position;
            Vector3 to = Vector3.zero;

            //find where we need to go:
            if (target != null)
            {
                //apply offset:
                if (target != null && maintainOffset)
                {
                    destination -= _offset;
                }

                to = target.position - destination;
            }
            else
            {
                to = targetLocation - transform.position;
            }

            //pursue:
            _rigidbody.AddForce(to * strength);
        }

        public void SetTarget(Transform t)
        {
            target = t;
        }

        public void RemoveTarget()
        {
            target = null;
        }
    }
}