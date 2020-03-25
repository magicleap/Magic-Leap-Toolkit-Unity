// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace MagicLeapTools
{
    [CustomEditor(typeof(VirtualButton), true)]
    public class VirtualButtonEditor : Editor
    {
        //Private Variables:
        private VirtualButton _virtualButton;
        private Color _originColor = new Color(1, 0, 0, .2f);
        private Color _hoverColor = new Color(0, 1, 0, .2f);
        private Color _touchColor = new Color(1, 0.92f, 0.016f, .2f);

        //Init:
        private void OnEnable()
        {
            //ref:
            _virtualButton = (VirtualButton)target;
        }

        //handles:
        protected virtual void OnSceneGUI()
        {
            //locations:
            Vector3 touch = _virtualButton.InputNormal * _virtualButton.ScaledTouchDistance + _virtualButton.transform.position;
            Vector3 hover = _virtualButton.InputNormal * _virtualButton.hoverDistance + _virtualButton.transform.position;

            //visuals:
            Handles.DrawLine(_virtualButton.transform.position, hover);
            Handles.color = _originColor;
            Handles.DrawSolidDisc(_virtualButton.transform.position, _virtualButton.InputNormal, _virtualButton.ScaledRadius * .5f);
            Handles.color = _touchColor;
            Handles.DrawSolidDisc(touch, _virtualButton.InputNormal, _virtualButton.ScaledRadius * .5f);
            Handles.color = _hoverColor;
            Handles.DrawSolidDisc(hover, _virtualButton.InputNormal, _virtualButton.ScaledRadius * .5f);
        }
    }
}