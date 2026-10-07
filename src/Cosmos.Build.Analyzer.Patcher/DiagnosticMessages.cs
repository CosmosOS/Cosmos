// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Cosmos.Build.Analyzer.Patcher;

public sealed class DiagnosticMessages
{
    public static readonly DiagnosticDescriptor MemberNeedsPlug = new(
        "NAOT0002",
        "Member Needs Plug",
        "Member '{0}' in class '{1}' requires a plug",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "Ensure that the member has a corresponding plug. See https://cosmosos.github.io/articles/dev/plugs.html for more information."
    );

    public static readonly DiagnosticDescriptor MemberCanNotBeUsed = new(
        "NAOT0003",
        "Member Can Not Be Used",
        "Member '{0}' can not be used because the current architecture ({1}) does not match the target architecture '{2}' of plug '{3}'",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "Ensure that the member can be used in the current environment."
    );

    public static readonly DiagnosticDescriptor PlugNameDoesNotMatch = new(
        "NAOT0004",
        "Plug Name Does Not Match",
        "Plug '{0}' should be renamed to '{1}'",
        "Naming",
        DiagnosticSeverity.Info,
        true,
        "Ensure that the plug name matches the plugged class name."
    );

    public static readonly DiagnosticDescriptor MethodNotImplemented = new(
        "NAOT0005",
        "Method Not Implemented",
        "Method '{0}' does not exist in '{1}'",
        "Usage",
        DiagnosticSeverity.Info,
        true,
        "Ensure that the method name is correct and that the method exists."
    );


    public static readonly DiagnosticDescriptor StaticConstructorTooManyParams = new(
        "NAOT0006",
        "Static Constructor Has Too Many Parameters",
        "The static constructor '{0}' contains too many parameters. A static constructor must not have more than one parameter.",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "A static constructor should have at most one parameter."
    );

    public static readonly DiagnosticDescriptor LayerViolation = new(
        "NAOT0007",
        "Layer Violation",
        "Assembly '{0}' is in layer '{1}' and cannot be referenced from a '{2}' layer project",
        "Architecture",
        DiagnosticSeverity.Warning,
        true,
        "Cosmos kernel layers must only reference the layer immediately below them. " +
        "Layer order (lowest to highest): Native, Core, HAL, System, User.",
        customTags: WellKnownDiagnosticTags.CompilationEnd
    );

    public static readonly DiagnosticDescriptor DriverAssemblyUsesUnsafeAccessor = new(
        "NAOT0008",
        "Driver assembly uses UnsafeAccessor",
        "Member '{0}' carries [{1}]; a driver assembly reaches the kernel through the public driver kit seam only",
        "Architecture",
        DiagnosticSeverity.Error,
        true,
        "A driver assembly (<CosmosDriverAssembly>true</CosmosDriverAssembly>) must use only what a third-party " +
        "driver library can use. UnsafeAccessor and UnsafeAccessorType reach internals without a grant, so they " +
        "are refused there. Add the missing capability to the driver kit instead."
    );

    public static readonly DiagnosticDescriptor DriverAssemblyGrantedInternals = new(
        "NAOT0009",
        "Driver assembly is granted internals",
        "Assembly '{0}' grants InternalsVisibleTo to this driver assembly; a driver assembly must not be granted internals",
        "Architecture",
        DiagnosticSeverity.Error,
        true,
        "A driver assembly (<CosmosDriverAssembly>true</CosmosDriverAssembly>) is proof that a driver needs " +
        "nothing beyond the public driver kit seam, and the compiler is that proof only while no Cosmos assembly " +
        "grants it internals. Remove the InternalsVisibleTo and add the missing capability to the driver kit.",
        customTags: WellKnownDiagnosticTags.CompilationEnd
    );

    public static ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        MemberNeedsPlug, MemberCanNotBeUsed, PlugNameDoesNotMatch, MethodNotImplemented, StaticConstructorTooManyParams,
        LayerViolation, DriverAssemblyUsesUnsafeAccessor, DriverAssemblyGrantedInternals);
}
