namespace CSharpier.SyntaxPrinter;

internal static class ConstraintClauses
{
    public static Doc Print(
        IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses,
        PrintingContext context
    )
    {
        var constraintClausesList = constraintClauses.ToArray();

        if (constraintClausesList.Length == 0)
        {
            return Doc.Null;
        }
        var body = Doc.Join(
            Doc.HardLine,
            constraintClausesList.Select(o => TypeParameterConstraintClause.Print(o, context))
        );

        return Doc.Group(Doc.Indent(Doc.HardLine, body));
    }
}
