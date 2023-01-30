// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to manage <see cref="CustomVariable"/> objects.
    /// </summary>
    public interface ICustomVariableManager : IEngineService<CustomVariablesConfiguration>
    {
        /// <summary>
        /// Invoked when a custom variable is created or its value changed.
        /// </summary>
        event Action<CustomVariableUpdatedArgs> OnVariableUpdated;

        /// <summary>
        /// Checks whether a variable with the provided name exists.
        /// </summary>
        bool VariableExists (string name);
        /// <summary>
        /// Attempts to retrieve value of a variable with the provided name. Variable names are case-insensitive. 
        /// When no variables of the provided name are found will return null.
        /// </summary>
        string GetVariableValue (string name);
        /// <summary>
        /// Retrieves all the managed custom variables.
        /// </summary>
        IReadOnlyCollection<CustomVariable> GetAllVariables ();
        /// <summary>
        /// Sets value of a variable with the provided name. Variable names are case-insensitive. 
        /// When no variables of the provided name are found, will add a new one and assign the value.
        /// In case the name is starting with <see cref="CustomVariablesConfiguration.GlobalPrefix"/>, the variable will be added to the global scope.
        /// </summary>
        void SetVariableValue (string name, string value);
        /// <summary>
        /// Purges all the custom local state variables and restores the 
        /// pre-defined values specified in the service configuration.
        /// </summary>
        void ResetLocalVariables ();
        /// <summary>
        /// Purges all the custom global state variables and restores the
        /// pre-defined values specified in the service configuration.
        /// </summary>
        void ResetGlobalVariables ();
    }
}
