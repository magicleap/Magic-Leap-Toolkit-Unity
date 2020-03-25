// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace MagicLeapTools
{
    [RequireComponent(typeof(Text))]
    public class SetText : MonoBehaviour
    {
        //Private Variables:
        private Text _label;

        //Init:
        private void Awake()
        {
            _label = GetComponent<Text>();
        }

        //Public Methods:
        public void Set(float a)
        {
            _label.text = a.ToString();
        }
    }
}