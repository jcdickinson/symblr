using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb70
{
    /// <summary>
    /// Represents a way to parse SRCSRV streams.
    /// </summary>
    internal static class SrcSrvParser
    {
        /// <summary>
        /// Represents a SRCSRV expression.
        /// </summary>
        private class SrcSrvExpression
        {
            public readonly string Value;
            public readonly bool IsDynamic;
            public readonly List<SrcSrvExpression> Expressions;

            /// <summary>
            /// Initializes a new instance of the <see cref="SrcSrvExpression"/> class.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="isDynamic">If set to <c>true</c> the expression is dynamic.</param>
            public SrcSrvExpression(string value, bool isDynamic)
            {
                Value = value;
                IsDynamic = isDynamic;
                Expressions = new List<SrcSrvExpression>();
            }

            /// <summary>
            /// Looks up a value in the dictionaries and appends it directly to the specified string builder.
            /// </summary>
            /// <param name="sb">The <see cref="StringBuilder"/> to append the value to.</param>
            /// <param name="name">The name of the expression.</param>
            /// <param name="expressions">The expressions.</param>
            /// <param name="lineVariables">The line variables.</param>
            private static void EvaluateLookup(
                StringBuilder sb, string name, Dictionary<string, SrcSrvExpression> expressions, Dictionary<string, string> lineVariables)
            {
                SrcSrvExpression expr;
                string value;
                if (expressions.TryGetValue(name, out expr))
                    expr.Evaluate(sb, expressions, lineVariables);
                else if (lineVariables.TryGetValue(name, out value))
                    sb.Append(value);
            }

            /// <summary>Evaluates this expression.</summary>
            /// <param name="expressions">The expressions.</param>
            /// <param name="lineVariables">The line variables.</param>
            /// <returns>The result of the evaluation.</returns>
            public string Evaluate(Dictionary<string, SrcSrvExpression> expressions, Dictionary<string, string> lineVariables)
            {
                if (IsDynamic)
                {
                    var sb = new StringBuilder();
                    Evaluate(sb, expressions, lineVariables);
                    return sb.ToString();
                }
                else
                {
                    return Value;
                }
            }

            /// <summary>
            /// Evaluates this expression.
            /// </summary>
            /// <param name="sb">The <see cref="StringBuilder"/> to append this expression to.</param>
            /// <param name="expressions">The expressions.</param>
            /// <param name="lineVariables">The line variables.</param>
            private void Evaluate(
                StringBuilder sb, Dictionary<string, SrcSrvExpression> expressions, Dictionary<string, string> lineVariables)
            {
                if (IsDynamic)
                {
                    switch (Value)
                    {
                        case "fnvar":
                            EvaluateVar(sb, expressions, lineVariables);
                            return;
                        case "fnbksl":
                            EvaluateBksl(sb, expressions, lineVariables);
                            return;
                        case "fnfile":
                            EvaluateFile(sb, expressions, lineVariables);
                            return;
                    }

                    if (Expressions.Count != 0)
                    {
                        foreach (var item in Expressions)
                            item.Evaluate(sb, expressions, lineVariables);
                    }
                    else if (!string.IsNullOrEmpty(Value))
                    {
                        EvaluateLookup(sb, Value, expressions, lineVariables);
                    }
                }
                else
                {
                    sb.Append(Value);
                }
            }

            /// <summary>
            /// Evaluates the the <code>%fnfile%</code> function.
            /// </summary>
            /// <param name="sb">The <see cref="StringBuilder"/> to append this expression to.</param>
            /// <param name="expressions">The expressions.</param>
            /// <param name="lineVariables">The line variables.</param>
            private void EvaluateFile(
                StringBuilder sb, Dictionary<string, SrcSrvExpression> expressions, Dictionary<string, string> lineVariables)
            {
                var ssb = new StringBuilder();
                foreach (var item in Expressions)
                    item.Evaluate(ssb, expressions, lineVariables);

                // Don't use System.IO.Path as the behavior differs on Linux.
                for (var i = ssb.Length - 1; i >= 0; i--)
                {
                    var c = ssb[i];
                    if (c == '/' || c == '\\')
                    {
                        ssb.Remove(0, i + 1);
                        break;
                    }
                }

                sb.Append(ssb.ToString());
            }

            /// <summary>
            /// Evaluates the the <code>%fnbksl%</code> function.
            /// </summary>
            /// <param name="sb">The <see cref="StringBuilder"/> to append this expression to.</param>
            /// <param name="expressions">The expressions.</param>
            /// <param name="lineVariables">The line variables.</param>
            private void EvaluateBksl(
                StringBuilder sb, Dictionary<string, SrcSrvExpression> expressions, Dictionary<string, string> lineVariables)
            {
                var pos = sb.Length;
                foreach (var item in Expressions)
                    item.Evaluate(sb, expressions, lineVariables);

                var seenSlash = false;
                for (var i = pos; i < sb.Length; i++)
                {
                    var c = sb[i];
                    if (c == '/' || c == '\\')
                    {
                        if (seenSlash)
                            sb.Remove(i--, 1);
                        else if (c == '/')
                            sb[i] = '\\';
                        seenSlash = true;
                    }
                    else
                        seenSlash = false;
                }
            }

            /// <summary>
            /// Evaluates the the <code>%fnvar%</code> function.
            /// </summary>
            /// <param name="sb">The <see cref="StringBuilder"/> to append this expression to.</param>
            /// <param name="expressions">The expressions.</param>
            /// <param name="lineVariables">The line variables.</param>
            private void EvaluateVar(
                StringBuilder sb, Dictionary<string, SrcSrvExpression> expressions, Dictionary<string, string> lineVariables)
            {
                // The documentation is unclear here. This is my best assumption.
                var ssb = new StringBuilder();
                foreach (var item in Expressions)
                    item.Evaluate(ssb, expressions, lineVariables);
                EvaluateLookup(sb, ssb.ToString(), expressions, lineVariables);
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            [ExcludeFromCodeCoverage]
            public override string ToString()
            {
                if (IsDynamic)
                {
                    var sb = new StringBuilder();

                    if (!string.IsNullOrEmpty(Value))
                    {
                        sb.AppendFormat("%{0}%", Value);
                        if (Expressions.Count != 0) sb.Append("(");
                    }

                    foreach (var item in Expressions)
                        sb.Append(item.ToString());

                    if (!string.IsNullOrEmpty(Value) && Expressions.Count != 0)
                        sb.Append(")");
                    return sb.ToString();
                }
                else
                {
                    return Value ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// Asynchronously parses the SRCSRV stream in the specified PDB file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{SourceInformationCollection}"/> that represents the asynchronous parses operation.
        /// </returns>
        public static Task<SourceInformationCollection> ParseAsync(Pdb70File file, CancellationToken cancellationToken)
        {
            if (!file.StreamExists("srcsrv")) return null;

            using (var reader = new StreamReader(file.GetStream("srcsrv")))
            {
                return ParseAsync(reader, cancellationToken);
            }
        }

        /// <summary>
        /// Asynchronously parses the SRCSRV stream in the specified PDB file.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{SourceInformationCollection}" /> that represents the asynchronous parses operation.
        /// </returns>
        [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules", Justification = "Local state machine.")]
        public static async Task<SourceInformationCollection> ParseAsync(TextReader reader, CancellationToken cancellationToken)
        {
            const int State_Start = 0;
            const int State_Ini = 1;
            const int State_Vars = 2;
            const int State_Files = 3;

            var vars = new Dictionary<string, SrcSrvExpression>(StringComparer.OrdinalIgnoreCase);
            var lineVars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var result = new SourceInformationCollection();

            var state = State_Start;
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (state)
                {
                    case State_Start:
                        if (line.StartsWith("SRCSRV: ini"))
                            state = State_Ini;
                        break;
                    case State_Ini:
                        if (line.StartsWith("SRCSRV: variables"))
                            state = State_Vars;
                        break;
                    case State_Vars:
                        if (line.StartsWith("SRCSRV: source files"))
                        {
                            if (!vars.ContainsKey("SRCSRVTRG")) return null;
                            state = State_Files;
                        }
                        else
                        {
                            var index = line.IndexOf('=');
                            if (index > 0)
                            {
                                var var = ParseVariable(line.Substring(index + 1));
                                if (var == null) return null;
                                vars.Add(line.Substring(0, index), var);
                            }
                        }

                        break;
                    case State_Files:
                        var split = line.Split('*');
                        if (!string.IsNullOrWhiteSpace(line) && split.Length > 0)
                        {
                            lineVars.Clear();
                            for (var i = 0; i < split.Length; i++)
                                lineVars[string.Format(CultureInfo.InvariantCulture, "VAR{0}", i + 1)] = split[i];
                            var trg = vars["SRCSRVTRG"].Evaluate(vars, lineVars);
                            result.Add(new SourceInformation(split[0], trg));
                        }

                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Parses a variable value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The variable expression.</returns>
        [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules", Justification = "Local state machine.")]
        private static SrcSrvExpression ParseVariable(string value)
        {
            const int State_Literal = 0;
            const int State_Token = 1;
            const int State_Expect_FunctionOpen = 2;

            var stack = new Stack<SrcSrvExpression>();
            stack.Push(new SrcSrvExpression(null, true));

            var sb = new StringBuilder();
            var state = State_Literal;

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (state)
                {
                    case State_Literal:
                        if (c == '%')
                        {
                            if (sb.Length > 0)
                            {
                                stack.Peek().Expressions.Add(new SrcSrvExpression(sb.ToString(), false));
                                sb.Clear();
                            }

                            state = State_Token;
                        }
                        else if (c == ')' && stack.Count > 1)
                            stack.Pop();
                        else
                            sb.Append(c);
                        break;
                    case State_Token:
                        if (c == '%')
                        {
                            var name = sb.ToString();
                            sb.Clear();
                            switch (name)
                            {
                                case "fnvar":
                                case "fnbksl":
                                case "fnfile":
                                    var expr = new SrcSrvExpression(name, true);
                                    stack.Peek().Expressions.Add(expr);
                                    stack.Push(expr);
                                    state = State_Expect_FunctionOpen;
                                    break;
                                case "":
                                    return null;
                                default:
                                    stack.Peek().Expressions.Add(new SrcSrvExpression(name, true));
                                    state = State_Literal;
                                    break;
                            }
                        }
                        else
                            sb.Append(c);
                        break;
                    case State_Expect_FunctionOpen:
                        if (c != '(') return null;
                        state = State_Literal;
                        break;
                }
            }

            switch (state)
            {
                case State_Literal:
                    if (sb.Length > 0)
                        stack.Peek().Expressions.Add(new SrcSrvExpression(sb.ToString(), false));
                    break;
            }

            if (stack.Count != 1) return null;
            return stack.Pop();
        }
    }
}
