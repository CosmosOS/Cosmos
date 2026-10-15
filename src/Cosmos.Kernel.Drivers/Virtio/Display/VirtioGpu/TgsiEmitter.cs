// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Globalization;
using Cosmos.Kernel.System.Graphics.CosmSl;

namespace Cosmos.Kernel.Drivers.Virtio.Display.VirtioGpu;

/// <summary>Which pipeline stage a shader is compiled for.</summary>
internal enum ShaderStage
{
    Vertex,
    Fragment,
}

/// <summary>
/// The compiled form of one CosmSl shader. <see cref="Text"/> is the TGSI
/// assembly the virgl host actually parses (virglrenderer translates shader
/// text through tgsi_text_translate), <see cref="Tokens"/> the equivalent
/// binary token stream for tools, tests and future transports.
/// </summary>
internal sealed class TgsiShader
{
    /// <summary>TGSI assembly, one instruction per line, terminated by END.</summary>
    public string Text { get; }

    /// <summary>The same shader as a flat array of 32-bit TGSI tokens.</summary>
    public uint[] Tokens { get; }

    public TgsiShader(string text, uint[] tokens)
    {
        Text = text;
        Tokens = tokens;
    }
}

/// <summary>
/// Lowers a <see cref="ShaderProgramNode"/> to TGSI (Tungsten Graphics Shader
/// Infrastructure). A shader becomes three sections in memory: the header
/// naming the processor stage, declarations registering inputs, outputs,
/// constants, temporaries, immediates and samplers, and the instruction
/// stream itself. Everything is vec4 based: scalars travel as replicated
/// swizzles, a mat4 occupies four consecutive constant registers, and
/// matrix-vector products lower to DP4 chains.
/// </summary>
internal static class TgsiEmitter
{
    // --- TGSI constants (virglrenderer's p_shader_tokens.h numbering) ---

    private const uint TokenDeclaration = 0;
    private const uint TokenImmediate = 1;
    private const uint TokenInstruction = 2;

    private const uint FileConstant = 1;
    private const uint FileInput = 2;
    private const uint FileOutput = 3;
    private const uint FileTemporary = 4;
    private const uint FileSampler = 5;
    private const uint FileImmediate = 7;

    private const uint SemanticPosition = 0;
    private const uint SemanticColor = 1;
    private const uint SemanticTexCoord = 6;

    private const uint ImmediateFloat32 = 0;

    private const uint OpcodeMov = 1;
    private const uint OpcodeRsq = 4;
    private const uint OpcodeMul = 7;
    private const uint OpcodeAdd = 8;
    private const uint OpcodeDp3 = 9;
    private const uint OpcodeDp4 = 10;
    private const uint OpcodeMin = 12;
    private const uint OpcodeMax = 13;
    private const uint OpcodeSlt = 14;
    private const uint OpcodeSge = 15;
    private const uint OpcodeSub = 17;
    private const uint OpcodeLrp = 18;
    private const uint OpcodeSqrt = 20;
    private const uint OpcodeFrc = 21;
    private const uint OpcodeFlr = 22;
    private const uint OpcodeRound = 23;
    private const uint OpcodeEx2 = 24;
    private const uint OpcodeLg2 = 25;
    private const uint OpcodePow = 26;
    private const uint OpcodeAbs = 28;
    private const uint OpcodeCos = 30;
    private const uint OpcodeSeq = 38;
    private const uint OpcodeSgt = 39;
    private const uint OpcodeSin = 40;
    private const uint OpcodeSle = 41;
    private const uint OpcodeSne = 42;
    private const uint OpcodeTex = 43;
    private const uint OpcodeDiv = 58;
    private const uint OpcodeDp2 = 59;

    // TGSI programs always end with an explicit END (opcode 98 in
    // virglrenderer's tgsi_opcode enum).
    private const uint OpcodeEnd = 98;

    private const uint Texture2D = 2;

    // The write mask covering all four components; per-component masks are
    // the low four bits, one bit each for xyzw.
    private const uint MaskXyzw = 0xF;

    /// <summary>
    /// Compiles the program's <c>main</c> function for the given stage.
    /// </summary>
    /// <exception cref="CosmSlException">The program cannot be lowered.</exception>
    internal static TgsiShader Emit(ShaderProgramNode ast, ShaderStage stage)
    {
        return new Emitter(ast, stage).Compile();
    }

    private sealed class Emitter
    {
        private readonly ShaderProgramNode _ast;
        private readonly ShaderStage _stage;
        private readonly FunctionDeclNode _main;

        private int _tempCount;
        private readonly List<TgsiDecl> _decls = [];
        private readonly List<float[]> _immediates = [];
        private readonly Dictionary<float, int> _immediateLookup = [];
        private readonly List<TgsiInst> _instructions = [];

        // Symbol tables: locals and parameters share one namespace, uniforms
        // live separately because they resolve to constant/sampler registers.
        private readonly Dictionary<string, VarInfo> _variables = [];
        private readonly Dictionary<string, UniformInfo> _uniforms = [];
        private int _constantSlots;
        private int _samplerCount;

        public Emitter(ShaderProgramNode ast, ShaderStage stage)
        {
            _ast = ast;
            _stage = stage;

            FunctionDeclNode? main = null;
            foreach (FunctionDeclNode function in ast.Functions)
            {
                if (function.Name == "main")
                {
                    main = function;
                    break;
                }
            }

            _main = main ?? throw new CosmSlException("The shader has no main() function.", 0);
        }

        public TgsiShader Compile()
        {
            DeclareUniforms();
            DeclareMainInputs();
            DeclareMainOutputs();

            foreach (StatementNode statement in _main.Body)
            {
                EmitStatement(statement);
            }

            _instructions.Add(new TgsiInst { Opcode = OpcodeEnd });

            if (_tempCount > 0)
            {
                _decls.Add(TgsiDecl.Range(FileTemporary, 0, _tempCount - 1));
            }

            return new TgsiShader(SerializeText(), SerializeTokens());
        }

        // --- Declarations ---

        private void DeclareUniforms()
        {
            foreach (UniformNode uniform in _ast.Uniforms)
            {
                if (uniform.Type == "sampler2D")
                {
                    _uniforms[uniform.Name] = UniformInfo.Sampler(_samplerCount++);
                    continue;
                }

                // A mat4 occupies four consecutive constant registers; the
                // engine uploads it in Matrix4x4 row order so DP4 chains can
                // consume the rows directly.
                int slots = uniform.Type == "mat4" ? 4 : 1;
                _uniforms[uniform.Name] = UniformInfo.Constant(_constantSlots, uniform.Type);
                _constantSlots += slots;
            }

            if (_constantSlots > 0)
            {
                _decls.Add(TgsiDecl.TwoDimensional(FileConstant, 0, 0, _constantSlots - 1));
            }

            for (int i = 0; i < _samplerCount; i++)
            {
                _decls.Add(TgsiDecl.Simple(FileSampler, i));
            }
        }

        private void DeclareMainInputs()
        {
            // One input register slot per struct field, in declaration order;
            // the vertex element setup on the engine side matches this order.
            foreach (ParameterNode parameter in _main.Parameters)
            {
                StructDeclNode? structure = FindStruct(parameter.Type)
                    ?? throw new CosmSlException($"main() parameter type '{parameter.Type}' is not a struct.", 0);

                VarInfo variable = VarInfo.StructParameter(parameter.Type);
                foreach (FieldNode field in structure.Fields)
                {
                    if (_stage == ShaderStage.Fragment)
                    {
                        // Fragment inputs link to the vertex outputs by
                        // semantic, so the names travel along. Vertex inputs
                        // bind positionally to the vertex elements and the
                        // TGSI text grammar rejects semantics on them.
                        (uint semantic, int index) = ResolveSemantic(field.Semantic);
                        _decls.Add(TgsiDecl.WithSemantic(FileInput, variable.FieldCount, semantic, index));
                    }
                    else
                    {
                        _decls.Add(TgsiDecl.Simple(FileInput, variable.FieldCount));
                    }

                    variable.AddField(field.Name, field.Type);
                }

                _variables[parameter.Name] = variable;
            }
        }

        private void DeclareMainOutputs()
        {
            if (_stage == ShaderStage.Vertex)
            {
                StructDeclNode? structure = FindStruct(_main.ReturnType)
                    ?? throw new CosmSlException($"Vertex main() must return a struct, not '{_main.ReturnType}'.", 0);

                int slot = 0;
                foreach (FieldNode field in structure.Fields)
                {
                    (uint semantic, int index) = ResolveSemantic(field.Semantic);
                    _decls.Add(TgsiDecl.WithSemantic(FileOutput, slot, semantic, index));
                    slot++;
                }

                return;
            }

            // Fragment: the return semantic names the render target.
            (uint name, int targetIndex) = ResolveSemantic(_main.ReturnSemantic);
            if (name != SemanticColor)
            {
                throw new CosmSlException("Fragment main() must return with an SV_TARGET semantic.", 0);
            }

            _decls.Add(TgsiDecl.WithSemantic(FileOutput, 0, name, targetIndex));
        }

        // --- Statements ---

        private void EmitStatement(StatementNode statement)
        {
            switch (statement)
            {
                case VariableDeclarationStatement declaration:
                    EmitDeclaration(declaration);
                    break;

                case AssignmentStatement assignment:
                    EmitAssignment(assignment);
                    break;

                case ReturnStatement ret:
                    EmitReturn(ret);
                    break;

                default:
                    throw new CosmSlException("Unsupported statement in shader body.", 0);
            }
        }

        private void EmitDeclaration(VariableDeclarationStatement statement)
        {
            VarInfo variable = DeclareLocal(statement.Type, statement.VariableName);

            if (statement.Initializer is null)
            {
                return;
            }

            if (variable.IsStruct)
            {
                throw new CosmSlException("Struct locals are initialized member by member.", 0);
            }

            Operand value = EmitExpression(statement.Initializer);
            Copy(variable.Single(), value);
        }

        private void EmitAssignment(AssignmentStatement statement)
        {
            // Only plain locals and struct members are writable; deeper chains
            // (o.a.b = ...) have no representation in a vec4 register file.
            Operand target;
            if (statement.Target is VariableRefExpression variableRef)
            {
                VarInfo variable = Lookup(variableRef.VariableName);
                if (variable.IsStruct)
                {
                    throw new CosmSlException("Whole-struct assignment is not supported; assign members.", 0);
                }

                target = variable.Single();
            }
            else if (statement.Target is MemberAccessExpression member)
            {
                target = EmitMemberTarget(member);
            }
            else
            {
                throw new CosmSlException("Unsupported assignment target.", 0);
            }

            Operand value = EmitExpression(statement.Value);
            Copy(target, value);
        }

        private void EmitReturn(ReturnStatement statement)
        {
            if (_stage == ShaderStage.Vertex)
            {
                if (statement.Expression is not VariableRefExpression reference)
                {
                    throw new CosmSlException("Vertex main() returns a struct variable.", 0);
                }

                VarInfo variable = Lookup(reference.VariableName);
                if (!variable.IsStruct)
                {
                    throw new CosmSlException("Vertex main() returns a struct variable.", 0);
                }

                // Copy every struct field into its output register slot; the
                // field order matches the output declarations.
                for (int i = 0; i < variable.FieldCount; i++)
                {
                    Operand field = variable.FieldByIndex(i);
                    Emit1(OpcodeMov, OutputOperand(i, field.Components), Src(field));
                }

                return;
            }

            if (statement.Expression is null)
            {
                throw new CosmSlException("Fragment main() must return a value.", 0);
            }

            Operand value = EmitExpression(statement.Expression);
            Emit1(OpcodeMov, OutputOperand(0, value.Components), Src(value));
        }

        // --- Expressions ---

        private Operand EmitExpression(ExpressionNode expression)
        {
            switch (expression)
            {
                case LiteralExpression literal:
                    return ImmediateOperand(literal.Value);

                case VariableRefExpression reference:
                    return Lookup(reference.VariableName).Single();

                case MemberAccessExpression member:
                    return EmitMemberRead(member);

                case BinaryOpExpression binary:
                    return EmitBinary(binary);

                case FunctionCallExpression call:
                    return EmitCall(call);

                default:
                    throw new CosmSlException("Unsupported expression.", 0);
            }
        }

        private Operand EmitMemberRead(MemberAccessExpression member)
        {
            // The object side is either a struct (field access) or a vector
            // (swizzle). Chained swizzles compose through the base swizzle.
            if (member.ObjectRef is MemberAccessExpression inner)
            {
                Operand source = EmitMemberRead(inner);
                if (source.Kind == OperandKind.Struct)
                {
                    throw new CosmSlException("Unsupported member chain.", 0);
                }
                return SwizzleOperand(source, member.MemberName);
            }

            if (member.ObjectRef is VariableRefExpression reference)
            {
                VarInfo variable = Lookup(reference.VariableName);

                if (variable.IsStruct)
                {
                    return variable.Field(member.MemberName)
                        ?? throw new CosmSlException($"Struct has no member '{member.MemberName}'.", 0);
                }

                return SwizzleOperand(variable.Single(), member.MemberName);
            }

            Operand operand = EmitExpression(member.ObjectRef);
            return SwizzleOperand(operand, member.MemberName);
        }

        /// <summary>
        /// Resolves the left side of an assignment, which may end in a struct
        /// field (writemask from the field type) or a swizzle subset.
        /// </summary>
        private Operand EmitMemberTarget(MemberAccessExpression member)
        {
            if (member.ObjectRef is MemberAccessExpression inner)
            {
                Operand nested = EmitMemberTarget(inner);
                return SwizzleWritable(nested, member.MemberName);
            }

            if (member.ObjectRef is VariableRefExpression reference)
            {
                VarInfo variable = Lookup(reference.VariableName);
                if (!variable.IsStruct)
                {
                    return SwizzleWritable(variable.Single(), member.MemberName);
                }

                return variable.Field(member.MemberName)
                    ?? throw new CosmSlException($"Struct has no member '{member.MemberName}'.", 0);
            }

            throw new CosmSlException("Unsupported assignment target.", 0);
        }

        private Operand EmitBinary(BinaryOpExpression binary)
        {
            Operand left = EmitExpression(binary.Left);
            Operand right = EmitExpression(binary.Right);

            // Matrix product: a mat4 on the left lowers to a DP4 chain, one
            // dot product per output component against a constant register
            // row (the rows are uploaded in Matrix4x4 row order).
            if (left.Kind == OperandKind.Matrix)
            {
                if (binary.Operator != "*" || right.Components != 4)
                {
                    throw new CosmSlException("A matrix only multiplies with a vec4 through '*'.", 0);
                }

                return MultiplyMatrix(left, right);
            }

            uint opcode = binary.Operator switch
            {
                "+" => OpcodeAdd,
                "-" => OpcodeSub,
                "*" => OpcodeMul,
                "/" => OpcodeDiv,
                _ => throw new CosmSlException($"Unknown operator '{binary.Operator}'.", 0),
            };

            (Operand a, Operand b) = AlignOperands(left, right);
            Operand result = NewTemp(a.Components);
            Emit2(opcode, result, Src(a), Src(b));
            return result;
        }

        private Operand MultiplyMatrix(Operand matrix, Operand vector)
        {
            Operand result = NewTemp(4);
            for (int row = 0; row < 4; row++)
            {
                Operand rowRegister = matrix with
                {
                    Index = matrix.Index + row,
                };

                // DP4 writes one channel, so each row lands in its component.
                Operand channel = result with
                {
                    WriteMask = 1u << row,
                };

                Emit2(OpcodeDp4, channel, Src(rowRegister), Src(vector));
            }

            return result;
        }

        private Operand EmitCall(FunctionCallExpression call)
        {
            string name = call.FunctionName;

            // Constructors first: they build vectors component by component.
            if (name == "vec2" || name == "vec3" || name == "vec4")
            {
                return ConstructVector(name[3] - '0', call.Arguments);
            }

            if (name == "float")
            {
                if (call.Arguments.Count != 1)
                {
                    throw new CosmSlException("float() takes one argument.", 0);
                }
                Operand value = EmitExpression(call.Arguments[0]);
                return value with { Components = 1, Swizzle = "x" };
            }

            switch (name)
            {
                case "mul":
                    return EmitMul(call);
                case "dot":
                    return EmitDot(call);
                case "tex2D":
                    return EmitTex2D(call);
                case "saturate":
                    return EmitUnary(call, OpcodeMov, saturate: true);
                case "abs":
                    return EmitUnary(call, OpcodeAbs);
                case "floor":
                    return EmitUnary(call, OpcodeFlr);
                case "frac":
                    return EmitUnary(call, OpcodeFrc);
                case "round":
                    return EmitUnary(call, OpcodeRound);
                case "sqrt":
                    return EmitUnary(call, OpcodeSqrt);
                case "rsq":
                    return EmitUnary(call, OpcodeRsq);
                case "exp2":
                    return EmitUnary(call, OpcodeEx2);
                case "log2":
                    return EmitUnary(call, OpcodeLg2);
                case "sin":
                    return EmitUnary(call, OpcodeSin);
                case "cos":
                    return EmitUnary(call, OpcodeCos);
                case "min":
                    return EmitBinaryIntrinsic(call, OpcodeMin);
                case "max":
                    return EmitBinaryIntrinsic(call, OpcodeMax);
                case "clamp":
                    return EmitClamp(call);
                case "lerp":
                    return EmitLerp(call);
                default:
                    throw new CosmSlException($"Unknown function '{name}'.", 0);
            }
        }

        private Operand EmitMul(FunctionCallExpression call)
        {
            if (call.Arguments.Count != 2)
            {
                throw new CosmSlException("mul() takes two arguments.", 0);
            }

            Operand left = EmitExpression(call.Arguments[0]);
            Operand right = EmitExpression(call.Arguments[1]);

            if (left.Kind == OperandKind.Matrix)
            {
                return MultiplyMatrix(left, right);
            }

            (Operand a, Operand b) = AlignOperands(left, right);
            Operand result = NewTemp(a.Components);
            Emit2(OpcodeMul, result, Src(a), Src(b));
            return result;
        }

        private Operand EmitDot(FunctionCallExpression call)
        {
            if (call.Arguments.Count != 2)
            {
                throw new CosmSlException("dot() takes two arguments.", 0);
            }

            (Operand a, Operand b) = AlignOperands(EmitExpression(call.Arguments[0]), EmitExpression(call.Arguments[1]));
            uint opcode = a.Components switch
            {
                2 => OpcodeDp2,
                3 => OpcodeDp3,
                4 => OpcodeDp4,
                _ => throw new CosmSlException("dot() needs vectors of two to four components.", 0),
            };

            Operand result = NewTemp(1);
            Emit2(opcode, result, Src(a), Src(b));
            return result;
        }

        private Operand EmitTex2D(FunctionCallExpression call)
        {
            if (call.Arguments.Count != 2)
            {
                throw new CosmSlException("tex2D() takes a sampler and a coordinate.", 0);
            }

            Operand sampler = EmitExpression(call.Arguments[0]);
            if (sampler.Kind != OperandKind.Sampler)
            {
                throw new CosmSlException("tex2D() needs a sampler2D uniform as its first argument.", 0);
            }

            Operand coordinate = EmitExpression(call.Arguments[1]);
            Operand result = NewTemp(4);

            _instructions.Add(new TgsiInst
            {
                Opcode = OpcodeTex,
                IsTexture = true,
                TextureTarget = Texture2D,
                Dst = result,
                Sources = [Src(coordinate), Src(sampler)],
            });

            return result;
        }

        private Operand EmitUnary(FunctionCallExpression call, uint opcode, bool saturate = false)
        {
            if (call.Arguments.Count != 1)
            {
                throw new CosmSlException($"{call.FunctionName}() takes one argument.", 0);
            }

            Operand value = EmitExpression(call.Arguments[0]);
            Operand result = NewTemp(value.Components);
            Emit1(opcode, result, Src(value), saturate);
            return result;
        }

        private Operand EmitBinaryIntrinsic(FunctionCallExpression call, uint opcode)
        {
            if (call.Arguments.Count != 2)
            {
                throw new CosmSlException($"{call.FunctionName}() takes two arguments.", 0);
            }

            (Operand a, Operand b) = AlignOperands(EmitExpression(call.Arguments[0]), EmitExpression(call.Arguments[1]));
            Operand result = NewTemp(a.Components);
            Emit2(opcode, result, Src(a), Src(b));
            return result;
        }

        private Operand EmitClamp(FunctionCallExpression call)
        {
            if (call.Arguments.Count != 3)
            {
                throw new CosmSlException("clamp() takes three arguments.", 0);
            }

            Operand value = EmitExpression(call.Arguments[0]);
            Operand low = EmitExpression(call.Arguments[1]);
            Operand high = EmitExpression(call.Arguments[2]);
            int components = Math.Max(value.Components, low.Components);
            (Operand a, Operand b) = AlignOperands(value, low);
            (Operand _, Operand c) = AlignOperands(value, high);

            Operand raised = NewTemp(components);
            Emit2(OpcodeMax, raised, Src(a), Src(b));

            Operand result = NewTemp(components);
            Emit2(OpcodeMin, result, Src(raised), Src(c));
            return result;
        }

        private Operand EmitLerp(FunctionCallExpression call)
        {
            if (call.Arguments.Count != 3)
            {
                throw new CosmSlException("lerp() takes three arguments.", 0);
            }

            Operand factor = EmitExpression(call.Arguments[0]);
            Operand first = EmitExpression(call.Arguments[1]);
            Operand second = EmitExpression(call.Arguments[2]);
            (Operand a, Operand b) = AlignOperands(first, second);
            (Operand _, Operand f) = AlignOperands(first, factor);

            Operand result = NewTemp(a.Components);
            Emit3(OpcodeLrp, result, Src(f), Src(a), Src(b));
            return result;
        }

        private Operand ConstructVector(int components, List<ExpressionNode> arguments)
        {
            if (arguments.Count == 0)
            {
                throw new CosmSlException("A vector constructor needs arguments.", 0);
            }

            // A single scalar splats across the vector.
            if (arguments.Count == 1)
            {
                Operand single = EmitExpression(arguments[0]);
                if (single.Components == 1)
                {
                    Operand splat = NewTemp(components);
                    Emit1(OpcodeMov, splat, Src(single));
                    return splat;
                }

                if (single.Components == components)
                {
                    return single;
                }
            }

            Operand result = NewTemp(components);
            int cursor = 0;
            foreach (ExpressionNode argument in arguments)
            {
                Operand value = EmitExpression(argument);
                for (int i = 0; i < value.Components; i++)
                {
                    if (cursor >= components)
                    {
                        throw new CosmSlException("Too many components in vector constructor.", 0);
                    }

                    Operand channel = result with
                    {
                        WriteMask = 1u << cursor,
                        Components = 1,
                        Swizzle = "x",
                    };
                    Emit1(OpcodeMov, channel, Src(Component(value, i)));
                    cursor++;
                }
            }

            if (cursor != components)
            {
                throw new CosmSlException("Not enough components in vector constructor.", 0);
            }

            return result;
        }

        // --- Helpers ---

        /// <summary>Moves a value into a target register slot, replicating
        /// scalars and honoring an existing write mask on the target.</summary>
        private void Copy(Operand target, Operand value)
        {
            uint mask = target.WriteMask != 0 ? target.WriteMask : MaskFromComponents(target.Components);
            int components = ComponentCountOfMask(mask);

            if (value.Components == 1 && components > 1)
            {
                value = value with { Swizzle = ScalarReplicate(components), Components = components };
            }
            else if (value.Components != components)
            {
                throw new CosmSlException("Assignment component counts do not match.", 0);
            }

            Emit1(OpcodeMov, target with { WriteMask = mask }, Src(value));
        }

        private static (Operand, Operand) AlignOperands(Operand left, Operand right)
        {
            if (left.Components == right.Components)
            {
                return (left, right);
            }

            if (right.Components == 1)
            {
                return (left, right with { Swizzle = ScalarReplicate(left.Components), Components = left.Components });
            }

            if (left.Components == 1)
            {
                return (left with { Swizzle = ScalarReplicate(right.Components), Components = right.Components }, right);
            }

            throw new CosmSlException("Operand component counts do not match.", 0);
        }

        private static string ScalarReplicate(int components) => components switch
        {
            1 => "x",
            2 => "xx",
            3 => "xxx",
            _ => "xxxx",
        };

        private static uint MaskFromComponents(int components) => components switch
        {
            1 => 0x1,
            2 => 0x3,
            3 => 0x7,
            _ => 0xF,
        };

        private static int ComponentCountOfMask(uint mask) => mask switch
        {
            0x1 => 1,
            0x3 => 2,
            0x7 => 3,
            0xF => 4,
            _ => throw new CosmSlException("Write masks must cover whole components in order.", 0),
        };

        private static Operand Component(Operand operand, int index)
        {
            char channel = operand.Swizzle[index];
            return operand with
            {
                Swizzle = channel.ToString(),
                Components = 1,
            };
        }

        private Operand SwizzleOperand(Operand operand, string member)
        {
            int[] mapped = MapSwizzle(member);
            char[] baseChars = operand.Swizzle.ToCharArray();
            char[] result = new char[mapped.Length];
            for (int i = 0; i < mapped.Length; i++)
            {
                result[i] = baseChars[mapped[i]];
            }

            return operand with
            {
                Swizzle = new string(result),
                Components = mapped.Length,
            };
        }

        private Operand SwizzleWritable(Operand operand, string member)
        {
            int[] mapped = MapSwizzle(member);

            uint mask = 0;
            char[] baseChars = operand.Swizzle.ToCharArray();
            char[] swizzle = new char[mapped.Length];
            for (int i = 0; i < mapped.Length; i++)
            {
                mask |= 1u << mapped[i];
                swizzle[i] = baseChars[mapped[i]];
            }

            return operand with
            {
                Swizzle = new string(swizzle),
                Components = mapped.Length,
                WriteMask = mask,
            };
        }

        /// <summary>Maps swizzle letters (xyzw or rgba) to component indices.</summary>
        private static int[] MapSwizzle(string member)
        {
            const string xyzw = "xyzw";
            const string rgba = "rgba";
            int[] result = new int[member.Length];

            for (int i = 0; i < member.Length; i++)
            {
                int x = xyzw.IndexOf(member[i]);
                if (x >= 0)
                {
                    result[i] = x;
                    continue;
                }

                int r = rgba.IndexOf(member[i]);
                if (r >= 0)
                {
                    result[i] = r;
                    continue;
                }

                throw new CosmSlException($"Unknown swizzle character '{member[i]}'.", 0);
            }

            return result;
        }

        private Operand ImmediateOperand(float value)
        {
            if (!_immediateLookup.TryGetValue(value, out int index))
            {
                index = _immediates.Count;
                _immediates.Add([value, 0f, 0f, 1f]);
                _immediateLookup[value] = index;
            }

            return new Operand
            {
                Kind = OperandKind.Vector,
                File = FileImmediate,
                Index = index,
                DimIndex = -1,
                Swizzle = "x",
                Components = 1,
            };
        }

        private Operand NewTemp(int components)
        {
            return new Operand
            {
                Kind = OperandKind.Vector,
                File = FileTemporary,
                Index = _tempCount++,
                DimIndex = -1,
                Swizzle = SwizzleFull(components),
                Components = components,
                WriteMask = MaskFromComponents(components),
            };
        }

        private static string SwizzleFull(int components) => components switch
        {
            1 => "x",
            2 => "xy",
            3 => "xyz",
            _ => "xyzw",
        };

        private static Operand OutputOperand(int slot, int components)
        {
            return new Operand
            {
                Kind = OperandKind.Vector,
                File = FileOutput,
                Index = slot,
                DimIndex = -1,
                Swizzle = SwizzleFull(components),
                Components = components,
                WriteMask = MaskFromComponents(components),
            };
        }

        private VarInfo Lookup(string name)
        {
            if (_variables.TryGetValue(name, out VarInfo? variable))
            {
                return variable;
            }

            if (_uniforms.TryGetValue(name, out UniformInfo? uniform))
            {
                return uniform.AsVariable();
            }

            throw new CosmSlException($"Unknown variable '{name}'.", 0);
        }

        private VarInfo DeclareLocal(string type, string name)
        {
            if (type == "sampler2D" || type == "mat4")
            {
                throw new CosmSlException($"'{type}' locals are not supported.", 0);
            }

            VarInfo variable;
            if (type == "float" || type == "vec2" || type == "vec3" || type == "vec4")
            {
                variable = VarInfo.VectorLocal(type);
                variable.AddTempField(string.Empty, type, _tempCount++);
            }
            else
            {
                StructDeclNode? structure = FindStruct(type)
                    ?? throw new CosmSlException($"Unknown type '{type}'.", 0);

                variable = VarInfo.StructLocal(type);
                foreach (FieldNode field in structure.Fields)
                {
                    variable.AddTempField(field.Name, field.Type, _tempCount++);
                }
            }

            _variables[name] = variable;
            return variable;
        }

        private StructDeclNode? FindStruct(string name)
        {
            foreach (StructDeclNode structure in _ast.Structs)
            {
                if (structure.Name == name)
                {
                    return structure;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves a source-level semantic (POSITION, COLOR, TEXCOORD0,
        /// SV_TARGET, SV_POSITION) to the TGSI semantic name and index.
        /// </summary>
        private static (uint semantic, int index) ResolveSemantic(string semantic)
        {
            string name = semantic;
            int index = 0;

            int digitStart = name.Length;
            while (digitStart > 0 && name[digitStart - 1] >= '0' && name[digitStart - 1] <= '9')
            {
                digitStart--;
            }

            if (digitStart < name.Length)
            {
                index = int.Parse(name[digitStart..], CultureInfo.InvariantCulture);
                name = name[..digitStart];
            }

            return name switch
            {
                "POSITION" or "SV_POSITION" => (SemanticPosition, 0),
                "COLOR" or "SV_TARGET" => (SemanticColor, index),
                "TEXCOORD" => (SemanticTexCoord, index),
                _ => throw new CosmSlException($"Unsupported semantic '{semantic}'.", 0),
            };
        }

        // --- Instruction emission ---

        private void Emit1(uint opcode, Operand dst, Operand src, bool saturate = false)
        {
            _instructions.Add(new TgsiInst
            {
                Opcode = opcode,
                Saturate = saturate,
                Dst = dst,
                Sources = [src],
            });
        }

        private void Emit2(uint opcode, Operand dst, Operand first, Operand second)
        {
            _instructions.Add(new TgsiInst
            {
                Opcode = opcode,
                Dst = dst,
                Sources = [first, second],
            });
        }

        private void Emit3(uint opcode, Operand dst, Operand first, Operand second, Operand third)
        {
            _instructions.Add(new TgsiInst
            {
                Opcode = opcode,
                Dst = dst,
                Sources = [first, second, third],
            });
        }

        /// <summary>Strips the write mask from a value used as a source.</summary>
        private static Operand Src(Operand operand)
        {
            return operand with { WriteMask = 0 };
        }

        // --- Serialization ---

        private string SerializeText()
        {
            var text = new global::System.Text.StringBuilder();
            text.AppendLine(_stage == ShaderStage.Vertex ? "VERT" : "FRAG");

            foreach (TgsiDecl decl in _decls)
            {
                text.Append("DCL ");
                text.Append(FileName(decl.File));
                text.Append('[');
                if (decl.HasDimension)
                {
                    text.Append(decl.Index2D);
                    text.Append("][");
                }
                text.Append(decl.First);
                if (decl.First != decl.Last)
                {
                    text.Append("..");
                    text.Append(decl.Last);
                }
                text.Append(']');

                if (decl.Mask != MaskXyzw)
                {
                    text.Append('.');
                    AppendMask(text, decl.Mask);
                }

                if (decl.HasSemantic)
                {
                    text.Append(", ");
                    text.Append(SemanticName(decl.Semantic));
                    if (decl.SemanticIndex != 0)
                    {
                        text.Append('[');
                        text.Append(decl.SemanticIndex);
                        text.Append(']');
                    }
                }

                text.AppendLine();
            }

            foreach (float[] values in _immediates)
            {
                text.Append("IMM FLT32 { ");
                for (int c = 0; c < 4; c++)
                {
                    if (c > 0)
                    {
                        text.Append(", ");
                    }
                    text.Append(values[c].ToString("0.0######", CultureInfo.InvariantCulture));
                }
                text.AppendLine(" }");
            }

            for (int i = 0; i < _instructions.Count; i++)
            {
                TgsiInst instruction = _instructions[i];
                text.Append(i);
                text.Append(": ");
                text.Append(OpcodeName(instruction));

                // END and friends carry no operands at all.
                if (instruction.Dst.File != 0)
                {
                    text.Append(' ');
                    AppendDst(text, instruction.Dst);
                }

                foreach (Operand source in instruction.Sources)
                {
                    text.Append(", ");
                    AppendSource(text, source);
                }
                if (instruction.IsTexture)
                {
                    text.Append(", 2D");
                }
                text.AppendLine();
            }

            return text.ToString();
        }

        private static string OpcodeName(TgsiInst instruction)
        {
            string name = instruction.Opcode switch
            {
                OpcodeMov => "MOV",
                OpcodeAdd => "ADD",
                OpcodeSub => "SUB",
                OpcodeMul => "MUL",
                OpcodeDiv => "DIV",
                OpcodeDp2 => "DP2",
                OpcodeDp3 => "DP3",
                OpcodeDp4 => "DP4",
                OpcodeMin => "MIN",
                OpcodeMax => "MAX",
                OpcodeLrp => "LRP",
                OpcodeFrc => "FRC",
                OpcodeFlr => "FLR",
                OpcodeRound => "ROUND",
                OpcodeEx2 => "EX2",
                OpcodeLg2 => "LG2",
                OpcodeAbs => "ABS",
                OpcodeSeq => "SEQ",
                OpcodeSgt => "SGT",
                OpcodeSle => "SLE",
                OpcodeSne => "SNE",
                OpcodeSlt => "SLT",
                OpcodeSge => "SGE",
                OpcodeSqrt => "SQRT",
                OpcodeRsq => "RSQ",
                OpcodeSin => "SIN",
                OpcodeCos => "COS",
                OpcodeTex => "TEX",
                OpcodeEnd => "END",
                _ => "NOP",
            };

            return instruction.Saturate ? $"{name}_SAT" : name;
        }

        private static void AppendDst(global::System.Text.StringBuilder text, Operand dst)
        {
            text.Append(FileName(dst.File));
            text.Append('[');
            text.Append(dst.Index);
            text.Append(']');

            if (dst.WriteMask != 0 && dst.WriteMask != MaskXyzw)
            {
                text.Append('.');
                AppendMask(text, dst.WriteMask);
            }
        }

        private static void AppendSource(global::System.Text.StringBuilder text, Operand source)
        {
            text.Append(FileName(source.File));
            text.Append('[');
            if (source.DimIndex >= 0)
            {
                text.Append(source.DimIndex);
                text.Append("][");
            }
            text.Append(source.Index);
            text.Append(']');

            // Sampler operands carry no swizzle: the host's translator appends
            // it verbatim to the GLSL sampler name, which then fails to compile.
            if (source.File == FileSampler || string.IsNullOrEmpty(source.Swizzle) || source.Swizzle == "xyzw")
            {
                return;
            }

            text.Append('.');
            // Source swizzles are always four components on the wire;
            // the host's parser rejects shorter forms.
            char[] swizzleChars = source.Swizzle.ToCharArray();
            for (int c = 0; c < 4; c++)
            {
                text.Append(swizzleChars[Math.Min(c, swizzleChars.Length - 1)]);
            }
        }

        private static void AppendMask(global::System.Text.StringBuilder text, uint mask)
        {
            if ((mask & 0x1) != 0)
            {
                text.Append('x');
            }
            if ((mask & 0x2) != 0)
            {
                text.Append('y');
            }
            if ((mask & 0x4) != 0)
            {
                text.Append('z');
            }
            if ((mask & 0x8) != 0)
            {
                text.Append('w');
            }
        }

        private static string FileName(uint file) => file switch
        {
            FileConstant => "CONST",
            FileInput => "IN",
            FileOutput => "OUT",
            FileTemporary => "TEMP",
            FileSampler => "SAMP",
            FileImmediate => "IMM",
            _ => "NULL",
        };

        private static string SemanticName(uint semantic) => semantic switch
        {
            SemanticPosition => "POSITION",
            SemanticColor => "COLOR",
            SemanticTexCoord => "TEXCOORD",
            _ => "GENERIC",
        };

        private uint[] SerializeTokens()
        {
            var tokens = new List<uint>
            {
                // Header token: 2 header dwords (header + processor), the
                // body size is patched once the body is known.
                2u,
                0u,
            };

            foreach (TgsiDecl decl in _decls)
            {
                int words = 2 + (decl.HasDimension ? 1 : 0) + (decl.HasSemantic ? 1 : 0);
                uint word = TokenDeclaration
                    | ((uint)words << 4)
                    | (decl.File << 12)
                    | (decl.Mask << 16);
                if (decl.HasDimension)
                {
                    word |= 1u << 20;
                }
                if (decl.HasSemantic)
                {
                    word |= 1u << 21;
                }
                tokens.Add(word);
                tokens.Add((uint)decl.First | ((uint)decl.Last << 16));
                if (decl.HasDimension)
                {
                    tokens.Add((uint)decl.Index2D);
                }
                if (decl.HasSemantic)
                {
                    tokens.Add(decl.Semantic | ((uint)decl.SemanticIndex << 8));
                }
            }

            foreach (float[] values in _immediates)
            {
                // IMM: NrTokens counts the header plus the four data dwords.
                tokens.Add(TokenImmediate | (5u << 4) | (ImmediateFloat32 << 18));
                foreach (float value in values)
                {
                    tokens.Add((uint)BitConverter.SingleToInt32Bits(value));
                }
            }

            foreach (TgsiInst instruction in _instructions)
            {
                bool hasDst = instruction.Dst.File != 0;
                int words = 1 + (hasDst ? 1 : 0) + instruction.Sources.Count + (instruction.IsTexture ? 1 : 0);
                uint word = TokenInstruction
                    | ((uint)words << 4)
                    | (instruction.Opcode << 13)
                    | (instruction.Saturate ? 1u << 21 : 0)
                    | ((hasDst ? 1u : 0u) << 22)
                    | ((uint)instruction.Sources.Count << 24);
                if (instruction.IsTexture)
                {
                    word |= 1u << 29;
                }
                tokens.Add(word);

                if (instruction.IsTexture)
                {
                    tokens.Add(instruction.TextureTarget);
                }

                if (hasDst)
                {
                    Operand dst = instruction.Dst;
                    tokens.Add(dst.File | (dst.WriteMask << 4) | ((uint)dst.Index << 10));
                }

                foreach (Operand source in instruction.Sources)
                {
                    uint srcWord = source.File;
                    if (source.DimIndex >= 0)
                    {
                        srcWord |= 1u << 5;
                    }

                    char[] swizzleChars = string.IsNullOrEmpty(source.Swizzle) ? ['x', 'y', 'z', 'w'] : source.Swizzle.ToCharArray();
                    for (int c = 0; c < 4; c++)
                    {
                        int channel = "xyzw".IndexOf(swizzleChars[Math.Min(c, swizzleChars.Length - 1)]);
                        srcWord |= (uint)channel << (22 + c * 2);
                    }

                    srcWord |= (uint)source.Index << 6;
                    tokens.Add(srcWord);

                    if (source.DimIndex >= 0)
                    {
                        tokens.Add((uint)source.DimIndex << 16);
                    }
                }
            }

            // Patch the header: the processor dword names the stage and the
            // body size covers everything past the two header dwords.
            tokens[0] = 2u | ((uint)(tokens.Count - 2) << 8);
            tokens[1] = _stage == ShaderStage.Vertex ? 0u : 1u;

            return [.. tokens];
        }
    }

    // --- Intermediate representation ---

    private enum OperandKind
    {
        Vector,
        Matrix,
        Sampler,
        Struct,
    }

    /// <summary>One register operand: a file, an index, a swizzle and a write mask.</summary>
    private struct Operand
    {
        public OperandKind Kind;
        public uint File;
        public int Index;
        public int DimIndex;
        public string Swizzle;
        public int Components;
        public uint WriteMask;
    }

    private sealed class TgsiDecl
    {
        public uint File;
        public int First;
        public int Last;
        public uint Mask = MaskXyzw;
        public bool HasSemantic;
        public uint Semantic;
        public int SemanticIndex;
        public bool HasDimension;
        public int Index2D;

        public static TgsiDecl Simple(uint file, int index) => new()
        {
            File = file,
            First = index,
            Last = index,
        };

        public static TgsiDecl Range(uint file, int first, int last) => new()
        {
            File = file,
            First = first,
            Last = last,
        };

        public static TgsiDecl WithSemantic(uint file, int index, uint semantic, int semanticIndex) => new()
        {
            File = file,
            First = index,
            Last = index,
            HasSemantic = true,
            Semantic = semantic,
            SemanticIndex = semanticIndex,
        };

        public static TgsiDecl TwoDimensional(uint file, int dim, int first, int last) => new()
        {
            File = file,
            First = first,
            Last = last,
            HasDimension = true,
            Index2D = dim,
        };
    }

    private sealed class TgsiInst
    {
        public uint Opcode;
        public bool Saturate;
        public bool IsTexture;
        public uint TextureTarget;
        public Operand Dst;
        public List<Operand> Sources = [];
    }

    /// <summary>
    /// A local, a parameter or a uniform. Struct variables carry one register
    /// per field: parameters read their fields from the input file, locals
    /// from temporaries, uniforms from the constant file (or samplers).
    /// </summary>
    private sealed class VarInfo
    {
        private readonly List<FieldRecord> _fields = [];

        private struct FieldRecord
        {
            public string Name;
            public int Components;
            public int Register;
        }

        public bool IsStruct;
        public bool IsInput;
        public bool IsLocal;
        public bool IsSampler;
        public string Type = string.Empty;
        public int Register = -1;

        public int FieldCount => _fields.Count;
        public List<(string name, int components, int register)> Fields =>
            _fields.ConvertAll(entry => (entry.Name, entry.Components, entry.Register));

        public static VarInfo StructParameter(string type) => new()
        {
            IsStruct = true,
            IsInput = true,
            Type = type,
        };

        public static VarInfo StructLocal(string type) => new()
        {
            IsStruct = true,
            IsLocal = true,
            Type = type,
        };

        public static VarInfo VectorLocal(string type) => new()
        {
            IsLocal = true,
            Type = type,
        };

        /// <summary>Adds a field reading from the input file (parameters).</summary>
        public void AddField(string name, string type)
        {
            _fields.Add(new FieldRecord
            {
                Name = name,
                Components = ComponentCount(type),
                Register = _fields.Count,
            });
        }

        /// <summary>Adds a field backed by an allocated temporary (locals).</summary>
        public void AddTempField(string name, string type, int temp)
        {
            _fields.Add(new FieldRecord
            {
                Name = name,
                Components = ComponentCount(type),
                Register = temp,
            });
        }

        public Operand? Field(string name)
        {
            foreach (FieldRecord field in _fields)
            {
                if (field.Name == name)
                {
                    return FieldOperand(field);
                }
            }

            return null;
        }

        public Operand FieldByIndex(int index)
        {
            return FieldOperand(_fields[index]);
        }

        /// <summary>The register of a plain (non-struct) variable.</summary>
        public Operand Single()
        {
            if (IsStruct)
            {
                return new Operand { Kind = OperandKind.Struct, DimIndex = -1, Components = _fields.Count };
            }

            if (IsSampler)
            {
                return new Operand
                {
                    Kind = OperandKind.Sampler,
                    File = FileSampler,
                    Index = Register,
                    DimIndex = -1,
                    Swizzle = "x",
                    Components = 1,
                };
            }

            if (IsLocal)
            {
                return FieldOperand(_fields[0]);
            }

            return new Operand
            {
                Kind = KindFor(Type),
                File = FileConstant,
                Index = Register,
                DimIndex = 0,
                Swizzle = SwizzleFor(Type),
                Components = ComponentCount(Type),
            };
        }

        private Operand FieldOperand(FieldRecord field)
        {
            return new Operand
            {
                Kind = OperandKind.Vector,
                File = IsInput ? FileInput : FileTemporary,
                Index = field.Register,
                DimIndex = -1,
                Swizzle = field.Components switch { 1 => "x", 2 => "xy", 3 => "xyz", _ => "xyzw" },
                Components = field.Components,
            };
        }

        private static OperandKind KindFor(string type) => type switch
        {
            "mat4" => OperandKind.Matrix,
            _ => OperandKind.Vector,
        };

        private static int ComponentCount(string type) => type switch
        {
            "float" => 1,
            "vec2" => 2,
            "vec3" => 3,
            "mat4" => 4,
            _ => 4,
        };

        private static string SwizzleFor(string type) => type switch
        {
            "float" => "x",
            "vec2" => "xy",
            "vec3" => "xyz",
            _ => "xyzw",
        };
    }

    private sealed class UniformInfo
    {
        public bool IsSampler;
        public int Index;
        public string Type = string.Empty;

        public static UniformInfo Constant(int slot, string type) => new()
        {
            Index = slot,
            Type = type,
        };

        public static UniformInfo Sampler(int samplerIndex) => new()
        {
            IsSampler = true,
            Index = samplerIndex,
        };

        public VarInfo AsVariable()
        {
            return new VarInfo
            {
                IsSampler = IsSampler,
                Type = IsSampler ? "sampler2D" : Type,
                Register = Index,
            };
        }
    }
}
