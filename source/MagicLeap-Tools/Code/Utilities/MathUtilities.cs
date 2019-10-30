// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;
using System;

namespace MagicLeapTools
{
    public static class MathUtilities
    {
        /// <summary>
        /// Returns a strongly rare randomized 10 digit ID. 
        /// </summary>
        /// <returns></returns>
        public static string UniqueID()
        {
            string output = "";
            string guid = Guid.NewGuid().ToString();
            string[] segments = guid.Split('-');

            //translate each "chunk" of a guid into a single number:
            foreach (var segment in segments)
            {
                int part = 0;
                foreach (char piece in segment)
                {
                    //change letters to numbers:
                    if (Char.IsLetter(piece))
                    {
                        part += (int)piece % 32;
                    }
                    else
                    {
                        part += (int)part;
                    }
                }

                //loop to keep range between 0-9:
                output += part % 10;
            }

            //pad a few extra random numbers to get us to 10:
            for (int i = 0; i < 5; i++)
            {
                output += Mathf.Round(UnityEngine.Random.value * 10) % 10;
            }

            return output;
        }
    }
}