// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.Events;
#if PLATFORM_LUMIN
using static UnityEngine.XR.MagicLeap.Native.MagicLeapNativeBindings;
#endif

namespace MagicLeapTools
{
    public class SpatialAlignment : MonoBehaviour
    {
        //Public Properties:
        public static bool Localized
        {
            get;
            private set;
        }

        //Events:
        public UnityEvent OnLocalized;

#if PLATFORM_LUMIN
        //Private Variables:
        private MLPersistentCoordinateFrames.PCF _anchorPCF;
        private MLPersistentCoordinateFrames.PCF _sharedPCF;
        private string _sharedPCFKey = "AnchorPCF";
        private Transform _camera;
        private float _pcfSearchTimeout = 1;
        private MLCoordinateFrameUID _cfuid;

        //Init:
        private void Awake()
        {
            //spatial alignment can not work in the Unity editor:
            if (Application.isEditor)
            {
                enabled = false;
            }
        }

        private IEnumerator Start()
        {
            //hooks:
            Transmission.Instance.OnOldestPeerUpdated.AddListener(HandleOldestPeerUpdated);
            Transmission.Instance.OnGlobalStringChanged.AddListener(HandleGlobalStringChanged);
            Transmission.Instance.OnGlobalStringsReceived.AddListener(HandleGlobalStringsReceived);

            //refs:
            _camera = Camera.main.transform;

            //system start-ups:
            if (!MLPersistentCoordinateFrames.IsStarted)
            {
                MLPersistentCoordinateFrames.Start();

                //wait for MLPersistentCoordinateFrames to localize:
                while (!MLPersistentCoordinateFrames.IsLocalized)
                {
                    yield return null;
                }

                //establish shared pcf:
                MLPersistentCoordinateFrames.FindClosestPCF(_camera.position, out _anchorPCF, MLPersistentCoordinateFrames.PCF.Types.MultiUserMultiSession);
                if (_anchorPCF == null)
                {
                    //keep looking:
                    yield return new WaitForSeconds(_pcfSearchTimeout);
                }

                //hooks:
                MLPersistentCoordinateFrames.OnLocalized += HandleLocalized;
                MLPersistentCoordinateFrames.PCF.OnStatusChange += HandlePCFStatusChange;
            }
        }

        //Deinit:
        private void OnDestroy()
        {
            //system turn offs:
            if (MLPersistentCoordinateFrames.IsStarted)
            {
                MLPersistentCoordinateFrames.Stop();
            }
        }

        //Coroutines:
        private IEnumerator WeAreTheOldest()
        {
            //make sure we found our anchor pcf:
            while (_anchorPCF == null)
            {
                yield return null;
            }

            //update the global shared pcf:
            _sharedPCF = _anchorPCF;
            Reorient();
            Transmission.SetGlobalString(_sharedPCFKey, _anchorPCF.CFUID.ToString());
        }

        private IEnumerator LocalizeToSharedPCF()
        {
            _cfuid = SerializationUtilities.StringToCFUID(Transmission.GetGlobalString(_sharedPCFKey));

            //find shared pcf:
            while (_sharedPCF == null)
            {
                //locate:
                MLPersistentCoordinateFrames.FindPCFByCFUID(_cfuid, out _sharedPCF);

                //keep looking:
                yield return new WaitForSeconds(_pcfSearchTimeout);
            }

            //we have our shared pcf!
            Reorient();
            Localized = true;
            OnLocalized?.Invoke();
        }

        //Event Handlers:
        private void HandleGlobalStringChanged(string key)
        {
            //is this a shared pcf update?
            if (key == _sharedPCFKey)
            {
                StartCoroutine("LocalizeToSharedPCF");
            }
        }

        private void HandleGlobalStringsReceived()
        {
            if (Transmission.HasGlobalString(_sharedPCFKey))
            {
                StartCoroutine("LocalizeToSharedPCF");
            }
        }

        private void HandleOldestPeerUpdated(string oldest)
        {
            if (oldest == NetworkUtilities.MyAddress)
            {
                StartCoroutine("WeAreTheOldest");
            }
        }

        private void HandleLocalized(bool localized)
        {
            if (localized)
            {
                //our shared pcf updated:
                Reorient();
            }
        }

        private void HandlePCFStatusChange(MLPersistentCoordinateFrames.PCF.Status pcfStatus, MLPersistentCoordinateFrames.PCF pcf)
        {
            if (_sharedPCF != null && pcf.CFUID == _sharedPCF.CFUID)
            {
                //our shared pcf updated:
                Reorient();
            }
        }

        //Private Methods:
        private void Reorient()
        {
            _sharedPCF.Update();
            Transmission.Instance.sharedOrigin = new Pose(_sharedPCF.Position, _sharedPCF.Rotation);
        }
#endif
    }
}