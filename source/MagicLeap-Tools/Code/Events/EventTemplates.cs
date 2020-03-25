// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using System;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeapTools
{
#if PLATFORM_LUMIN
    [System.Serializable]
    public class TouchpadGestureDirectionEvent : UnityEvent<MLInput.Controller.TouchpadGesture.GestureDirection>
    {
    }
#endif

    [System.Serializable]
    public class PeerFoundEvent : UnityEvent<string, long>
    {
    }

    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider>
    {
    }

    [System.Serializable]
    public class CollisionEvent : UnityEvent<Collision>
    {
    }

    [System.Serializable]
    public class InteractionPointEvent : UnityEvent<InteractionPoint>
    {
    }

    [System.Serializable]
    public class InteractionPointDragEvent : UnityEvent<InteractionPoint[], Vector3, Quaternion, float>
    {
    }

    [System.Serializable]
    public class SpatialAlignmentMsgEvent : UnityEvent<string>
    {
    }

    [System.Serializable]
    public class PoseEvent : UnityEvent<Pose>
    {
    }

    [System.Serializable]
    public class PoseArrayEvent : UnityEvent<Pose[]>
    {
    }

    [System.Serializable]
    public class PoseMsgEvent : UnityEvent<PoseMessage>
    {
    }

    [System.Serializable]
    public class PoseArrayMsgEvent : UnityEvent<PoseArrayMessage>
    {
    }

    [System.Serializable]
    public class ByteArrayMsgEvent : UnityEvent<ByteArrayMessage>
    {
    }

    [System.Serializable]
    public class ByteArrayEvent : UnityEvent<byte[]>
    {
    }

    [System.Serializable]
    public class TransmissionObjectMsgEvent : UnityEvent<TransmissionObject>
    {
    }

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool>
    {
    }

    [System.Serializable]
    public class BoolArrayEvent : UnityEvent<bool[]>
    {
    }

    [System.Serializable]
    public class BoolMsgEvent : UnityEvent<BoolMessage>
    {
    }

    [System.Serializable]
    public class BoolArrayMsgEvent : UnityEvent<BoolArrayMessage>
    {
    }

    [System.Serializable]
    public class StringEvent : UnityEvent<string>
    {
    }

    [System.Serializable]
    public class StringArrayEvent : UnityEvent<string[]>
    {
    }

    [System.Serializable]
    public class StringMsgEvent : UnityEvent<StringMessage>
    {
    }

    [System.Serializable]
    public class StringArrayMsgEvent : UnityEvent<StringArrayMessage>
    {
    }

    [System.Serializable]
    public class Vector2Event : UnityEvent<Vector2>
    {
    }

    [System.Serializable]
    public class Vector2ArrayEvent : UnityEvent<Vector2[]>
    {
    }

    [System.Serializable]
    public class Vector2MsgEvent : UnityEvent<Vector2Message>
    {
    }

    [System.Serializable]
    public class Vector2ArrayMsgEvent : UnityEvent<Vector2ArrayMessage>
    {
    }

    [System.Serializable]
    public class Vector3Event : UnityEvent<Vector3>
    {
    }

    [System.Serializable]
    public class Vector3ArrayEvent : UnityEvent<Vector3[]>
    {
    }

    [System.Serializable]
    public class Vector3MsgEvent : UnityEvent<Vector3Message>
    {
    }

    [System.Serializable]
    public class Vector3ArrayMsgEvent : UnityEvent<Vector3ArrayMessage>
    {
    }

    [System.Serializable]
    public class ColorEvent : UnityEvent<Color>
    {
    }

    [System.Serializable]
    public class ColorArrayEvent : UnityEvent<Color[]>
    {
    }

    [System.Serializable]
    public class ColorMsgEvent : UnityEvent<ColorMessage>
    {
    }

    [System.Serializable]
    public class ColorArrayMsgEvent : UnityEvent<ColorArrayMessage>
    {
    }

    [System.Serializable]
    public class Vector4Event : UnityEvent<Vector4>
    {
    }

    [System.Serializable]
    public class Vector4ArrayEvent : UnityEvent<Vector4[]>
    {
    }

    [System.Serializable]
    public class Vector4MsgEvent : UnityEvent<Vector4Message>
    {
    }

    [System.Serializable]
    public class Vector4ArrayMsgEvent : UnityEvent<Vector4ArrayMessage>
    {
    }

    [System.Serializable]
    public class FloatEvent : UnityEvent<float>
    {
    }

    [System.Serializable]
    public class FloatArrayEvent : UnityEvent<float[]>
    {
    }

    [System.Serializable]
    public class FloatMsgEvent : UnityEvent<FloatMessage>
    {
    }

    [System.Serializable]
    public class FloatArrayMsgEvent : UnityEvent<FloatArrayMessage>
    {
    }

    [System.Serializable]
    public class GameObjectEvent : UnityEvent<GameObject>
    {
    }

    [System.Serializable]
    public class GameObjectArrayEvent : UnityEvent<GameObject[]>
    {
    }

    [System.Serializable]
    public class QuaternionEvent : UnityEvent<Quaternion>
    {
    }

    [System.Serializable]
    public class QuaternionArrayEvent : UnityEvent<Quaternion[]>
    {
    }

    [System.Serializable]
    public class QuaternionMsgEvent : UnityEvent<QuaternionMessage>
    {
    }

    [System.Serializable]
    public class QuaternionArrayMsgEvent : UnityEvent<QuaternionArrayMessage>
    {
    }

    [System.Serializable]
    public class FloatGameObjectEvent : UnityEvent<float, GameObject>
    {
    }

    [System.Serializable]
    public class CollisionGameObjectEvent : UnityEvent<Collision, GameObject>
    {
    }

    [System.Serializable]
    public class TransformSyncMsgEvent : UnityEvent<TransformSyncMessage>
    {
    }
}