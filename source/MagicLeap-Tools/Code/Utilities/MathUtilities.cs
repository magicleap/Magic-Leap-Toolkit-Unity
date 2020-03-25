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

        /// <summary>
        /// Provides the percentage between two points.
        /// </summary>
        /// <returns></returns>
        public static float TraveledPercentage(Vector3 start, Vector3 end, Vector3 position)
        {
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            direction.Normalize();
            Vector3 offset = position - start;
            return Vector3.Dot(offset, direction) / distance;
        }

        /// <summary>
        /// Are two line segments intersecting if they are projected on the ground?
        /// </summary>
        public static bool LineSegmentsIntersecting(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, bool shouldIncludeEndPoints)
        {
            //pieces:
            float epsilon = 0.00001f;
            bool intersecting = false;
            float denominator = (b2.z - b1.z) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.z - a1.z);

            //make sure the denominator is > 0, if not the lines are parallel:
            if (denominator != 0f)
            {
                float u_a = ((b2.x - b1.x) * (a1.z - b1.z) - (b2.z - b1.z) * (a1.x - b1.x)) / denominator;
                float u_b = ((a2.x - a1.x) * (a1.z - b1.z) - (a2.z - a1.z) * (a1.x - b1.x)) / denominator;

                //are the line segments intersecting if the end points are the same:
                if (shouldIncludeEndPoints)
                {
                    //is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1:
                    if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
                    {
                        intersecting = true;
                    }
                }
                else
                {
                    //is intersecting if u_a and u_b are between 0 and 1:
                    if (u_a > 0f + epsilon && u_a < 1f - epsilon && u_b > 0f + epsilon && u_b < 1f - epsilon)
                    {
                        intersecting = true;
                    }
                }
            }

            return intersecting;
        }
        
        /// <summary>
        /// Finds the intersection of two infinite rays projected on the ground. Y of found intersection will use the one from the origin of the "a" ray.
        /// </summary>
        public static bool RayIntersection(Ray a, Ray b, ref Vector3 intersection)
        {
            //flatten directions:
            a.direction = Vector3.ProjectOnPlane(a.direction, Vector3.up).normalized;
            b.direction = Vector3.ProjectOnPlane(b.direction, Vector3.up).normalized;

            //points on line:
            Vector3 a2 = a.GetPoint(1);
            Vector3 b2 = b.GetPoint(1);

            //pieces:
            float denominator = (b2.x - b.origin.x) * (a2.z - a.origin.z) - (b2.z - b.origin.z) * (a2.x - a.origin.x);

            //parallel?
            if (denominator == 0)
            {
                return false;
            }

            //find intersection:
            float u = ((a.origin.x - b.origin.x) * (a2.z - a.origin.z) - (a.origin.z - b.origin.z) * (a2.x - a.origin.x)) / denominator;
            float x = b.origin.x + (b2.x - b.origin.x) * u;
            float y = b.origin.z + (b2.z - b.origin.z) * u;
            intersection = new Vector3(x, a.origin.y, y);

            return true;
        }
    }
}