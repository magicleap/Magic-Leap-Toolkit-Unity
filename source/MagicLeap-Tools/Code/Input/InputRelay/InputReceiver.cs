// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MagicLeapTools
{
    /// <summary>
    /// Consumes input from a targeting InputSender or Pointer.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InputReceiver : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Events:
        public GameObjectEvent OnTargetEnter;
        public GameObjectEvent OnTargetExit;
        public GameObjectEvent OnSelected;
        public GameObjectEvent OnDeselected;
        public GameObjectEvent OnClick;
        public GameObjectEvent OnDragBegin;
        public GameObjectEvent OnDragEnd;
        public GameObjectEvent OnUp;
        public GameObjectEvent OnDown;
        public GameObjectEvent OnLeft;
        public GameObjectEvent OnRight;
        public FloatGameObjectEvent OnRadialDrag;
        public GameObjectEvent OnFire0Down;
        public GameObjectEvent OnFire0Up;
        public GameObjectEvent OnFire1Down;
        public GameObjectEvent OnFire1Up;
        public GameObjectEvent OnFire2Down;
        public GameObjectEvent OnFire2Up;

        //Public Properties:
        public List<GameObject> TargetedBy
        {
            get;
            private set;
        }

        public List<GameObject> SelectedBy
        {
            get;
            private set;
        }

        public List<GameObject> DraggedBy
        {
            get;
            private set;
        }

        //Private Variables:
        protected Transform _mainCamera;
        protected Collider _collider;
        private Button _button;

        //Init:
        private void Reset()
        {
            //steps:
            ScaleGuiCollider();
        }

        private void Awake()
        {
            //refs:
            _button = GetComponent<Button>();
            _mainCamera = Camera.main.transform;
            _collider = GetComponent<Collider>();

            AwakeInherited();
        }

        //Flow:
        private void OnEnable()
        {
            //sets:
            TargetedBy = new List<GameObject>();
            SelectedBy = new List<GameObject>();
            DraggedBy = new List<GameObject>();

            OnEnableInherited();
        }

        //Virtual Methods:
        protected virtual void AwakeInherited()
        {
        }

        protected virtual void OnEnableInherited()
        {
        }

        protected virtual void ResetInherited()
        {
        }

        public virtual void UpReceived(GameObject sender)
        {
            OnUp?.Invoke(sender);
        }

        public virtual void DownReceived(GameObject sender)
        {
            OnDown?.Invoke(sender);
        }

        public virtual void LeftReceived(GameObject sender)
        {
            OnLeft?.Invoke(sender);
        }

        public virtual void RightReceived(GameObject sender)
        {
            OnRight?.Invoke(sender);
        }

        public virtual void RadialDragReceived(float delta, GameObject sender)
        {
            OnRadialDrag?.Invoke(delta, sender);
        }

        public virtual void Fire0DownReceived(GameObject sender)
        {
            if (SelectedBy.Contains(sender))
            {
                return;
            }

            SelectedBy.Add(sender);
            OnSelected?.Invoke(sender);
            OnFire0Down?.Invoke(sender);

            //do we have a button - click it!
            _button?.onClick.Invoke();
        }

        public virtual void Fire0UpReceived(GameObject sender)
        {
            if (!SelectedBy.Contains(sender))
            {
                return;
            }

            SelectedBy.Remove(sender);
            OnDeselected?.Invoke(sender);
            OnFire0Up?.Invoke(sender);
        }

        public virtual void Fire1DownReceived(GameObject sender)
        {
            if (!TargetedBy.Contains(sender))
            {
                return;
            }

            OnFire1Down?.Invoke(sender);
        }

        public virtual void Fire1UpReceived(GameObject sender)
        {
            if (!TargetedBy.Contains(sender))
            {
                return;
            }

            OnFire1Up?.Invoke(sender);
        }

        public virtual void Fire2DownReceived(GameObject sender)
        {
            if (!TargetedBy.Contains(sender))
            {
                return;
            }

            OnFire2Down?.Invoke(sender);
        }

        public virtual void Fire2UpReceived(GameObject sender)
        {
            if (!TargetedBy.Contains(sender))
            {
                return;
            }

            OnFire2Up?.Invoke(sender);
        }

        public virtual void TargetEnter(GameObject sender)
        {
            if (TargetedBy.Contains(sender))
            {
                return;
            }

            TargetedBy.Add(sender);
            OnTargetEnter?.Invoke(sender);
        }

        public virtual void TargetExit(GameObject sender)
        {
            if (!TargetedBy.Contains(sender))
            {
                return;
            }

            TargetedBy.Remove(sender);
            OnTargetExit?.Invoke(sender);
        }

        public virtual void DragBegin(GameObject sender)
        {
            if (!TargetedBy.Contains(sender))
            {
                return;
            }

            if (DraggedBy.Contains(sender))
            {
                return;
            }

            DraggedBy.Add(sender);
            OnDragBegin?.Invoke(sender);
        }

        public virtual void DragEnd(GameObject sender)
        {
            if (!DraggedBy.Contains(sender))
            {
                return;
            }
            
            DraggedBy.Remove(sender);
            OnDragEnd?.Invoke(sender);
        }

        public virtual void Clicked(GameObject sender)
        {
            if (!TargetedBy.Contains(sender))
            {
                return;
            }

            OnClick?.Invoke(sender);
        }

        private void ScaleGuiCollider()
        {
            //scale a box collider on a gui element (if we are on one) just to be helpful:
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                BoxCollider collider = GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, collider.size.z);
                }
            }
        }
#endif
    }
}