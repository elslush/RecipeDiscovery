using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace DatabaseFunctions
{
    //https://nielsberglund.com/2017/07/23/sql-server-2017-sqlclr---whitelisting-assemblies/
    public static class IngredientCleaner
    {
        public static string CleanText(string value)
        {
            value = RemoveUnicodeFractions(value);
            value = RemoveAbbreviations(value);
            value = RemoveDoubleUnits(value);
            value = ClumpFractions(value);
            return value;
        }

        private static readonly Regex clumpFractions = new Regex("(\\d+)\\s+(\\d)/(\\d)");
        private static string ClumpFractions(string value) => clumpFractions.Replace(value, "$1$$$2/$3");

        private static readonly ReadOnlyCollection<string> americanUnits = Array.AsReadOnly(new string[]
        {
        "cup", "tablespoon", "teaspoon", "pound", "ounce", "quart", "pint",
        });

        private static string RemoveDoubleUnits(string value)
        {
            foreach (var unit in americanUnits)
            {
                value = value.Replace(unit + '/', unit + ' ');
                value = value.Replace(unit + "s/", unit + "s ");
            }
            return value;
        }

        private static readonly Regex gramsAbbr = new Regex("(\\d+)g", RegexOptions.IgnoreCase),
            ouncesAbbr = new Regex("(\\d+)oz", RegexOptions.IgnoreCase),
            milAbbr = new Regex("(\\d+)ml", RegexOptions.IgnoreCase),
            tspAbbr = new Regex("(\\d+)tsp", RegexOptions.IgnoreCase),
            tbspAbbr = new Regex("(\\d+)tbsp", RegexOptions.IgnoreCase),
            tAbbr = new Regex("(\\d+)t", RegexOptions.IgnoreCase),
            tbAbbr = new Regex("(\\d+)tb", RegexOptions.IgnoreCase),
            lbAbbr = new Regex("(\\d+)lb", RegexOptions.IgnoreCase),
            lbsAbbr = new Regex("(\\d+)lbs", RegexOptions.IgnoreCase),
            cupAbbr = new Regex("(\\d+)c", RegexOptions.IgnoreCase),
            flozAbbr = new Regex("(\\d+)fl oz", RegexOptions.IgnoreCase),
            lAbbr = new Regex("(\\d+)l", RegexOptions.IgnoreCase),
            ptAbbr = new Regex("(\\d+)pt", RegexOptions.IgnoreCase),
            qtAbbr = new Regex("(\\d+)qt", RegexOptions.IgnoreCase),
            kgAbbr = new Regex("(\\d+)kg", RegexOptions.IgnoreCase),
            mgAbbr = new Regex("(\\d+)mg", RegexOptions.IgnoreCase),
            dozAbbr = new Regex("(\\d+)doz", RegexOptions.IgnoreCase);

        private static string RemoveAbbreviations(string value)
        {
            value = gramsAbbr.Replace(value, "$1 gram");
            value = ouncesAbbr.Replace(value, "$1 ounce");
            value = milAbbr.Replace(value, "$1 milliliter");
            value = tspAbbr.Replace(value, "$1 teaspoon");
            value = tbspAbbr.Replace(value, "$1 tablespoon");
            value = tAbbr.Replace(value, "$1 tablespoon");
            value = tbAbbr.Replace(value, "$1 tablespoon");
            value = lbAbbr.Replace(value, "$1 pound");
            value = lbsAbbr.Replace(value, "$1 pounds");
            value = cupAbbr.Replace(value, "$1 cup");
            value = flozAbbr.Replace(value, "$1 fluid ounce");
            value = lAbbr.Replace(value, "$1 liter");
            value = ptAbbr.Replace(value, "$1 pint");
            value = qtAbbr.Replace(value, "$1 quart");
            value = kgAbbr.Replace(value, "$1 kilogram");
            value = mgAbbr.Replace(value, "$1 milligram");
            value = dozAbbr.Replace(value, "$1 dozen");
            return value;
        }

        private static readonly ReadOnlyCollection<(string, string)> fractions = Array.AsReadOnly(new (string, string)[]
        {
        ("\x215b", " 1/8" ),
        ( "\x215c", " 3/8" ),
        ( "\x215d", " 5/8" ),
        ( "\x215e", " 7/8" ),
        ( "\x2159", " 1/6" ),
        ( "\x215a", " 5/6" ),
        ( "\x2155", " 1/5" ),
        ( "\x2156", " 2/5" ),
        ( "\x2157", " 3/5" ),
        ( "\x2158", " 4/5" ),
        ( "\xbc", " 1/4" ),
        ( "\xbe", " 3/4" ),
        ( "\x2153", " 1/3" ),
        ( "\x2154", " 2/3" ),
        ( "\xbd", " 1/2" ),
        });
        private static string RemoveUnicodeFractions(string value)
        {
            foreach (var item in fractions)
                value = value.Replace(item.Item1, item.Item2);
            return value;
        }
    }
}
