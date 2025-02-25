namespace CSharpier.DocTypes;

internal sealed class StringDoc(string value, bool isDirective = false) : Doc
{
    public string Value { get; } = value;
    public bool IsDirective { get; } = isDirective;

    public static StringDoc Create(string value)
    {
        if (value.Length != 1)
        {
            return new StringDoc(value);
        }

        return value switch
        {
            " " => SpaceString,
            "\t" => TabString,
            "," => CommaString,
            "=" => EqualsString,
            "." => DotString,
            "{" => OpenBraceString,
            "}" => ClosedBraceString,
            "(" => OpenBracketString,
            ")" => ClosedBracketString,
            ";" => SemiColonString,

            _ => new StringDoc(value),
        };
    }

    private static readonly StringDoc SpaceString = new(" ");
    private static readonly StringDoc TabString = new("\t");
    private static readonly StringDoc CommaString = new(",");
    private static readonly StringDoc EqualsString = new("=");
    private static readonly StringDoc DotString = new(".");
    private static readonly StringDoc OpenBraceString = new("{");
    private static readonly StringDoc ClosedBraceString = new("}");
    private static readonly StringDoc OpenBracketString = new("(");
    private static readonly StringDoc ClosedBracketString = new(")");
    private static readonly StringDoc SemiColonString = new(";");

    public static StringDoc GetStringDoc(SyntaxToken token)
    {
        var kind = token.Kind();

        switch (kind)
        {
            case SyntaxKind.TildeToken:
                return TildeTokenStringDoc;
            case SyntaxKind.ExclamationToken:
                return ExclamationTokenStringDoc;
            case SyntaxKind.DollarToken:
                return DollarTokenStringDoc;
            case SyntaxKind.PercentToken:
                return PercentTokenStringDoc;
            case SyntaxKind.CaretToken:
                return CaretTokenStringDoc;
            case SyntaxKind.AmpersandToken:
                return AmpersandTokenStringDoc;
            case SyntaxKind.AsteriskToken:
                return AsteriskTokenStringDoc;
            case SyntaxKind.OpenParenToken:
                return OpenParenTokenStringDoc;
            case SyntaxKind.CloseParenToken:
                return CloseParenTokenStringDoc;
            case SyntaxKind.MinusToken:
                return MinusTokenStringDoc;
            case SyntaxKind.PlusToken:
                return PlusTokenStringDoc;
            case SyntaxKind.EqualsToken:
                return EqualsTokenStringDoc;
            case SyntaxKind.OpenBraceToken:
                return OpenBraceTokenStringDoc;
            case SyntaxKind.CloseBraceToken:
                return CloseBraceTokenStringDoc;
            case SyntaxKind.OpenBracketToken:
                return OpenBracketTokenStringDoc;
            case SyntaxKind.CloseBracketToken:
                return CloseBracketTokenStringDoc;
            case SyntaxKind.BarToken:
                return BarTokenStringDoc;
            case SyntaxKind.BackslashToken:
                return BackslashTokenStringDoc;
            case SyntaxKind.ColonToken:
                return ColonTokenStringDoc;
            case SyntaxKind.SemicolonToken:
                return SemicolonTokenStringDoc;
            case SyntaxKind.DoubleQuoteToken:
                return DoubleQuoteTokenStringDoc;
            case SyntaxKind.SingleQuoteToken:
                return SingleQuoteTokenStringDoc;
            case SyntaxKind.LessThanToken:
                return LessThanTokenStringDoc;
            case SyntaxKind.CommaToken:
                return CommaTokenStringDoc;
            case SyntaxKind.GreaterThanToken:
                return GreaterThanTokenStringDoc;
            case SyntaxKind.DotToken:
                return DotTokenStringDoc;
            case SyntaxKind.QuestionToken:
                return QuestionTokenStringDoc;
            case SyntaxKind.HashToken:
                return HashTokenStringDoc;
            case SyntaxKind.SlashToken:
                return SlashTokenStringDoc;
            case SyntaxKind.SlashGreaterThanToken:
                return SlashGreaterThanTokenStringDoc;
            case SyntaxKind.LessThanSlashToken:
                return LessThanSlashTokenStringDoc;
            case SyntaxKind.XmlCommentStartToken:
                return XmlCommentStartTokenStringDoc;
            case SyntaxKind.XmlCommentEndToken:
                return XmlCommentEndTokenStringDoc;
            case SyntaxKind.XmlCDataStartToken:
                return XmlCDataStartTokenStringDoc;
            case SyntaxKind.XmlCDataEndToken:
                return XmlCDataEndTokenStringDoc;
            case SyntaxKind.XmlProcessingInstructionStartToken:
                return XmlProcessingInstructionStartTokenStringDoc;
            case SyntaxKind.XmlProcessingInstructionEndToken:
                return XmlProcessingInstructionEndTokenStringDoc;

            // compound
            case SyntaxKind.BarBarToken:
                return BarBarTokenStringDoc;
            case SyntaxKind.AmpersandAmpersandToken:
                return AmpersandAmpersandTokenStringDoc;
            case SyntaxKind.MinusMinusToken:
                return MinusMinusTokenStringDoc;
            case SyntaxKind.PlusPlusToken:
                return PlusPlusTokenStringDoc;
            case SyntaxKind.ColonColonToken:
                return ColonColonTokenStringDoc;
            case SyntaxKind.QuestionQuestionToken:
                return QuestionQuestionTokenStringDoc;
            case SyntaxKind.MinusGreaterThanToken:
                return MinusGreaterThanTokenStringDoc;
            case SyntaxKind.ExclamationEqualsToken:
                return ExclamationEqualsTokenStringDoc;
            case SyntaxKind.EqualsEqualsToken:
                return EqualsEqualsTokenStringDoc;
            case SyntaxKind.EqualsGreaterThanToken:
                return EqualsGreaterThanTokenStringDoc;
            case SyntaxKind.LessThanEqualsToken:
                return LessThanEqualsTokenStringDoc;
            case SyntaxKind.LessThanLessThanToken:
                return LessThanLessThanTokenStringDoc;
            case SyntaxKind.LessThanLessThanEqualsToken:
                return LessThanLessThanEqualsTokenStringDoc;
            case SyntaxKind.GreaterThanEqualsToken:
                return GreaterThanEqualsTokenStringDoc;
            case SyntaxKind.GreaterThanGreaterThanToken:
                return GreaterThanGreaterThanTokenStringDoc;
            case SyntaxKind.GreaterThanGreaterThanEqualsToken:
                return GreaterThanGreaterThanEqualsTokenStringDoc;
            case SyntaxKind.GreaterThanGreaterThanGreaterThanToken:
                return GreaterThanGreaterThanGreaterThanTokenStringDoc;
            case SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken:
                return GreaterThanGreaterThanGreaterThanEqualsTokenStringDoc;
            case SyntaxKind.SlashEqualsToken:
                return SlashEqualsTokenStringDoc;
            case SyntaxKind.AsteriskEqualsToken:
                return AsteriskEqualsTokenStringDoc;
            case SyntaxKind.BarEqualsToken:
                return BarEqualsTokenStringDoc;
            case SyntaxKind.AmpersandEqualsToken:
                return AmpersandEqualsTokenStringDoc;
            case SyntaxKind.PlusEqualsToken:
                return PlusEqualsTokenStringDoc;
            case SyntaxKind.MinusEqualsToken:
                return MinusEqualsTokenStringDoc;
            case SyntaxKind.CaretEqualsToken:
                return CaretEqualsTokenStringDoc;
            case SyntaxKind.PercentEqualsToken:
                return PercentEqualsTokenStringDoc;
            case SyntaxKind.QuestionQuestionEqualsToken:
                return QuestionQuestionEqualsTokenStringDoc;
            case SyntaxKind.DotDotToken:
                return DotDotTokenStringDoc;

            // Keywords
            case SyntaxKind.BoolKeyword:
                return BoolKeywordStringDoc;
            case SyntaxKind.ByteKeyword:
                return ByteKeywordStringDoc;
            case SyntaxKind.SByteKeyword:
                return SByteKeywordStringDoc;
            case SyntaxKind.ShortKeyword:
                return ShortKeywordStringDoc;
            case SyntaxKind.UShortKeyword:
                return UShortKeywordStringDoc;
            case SyntaxKind.IntKeyword:
                return IntKeywordStringDoc;
            case SyntaxKind.UIntKeyword:
                return UIntKeywordStringDoc;
            case SyntaxKind.LongKeyword:
                return LongKeywordStringDoc;
            case SyntaxKind.ULongKeyword:
                return ULongKeywordStringDoc;
            case SyntaxKind.DoubleKeyword:
                return DoubleKeywordStringDoc;
            case SyntaxKind.FloatKeyword:
                return FloatKeywordStringDoc;
            case SyntaxKind.DecimalKeyword:
                return DecimalKeywordStringDoc;
            case SyntaxKind.StringKeyword:
                return StringKeywordStringDoc;
            case SyntaxKind.CharKeyword:
                return CharKeywordStringDoc;
            case SyntaxKind.VoidKeyword:
                return VoidKeywordStringDoc;
            case SyntaxKind.ObjectKeyword:
                return ObjectKeywordStringDoc;
            case SyntaxKind.TypeOfKeyword:
                return TypeOfKeywordStringDoc;
            case SyntaxKind.SizeOfKeyword:
                return SizeOfKeywordStringDoc;
            case SyntaxKind.NullKeyword:
                return NullKeywordStringDoc;
            case SyntaxKind.TrueKeyword:
                return TrueKeywordStringDoc;
            case SyntaxKind.FalseKeyword:
                return FalseKeywordStringDoc;
            case SyntaxKind.IfKeyword:
                return IfKeywordStringDoc;
            case SyntaxKind.ElseKeyword:
                return ElseKeywordStringDoc;
            case SyntaxKind.WhileKeyword:
                return WhileKeywordStringDoc;
            case SyntaxKind.ForKeyword:
                return ForKeywordStringDoc;
            case SyntaxKind.ForEachKeyword:
                return ForEachKeywordStringDoc;
            case SyntaxKind.DoKeyword:
                return DoKeywordStringDoc;
            case SyntaxKind.SwitchKeyword:
                return SwitchKeywordStringDoc;
            case SyntaxKind.CaseKeyword:
                return CaseKeywordStringDoc;
            case SyntaxKind.DefaultKeyword:
                return DefaultKeywordStringDoc;
            case SyntaxKind.TryKeyword:
                return TryKeywordStringDoc;
            case SyntaxKind.CatchKeyword:
                return CatchKeywordStringDoc;
            case SyntaxKind.FinallyKeyword:
                return FinallyKeywordStringDoc;
            case SyntaxKind.LockKeyword:
                return LockKeywordStringDoc;
            case SyntaxKind.GotoKeyword:
                return GotoKeywordStringDoc;
            case SyntaxKind.BreakKeyword:
                return BreakKeywordStringDoc;
            case SyntaxKind.ContinueKeyword:
                return ContinueKeywordStringDoc;
            case SyntaxKind.ReturnKeyword:
                return ReturnKeywordStringDoc;
            case SyntaxKind.ThrowKeyword:
                return ThrowKeywordStringDoc;
            case SyntaxKind.PublicKeyword:
                return PublicKeywordStringDoc;
            case SyntaxKind.PrivateKeyword:
                return PrivateKeywordStringDoc;
            case SyntaxKind.InternalKeyword:
                return InternalKeywordStringDoc;
            case SyntaxKind.ProtectedKeyword:
                return ProtectedKeywordStringDoc;
            case SyntaxKind.StaticKeyword:
                return StaticKeywordStringDoc;
            case SyntaxKind.ReadOnlyKeyword:
                return ReadOnlyKeywordStringDoc;
            case SyntaxKind.SealedKeyword:
                return SealedKeywordStringDoc;
            case SyntaxKind.ConstKeyword:
                return ConstKeywordStringDoc;
            case SyntaxKind.FixedKeyword:
                return FixedKeywordStringDoc;
            case SyntaxKind.StackAllocKeyword:
                return StackAllocKeywordStringDoc;
            case SyntaxKind.VolatileKeyword:
                return VolatileKeywordStringDoc;
            case SyntaxKind.NewKeyword:
                return NewKeywordStringDoc;
            case SyntaxKind.OverrideKeyword:
                return OverrideKeywordStringDoc;
            case SyntaxKind.AbstractKeyword:
                return AbstractKeywordStringDoc;
            case SyntaxKind.VirtualKeyword:
                return VirtualKeywordStringDoc;
            case SyntaxKind.EventKeyword:
                return EventKeywordStringDoc;
            case SyntaxKind.ExternKeyword:
                return ExternKeywordStringDoc;
            case SyntaxKind.RefKeyword:
                return RefKeywordStringDoc;
            case SyntaxKind.OutKeyword:
                return OutKeywordStringDoc;
            case SyntaxKind.InKeyword:
                return InKeywordStringDoc;
            case SyntaxKind.IsKeyword:
                return IsKeywordStringDoc;
            case SyntaxKind.AsKeyword:
                return AsKeywordStringDoc;
            case SyntaxKind.ParamsKeyword:
                return ParamsKeywordStringDoc;
            case SyntaxKind.ArgListKeyword:
                return ArgListKeywordStringDoc;
            case SyntaxKind.MakeRefKeyword:
                return MakeRefKeywordStringDoc;
            case SyntaxKind.RefTypeKeyword:
                return RefTypeKeywordStringDoc;
            case SyntaxKind.RefValueKeyword:
                return RefValueKeywordStringDoc;
            case SyntaxKind.ThisKeyword:
                return ThisKeywordStringDoc;
            case SyntaxKind.BaseKeyword:
                return BaseKeywordStringDoc;
            case SyntaxKind.NamespaceKeyword:
                return NamespaceKeywordStringDoc;
            case SyntaxKind.UsingKeyword:
                return UsingKeywordStringDoc;
            case SyntaxKind.ClassKeyword:
                return ClassKeywordStringDoc;
            case SyntaxKind.StructKeyword:
                return StructKeywordStringDoc;
            case SyntaxKind.InterfaceKeyword:
                return InterfaceKeywordStringDoc;
            case SyntaxKind.EnumKeyword:
                return EnumKeywordStringDoc;
            case SyntaxKind.DelegateKeyword:
                return DelegateKeywordStringDoc;
            case SyntaxKind.CheckedKeyword:
                return CheckedKeywordStringDoc;
            case SyntaxKind.UncheckedKeyword:
                return UncheckedKeywordStringDoc;
            case SyntaxKind.UnsafeKeyword:
                return UnsafeKeywordStringDoc;
            case SyntaxKind.OperatorKeyword:
                return OperatorKeywordStringDoc;
            case SyntaxKind.ImplicitKeyword:
                return ImplicitKeywordStringDoc;
            case SyntaxKind.ExplicitKeyword:
                return ExplicitKeywordStringDoc;
            case SyntaxKind.ElifKeyword:
                return ElifKeywordStringDoc;
            case SyntaxKind.EndIfKeyword:
                return EndIfKeywordStringDoc;
            case SyntaxKind.RegionKeyword:
                return RegionKeywordStringDoc;
            case SyntaxKind.EndRegionKeyword:
                return EndRegionKeywordStringDoc;
            case SyntaxKind.DefineKeyword:
                return DefineKeywordStringDoc;
            case SyntaxKind.UndefKeyword:
                return UndefKeywordStringDoc;
            case SyntaxKind.WarningKeyword:
                return WarningKeywordStringDoc;
            case SyntaxKind.ErrorKeyword:
                return ErrorKeywordStringDoc;
            case SyntaxKind.LineKeyword:
                return LineKeywordStringDoc;
            case SyntaxKind.PragmaKeyword:
                return PragmaKeywordStringDoc;
            case SyntaxKind.HiddenKeyword:
                return HiddenKeywordStringDoc;
            case SyntaxKind.ChecksumKeyword:
                return ChecksumKeywordStringDoc;
            case SyntaxKind.DisableKeyword:
                return DisableKeywordStringDoc;
            case SyntaxKind.RestoreKeyword:
                return RestoreKeywordStringDoc;
            case SyntaxKind.ReferenceKeyword:
                return ReferenceKeywordStringDoc;
            case SyntaxKind.LoadKeyword:
                return LoadKeywordStringDoc;
            case SyntaxKind.NullableKeyword:
                return NullableKeywordStringDoc;
            case SyntaxKind.EnableKeyword:
                return EnableKeywordStringDoc;
            case SyntaxKind.WarningsKeyword:
                return WarningsKeywordStringDoc;
            case SyntaxKind.AnnotationsKeyword:
                return AnnotationsKeywordStringDoc;

            // contextual keywords
            case SyntaxKind.YieldKeyword:
                return YieldKeywordStringDoc;
            case SyntaxKind.PartialKeyword:
                return PartialKeywordStringDoc;
            case SyntaxKind.FromKeyword:
                return FromKeywordStringDoc;
            case SyntaxKind.GroupKeyword:
                return GroupKeywordStringDoc;
            case SyntaxKind.JoinKeyword:
                return JoinKeywordStringDoc;
            case SyntaxKind.IntoKeyword:
                return IntoKeywordStringDoc;
            case SyntaxKind.LetKeyword:
                return LetKeywordStringDoc;
            case SyntaxKind.ByKeyword:
                return ByKeywordStringDoc;
            case SyntaxKind.WhereKeyword:
                return WhereKeywordStringDoc;
            case SyntaxKind.SelectKeyword:
                return SelectKeywordStringDoc;
            case SyntaxKind.GetKeyword:
                return GetKeywordStringDoc;
            case SyntaxKind.SetKeyword:
                return SetKeywordStringDoc;
            case SyntaxKind.AddKeyword:
                return AddKeywordStringDoc;
            case SyntaxKind.RemoveKeyword:
                return RemoveKeywordStringDoc;
            case SyntaxKind.OrderByKeyword:
                return OrderByKeywordStringDoc;
            case SyntaxKind.AliasKeyword:
                return AliasKeywordStringDoc;
            case SyntaxKind.OnKeyword:
                return OnKeywordStringDoc;
            case SyntaxKind.EqualsKeyword:
                return EqualsKeywordStringDoc;
            case SyntaxKind.AscendingKeyword:
                return AscendingKeywordStringDoc;
            case SyntaxKind.DescendingKeyword:
                return DescendingKeywordStringDoc;
            case SyntaxKind.AssemblyKeyword:
                return AssemblyKeywordStringDoc;
            case SyntaxKind.ModuleKeyword:
                return ModuleKeywordStringDoc;
            case SyntaxKind.TypeKeyword:
                return TypeKeywordStringDoc;
            case SyntaxKind.FieldKeyword:
                return FieldKeywordStringDoc;
            case SyntaxKind.MethodKeyword:
                return MethodKeywordStringDoc;
            case SyntaxKind.ParamKeyword:
                return ParamKeywordStringDoc;
            case SyntaxKind.PropertyKeyword:
                return PropertyKeywordStringDoc;
            case SyntaxKind.TypeVarKeyword:
                return TypeVarKeywordStringDoc;
            case SyntaxKind.GlobalKeyword:
                return GlobalKeywordStringDoc;
            case SyntaxKind.NameOfKeyword:
                return NameOfKeywordStringDoc;
            case SyntaxKind.AsyncKeyword:
                return AsyncKeywordStringDoc;
            case SyntaxKind.AwaitKeyword:
                return AwaitKeywordStringDoc;
            case SyntaxKind.WhenKeyword:
                return WhenKeywordStringDoc;
            case SyntaxKind.InterpolatedStringStartToken:
                return InterpolatedStringStartTokenStringDoc;
            case SyntaxKind.InterpolatedStringEndToken:
                return InterpolatedStringEndTokenStringDoc;
            case SyntaxKind.InterpolatedVerbatimStringStartToken:
                return InterpolatedVerbatimStringStartTokenStringDoc;
            case SyntaxKind.UnderscoreToken:
                return UnderscoreTokenStringDoc;
            case SyntaxKind.VarKeyword:
                return VarKeywordStringDoc;
            case SyntaxKind.AndKeyword:
                return AndKeywordStringDoc;
            case SyntaxKind.OrKeyword:
                return OrKeywordStringDoc;
            case SyntaxKind.NotKeyword:
                return NotKeywordStringDoc;
            case SyntaxKind.WithKeyword:
                return WithKeywordStringDoc;
            case SyntaxKind.InitKeyword:
                return InitKeywordStringDoc;
            case SyntaxKind.RecordKeyword:
                return RecordKeywordStringDoc;
            case SyntaxKind.ManagedKeyword:
                return ManagedKeywordStringDoc;
            case SyntaxKind.UnmanagedKeyword:
                return UnmanagedKeywordStringDoc;
            case SyntaxKind.RequiredKeyword:
                return RequiredKeywordStringDoc;
            case SyntaxKind.ScopedKeyword:
                return ScopedKeywordStringDoc;
            case SyntaxKind.FileKeyword:
                return FileKeywordStringDoc;
            case SyntaxKind.AllowsKeyword:
                return AllowsKeywordStringDoc;
            default:
                return new StringDoc(token.Text);
        }
    }

    private static readonly StringDoc TildeTokenStringDoc = new("~");
    private static readonly StringDoc ExclamationTokenStringDoc = new("!");
    private static readonly StringDoc DollarTokenStringDoc = new("$");
    private static readonly StringDoc PercentTokenStringDoc = new("%");
    private static readonly StringDoc CaretTokenStringDoc = new("^");
    private static readonly StringDoc AmpersandTokenStringDoc = new("&");
    private static readonly StringDoc AsteriskTokenStringDoc = new("*");
    private static readonly StringDoc OpenParenTokenStringDoc = new("(");
    private static readonly StringDoc CloseParenTokenStringDoc = new(")");
    private static readonly StringDoc MinusTokenStringDoc = new("-");
    private static readonly StringDoc PlusTokenStringDoc = new("+");
    private static readonly StringDoc EqualsTokenStringDoc = new("=");
    private static readonly StringDoc OpenBraceTokenStringDoc = new("{");
    private static readonly StringDoc CloseBraceTokenStringDoc = new("}");
    private static readonly StringDoc OpenBracketTokenStringDoc = new("[");
    private static readonly StringDoc CloseBracketTokenStringDoc = new("]");
    private static readonly StringDoc BarTokenStringDoc = new("|");
    private static readonly StringDoc BackslashTokenStringDoc = new("\\");
    private static readonly StringDoc ColonTokenStringDoc = new(":");
    private static readonly StringDoc SemicolonTokenStringDoc = new(";");
    private static readonly StringDoc DoubleQuoteTokenStringDoc = new("\"");
    private static readonly StringDoc SingleQuoteTokenStringDoc = new("'");
    private static readonly StringDoc LessThanTokenStringDoc = new("<");
    private static readonly StringDoc CommaTokenStringDoc = new(",");
    private static readonly StringDoc GreaterThanTokenStringDoc = new(">");
    private static readonly StringDoc DotTokenStringDoc = new(".");
    private static readonly StringDoc QuestionTokenStringDoc = new("?");
    private static readonly StringDoc HashTokenStringDoc = new("#");
    private static readonly StringDoc SlashTokenStringDoc = new("/");
    private static readonly StringDoc SlashGreaterThanTokenStringDoc = new("/>");
    private static readonly StringDoc LessThanSlashTokenStringDoc = new("</");
    private static readonly StringDoc XmlCommentStartTokenStringDoc = new("<!--");
    private static readonly StringDoc XmlCommentEndTokenStringDoc = new("-->");
    private static readonly StringDoc XmlCDataStartTokenStringDoc = new("<![CDATA[");
    private static readonly StringDoc XmlCDataEndTokenStringDoc = new("]]>");
    private static readonly StringDoc XmlProcessingInstructionStartTokenStringDoc = new("<?");
    private static readonly StringDoc XmlProcessingInstructionEndTokenStringDoc = new("?>");

    private static readonly StringDoc BarBarTokenStringDoc = new("||");
    private static readonly StringDoc AmpersandAmpersandTokenStringDoc = new("&&");
    private static readonly StringDoc MinusMinusTokenStringDoc = new("--");
    private static readonly StringDoc PlusPlusTokenStringDoc = new("++");
    private static readonly StringDoc ColonColonTokenStringDoc = new("::");
    private static readonly StringDoc QuestionQuestionTokenStringDoc = new("??");
    private static readonly StringDoc MinusGreaterThanTokenStringDoc = new("->");
    private static readonly StringDoc ExclamationEqualsTokenStringDoc = new("!=");
    private static readonly StringDoc EqualsEqualsTokenStringDoc = new("==");
    private static readonly StringDoc EqualsGreaterThanTokenStringDoc = new("=>");
    private static readonly StringDoc LessThanEqualsTokenStringDoc = new("<=");
    private static readonly StringDoc LessThanLessThanTokenStringDoc = new("<<");
    private static readonly StringDoc LessThanLessThanEqualsTokenStringDoc = new("<<=");
    private static readonly StringDoc GreaterThanEqualsTokenStringDoc = new(">=");
    private static readonly StringDoc GreaterThanGreaterThanTokenStringDoc = new(">>");
    private static readonly StringDoc GreaterThanGreaterThanEqualsTokenStringDoc = new(">>=");
    private static readonly StringDoc GreaterThanGreaterThanGreaterThanTokenStringDoc = new(">>>");
    private static readonly StringDoc GreaterThanGreaterThanGreaterThanEqualsTokenStringDoc = new(
        ">>>="
    );
    private static readonly StringDoc SlashEqualsTokenStringDoc = new("/=");
    private static readonly StringDoc AsteriskEqualsTokenStringDoc = new("*=");
    private static readonly StringDoc BarEqualsTokenStringDoc = new("|=");
    private static readonly StringDoc AmpersandEqualsTokenStringDoc = new("&=");
    private static readonly StringDoc PlusEqualsTokenStringDoc = new("+=");
    private static readonly StringDoc MinusEqualsTokenStringDoc = new("-=");
    private static readonly StringDoc CaretEqualsTokenStringDoc = new("^=");
    private static readonly StringDoc PercentEqualsTokenStringDoc = new("%=");
    private static readonly StringDoc QuestionQuestionEqualsTokenStringDoc = new("??=");
    private static readonly StringDoc DotDotTokenStringDoc = new("..");

    private static readonly StringDoc BoolKeywordStringDoc = new("bool");
    private static readonly StringDoc ByteKeywordStringDoc = new("byte");
    private static readonly StringDoc SByteKeywordStringDoc = new("sbyte");
    private static readonly StringDoc ShortKeywordStringDoc = new("short");
    private static readonly StringDoc UShortKeywordStringDoc = new("ushort");
    private static readonly StringDoc IntKeywordStringDoc = new("int");
    private static readonly StringDoc UIntKeywordStringDoc = new("uint");
    private static readonly StringDoc LongKeywordStringDoc = new("long");
    private static readonly StringDoc ULongKeywordStringDoc = new("ulong");
    private static readonly StringDoc DoubleKeywordStringDoc = new("double");
    private static readonly StringDoc FloatKeywordStringDoc = new("float");
    private static readonly StringDoc DecimalKeywordStringDoc = new("decimal");
    private static readonly StringDoc StringKeywordStringDoc = new("string");
    private static readonly StringDoc CharKeywordStringDoc = new("char");
    private static readonly StringDoc VoidKeywordStringDoc = new("void");
    private static readonly StringDoc ObjectKeywordStringDoc = new("object");
    private static readonly StringDoc TypeOfKeywordStringDoc = new("typeof");
    private static readonly StringDoc SizeOfKeywordStringDoc = new("sizeof");
    private static readonly StringDoc NullKeywordStringDoc = new("null");
    private static readonly StringDoc TrueKeywordStringDoc = new("true");
    private static readonly StringDoc FalseKeywordStringDoc = new("false");
    private static readonly StringDoc IfKeywordStringDoc = new("if");
    private static readonly StringDoc ElseKeywordStringDoc = new("else");
    private static readonly StringDoc WhileKeywordStringDoc = new("while");
    private static readonly StringDoc ForKeywordStringDoc = new("for");
    private static readonly StringDoc ForEachKeywordStringDoc = new("foreach");
    private static readonly StringDoc DoKeywordStringDoc = new("do");
    private static readonly StringDoc SwitchKeywordStringDoc = new("switch");
    private static readonly StringDoc CaseKeywordStringDoc = new("case");
    private static readonly StringDoc DefaultKeywordStringDoc = new("default");
    private static readonly StringDoc TryKeywordStringDoc = new("try");
    private static readonly StringDoc CatchKeywordStringDoc = new("catch");
    private static readonly StringDoc FinallyKeywordStringDoc = new("finally");
    private static readonly StringDoc LockKeywordStringDoc = new("lock");
    private static readonly StringDoc GotoKeywordStringDoc = new("goto");
    private static readonly StringDoc BreakKeywordStringDoc = new("break");
    private static readonly StringDoc ContinueKeywordStringDoc = new("continue");
    private static readonly StringDoc ReturnKeywordStringDoc = new("return");
    private static readonly StringDoc ThrowKeywordStringDoc = new("throw");
    private static readonly StringDoc PublicKeywordStringDoc = new("public");
    private static readonly StringDoc PrivateKeywordStringDoc = new("private");
    private static readonly StringDoc InternalKeywordStringDoc = new("internal");
    private static readonly StringDoc ProtectedKeywordStringDoc = new("protected");
    private static readonly StringDoc StaticKeywordStringDoc = new("static");
    private static readonly StringDoc ReadOnlyKeywordStringDoc = new("readonly");
    private static readonly StringDoc SealedKeywordStringDoc = new("sealed");
    private static readonly StringDoc ConstKeywordStringDoc = new("const");
    private static readonly StringDoc FixedKeywordStringDoc = new("fixed");
    private static readonly StringDoc StackAllocKeywordStringDoc = new("stackalloc");
    private static readonly StringDoc VolatileKeywordStringDoc = new("volatile");
    private static readonly StringDoc NewKeywordStringDoc = new("new");
    private static readonly StringDoc OverrideKeywordStringDoc = new("override");
    private static readonly StringDoc AbstractKeywordStringDoc = new("abstract");
    private static readonly StringDoc VirtualKeywordStringDoc = new("virtual");
    private static readonly StringDoc EventKeywordStringDoc = new("event");
    private static readonly StringDoc ExternKeywordStringDoc = new("extern");
    private static readonly StringDoc RefKeywordStringDoc = new("ref");
    private static readonly StringDoc OutKeywordStringDoc = new("out");
    private static readonly StringDoc InKeywordStringDoc = new("in");
    private static readonly StringDoc IsKeywordStringDoc = new("is");
    private static readonly StringDoc AsKeywordStringDoc = new("as");
    private static readonly StringDoc ParamsKeywordStringDoc = new("params");
    private static readonly StringDoc ArgListKeywordStringDoc = new("__arglist");
    private static readonly StringDoc MakeRefKeywordStringDoc = new("__makeref");
    private static readonly StringDoc RefTypeKeywordStringDoc = new("__reftype");
    private static readonly StringDoc RefValueKeywordStringDoc = new("__refvalue");
    private static readonly StringDoc ThisKeywordStringDoc = new("this");
    private static readonly StringDoc BaseKeywordStringDoc = new("base");
    private static readonly StringDoc NamespaceKeywordStringDoc = new("namespace");
    private static readonly StringDoc UsingKeywordStringDoc = new("using");
    private static readonly StringDoc ClassKeywordStringDoc = new("class");
    private static readonly StringDoc StructKeywordStringDoc = new("struct");
    private static readonly StringDoc InterfaceKeywordStringDoc = new("interface");
    private static readonly StringDoc EnumKeywordStringDoc = new("enum");
    private static readonly StringDoc DelegateKeywordStringDoc = new("delegate");
    private static readonly StringDoc CheckedKeywordStringDoc = new("checked");
    private static readonly StringDoc UncheckedKeywordStringDoc = new("unchecked");
    private static readonly StringDoc UnsafeKeywordStringDoc = new("unsafe");
    private static readonly StringDoc OperatorKeywordStringDoc = new("operator");
    private static readonly StringDoc ImplicitKeywordStringDoc = new("implicit");
    private static readonly StringDoc ExplicitKeywordStringDoc = new("explicit");
    private static readonly StringDoc ElifKeywordStringDoc = new("elif");
    private static readonly StringDoc EndIfKeywordStringDoc = new("endif");
    private static readonly StringDoc RegionKeywordStringDoc = new("region");
    private static readonly StringDoc EndRegionKeywordStringDoc = new("endregion");
    private static readonly StringDoc DefineKeywordStringDoc = new("define");
    private static readonly StringDoc UndefKeywordStringDoc = new("undef");
    private static readonly StringDoc WarningKeywordStringDoc = new("warning");
    private static readonly StringDoc ErrorKeywordStringDoc = new("error");
    private static readonly StringDoc LineKeywordStringDoc = new("line");
    private static readonly StringDoc PragmaKeywordStringDoc = new("pragma");
    private static readonly StringDoc HiddenKeywordStringDoc = new("hidden");
    private static readonly StringDoc ChecksumKeywordStringDoc = new("checksum");
    private static readonly StringDoc DisableKeywordStringDoc = new("disable");
    private static readonly StringDoc RestoreKeywordStringDoc = new("restore");
    private static readonly StringDoc ReferenceKeywordStringDoc = new("r");
    private static readonly StringDoc LoadKeywordStringDoc = new("load");
    private static readonly StringDoc NullableKeywordStringDoc = new("nullable");
    private static readonly StringDoc EnableKeywordStringDoc = new("enable");
    private static readonly StringDoc WarningsKeywordStringDoc = new("warnings");
    private static readonly StringDoc AnnotationsKeywordStringDoc = new("annotations");

    private static readonly StringDoc YieldKeywordStringDoc = new("yield");
    private static readonly StringDoc PartialKeywordStringDoc = new("partial");
    private static readonly StringDoc FromKeywordStringDoc = new("from");
    private static readonly StringDoc GroupKeywordStringDoc = new("group");
    private static readonly StringDoc JoinKeywordStringDoc = new("join");
    private static readonly StringDoc IntoKeywordStringDoc = new("into");
    private static readonly StringDoc LetKeywordStringDoc = new("let");
    private static readonly StringDoc ByKeywordStringDoc = new("by");
    private static readonly StringDoc WhereKeywordStringDoc = new("where");
    private static readonly StringDoc SelectKeywordStringDoc = new("select");
    private static readonly StringDoc GetKeywordStringDoc = new("get");
    private static readonly StringDoc SetKeywordStringDoc = new("set");
    private static readonly StringDoc AddKeywordStringDoc = new("add");
    private static readonly StringDoc RemoveKeywordStringDoc = new("remove");
    private static readonly StringDoc OrderByKeywordStringDoc = new("orderby");
    private static readonly StringDoc AliasKeywordStringDoc = new("alias");
    private static readonly StringDoc OnKeywordStringDoc = new("on");
    private static readonly StringDoc EqualsKeywordStringDoc = new("equals");
    private static readonly StringDoc AscendingKeywordStringDoc = new("ascending");
    private static readonly StringDoc DescendingKeywordStringDoc = new("descending");
    private static readonly StringDoc AssemblyKeywordStringDoc = new("assembly");
    private static readonly StringDoc ModuleKeywordStringDoc = new("module");
    private static readonly StringDoc TypeKeywordStringDoc = new("type");
    private static readonly StringDoc FieldKeywordStringDoc = new("field");
    private static readonly StringDoc MethodKeywordStringDoc = new("method");
    private static readonly StringDoc ParamKeywordStringDoc = new("param");
    private static readonly StringDoc PropertyKeywordStringDoc = new("property");
    private static readonly StringDoc TypeVarKeywordStringDoc = new("typevar");
    private static readonly StringDoc GlobalKeywordStringDoc = new("global");
    private static readonly StringDoc NameOfKeywordStringDoc = new("nameof");
    private static readonly StringDoc AsyncKeywordStringDoc = new("async");
    private static readonly StringDoc AwaitKeywordStringDoc = new("await");
    private static readonly StringDoc WhenKeywordStringDoc = new("when");
    private static readonly StringDoc InterpolatedStringStartTokenStringDoc = new("$\"");
    private static readonly StringDoc InterpolatedStringEndTokenStringDoc = new("\"");
    private static readonly StringDoc InterpolatedVerbatimStringStartTokenStringDoc = new("$@\"");
    private static readonly StringDoc UnderscoreTokenStringDoc = new("_");
    private static readonly StringDoc VarKeywordStringDoc = new("var");
    private static readonly StringDoc AndKeywordStringDoc = new("and");
    private static readonly StringDoc OrKeywordStringDoc = new("or");
    private static readonly StringDoc NotKeywordStringDoc = new("not");
    private static readonly StringDoc WithKeywordStringDoc = new("with");
    private static readonly StringDoc InitKeywordStringDoc = new("init");
    private static readonly StringDoc RecordKeywordStringDoc = new("record");
    private static readonly StringDoc ManagedKeywordStringDoc = new("managed");
    private static readonly StringDoc UnmanagedKeywordStringDoc = new("unmanaged");
    private static readonly StringDoc RequiredKeywordStringDoc = new("required");
    private static readonly StringDoc ScopedKeywordStringDoc = new("scoped");
    private static readonly StringDoc FileKeywordStringDoc = new("file");
    private static readonly StringDoc AllowsKeywordStringDoc = new("allows");
}
