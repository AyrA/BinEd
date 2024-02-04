namespace BinEd
{
    internal class Program
    {
        private static BinFile? currentFile;

        private static void Main(string[] args)
        {
            if (args.Contains("/?"))
            {
                Help();
                return;
            }
            Console.ResetColor();
            Console.WriteLine("BinEd | Type ? for help");
            while (true)
            {
                SetTitle();
                var cmd = ReadCommand();
                try
                {
                    switch (cmd.Command)
                    {
                        case CommandType.Invalid:
                            Console.WriteLine("Command or arguments are invalid");
                            break;
                        case CommandType.Unknown:
                            Console.WriteLine("Unknown command");
                            break;
                        case CommandType.Empty:
                            Console.WriteLine("BinEd | Type ? for help");
                            break;
                        case CommandType.Exit:
                            Console.WriteLine("Exiting BinEd");
                            currentFile?.Dispose();
                            return;
                        case CommandType.Open:
                            try
                            {
                                var f = new BinFile(cmd.Arguments[0], false);
                                if (currentFile != null)
                                {
                                    currentFile.Dispose();
                                    Console.WriteLine("Closed existing file");
                                }
                                currentFile = f;
                                Console.WriteLine("Opened {0}", f.FileName);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Failed to open {0}. {1}", cmd.Arguments[0], ex.Message);
                            }
                            break;
                        case CommandType.Close:
                            if (currentFile != null)
                            {
                                currentFile.Dispose();
                                currentFile = null;
                                Console.WriteLine("File closed");
                            }
                            else
                            {
                                Console.WriteLine("No file open");
                            }
                            break;
                        case CommandType.Read:
                            if (currentFile == null)
                            {
                                Console.WriteLine("No file open");
                            }
                            else
                            {
                                Console.WriteLine(Dump.HexDump(currentFile.Read(int.Parse(cmd.Arguments[0]))));
                            }
                            break;
                        case CommandType.Write:
                            break;
                        case CommandType.Seek:
                            if (currentFile == null)
                            {
                                Console.WriteLine("No file open");
                            }
                            else
                            {
                                var offset = long.Parse(cmd.Arguments[1]);
                                var type = Enum.Parse<OffsetType>(cmd.Arguments[0]);
                                if (type == OffsetType.Absolute)
                                {
                                    currentFile.Seek(offset);
                                }
                                else
                                {
                                    currentFile.Seek(offset + currentFile.Position);
                                }
                                Console.WriteLine("New position: {0}", currentFile.Position);
                            }
                            break;
                        case CommandType.Copy:
                            break;
                        case CommandType.Paste:
                            break;
                        case CommandType.Dump:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed to execute command. {0}", ex.Message);
                    Console.ResetColor();
                }
            }
        }

        private static CommandInfo ReadCommand()
        {
            Console.Write("Command: ");
            Console.ForegroundColor = ConsoleColor.Green;
            var ret = new CommandInfo(Console.ReadLine());
            Console.ResetColor();
            return ret;
        }

        private static void SetTitle()
        {
            if (currentFile == null)
            {
                Console.Title = "BinEd | <No File>";
            }
            else
            {
                Console.Title = $"BinEd | {Path.GetFileName(currentFile.FileName)}:{currentFile.Position}";
            }
        }

        private static void Help()
        {
            Console.WriteLine(@"BinEd [/S] [file]
Editor for binary files of arbitrary size

file    File to open
/S      Treat the file as a script");
        }
    }
}