// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Globalization;

namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="ICustomVariableManager"/>.
    /// </summary>
    public static class CustomVariableManagerExtensions
    {
        /// <summary>
        /// Attempts to retrieve value of a variable with the provided name and type. Variable names are case-insensitive. 
        /// When no variables of the provided name are found or when the string value can't be parsed to the requested type, will return false.
        /// </summary>
        public static bool TryGetVariableValue<TValue> (this ICustomVariableManager manager, string name, out TValue value)
        {
            value = default;
            var stringValue = manager.GetVariableValue(name);
            if (stringValue is null) return false;

            var objValue = CustomVariablesConfiguration.ParseVariableValue(stringValue);
            if (objValue is TValue)
            {
                value = (TValue)objValue;
                return true;
            }
            if (typeof(TValue) == typeof(float) && objValue is int intValue)
            {
                value = (TValue)(object)(float)intValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to set value of a variable with the provided name and type. Variable names are case-insensitive. 
        /// When no variables of the provided name are found, will add a new one and assign the value.
        /// In case the name is starting with <see cref="CustomVariablesConfiguration.GlobalPrefix"/>, the variable will be added to the global scope.
        /// When value type can't be represented as a string, won't have any effect and return false.
        /// </summary>
        public static bool TrySetVariableValue<TValue> (this ICustomVariableManager manager, string name, TValue value)
        {
            string stringValue;
            switch (value)
            {
                case float @float:
                    stringValue = @float.ToString(CultureInfo.InvariantCulture);
                    if (!stringValue.Contains(".")) 
                        stringValue += ".0";  // Required by CustomVariablesConfiguration.ParseVariableValue().
                    break;
                case int @int:
                    stringValue = @int.ToString(CultureInfo.InvariantCulture);
                    break;
                case bool @bool:
                    stringValue = @bool.ToString(CultureInfo.InvariantCulture);
                    break;
                case string @string:
                    stringValue = @string;
                    break;
                default: return false;
            }
            manager.SetVariableValue(name, stringValue);
            return true;
        }
    }
}
