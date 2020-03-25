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
using UnityEngine.XR.MagicLeap;
using MagicLeapTools;

public class HandInputExample : MonoBehaviour
{
#if PLATFORM_LUMIN
    //Public Variables:
    public PlaceInFront contentRoot;

    //Private Classes:
    private class TransformStatus
    {
        //Public Variables:
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        //Constructors:
        public TransformStatus(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    //Private Variables:
    private Dictionary<Transform, TransformStatus> _initialTransform = new Dictionary<Transform, TransformStatus>();
    private float _thumbsUpVerificationTime = .5f;

    //Init:
    private void Awake()
    {
        //cache:
        foreach (Transform item in contentRoot.transform)
        {
            _initialTransform.Add(item, new TransformStatus(item.localPosition, item.localRotation, item.localScale));
        }

        //hooks:
        HandInput.OnReady += HandleHandInputReady;
    }

    //Public Variables:
    public void ResetContent()
    {
        foreach (var item in contentRoot.GetComponentsInChildren<Rigidbody>())
        {
            //stop physics:
            item.velocity = Vector3.zero;
            item.angularVelocity = Vector3.zero;
        }

        foreach (var item in _initialTransform)
        {
            item.Key.localPosition = item.Value.position;
            item.Key.localRotation = item.Value.rotation;
            item.Key.localScale = item.Value.scale;
        }
    }

    //Event Handlers:
    private void HandleHandInputReady()
    {
        //hooks:
        HandInput.Right.Gesture.OnKeyPoseChanged += HandleKeyPoseChanged;
        HandInput.Left.Gesture.OnKeyPoseChanged += HandleKeyPoseChanged;
    }

    private void HandleKeyPoseChanged(ManagedHand hand, MLHandTracking.HandKeyPose pose)
    {
        //both hands holding thumbs?
        if (HandInput.Left.Hand.KeyPose == MLHandTracking.HandKeyPose.Thumb && HandInput.Right.Hand.KeyPose == MLHandTracking.HandKeyPose.Thumb)
        {
            StartCoroutine("ThumbsUpReset");
        }
        else
        {
            StopCoroutine("ThumbsUpReset");
        }
    }

    //Coroutines:
    private IEnumerator ThumbsUpReset()
    {
        yield return new WaitForSeconds(_thumbsUpVerificationTime);
        contentRoot.Place();
        ResetContent();
    }
#endif
}