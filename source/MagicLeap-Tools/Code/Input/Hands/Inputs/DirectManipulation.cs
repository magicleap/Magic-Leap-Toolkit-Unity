// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeapTools
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Collider))]
    public class DirectManipulation : MonoBehaviour, IDirectManipulation
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public Color idleColor = Color.white;
        public Color grabbedColor = Color.green;
        public AudioClip grabbedSound;
        public AudioClip releasedSound;
        [Tooltip("Can this object be moved by a hand?")]
        public bool draggable = true;
        [Tooltip("Can this object be rotated by a hand?")]
        public bool rotatable = true;
        [Tooltip("Does velocity get applied when released?")]
        public bool throwable = true;
        [Tooltip("Can this object be scaled by two hands?")]
        public bool scalable = true;
        [Tooltip("Minimum scale is scalable is true.")]
        public float minScale;
        [Tooltip("Maximum scale is scalable is true.")]
        public float maxScale;
        [Tooltip("All of the interaction points that are actively grabbing.")]
        public List<InteractionPoint> activeInteractionPoints = new List<InteractionPoint>();
        [Tooltip("How slow should we move to the target position?")]
        public float positionSmoothTime = .025f;
        [Tooltip("How slow should we move to the target rotation?")]
        public float rotationSmoothTime = .05f;
        [Tooltip("Reposition when grabbed with a fist? Leaving this at 0,0,0 will snap to the hand position.")]
        public bool repositionOnFistGrab;
        [Tooltip("Positional offset from the right hand fist used when reorientOnFistGrab is true - mirrored for the left hand.")]
        public Vector3 grabPositionOffset;
        [Tooltip("Reorient when grabbed with a fist? Leaving this at 0,0,0 will snap to the hand orientation.")]
        public bool reorientOnFistGrab;
        [Tooltip("Rotational offset from the right hand fist used when repositionOnFistGrab is true - mirrored for the left hand.")]
        public Vector3 grabRotationOffset;
        [Tooltip("Reposition when grabbed with a pinch? Leaving this at 0,0,0 will snap to the interaction point (between the index and thumb tip).")]
        public bool repositionOnPinchGrab;
        [Tooltip("Positional offset from the right hand pinch used when reorientOnPinchGrab is true - mirrored for the left hand.")]
        public Vector3 pinchPositionOffset;
        [Tooltip("Reorient when grabbed with a pinch? Leaving this at 0,0,0 will use the interaction point's rotation (between the index and thumb tip).")]
        public bool reorientOnPinchGrab;
        [Tooltip("Rotational offset from the right hand pinch used when repositionOnPinchGrab is true - mirrored for the left hand.")]
        public Vector3 pinchRotationOffset;
        [Tooltip("Should we drop when released?")]
        public bool enableGravityOnRelease;

        //Events:
        /// <summary>
        /// Thrown when a Pinch or Grasp occurs on this object.
        /// </summary>
        public InteractionPointEvent OnGrabBegin = new InteractionPointEvent();
        /// <summary>
        /// Thrown when a Pinch or Grasp operation begins to change location while operating on this object.
        /// </summary>
        public InteractionPointEvent OnDragBegin = new InteractionPointEvent();
        /// <summary>
        /// Thrown when a Pinch or Grasp operation changes location while operating on this object.
        /// </summary>
        public InteractionPointDragEvent OnDragUpdate = new InteractionPointDragEvent();
        /// <summary>
        /// Thrown when a Pinch or Grasp releases from this object.
        /// </summary>
        public InteractionPointEvent OnDragEnd = new InteractionPointEvent();
        /// <summary>
        /// Thrown when a Pinch or Grasp releases from this object.
        /// </summary>
        public InteractionPointEvent OnGrabEnd = new InteractionPointEvent();

        //Public Properties:
        /// <summary>
        /// Is at least one interaction point interacting with us?
        /// </summary>
        public bool Grabbed
        {
            get;
            private set;
        }

        /// <summary>
        /// Are we being dragged?
        /// </summary>
        public bool Dragging
        {
            get;
            private set;
        }

        //Private Variables:
        private Dictionary<InteractionPoint, Pose> _initialInteractionPoses = new Dictionary<InteractionPoint, Pose>();
        private float _dragThreshold = .008f;
        private Pose _offset;
        private string _handConnectionLine = "BimanualConnection";
        private Rigidbody _rigidBody;
        private Quaternion _bimanualBaseRotation;
        private Transform _camera;
        private float _scaleInitialDistance;
        private Vector3 _scaleBase;
        private List<Vector3> _averageVelocity = new List<Vector3>();
        private List<Vector3> _averageAngularVelocity = new List<Vector3>();
        private int _averageVelocityCount = 8;
        private int _averageVelocityEndTrim = 4;
        private Vector3 _positionVelocity;
        private Quaternion _rotationVelocity;
        private Vector3 _position;
        private Quaternion _rotation;
        private AudioSource _audioSource;

        //Init:
        private void Reset()
        {
            //set initial scale limits to something logical:
            minScale = Mathf.Min(transform.localScale.x, transform.localScale.y, transform.localScale.z) * .25f;
            maxScale = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z) * 2.25f;
        }

        private void Awake()
        {
            //refs:
            _camera = Camera.main.transform;
            _rigidBody = GetComponent<Rigidbody>();
            _audioSource = GetComponents<AudioSource>()[0];
        }

        //Public Methods:
        public void GrabBegan(InteractionPoint interactionPoint)
        {
            if (activeInteractionPoints.Contains(interactionPoint))
            {
                return;
            }

            //sound:
            if (grabbedSound != null)
            {
                _audioSource.PlayOneShot(grabbedSound, 1);
            }

            //colorize:
            foreach (var item in GetComponentsInChildren<Renderer>())
            {
                if (item.material.HasProperty("_Color"))
                {
                    item.material.SetColor("_Color", grabbedColor);
                }
            }

            //grasped?
            if (interactionPoint.managedHand.Gesture.Grasp.active)
            {
                if (reorientOnFistGrab)
                {
                    //initial set:
                    transform.rotation = interactionPoint.managedHand.Skeleton.Rotation;

                    //offset:
                    if (interactionPoint.managedHand.Hand.Type == MLHandTracking.HandType.Left)
                    {
                        grabRotationOffset.z *= -1;
                    }
                    transform.Rotate(grabRotationOffset);
                }

                if (repositionOnFistGrab)
                {
                    //initial set:
                    transform.position = interactionPoint.managedHand.Skeleton.Position;
                    
                    //offset:
                    if (interactionPoint.managedHand.Hand.Type == MLHandTracking.HandType.Left)
                    {
                        grabPositionOffset.x *= -1;
                    }
                    transform.Translate(grabPositionOffset);
                }
            }

            //pinched?
            if (interactionPoint.managedHand.Gesture.Pinch.active)
            {
                if (reorientOnPinchGrab)
                {
                    //initial set:
                    transform.rotation = interactionPoint.rotation;

                    //offset:
                    if (interactionPoint.managedHand.Hand.Type == MLHandTracking.HandType.Left)
                    {
                        pinchRotationOffset.z *= -1;
                    }
                    transform.Rotate(pinchRotationOffset);
                }

                if (repositionOnPinchGrab)
                {
                    //initial set:
                    transform.position = interactionPoint.position;

                    //offset:
                    if (interactionPoint.managedHand.Hand.Type == MLHandTracking.HandType.Left)
                    {
                        pinchPositionOffset.x *= -1;
                    }
                    transform.Translate(pinchPositionOffset);
                }
            }

            Grabbed = true;

            activeInteractionPoints.Add(interactionPoint);
            _initialInteractionPoses.Add(interactionPoint, new Pose(interactionPoint.position, interactionPoint.rotation));
            interactionPoint.managedHand.OnVisibilityChanged += HandleHandVisibility;
            OnGrabBegin?.Invoke(interactionPoint);

            //bimanual offsets:
            if (activeInteractionPoints.Count == 2)
            {
                Dragging = true;

                //base bimanual rotation:
                Plane plane = new Plane(activeInteractionPoints[0].position, activeInteractionPoints[1].position, _camera.position);
                Vector3 forward = activeInteractionPoints[0].position - activeInteractionPoints[1].position;
                _bimanualBaseRotation = Quaternion.LookRotation(forward.normalized, plane.normal);
                _offset.rotation = Quaternion.Inverse(_bimanualBaseRotation) * transform.rotation;

                //bimanual offset:
                Vector3 midPoint = Vector3.Lerp(activeInteractionPoints[0].position, activeInteractionPoints[1].position, .5f);
                Matrix4x4 matrix = Matrix4x4.TRS(midPoint, _bimanualBaseRotation, Vector3.one);
                _offset.position = matrix.inverse.MultiplyPoint3x4(transform.position);

                //scale
                _scaleBase = transform.localScale;
                _scaleInitialDistance = forward.magnitude;
            }
        }

        public void GrabUpdate(InteractionPoint interactionPoint)
        {
            //dragging started?
            if (!Dragging)
            {
                foreach (var item in _initialInteractionPoses)
                {
                    if (Vector3.Distance(item.Key.position, item.Value.position) > _dragThreshold)
                    {
                        //halt physics:
                        if (_rigidBody != null)
                        {
                            _rigidBody.velocity = Vector3.zero;
                            _rigidBody.angularVelocity = Vector3.zero;
                            _rigidBody.isKinematic = true;
                        }

                        //find offset:
                        Matrix4x4 matrix = Matrix4x4.TRS(item.Key.position, item.Key.rotation, Vector3.one);
                        Vector3 positionOffset = matrix.inverse.MultiplyPoint3x4(transform.position);
                        Quaternion rotationOffset = Quaternion.Inverse(item.Key.rotation) * transform.rotation;
                        _offset = new Pose(positionOffset, rotationOffset);

                        //resets:
                        _position = Vector3.zero;
                        _rotation = Quaternion.identity;

                        //status:
                        Dragging = true;
                        OnDragBegin?.Invoke(item.Key);
                        break;
                    }
                }
            }
            
            //dragging?
            if (Dragging)
            {
                //find center of interaction points involved with this drag:
                Bounds dragBounds = new Bounds(activeInteractionPoints[0].position, Vector3.zero);
                for (int i = 1; i < activeInteractionPoints.Count; i++)
                {
                    dragBounds.Encapsulate(activeInteractionPoints[i].position);
                }

                //discover drag distance and percentage;
                float biManualDistance = dragBounds.size.magnitude;

                //holders:
                Vector3 dragLocation = Vector3.zero;
                Quaternion dragOrientation = Quaternion.identity;
                Vector3 forward = Vector3.zero;
                float scaleDelta = 0;
 
                //rotation:
                if (activeInteractionPoints.Count == 2)
                {
                    //get rotated amount:
                    forward = activeInteractionPoints[0].position - activeInteractionPoints[1].position;
                    Vector3 forwardNormalized = forward.normalized;
                    Vector3 previousForward = _bimanualBaseRotation * Vector3.forward;
                    Vector3 up = Vector3.Cross(forwardNormalized, previousForward).normalized;
                    float angle = Vector3.SignedAngle(previousForward, forwardNormalized, up);

                    //update rotation:
                    Quaternion rotationDelta = Quaternion.AngleAxis(angle, up);
                    _bimanualBaseRotation = rotationDelta * _bimanualBaseRotation;
                    dragOrientation = _bimanualBaseRotation * _offset.rotation;
                }
                else
                {
                    dragOrientation = activeInteractionPoints[0].rotation * _offset.rotation;
                }

                //position:
                if (activeInteractionPoints.Count == 2)
                {
                    Matrix4x4 matrix = Matrix4x4.TRS(dragBounds.center, _bimanualBaseRotation, Vector3.one);
                    dragLocation = matrix.MultiplyPoint3x4(_offset.position);
                }
                else
                {
                    Matrix4x4 matrix = Matrix4x4.TRS(activeInteractionPoints[0].position, activeInteractionPoints[0].rotation, Vector3.one);
                    dragLocation = matrix.MultiplyPoint3x4(_offset.position);
                }

                //scale:
                if (activeInteractionPoints.Count == 2)
                {
                    scaleDelta = biManualDistance - _scaleInitialDistance;
                }

                //set smoothing origins:
                if (_position == Vector3.zero || _rotation == Quaternion.identity)
                {
                    _position = dragLocation;
                    _rotation = dragOrientation;
                }

                //application:
                if (draggable)
                {
                    _position = Vector3.SmoothDamp(_position, dragLocation, ref _positionVelocity, positionSmoothTime);
                    if (_rigidBody != null)
                    {
                        _rigidBody.MovePosition(_position);
                        _averageVelocity.Add(_rigidBody.velocity);
                        _averageAngularVelocity.Add(_rigidBody.angularVelocity);
                    }
                    else
                    {
                        transform.position = _position;
                    }
                }

                if (rotatable)
                {
                    _rotation = MotionUtilities.SmoothDamp(_rotation, dragOrientation, ref _rotationVelocity, rotationSmoothTime);
                    if (_rigidBody != null)
                    {
                        _rigidBody.MoveRotation(_rotation);
                    }
                    else
                    {
                        transform.rotation = _rotation;
                    }
                }

                if (scalable && activeInteractionPoints.Count == 2)
                {
                    Vector3 proposedScale = _scaleBase + (Vector3.one * scaleDelta);

                    //constrain scale and do not not assume uniform initial scale:
                    if (Mathf.Max(proposedScale.x, proposedScale.y, proposedScale.z) <= maxScale && 
                        Mathf.Min(proposedScale.x, proposedScale.y, proposedScale.z) >= minScale)
                    {
                        transform.localScale = _scaleBase + (Vector3.one * scaleDelta);
                    }
                }

                //visuals:
                if (activeInteractionPoints.Count == 2)
                {
                    Lines.SetVisibility(_handConnectionLine, true);
                    Lines.DrawLine(_handConnectionLine, Color.cyan, Color.cyan, .0005f, activeInteractionPoints[0].position, activeInteractionPoints[1].position);
                }

                //status:
                OnDragUpdate?.Invoke(activeInteractionPoints.ToArray(), _position, _rotation, scaleDelta);
            }
        }

        /// <summary>
        /// Force the end of a grab.
        /// </summary>
        public void StopGrab()
        {
            //copy to array before forcing a grab end:
            foreach (var item in activeInteractionPoints.ToArray())
            {
                GrabEnd(item);
            }
        }

        public void GrabEnd(InteractionPoint interactionPoint)
        {
            //avoid issues with an actual physical release if StopGrab was used prior:
            if (activeInteractionPoints.Count == 0)
            {
                return;
            }

            //sound:
            if (releasedSound != null)
            {
                _audioSource.PlayOneShot(releasedSound, 1);
            }

            //colorize:
            foreach (var item in GetComponentsInChildren<Renderer>())
            {
                if (item.material.HasProperty("_Color"))
                {
                    item.material.SetColor("_Color", idleColor);
                }
            }

            Grabbed = false;

            //clear up bimanual manipulation:
            if (activeInteractionPoints.Count == 2)
            {
                activeInteractionPoints.Clear();
                _initialInteractionPoses.Clear();
            }

            //remove:
            _initialInteractionPoses.Remove(interactionPoint);
            activeInteractionPoints.Remove(interactionPoint);

            if (Dragging && activeInteractionPoints.Count == 0)
            {
                if (_rigidBody != null)
                {
                    if (throwable && activeInteractionPoints.Count == 0)
                    {
                        //gavity?
                        if (enableGravityOnRelease)
                        {
                            _rigidBody.useGravity = true;
                        }

                        //calculate an average velocity:
                        Vector3 velocity = Vector3.zero;
                        int start = _averageVelocity.Count - _averageVelocityCount;
                        start = Mathf.Clamp(start, _averageVelocityEndTrim, start);
                        int count = 0;
                        for (int i = start; i < _averageVelocity.Count; i++)
                        {
                            velocity += _averageVelocity[i];
                            count++;
                        }
                        velocity /= count;
                        _rigidBody.velocity = velocity;

                        //calculate an average angular velocity:
                        velocity = Vector3.zero;
                        start = _averageAngularVelocity.Count - _averageVelocityCount;
                        start = Mathf.Clamp(start, _averageVelocityEndTrim, start);
                        count = 0;
                        for (int i = start; i < _averageAngularVelocity.Count; i++)
                        {
                            velocity += _averageAngularVelocity[i];
                            count++;
                        }
                        velocity /= count;
                        _rigidBody.angularVelocity = velocity;

                        //clear:
                        _averageVelocity.Clear();
                        _averageAngularVelocity.Clear();
                    }
                    else
                    {
                        _rigidBody.velocity = Vector3.zero;
                        _rigidBody.angularVelocity = Vector3.zero;
                    }

                    _rigidBody.isKinematic = false;
                }
                
                Dragging = false;
                OnDragEnd?.Invoke(interactionPoint);
                Lines.SetVisibility(_handConnectionLine, false);
            }

            OnGrabEnd?.Invoke(interactionPoint);
        }

        //Event Handlers:
        private void HandleHandVisibility(ManagedHand managedHand, bool visible)
        {
            //cancel bimanual operations on FOS breach:
            if (!visible && activeInteractionPoints.Count == 2)
            {
                activeInteractionPoints[0].Touch.FireEnd();
                return;
            }

            //hide when hand leaves FOS while dragging - needs repair:
            //if (activeInteractionPoints.Count == 1)
            //{
            //    //visibility:
            //    foreach (var item in GetComponentsInChildren<Renderer>())
            //    {
            //        item.enabled = visible;
            //    }
            //}
        }
#endif
    }
}