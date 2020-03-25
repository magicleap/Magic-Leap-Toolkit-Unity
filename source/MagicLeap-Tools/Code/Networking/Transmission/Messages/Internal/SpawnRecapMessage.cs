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
    public class SpawnRecapMessage : TransmissionMessage
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
        public SpawnRecapMessage(TransmissionObject transmissionObject, string destination) : base(TransmissionMessageType.SpawnRecapMessage, TransmissionAudience.SinglePeer, destination, true)
        {
            //different remote prefab?
            if (transmissionObject.remotePrefab != null)
            {
                rf = transmissionObject.remotePrefab.name;
            }
            else
            {
                rf = transmissionObject.resourceFileName;
            }

            i = transmissionObject.guid;

            //truncate precision:
            px = Math.Round(transmissionObject.transform.position.x, 3);
            py = Math.Round(transmissionObject.transform.position.y, 3);
            pz = Math.Round(transmissionObject.transform.position.z, 3);
            rx = Math.Round(transmissionObject.transform.rotation.x, 3);
            ry = Math.Round(transmissionObject.transform.rotation.y, 3);
            rz = Math.Round(transmissionObject.transform.rotation.z, 3);
            rw = Math.Round(transmissionObject.transform.rotation.w, 3);
            sx = Math.Round(transmissionObject.transform.localScale.x, 3);
            sy = Math.Round(transmissionObject.transform.localScale.y, 3);
            sz = Math.Round(transmissionObject.transform.localScale.z, 3);
        }
    }
}