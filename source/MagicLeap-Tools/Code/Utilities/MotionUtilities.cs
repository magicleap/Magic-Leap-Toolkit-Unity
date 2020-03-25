// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicLeapTools
{
    public static class MotionUtilities
    {
        public static Quaternion SmoothDamp(Quaternion current, Quaternion target, ref Quaternion velocity, float duration)
        {
            //double-cover correction:
            float dot = Quaternion.Dot(current, target);
            float multi = dot > 0f ? 1f : -1f;
            target.x *= multi;
            target.y *= multi;
            target.z *= multi;
            target.w *= multi;

            //smooth damp:
            Vector4 smoothDamped = Vector4.Normalize(new Vector4(
                Mathf.SmoothDamp(current.x, target.x, ref velocity.x, duration),
                Mathf.SmoothDamp(current.y, target.y, ref velocity.y, duration),
                Mathf.SmoothDamp(current.z, target.z, ref velocity.z, duration),
                Mathf.SmoothDamp(current.w, target.w, ref velocity.w, duration)
            ));

            //velocities:
            var dtInv = 1f / Time.deltaTime;
            velocity.x = (smoothDamped.x - current.x) * dtInv;
            velocity.y = (smoothDamped.y - current.y) * dtInv;
            velocity.z = (smoothDamped.z - current.z) * dtInv;
            velocity.w = (smoothDamped.w - current.w) * dtInv;

            return new Quaternion(smoothDamped.x, smoothDamped.y, smoothDamped.z, smoothDamped.w);
        }
    }
}