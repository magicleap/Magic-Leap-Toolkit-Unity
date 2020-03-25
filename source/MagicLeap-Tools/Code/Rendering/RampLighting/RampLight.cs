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
    [ExecuteAlways]
    public class RampLight : MonoBehaviour
    {
        //Public Variables:
        public float intensity = 1;
        public Texture ramp;
        public Texture cubeMap;

        //Private Variables:
        private Vector3 _previousForward;
        private Texture _previousRamp;
        private Texture _previousCube;
        private float _previousIntensity;

        //Init:
        private void Awake()
        {
            RampLight[] currentRampLights = FindObjectsOfType<RampLight>();
            if (currentRampLights.Length != 1)
            {
                Debug.LogError($"Only use one RampLight in your scene. You currently have {currentRampLights.Length}.");
            }
        }

        //Loops:
        private void Update()
        {
            //update direction:
            if (_previousForward != transform.forward)
            {
                _previousForward = transform.forward;
                Shader.SetGlobalVector("_LightVector", -transform.forward);
            }

            //update ramp:
            if (_previousRamp != ramp)
            {
                _previousRamp = ramp;
                Shader.SetGlobalTexture("_Ramp", ramp);
            }

            //update cube:
            if (_previousCube != cubeMap)
            {
                _previousCube = cubeMap;
                Shader.SetGlobalTexture("_RefCube", cubeMap);
            }

            //update intensity:
            if (_previousIntensity != intensity)
            {
                if (intensity < 0)
                {
                    intensity = 0;
                }
                _previousIntensity = intensity;
                Shader.SetGlobalFloat("_Intensity", intensity);
            }
        }
    }
}