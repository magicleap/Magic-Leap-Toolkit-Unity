// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

namespace MagicLeapTools
{
    public class DespawnMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// instanceGUID
        /// </summary>
        public string ig;

        //Constructors:
        public DespawnMessage(string instanceGuid) : base(TransmissionMessageType.DespawnMessage, TransmissionAudience.KnownPeers, "", true)
        {
            ig = instanceGuid;
        }
    }
}