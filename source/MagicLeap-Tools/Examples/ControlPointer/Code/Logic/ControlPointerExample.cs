// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeapTools;

public class ControlPointerExample : MonoBehaviour
{
#if PLATFORM_LUMIN
    //Public Variables:
    public Transform content;
    public ControlInput controlInput;

    //Private Variables:
    private Camera _mainCamera;

    //Init:
    private void Awake()
    {
        //refs:
        _mainCamera = Camera.main;

        //hooks:
        controlInput.OnHomeButtonTap.AddListener(HandleHomeButton);
    }

    //Event Handlers:
    private void HandleHomeButton()
    {
        //orient the content in front of the user:
        content.transform.position = _mainCamera.transform.position + _mainCamera.transform.forward;
        Vector3 to = content.transform.position - _mainCamera.transform.position;
        Vector3 flat = Vector3.ProjectOnPlane(to, Vector3.up);
        content.transform.rotation = Quaternion.LookRotation(flat);
    }
#endif
}