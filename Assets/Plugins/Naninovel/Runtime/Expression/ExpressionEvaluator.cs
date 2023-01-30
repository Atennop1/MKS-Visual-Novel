// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using Naninovel.NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Allows parsing and evaluating mathematical and logical expressions.
    /// </summary>
    public static class ExpressionEvaluator
    {
        public const string ManagedTextScriptCategory = "Script";
        public const string ManagedTextKeyPrefix = "t_";

        private static readonly Regex unescapedSingleQuote = new Regex(@"(?<!\\)'", RegexOptions.Compiled);
        private static readonly Regex unescapedDoubleQuote = new Regex(@"(?<!\\)""", RegexOptions.Compiled);
        private static readonly List<MethodInfo> functions = new List<MethodInfo>();

        public static void Initialize ()
        {
            functions.AddRange(typeof(Mathf).GetMethods(BindingFlags.Public | BindingFlags.Static));
            functions.AddRange(typeof(Math).GetMethods(BindingFlags.Public | BindingFlags.Static));

            var customFunctions = Engine.Types
                .Where(t => t.IsDefined(typeof(ExpressionFunctionsAttribute)))
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static)).ToList();
            functions.AddRange(customFunctions);
        }

        public static TResult Evaluate<TResult> (string expressionString, Action<string> onError = null)
        {
            var resultType = typeof(TResult);
            var result = Evaluate(expressionString, resultType, onError);
            return resultType.IsInstanceOfType(result) ? (TResult)result : default;
        }

        public static bool TryEvaluate<TResult> (string expressionString, out TResult result, Action<string> onError = null)
        {
            return (result = Evaluate<TResult>(expressionString, onError)) != null;
        }

        public static object Evaluate (string expressionString, Type resultType, Action<string> onError = null)
        {
            if (functions.Count == 0)
                Initialize();

            if (string.IsNullOrWhiteSpace(expressionString))
            {
                onError?.Invoke("Expression is missing.");
                return default;
            }

            // Escape all the un-escaped single quotes.
            expressionString = unescapedSingleQuote.Replace(expressionString, @"\'");
            // Replace un-escaped double quotes with single quotes.
            expressionString = unescapedDoubleQuote.Replace(expressionString, @"'");

            var expression = new Expression(expressionString, EvaluateOptions.IgnoreCase | EvaluateOptions.MatchStringsOrdinal);
            expression.EvaluateParameter += EvaluateExpressionParameter;
            expression.EvaluateFunction += EvaluateExpressionFunction;

            if (expression.HasErrors())
            {
                onError?.Invoke($"Expression `{expressionString}` syntax error: {expression.Error}");
                return default;
            }

            var resultObj = expression.Evaluate();
            if (resultObj is null)
            {
                onError?.Invoke($"Expression `{expressionString}` result is null.");
                return default;
            }

            return Convert.ChangeType(resultObj, resultType, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static void EvaluateExpressionParameter (string name, ParameterArgs args)
        {
            if (name.StartsWith(ManagedTextKeyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var textManager = Engine.GetService<ITextManager>();
                var managedTextValue = textManager.GetRecordValue(name, ManagedTextScriptCategory);
                if (string.IsNullOrEmpty(managedTextValue))
                    Debug.LogWarning($"Failed to find a managed text value of `{name}`. Make sure the corresponding record exists in a `{ManagedTextScriptCategory}` managed text document.");
                args.Result = managedTextValue;
                return;
            }

            var variableManager = Engine.GetService<ICustomVariableManager>();
            if (!variableManager.VariableExists(name))
                Debug.LogWarning($"Custom variable `{name}` doesn't exist, but its value is requested in a script expression; this could lead to evaluation errors. Make sure to initialize variables with `@set` command or via `Custom Variables` configuration menu before using them.");
            var strValue = variableManager.GetVariableValue(name) ?? string.Empty;
            args.Result = CustomVariablesConfiguration.ParseVariableValue(strValue);
        }

        private static void EvaluateExpressionFunction (string name, FunctionArgs args)
        {
            foreach (var methodInfo in functions)
            {
                // Check name equality.
                if (!methodInfo.Name.EqualsFastIgnoreCase(name)) continue;

                var functionParams = args.Parameters.Select(p => p.Evaluate()).ToArray();
                var methodParams = methodInfo.GetParameters();

                // Handle functions with single `params` argument.
                if (methodParams.Length == 1 && methodParams[0].IsDefined(typeof(ParamArrayAttribute)) &&
                    functionParams.All(p => p.GetType() == methodParams[0].ParameterType.GetElementType()))
                {
                    var elementType = methodParams[0].ParameterType.GetElementType();
                    var paramsValue = Array.CreateInstance(elementType, functionParams.Length);
                    Array.Copy(functionParams, paramsValue, functionParams.Length);
                    args.Result = methodInfo.Invoke(null, new object[] { paramsValue });
                    break;
                }

                // Check argument count equality.
                if (methodParams.Length != functionParams.Length) continue;

                // Check argument type and order equality.
                var paramTypeCheckPassed = true;
                for (int i = 0; i < methodParams.Length; i++)
                    if (methodParams[i].ParameterType != functionParams[i].GetType())
                    {
                        paramTypeCheckPassed = false;
                        break;
                    }
                if (!paramTypeCheckPassed) continue;

                args.Result = methodInfo.Invoke(null, functionParams);
                break;
            }
        }
    }
}
