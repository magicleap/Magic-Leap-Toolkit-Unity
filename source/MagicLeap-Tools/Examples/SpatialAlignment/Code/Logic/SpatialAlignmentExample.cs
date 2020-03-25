// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using MagicLeapTools;
using UnityEngine.UI;

public class SpatialAlignmentExample : MonoBehaviour
{
    //Public Variables:
    public ControlInput controlLocator;
    public Text info;

#if PLATFORM_LUMIN
    //Private Variables:
    private List<TransmissionObject> _spawned = new List<TransmissionObject>();
    private string _initialInfo;

    //Init:
    private void Awake()
    {
        //hooks:
        controlLocator.OnTriggerDown.AddListener(HandleTriggerDown);
        controlLocator.OnBumperDown.AddListener(HandleBumperDown);

        //shared head locator:
        TransmissionObject headTransmissionObject = Transmission.Spawn("SampleTransmissionObject", Vector3.zero, Quaternion.identity, Vector3.one);
        headTransmissionObject.motionSource = Camera.main.transform;

        //shared controll locator:
        TransmissionObject controlTransmissionObject = Transmission.Spawn("SampleTransmissionObject", Vector3.zero, Quaternion.identity, Vector3.one);
        controlTransmissionObject.motionSource = controlLocator.transform;

        //sets:
        _initialInfo = info.text;
    }

    //Event Handlers:
    private void HandleTriggerDown()
    {
        //stamp a cube in space:
        TransmissionObject spawn = Transmission.Spawn("SampleTransmissionObject", controlLocator.Position, controlLocator.Orientation, Vector3.one);
        _spawned.Add(spawn);
    }

    private void HandleBumperDown()
    {
        //remove all stamped cubes:
        foreach (var item in _spawned)
        {
            item.Despawn();
        }

        _spawned.Clear();
    }

    //Loops:
    private void Update()
    {
        string output = _initialInfo + System.Environment.NewLine;
        output += "Peers Available: " + Transmission.Instance.Peers.Length + System.Environment.NewLine;
        output += "Localized: " + SpatialAlignment.Localized;

        info.text = output;
    }
#endif
}