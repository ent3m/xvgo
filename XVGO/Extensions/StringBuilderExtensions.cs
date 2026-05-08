using System.Text;

namespace XVGO.Extensions;

public static class StringBuilderExtensions
{
    public static StringBuilder TrimEnd(this StringBuilder sb)
    {
        while (sb.Length > 0 && char.IsWhiteSpace(sb[^1]))
        {
            sb.Length--;
        }
        return sb;
    }
}