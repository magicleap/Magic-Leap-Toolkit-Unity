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
    public class SpatialAlignmentHistory
    {
        //Public Variables:
        public List<Vector3> positions = new List<Vector3>();
        public List<Quaternion> rotations = new List<Quaternion>();
        public int interval;

        //Public Properties:
        public Vector3 AveragePosition
        {
            get
            {
                Vector3 tempPositionAverage = Vector3.zero;

                foreach (var item in positions)
                {
                    tempPositionAverage += item;
                }

                return tempPositionAverage / positions.Count;
            }
        }

        public Quaternion AverageRotation
        {
            get
            {
                //rotation:
                float x = 0;
                float y = 0;
                float z = 0;
                float w = 0;

                foreach (var item in rotations)
                {
                    x += item.x;
                    y += item.y;
                    z += item.z;
                    w += item.w;
                }
                float k = 1 / Mathf.Sqrt(x * x + y * y + z * z + w * w);
                return new Quaternion(x * k, y * k, z * k, w * k);
            }
        }

        //Constructors:
        public SpatialAlignmentHistory(int interval)
        {
            this.interval = interval;
        }

        //Public Methods:
        public void Clear()
        {
            positions.Clear();
            rotations.Clear();
        }
    }
}