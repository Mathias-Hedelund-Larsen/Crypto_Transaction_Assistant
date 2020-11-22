using System.Linq;

public static class StringExtensions 
{
    public static string RemoveWhiteSpaces(this string source)
    {
        return new string(source.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    public static string RemoveQuotationMarks(this string source)
    {
        return new string(source.Where(c => c != '\"').ToArray());
    }
}
