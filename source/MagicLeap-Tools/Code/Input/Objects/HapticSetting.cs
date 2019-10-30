// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
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
        public MLInputControllerFeedbackPatternVibe pattern;
        public MLInputControllerFeedbackIntensity instensity;

        //Constructors:
        public HapticSetting(bool enabled, MLInputControllerFeedbackPatternVibe pattern, MLInputControllerFeedbackIntensity intensity)
        {
            this.enabled = enabled;
            this.pattern = pattern;
            this.instensity = intensity;
        }
#endif
    }
}
