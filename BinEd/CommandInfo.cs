using System.Text.RegularExpressions;

namespace BinEd
{
    internal partial class CommandInfo
    {
        private readonly Regex argmentExtractor = CommandExtractor();
        private readonly Dictionary<string, Action<string>> cmdMap;
        private string[] _arguments = [];

        public CommandType Command { get; private set; }
        public string[] Arguments => _arguments;

        public CommandInfo(string? command)
        {
            cmdMap = [];
            cmdMap.Add("EXIT", CmdExit);
            cmdMap.Add("OPEN", CmdOpen);
            cmdMap.Add("CLOSE", CmdClose);
            cmdMap.Add("READ", CmdRead);
            cmdMap.Add("SEEK", CmdSeek);

            if (string.IsNullOrWhiteSpace(command))
            {
                Command = CommandType.Empty;
                return;
            }
            command = (command ?? "EXIT").Trim();

            var commandName = command.Split(' ')[0].ToUpperInvariant();
            if (cmdMap.TryGetValue(commandName, out var cmdFunc))
            {
                cmdFunc(command);
            }
            else
            {
                Command = CommandType.Unknown;
            }
        }

        private void CmdExit(string command)
        {
            Command = CommandType.Exit;
        }

        private void CmdClose(string command)
        {
            Command = CommandType.Close;
        }

        private void CmdOpen(string command)
        {
            Command = CommandType.Open;
            var parts = CommandExtractor().Match(command);
            if (!parts.Success)
            {
                Command = CommandType.Invalid;
                return;
            }
            var fn = parts.Groups["arg"].Value.Trim();
            if (fn.StartsWith('"') && fn.EndsWith('"'))
            {
                fn = fn[1..^1];
            }
            if (string.IsNullOrWhiteSpace(fn))
            {
                Command = CommandType.Invalid;
                return;
            }
            _arguments = [fn];
        }

        private void CmdRead(string command)
        {
            Command = CommandType.Read;
            var parts = CommandExtractor().Match(command);
            if (!parts.Success)
            {
                Command = CommandType.Invalid;
                return;
            }
            var count = parts.Groups["arg"].Value.Trim();
            if (!NumberParser.Parse(count, out var value) || value < 0)
            {
                Command = CommandType.Invalid;
                return;
            }
            _arguments = [value.ToString()];
        }

        private void CmdSeek(string command)
        {
            Command = CommandType.Seek;
            var parts = CommandExtractor().Match(command);
            if (!parts.Success)
            {
                Command = CommandType.Invalid;
                return;
            }
            var count = parts.Groups["arg"].Value.Trim();
            var offsetType = NumberParser.GetOffsetType(count);
            if (!NumberParser.Parse(count, out var value))
            {
                Command = CommandType.Invalid;
                return;
            }
            _arguments = [offsetType.ToString(), value.ToString()];
        }

        private static string[] Parse(string? command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return [];
            }
            var parts = new List<string>();
            return [.. parts];
        }

        [GeneratedRegex(@"^(?<cmd>\S+)\s+(?<arg>.+)$")]
        private static partial Regex CommandExtractor();
    }

    public enum CommandType
    {
        Invalid,
        Unknown,
        Empty,
        Exit,
        Open,
        Close,
        Read,
        Write,
        Seek,
        Copy,
        Paste,
        Dump
    }
}
