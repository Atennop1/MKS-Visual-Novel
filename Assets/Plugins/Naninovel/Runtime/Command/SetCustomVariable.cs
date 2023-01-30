// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel.Commands
{
    /// <summary>
    /// Assigns result of a [script expression](/guide/script-expressions.md) to a [custom variable](/guide/custom-variables.md).
    /// </summary>
    /// <remarks>
    /// If a variable with the provided name doesn't exist, it will be automatically created.
    /// <br/><br/>
    /// It's possible to define multiple set expressions in one line by separating them with `;`. The expressions will be executed in sequence by the order of declaration.
    /// <br/><br/>
    /// In case variable name starts with `T_` or `t_` it's considered a reference to a value stored in 'Script' [managed text](/guide/managed-text.md) document. 
    /// Such variables can't be assigned and mostly used for referencing localizable text values.
    /// </remarks>
    [CommandAlias("set")]
    public class SetCustomVariable : Command, Command.IForceWait
    {
        /// <summary>
        /// Set expression. 
        /// <br/><br/>
        /// The expression should be in the following format: `VariableName=ExpressionBody`, where `VariableName` is the name of the custom 
        /// variable to assign and `ExpressionBody` is a [script expression](/guide/script-expressions.md), the result of which should be assigned to the variable.
        /// <br/><br/>
        /// It's also possible to use increment and decrement unary operators (`@set foo++`, `@set foo--`) and compound assignment (`@set foo+=10`, `@set foo-=3`, `@set foo*=0.1`, `@set foo/=2`).
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter, ExpressionContext]
        public StringParameter Expression;

        private static readonly char[] splitChars = { ';' };
        private const string assignmentLiteral = "=";
        private const string incrementLiteral = "++";
        private const string decrementLiteral = "--";
        private const string addLiteral = "+";
        private const string subtractLiteral = "-";
        private const string multiplyLiteral = "*";
        private const string divideLiteral = "/";

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var modifiedGlobalVariable = false;
            var expressions = Expression.Value.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < expressions.Length; i++)
                ProcessExpression(expressions[i], ref modifiedGlobalVariable);
            if (modifiedGlobalVariable)
                await Engine.GetService<IStateManager>().SaveGlobalAsync();
        }

        protected virtual void ProcessExpression (string expression, ref bool modifiedGlobalVariable)
        {
            ProcessUnaryOperators(ref expression);
            if (!TryExtractNameAndBody(expression, out var variableName, out var expressionBody)) return;
            ProcessCompoundAssignment(ref variableName, ref expressionBody);
            if (!ExpressionEvaluator.TryEvaluate<string>(expressionBody, out var result, LogErrorMessage)) return;
            Engine.GetService<ICustomVariableManager>().SetVariableValue(variableName, result);
            modifiedGlobalVariable = modifiedGlobalVariable || CustomVariablesConfiguration.IsGlobalVariable(variableName);
        }

        protected virtual void ProcessUnaryOperators (ref string expression)
        {
            if (expression.EndsWithFast(incrementLiteral))
                expression = expression.Replace(incrementLiteral, $"={expression.GetBefore(incrementLiteral)}+1");
            else if (expression.EndsWithFast(decrementLiteral))
                expression = expression.Replace(decrementLiteral, $"={expression.GetBefore(decrementLiteral)}-1");
        }

        protected virtual bool TryExtractNameAndBody (string expression, out string variableName, out string expressionBody)
        {
            variableName = expression.GetBefore(assignmentLiteral)?.TrimFull();
            expressionBody = expression.GetAfterFirst(assignmentLiteral)?.TrimFull();
            if (!string.IsNullOrWhiteSpace(variableName) && !string.IsNullOrWhiteSpace(expressionBody)) return true;
            LogErrorMessage("Failed to extract variable name and/or expression body.");
            return false;
        }

        protected virtual void ProcessCompoundAssignment (ref string variableName, ref string expressionBody)
        {
            if (!variableName.EndsWithFast(addLiteral) && !variableName.EndsWithFast(subtractLiteral) &&
                !variableName.EndsWithFast(multiplyLiteral) && !variableName.EndsWithFast(divideLiteral)) return;
            expressionBody = variableName + expressionBody;
            variableName = variableName.Substring(0, variableName.Length - 1);
        }

        protected virtual void LogErrorMessage (string desc = null)
        {
            LogErrorWithPosition($"Failed to evaluate set expression `{Expression}`. {desc ?? string.Empty}");
        }
    }
}
