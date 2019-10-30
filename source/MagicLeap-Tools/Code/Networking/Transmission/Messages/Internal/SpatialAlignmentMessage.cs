// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicLeapTools
{
    public class SpatialAlignmentMessage : TransmissionMessage
    {
        //Constructors:
        public SpatialAlignmentMessage(string targetAddress) : base(TransmissionMessageType.SpatialAlignmentMessage, TransmissionAudience.SinglePeer, targetAddress, true)
        {
        }
    }
}