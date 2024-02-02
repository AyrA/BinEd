using System.Text;

namespace BinEd
{
    internal static class Dump
    {
        private const string Charset = ".☺☻♥♦♣♠•◘○◙♂♀♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼ !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~⌂ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜ¢£¥₧ƒáíóúñÑªº¿⌐¬½¼¡«»░▒▓│┤╡╢╖╕╣║╗╝╜╛┐└┴┬├─┼╞╟╚╔╩╦╠═╬╧╨╤╥╙╘╒╓╫╪┘┌█▄▌▐▀αßΓπΣσµτΦΘΩδ∞φε∩≡±≥≤⌠⌡÷≈°∙·√ⁿ²■ ";

        public static string HexDump(byte[] data, int width = 16)
        {
            ArgumentNullException.ThrowIfNull(data);

            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i += width)
            {
                var chunk = data.Skip(i).Take(width).ToArray();
                sb.AppendFormat("0x{0:X8}\t{1}\t{2}{3}", i,
                    string.Join(' ', chunk.Select(m => m.ToString("X2"))).PadRight(width * 3),
                    string.Concat(chunk.Select(m => Charset[m])),
                    Environment.NewLine
                    );
            }
            return sb.ToString().TrimEnd();
        }
    }
}
