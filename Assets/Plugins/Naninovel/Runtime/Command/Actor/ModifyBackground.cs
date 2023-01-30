// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Modifies a [background actor](/guide/backgrounds.md).
    /// </summary>
    /// <remarks>
    /// Backgrounds are handled a bit differently from characters to better accommodate traditional VN game flow. 
    /// Most of the time you'll probably have a single background actor on scene, which will constantly transition to different appearances.
    /// To remove the hassle of repeating same actor ID in scripts, it's possible to provide only 
    /// the background appearance and transition type (optional) as a nameless parameter assuming `MainBackground` 
    /// actor should be affected. When this is not the case, ID of the background actor can be explicitly provided via the `id` parameter.
    /// </remarks>
    [CommandAlias("back")]
    public class ModifyBackground : ModifyOrthoActor<IBackgroundActor, BackgroundState, BackgroundMetadata, BackgroundsConfiguration, IBackgroundManager>
    {
        /// <summary>
        /// Appearance (or [pose](/guide/backgrounds.md#poses)) to set for the modified background and type of a [transition effect](/guide/transition-effects.md) to use.
        /// When transition is not provided, a cross-fade effect will be used by default.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), AppearanceContext(0), ConstantContext(typeof(TransitionType), 1)]
        public NamedStringParameter AppearanceAndTransition;

        protected override bool AllowPreload => Assigned(AppearanceAndTransition) && !AppearanceAndTransition.DynamicValue;
        protected override string AssignedId => base.AssignedId ?? BackgroundsConfiguration.MainActorId;
        protected override string AlternativeAppearance => AppearanceAndTransition?.Name;
        protected override string AssignedTransition => base.AssignedTransition ?? AppearanceAndTransition?.NamedValue;
    }
}
