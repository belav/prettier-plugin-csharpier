using System.Text;

namespace CSharpier.Utilities
{
    public static class StringBuilderExtensions
    {
        public static bool EndsWithNewLineAndWhitespace(this StringBuilder stringBuilder)
        {
            for (var index = 1; index <= stringBuilder.Length; index++)
            {
                var next = stringBuilder[^index];
                if (next == ' ' || next == '\t')
                {
                    continue;
                }
                else if (next == '\n')
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public static int TrimTrailingWhitespace(this StringBuilder stringBuilder)
        {
            if (stringBuilder.Length == 0)
            {
                return 0;
            }

            var trimmed = 0;
            for (; trimmed < stringBuilder.Length; trimmed++)
            {
                if (stringBuilder[^(trimmed + 1)] != ' ' && stringBuilder[^(trimmed + 1)] != '\t')
                {
                    break;
                }
            }

            stringBuilder.Length -= trimmed;
            return trimmed;
        }
    }
}
