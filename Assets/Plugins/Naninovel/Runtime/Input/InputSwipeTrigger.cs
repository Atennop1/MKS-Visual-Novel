// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Allows detecting touch swipes by sampling user input.
    /// </summary>
    [Serializable]
    public class InputSwipeTrigger
    {
        [Tooltip("Swipe of which direction should be registered.")]
        public InputSwipeDirection Direction = default;
        [Tooltip("How much fingers (touches) should be active to register the swipe."), Range(1, 5)]
        public int FingerCount = 1;
        [Tooltip("Minimum required swipe distance to activate the trigger, in pixels.")]
        public float MinimumDistance = 50f;
        [Tooltip("Whether to activate the input while moving fingers. When disabled, will only active when fingers are released.")]
        public bool ActivateOnMove = false;
        
        private Vector2 startPosition;

        /// <summary>
        /// Returns whether the swipe is currently registered.
        /// </summary>
        public bool Sample ()
        {
            #if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.touchCount != FingerCount) return false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (CheckTouch(touch)) return true;
            }
            #endif
            return false;
        }

        #if ENABLE_LEGACY_INPUT_MANAGER
        private bool CheckTouch (Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began: startPosition = touch.position; return false;
                case TouchPhase.Moved: return ActivateOnMove && CheckSwipe(touch.position);
                case TouchPhase.Ended: return CheckSwipe(touch.position);
                default: return false;
            }
        }

        private bool CheckSwipe (Vector2 endPosition)
        {
            var horDist = Mathf.Abs(endPosition.x - startPosition.x);
            var verDist = Mathf.Abs(endPosition.y - startPosition.y);

            switch (Direction)
            {
                case InputSwipeDirection.Up: return MovedUp();
                case InputSwipeDirection.Down: return MovedDown();
                case InputSwipeDirection.Left: return MovedLeft();
                case InputSwipeDirection.Right: return MovedRight();
                default: return false;
            }

            bool MovedHorizontally () => horDist > MinimumDistance && horDist > verDist;
            bool MovedVertically () => verDist > MinimumDistance && verDist > horDist;
            bool MovedRight () => MovedHorizontally() && endPosition.x - startPosition.x > 0;
            bool MovedLeft () => MovedHorizontally() && endPosition.x - startPosition.x < 0;
            bool MovedUp () => MovedVertically() && endPosition.y - startPosition.y > 0;
            bool MovedDown () => MovedVertically() && endPosition.y - startPosition.y < 0;
        }
        #endif
    }
}
