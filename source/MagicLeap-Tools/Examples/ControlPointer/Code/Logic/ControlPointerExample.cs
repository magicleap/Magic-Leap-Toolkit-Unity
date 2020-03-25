// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using MagicLeapTools;

public class ControlPointerExample : MonoBehaviour
{
#if PLATFORM_LUMIN
    //Public Variables:
    public PlaceInFront contentRoot;
    public ControlInput controlInput;

    //Init:
    private void Awake()
    {
        //hooks:
        controlInput.OnHomeButtonTap.AddListener(HandleHomeButton);
    }

    //Event Handlers:
    private void HandleHomeButton()
    {
        contentRoot.Place();
    }
#endif
}