// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;

namespace Cosmos.Kernel.System.Graphics.CosmSl;

/// <summary>
/// Recursive descent parser turning the <see cref="CosmSlLexer"/> token
/// stream into a <see cref="ShaderProgramNode"/>. One token of lookahead,
/// no backtracking. The parser resolves semantics as it goes: a field
/// without an explicit one after the colon gets a default (<c>vec4</c>
/// fields become COLOR, everything else TEXCOORD), so plain sources like the
/// engine's built-in shaders parse into hardware-ready declarations.
/// </summary>
internal sealed class CosmSlParser
{
    // The scalar and vector type keywords every shader may use, plus the
    // samplers and matrices uniforms may use. Struct names are discovered
    // from the source and added to this set.
    private static readonly string[] s_builtinTypes =
    [
        "float", "int", "bool",
        "vec2", "vec3", "vec4",
        "mat4", "sampler2D",
    ];

    private readonly List<CosmSlLexer.Token> _tokens;
    private int _position;
    private readonly HashSet<string> _structNames = [];

    private CosmSlParser(List<CosmSlLexer.Token> tokens)
    {
        _tokens = tokens;
    }

    /// <summary>Parses a full shader program.</summary>
    internal static ShaderProgramNode Parse(string source)
    {
        return new CosmSlParser(CosmSlLexer.Lex(source)).ParseProgram();
    }

    private ShaderProgramNode ParseProgram()
    {
        ShaderProgramNode program = new();

        while (Current.Kind != CosmSlLexer.TokenKind.End)
        {
            if (IsKeyword("struct"))
            {
                program.Structs.Add(ParseStruct());
            }
            else if (IsKeyword("uniform"))
            {
                program.Uniforms.Add(ParseUniform());
            }
            else
            {
                program.Functions.Add(ParseFunction());
            }
        }

        return program;
    }

    private StructDeclNode ParseStruct()
    {
        ExpectKeyword("struct");

        StructDeclNode decl = new()
        {
            Name = ExpectIdentifier("a struct name"),
        };
        _structNames.Add(decl.Name);

        ExpectSymbol("{");
        while (!AcceptSymbol("}"))
        {
            string type = ExpectIdentifier("a field type");
            string name = ExpectIdentifier("a field name");

            string semantic;
            if (AcceptSymbol(":"))
            {
                semantic = ExpectIdentifier("a semantic name");
            }
            else
            {
                // No colon: the field falls back to a default semantic, the
                // common case for fragment inputs.
                semantic = DefaultSemantic(type, decl.Fields);
            }

            decl.Fields.Add(new FieldNode
            {
                Type = type,
                Name = name,
                Semantic = semantic,
            });

            ExpectSymbol(";");
        }

        ExpectSymbol(";");
        return decl;
    }

    private UniformNode ParseUniform()
    {
        ExpectKeyword("uniform");

        UniformNode uniform = new()
        {
            Type = ExpectIdentifier("a uniform type"),
            Name = ExpectIdentifier("a uniform name"),
        };

        ExpectSymbol(";");
        return uniform;
    }

    private FunctionDeclNode ParseFunction()
    {
        FunctionDeclNode function = new()
        {
            ReturnType = ExpectIdentifier("a return type"),
            Name = ExpectIdentifier("a function name"),
        };

        ExpectSymbol("(");
        if (!AcceptSymbol(")"))
        {
            while (true)
            {
                function.Parameters.Add(new ParameterNode
                {
                    Type = ExpectIdentifier("a parameter type"),
                    Name = ExpectIdentifier("a parameter name"),
                });

                if (AcceptSymbol(","))
                {
                    continue;
                }
                ExpectSymbol(")");
                break;
            }
        }

        if (AcceptSymbol(":"))
        {
            function.ReturnSemantic = ExpectIdentifier("a semantic name");
        }

        ExpectSymbol("{");
        while (!AcceptSymbol("}"))
        {
            function.Body.Add(ParseStatement());
        }

        return function;
    }

    private StatementNode ParseStatement()
    {
        if (IsKeyword("return"))
        {
            Advance();
            ReturnStatement statement = new();
            if (!AcceptSymbol(";"))
            {
                statement.Expression = ParseAdditive();
                ExpectSymbol(";");
            }
            return statement;
        }

        // A type followed by an identifier opens a local declaration;
        // anything else must be an assignment to a variable or a struct
        // member ("output.position = ...").
        if (IsType(Current) && Peek(1).Kind == CosmSlLexer.TokenKind.Identifier)
        {
            VariableDeclarationStatement statement = new()
            {
                Type = Advance().Text,
                VariableName = ExpectIdentifier("a variable name"),
            };

            if (AcceptSymbol("="))
            {
                statement.Initializer = ParseAdditive();
            }

            ExpectSymbol(";");
            return statement;
        }

        ExpressionNode target = ParseReference();
        if (target is not VariableRefExpression and not MemberAccessExpression)
        {
            throw new CosmSlException("Only variables and struct members can be assigned.", Current.Line);
        }

        // The lexer emits single-character symbols, so a compound assignment
        // ("dx += pull;") arrives as two tokens. In statement position after
        // a reference target the only legal continuations are "=" (plain
        // assignment) and op "=" (compound), so the desugar below is
        // unambiguous: the compound form lowers to the plain assignment
        // "dx = dx + pull" right here, and no new statement kind exists.
        // The target node is evaluated once per execution exactly like the
        // explicit form would be, so member targets behave identically.
        string? compoundOp = null;
        if (Current.Kind == CosmSlLexer.TokenKind.Symbol &&
            (Current.Text == "+" || Current.Text == "-" || Current.Text == "*" || Current.Text == "/"))
        {
            compoundOp = Advance().Text;
        }

        ExpectSymbol("=");

        ExpressionNode value = ParseAdditive();
        if (compoundOp is not null)
        {
            value = new BinaryOpExpression
            {
                Operator = compoundOp,
                Left = target,
                Right = value,
            };
        }

        AssignmentStatement assignment = new()
        {
            Target = target,
            Value = value,
        };
        ExpectSymbol(";");
        return assignment;
    }

    private ExpressionNode ParseAdditive()
    {
        ExpressionNode left = ParseMultiplicative();
        while (Current.Kind == CosmSlLexer.TokenKind.Symbol && (Current.Text == "+" || Current.Text == "-"))
        {
            string op = Advance().Text;
            left = new BinaryOpExpression
            {
                Operator = op,
                Left = left,
                Right = ParseMultiplicative(),
            };
        }
        return left;
    }

    private ExpressionNode ParseMultiplicative()
    {
        ExpressionNode left = ParseUnary();
        while (Current.Kind == CosmSlLexer.TokenKind.Symbol && (Current.Text == "*" || Current.Text == "/"))
        {
            string op = Advance().Text;
            left = new BinaryOpExpression
            {
                Operator = op,
                Left = left,
                Right = ParseUnary(),
            };
        }
        return left;
    }

    private ExpressionNode ParseUnary()
    {
        if (Current.Kind == CosmSlLexer.TokenKind.Symbol && Current.Text == "-")
        {
            Advance();
            ExpressionNode inner = ParseUnary();
            if (inner is LiteralExpression literal)
            {
                return new LiteralExpression { Value = -literal.Value };
            }
            return new BinaryOpExpression
            {
                Operator = "-",
                Left = new LiteralExpression { Value = 0f },
                Right = inner,
            };
        }

        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        if (Current.Kind == CosmSlLexer.TokenKind.Number)
        {
            return new LiteralExpression { Value = Advance().Number };
        }

        if (Current.Kind == CosmSlLexer.TokenKind.Symbol && Current.Text == "(")
        {
            Advance();
            ExpressionNode inner = ParseAdditive();
            ExpectSymbol(")");
            return inner;
        }

        if (Current.Kind == CosmSlLexer.TokenKind.Identifier)
        {
            string name = Advance().Text;

            if (AcceptSymbol("("))
            {
                FunctionCallExpression call = new()
                {
                    FunctionName = name,
                };

                if (!AcceptSymbol(")"))
                {
                    while (true)
                    {
                        call.Arguments.Add(ParseAdditive());
                        if (AcceptSymbol(","))
                        {
                            continue;
                        }
                        ExpectSymbol(")");
                        break;
                    }
                }

                return call;
            }

            return ParseMemberChain(name);
        }

        throw new CosmSlException($"Unexpected token '{Current.Text}'.", Current.Line);
    }

    /// <summary>
    /// Parses an identifier plus any number of member suffixes. Used both in
    /// expressions and as the assignment target form.
    /// </summary>
    private ExpressionNode ParseReference()
    {
        if (Current.Kind != CosmSlLexer.TokenKind.Identifier)
        {
            throw new CosmSlException($"Unexpected token '{Current.Text}'.", Current.Line);
        }

        string name = Advance().Text;
        return ParseMemberChain(name);
    }

    private ExpressionNode ParseMemberChain(string name)
    {
        ExpressionNode expression = new VariableRefExpression { VariableName = name };
        while (AcceptSymbol("."))
        {
            expression = new MemberAccessExpression
            {
                ObjectRef = expression,
                MemberName = ExpectIdentifier("a member name"),
            };
        }

        return expression;
    }

    private static string DefaultSemantic(string type, List<FieldNode> previousFields)
    {
        // vec4 fields default to COLOR (the render target / interpolant
        // color), every other type to TEXCOORD. The index counts the fields
        // already resolved to the same semantic in this struct, so
        // "vec4 color; vec2 uv;" lands on COLOR and TEXCOORD0.
        string semantic = type == "vec4" ? "COLOR" : "TEXCOORD";
        int index = 0;
        foreach (FieldNode field in previousFields)
        {
            if (field.Semantic == semantic || field.Semantic.StartsWith(semantic, StringComparison.Ordinal))
            {
                index++;
            }
        }

        return index == 0 ? semantic : $"{semantic}{index}";
    }

    private bool IsKeyword(string keyword) =>
        Current.Kind == CosmSlLexer.TokenKind.Identifier && Current.Text == keyword;

    private void ExpectKeyword(string keyword)
    {
        if (!IsKeyword(keyword))
        {
            throw new CosmSlException($"Expected '{keyword}'.", Current.Line);
        }
        Advance();
    }

    private bool IsType(CosmSlLexer.Token token) =>
        token.Kind == CosmSlLexer.TokenKind.Identifier &&
        (Array.IndexOf(s_builtinTypes, token.Text) >= 0 || _structNames.Contains(token.Text));

    private string ExpectIdentifier(string what)
    {
        if (Current.Kind != CosmSlLexer.TokenKind.Identifier)
        {
            throw new CosmSlException($"Expected {what}.", Current.Line);
        }
        return Advance().Text;
    }

    private void ExpectSymbol(string symbol)
    {
        if (Current.Kind != CosmSlLexer.TokenKind.Symbol || Current.Text != symbol)
        {
            throw new CosmSlException($"Expected '{symbol}'.", Current.Line);
        }
        Advance();
    }

    private bool AcceptSymbol(string symbol)
    {
        if (Current.Kind == CosmSlLexer.TokenKind.Symbol && Current.Text == symbol)
        {
            Advance();
            return true;
        }
        return false;
    }

    private CosmSlLexer.Token Peek(int ahead)
    {
        int index = _position + ahead;
        return index < _tokens.Count ? _tokens[index] : _tokens[^1];
    }

    private CosmSlLexer.Token Current => Peek(0);

    private CosmSlLexer.Token Advance()
    {
        CosmSlLexer.Token token = Current;
        if (_position < _tokens.Count - 1)
        {
            _position++;
        }
        return token;
    }
}
