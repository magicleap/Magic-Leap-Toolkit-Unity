// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeapTools
{
    public class SpatialMapperThrottle : MonoBehaviour
    {
        //Private Variables:
        private MLSpatialMapper _spatialMapper;
        private Camera _mainCamera;
        private float _minPollingRate = 0;
        private float _maxPollingRate = .25f;
        private float _raycastDistance = 6.096f;

        //Init:
        private void Start()
        {
            _mainCamera = Camera.main;
            _spatialMapper = FindObjectOfType<MLSpatialMapper>();

            //if no spatial mapper is found then disable
            //since this is just a nice helper there is no real need to announce this
            if (_spatialMapper == null)
            {
                enabled = false;
                return;
            }

            _spatialMapper.pollingRate = _minPollingRate;
        }

        void Update()
        {
            //mesh in front of us?
            RaycastHit[] hits = Physics.RaycastAll(_mainCamera.transform.position, _mainCamera.transform.forward, _raycastDistance);
            bool hitMesh = false;
            for (int i = 0; i < hits.Length; i++)
            {
                string[] nameSplit = hits[i].transform.name.Split('-');
                if (nameSplit.Length == 2 && nameSplit[0].Contains("Mesh "))
                {
                    hitMesh = true;
                    break;
                }
            }

            //reduce or increase polling based on presence of meshing in front of us:
            if (!hitMesh)
            {
                if (_spatialMapper.pollingRate != _minPollingRate)
                {
                    _spatialMapper.pollingRate = _minPollingRate;
                }
            }
            else
            {
                if (_spatialMapper.pollingRate != _maxPollingRate)
                {
                    _spatialMapper.pollingRate = _maxPollingRate;
                }
            }
        }
    }
}