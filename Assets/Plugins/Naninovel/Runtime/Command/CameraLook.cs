// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Activates/disables camera look mode, when player can offset the main camera with input devices 
    /// (eg, by moving a mouse or using gamepad analog stick).
    /// Check [this video](https://youtu.be/rC6C9mA7Szw) for a quick demonstration of the command.
    /// </summary>
    /// <remarks>
    /// It's also possible to control the look by rotating a mobile device (in case it has a gyroscope).
    /// This requires using Unity's new input system and manually [enabling gyroscope](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Sensors.html) device.
    /// Check out [input example project](https://github.com/Naninovel/Input) for a reference on how to setup camera look with gyroscope.
    /// </remarks>
    [CommandAlias("look")]
    public class CameraLook : Command
    {
        /// <summary>
        /// Whether to enable or disable the camera look mode. Default: true.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ParameterDefaultValue("true")]
        public BooleanParameter Enable = true;
        /// <summary>
        /// A bound box with X,Y sizes in units from the initial camera position, 
        /// describing how far the camera can be moved. Default: 5,3.
        /// </summary>
        [ParameterAlias("zone")]
        public DecimalListParameter LookZone;
        /// <summary>
        /// Camera movement speed (sensitivity) by X,Y axes. Default: 1.5,1.
        /// </summary>
        [ParameterAlias("speed")]
        public DecimalListParameter LookSpeed;
        /// <summary>
        /// Whether to automatically move camera to the initial position when the look input is not active 
        /// (eg, mouse is not moving or analog stick is in default position). Default: false.
        /// </summary>
        [ParameterDefaultValue("false")]
        public BooleanParameter Gravity = false;

        private static readonly Vector2 defaultZone = new Vector2(5, 3);
        private static readonly Vector2 defaultSpeed = new Vector2(1.5f, 1);

        public override UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var zone = ArrayUtils.ToVector2(LookZone, defaultZone);
            var speed = ArrayUtils.ToVector2(LookSpeed, defaultSpeed);
            var cameraManager = Engine.GetService<ICameraManager>();
            cameraManager.SetLookMode(Enable, zone, speed, Gravity);

            return UniTask.CompletedTask;
        }
    }
}
