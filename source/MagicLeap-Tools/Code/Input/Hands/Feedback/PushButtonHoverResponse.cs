// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicLeapTools
{
    public class PushButtonHoverResponse : PercentageResponse
    {
        //Public Variables:
        [Tooltip("Scale when zero is processed.")]
        public Vector3 zeroScale;
        [Tooltip("Color when zero is processed.")]
        public Color zeroColor = new Color(1, 1, 1, 0);
        [Tooltip("Scale when one is processed.")]
        public Vector3 oneScale;
        [Tooltip("Color when one is processed.")]
        public Color oneColor = Color.white;

        //Private Variables:
        private Renderer _renderer;

        //Init:
        private void Reset()
        {
            zeroScale = transform.localScale * 2;
            oneScale = transform.localScale;
        }

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            Process(0);
        }

        //Public Methods:
        public override void Process(float percentage)
        {
            percentage = Mathf.Clamp01(percentage);
            _renderer.material.color = Color.Lerp(zeroColor, oneColor, percentage);
            transform.localScale = Vector3.Lerp(zeroScale, oneScale, percentage);
        }
    }
}