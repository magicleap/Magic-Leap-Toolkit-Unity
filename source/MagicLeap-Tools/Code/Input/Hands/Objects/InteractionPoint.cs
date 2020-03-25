// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace MagicLeapTools
{
    public class InteractionPoint
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public Vector3 position;
        public Quaternion rotation;
        public float radius;
        public bool active;
        public ManagedHand managedHand;

        //Public Properties:
        public InteractionState Touch
        {
            get;
            private set;
        }

        public InteractionState Hover
        {
            get;
            private set;
        }

        public List<IDirectManipulation> DirectManipulations
        {
            get;
            private set;
        }

        //Private Variables:
        private float _minimumColliderRadius = 0.0381f;
        private float _maximumColliderRadius = 0.1016f;

        //Constructors:
        public InteractionPoint(ManagedHand managedHand)
        {
            //sets:
            this.managedHand = managedHand;
            Touch = new InteractionState(this);
            Hover = new InteractionState(this);
            rotation = Quaternion.identity;

            //hooks:
            Touch.OnBegin += HandleTouchBegan;
            Touch.OnUpdate += HandleTouchUpdate;
            Touch.OnEnd += HandleTouchEnd;
        }

        //Event Handlers:
        private void HandleTouchBegan()
        {
            //collect direct manipulations:
            Collider[] colliders = Physics.OverlapSphere(position, Mathf.Clamp(radius, _minimumColliderRadius, _maximumColliderRadius));
            DirectManipulations = new List<IDirectManipulation>();
            foreach (var item in colliders)
            {
                IDirectManipulation dm = (IDirectManipulation)item.GetComponent(typeof(IDirectManipulation));
                if (dm != null)
                {
                    DirectManipulations.Add(dm);
                }
            }

            //fire event:
            foreach (var item in DirectManipulations)
            {
                item.GrabBegan(this);
            }
        }

        private void HandleTouchUpdate()
        {
            //fire event:
            foreach (var item in DirectManipulations)
            {
                item.GrabUpdate(this);
            }
        }

        private void HandleTouchEnd()
        {
            //fire event:
            foreach (var item in DirectManipulations)
            {
                item.GrabEnd(this);
            }
        }
#endif
    }
}