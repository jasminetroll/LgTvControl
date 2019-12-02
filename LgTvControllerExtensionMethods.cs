using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static System.IO.Path;
using static System.String;
using System.Collections.Generic;

namespace LagrangianDesign.LgTvControl.LgTvControllerExtensions {
    public static class LgTvControllerExtensionMethods {
        public static IMvcBuilder AddLgTvControllerApplicationPart(this IMvcBuilder builder) {
            if (builder is null) {
                throw new ArgumentNullException(nameof(builder));
            }
            builder.AddApplicationPart(CreateLgTvControllerAssembly());
            return builder;
        }

        static Assembly CreateLgTvControllerAssembly() => CreateAssembly(
            "LagrangianDesign.LgTvControl.Controllers",
            LgTvControllerSyntax,
            typeof(Microsoft.AspNetCore.Mvc.ControllerBase),
            typeof(Microsoft.Extensions.Logging.ILogger));

        static String LgTvControllerCs {
            get {
                using (var r = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("LagrangianDesign.LgTvControl.LgTvController.cs"))) {
                    return r.ReadToEnd();
                }
            }
        }

        static CompilationUnitSyntax LgTvControllerSyntax {
            get {
                var root = (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(LgTvControllerCs).GetRoot();
                var theClass = root.Members.OfType<NamespaceDeclarationSyntax>().Single().Members.OfType<ClassDeclarationSyntax>().Single();
                return root.ReplaceNode(
                    theClass,
                    theClass.AddMembers(
                        GetPropertySyntax(
                            typeof(LgTvSerialPort)
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .OrderBy(x => x.Name))
                                .ToArray()));
            }
        }

        static Assembly CreateAssembly(String baseName, CompilationUnitSyntax syntax, params Type[] referencedTypes) {
            var compilation = CSharpCompilation.Create(
                $"{baseName}_{Rng.Next():X8}",
                new[] { SyntaxTree(syntax) },
                referencedTypes
                    .Select(t => t.Assembly.Location)
                    .Concat(((String)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(PathSeparator))
                    .Distinct()
                    .Select(l => MetadataReference.CreateFromFile(l)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var ms = new MemoryStream()) {
                var result = compilation.Emit(ms);
                if (!result.Success) {
                    throw new InvalidProgramException(Join('\n',
                        result.Diagnostics.Select(x => x.ToString())));
                }
                return Assembly.Load(ms.ToArray());
            }
        }

        static readonly Random Rng = new Random();

        static IEnumerable<MemberDeclarationSyntax> GetPropertySyntax(IEnumerable<PropertyInfo> properties) {
            foreach (var property in properties) {
                var typeName = property.PropertyType.Name;
                var propertyName = property.Name;
                if (property.CanRead) {
                    yield return MethodDeclaration(
                            IdentifierName(typeName),
                            Identifier($"Get{propertyName}"))
                        .WithAttributeLists(
                            SingletonList<AttributeListSyntax>(
                                AttributeList(
                                    SingletonSeparatedList<AttributeSyntax>(
                                        Attribute(
                                            IdentifierName("HttpGet"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList<AttributeArgumentSyntax>(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(propertyName))))))))))
                        .WithModifiers(
                            TokenList(
                                Token(SyntaxKind.PublicKeyword)))
                        .WithExpressionBody(
                            ArrowExpressionClause(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("LgTvSerialPort"),
                                    IdentifierName(propertyName))))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken));
                }
                if (property.CanWrite) {
                    yield return MethodDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.VoidKeyword)),
                        Identifier($"Set{propertyName}"))
                    .WithAttributeLists(
                        SingletonList<AttributeListSyntax>(
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("HttpPut"))
                                    .WithArgumentList(
                                        AttributeArgumentList(
                                            SingletonSeparatedList<AttributeArgumentSyntax>(
                                                AttributeArgument(
                                                    LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        Literal(propertyName))))))))))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList<ParameterSyntax>(
                                Parameter(
                                    Identifier("value"))
                                .WithAttributeLists(
                                    SingletonList<AttributeListSyntax>(
                                        AttributeList(
                                            SingletonSeparatedList<AttributeSyntax>(
                                                Attribute(
                                                    IdentifierName("FromBodyAttribute"))))))
                                .WithType(
                                    IdentifierName(typeName)))))
                    .WithBody(
                        Block(
                            SingletonList<StatementSyntax>(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("LgTvSerialPort"),
                                            IdentifierName(propertyName)),
                                        IdentifierName("value"))))));
                }
            }
        }
    }
}
