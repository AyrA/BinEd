﻿using System;
using System.IO;
using System.Linq;

namespace BinEd
{
    /// <summary>
    /// Contains Structures, Enums and Constants
    /// </summary>
    public partial class Program
    {
        #region Const+Enum

        /// <summary>
        /// Status codes unrelated to Command results
        /// </summary>
        private struct RET
        {
            /// <summary>
            /// Application exited sucessfully
            /// </summary>
            /// <remarks>This is always the case unless the "Fatal" option is enabled</remarks>
            public const int OK = 0;
            /// <summary>
            /// Help was displayed
            /// </summary>
            public const int HELP = 254;
            /// <summary>
            /// Invalid Command line Arguments
            /// </summary>
            public const int INVALID = 255;

        }

        /// <summary>
        /// Result codes in relation to Commands
        /// </summary>
        private struct RESULT
        {
            /// <summary>
            /// Command executed sucessfully
            /// </summary>
            public const int OK = 0;
            /// <summary>
            /// Command is invalid
            /// </summary>
            public const int INVALID_COMMAND = 1;
            /// <summary>
            /// No file is open but command needs a file
            /// </summary>
            public const int NOFILE = 2;
            /// <summary>
            /// Number of arguments doesn't matches Command requirements
            /// </summary>
            public const int ARGUMENT_MISMATCH = 3;
            /// <summary>
            /// File already open
            /// </summary>
            public const int FILE_OPEN = 4;
            /// <summary>
            /// Command Valid but File/FileName related issues happened.
            /// </summary>
            public const int IO_ERROR = 5;
            /// <summary>
            /// The given argument is not a valid number
            /// </summary>
            public const int INVALID_NUMBER = 6;
            /// <summary>
            /// The given argument is not a valid argument
            /// </summary>
            public const int INVALID_ARG = 7;
            /// <summary>
            /// The command succeeded but might not have done what was intended
            /// </summary>
            public const int PART_OK = 8;
        }

        /// <summary>
        /// Types of Command
        /// </summary>
        private enum CommandType
        {
            /// <summary>
            /// No command (blank line)
            /// </summary>
            Blank,
            /// <summary>
            /// Command is invalid
            /// </summary>
            Invalid,
            /// <summary>
            /// Create a file
            /// </summary>
            CreateFile,
            /// <summary>
            /// Open an existing file
            /// </summary>
            OpenFile,
            /// <summary>
            /// Close a file
            /// </summary>
            CloseFile,
            /// <summary>
            /// Close and delete current file
            /// </summary>
            DeleteFile,
            /// <summary>
            /// Concatenate files
            /// </summary>
            ConcatFile,
            /// <summary>
            /// Set an option
            /// </summary>
            SetOption,
            /// <summary>
            /// Write specific bytes to file
            /// </summary>
            WriteBytes,
            /// <summary>
            /// Repeatedly writes the given bytes to the file
            /// </summary>
            RepeatBytes,
            /// <summary>
            /// Writes the given number of random bytes to the file
            /// </summary>
            WriteRandom,
            /// <summary>
            /// Seek to the given location
            /// </summary>
            SeekTo,
            /// <summary>
            /// Set file length
            /// </summary>
            SetLength,
            /// <summary>
            /// Find content in file
            /// </summary>
            Find,
            /// <summary>
            /// Dump file content to console
            /// </summary>
            Dump,
            /// <summary>
            /// Current file status
            /// </summary>
            Status,
            /// <summary>
            /// Show Command Help
            /// </summary>
            Help,
            /// <summary>
            /// Show more detailed help
            /// </summary>
            HelpDetails,
            /// <summary>
            /// Quit application
            /// </summary>
            Quit
        }

        /// <summary>
        /// Byte Operations
        /// </summary>
        /// <remarks>Modes that don't overwrite need Read access to the stream</remarks>
        private enum ByteMode
        {
            /// <summary>
            /// Overwrite content
            /// </summary>
            Overwrite,
            /// <summary>
            /// Add value to current byte
            /// </summary>
            Add,
            /// <summary>
            /// Subtract value from current byte
            /// </summary>
            Subtract,
            /// <summary>
            /// AND value with current byte
            /// </summary>
            AND,
            /// <summary>
            /// OR value with current byte
            /// </summary>
            OR,
            /// <summary>
            /// XOR value with current byte
            /// </summary>
            /// <remarks>There is no bitwise NOT operation since NOT is identical to x^0xFF</remarks>
            XOR
        }

        #endregion

        #region Structs

        /// <summary>
        /// Represents a Command from the User
        /// </summary>
        private struct Command
        {
            /// <summary>
            /// Command Type
            /// </summary>
            public CommandType CommandType;
            /// <summary>
            /// Command Arguments
            /// </summary>
            public string[] Arguments;
        }

        /// <summary>
        /// Runtime Options
        /// </summary>
        private struct Options
        {
            /// <summary>
            /// Enable Console output
            /// </summary>
            public bool EnableOutput;
            /// <summary>
            /// Enable Pipe mode (minimalistic Output)
            /// </summary>
            public bool PipeMode;
            /// <summary>
            /// Enable Hard Fail
            /// </summary>
            public bool Fail;
            /// <summary>
            /// Enable FileShare Permissions (ability for other applications to read/write)
            /// </summary>
            /// <remarks>This is very dangerous. Be careful</remarks>
            public bool Share;
        }

        /// <summary>
        /// Byte Operations for 'w' and 'r' instruction
        /// </summary>
        private struct ByteOperation
        {
            /// <summary>
            /// Byte Processing Mode
            /// </summary>
            public ByteMode Mode;
            /// <summary>
            /// Bytes to Process
            /// </summary>
            public byte[] Bytes;
            /// <summary>
            /// Last error from Constructor or ProcessBytes call
            /// </summary>
            public Exception LastError
            { get; private set; }

            public ByteOperation(string Param)
            {
                const string PREFIXES = "+-&|^";

                Mode = ByteMode.Overwrite;
                Bytes = null;
                LastError = null;

                if (!string.IsNullOrWhiteSpace(Param))
                {
                    if (PREFIXES.Contains(Param[0]))
                    {
                        switch (Param[0])
                        {
                            case '+':
                                Mode = ByteMode.Add;
                                break;
                            case '-':
                                Mode = ByteMode.Subtract;
                                break;
                            case '&':
                                Mode = ByteMode.AND;
                                break;
                            case '|':
                                Mode = ByteMode.OR;
                                break;
                            case '^':
                                Mode = ByteMode.XOR;
                                break;
                            default:
                                LastError = new ArgumentException("Invalid Prefix");
                                break;
                        }
                        Bytes = GetBytes(Param.Substring(1));
                    }
                    else
                    {
                        Bytes = GetBytes(Param);
                    }
                }
            }

            /// <summary>
            /// Processes the bytes
            /// </summary>
            /// <param name="S">Stream to operate on</param>
            public bool ProcessBytes(Stream S)
            {
                LastError = null;
                if (Bytes != null && Bytes.Length > 0)
                {
                    if (Mode == ByteMode.Overwrite)
                    {
                        if (S.CanWrite)
                        {
                            try
                            {
                                S.Write(Bytes, 0, Bytes.Length);
                            }
                            catch (Exception ex)
                            {
                                LastError = ex;
                            }
                        }
                        else
                        {
                            LastError = new IOException("Stream is not writable");
                        }
                    }
                    else
                    {
                        if (S.CanSeek && S.CanRead && S.CanWrite)
                        {
                            var Pos = S.Position;
                            var Data = new byte[Bytes.Length];
                            var Readed = S.Read(Data, 0, Data.Length);
                            switch (Mode)
                            {
                                case ByteMode.Add:
                                    Data = Bytes.Select((v, i) => b(v + Data[i])).ToArray();
                                    break;
                                case ByteMode.Subtract:
                                    Data = Bytes.Select((v, i) => b(v - Data[i])).ToArray();
                                    break;
                                case ByteMode.XOR:
                                    Data = Bytes.Select((v, i) => b(v ^ Data[i])).ToArray();
                                    break;
                                case ByteMode.OR:
                                    Data = Bytes.Select((v, i) => b(v | Data[i])).ToArray();
                                    break;
                                case ByteMode.AND:
                                    Data = Bytes.Select((v, i) => b(v & Data[i])).ToArray();
                                    break;
                                default:
                                    LastError = new NotImplementedException($"ByteMode {Mode} is not implemented");
                                    break;
                            }
                            if (LastError == null)
                            {
                                try
                                {
                                    S.Position = Pos;
                                    S.Write(Data, 0, Data.Length);
                                }
                                catch (Exception ex)
                                {
                                    LastError = ex;
                                }
                            }
                        }
                        else
                        {
                            LastError = new IOException("Stream is not writable/readable/seekable");
                        }
                    }
                }
                return LastError == null;
            }

            private static byte b(int i)
            {
                return (byte)(i & 0xFF);
            }
        }

        #endregion
    }
}
