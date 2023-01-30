// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Naninovel
{
    [EditInProjectSettings]
    public class InputConfiguration : Configuration
    {
        public const string SubmitName = "Submit";
        public const string CancelName = "Cancel";
        public const string DeleteName = "Delete";
        public const string ContinueName = "Continue";
        public const string PauseName = "Pause";
        public const string SkipName = "Skip";
        public const string ToggleSkipName = "ToggleSkip";
        public const string AutoPlayName = "AutoPlay";
        public const string ToggleUIName = "ToggleUI";
        public const string ShowBacklogName = "ShowBacklog";
        public const string RollbackName = "Rollback";
        public const string CameraLookXName = "CameraLookX";
        public const string CameraLookYName = "CameraLookY";

        [Tooltip("Whether to spawn an event system when initializing.")]
        public bool SpawnEventSystem = true;
        [Tooltip("A prefab with an `EventSystem` component to spawn for input processing. Will spawn a default one when not specified.")]
        public EventSystem CustomEventSystem = null;
        [Tooltip("Whether to spawn an input module when initializing.")]
        public bool SpawnInputModule = true;
        [Tooltip("A prefab with an `InputModule` component to spawn for input processing. Will spawn a default one when not specified.")]
        public BaseInputModule CustomInputModule = null;
        [Tooltip("Limits frequency of the registered touch inputs, in seconds.")]
        public float TouchFrequencyLimit = .1f;
        [Tooltip("Limits distance of the registered touch inputs, in pixels.")]
        public float TouchDistanceLimit = 25f;
        #if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_AVAILABLE
        [Tooltip("When Unity's new input system is installed, assign input actions asset here.\n\nTo map input actions to Naninovel's input bindings, create `Naninovel` action map and add actions with names equal to the binding names (found below under `Control Scheme` -> Bindings list).\n\nBe aware, that 2-dimensional (Vector2) axes are not supported.")]
        public UnityEngine.InputSystem.InputActionAsset InputActions = default;
        #endif
        [Tooltip("Whether to process legacy input bindings. Disable in case you're using Unity's new input system and don't want the legacy bindings to work in addition to input actions.")]
        public bool ProcessLegacyBindings = true;

        [Header("Control Scheme"), Tooltip("Bindings to process input for.")]
        public List<InputBinding> Bindings = new List<InputBinding> {
            new InputBinding {
                Name = SubmitName,
                Keys = new List<KeyCode> { KeyCode.Return, KeyCode.JoystickButton0 }
            },
            new InputBinding {
                Name = CancelName,
                Keys = new List<KeyCode> { KeyCode.Escape, KeyCode.JoystickButton1 }, AlwaysProcess = true
            },
            new InputBinding {
                Name = DeleteName,
                Keys = new List<KeyCode> { KeyCode.Delete, KeyCode.JoystickButton7 }, AlwaysProcess = true
            },
            new InputBinding {
                Name = ContinueName,
                Keys = new List<KeyCode> { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.JoystickButton0 },
                Axes = new List<InputAxisTrigger> { new InputAxisTrigger { AxisName = "Mouse ScrollWheel", TriggerMode = InputAxisTriggerMode.Negative } },
                Swipes = new List<InputSwipeTrigger> { new InputSwipeTrigger { Direction = InputSwipeDirection.Left } }
            },
            new InputBinding {
                Name = PauseName,
                Keys = new List<KeyCode> { KeyCode.Backspace, KeyCode.JoystickButton7 }
            },
            new InputBinding {
                Name = SkipName,
                Keys = new List<KeyCode> { KeyCode.LeftControl, KeyCode.RightControl, KeyCode.JoystickButton1 }
            },
            new InputBinding {
                Name = ToggleSkipName,
                Keys = new List<KeyCode> { KeyCode.Tab, KeyCode.JoystickButton9 }
            },
            new InputBinding {
                Name = AutoPlayName,
                Keys = new List<KeyCode> { KeyCode.A, KeyCode.JoystickButton2 }
            },
            new InputBinding {
                Name = ToggleUIName,
                Keys = new List<KeyCode> { KeyCode.Space, KeyCode.JoystickButton3 },
                Swipes = new List<InputSwipeTrigger> { new InputSwipeTrigger { Direction = InputSwipeDirection.Down } }
            },
            new InputBinding {
                Name = ShowBacklogName,
                Keys = new List<KeyCode> { KeyCode.L, KeyCode.JoystickButton5 },
                Swipes = new List<InputSwipeTrigger> { new InputSwipeTrigger { Direction = InputSwipeDirection.Up } }
            },
            new InputBinding {
                Name = RollbackName,
                Keys = new List<KeyCode> { KeyCode.JoystickButton4 },
                Axes = new List<InputAxisTrigger> { new InputAxisTrigger { AxisName = "Mouse ScrollWheel", TriggerMode = InputAxisTriggerMode.Positive } },
                Swipes = new List<InputSwipeTrigger> { new InputSwipeTrigger { Direction = InputSwipeDirection.Right } },
            },
            new InputBinding {
                Name = CameraLookXName,
                Axes = new List<InputAxisTrigger> {
                    new InputAxisTrigger { AxisName = "Horizontal", TriggerMode = InputAxisTriggerMode.Both },
                    new InputAxisTrigger { AxisName = "Mouse X", TriggerMode = InputAxisTriggerMode.Both }
                }
            },
            new InputBinding {
                Name = CameraLookYName,
                Axes = new List<InputAxisTrigger> {
                    new InputAxisTrigger { AxisName = "Vertical", TriggerMode = InputAxisTriggerMode.Both },
                    new InputAxisTrigger { AxisName = "Mouse Y", TriggerMode = InputAxisTriggerMode.Both }
                }
            }
        };
    }
}
