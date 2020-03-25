// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace MagicLeapTools
{
    [CustomEditor(typeof(PlaceOnFloor))]
    public class PlaceOnFloorEditor : Editor
    {
#if PLATFORM_LUMIN
        //Private Variables:
        private PlaceOnFloor _target;

        //Flow:
        private void OnEnable()
        {
            _target = target as PlaceOnFloor;
        }

        //Inspector GUI:
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (_target.content == _target.gameObject)
            {
                EditorStyles.label.wordWrap = true;
                GUI.color = Color.red;
                EditorGUILayout.HelpBox("PlaceOnFloor can not be used on it's own content.  Move the PlaceOnFloor to a separate object.", MessageType.Warning);
            }
        }
#endif
    }
}