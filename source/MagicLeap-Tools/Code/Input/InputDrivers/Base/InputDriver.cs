// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using System;

namespace MagicLeapTools
{
    /// <summary>
    /// Provides a unified funnel with which to inject input events and gestures.
    /// </summary>
    public abstract class InputDriver : MonoBehaviour
    {
        //Events:
        public event Action<InputDriver> OnRight;
        public event Action<InputDriver> OnLeft;
        public event Action<InputDriver> OnUp;
        public event Action<InputDriver> OnDown;
        public event Action<InputDriver> OnFire0Down;
        public event Action<InputDriver> OnFire0Up;
        public event Action<InputDriver> OnFire1Down;
        public event Action<InputDriver> OnFire1Up;
        public event Action<InputDriver> OnFire2Down;
        public event Action<InputDriver> OnFire2Up;
        public event Action<float, InputDriver> OnRadialDrag;
        public event Action<InputDriver> OnActivate;
        public event Action<InputDriver> OnDeactivate;

        //Public Variables:
        public Transform motionSource;

        //Private Variables:
        private bool _active;
        private float _selectValue;

        //Public Properties:
        public bool Active
        {
            get
            {
                return _active;
            }

            protected set
            {
                if (value != _active)
                {
                    _active = value;

                    if (_active)
                    {
                        OnActivate?.Invoke(this);
                    }
                    else
                    {
                        OnDeactivate?.Invoke(this);
                    }
                }
            }
        }

        //Init:
        private void Reset()
        {
            motionSource = transform;
        }

        //Protected Methods:
        protected void Up()
        {
            OnUp?.Invoke(this);
        }

        protected void Down()
        {
            OnDown?.Invoke(this);
        }
        protected void Left()
        {
            OnLeft?.Invoke(this);
        }

        protected void Right()
        {
            OnRight?.Invoke(this);
        }

        protected void Fire0Down()
        {
            OnFire0Down?.Invoke(this);
        }

        protected void Fire0Up()
        {
            OnFire0Up?.Invoke(this);
        }

        protected void Fire1Down()
        {
            OnFire1Down?.Invoke(this);
        }

        protected void Fire1Up()
        {
            OnFire1Up?.Invoke(this);
        }

        protected void Fire2Down()
        {
            OnFire2Down?.Invoke(this);
        }

        protected void Fire2Up()
        {
            OnFire2Up?.Invoke(this);
        }

        protected void RadialDrag(float delta)
        {
            OnRadialDrag?.Invoke(delta, this);
        }

        protected void Activate()
        {
            Active = true;
            OnActivate?.Invoke(this);
        }

        protected void Deactivate()
        {
            Active = false;
            OnDeactivate?.Invoke(this);
        }
    }
}