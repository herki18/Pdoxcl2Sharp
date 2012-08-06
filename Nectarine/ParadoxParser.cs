﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nectarine
{
    enum LexerToken
    {
        Equals,
        Quote,
        LeftCurly,
        RightCurly,
        LeftParanthesis,
        RightParanthesis,
        Comment,
        Comma,
        Untyped
    }
    public class ParadoxParser
    {
        // White space constants
        private const byte SPACE = 0x20;
        private const byte HORIZONTAL_TAB = 0x09;
        private const byte NEWLINE = 0x0A;
        private const byte VERTICAL_TAB = 0x0B;
        private const byte FEED = 0x0C;
        private const byte CARRIAGE_RETURN = 0x0D;

        //Single character tokens
        private const byte EQUALS = 0x3D;
        private const byte QUOTE = 0x22;
        private const byte RIGHT_CURLY = 0x7D;
        private const byte LEFT_CURLY = 0x7B;
        private const byte COMMENT = 0x23;
        private const byte SEMI_COLON = 0x3B;
        private const byte LEFTPARANTHESIS = 0x28;
        private const byte RIGHTPARANTHESIS = 0x29;
        private const byte EXCLAMATION = 0x21;
        private const byte COMMA = 0x2C;

        public static bool IsSpace(byte c)
        {
            return c == SPACE || (c >= HORIZONTAL_TAB && c <= CARRIAGE_RETURN);
        }

        private static LexerToken IsSingleCharTok(byte c)
        {
            switch (c)
            {
                case EQUALS:
                    return LexerToken.Equals;
                case QUOTE:
                    return LexerToken.Quote;
                case LEFT_CURLY:
                    return LexerToken.LeftCurly;
                case RIGHT_CURLY:
                    return LexerToken.RightCurly;
                case LEFTPARANTHESIS:
                    return LexerToken.LeftParanthesis;
                case RIGHTPARANTHESIS:
                    return LexerToken.RightParanthesis;
                case COMMENT:
                case EXCLAMATION:
                case COMMA:
                    return LexerToken.Comment;
                default:
                    return LexerToken.Untyped;
            }
        }


        private byte currentByte;
        private byte lastByte;
        private int currentPosition;
        private int bufferSize;
        private int desiredBufferSize;
        private byte[] buffer;
        private StringBuilder stringBuffer;
        private Stream stream;

        private bool eof = false;

        private IDictionary<string, Action<ParadoxParser>> strategy;
        public string CurrentToken { get; private set; }

        public ParadoxParser(byte[] data, IDictionary<string, Action<ParadoxParser>> parseStrategy, int bufferSize = Globals.BUFFER_SIZE)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (parseStrategy == null)
                throw new ArgumentNullException("parseStrategy");

            if (bufferSize < 1)
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Buffer size must be greater than 0");

            this.strategy = parseStrategy;
            this.desiredBufferSize = bufferSize;
            this.buffer = new byte[desiredBufferSize];
            this.stringBuffer = new StringBuilder();

            using (stream = new MemoryStream(data))
            {
                parse(stream);
            }
        }

        public ParadoxParser(string filePath, IDictionary<string, Action<ParadoxParser>> parseStrategy, int bufferSize = Globals.BUFFER_SIZE)
        {
            if (parseStrategy == null)
                throw new ArgumentNullException("parseStrategy");

            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            if (bufferSize < 1)
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Buffer size must be greater than 0");

            this.strategy = parseStrategy;
            this.desiredBufferSize = bufferSize;
            this.buffer = new byte[desiredBufferSize];
            this.stringBuffer = new StringBuilder();

            using (stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                parse(stream);
            }
        }

        private void parse(Stream stream)
        {
            Action<ParadoxParser> action;
            do
            {
                string currentLine = GetToken(stream);
                if (!String.IsNullOrEmpty(currentLine))
                {
                    if (strategy.TryGetValue(currentLine, out action))
                        action(this);
                }
            } while (!eof);
        }

        private string GetToken(Stream fs)
        {
            if (eof)
                return null;

            while (IsSpace(currentByte = Get(fs)) && !eof)
                ;


            if (currentByte == COMMENT)
            {
                while ((currentByte = Get(fs)) != NEWLINE && !eof)
                    ;
                return GetToken(fs);
            }
            else if (currentByte == QUOTE)
            {
                while ((currentByte = Get(fs)) != QUOTE && !eof)
                    stringBuffer.Append((char)currentByte);

                CurrentToken = stringBuffer.ToString();
                stringBuffer.Clear();
                return CurrentToken;
            }

            do
            {
                stringBuffer.Append((char)currentByte);
            } while (!IsSpace(currentByte = Get(fs)) && IsSingleCharTok(currentByte) == LexerToken.Untyped && !eof);

            CurrentToken = stringBuffer.ToString();
            stringBuffer.Clear();
            return CurrentToken;
        }

        private byte Get(Stream stream)
        {
            if (currentPosition == bufferSize && !eof)
            {
                bufferSize = stream.Read(buffer, 0, desiredBufferSize);
                currentPosition = 0;

                if (bufferSize == 0)
                {
                    eof = true;
                    return 0;
                }
            }

            return buffer[currentPosition++];
        }

        public string ReadString()
        {
            return GetToken(stream);
        }

        public int ReadInt32()
        {
            int result = 0;
            while (IsSingleCharTok(currentByte = Get(stream)) == LexerToken.Untyped && !eof)
            {
                if (currentByte >= 0x30 && currentByte <= 0x39)
                {
                    result = 10 * result + (currentByte - 0x30);
                }
            }
            return result;
        }
    }
}