// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if PLATFORM_LUMIN
using static UnityEngine.XR.MagicLeap.Native.MagicLeapNativeBindings;
#endif

namespace MagicLeapTools
{
    //Public Methods:
    public static class SerializationUtilities
    {
        public static T Deserialize<T>(string array)
        {
            if (typeof(T) == typeof(int[]))
            {
                string[] input = array.Split('~');
                int[] output = Array.ConvertAll(input, int.Parse);
                return (T)Convert.ChangeType(output, typeof(T));
            }

            if (typeof(T) == typeof(Vector3[]))
            {
                string[] input = array.Split('~');
                List<Vector3> output = new List<Vector3>();
                for (int i = 0; i < input.Length; i++)
                {
                    output.Add(JsonUtility.FromJson<Vector3>(input[i]));
                }
                return (T)Convert.ChangeType(output.ToArray(), typeof(T));
            }

            if (typeof(T) == typeof(PlayspaceWall[]))
            {
                string[] input = array.Split('~');
                List<PlayspaceWall> output = new List<PlayspaceWall>();
                for (int i = 0; i < input.Length; i++)
                {
                    output.Add(JsonUtility.FromJson<PlayspaceWall>(input[i]));
                }
                return (T)Convert.ChangeType(output.ToArray(), typeof(T));
            }

            return default(T);
        }

        public static string Serialize(int[] array)
        {
            string output = "";
            for (int i = 0; i < array.Length; i++)
            {
                output += array[i];
                if (i < array.Length - 1)
                {
                    output += "~";
                }
            }
            return output;
        }

        public static string Serialize(Vector3[] array)
        {
            string output = "";
            for (int i = 0; i < array.Length; i++)
            {
                output += JsonUtility.ToJson(array[i]);
                if (i < array.Length - 1)
                {
                    output += "~";
                }
            }

            return output;
        }

        public static string Serialize(PlayspaceWall[] array)
        {
            string output = "";
            for (int i = 0; i < array.Length; i++)
            {
                output += JsonUtility.ToJson(array[i]);
                if (i < array.Length - 1)
                {
                    output += "~";
                }
            }

            return output;
        }

#if PLATFORM_LUMIN
        public static MLCoordinateFrameUID StringToCFUID(string cfuid)
        {
            Guid guid = Guid.Parse(cfuid);
            string guidString = guid.ToString("N");
            ulong flippedFirst = ulong.Parse(guidString.Substring(0, 16), System.Globalization.NumberStyles.HexNumber);
            ulong flippedSecond = ulong.Parse(guidString.Substring(16, 16), System.Globalization.NumberStyles.HexNumber);
            byte[] bytes = BitConverter.GetBytes(flippedFirst);
            FlipGuidComponents(bytes);
            ulong first = BitConverter.ToUInt64(bytes, 0);
            bytes = BitConverter.GetBytes(flippedSecond);
            FlipGuidComponents(bytes);
            ulong second = BitConverter.ToUInt64(bytes, 0);
            MLCoordinateFrameUID outCfuid = new MLCoordinateFrameUID();
            outCfuid.First = first;
            outCfuid.Second = second;
            return outCfuid;
        }
#endif

        //Private Methods:
        private static void FlipGuidComponents(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
        }
    }
}