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
using UnityEngine.UI;

public class GuiFadeIn : MonoBehaviour
{
    //Flow:
    private void OnEnable()
    {
        foreach (var item in GetComponentsInChildren<Graphic>())
        {
            item.CrossFadeAlpha(0, 0, true);
            item.CrossFadeAlpha(1, 1, true);
        }
    }
}