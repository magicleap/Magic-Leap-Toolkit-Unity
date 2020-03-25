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
    //Public Enums:
    public enum SurfaceType { None, Floor, Seat, Table, Underside, Wall, Ceiling }

    [RequireComponent(typeof(SpatialMapperThrottle))]
    public class SurfaceDetails : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Properties:
        public static bool FloorFound
        {
            get;
            private set;
        }

        public static bool CeilingFound
        {
            get;
            private set;
        }

        public static float RoomHeight
        {
            get
            {
                return CeilingHeight - FloorHeight;
            }
        }

        public static float FloorHeight
        {
            get;
            private set;
        }

        public static float CeilingHeight
        {
            get;
            private set;
        }

        public static float VerticalCenter
        {
            get
            {
                return Mathf.Lerp(FloorHeight, CeilingHeight, .5f);
            }
        }

        //Events:
        /// <summary>
        /// Thrown once the floor has been found.
        /// </summary>
        public FloatEvent OnFloorFound = new FloatEvent();
        /// <summary>
        /// Thrown once the floor has been updated.
        /// </summary>
        public FloatEvent OnFloorUpdated = new FloatEvent();
        /// <summary>
        /// Thrown once the ceiling has been found.
        /// </summary>
        public FloatEvent OnCeilingFound = new FloatEvent();
        /// <summary>
        /// Thrown once the ceiling has been updated.
        /// </summary>
        public FloatEvent OnCeilingUpdated = new FloatEvent();


        //Private Variables:
        private static Transform _mainCamera;
        private static float _wallThreshold = .65f;
        private static float _minimumSeatHeight = 0.4064f;
        private static float _minimumTableHeight = 0.6604f;
        private static float _undersideHeight = 1.2192f;
        private static bool _initialized;
        private static Vector3 _planesQueryBoundsExtents = new Vector3(6, 6, 6);
        private static float _detectionInterval = .5f;
        private static float _averageCeilingHeight = 2.4384f;
        private static float _averageHeight = 1.76022f;

        //Init:
        private void Start()
        {
            Initialize();

            //start planes:
            MLResult r = MLPlanes.Start();
            
            GetPlanes();
        }

        //Shutdown:
        private void OnDestroy()
        {
            if (MLPlanes.IsStarted)
            {
                MLPlanes.Stop();
            }
        }

        //Public Methods:
        public static SurfaceType Analyze(RaycastHit hit)
        {
            Initialize();

            //determine surface:
            float dot = Vector3.Dot(Vector3.up, hit.normal);
            if (Mathf.Abs(dot) <= _wallThreshold)
            {
                return SurfaceType.Wall;
            }
            else
            {
                if (Mathf.Sign(dot) == 1)
                {
                    //status:
                    float floorDistance = Mathf.Abs(hit.point.y - FloorHeight);
                    float headDistance = Mathf.Abs(_mainCamera.position.y - FloorHeight);

                    if (headDistance < _minimumTableHeight)
                    {
                        return SurfaceType.Table;
                    }

                    if (hit.point.y >= FloorHeight + _minimumTableHeight)
                    {
                        return SurfaceType.Table;
                    }

                    if (hit.point.y >= FloorHeight + _minimumSeatHeight)
                    {
                        return SurfaceType.Seat;
                    }

                    return SurfaceType.Floor;
                }
                else
                {
                    if (hit.point.y <= FloorHeight + _undersideHeight)
                    {
                        return SurfaceType.Underside;
                    }

                    return SurfaceType.Ceiling;
                }
            }
        }

        //Private Methods:
        private void GetPlanes()
        {
            MLPlanes.QueryParams query = new MLPlanes.QueryParams();
            query.MaxResults = 20;
            query.Flags = MLPlanes.QueryFlags.Inner | MLPlanes.QueryFlags.SemanticFloor | MLPlanes.QueryFlags.SemanticCeiling;
            query.BoundsCenter = _mainCamera.position;
            query.BoundsRotation = _mainCamera.rotation;
            query.BoundsExtents = _planesQueryBoundsExtents;
            MLPlanes.GetPlanes(query, HandlePlanes);
        }

        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            //refs:
            _mainCamera = Camera.main.transform;

            //instance:
            if (FindObjectOfType<SurfaceDetails>() == null)
            {
                GameObject surfaceDetails = new GameObject("(SurfaceDetails)", typeof(SurfaceDetails));
            }

            //quick guesses:
            FloorHeight = _mainCamera.position.y - _averageHeight;
            CeilingHeight = FloorHeight + _averageCeilingHeight;

            _initialized = true;
        }

        //Event Handlers:
        private void HandlePlanes(MLResult result, MLPlanes.Plane[] planes, MLPlanes.Boundaries[] boundaries)
        {
            //sets:
            float largestCeilingPlane = 0;
            int ceilingID = -1;
            float lowestFloorPlane = float.MaxValue;
            int floorID = -1;

            //iterate found planes:
            for (int i = 0; i < planes.Length; i++)
            {
                switch ((MLPlanes.QueryFlags)planes[i].Flags)
                {
                    case MLPlanes.QueryFlags.Horizontal | MLPlanes.QueryFlags.Inner | MLPlanes.QueryFlags.SemanticCeiling:
                        //find largest ceiling plane:
                        if (planes[i].Center.y > _mainCamera.transform.position.y)
                        {
                            float size = planes[i].Width * planes[i].Height;
                            if (size > largestCeilingPlane)
                            {
                                largestCeilingPlane = size;
                                ceilingID = i;
                            }
                        }
                        break;

                    case MLPlanes.QueryFlags.Horizontal | MLPlanes.QueryFlags.Inner | MLPlanes.QueryFlags.SemanticFloor:
                        //find lowest floor plane:
                        if (planes[i].Center.y < _mainCamera.transform.position.y)
                        {
                            if (planes[i].Center.y < lowestFloorPlane)
                            {
                                lowestFloorPlane = planes[i].Center.y;
                                floorID = i;
                            }
                        }
                        break;
                }
            }
            
            //ceiling found:
            if (ceilingID != -1)
            {
                if (!CeilingFound)
                {
                    CeilingHeight = planes[ceilingID].Center.y;
                    OnCeilingFound?.Invoke(CeilingHeight);
                    CeilingFound = true;
                }
                else
                {
                    if (CeilingHeight != planes[ceilingID].Center.y)
                    {
                        CeilingHeight = planes[ceilingID].Center.y;
                        OnCeilingUpdated?.Invoke(CeilingHeight);
                    }
                }
            }

            //floor found:
            if (floorID != -1)
            {
                if (!FloorFound)
                {
                    FloorHeight = planes[floorID].Center.y;
                    OnFloorFound?.Invoke(FloorHeight);
                    FloorFound = true;
                }
                else
                {
                    if (FloorHeight != planes[floorID].Center.y)
                    {
                        FloorHeight = planes[floorID].Center.y;
                        OnFloorUpdated?.Invoke(FloorHeight);
                    }
                }

                //set an initial ceiling:
                if (!CeilingFound)
                {
                    CeilingHeight = FloorHeight + _averageCeilingHeight;
                }
            }

            //repeat:
            Invoke("GetPlanes", _detectionInterval);
        }
#endif
    }
}