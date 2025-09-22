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
    private readonly Dictionary<string, List<(string VersionedName, FieldMetadata Metadata)>> _renamed = [];

    public Rewriter(SemanticModel model) {
        _semanticModel = model;
    }

    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) {
        node = (StructDeclarationSyntax)base.VisitStructDeclaration(node)!;

        return AddUnifiedProperties(node);
    }

    // public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node) {
    //     if (node.Identifier.ToString() != "Header") return base.VisitConstructorDeclaration(node);
    //
    //     var body = node.Body;
    //     if (body is null) return base.VisitConstructorDeclaration(node);
    //
    //     var parent = (ClassDeclarationSyntax)node.Parent!;
    //     var properties = parent.Members.OfType<PropertyDeclarationSyntax>();
    //     foreach (var prop in properties) {
    //         var id = prop.Identifier;
    //         var type = _semanticModel.GetTypeInfo(prop.Type).Type;
    //         if (type is null) throw new Exception($"Failed to determine type for {id}.");
    //
    //         var isBuiltin = IsBuiltin(type);
    //         var isArray = IsArray(type); // array types are not counted as builtin
    //         var isEnum = IsEnum(type);
    //
    //         var metadataComment = GetTrailingComment(prop);
    //         var metadata = FieldMetadata.FromString(metadataComment);
    //
    //         if (!string.IsNullOrEmpty(metadataComment)) Log.Verbose("found metadata comment, applying changes {metadata}", metadata);
    //
    //         var versionSince = metadata.VersionSince;
    //         var versionUntil = metadata.VersionUntil;
    //
    //         if (versionUntil is not null || versionSince is not null) {
    //             Log.Verbose("Versioned field {FieldName} found, applying versioned functionality", id.ValueText);
    //         }
    //
    //         var readerFunction = isBuiltin ? $"reader.{GetReadMethodForBuiltin(type)}()" : $"new {type}(reader)";
    //         if (isEnum) readerFunction = $"reader.ReadEnum<{type}>()";
    //         if (isArray) {
    //             if (metadata.SizeIdentifier is null) throw new Exception("tried to read array without size identifier metadata");
    //
    //             var innerType = GetInnerArrayType(type);
    //             var isInnerEnum = IsEnum(innerType);
    //
    //             if (IsBuiltin(innerType)) {
    //                 readerFunction = $"reader.ReadArray<{innerType}>({metadata.SizeIdentifier}, reader.{GetReadMethodForBuiltin(innerType)})";
    //             } else {
    //                 if (isInnerEnum) { } else {
    //                     readerFunction = $"reader.ReadArray({metadata.SizeIdentifier}, () => new {innerType}(reader))";
    //                 }
    //             }
    //         }
    //
    //         if (string.IsNullOrEmpty(readerFunction)) {
    //             Log.Warning("skipping {id} as no reader function was created", id);
    //
    //             continue;
    //         }
    //
    //         var injectedStatement = SyntaxFactory.ParseStatement($"{id} = {readerFunction};");
    //         body = body.AddStatements(injectedStatement);
    //
    //         Log.Verbose("Created reader for {id} in {NodeName}", id, node.Identifier.ValueText);
    //     }
    //
    //     return node.WithBody(body);
    // }

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node) {
        var id = node.Identifier.Text;
        var metadataComment = GetTrailingComment(node);
        var metadata = FieldMetadata.FromString(metadataComment);

        var since = metadata.VersionSince?.Replace(".", "_");
        var until = metadata.VersionUntil?.Replace(".", "_");
        var versionString = (since, until) switch {
            (null, null) => string.Empty,
            (not null, null) => since,
            (null, not null) => until,
            (not null, not null) => $"{since}_{until}",
        };

        if (since is null && until is null && metadata.VersionCondition is not null) {
            if (!_renamed.TryGetValue(id, out var value)) {
                value = [];
                _renamed[id] = value;
            }

            var count = value.Count + 1;
            versionString = $"v{count}";
        }

        if (string.IsNullOrEmpty(versionString)) {
            return base.VisitPropertyDeclaration(node);
        }

        var newModifiers = SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxFactory.Space)
        );

        var newId = SyntaxFactory.Identifier($"{id}_{versionString}");

        Log.Verbose("Renamed {OldNodeName} -> {NewNodeName}", id, newId);

        if (!_renamed.ContainsKey(id)) _renamed[id] = [];
        _renamed[id].Add((newId.Text, metadata));

        return node.WithModifiers(newModifiers).WithIdentifier(newId);
    }

    private T AddUnifiedProperties<T>(T node) where T : TypeDeclarationSyntax {
        var newMembers = new List<MemberDeclarationSyntax>(node.Members);

        foreach (var (baseName, versions) in _renamed) {
            var candidateTypeNames = versions
                .Select(v => node.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .First(p => p.Identifier.Text == v.VersionedName)
                    .Type.ToString())
                .ToList();

            var commonTypeName = PromoteIntegerTypeNames(candidateTypeNames);
            var typeSyntax = SyntaxFactory.ParseTypeName(commonTypeName);

            var getterBody = new List<StatementSyntax>();
            foreach (var (fieldName, meta) in versions) {
                if (meta.VersionCondition is not null) {
                    getterBody.Add(SyntaxFactory.ParseStatement(
                        $"if (VersionHelpers.Matches(\"{meta.VersionCondition}\", _reader)) return {fieldName};"));

                    continue;
                }

                var since = meta.VersionSince is not null ? $"NifVersionValue.V{meta.VersionSince.Replace(".", "_")}" : "null";
                var until = meta.VersionUntil is not null ? $"NifVersionValue.V{meta.VersionUntil.Replace(".", "_")}" : "null";

                getterBody.Add(SyntaxFactory.ParseStatement(
                    $"if (VersionHelpers.Matches(_reader.Version.Value, new VersionRange({since}, {until}))) return {fieldName};"));
            }

            getterBody.Add(SyntaxFactory.ParseStatement(
                $"throw new NotSupportedException($\"Version {{_reader.Version}} not supported for {baseName}\");"));

            var setterBody = new List<StatementSyntax>();
            foreach (var (fieldName, meta) in versions) {
                var fieldTypeName = node.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .First(p => p.Identifier.Text == fieldName)
                    .Type.ToString();

                if (meta.VersionCondition is not null) {
                    setterBody.Add(SyntaxFactory.ParseStatement(
                        $"if (VersionHelpers.Matches(\"{meta.VersionCondition}\", _reader)) {{ {fieldName} = ({fieldTypeName})value; return; }}"));

                    continue;
                }

                var since = meta.VersionSince is not null ? $"NifVersionValue.V{meta.VersionSince.Replace(".", "_")}" : "null";
                var until = meta.VersionUntil is not null ? $"NifVersionValue.V{meta.VersionUntil.Replace(".", "_")}" : "null";

                setterBody.Add(SyntaxFactory.ParseStatement(
                    $"if (VersionHelpers.Matches(_reader.Version.Value, new VersionRange({since}, {until}))) {{ {fieldName} = ({fieldTypeName})value; return; }}"));
            }

            setterBody.Add(SyntaxFactory.ParseStatement(
                $"throw new NotSupportedException($\"Version {{_reader.Version}} not supported for {baseName}\");"));

            // Emit the unified property using *fresh* type syntax
            var property = SyntaxFactory.PropertyDeclaration(typeSyntax, baseName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithBody(SyntaxFactory.Block(getterBody)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithBody(SyntaxFactory.Block(setterBody))
                );

            newMembers.Add(property);
        }

        _renamed.Clear();
        return (T)node.WithMembers(SyntaxFactory.List(newMembers));
    }


    private static string PromoteIntegerTypeNames(IEnumerable<string> typeNames) {
        var rank = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
            { "sbyte", 1 }, { "System.SByte", 1 },
            { "byte", 2 }, { "System.Byte", 2 },
            { "short", 3 }, { "System.Int16", 3 },
            { "ushort", 4 }, { "System.UInt16", 4 },
            { "int", 5 }, { "System.Int32", 5 },
            { "uint", 6 }, { "System.UInt32", 6 },
            { "long", 7 }, { "System.Int64", 7 },
            { "ulong", 8 }, { "System.UInt64", 8 }
        };

        var enumerable = typeNames.ToList();
        var cleaned = enumerable.Select(N).ToList();
        if (!cleaned.All(t => rank.ContainsKey(t))) return enumerable.First();

        var max = cleaned.Max(t => rank[t]);
        return max switch {
            1 => "sbyte",
            2 => "byte",
            3 => "short",
            4 => "ushort",
            5 => "int",
            6 => "uint",
            7 => "long",
            8 => "ulong",
            _ => "int",
        };

        static string N(string s) => s.Replace(" ", "");
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
        var trivia = prop.GetTrailingTrivia()
            .FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia));

        return trivia.ToFullString().TrimStart('/').Trim();
    }

}