using Humanizer;
using RecipeLearning.DataCollection;
using RecipeLearning.DataCollection.Data;
using RecipeLearning.DataParsing.Data;
using Sylvan.Data;
using System.Text.RegularExpressions;

namespace RecipeLearning.DataParsing;

public static class SnapshotLabeler
{
    public static IEnumerable<SnapshotLabel> Label(IngredientSnapshot value)
    {
        List<(string, HashSet<string>)> tokens = new();
        foreach (var token in Split(value.CleanedInput))
            tokens.Add((token, GetTokenGuesses(token, value).ToHashSet()));

        HashSet<string> oldTags = new();
        foreach (var (token, tags) in tokens)
        {
            string? tag;
            if (tags.Count < 1)
                tag = "OTHER";
            else if (tags.Count == 1)
                tag = tags.First();
            else
                tag = tags.First(tag => !tag.Equals("COMMENT", StringComparison.OrdinalIgnoreCase));
            tag = $"{(oldTags.Contains(tag) ? "I" : "B")}-{tag}";

            yield return new SnapshotLabel()
            {
                Value = token,
                TagId = TagImporter.TagToId[tag],
                IngredientSnapshotId = value.Id,
            };

            oldTags = tags;
        }
    }

    private static readonly Regex split = new("([,()\\s]{1})");
    private static IEnumerable<string> Split(string value)
    {
        return split.Split(value).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim());
    }

    private static IEnumerable<string> GetTokenGuesses(string token, IngredientSnapshot value)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Enumerable.Empty<string>();

        token = token.Singularize();
        var numberToken = token.ParseNumber();

        return GetTokenGuesses(token, numberToken, value.Name, nameof(value.Name).ToUpper())
            .Concat(GetTokenGuesses(token, numberToken, value.Unit, nameof(value.Unit).ToUpper()))
            .Concat(GetTokenGuesses(token, numberToken, value.Quantity, nameof(value.Quantity).ToUpper()))
            .Concat(GetTokenGuesses(token, numberToken, value.Comment, nameof(value.Comment).ToUpper()))
            .Concat(GetTokenGuesses(token, numberToken, value.RangeEnd, nameof(value.RangeEnd).ToUpper()));
    }


    private static IEnumerable<string> GetTokenGuesses(string token, decimal? numberToken, string? labelValue, string label)
    {
        if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(labelValue))
        {
            var numberLabel = labelValue.ParseNumber();

            if (numberLabel.HasValue)
            {
                if (numberToken.HasValue && labelValue.ParseNumber().Equals(numberToken))
                    yield return label;
            }
            else if (!string.IsNullOrWhiteSpace(labelValue))
            {
                foreach (var labelToken in Split(labelValue))
                {
                    if (labelToken.Singularize().Equals(token))
                        yield return label;
                }
            }
        }
    }
}
