// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System;

namespace MagicLeapTools
{
    public class InteractionState
    {
        //Events:
        public event Action OnBegin;
        public event Action OnEnd;
        public event Action OnUpdate;

        //Public Properties:
        public bool Active
        {
            get;
            private set;
        }

        //Private Variables:
        private InteractionPoint _interactionPoint;

        //Constructors:
        public InteractionState(InteractionPoint interactionPoint)
        {
            _interactionPoint = interactionPoint;
        }

        //Public Methods:
        public void FireBegin()
        {
            Active = true;
            OnBegin?.Invoke();
        }

        public void FireEnd()
        {
            Active = false;
            OnEnd?.Invoke();
        }

        public void FireUpdate()
        {
            OnUpdate?.Invoke();
        }
    }
}