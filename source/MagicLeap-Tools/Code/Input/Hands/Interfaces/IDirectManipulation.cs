// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

namespace MagicLeapTools
{
    public interface IDirectManipulation 
    {
#if PLATFORM_LUMIN
        void GrabBegan(InteractionPoint interactionPoint);
        void GrabUpdate(InteractionPoint interactionPoint);
        void GrabEnd(InteractionPoint interactionPoint);
#endif
    }
}