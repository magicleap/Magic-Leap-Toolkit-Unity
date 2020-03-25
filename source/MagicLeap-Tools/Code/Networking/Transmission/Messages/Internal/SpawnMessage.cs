// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using System;

namespace MagicLeapTools
{
    public class SpawnMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// resourceFileName
        /// </summary>
        public string rf;
        /// <summary>
        /// instanceGUID
        /// </summary>
        public string i;
        /// <summary>
        /// position x
        /// </summary>
        public double px;
        /// <summary>
        /// position y
        /// </summary>
        public double py;
        /// <summary>
        /// position z
        /// </summary>
        public double pz;
        /// <summary>
        /// rotation x
        /// </summary>
        public double rx;
        /// <summary>
        /// rotation y
        /// </summary>
        public double ry;
        /// <summary>
        /// rotation z
        /// </summary>
        public double rz;
        /// <summary>
        /// rotation w
        /// </summary>
        public double rw;
        /// <summary>
        /// scale x
        /// </summary>
        public double sx;
        /// <summary>
        /// scale y
        /// </summary>
        public double sy;
        /// <summary>
        /// scale z
        /// </summary>
        public double sz;

        //Constructors:
        public SpawnMessage(string resourceFileName, string instanceGuid, Vector3 position, Quaternion rotation, Vector3 scale) : base(TransmissionMessageType.SpawnMessage, TransmissionAudience.KnownPeers, "", true)
        {
            rf = resourceFileName;
            i = instanceGuid;

            //truncate precision:
            px = Math.Round(position.x, 3);
            py = Math.Round(position.y, 3);
            pz = Math.Round(position.z, 3);
            rx = Math.Round(rotation.x, 3);
            ry = Math.Round(rotation.y, 3);
            rz = Math.Round(rotation.z, 3);
            rw = Math.Round(rotation.w, 3);
            sx = Math.Round(scale.x, 3);
            sy = Math.Round(scale.y, 3);
            sz = Math.Round(scale.z, 3);
        }
    }
}