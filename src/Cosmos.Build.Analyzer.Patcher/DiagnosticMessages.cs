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
        "Layer order (lowest to highest): Native, Core, HAL, System, User."
    );

    public static ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        MemberNeedsPlug, MemberCanNotBeUsed, PlugNameDoesNotMatch, MethodNotImplemented, StaticConstructorTooManyParams,
        LayerViolation);
}
