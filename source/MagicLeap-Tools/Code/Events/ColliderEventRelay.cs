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
using UnityEngine.Events;

namespace MagicLeapTools
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class ColliderEventRelay : MonoBehaviour
    {
        //Events:
        public TriggerEvent OnTriggerEntered = new TriggerEvent();
        public TriggerEvent OnTriggerStayed = new TriggerEvent();
        public TriggerEvent OnTriggerExited = new TriggerEvent();
        public CollisionEvent OnCollisionEntered = new CollisionEvent();
        public CollisionEvent OnCollisionStayed = new CollisionEvent();
        public CollisionEvent OnCollisionExited = new CollisionEvent();

        //Public Properties:
        public Collider[] Colliders
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponentsInChildren<Collider>();
                }
                return _collider;
            }
        }

        public Rigidbody Rigidbody
        {
            get
            {
                if (_rigidbody == null)
                {
                    _rigidbody = GetComponent<Rigidbody>();
                }
                return _rigidbody;
            }
        }

        //Private Variables:
        private Collider[] _collider;
        private Rigidbody _rigidbody;

        //Event Handlers:
        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEntered?.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            OnTriggerStayed?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            OnTriggerExited?.Invoke(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnCollisionEntered?.Invoke(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            OnCollisionStayed?.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            OnCollisionExited?.Invoke(collision);
        }
    }
}