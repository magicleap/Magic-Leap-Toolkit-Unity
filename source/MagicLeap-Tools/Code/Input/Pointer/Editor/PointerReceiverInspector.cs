// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MagicLeapTools
{
    [CustomEditor(typeof(PointerReceiver))]
    public class PointerReceiverInspector : Editor
    {
        //GUI:
        public override void OnInspectorGUI()
        {
            //inspector flow begin:
            serializedObject.Update();

            //reorder the inspector so these aren't buried under all of the inherited events:
            EditorGUILayout.PropertyField(serializedObject.FindProperty("draggable"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("kinematicWhileIdle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("faceWhileDragging"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("matchWallWhileDragging"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("invertForward"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTargetEnter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTargetExit"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSelected"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDeselected"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnClick"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDragBegin"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDragEnd"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnUp"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnLeft"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnRight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnRadialDrag"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFire0Down"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFire0Up"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFire1Down"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFire1Up"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFire2Down"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFire2Up"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDraggedCollisionEnter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDraggedAlongSurface"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDraggedCollisionStay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDraggedCollisionExit"));

            //inspector flow end:
            serializedObject.ApplyModifiedProperties();
        }
    }
}