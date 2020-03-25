// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLeapTools
{
    [CustomEditor(typeof(RampLight))]
    public class RampLightEditor : Editor
    {
        //Private Variables:
        private float _size = .25f;
        private float _visualRayCount = 8;
        private float _visualRayLength = 2;
        private Color _color = Color.yellow;

        //Handles:
        protected virtual void OnSceneGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                Handles.color = _color;

                //refs:
                Transform transform = ((RampLight)target).transform;

                //directional circle:
                Handles.CircleHandleCap(
                    0,
                    transform.position,
                    transform.rotation * Quaternion.LookRotation(Vector3.forward),
                    HandleUtility.GetHandleSize(transform.position) * _size,
                    EventType.Repaint
                );

                //draw rays:
                for (int i = 0; i < _visualRayCount; i++)
                {
                    float degrees = (i / _visualRayCount) * 360;
                    float radians = degrees * Mathf.Deg2Rad;
                    float x = Mathf.Cos(radians);
                    float y = Mathf.Sin(radians);
                    Vector3 pos = new Vector3(x, y, 0) * (HandleUtility.GetHandleSize(transform.position) * _size);
                    pos = transform.TransformPoint(pos);
                    Handles.DrawLine(pos, pos + (transform.forward * (HandleUtility.GetHandleSize(transform.position) * _visualRayLength)));
                }

            }
        }
    }
}