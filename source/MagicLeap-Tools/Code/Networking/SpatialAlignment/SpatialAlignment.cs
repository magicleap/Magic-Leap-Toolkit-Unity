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
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif
using System.Linq;

namespace MagicLeapTools
{
    public class SpatialAlignment : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Events
        /// <summary>
        /// Fired when a peer aligns with us.
        /// </summary>
        public StringEvent OnPeerAligned;

        //Public Variables:
        public List<string> alignedPeers = new List<string>();

        //Public Properties:
        public static bool Localized
        {
            get
            {
                return _localPCFs.Count > 0;
            }
        }

        //Private Variables:
        private const float QueryInterval = 2;
        private const float SendDuration = 1;
        private const int QueryCount = 50;
        private const int OutboundCount = 10;
        private static List<MLPCF> _localPCFs = new List<MLPCF>();
        private List<MLPCF> _localPCFData = new List<MLPCF>();
        private Dictionary<string, MLPCF> _localPCFReferences = new Dictionary<string, MLPCF>();
        private List<PCFMessage> _outboundPCFs = new List<PCFMessage>();
        private Dictionary<string, SpatialAlignmentHistory> _alignmentHistory = new Dictionary<string, SpatialAlignmentHistory>();
        private Transform _transformHelper;
        private Transform _mainCamera;
        private int _interval;

        //Init:
        private void Awake()
        {
            //refs:
            _mainCamera = Camera.main.transform;

            //offset helper:
            _transformHelper = new GameObject("(TransformHelper)").transform;
            _transformHelper.gameObject.hideFlags = HideFlags.HideInHierarchy;

            //start systems:
            MLPersistentCoordinateFrames.Start();
            StartCoroutine("PCFDiscovery");

            //hooks:
            Transmission.Instance.OnPCF.AddListener(HandlePCFReceived);
            Transmission.Instance.OnSpatialAlignment.AddListener(HandleSpatialAlignmentNotification);
        }

        //Deint:
        private void OnDestroy()
        {
            //unhooks:
            if (Transmission.Instance != null)
            {
                Transmission.Instance.OnPCF.RemoveListener(HandlePCFReceived);
            }

            //shutdowns:
            StopAllCoroutines();
            if (MLPersistentCoordinateFrames.IsStarted)
            {
                MLPersistentCoordinateFrames.Stop();
            }
        }

        //Coroutines:
        private IEnumerator PCFDiscovery()
        {
            //query until we get PCFs:
            while (_localPCFs.Count == 0)
            {
                MLPersistentCoordinateFrames.GetAllPCFs(out _localPCFs, QueryCount);
                yield return null;
            }

            while (true)
            {
                DiscoverPCFs();
                yield return new WaitForSeconds(QueryInterval);
                yield return null;
            }
        }

        //Private Methods:
        private void DiscoverPCFs()
        {
            //interval:
            _interval++;
            _interval = _interval % 100;

            //if previous send queue isn't finished then interrupt it:
            StopCoroutine("SendPCFs");

            //clear lists:
            _localPCFs.Clear();
            _localPCFData.Clear();
            _localPCFReferences.Clear();
            _outboundPCFs.Clear();

            //get pcfs:
            MLPersistentCoordinateFrames.GetAllPCFs(out _localPCFs, QueryCount);

            //request poses:
            foreach (var item in _localPCFs)
            {
                MLPersistentCoordinateFrames.GetPCFPosition(item, HandlePCFPoseRetrieval);
            }
        }

        //Event Handlers:
        private void HandleSpatialAlignmentNotification(string from)
        {
            if (!alignedPeers.Contains(from))
            {
                //save and report alignment:
                alignedPeers.Add(from);
                OnPeerAligned.Invoke(from);
            }
        }

        private void HandlePCFPoseRetrieval(MLResult result, MLPCF pcf)
        {
            //save results:
            _localPCFData.Add(pcf);

            //do we have all of them?
            if (_localPCFData.Count == _localPCFs.Count)
            {
                //sort by distance:
                _localPCFData = _localPCFData.OrderBy(p => Vector3.Distance(_mainCamera.position, p.Position)).ToList();

                //grab a chunk of results:
                for (int i = 0; i < Mathf.Min(OutboundCount, _localPCFData.Count); i++)
                {
                    //find offsets:
                    _transformHelper.SetPositionAndRotation(_localPCFData[i].Position, _localPCFData[i].Orientation);
                    Vector3 positionOffset = _transformHelper.InverseTransformPoint(Vector3.zero);
                    Quaternion rotationOffset = Quaternion.Inverse(_transformHelper.rotation) * Quaternion.LookRotation(Vector3.forward);

                    //catalog:
                    _outboundPCFs.Add(new PCFMessage(_localPCFData[i].CFUID.ToString(), new Pose(positionOffset, rotationOffset), _interval));
                    _localPCFReferences.Add(_localPCFData[i].CFUID.ToString(), _localPCFData[i]);
                }

                //send them out:
                StartCoroutine("SendPCFs");
            }
        }

        private void HandlePCFReceived(string from, string CFUID, Pose offset, int interval)
        {
            //find a matching local PCF:
            if (_localPCFReferences.ContainsKey(CFUID))
            {
                if (!alignedPeers.Contains(from))
                {
                    //save and report alignment:
                    alignedPeers.Add(from);
                    OnPeerAligned.Invoke(from);
                }

                //get peer root offset from this PCF:
                _transformHelper.SetPositionAndRotation(_localPCFReferences[CFUID].Position, _localPCFReferences[CFUID].Orientation);
                Vector3 position = _transformHelper.TransformPoint(offset.position);
                Quaternion rotation = _transformHelper.rotation * offset.rotation;

                //first pcf message from this peer?
                if (!_alignmentHistory.ContainsKey(from))
                {
                    //establish alignment history:
                    _alignmentHistory.Add(from, new SpatialAlignmentHistory(interval));

                    //snap peer root to initial:
                    TransmissionRoot.Get(from).SetPositionAndRotationTargets(position, rotation);
                }

                //same interval:
                if (_alignmentHistory[from].interval == interval)
                {
                    _alignmentHistory[from].positions.Add(position);
                    _alignmentHistory[from].rotations.Add(rotation);
                }
                else
                {
                    //new interval:
                    _alignmentHistory[from].interval = interval;
                    TransmissionRoot.Get(from).SetPositionAndRotationTargets(_alignmentHistory[from].AveragePosition, _alignmentHistory[from].AverageRotation);

                    //get ready for next interval's queue:
                    _alignmentHistory[from].Clear();
                    _alignmentHistory[from].positions.Add(position);
                    _alignmentHistory[from].rotations.Add(rotation);
                }
            }
        }

        //Coroutines:
        private IEnumerator SendPCFs()
        {
            //this will send out all pcfs sequentially throughout the SendDuration to not flood the socket:
            float sendTimeout = SendDuration / _localPCFs.Count;

            for (int i = 0; i < _outboundPCFs.Count; i++)
            {
                Transmission.Send(_outboundPCFs[i]);
                yield return new WaitForSeconds(sendTimeout);
                yield return null;
            }
        }
#endif
    }
}