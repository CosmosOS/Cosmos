// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Patcher;
using Cosmos.Patcher.Patching;
using Cosmos.Patcher.Resolution;
using Cosmos.Tests.NativeWrapper;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Cosmos.Tests.Patcher;

/// <summary>
/// A patched assembly that holds a <c>const</c> or a default parameter typed
/// as an enum declared in another assembly can only be written back when that
/// assembly resolves: Mono.Cecil looks the enum up to find its underlying
/// type. The build keeps every referenced assembly in directories Cecil's
/// default resolver never searches, so the patcher supplies its own.
/// </summary>
[Collection("PatcherTests")]
public sealed class PlugPatcherTest_CrossAssemblyConstants : IDisposable
{
    private const string EnumAssemblyName = "CrossAssemblyEnumLib";
    private const string ConsumerAssemblyName = "CrossAssemblyConsumer";
    private const string Namespace = "CrossAssembly";
    private const string EnumTypeName = "Kind";
    private const string HolderTypeName = "Holder";
    private const string ConstantFieldName = "DefaultKind";
    private const string MethodName = "Use";
    private const int ConstantValue = 1;

    private readonly string _directory;
    private readonly string _consumerPath;

    public PlugPatcherTest_CrossAssemblyConstants()
    {
        _directory = Path.Combine(Path.GetTempPath(), "cosmos-patcher-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        _consumerPath = WriteAssemblies(_directory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Write_WithoutResolverForTheEnumAssembly_CannotWriteBackTheConstants()
    {
        using AssemblyDefinition consumer = AssemblyDefinition.ReadAssembly(_consumerPath);
        using MemoryStream output = new();

        Exception exception = Assert.ThrowsAny<Exception>(() => consumer.Write(output));

        Assert.True(exception is AssemblyResolutionException or ResolutionException,
            $"expected an unresolved assembly, got {exception.GetType().Name}: {exception.Message}");
    }

    [Fact]
    public void Write_WithTheAssemblyDirectoryResolved_KeepsTheConstants()
    {
        PatcherAssemblyResolver resolver = new();
        resolver.AddAssemblyDirectory(_consumerPath);

        using AssemblyDefinition consumer = PatcherAssemblyLoader.ReadTarget(_consumerPath, resolver, out bool hasSymbols);
        Assert.False(hasSymbols);

        // The same pass the command runs: a plug assembly whose plugs target
        // nothing in the consumer leaves it unchanged, and the write is what
        // this test is about.
        PlugPatcher patcher = new(new PlugScanner());
        using AssemblyDefinition plugAssembly = PatcherAssemblyLoader.Read(typeof(TestClassPlug).Assembly.Location, resolver);
        patcher.PatchAssembly(consumer, plugAssembly);

        using MemoryStream output = new();
        consumer.Write(output);
        output.Position = 0;

        using AssemblyDefinition written = AssemblyDefinition.ReadAssembly(output);
        TypeDefinition holder = written.MainModule.GetType(Namespace, HolderTypeName);
        FieldDefinition constant = holder.Fields.Single(field => field.Name == ConstantFieldName);
        ParameterDefinition parameter = holder.Methods.Single(method => method.Name == MethodName).Parameters[0];

        Assert.True(constant.HasConstant);
        Assert.Equal(ConstantValue, constant.Constant);
        Assert.Equal($"{Namespace}.{EnumTypeName}", constant.FieldType.FullName);
        Assert.True(parameter.HasConstant);
        Assert.Equal(ConstantValue, parameter.Constant);
        Assert.Equal($"{Namespace}.{EnumTypeName}", parameter.ParameterType.FullName);
    }

    [Fact]
    public void Resolver_AddsEachDirectoryOnce_AndSkipsMissingOnes()
    {
        PatcherAssemblyResolver resolver = new();
        resolver.AddAssemblyDirectory(_consumerPath);
        resolver.AddDirectory(_directory);
        resolver.AddDirectories($"{_directory};{Path.Combine(_directory, "missing")},{_directory}");
        resolver.AddDirectory(null);
        resolver.AddAssemblyDirectory(string.Empty);

        Assert.Single(resolver.Directories);
        Assert.Contains(Path.GetFullPath(_directory).TrimEnd(Path.DirectorySeparatorChar), resolver.Directories);
    }

    /// <summary>
    /// Writes two assemblies to <paramref name="directory"/>: one declaring an
    /// enum, and one holding a <c>const</c> field and a default parameter of
    /// that enum type, the way a driver declares a constant of a kit enum.
    /// </summary>
    /// <returns>The path of the consuming assembly.</returns>
    private static string WriteAssemblies(string directory)
    {
        string enumPath = Path.Combine(directory, $"{EnumAssemblyName}.dll");
        string consumerPath = Path.Combine(directory, $"{ConsumerAssemblyName}.dll");

        using (AssemblyDefinition enumAssembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(EnumAssemblyName, new Version(1, 0, 0, 0)), EnumAssemblyName, ModuleKind.Dll))
        {
            ModuleDefinition module = enumAssembly.MainModule;
            TypeDefinition kind = new(Namespace, EnumTypeName,
                TypeAttributes.Public | TypeAttributes.Sealed, module.ImportReference(typeof(Enum)));
            kind.Fields.Add(new FieldDefinition("value__",
                FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName, module.TypeSystem.Int32));
            kind.Fields.Add(EnumMember("None", 0, kind));
            kind.Fields.Add(EnumMember("Some", ConstantValue, kind));
            module.Types.Add(kind);
            enumAssembly.Write(enumPath);
        }

        // The consumer's own write needs the enum too, so it is built with a
        // resolver that knows the directory the enum was just written to.
        PatcherAssemblyResolver resolver = new();
        resolver.AddDirectory(directory);
        ModuleParameters parameters = new() { Kind = ModuleKind.Dll, AssemblyResolver = resolver };
        using (AssemblyDefinition consumer = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(ConsumerAssemblyName, new Version(1, 0, 0, 0)), ConsumerAssemblyName, parameters))
        {
            ModuleDefinition module = consumer.MainModule;
            using AssemblyDefinition enumAssembly = AssemblyDefinition.ReadAssembly(enumPath);
            TypeReference kind = module.ImportReference(enumAssembly.MainModule.GetType(Namespace, EnumTypeName));

            TypeDefinition holder = new(Namespace, HolderTypeName,
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class,
                module.TypeSystem.Object);
            holder.Fields.Add(new FieldDefinition(ConstantFieldName,
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault, kind)
            {
                Constant = ConstantValue
            });

            MethodDefinition use = new(MethodName,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.Void);
            use.Parameters.Add(new ParameterDefinition("kind", ParameterAttributes.Optional | ParameterAttributes.HasDefault, kind)
            {
                Constant = ConstantValue
            });
            use.Body.GetILProcessor().Emit(OpCodes.Ret);
            holder.Methods.Add(use);

            module.Types.Add(holder);
            consumer.Write(consumerPath);
        }

        return consumerPath;
    }

    private static FieldDefinition EnumMember(string name, int value, TypeDefinition enumType) =>
        new(name, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault, enumType)
        {
            Constant = value
        };
}
