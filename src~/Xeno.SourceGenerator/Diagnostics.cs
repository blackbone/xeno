using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

internal static class Diagnostics {
     private static readonly DiagnosticDescriptor NonClassSystemDeclaration = new(
         "XEN000",
         "System is not a class",
         "System must be a class!",
         "Xeno",
         DiagnosticSeverity.Error,
         true,
         "Don't use [System] attribute on structs or records, it can cause unexpected behaviour!");

     private static readonly DiagnosticDescriptor NonPartialClassSystemDeclaration = new(
         "XEN001",
         "System is not partial",
         "System must be partial!",
         "Xeno",
         DiagnosticSeverity.Error,
         true,
         "Add partial modifier to system type under [System] attribute.");

     private static readonly DiagnosticDescriptor NoMethodsDeclared = new(
         "XEN002",
         "No Methods Declared",
         "At least one method with [SystemMethod] must be declared!",
         "Xeno",
         DiagnosticSeverity.Error,
         true,
         "Add methods with with [SystemMethod] attributes.");

     private static readonly DiagnosticDescriptor SystemMethodNotMachSignature = new(
         "XEN003",
         "Invalid method signature",
         "System method must match signature void(ref C1, ref C2, ref C3...) or void(in Entity, ref C1, ref C2, ref C3...) or void(in Uniform, ref C1, ref C2, ref C3...) or void(in Entity, in Uniform, ref C1, ref C2, ref C3...)",
         "Xeno",
         DiagnosticSeverity.Error,
         true,
         "Change method signature to match signature void(ref C1, ref C2, ref C3...) or void(in Entity, ref C1, ref C2, ref C3...) or void(in Uniform, ref C1, ref C2, ref C3...) or void(in Entity, in Uniform, ref C1, ref C2, ref C3...).");

     private static readonly DiagnosticDescriptor SystemMethodNotMachSignature_ReturnType = new(
         "XEN003.1",
         "Invalid method signature",
         "System method must return void",
         "Xeno",
         DiagnosticSeverity.Error,
         true,
         "Change method return type to void.");

     private static readonly DiagnosticDescriptor SystemMethodNotMachSignature_NoParameters = new(
         "XEN003.2",
         "Invalid method signature",
         "System method must return have parameters",
         "Xeno",
         DiagnosticSeverity.Error,
         true,
         "Change method signature to match signature void(ref C1, ref C2, ref C3...) or void(in Entity, ref C1, ref C2, ref C3...) or void(in Uniform, ref C1, ref C2, ref C3...) or void(in Entity, in Uniform, ref C1, ref C2, ref C3...).");

     private static readonly DiagnosticDescriptor StartupShutdownMethodInvalidSignature = new(
         "XEN004",
         "Invalid method signature",
         "Startup and shut down methods must be void with no parameters",
         "Xeno",
         DiagnosticSeverity.Error,
         true,
         "Change method signature to void().");

     // old ones is above

     private const string Xeno = "Xeno";
     public static readonly DiagnosticDescriptor MultipleSameComponentTypes = new(
         "XEN0001",
         "Multiple same component types",
         "Multiple same component types used in the api",
         Xeno,
         DiagnosticSeverity.Error,
         true,
         "Change types or remove arguments."
         );
}
