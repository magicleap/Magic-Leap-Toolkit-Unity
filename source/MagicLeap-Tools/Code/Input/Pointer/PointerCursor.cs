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

namespace MagicLeapTools
{
    public class PointerCursor : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public Pointer pointer;

        //Init:
        private void Reset()
        {
            //refs:
            pointer = transform.parent.GetComponent<Pointer>();
        }

        //Loops:
        private void LateUpdate()
        {
            if (pointer == null)
            {
                return;
            }

            //apply:
            transform.position = pointer.Tip;
            transform.rotation = Quaternion.LookRotation(pointer.Normal);
        }
#endif
    }
}