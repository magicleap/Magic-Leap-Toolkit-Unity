// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeapTools
{
    [System.Serializable]
    public class HapticSetting
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public bool enabled;
        public MLInput.Controller.FeedbackPatternVibe pattern;
        public MLInput.Controller.FeedbackIntensity intensity;

        //Constructors:
        public HapticSetting(bool enabled, MLInput.Controller.FeedbackPatternVibe pattern, MLInput.Controller.FeedbackIntensity intensity)
        {
            this.enabled = enabled;
            this.pattern = pattern;
            this.intensity = intensity;
        }
#endif
    }
}
