// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEditor;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeapTools
{
    [CustomEditor(typeof(Transmission))]
    public class TransmissionEditor : Editor
    {
#if PLATFORM_LUMIN
        //Private Variables:
        private Transmission _target;

        //Flow:
        private void OnEnable()
        {
            _target = target as Transmission;
        }

        //Inspector GUI:
        public override void OnInspectorGUI()
        {
            EditorUtilities.ComponentRequired(typeof(MLPrivilegeRequesterBehavior));
            EditorUtilities.SensitivePrivilegeRequired(MLPrivileges.RuntimeRequestId.LocalAreaNetwork);
            DrawDefaultInspector();
        }
#endif
        }
    }