// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="IInputManager"/>.
    /// </summary>
    public static class InputManagerExtensions
    {
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.SubmitName"/>. 
        /// </summary>
        public static IInputSampler GetSubmit (this IInputManager m) => m.GetSampler(InputConfiguration.SubmitName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.CancelName"/>. 
        /// </summary>
        public static IInputSampler GetCancel (this IInputManager m) => m.GetSampler(InputConfiguration.CancelName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.DeleteName"/>. 
        /// </summary>
        public static IInputSampler GetDelete (this IInputManager m) => m.GetSampler(InputConfiguration.DeleteName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.ContinueName"/>. 
        /// </summary>
        public static IInputSampler GetContinue (this IInputManager m) => m.GetSampler(InputConfiguration.ContinueName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.SkipName"/>. 
        /// </summary>
        public static IInputSampler GetSkip (this IInputManager m) => m.GetSampler(InputConfiguration.SkipName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.ToggleSkipName"/>. 
        /// </summary>
        public static IInputSampler GetToggleSkip (this IInputManager m) => m.GetSampler(InputConfiguration.ToggleSkipName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.AutoPlayName"/>. 
        /// </summary>
        public static IInputSampler GetAutoPlay (this IInputManager m) => m.GetSampler(InputConfiguration.AutoPlayName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.ToggleUIName"/>. 
        /// </summary>
        public static IInputSampler GetToggleUI (this IInputManager m) => m.GetSampler(InputConfiguration.ToggleUIName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.ShowBacklogName"/>. 
        /// </summary>
        public static IInputSampler GetShowBacklog (this IInputManager m) => m.GetSampler(InputConfiguration.ShowBacklogName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.RollbackName"/>. 
        /// </summary>
        public static IInputSampler GetRollback (this IInputManager m) => m.GetSampler(InputConfiguration.RollbackName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.CameraLookXName"/>. 
        /// </summary>
        public static IInputSampler GetCameraLookX (this IInputManager m) => m.GetSampler(InputConfiguration.CameraLookXName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.CameraLookYName"/>. 
        /// </summary>
        public static IInputSampler GetCameraLookY (this IInputManager m) => m.GetSampler(InputConfiguration.CameraLookYName);
        /// <summary>
        /// Attempts to <see cref="IInputManager.GetSampler(string)"/> of <see cref="InputConfiguration.PauseName"/>. 
        /// </summary>
        public static IInputSampler GetPause (this IInputManager m) => m.GetSampler(InputConfiguration.PauseName);
    }
}
