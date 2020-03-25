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
    public class AxisVisualizer : MonoBehaviour
    {
        //Public Variables:
        public float length = 0.09f;
        public float width = .003f;

        //Update:
        private void Update()
        {
            Lines.DrawRay($"Forward{GetInstanceID()}", Color.blue, Color.black, transform.position, transform.forward * length, width);
            Lines.DrawRay($"Right_{GetInstanceID()}", Color.red, Color.black, transform.position, transform.right * length, width);
            Lines.DrawRay($"Up_{GetInstanceID()}", Color.green, Color.black, transform.position, transform.up * length, width);
        }

        //Flow:
        private void OnEnable()
        {
            Lines.SetVisibility($"Forward{GetInstanceID()}", true);
            Lines.SetVisibility($"Right_{GetInstanceID()}", true);
            Lines.SetVisibility($"Up_{GetInstanceID()}", true);
        }

        private void OnDisable()
        {
            Lines.SetVisibility($"Forward{GetInstanceID()}", false);
            Lines.SetVisibility($"Right_{GetInstanceID()}", false);
            Lines.SetVisibility($"Up_{GetInstanceID()}", false);
        }

        //Gizmos:
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * length);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.right * length);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.up * length);
        }
    }
}