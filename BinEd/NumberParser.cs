using System.Globalization;
using System.Text.RegularExpressions;

namespace BinEd
{
    internal static partial class NumberParser
    {
        private static readonly Regex HexNumber = HexMatch();
        private static readonly Regex RegularNumber = RegularMatch();

        public static OffsetType GetOffsetType(string? number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return OffsetType.Absolute;
            }
            if (number.TrimStart().IndexOfAny(['-', '+']) == 0)
            {
                return OffsetType.Relative;
            }
            return OffsetType.Absolute;
        }

        public static bool Parse(string? number, out long value)
        {
            value = default;
            if (string.IsNullOrWhiteSpace(number))
            {
                return false;
            }

            if (!HexNumber.IsMatch(number) && !RegularNumber.IsMatch(number))
            {
                return false;
            }

            bool neg = false;
            var m = HexNumber.Match(number);

            if (m.Success)
            {
                neg = m.Groups["sign"].Value == "-";
                if (!long.TryParse(m.Groups["value"].Value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out value))
                {
                    return false;
                }
                if (neg)
                {
                    value *= -1;
                }
                return true;
            }

            m = RegularNumber.Match(number);
            if (m.Success)
            {
                if (!long.TryParse(m.Groups["value"].Value, out value))
                {
                    return false;
                }
                if (neg)
                {
                    value *= -1;
                }
                return true;
            }
            return false;
        }

        [GeneratedRegex(@"^\s*(?<sign>[-+]?)0[xX](?<value>[\dA-Fa-f]+)\s*$")]
        private static partial Regex HexMatch();

        [GeneratedRegex(@"^\s*(?<sign>[-+]?)(?<value>\d+)\s*$")]
        private static partial Regex RegularMatch();
    }

    public enum OffsetType
    {
        Relative,
        Absolute
    }
}
