// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Generic;

namespace Cosmos.Kernel.System.Graphics.CosmSl;

/// <summary>
/// The abstract syntax tree produced by <see cref="CosmCompiler.Parse"/> from
/// a CosmSl shader source. The tree is backend neutral: the 3D drivers walk
/// it and lower it to whatever their device consumes (TGSI for the virgl
/// path, DirectX-style bytecode for the SVGA II path).
/// </summary>
public class ShaderProgramNode
{
    /// <summary>The input/output structures declared at the top of the source.</summary>
    public List<StructDeclNode> Structs { get; set; } = [];

    /// <summary>The uniform constants declared at the top of the source.</summary>
    public List<UniformNode> Uniforms { get; set; } = [];

    /// <summary>The functions of the program; the entry point is the one named <c>main</c>.</summary>
    public List<FunctionDeclNode> Functions { get; set; } = [];
}

/// <summary>
/// A struct declaration. Fields carry the hardware semantic they bind to;
/// the parser fills a default semantic in when the source omits the colon.
/// </summary>
public class StructDeclNode
{
    /// <summary>The name of the struct as written in the source.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The fields in declaration order; order decides the register slots.</summary>
    public List<FieldNode> Fields { get; set; } = [];
}

/// <summary>
/// One field of a <see cref="StructDeclNode"/>. The <see cref="Semantic"/> is
/// the uppercase hardware semantic (POSITION, COLOR, TEXCOORD) with its index
/// suffix, e.g. <c>TEXCOORD0</c>.
/// </summary>
public class FieldNode
{
    /// <summary>The value type of the field (float, vec2, vec3, vec4).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>The field name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The resolved hardware semantic, defaults included.</summary>
    public string Semantic { get; set; } = string.Empty;
}

/// <summary>A global uniform constant.</summary>
public class UniformNode
{
    /// <summary>The value type of the uniform (float, vec2, vec3, vec4, mat4, sampler2D).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>The uniform name.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>A function declaration with its statement body.</summary>
public class FunctionDeclNode
{
    /// <summary>The function name; the driver entry point is <c>main</c>.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The return type (vec4, a struct name, or void).</summary>
    public string ReturnType { get; set; } = string.Empty;

    /// <summary>The hardware semantic of the return value (SV_TARGET, SV_POSITION).</summary>
    public string ReturnSemantic { get; set; } = string.Empty;

    /// <summary>The parameters in declaration order.</summary>
    public List<ParameterNode> Parameters { get; set; } = [];

    /// <summary>The statement list forming the body.</summary>
    public List<StatementNode> Body { get; set; } = [];
}

/// <summary>A function parameter.</summary>
public class ParameterNode
{
    /// <summary>The parameter type (a struct name for the input bundles).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>The parameter name.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>Base class of every statement in a function body.</summary>
public abstract class StatementNode
{
}

/// <summary>
/// Declares a local variable, optionally with an initializer expression.
/// </summary>
public class VariableDeclarationStatement : StatementNode
{
    /// <summary>The declared value type.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>The local variable name.</summary>
    public string VariableName { get; set; } = string.Empty;

    /// <summary>The initializer, or <see langword="null"/> when the variable starts undefined.</summary>
    public ExpressionNode? Initializer { get; set; }
}

/// <summary>
/// Assigns to a local variable or to a member of a local struct variable
/// (<c>output.position = ...</c>).
/// </summary>
public class AssignmentStatement : StatementNode
{
    /// <summary>The assignment target: a variable reference or a member access.</summary>
    public ExpressionNode Target { get; set; } = null!;

    /// <summary>The value assigned.</summary>
    public ExpressionNode Value { get; set; } = null!;
}

/// <summary>Returns from the enclosing function, optionally with a value.</summary>
public class ReturnStatement : StatementNode
{
    /// <summary>The returned expression, or <see langword="null"/> for void functions.</summary>
    public ExpressionNode? Expression { get; set; }
}

/// <summary>Base class of every expression node.</summary>
public abstract class ExpressionNode
{
}

/// <summary>A numeric literal. Integer literals are widened to float.</summary>
public class LiteralExpression : ExpressionNode
{
    /// <summary>The literal value.</summary>
    public float Value { get; set; }
}

/// <summary>A reference to a local variable, a uniform or a parameter.</summary>
public class VariableRefExpression : ExpressionNode
{
    /// <summary>The referenced name.</summary>
    public string VariableName { get; set; } = string.Empty;
}

/// <summary>
/// A member access: a swizzle (<c>v.xz</c>) on a vector, a struct field
/// (<c>input.color</c>), or both chained.
/// </summary>
public class MemberAccessExpression : ExpressionNode
{
    /// <summary>The expression the member is read from.</summary>
    public ExpressionNode ObjectRef { get; set; } = null!;

    /// <summary>The member name or swizzle string.</summary>
    public string MemberName { get; set; } = string.Empty;
}

/// <summary>A binary arithmetic expression.</summary>
public class BinaryOpExpression : ExpressionNode
{
    /// <summary>The operator: one of <c>+</c>, <c>-</c>, <c>*</c>, <c>/</c>.</summary>
    public string Operator { get; set; } = string.Empty;

    /// <summary>The left operand.</summary>
    public ExpressionNode Left { get; set; } = null!;

    /// <summary>The right operand.</summary>
    public ExpressionNode Right { get; set; } = null!;
}

/// <summary>
/// An intrinsic or constructor call: <c>mul</c>, <c>dot</c>, <c>tex2D</c>,
/// <c>saturate</c> and friends, plus the vector constructors.
/// </summary>
public class FunctionCallExpression : ExpressionNode
{
    /// <summary>The function name as written in the source.</summary>
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>The arguments in call order.</summary>
    public List<ExpressionNode> Arguments { get; set; } = [];
}
