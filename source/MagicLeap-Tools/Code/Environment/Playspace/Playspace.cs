// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap.Native;
#endif
using UnityEngine.Events;

namespace MagicLeapTools
{
    [RequireComponent(typeof(SpatialMapperThrottle))]
    public class Playspace : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Private Classes:
        private class VirtualWall
        {
            //Public Variables:
            public Vector3 cornerA;
            public Vector3 cornerB;
            public Vector3 normal;
            public MLPlanes.Plane plane;
            public MLPlanes.QueryParams query;
            public bool physical = true;

            //Public Properties:
            public Vector3 Center
            {
                get
                {
                    return Vector3.Lerp(cornerA, cornerB, .5f);
                }
            }

            public Vector3 Vector
            {
                get
                {
                    return cornerB - cornerA;
                }
            }

            //Constructors:
            public VirtualWall(Vector3 cornerA, Vector3 cornerB)
            {
                this.cornerA = cornerA;
                this.cornerB = cornerB;
            }
        }
#endif

        //Public Variables:
        public bool runAtStart = true;
        [Tooltip("The gui to display when a user needs to find the floor.")]
        public GameObject findFloorGUI;
        [Tooltip("The gui to display when a user needs to find the ceiling.")]
        public GameObject findCeilingGUI;
        [Tooltip("The gui to display when a user needs to plot the corners of their room.")]
        public GameObject drawPerimeterGUI;
        [Tooltip("The gui to display when confirming the perimeter of the playspace.")]
        public GameObject confirmAreaGUI;
        [Tooltip("The gui to be displayed when a user needs to select the primary wall.")]
        public GameObject selectPrimaryWallGUI;
        [Tooltip("The gui to be shown on the wall when the primary wall is known.")]
        public GameObject primaryWallPlaque;
        public Material cornerMaterial;
        [Tooltip("The material used when the outline of the playspace is shown for confirmation.")]
        public Material perimeterOutlineMaterial;
        public Material wallMaterial;
        public Material floorMaterial;
        public Material ceilingMaterial;
        [Tooltip("How far behind a plotted wall should we look to find a physical wall.")]
        public float maxPlaneDepthTest = 1.524f;
        [HideInInspector] public MLPersistentCoordinateFrames.PCF pcfAnchor;

        //Events:
        /// <summary>
        /// Thrown when the user completes building a playspace.
        /// </summary>
        public UnityEvent OnCompleted;
        /// <summary>
        /// Thrown if something has caused the playspace to update sucha as a PCF relocating.
        /// </summary>
        public UnityEvent OnUpdated;
        /// <summary>
        /// Thrown when the user declines the solved playspace so they can try again.  Useful for clearing anything that was using the playspace.
        /// </summary>
        public UnityEvent OnCleared;

        //Enums:
        public enum State { Idle, Restore, FindPCF, FindFloor, FindCeiling, DrawPerimeter, FindWalls, ConfirmArea, ConfirmAreaReload, ConfirmPrimaryWall, Complete };

        //Public Properties:
        public static Playspace Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Playspace>();
                }

                if (_instance == null)
                {
                    Debug.LogError("No instance of Playspace found in scene.");
                }

                return _instance;
            }
        }

        public bool Ready
        {
            get;
            private set;
        }

        public State CurrentState
        {
            get;
            private set;
        }

        public GameObject FloorGeometry
        {
            get;
            private set;
        }

        public GameObject WallGeometry
        {
            get;
            private set;
        }

        public GameObject CeilingGeometry
        {
            get;
            private set;
        }

        public PlayspaceWall[] Walls
        {
            get;
            private set;
        }

        public Vector3 Center
        {
            get
            {
#if PLATFORM_LUMIN
                //we make this relative to the pcf so relocalization keeps this value accurate:
                return TransformUtilities.WorldPosition(pcfAnchor.Position, pcfAnchor.Rotation, _playspaceCenter);
#else
                return Vector3.zero;
#endif
            }
        }

        public Vector3 CeilingCenter
        {
            get
            {
                Vector3 location = Center;
                location.y += Height * .5f;
                return location;
            }
        }

        public Vector3 FloorCenter
        {
            get
            {
                Vector3 location = Center;
                location.y -= Height * .5f;
                return location;
            }
        }

        public float Height
        {
            get;
            private set;
        }

        /// <summary>
        /// The index in the Walls array of the wall a user has defined as primary.
        /// </summary>
        public int PrimaryWall
        {
            get;
            private set;
        }

        /// <summary>
        /// The index in the Walls array of the largest rear wall.
        /// </summary>
        public int RearWall
        {
            get;
            private set;
        }

        /// <summary>
        /// The index in the Walls array of the largest left wall.
        /// </summary>
        public int LeftWall
        {
            get;
            private set;
        }

        /// <summary>
        /// The index in the Walls array of the largest right wall.
        /// </summary>
        public int RightWall
        {
            get;
            private set;
        }

        //Private Variables:
        private static Playspace _instance;
#if PLATFORM_LUMIN
        private Transform _camera;
        private List<Transform> _plottedCorners = new List<Transform>();
        private List<VirtualWall> _virtualWalls = new List<VirtualWall>();
        private float _loopClosureDistance = 0.3048f;
        private int _queryCount;
        private float _ceilingHuntMaxDuration = 5;
        private Bounds _plottedBounds;
        private List<Vector3> _playspaceCorners;
        private GameObject _currentGUI;
        private SurfaceDetails _surfaceDetails;
        private float _roomVerticalCenter;
        private float _roomCeilingHeight;
        private float _roomFloorHeight;
        private string _sessionDataKey = "playspace_data";
        private string _sessionMeshKey = "playspace_mesh";
        private string _serializedMeshes;
        private int _restoreAttempt;
        private int _maxRestoreAttempts = 3;
        private float _restoreRetryDelay = 1;
        private Vector3 _playspaceCenter;
        private Vector3 _cachedPrimaryWallGUIScale;

        //Init:
        private IEnumerator Start()
        {
            HideGUI();

            //refs:
            _camera = Camera.main.transform;

            //sets:
            _cachedPrimaryWallGUIScale = selectPrimaryWallGUI.transform.localScale;

            //features:
            MLPersistentCoordinateFrames.Start();
            MLPlanes.Start();
            MLInput.Start();

            //wait for service startup:
            Debug.Log("Waiting for PersistentCoordinateFrames service to localize...");
            while (!MLPersistentCoordinateFrames.IsLocalized)
            {
                yield return null;
            }
            Debug.Log("PersistentCoordinateFrames service localized!");

            //hooks:
            MLPersistentCoordinateFrames.OnLocalized += HandleOnLocalized;
            MLPersistentCoordinateFrames.PCF.OnStatusChange += HandlePCFChanged;

            //requirements:
            _surfaceDetails = FindObjectOfType<SurfaceDetails>();
            if (_surfaceDetails == null)
            {
                _surfaceDetails = gameObject.AddComponent<SurfaceDetails>();
            }

            if (runAtStart)
            {
                Create();
            }
        }

        //Deinit:
        private void OnDestroy()
        {
            if (MLPersistentCoordinateFrames.IsStarted)
            {
                MLPersistentCoordinateFrames.Stop();
            }

            if (MLPlanes.IsStarted)
            {
                MLPlanes.Stop();
            }

            if (MLInput.IsStarted)
            {
                MLInput.Stop();
            }
        }

        //Public Methods:
        /// <summary>
        /// Forces a new Playspace creation from scratch.
        /// </summary>
        public void Rebuild()
        {
            PlayerPrefs.DeleteKey(_sessionDataKey);
            PlayerPrefs.DeleteKey(_sessionMeshKey);
            Create();
        }

        /// <summary>
        /// Initiates the guided system for generating a Playspace.
        /// </summary>
        public void Create()
        {
            Ready = false;

            OnCleared?.Invoke();

            //clean up:
            HideGUI();
            RemovePlottedBounds();
            RemoveDebugLines();

            //remove previous geometry:
            if (WallGeometry != null)
            {
                Destroy(WallGeometry);
            }

            if (CeilingGeometry != null)
            {
                Destroy(CeilingGeometry);
            }

            if (FloorGeometry != null)
            {
                Destroy(FloorGeometry);
            }

            //reload or create new:
            if (PlayerPrefs.HasKey(_sessionDataKey) && PlayerPrefs.HasKey(_sessionMeshKey))
            {
                ChangeState(State.Restore);
            }
            else
            {
                ChangeState(State.FindPCF);
            }
        }

        /// <summary>
        /// Returns true if the position is inside the playspace.
        /// </summary>
        public bool Inside(Vector3 position)
        {
            //can't operate without walls:
            if (Walls.Length == 0)
            {
                return false;
            }

            float angle = 0;

            for (int i = 0; i < Walls.Length; i++)
            {
                //add up angles:
                Vector3 to = Walls[i].LeftEdge - position;
                Vector3 toNext = Walls[i].RightEdge - position;
                angle += Vector3.SignedAngle(to, toNext, Vector3.down);
            }

            //set inside status - if we ended up at about 0 we are outside if we ended up at about 360 we are inside:
            return angle > 180;
        }

        /// <summary>
        /// Given a position and rotation the index of the facing wall will be returned.
        /// </summary>
        public int FacingWall(Vector3 position, Vector3 forward, ref Vector3 intersection)
        {
            //can't operate without walls:
            if (Walls.Length == 0)
            {
                return -1;
            }

            int facingWallID = -1;
            float furthestFacingWallDistance = float.MinValue;

            for (int i = 0; i < Walls.Length; i++)
            {
                //find facing wall
                Vector3 stretchedForward = position + forward * 100;
                if (MathUtilities.LineSegmentsIntersecting(position, stretchedForward, Walls[i].LeftEdge, Walls[i].RightEdge, true))
                {
                    Vector3 _intersection = Vector3.zero;
                    MathUtilities.RayIntersection(new Ray(position, forward), new Ray(Walls[i].LeftEdge, Walls[i].Right), ref _intersection);
                    float distance = Vector3.Distance(position, _intersection);
                    if (distance > furthestFacingWallDistance)
                    {
                        intersection = _intersection;
                        furthestFacingWallDistance = distance;
                        facingWallID = i;
                    }
                }
            };

            return facingWallID;
        }

        //Private Methods:
        private void FireOnUpdated()
        {
            //only fire this if we aren't in the middle of building a playspace:
            if (Ready)
            {
                OnUpdated?.Invoke();
            }
        }

        private void PreviewArea()
        {
            //visualize:
            Vector3[] playspacePerimeterOnFloor = new Vector3[_playspaceCorners.Count];
            Vector3[] playspacePerimeterOnCeiling = new Vector3[_playspaceCorners.Count];

            //populate floor and ceiling corner loops:
            for (int i = 0; i < _playspaceCorners.Count; i++)
            {
                //cache floor and ceiling locations:
                Vector3 floor = TransformUtilities.WorldPosition(pcfAnchor.Position, pcfAnchor.Rotation, _playspaceCorners[i]);
                Vector3 ceiling = floor + new Vector3(0, Height);
                playspacePerimeterOnFloor[i] = floor;
                playspacePerimeterOnCeiling[i] = ceiling;

                //draw corner supports:
                LineRenderer cornerSupportsOutline = Lines.DrawLine($"PlayspaceCornerSupport{i}", Color.green, Color.green, .005f, floor, ceiling);
                cornerSupportsOutline.material = perimeterOutlineMaterial;
            }

            //draw ceiling and floor perimeter:
            LineRenderer ceilingOutline = Lines.DrawLine($"PlayspaceCeilingOutline", Color.green, Color.green, .005f, playspacePerimeterOnCeiling);
            LineRenderer floorOutline = Lines.DrawLine($"PlayspaceFloorOutline", Color.green, Color.green, .005f, playspacePerimeterOnFloor);
            ceilingOutline.material = perimeterOutlineMaterial;
            floorOutline.material = perimeterOutlineMaterial;
        }

        private void AnchorToPCF()
        {
            //if we don't have anything to anchor then exit:
            if (WallGeometry == null || FloorGeometry == null || CeilingGeometry == null)
            {
                return;
            }

            //anchor to PCF and correct any tilt:
            Vector3 correctedForward = Vector3.ProjectOnPlane(pcfAnchor.Rotation * Vector3.forward, Vector3.up).normalized;
            Quaternion correctedRotation = Quaternion.LookRotation(correctedForward);
            WallGeometry.transform.SetPositionAndRotation(pcfAnchor.Position, correctedRotation);
            FloorGeometry.transform.SetPositionAndRotation(pcfAnchor.Position, correctedRotation);
            CeilingGeometry.transform.SetPositionAndRotation(pcfAnchor.Position, correctedRotation);
        }

        private void BuildGeometry(Vector3[] wallVerticies, int[] wallTriangles, Vector3[] floorVerticies, int[] floorTriangles, Vector3[] ceilingVerticies)
        {
            //setup geometry:
            WallGeometry = new GameObject("(PlayspaceWalls)", typeof(MeshFilter), typeof(MeshRenderer));
            FloorGeometry = new GameObject("(PlayspaceFloor)", typeof(MeshFilter), typeof(MeshRenderer));
            CeilingGeometry = new GameObject("(PlayspaceCeiling)", typeof(MeshFilter), typeof(MeshRenderer));

            AnchorToPCF();

            //setup renderers:
            MeshRenderer wallsRenderer = WallGeometry.GetComponent<MeshRenderer>();
            if (wallMaterial != null)
            {
                wallsRenderer.material = wallMaterial;
            }
            else
            {
                wallsRenderer.enabled = false;
            }

            MeshRenderer floorRenderer = FloorGeometry.GetComponent<MeshRenderer>();
            if (floorMaterial != null)
            {
                floorRenderer.material = floorMaterial;
            }
            else
            {
                floorRenderer.enabled = false;
            }

            MeshRenderer ceilingRenderer = CeilingGeometry.GetComponent<MeshRenderer>();
            if (ceilingMaterial != null)
            {
                ceilingRenderer.material = ceilingMaterial;
            }
            else
            {
                ceilingRenderer.enabled = false;
            }

            //apply mesh:
            Mesh wallMesh = new Mesh();
            wallMesh.vertices = wallVerticies;
            wallMesh.triangles = wallTriangles;
            WallGeometry.GetComponent<MeshFilter>().mesh = wallMesh;

            Mesh floorMesh = new Mesh();
            floorMesh.vertices = floorVerticies;
            floorMesh.triangles = floorTriangles;
            FloorGeometry.GetComponent<MeshFilter>().mesh = floorMesh;

            Mesh ceilingMesh = new Mesh();
            ceilingMesh.vertices = ceilingVerticies;
            ceilingMesh.triangles = floorTriangles.Reverse().ToArray();
            CeilingGeometry.GetComponent<MeshFilter>().mesh = ceilingMesh;

            //apply colliders:
            WallGeometry.AddComponent<MeshCollider>();
            FloorGeometry.AddComponent<MeshCollider>();
            CeilingGeometry.AddComponent<MeshCollider>();

            //serialize mesh:
            string wallVertsSerialized = SerializationUtilities.Serialize(wallMesh.vertices);
            string wallTrisSerialized = SerializationUtilities.Serialize(wallMesh.triangles);
            string floorVertsSerialized = SerializationUtilities.Serialize(floorMesh.vertices);
            string floorTrisSerialized = SerializationUtilities.Serialize(floorMesh.triangles);
            string ceilingVertsSerialized = SerializationUtilities.Serialize(ceilingMesh.vertices);
            _serializedMeshes = wallVertsSerialized + "|" + wallTrisSerialized + "|" + floorVertsSerialized + "|" + floorTrisSerialized + "|" + ceilingVertsSerialized;
        }

        private void ShowGUI(GameObject next)
        {
            if (_currentGUI != null)
            {
                _currentGUI.SetActive(false);
            }

            _currentGUI = next;
            next.SetActive(true);
        }

        private void HideGUI()
        {
            findFloorGUI.SetActive(false);
            findCeilingGUI.SetActive(false);
            drawPerimeterGUI.SetActive(false);
            confirmAreaGUI.SetActive(false);
            selectPrimaryWallGUI.SetActive(false);
            primaryWallPlaque.SetActive(false);
        }

        private void RemoveDebugLines()
        {
            Lines.DestroyAllLines();
        }

        private void RemovePlottedBounds()
        {
            //clean up previous items:
            foreach (var item in _plottedCorners)
            {
                Destroy(item.gameObject);
            }
            _plottedCorners.Clear();

            //remove lines:
            Lines.DestroyAllLines();
        }

        private void ChangeState(State nextState)
        {
            StopAllCoroutines();

            //find and run current state's exit:
            GetType().GetMethod(CurrentState + "_Exit", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(this, null);

            //update current state:
            CurrentState = nextState;
            string currentStateName = nextState.ToString();

            //find and run state's enter:
            GetType().GetMethod(currentStateName + "_Enter", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(this, null);

            //find and run state's update:
            if (GetType().GetMethod(currentStateName + "_Update", BindingFlags.Instance | BindingFlags.NonPublic) != null)
            {
                StartCoroutine(currentStateName + "_Update");
            }
        }

        //Event Handlers:
        private void HandlePCFChanged(MLPersistentCoordinateFrames.PCF.Status pcfStatus, MLPersistentCoordinateFrames.PCF pcf)
        {
            if (pcfAnchor != null && pcf.CFUID == pcfAnchor.CFUID)
            {
                //adjust to pcf:
                pcfAnchor.Update();
                AnchorToPCF();
                FireOnUpdated();
            }
        }

        private void HandleOnLocalized(bool localized)
        {
            if (localized)
            {
                Debug.Log("Playspace has been realigned to the world.");
                //adjust to pcf:
                pcfAnchor.Update();
                AnchorToPCF();
                FireOnUpdated();
            }
            else
            {
                Debug.Log("Tracking lost - look around some to localize so Playspace can realign to the world.");
            }
        }

        //State Methods:
        private void Restore_Enter()
        {
            //deserialize previous session:
            string[] sessionData = PlayerPrefs.GetString(_sessionDataKey).Split('|');
            Walls = SerializationUtilities.Deserialize<PlayspaceWall[]>(sessionData[1]);
            _playspaceCenter = JsonUtility.FromJson<Vector3>(sessionData[2]);
            PrimaryWall = int.Parse(sessionData[3]);
            RightWall = int.Parse(sessionData[4]);
            RearWall = int.Parse(sessionData[5]);
            LeftWall = int.Parse(sessionData[6]);
            _playspaceCorners = SerializationUtilities.Deserialize<Vector3[]>(sessionData[7]).ToList<Vector3>();
            Height = float.Parse(sessionData[8]);

            //determine restore success:
            if (PrimaryWall == -1 || RightWall == -1 || RearWall == -1 || LeftWall == -1)
            {
                //something is not right with this room so let's force a reset:
                PlayerPrefs.DeleteKey(_sessionMeshKey);
                PlayerPrefs.DeleteKey(_sessionDataKey);
                Create();
            }
            else
            {
                //locate pcf:
                StartCoroutine("Restore_Relocalize", sessionData[0]);
            }
        }

        private IEnumerator Restore_Relocalize(string pcfCFUID)
        {
            MagicLeapNativeBindings.MLCoordinateFrameUID cfuid = SerializationUtilities.StringToCFUID(pcfCFUID);

            while (true)
            {
                MLPersistentCoordinateFrames.FindPCFByCFUID(cfuid, out pcfAnchor);

                if (pcfAnchor == null)
                {
                    //we didn't find the pcf we needed this time:
                    _restoreAttempt++;
                    if (_restoreAttempt < _maxRestoreAttempts)
                    {
                        //retry:
                        Debug.Log($"Previous PCF not located.  Trying again.  Attempt: {_restoreAttempt}/{_maxRestoreAttempts}");
                        yield return new WaitForSeconds(_restoreRetryDelay);
                    }
                    else
                    {
                        //failed:
                        Debug.Log($"Failed to locate PCF attempt: {_restoreAttempt}/{_maxRestoreAttempts}.  Creating new Playspace.");
                        ChangeState(State.FindPCF);
                        yield break;
                    }
                    yield return null;
                }
                else
                {
                    //deserialize meshes:
                    string[] meshData = PlayerPrefs.GetString(_sessionMeshKey).Split('|');
                    Vector3[] wallVerts = SerializationUtilities.Deserialize<Vector3[]>(meshData[0]);
                    int[] wallTris = SerializationUtilities.Deserialize<int[]>(meshData[1]);
                    Vector3[] floorVerts = SerializationUtilities.Deserialize<Vector3[]>(meshData[2]);
                    int[] floorTris = SerializationUtilities.Deserialize<int[]>(meshData[3]);
                    Vector3[] ceilingVerts = SerializationUtilities.Deserialize<Vector3[]>(meshData[4]);

                    BuildGeometry(wallVerts, wallTris, floorVerts, floorTris, ceilingVerts);

                    ChangeState(State.ConfirmAreaReload);
                    yield break;
                }
            }
        }

        private void FindPCF_Enter()
        {
            MLPersistentCoordinateFrames.FindClosestPCF(_camera.position, out pcfAnchor);
            ChangeState(State.FindFloor);
        }

        private void FindFloor_Enter()
        {
            ShowGUI(findFloorGUI);
        }

        private IEnumerator FindFloor_Update()
        {
            while (true)
            {
                if (SurfaceDetails.FloorFound)
                {
                    ChangeState(State.FindCeiling);
                }
                yield return null;
            }
        }

        private void FindCeiling_Enter()
        {
            ShowGUI(findCeilingGUI);
        }

        private IEnumerator FindCeiling_Update()
        {
            float startedTime = Time.realtimeSinceStartup;

            while (true)
            {
                if (SurfaceDetails.CeilingFound)
                {
                    ChangeState(State.DrawPerimeter);
                }

                if (startedTime - Time.realtimeSinceStartup > _ceilingHuntMaxDuration)
                {
                    Debug.Log("Timed out locating ceiling.");
                    ChangeState(State.DrawPerimeter);
                }
                yield return null;
            }
        }

        private void DrawPerimeter_Enter()
        {
            ShowGUI(drawPerimeterGUI);

            //reset:
            _plottedCorners.Clear();

            //hook:
            MLInput.OnTriggerDown += DrawPerimeter_HandleTriggerDown;
        }

        private void DrawPerimeter_HandleTriggerDown(byte controllerId, float triggerValue)
        {
            //cache room details (to avoid tilt if room analyzation updates):
            if (_plottedCorners.Count == 0)
            {
                _roomVerticalCenter = SurfaceDetails.VerticalCenter;
                _roomCeilingHeight = SurfaceDetails.CeilingHeight;
                _roomFloorHeight = SurfaceDetails.FloorHeight;
                Height = SurfaceDetails.RoomHeight;
            }

            //corner details:
            Vector3 controlPosition = MLInput.GetController(controllerId).Position;
            Vector3 cornerLocation = new Vector3(controlPosition.x, _roomVerticalCenter, controlPosition.z);

            //invalid placement if this proposed segment would overlap any previous ones:
            if (_plottedCorners.Count >= 3)
            {
                //proposed line segment:
                Vector2 proposedStart = new Vector2(_plottedCorners[_plottedCorners.Count - 1].position.x, _plottedCorners[_plottedCorners.Count - 1].position.z);
                Vector2 proposedEnd = new Vector2(cornerLocation.x, cornerLocation.z);

                //look for any intersections (in 2d):
                for (int i = 1; i < _plottedCorners.Count; i++)
                {
                    //get a pervious segment:
                    Vector2 startA = new Vector2(_plottedCorners[i - 1].position.x, _plottedCorners[i - 1].position.z);
                    Vector2 endA = new Vector2(_plottedCorners[i].position.x, _plottedCorners[i].position.z);

                    //is there an intersection with something previous?
                    Vector2 previousIntersection = Vector2.zero;
                    if (MathUtilities.LineSegmentsIntersecting(startA, endA, proposedStart, proposedEnd, false))
                    {
                        //ignore this proposed corner since it would create an overlapped wall:
                        return;
                    }
                }
            }

            //bounds:
            if (_plottedCorners.Count == 0)
            {
                _plottedBounds = new Bounds(cornerLocation, Vector3.zero);
            }
            else
            {
                _plottedBounds.Encapsulate(cornerLocation);
            }

            //loop complete?
            bool loopComplete = false;
            if (_plottedCorners.Count > 3)
            {
                //close to first? close to last? close the loop:
                if (Vector3.Distance(cornerLocation, _plottedCorners[0].position) <= _loopClosureDistance || Vector3.Distance(cornerLocation, _plottedCorners[_plottedCorners.Count - 1].position) <= _loopClosureDistance)
                {
                    _plottedCorners.Add(_plottedCorners[0]);
                    loopComplete = true;
                }
            }

            if (!loopComplete)
            {
                //visualize corner:
                GameObject corner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                corner.GetComponent<Renderer>().material = cornerMaterial;
                corner.name = $"(Corner{_plottedCorners.Count})";
                corner.transform.position = cornerLocation;
                corner.transform.localScale = new Vector3(0.0508f, Height * .5f, 0.0508f);
                _plottedCorners.Add(corner.transform);

                //visualize boundry:
                Lines.DrawLine("Boundry", Color.white, Color.white, .01f, _plottedCorners.ToArray());
            }
            else
            {
                //visualize boundry:
                Lines.DrawLine("Boundry", Color.white, Color.white, .01f, _plottedCorners.ToArray());

                ChangeState(State.FindWalls);
            }
        }

        private void DrawPerimeter_Exit()
        {
            //unhook:
            MLInput.OnTriggerDown -= DrawPerimeter_HandleTriggerDown;
        }

        private void FindWalls_Enter()
        {
            //generate virtual walls:
            _virtualWalls.Clear();
            for (int i = 1; i < _plottedCorners.Count; i++)
            {
                _virtualWalls.Add(new VirtualWall(_plottedCorners[i - 1].position, _plottedCorners[i].position));
            }

            //discover winding direction:
            float angleCount = 0;
            for (int i = 1; i < _plottedCorners.Count; i++)
            {
                Vector3 centerToPrevious = Vector3.Normalize(_plottedCorners[i - 1].position - _plottedBounds.center);
                Vector3 centerToCurrent = Vector3.Normalize(_plottedCorners[i].position - _plottedBounds.center);
                angleCount += Vector3.SignedAngle(centerToPrevious, centerToCurrent, Vector3.up);
            }
            float windingDirection = Mathf.Sign(angleCount); //1 = clockwise, -1 = counterclockwise

            //set normals:
            for (int i = 0; i < _virtualWalls.Count; i++)
            {
                _virtualWalls[i].normal = Vector3.Normalize(Quaternion.AngleAxis(90 * windingDirection, Vector3.up) * _virtualWalls[i].Vector);
            }

            //generate world plane queries:
            for (int i = 0; i < _virtualWalls.Count; i++)
            {
                //establish and cache query:
                MLPlanes.QueryParams query = new MLPlanes.QueryParams();
                query.MaxResults = 10;
                query.Flags = MLPlanes.QueryFlags.Vertical | MLPlanes.QueryFlags.OrientToGravity | MLPlanes.QueryFlags.Inner;
                query.BoundsCenter = _virtualWalls[i].Center - _virtualWalls[i].normal * (maxPlaneDepthTest * .5f);
                query.BoundsRotation = Quaternion.LookRotation(_virtualWalls[i].normal);
                query.BoundsExtents = new Vector3(_virtualWalls[i].Vector.magnitude, _plottedCorners[i].localScale.y, maxPlaneDepthTest);
                _virtualWalls[i].query = query;
            }

            //execute world planes queries:
            _queryCount = -1;
            FindWalls_ResolveToPhysicalWalls();
        }

        private void FindWalls_ResolveToPhysicalWalls()
        {
            _queryCount++;
            MLPlanes.GetPlanes(_virtualWalls[_queryCount].query, FindWalls_HandleLocatePhysicalWallPlaneQuery);
        }

        private void FindWalls_HandleLocatePhysicalWallPlaneQuery(MLResult result, MLPlanes.Plane[] planes, MLPlanes.Boundaries[] boundaries)
        {
            //resets:
            Vector3 sourceWallCenter = Vector3.Lerp(_plottedCorners[_queryCount].position, _plottedCorners[_queryCount + 1].position, .5f);

            //find furthest:
            int primeID = -1;
            float largest = 0;
            for (int i = 0; i < planes.Length; i++)
            {
                //check to make sure this plane is close to the same orientation as the virtual one:
                float dot = Vector3.Dot(planes[i].Rotation * Vector3.back, _virtualWalls[_queryCount].normal);

                //candidate - test for largest plane:
                if (dot > .9f)
                {
                    float size = planes[i].Width + planes[i].Height;
                    if (size > largest)
                    {
                        largest = size;
                        primeID = i;
                    }
                }
            }

            //use found wall or just use boundry from virtual wall:
            if (primeID != -1)
            {
                MLPlanes.Plane foundPlane = planes[primeID];
                foundPlane.Center.y = _roomVerticalCenter;
                foundPlane.Width = _virtualWalls[_queryCount].Vector.magnitude;
                foundPlane.Height = Height;
                _virtualWalls[_queryCount].plane = foundPlane;
            }
            else
            {
                //if we didn't find a physical wall let's just make the virtual wall a boundary plane as is:
                MLPlanes.Plane plane = new MLPlanes.Plane();
                Vector3 center = _virtualWalls[_queryCount].Center;
                center.y = _roomVerticalCenter;
                plane.Center = center;
                plane.Rotation = Quaternion.LookRotation(_virtualWalls[_queryCount].normal);
                plane.Width = _virtualWalls[_queryCount].Vector.magnitude;
                plane.Height = Height;
                _virtualWalls[_queryCount].plane = plane;
                _virtualWalls[_queryCount].physical = false;
            }

            //continue additional queries:
            if (_queryCount < _virtualWalls.Count - 1)
            {
                FindWalls_ResolveToPhysicalWalls();
            }
            else
            {
                FindWalls_LocateBoundry();
            }
        }

        private void FindWalls_LocateBoundry()
        {
            //remove initial guides:
            RemovePlottedBounds();

            //sort by angles from a base vector to find clockwise order:
            Vector3 baseVector = Vector3.Normalize(_virtualWalls[0].plane.Center - _plottedBounds.center);
            SortedDictionary<float, int> sortedDirection = new SortedDictionary<float, int>();
            for (int i = 0; i < _virtualWalls.Count; i++)
            {
                Vector3 toNext = Vector3.Normalize(_virtualWalls[i].plane.Center - _plottedBounds.center);
                float angle = Vector3.SignedAngle(baseVector, toNext, Vector3.up) + 180;

                if (sortedDirection.ContainsKey(angle))
                {
                    //we have a bad set of locations so it is best to just everything:
                    Create();
                    break;
                }
                else
                {
                    sortedDirection.Add(angle, i);
                }
            }
            int[] clockwiseOrder = sortedDirection.Values.ToArray<int>();

            //find and connect 'betweens' which end up being final walls of playspace
            List<bool> physicalStatus = new List<bool>();
            _playspaceCorners = new List<Vector3>();
            for (int i = 0; i < clockwiseOrder.Length; i++)
            {
                //parts:
                int next = (i + 1) % clockwiseOrder.Length;
                float angle = Vector3.Angle(_virtualWalls[clockwiseOrder[i]].plane.Rotation * Vector3.right, _virtualWalls[clockwiseOrder[next]].plane.Rotation * Vector3.right);

                //save physical status:
                physicalStatus.Add(_virtualWalls[clockwiseOrder[next]].physical);

                //add solved between:
                if (angle < 45 || angle > 135)
                {
                    //wall - use mid point:
                    Vector3 mid = Vector3.Lerp(_virtualWalls[clockwiseOrder[i]].plane.Center, _virtualWalls[clockwiseOrder[next]].plane.Center, .5f);
                    mid.y = _roomFloorHeight;
                    _playspaceCorners.Add(TransformUtilities.LocalPosition(pcfAnchor.Position, pcfAnchor.Rotation, mid));
                }
                else
                {
                    //corner - use intersection by creating inverted lines from each plane:
                    Vector3 point = Vector2.zero;
                    if (MathUtilities.RayIntersection(
                        new Ray(_virtualWalls[clockwiseOrder[i]].plane.Center, _virtualWalls[clockwiseOrder[i]].plane.Rotation * Vector3.right),
                        new Ray(_virtualWalls[clockwiseOrder[next]].plane.Center, _virtualWalls[clockwiseOrder[next]].plane.Rotation * Vector3.left),
                        ref point))
                    {
                        point.y = _roomFloorHeight;
                        _playspaceCorners.Add(TransformUtilities.LocalPosition(pcfAnchor.Position, pcfAnchor.Rotation, point));
                    }
                }
            }

            //close loop:
            _playspaceCorners.Add(_playspaceCorners[0]);

            //store walls:
            List<PlayspaceWall> playspaceWalls = new List<PlayspaceWall>();
            for (int i = 0; i < _playspaceCorners.Count - 1; i++)
            {
                Vector3 pointA = TransformUtilities.WorldPosition(pcfAnchor.Position, pcfAnchor.Rotation, _playspaceCorners[i]);
                Vector3 pointB = TransformUtilities.WorldPosition(pcfAnchor.Position, pcfAnchor.Rotation, _playspaceCorners[i + 1]);
                Vector3 vector = pointB - pointA;
                Vector3 normal = Quaternion.AngleAxis(90, Vector3.up) * vector.normalized;
                Vector3 center = Vector3.Lerp(pointA, pointB, .5f);
                center.y = _roomVerticalCenter;
                float width = vector.magnitude;
                PlayspaceWall wall = new PlayspaceWall(center, Quaternion.LookRotation(normal), width, Height, physicalStatus[i]);
                playspaceWalls.Add(wall);
            }
            Walls = playspaceWalls.ToArray();

            FindWalls_GenerateGeometry();
        }

        private void FindWalls_GenerateGeometry()
        {
            //transform matrix:
            Vector3 correctedForward = Vector3.ProjectOnPlane(pcfAnchor.Rotation * Vector3.forward, Vector3.up).normalized;
            Quaternion correctedRotation = Quaternion.LookRotation(correctedForward);
            Matrix4x4 transformMatrix = Matrix4x4.TRS(pcfAnchor.Position, correctedRotation, Vector3.one);

            //build wall geometry:
            Vector3 verticalOffset = Vector3.up * Height;
            List<Vector3> wallVerticies = new List<Vector3>();
            List<int> wallTriangles = new List<int>();
            for (int i = 0; i < _playspaceCorners.Count - 1; i++)
            {
                Vector3 pointA = TransformUtilities.WorldPosition(pcfAnchor.Position, pcfAnchor.Rotation, _playspaceCorners[i]);
                Vector3 pointB = TransformUtilities.WorldPosition(pcfAnchor.Position, pcfAnchor.Rotation, _playspaceCorners[i + 1]);

                //locate verticies (local space):
                Vector3 a = transformMatrix.inverse.MultiplyPoint3x4(pointA);
                Vector3 b = transformMatrix.inverse.MultiplyPoint3x4(pointB);
                Vector3 c = b + verticalOffset;
                Vector3 d = a + verticalOffset;

                //store verticies (reverse order so normals face inwards):
                wallVerticies.Add(d);
                wallVerticies.Add(c);
                wallVerticies.Add(b);
                wallVerticies.Add(a);

                //store triangles
                wallTriangles.Add(wallVerticies.Count - 4);
                wallTriangles.Add(wallVerticies.Count - 3);
                wallTriangles.Add(wallVerticies.Count - 2);
                wallTriangles.Add(wallVerticies.Count - 2);
                wallTriangles.Add(wallVerticies.Count - 1);
                wallTriangles.Add(wallVerticies.Count - 4);
            }

            //get points for polygon construction:
            Vector2[] polygonPoints = new Vector2[_playspaceCorners.Count - 1];
            Vector3[] floorVerticies = new Vector3[_playspaceCorners.Count - 1];
            Vector3[] ceilingVerticies = new Vector3[_playspaceCorners.Count - 1];
            for (int i = 0; i < _playspaceCorners.Count - 1; i++)
            {
                Vector3 point = TransformUtilities.WorldPosition(pcfAnchor.Position, pcfAnchor.Rotation, _playspaceCorners[i]);
                Vector3 local = transformMatrix.inverse.MultiplyPoint3x4(point);
                polygonPoints[i] = new Vector2(local.x, local.z);
                floorVerticies[i] = local;
                ceilingVerticies[i] = local + verticalOffset;
            }

            //triangles:
            int[] floorTriangles = new Triangulator(polygonPoints).Triangulate();

            BuildGeometry(wallVerticies.ToArray(), wallTriangles.ToArray(), floorVerticies, floorTriangles, ceilingVerticies);

            //FindWalls_Analyze();
            ChangeState(State.ConfirmArea);
        }

        private void ConfirmArea_Enter()
        {
            //gui:
            ShowGUI(confirmAreaGUI);

            PreviewArea();

            //hook:
            MLInput.OnControllerButtonDown += ConfirmArea_HandleControllerButton;
            MLInput.OnTriggerDown += ConfirmArea_HandleTriggerDown;
        }

        private void ConfirmArea_HandleTriggerDown(byte controllerId, float triggerValue)
        {
            ChangeState(State.ConfirmPrimaryWall);
        }

        private void ConfirmArea_HandleControllerButton(byte controllerId, MLInput.Controller.Button button)
        {
            switch (button)
            {
                case MLInput.Controller.Button.Bumper:
                    PlayerPrefs.DeleteKey(_sessionDataKey);
                    PlayerPrefs.DeleteKey(_sessionMeshKey);
                    Create();
                    break;
            }
        }

        private void ConfirmArea_Exit()
        {
            //unhook:
            MLInput.OnControllerButtonDown -= ConfirmArea_HandleControllerButton;
            MLInput.OnTriggerDown -= ConfirmArea_HandleTriggerDown;
        }

        private void ConfirmAreaReload_Enter()
        {
            //gui:
            ShowGUI(confirmAreaGUI);
            Vector3 primaryPlaqueLocation = Walls[PrimaryWall].Center + Vector3.up * .5f;
            primaryWallPlaque.transform.SetPositionAndRotation(primaryPlaqueLocation, Quaternion.LookRotation(Walls[PrimaryWall].Back));
            primaryWallPlaque.transform.Translate(Vector3.back * .1f); //pull primary plaque off the wall a little to avoid clipping
            primaryWallPlaque.SetActive(true);

            PreviewArea();

            //hook:
            MLInput.OnControllerButtonDown += ConfirmArea_HandleControllerButton;
            MLInput.OnTriggerDown += ConfirmAreaReload_HandleTriggerDown;
        }

        private void ConfirmAreaReload_HandleTriggerDown(byte controllerId, float triggerValue)
        {
            ChangeState(State.Complete);
        }

        private void ConfirmAreaReload_Exit()
        {
            //unhook:
            MLInput.OnControllerButtonDown -= ConfirmArea_HandleControllerButton;
            MLInput.OnTriggerDown -= ConfirmAreaReload_HandleTriggerDown;
        }

        private void ConfirmPrimaryWall_Enter()
        {
            ShowGUI(selectPrimaryWallGUI);

            //hook:
            MLInput.OnTriggerDown += ConfirmPrimaryWall_HandleTriggerDown;
        }

        private IEnumerator ConfirmPrimaryWall_Update()
        {
            while (true)
            {
                Vector3 intersection = Vector3.zero;
                PrimaryWall = FacingWall(_camera.position, _camera.forward, ref intersection);
                if (intersection != Vector3.zero)
                {
                    //push gui back to wall and enlarge it:
                    intersection.y = selectPrimaryWallGUI.transform.position.y;
                    selectPrimaryWallGUI.transform.position = intersection;
                    selectPrimaryWallGUI.transform.localScale = _cachedPrimaryWallGUIScale * 2;

                    //match gui to normal of wall and then push it off the wall a bit to avoid clipping:
                    selectPrimaryWallGUI.transform.rotation = Quaternion.LookRotation(Walls[PrimaryWall].Back);
                    selectPrimaryWallGUI.transform.Translate(Vector3.back * .1f);
                }
                else
                {
                    //put gui back in front of and facing the user:
                    selectPrimaryWallGUI.transform.localPosition = Vector3.zero;
                    selectPrimaryWallGUI.transform.localRotation = Quaternion.identity;
                    selectPrimaryWallGUI.transform.localScale = _cachedPrimaryWallGUIScale;
                }
                yield return null;
            }
        }

        private void ConfirmPrimaryWall_HandleTriggerDown(byte controllerId, float triggerValue)
        {
            //we must have a primary wall:
            if (PrimaryWall == -1)
            {
                return;
            }

            //reset walls:
            RightWall = -1;
            LeftWall = -1;
            RearWall = -1;

            //find room center (this may need additional calculation for more accuracy):
            Bounds finalBounds = new Bounds(Walls[0].Center, Vector3.zero);
            for (int i = 1; i < Walls.Length; i++)
            {
                finalBounds.Encapsulate(Walls[i].Center);
            }

            //make center relative to pcf so relocalization keeps this value accurate:
            _playspaceCenter = TransformUtilities.LocalPosition(pcfAnchor.Position, pcfAnchor.Rotation, finalBounds.center);

            //find additional largest key walls:
            SortedDictionary<float, int> rightWalls = new SortedDictionary<float, int>();
            SortedDictionary<float, int> rearWalls = new SortedDictionary<float, int>();
            SortedDictionary<float, int> leftWalls = new SortedDictionary<float, int>();
            for (int i = 0; i < Walls.Length; i++)
            {
                //skip walls that match the primary:
                if (i != PrimaryWall) //skip primary wall and walls on the same plane
                {
                    //get angle relationship:
                    float angle = Vector3.SignedAngle(Walls[PrimaryWall].Normal, Walls[i].Normal, Vector3.up);
                    if (angle < 0)
                    {
                        angle += 360;
                    }

                    //get size:
                    float wallSize = Walls[i].width * Walls[i].height;

                    //right:
                    if (angle >= 225 && angle < 315)
                    {
                        leftWalls.Add(wallSize, i);
                    }

                    //rear:
                    if (angle > 135 && angle < 225)
                    {
                        rearWalls.Add(wallSize, i);
                    }

                    //left:
                    if (angle >= 45 && angle <= 135)
                    {
                        rightWalls.Add(wallSize, i);
                    }
                }
            }

            //set key walls to largest (might need to verify they are also facing the correct direction as well later):
            if (rightWalls.Count != 0)
            {
                RightWall = rightWalls.ElementAt(rightWalls.Count - 1).Value;
            }

            if (rearWalls.Count != 0)
            {
                RearWall = rearWalls.ElementAt(rearWalls.Count - 1).Value;
            }

            if (leftWalls.Count != 0)
            {
                LeftWall = leftWalls.ElementAt(leftWalls.Count - 1).Value;
            }

            //unhook:
            MLInput.OnTriggerDown -= ConfirmPrimaryWall_HandleTriggerDown;

            //something went wrong with the shape of the room, best to force a rest:
            if (PrimaryWall == -1 || LeftWall == -1 || RightWall == -1 || RearWall == -1)
            {
                Create();
            }
            else
            {
                ChangeState(State.Complete);
            }
        }

        private void Complete_Enter()
        {
            Ready = true;

            //serialize session:
            string sessionData = "";
            sessionData += $"{pcfAnchor.CFUID.ToString()}"; //pcf
            sessionData += $"|{SerializationUtilities.Serialize(Walls)}"; //walls array
            sessionData += $"|{JsonUtility.ToJson(_playspaceCenter)}"; //center
            sessionData += $"|{PrimaryWall}|{RightWall}|{RearWall}|{LeftWall}"; //wall ids (clockwise)
            sessionData += $"|{SerializationUtilities.Serialize(_playspaceCorners.ToArray())}"; //playspace corners
            sessionData += $"|{(Height)}"; //room dimensions

            //save session:
            PlayerPrefs.SetString(_sessionDataKey, sessionData);
            PlayerPrefs.SetString(_sessionMeshKey, _serializedMeshes);
            PlayerPrefs.Save();

            OnCompleted?.Invoke();

            //clean up:
            HideGUI();
            RemovePlottedBounds();
            RemoveDebugLines();

            ChangeState(State.Idle);
        }
#endif
    }
}