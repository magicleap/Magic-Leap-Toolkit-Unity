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
using MagicLeapTools;

public class SurfaceDetailsExample : MonoBehaviour
{
#if PLATFORM_LUMIN
    //Public Variables:
    public Transform controllerPose;
    public Transform cursor;
    [Tooltip("The distance to separate the cursor from a collision to avoid occlusion.")]
    public float surfaceOffset = 0.0254f;
    public Text status;
    public Transform ceiling;
    public Transform floor;

    //Private Variables:
    private Transform _camera;

    //Init:
    private void Awake()
    {
        _camera = Camera.main.transform;
    }

    //Loops:
    private void Update()
    {
        //used to orient the ceiling and floor visuals:
        Vector3 flatForward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up);
        
        //set ceiling and floor visuals:
        if (SurfaceDetails.CeilingFound)
        {
            ceiling.position = new Vector3(_camera.position.x, SurfaceDetails.CeilingHeight, _camera.position.z);
            ceiling.rotation = Quaternion.LookRotation(Vector3.up, flatForward);
        }

        if (SurfaceDetails.FloorFound)
        {
            floor.position = new Vector3(_camera.position.x, SurfaceDetails.FloorHeight, _camera.position.z);
            floor.rotation = Quaternion.LookRotation(Vector3.down, flatForward);
        }

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
#endif
}