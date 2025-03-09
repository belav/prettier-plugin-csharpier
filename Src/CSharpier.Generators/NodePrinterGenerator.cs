using Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.Generators;

[Generator]
public class NodePrinterGenerator : TemplatedGenerator
{
    protected override string SourceName => "Node";

    private static Dictionary<string, string[]> SpecialCase = new()
    {
        {
            nameof(AssignmentExpressionSyntax),
            new[]
            {
                nameof(SyntaxKind.SimpleAssignmentExpression),
                nameof(SyntaxKind.AddAssignmentExpression),
                nameof(SyntaxKind.SubtractAssignmentExpression),
                nameof(SyntaxKind.MultiplyAssignmentExpression),
                nameof(SyntaxKind.DivideAssignmentExpression),
                nameof(SyntaxKind.ModuloAssignmentExpression),
                nameof(SyntaxKind.AndAssignmentExpression),
                nameof(SyntaxKind.ExclusiveOrAssignmentExpression),
                nameof(SyntaxKind.OrAssignmentExpression),
                nameof(SyntaxKind.LeftShiftAssignmentExpression),
                nameof(SyntaxKind.RightShiftAssignmentExpression),
                nameof(SyntaxKind.UnsignedRightShiftAssignmentExpression),
                nameof(SyntaxKind.CoalesceAssignmentExpression),
            }
        },
        {
            nameof(BaseExpressionColonSyntax),
            new[] { nameof(SyntaxKind.ExpressionColon), nameof(SyntaxKind.NameColon) }
        },
        {
            nameof(BaseFieldDeclarationSyntax),
            new[] { nameof(SyntaxKind.FieldDeclaration), nameof(SyntaxKind.EventFieldDeclaration) }
        },
        {
            nameof(BaseMethodDeclarationSyntax),
            new[]
            {
                nameof(SyntaxKind.MethodDeclaration),
                nameof(SyntaxKind.OperatorDeclaration),
                nameof(SyntaxKind.ConversionOperatorDeclaration),
                nameof(SyntaxKind.ConstructorDeclaration),
                nameof(SyntaxKind.DestructorDeclaration),
            }
        },
        {
            nameof(BasePropertyDeclarationSyntax),
            new[]
            {
                nameof(SyntaxKind.PropertyDeclaration),
                nameof(SyntaxKind.EventDeclaration),
                nameof(SyntaxKind.IndexerDeclaration),
            }
        },
        {
            nameof(BaseTypeDeclarationSyntax),
            new[]
            {
                nameof(SyntaxKind.ClassDeclaration),
                nameof(SyntaxKind.StructDeclaration),
                nameof(SyntaxKind.InterfaceDeclaration),
                nameof(SyntaxKind.RecordDeclaration),
                nameof(SyntaxKind.RecordStructDeclaration),
                nameof(SyntaxKind.EnumDeclaration),
            }
        },
        {
            nameof(BinaryExpressionSyntax),
            new[]
            {
                nameof(SyntaxKind.AddExpression),
                nameof(SyntaxKind.SubtractExpression),
                nameof(SyntaxKind.MultiplyExpression),
                nameof(SyntaxKind.DivideExpression),
                nameof(SyntaxKind.ModuloExpression),
                nameof(SyntaxKind.LeftShiftExpression),
                nameof(SyntaxKind.RightShiftExpression),
                nameof(SyntaxKind.UnsignedRightShiftExpression),
                nameof(SyntaxKind.LogicalOrExpression),
                nameof(SyntaxKind.LogicalAndExpression),
                nameof(SyntaxKind.BitwiseOrExpression),
                nameof(SyntaxKind.BitwiseAndExpression),
                nameof(SyntaxKind.ExclusiveOrExpression),
                nameof(SyntaxKind.EqualsExpression),
                nameof(SyntaxKind.NotEqualsExpression),
                nameof(SyntaxKind.LessThanExpression),
                nameof(SyntaxKind.LessThanOrEqualExpression),
                nameof(SyntaxKind.GreaterThanExpression),
                nameof(SyntaxKind.GreaterThanOrEqualExpression),
                nameof(SyntaxKind.IsExpression),
                nameof(SyntaxKind.AsExpression),
                nameof(SyntaxKind.CoalesceExpression),
            }
        },
        {
            nameof(BinaryPatternSyntax),
            new[] { nameof(SyntaxKind.OrPattern), nameof(SyntaxKind.AndPattern) }
        },
        {
            nameof(CheckedExpressionSyntax),
            new[] { nameof(SyntaxKind.CheckedExpression), nameof(SyntaxKind.UncheckedExpression) }
        },
        {
            nameof(CheckedStatementSyntax),
            new[] { nameof(SyntaxKind.CheckedStatement), nameof(SyntaxKind.UncheckedStatement) }
        },
        {
            nameof(ClassOrStructConstraintSyntax),
            new[] { nameof(SyntaxKind.ClassConstraint), nameof(SyntaxKind.StructConstraint) }
        },
        {
            nameof(CommonForEachStatementSyntax),
            new[]
            {
                nameof(SyntaxKind.ForEachStatement),
                nameof(SyntaxKind.ForEachVariableStatement),
            }
        },
        {
            nameof(GotoStatementSyntax),
            new[]
            {
                nameof(SyntaxKind.GotoStatement),
                nameof(SyntaxKind.GotoCaseStatement),
                nameof(SyntaxKind.GotoDefaultStatement),
            }
        },
        {
            nameof(InitializerExpressionSyntax),
            new[]
            {
                nameof(SyntaxKind.ObjectInitializerExpression),
                nameof(SyntaxKind.CollectionInitializerExpression),
                nameof(SyntaxKind.ArrayInitializerExpression),
                nameof(SyntaxKind.ComplexElementInitializerExpression),
                nameof(SyntaxKind.WithInitializerExpression),
            }
        },
        {
            nameof(LiteralExpressionSyntax),
            new[]
            {
                nameof(SyntaxKind.ArgListExpression),
                nameof(SyntaxKind.NumericLiteralExpression),
                nameof(SyntaxKind.StringLiteralExpression),
                nameof(SyntaxKind.Utf8StringLiteralExpression),
                nameof(SyntaxKind.CharacterLiteralExpression),
                nameof(SyntaxKind.TrueLiteralExpression),
                nameof(SyntaxKind.FalseLiteralExpression),
                nameof(SyntaxKind.NullLiteralExpression),
                nameof(SyntaxKind.DefaultLiteralExpression),
            }
        },
        {
            nameof(MemberAccessExpressionSyntax),
            new[]
            {
                nameof(SyntaxKind.SimpleMemberAccessExpression),
                nameof(SyntaxKind.PointerMemberAccessExpression),
            }
        },
        {
            nameof(PostfixUnaryExpressionSyntax),
            new[]
            {
                nameof(SyntaxKind.PostIncrementExpression),
                nameof(SyntaxKind.PostDecrementExpression),
                nameof(SyntaxKind.SuppressNullableWarningExpression),
            }
        },
        {
            nameof(PrefixUnaryExpressionSyntax),
            new[]
            {
                nameof(SyntaxKind.UnaryPlusExpression),
                nameof(SyntaxKind.UnaryMinusExpression),
                nameof(SyntaxKind.BitwiseNotExpression),
                nameof(SyntaxKind.LogicalNotExpression),
                nameof(SyntaxKind.PreIncrementExpression),
                nameof(SyntaxKind.PreDecrementExpression),
                nameof(SyntaxKind.AddressOfExpression),
                nameof(SyntaxKind.PointerIndirectionExpression),
                nameof(SyntaxKind.IndexExpression),
            }
        },
        { nameof(UnaryPatternSyntax), new[] { nameof(SyntaxKind.NotPattern) } },
        {
            nameof(YieldStatementSyntax),
            new[]
            {
                nameof(SyntaxKind.YieldReturnStatement),
                nameof(SyntaxKind.YieldBreakStatement),
            }
        },
    };

    protected override object GetModel(GeneratorExecutionContext context)
    {
        var nodeTypes = context
            .Compilation.SyntaxTrees.Where(o => o.FilePath.Contains("SyntaxNodePrinters"))
            .Select(o => Path.GetFileNameWithoutExtension(o.FilePath))
            .Select(fileName => new
            {
                PrinterName = fileName,
                SyntaxNodeName = $"{fileName}Syntax",
                SyntaxKinds = SpecialCase.TryGetValue($"{fileName}Syntax", out var kinds)
                    ? string.Join(" or ", kinds.Select(x => $"SyntaxKind.{x}"))
                    : $"SyntaxKind.{fileName}",
            })
            .OrderBy(o => o.SyntaxNodeName)
            .ToArray();

        var syntaxNodeTypes = string.Join(" or ", nodeTypes.Select(x => x.SyntaxNodeName));

        return new { NodeTypes = nodeTypes, SyntaxNodeTypes = syntaxNodeTypes };
    }
}
