// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicLeapTools
{
    [System.Serializable]
    public class PlayspaceWall
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public float width;
        public float height;
        public bool physical; //is this wall touching a real wall

        //Private Variables:
        [SerializeField][HideInInspector] private Vector3 _localCenter;
        [SerializeField][HideInInspector] private Quaternion _localRotation;

        //Public Properties:
        //relative to pcf:
        public Vector3 Center
        {
            get
            {
                return TransformUtilities.WorldPosition(Playspace.Instance.pcfAnchor.Position, Playspace.Instance.pcfAnchor.Rotation, _localCenter);
            }

            set
            {
                _localCenter = TransformUtilities.LocalPosition(Playspace.Instance.pcfAnchor.Position, Playspace.Instance.pcfAnchor.Rotation, value);
            }
        }

        //relative to pcf:
        public Quaternion Rotation
        {
            get
            {
                return TransformUtilities.ApplyRotationOffset(Playspace.Instance.pcfAnchor.Rotation, _localRotation);
            }

            set
            {
                _localRotation = TransformUtilities.GetRotationOffset(Playspace.Instance.pcfAnchor.Rotation, value);
            }
        }

        public Vector3 RightEdge
        {
            get
            {
                return Center + Right * (width * .5f);
            }
        }

        public Vector3 LeftEdge
        {
            get
            {
                return Center - Right * (width * .5f);
            }
        }

        public Vector3 Normal
        {
            get
            {
                return Rotation * Vector3.forward;
            }
        }

        public Vector3 Forward
        {
            get
            {
                return Rotation * Vector3.forward;
            }
        }

        public Vector3 Back
        {
            get
            {
                return Rotation * Vector3.back;
            }
        }

        public Vector3 Right
        {
            get
            {
                return Rotation * Vector3.right;
            }
        }

        //Constructors:
        public PlayspaceWall(Vector3 center, Quaternion rotation, float width, float height, bool physical)
        {
            Center = center;
            Rotation = rotation;
            this.width = width;
            this.height = height;
            this.physical = physical;
        }
#endif
    }
}