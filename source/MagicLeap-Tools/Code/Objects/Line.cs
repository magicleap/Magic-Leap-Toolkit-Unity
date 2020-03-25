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
    [System.Serializable]
    public class Line
    {
        //Public Variables:
        public Vector3 start;
        public Vector3 end;

        //Public Properties:
        public Vector3 Vector
        {
            get
            {
                return end - start;
            }
        }

        //Constructors:
        public Line(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
        }
    }
}