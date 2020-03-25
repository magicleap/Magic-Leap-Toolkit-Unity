// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System;
using UnityEngine;

namespace MagicLeapTools
{
    public enum TransmissionMessageType
    {
        GlobalStringsRequestMessage,
        GlobalFloatsRequestMessage,
        GlobalBoolsRequestMessage,
        GlobalVector2RequestMessage,
        GlobalVector3RequestMessage,
        GlobalVector4RequestMessage,
        HeartbeatMessage,
        ConfirmedMessage,
        AwakeMessage,
        DespawnMessage,
        GlobalBoolChangedMessage,
        GlobalBoolsRecapMessage,
        GlobalFloatChangedMessage,
        GlobalFloatsRecapMessage,
        GlobalStringChangedMessage,
        GlobalStringsRecapMessage,
        GlobalVector2ChangedMessage,
        GlobalVector2RecapMessage,
        GlobalVector3ChangedMessage,
        GlobalVector3RecapMessage,
        GlobalVector4ChangedMessage,
        GlobalVector4RecapMessage,
        OnDisabledMessage,
        OnEnabledMessage,
        OwnershipTransferenceDeniedMessage,
        OwnershipTransferenceGrantedMessage,
        OwnershipTransferenceRequestMessage,
        SpawnMessage,
        SpawnRecapMessage,
        TransformSyncMessage,
        BoolArrayMessage,
        BoolMessage,
        ByteArrayMessage,
        ColorArrayMessage,
        ColorMessage,
        FloatArrayMessage,
        FloatMessage,
        PoseArrayMessage,
        PoseMessage,
        QuaternionArrayMessage,
        QuaternionMessage,
        RPCMessage,
        StringArrayMessage,
        StringMessage,
        Vector2ArrayMessage,
        Vector2Message,
        Vector3ArrayMessage,
        Vector3Message,
        Vector4ArrayMessage,
        Vector4Message,
        SpatialAlignmentMessage
    }

    public class TransmissionMessage
    {
        //Public Variables (truncated to reduce packet size):
        /// <summary>
        /// to
        /// </summary>
        public string t;
        /// <summary>
        /// from
        /// </summary>
        public string f;
        /// <summary>
        /// guid
        /// </summary>
        public string g;
        /// <summary>
        /// reliable
        /// </summary>
        public int r;
        /// <summary>
        /// targets
        /// </summary>
        public int ts;
        /// <summary>
        /// time
        /// </summary>
        public double ti;
        /// <summary>
        /// data
        /// </summary>
        public string d;
        /// <summary>
        /// type
        /// </summary>
        public short ty;
        /// <summary>
        /// appKey
        /// </summary>
        public string a;
        /// <summary>
        /// privatekey
        /// </summary>
        public string p;

        //Constructors:
        public TransmissionMessage(TransmissionMessageType type, TransmissionAudience audience, string targetAddress = "", bool reliable = false, string data = "", string guid = "")
        {
            switch (audience)
            {
                case TransmissionAudience.SinglePeer:
                    r = reliable ? 1 : 0;
                    t = targetAddress;
                    break;

                case TransmissionAudience.KnownPeers:
                    r = reliable ? 1 : 0;
                    t = "";
                    break;

                case TransmissionAudience.NetworkBroadcast:
                    r = 0;
                    t = "255.255.255.255";
                    break;
            }

            //guids are only required for reliable messages:
            if (reliable && string.IsNullOrEmpty(guid))
            {
                g = Guid.NewGuid().ToString();
            }
            else
            {
                g = guid;
            }

            
            f = NetworkUtilities.MyAddress;
            ti = Math.Round(Time.realtimeSinceStartup, 3);
            d = data;
            ty = (short)type;
            a = Transmission.Instance.appKey;
            p = Transmission.Instance.privateKey;
        }
    }
}