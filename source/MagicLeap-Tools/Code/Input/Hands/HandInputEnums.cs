// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeapTools
{
    [System.Serializable]
    public enum FingerType
    {
        None,
        Thumb,
        Index,
        Middle,
        Ring,
        Pinky
    }

    [System.Serializable]
    public enum FilterType
    {
        Raw,
        Filtered
    }

    [System.Serializable]
    public enum KeypointType
    {
        //aka knuckle
        MCP,
        //aka middle joint
        PIP,
        //aka finger tip
        Tip, 
        HandCenter,
        Wrist
    }

    [System.Serializable]
    public enum InteractionPointType
    {
        Pinch,
        Grasp,
        Point,
        Current
    }
}
