// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

namespace MagicLeapTools
{
    public class RPCMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// methodToCall
        /// </summary>
        public string m;
        /// <summary>
        /// parameter
        /// </summary>
        public string pa;

        //Constructors:
        public RPCMessage(string methodToCall, string parameter = "", string data = "", TransmissionAudience audience = TransmissionAudience.KnownPeers, string targetAddress = "") : base(TransmissionMessageType.RPCMessage, audience, targetAddress, true, data)
        {
            m = methodToCall;
            pa = parameter;
        }
    }
}