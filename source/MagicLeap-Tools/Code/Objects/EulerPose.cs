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
    /// A compliment to Unity's built-in Pose class but for using a Vector3 for the rotation.
    /// </summary>
    [System.Serializable]
    public class EulerPose
    {
        //Public Variables:
        public Vector3 position;
        public Vector3 rotation;

        //Constructors:
        public EulerPose(Vector3 position, Vector3 rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}