using System.Text.RegularExpressions;

namespace RecipeLearning;

public static class Extensions
{
    private static readonly Regex removeDollar = new("\\$"), isFracComb = new("(\\d+)\\s+(\\d)/(\\d)"), isFrac = new("^(\\d)/(\\d)$");
    public static decimal? ParseNumber(this string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var newToken = removeDollar.Replace(token, " ");
        if (decimal.TryParse(newToken, out decimal result))
            return Math.Round(result, 2);

        var frac = isFrac.Match(newToken);
        if (frac.Success)
            return Math.Round(decimal.Parse(frac.Groups[1].Value) / decimal.Parse(frac.Groups[2].Value), 2);

        var fracComb = isFracComb.Match(newToken);
        if (fracComb.Success)
            return Math.Round(int.Parse(fracComb.Groups[1].Value) + (decimal.Parse(fracComb.Groups[2].Value) / decimal.Parse(fracComb.Groups[3].Value)), 2);

        return null;
    }
}
