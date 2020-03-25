// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using MagicLeapTools;

public class PlayspaceExample : MonoBehaviour
{
#if PLATFORM_LUMIN
    //Public Variables:
    public Transform primaryWallPlaque;
    public Transform rearWallPlaque;
    public Transform rightWallPlaque;
    public Transform leftWallPlaque;
    public Transform centerPlaque;
    public Transform floorPlaque;
    public Transform ceilingPlaque;
        
    //Init:
    private void Awake()
    {
        //hooks:
        Playspace.Instance.OnCleared.AddListener(HandleCleared);
        Playspace.Instance.OnCompleted.AddListener(HandleCompleted);
    }

    private void HandleCleared()
    {
        primaryWallPlaque.gameObject.SetActive(false);
        rearWallPlaque.gameObject.SetActive(false);
        rightWallPlaque.gameObject.SetActive(false);
        leftWallPlaque.gameObject.SetActive(false);
        ceilingPlaque.gameObject.SetActive(false);
        floorPlaque.gameObject.SetActive(false);
        centerPlaque.gameObject.SetActive(false);
    }

    private void HandleCompleted()
    {
        //place plaques:
        PlayspaceWall primaryWall = Playspace.Instance.Walls[Playspace.Instance.PrimaryWall];
        primaryWallPlaque.gameObject.SetActive(true);
        primaryWallPlaque.position = primaryWall.Center + Vector3.up * .5f;
        primaryWallPlaque.rotation = Quaternion.LookRotation(primaryWall.Back);

        PlayspaceWall rearWall = Playspace.Instance.Walls[Playspace.Instance.RearWall];
        rearWallPlaque.gameObject.SetActive(true);
        rearWallPlaque.position = rearWall.Center + Vector3.up * .5f;
        rearWallPlaque.rotation = Quaternion.LookRotation(rearWall.Back);

        PlayspaceWall rightWall = Playspace.Instance.Walls[Playspace.Instance.RightWall];
        rightWallPlaque.gameObject.SetActive(true);
        rightWallPlaque.position = rightWall.Center + Vector3.up * .5f;
        rightWallPlaque.rotation = Quaternion.LookRotation(rightWall.Back);

        PlayspaceWall leftWall = Playspace.Instance.Walls[Playspace.Instance.LeftWall];
        leftWallPlaque.gameObject.SetActive(true);
        leftWallPlaque.position = leftWall.Center + Vector3.up * .5f;
        leftWallPlaque.rotation = Quaternion.LookRotation(leftWall.Back);

        ceilingPlaque.gameObject.SetActive(true);
        ceilingPlaque.position = Playspace.Instance.CeilingCenter;
        ceilingPlaque.rotation = Quaternion.LookRotation(Vector3.up, primaryWall.Forward);

        floorPlaque.gameObject.SetActive(true);
        floorPlaque.position = Playspace.Instance.FloorCenter;
        floorPlaque.rotation = Quaternion.LookRotation(Vector3.down, primaryWall.Back);

        centerPlaque.gameObject.SetActive(true);
        centerPlaque.position = Playspace.Instance.Center;
    }
#endif
}