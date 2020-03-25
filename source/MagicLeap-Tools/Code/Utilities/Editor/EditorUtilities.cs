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
using System;
using System.Linq;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeapTools
{
    public class EditorUtilities
    {
        //Public Methods:
        public static void ComponentRequired(Type componentType)
        {
            if (GameObject.FindObjectOfType(componentType) == null)
            {
                InspectorInfoBox($"This component requires an instance of {componentType.Name} in your scene.", MessageType.Error);
            }
        }

#if PLATFORM_LUMIN
        public static void SensitivePrivilegeRequired(MLPrivileges.RuntimeRequestId privilege)
        {
            MLPrivilegeRequesterBehavior privilegeRequester = GameObject.FindObjectOfType<MLPrivilegeRequesterBehavior>();
            if (privilegeRequester != null)
            {
                if (privilegeRequester.Privileges == null)
                {
                    InspectorInfoBox($"This component requires a sensitive privilige.  Include {privilege} in your instance of PrivilegeRequester and make sure the equivalent privilege has been added to your manifest.", MessageType.Error);
                }
                else if (Array.IndexOf<MLPrivileges.RuntimeRequestId>(privilegeRequester.Privileges, privilege) == -1)
                {
                    InspectorInfoBox($"This component requires a sensitive privilige.  Include {privilege} in your instance of PrivilegeRequester and make sure the equivalent privilege has been added to your manifest.", MessageType.Error);
                }
            }
        }
#endif

        //Private Methods:
        private static void InspectorInfoBox(string text, MessageType messageType)
        {
            Color currentColor = GUI.color;
            EditorStyles.label.wordWrap = true;
            EditorGUILayout.HelpBox(text, messageType);
            GUILayout.Space(10);
        }
    }
}