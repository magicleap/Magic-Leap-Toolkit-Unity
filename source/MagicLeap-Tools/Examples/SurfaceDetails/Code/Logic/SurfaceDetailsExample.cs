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
using UnityEngine.UI;
using MagicLeapTools;

public class SurfaceDetailsExample : MonoBehaviour
{
    //Public Variables:
    public Transform controllerPose;
    public Transform cursor;
    [Tooltip("The distance to separate the cursor from a collision to avoid occlusion.")]
    public float surfaceOffset = 0.0508f;
    public Text status;

    //Loops:
    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(controllerPose.position, controllerPose.forward, out hit))
        {
            //adjust cursor:
            cursor.gameObject.SetActive(true);
            cursor.position = hit.point + hit.normal * surfaceOffset;
            cursor.rotation = Quaternion.LookRotation(hit.normal);

            //handle the surface we are hitting:
            switch (SurfaceDetails.Analyze(hit))
            {
                case SurfaceType.Ceiling:
                    status.text = "THE CEILING";
                    break;

                case SurfaceType.Floor:
                    status.text = "THE FLOOR";
                    break;

                case SurfaceType.Seat:
                    status.text = "A SEAT";
                    break;

                case SurfaceType.Table:
                    status.text = "A TABLE TOP";
                    break;

                case SurfaceType.Underside:
                    status.text = "UNDERSIDE";
                    break;

                case SurfaceType.Wall:
                    status.text = "A WALL";
                    break;
            }
        }
        else
        {
            //adjust cursor:
            cursor.gameObject.SetActive(false);

            status.text = "NOTHING!";
        }
    }
}