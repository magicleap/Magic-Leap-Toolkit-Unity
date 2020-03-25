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
    public static class TransformUtilities
    {
        //Private Variables:
        private static float _nearClipBuffer = .05f;
        private static Camera _mainCamera;

        //Private Properties:
        private static Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }
                return _mainCamera;
            }
        }

        private static Plane CameraPlane
        {
            get
            {
                return new Plane(MainCamera.transform.forward, MainCamera.transform.position + MainCamera.transform.forward * (MainCamera.nearClipPlane + _nearClipBuffer));
            }
        }

        //Public Methods:
        public static bool InsideClipPlane(Vector3 location)
        {
            return !CameraPlane.GetSide(location);
        }

        public static Vector3 LocationOnClipPlane(Vector3 location)
        {
            return CameraPlane.ClosestPointOnPlane(location);
        }

        public static float DistanceInsideClipPlane(Vector3 location)
        {
            return Vector3.Distance(LocationOnClipPlane(location), location);
        }

        /// <summary>
        /// Equivalent to Transform.InverseTransformPoint - from world space to local space.
        /// </summary>
        public static Vector3 LocalPosition(Vector3 worldPosition, Quaternion worldRotation, Vector3 targetWorldPosition)
        {
            worldRotation.Normalize();
            Matrix4x4 trs = Matrix4x4.TRS(worldPosition, worldRotation, Vector3.one);
            return trs.inverse.MultiplyPoint3x4(targetWorldPosition);
        }

        /// <summary>
        /// Equivalent to Transform.TransformPoint - from local space to world space.
        /// </summary>
        public static Vector3 WorldPosition(Vector3 worldPosition, Quaternion worldRotation, Vector3 localPosition)
        {
            worldRotation.Normalize();
            Matrix4x4 trs = Matrix4x4.TRS(worldPosition, worldRotation, Vector3.one);
            return trs.MultiplyPoint3x4(localPosition);
        }

        public static Quaternion GetRotationOffset(Quaternion from, Quaternion to)
        {
            from.Normalize();
            return Quaternion.Inverse(from) * to;
        }

        public static Quaternion ApplyRotationOffset(Quaternion from, Quaternion offset)
        {
            from.Normalize();
            return from * offset;
        }

        public static Quaternion RotateQuaternion(Quaternion rotation, Vector3 amount)
        {
            return Quaternion.AngleAxis(amount.x, rotation * Vector3.right) *
                Quaternion.AngleAxis(amount.y, rotation * Vector3.up) *
                Quaternion.AngleAxis(amount.z, rotation * Vector3.forward) *
                rotation;
        }
    }
}