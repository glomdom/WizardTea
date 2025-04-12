using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Serilog;
using WizardTea.Generator.Metadata;

namespace WizardTea.Generator;

/// <summary>
/// Adds read functionality to objects.
/// </summary>
public class Rewriter : CSharpSyntaxRewriter {
    private readonly SemanticModel _semanticModel;

    public Rewriter(SemanticModel model) {
        _semanticModel = model;
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node) {
        if (node.Identifier.ToString() != "Header") return base.VisitConstructorDeclaration(node);

        var body = node.Body;
        if (body is null) return base.VisitConstructorDeclaration(node);

        var properties = GetPropertiesFromParent(node.Parent);
        foreach (var prop in properties) {
            var id = prop.Identifier;
            var type = _semanticModel.GetTypeInfo(prop.Type).Type;
            if (type is null) throw new Exception($"failed to determine type for {id}");

            var isBuiltin = IsBuiltin(type);
            var isArray = IsArray(type); // array types are not counted as builtin
            var isEnum = IsEnum(type);

            var metadataComment = GetTrailingComment(prop);
            var metadata = FieldMetadata.FromString(metadataComment);

            if (!string.IsNullOrEmpty(metadataComment)) Log.Verbose("found metadata comment, applying changes {metadata}", metadata);

            var readerFunction = isBuiltin ? $"reader.{GetReadMethodForBuiltin(type)}()" : $"new {type}(reader)";
            if (isEnum) readerFunction = $"reader.ReadEnum<{type}>()";
            if (isArray) {
                if (metadata.SizeIdentifier is null) throw new Exception("tried to read array without size identifier metadata");
                
                var innerType = GetInnerArrayType(type);
                var isInnerEnum = IsEnum(innerType);

                if (IsBuiltin(innerType)) {
                    readerFunction = $"reader.ReadArray<{innerType}>({metadata.SizeIdentifier}, reader.{GetReadMethodForBuiltin(innerType)})";
                } else {
                    if (isInnerEnum) { } else {
                        readerFunction = $"reader.ReadArray({metadata.SizeIdentifier}, () => new {innerType}(reader))";
                    }
                }
            }

            if (string.IsNullOrEmpty(readerFunction)) {
                Log.Warning("skipping {id} as no reader function was created", id);

                continue;
            }

            var injectedStatement = SyntaxFactory.ParseStatement($"{id} = {readerFunction};");
            body = body.AddStatements(injectedStatement);

            Log.Verbose("created reader for {id}", id);
        }

        return node.WithBody(body);
    }

    private static IEnumerable<PropertyDeclarationSyntax> GetPropertiesFromParent(SyntaxNode? parent) {
        return parent switch {
            ClassDeclarationSyntax @class => @class.Members.OfType<PropertyDeclarationSyntax>(),
            StructDeclarationSyntax @struct => @struct.Members.OfType<PropertyDeclarationSyntax>(),

            _ => []
        };
    }

    private static string GetReadMethodForBuiltin(ITypeSymbol type) {
        return type.SpecialType switch {
            SpecialType.System_Boolean => "ReadBoolean",
            SpecialType.System_Byte => "ReadByte",
            SpecialType.System_SByte => "ReadSByte",
            SpecialType.System_Int16 => "ReadInt16",
            SpecialType.System_UInt16 => "ReadUInt16",
            SpecialType.System_Int32 => "ReadInt32",
            SpecialType.System_UInt32 => "ReadUInt32",
            SpecialType.System_Int64 => "ReadInt64",
            SpecialType.System_UInt64 => "ReadUInt64",

            _ => string.Empty,
        };
    }

    private static bool IsBuiltin(ITypeSymbol type) {
        return type.SpecialType switch {
            SpecialType.System_Boolean or
                SpecialType.System_Byte or
                SpecialType.System_SByte or
                SpecialType.System_Int16 or
                SpecialType.System_UInt16 or
                SpecialType.System_Int32 or
                SpecialType.System_UInt32 or
                SpecialType.System_Int64 or
                SpecialType.System_UInt64 or
                SpecialType.System_Single or
                SpecialType.System_Double or
                SpecialType.System_Char or
                SpecialType.System_String or
                SpecialType.System_Object => true,

            _ => false
        };
    }

    private static bool IsArray(ITypeSymbol type) {
        return type is IArrayTypeSymbol;
    }

    private static bool IsEnum(ITypeSymbol type) {
        return type is INamedTypeSymbol { TypeKind: TypeKind.Enum };
    }

    private static ITypeSymbol GetInnerArrayType(ITypeSymbol type) {
        if (!IsArray(type)) return type;

        var arr = type as IArrayTypeSymbol;

        return arr!.ElementType;
    }

    private static string GetTrailingComment(PropertyDeclarationSyntax prop) {
        var closeBrace = prop.AccessorList!.CloseBraceToken;

        var commentTrivia = closeBrace.TrailingTrivia
            .FirstOrDefault(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia));

        return commentTrivia.ToFullString().TrimStart('/').Trim();
    }
}