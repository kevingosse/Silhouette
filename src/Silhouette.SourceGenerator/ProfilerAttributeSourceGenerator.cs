using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Silhouette.SourceGenerator;

[Generator]
public class ProfilerAttributeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var profilerProvider = context.SyntaxProvider.ForAttributeWithMetadataName("Silhouette.ProfilerAttribute", static (_, _) => true, Transform);

        context.RegisterSourceOutput(profilerProvider, static (ctx, state) =>
        {
            var (profilerGuid, className, diagnostic) = state;

            if (diagnostic != null)
            {
                ctx.ReportDiagnostic(diagnostic);
                return;
            }

            string source = $$"""
                              namespace Silhouette._Generated
                              {
                                  using System;
                                  using System.Runtime.InteropServices;

                                  file static class DllMain
                                  {
                                      [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
                                      public static unsafe HResult DllGetClassObject(Guid* rclsid, Guid* riid, nint* ppv)
                                      {
                                          if (*rclsid != new Guid("{{profilerGuid}}"))
                                          {
                                              return HResult.CORPROF_E_PROFILER_CANCEL_ACTIVATION;
                                          }
                              
                                          *ppv = ClassFactory.For(new {{className}}());
                                          return HResult.S_OK;
                                      }
                                  }
                              }
                              """;

            ctx.AddSource("silhouette.dllmain.g.cs", source);
        });
    }

    private static readonly DiagnosticDescriptor MissingGuidArgumentDescriptor = new(
        id: "SILH002",
        title: "MissingGuidArgumentDescriptor",
        messageFormat: "ProfilerAttribute must have a valid GUID argument.",
        category: "Silhouette.SourceGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidGuidDescriptor = new(
        id: "SILH001",
        title: "Invalid Profiler GUID",
        messageFormat: "The ProfilerAttribute argument '{0}' is not a valid GUID.",
        category: "Silhouette.SourceGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static (Guid ProfilerGuid, string? ClassName, Diagnostic? Error) Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        // context.TargetNode is the ClassDeclarationSyntax
        var classDecl = (ClassDeclarationSyntax)context.TargetNode;
        var attributeSyntax = classDecl.AttributeLists
            .SelectMany(a => a.Attributes)
            .FirstOrDefault(a => context.SemanticModel.GetTypeInfo(a, cancellationToken).Type?.ToDisplayString() == "Silhouette.ProfilerAttribute");

        if (attributeSyntax == null || attributeSyntax.ArgumentList == null || attributeSyntax.ArgumentList.Arguments.Count == 0)
        {
            var diagnostic = Diagnostic.Create(
                MissingGuidArgumentDescriptor,
                classDecl.Identifier.GetLocation());
            return (Guid.Empty, null, diagnostic);
        }

        var guidExpr = attributeSyntax.ArgumentList.Arguments[0].Expression;
        string guidString;

        // Try to get constant value using semantic model
        var constantValue = context.SemanticModel.GetConstantValue(guidExpr, cancellationToken);
        if (constantValue.HasValue && constantValue.Value is string s)
        {
            guidString = s;
        }
        else if (guidExpr is LiteralExpressionSyntax literal && literal.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression))
        {
            guidString = literal.Token.ValueText;
        }
        else
        {
            var diagnostic = Diagnostic.Create(
                InvalidGuidDescriptor,
                guidExpr.GetLocation(),
                guidExpr);
            return (Guid.Empty, null, diagnostic);
        }

        if (!Guid.TryParse(guidString, out var guid))
        {
            var diagnostic = Diagnostic.Create(
                InvalidGuidDescriptor,
                guidExpr.GetLocation(),
                guidString);
            return (Guid.Empty, null, diagnostic);
        }

        var fullClassName = context.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return (guid, fullClassName, null);
    }
}
