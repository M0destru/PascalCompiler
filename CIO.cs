using System;
using System.IO;
using System.Collections.Generic;

namespace PascalCompiler
{

    abstract class CToken
    {
        public enum ETokenType
        {
            Identifier,
            Operation,
            Const
        }

        public enum EOperation
        {
            /* операции */
            Plus, // +
            Min, // -
            Mul, // *
            Division, // /
            Equals, // =
            NotEquals, // <>
            Less, // <
            Bigger, // >
            Lesseqv, // <=
            Bigeqv, // >=
            Point, // .
            TwoPoint, // ..
            Comma, // ,
            Semicolon, // ;
            Colon, // :
            Assignment, // :=
            At, // @
            Quote, // '
            LeftBrace, // {
            RightBrace, // }
            LeftSqBracket, // [
            RightSqBracket, // ]
            LeftBracket, // (
            RightBracket, // )
            Grid, // #
            Dollar, // $
            Lid, // ^
            DoubleSlash, // //
            /* ключевые слова */
            Abs,
            And,
            Array,
            Begin,
            Case,
            Const,
            Div,
            Do,
            Downto,
            Else,
            End,
            File,
            For,
            Function,
            Goto,
            If,
            In,
            Label,
            Mod,
            Nil,
            Not,
            Of,
            Or,
            Packed,
            Procedure,
            Program,
            Read,
            Readln,
            Record,
            Repeat,
            Set,
            Then,
            To,
            Type,
            Until,
            Uses,
            Var,
            While,
            With,
            Write,
            Writeln,
            Integer,
            Float,
            String
        }

        public ETokenType TokenType { get; set; }

        public override string ToString()
        {
            return $"{TokenType}";
        }
    }

    class OperationToken : CToken
    {
        public EOperation OperType { get; private set; }
        public string Oper { get; private set; }

        public OperationToken(EOperation operType, string oper)
        {
            TokenType = CToken.ETokenType.Operation;
            OperType = operType;
            Oper = oper;
        }

        public override string ToString()
        {
            return $"{$"{Oper}",-20} | {base.ToString()} ({OperType})";
        }
    }

    class IdentifierToken : CToken
    {
        public static List<string> IdentifierList = new List<string>();

        public string IdentifierName { get; private set; }

        public IdentifierToken(string name)
        {
            TokenType = CToken.ETokenType.Identifier;
            IdentifierName = name;
            IdentifierList.Add(name);
        }

        public override string ToString()
        {
            return $"{IdentifierName,-20} | {base.ToString()}";
        }
    }

    class ConstValueToken : CToken
    {
        public CVariant ConstVal;

        public ConstValueToken(int value)
        {
            TokenType = ETokenType.Const;
            ConstVal = new IntegerVariant(value);
        }

        public ConstValueToken(float value)
        {
            TokenType = ETokenType.Const;
            ConstVal = new FloatVariant(value);
        }

        public ConstValueToken(string value)
        {
            TokenType = ETokenType.Const;
            ConstVal = new StringVariant(value);
        }

        public override string ToString()
        {
            return $"{ConstVal,-20} | {base.ToString()} ({ConstVal.ValueType})";
        }
    }

    abstract class CVariant
    {
        public enum EValueType
        {
            Integer,
            Float,
            String
        };
        public EValueType ValueType { get; set; }

        public override string ToString()
        {
            return $"{ValueType}";
        }
    }

    class IntegerVariant : CVariant
    {
        public int IntegerValue { get; private set; }
        public IntegerVariant(int intValue)
        {
            ValueType = EValueType.Integer;
            IntegerValue = intValue;
        }


        public override string ToString()
        {
            return $"{IntegerValue}";
        }
    }

    class FloatVariant : CVariant
    {
        public float FloatValue { get; private set; }

        public FloatVariant(float floatValue)
        {
            ValueType = EValueType.Float;
            FloatValue = floatValue;
        }

        public override string ToString()
        {
            return $"{FloatValue}";
        }
    }

    class StringVariant : CVariant
    {
        public string StringValue { get; private set; }

        public StringVariant(string stringValue)
        {
            ValueType = EValueType.String;
            StringValue = stringValue;
        }

        public override string ToString()
        {
            return $"{StringValue}";
        }
    }

    class CIO
    {
        public static Dictionary<string, CToken.EOperation> operationMap = new Dictionary<string, CToken.EOperation>()
        {
            ["+"] = CToken.EOperation.Plus,
            ["-"] = CToken.EOperation.Min,
            ["*"] = CToken.EOperation.Mul,
            ["/"] = CToken.EOperation.Div,
            ["="] = CToken.EOperation.Equals,
            ["<>"] = CToken.EOperation.NotEquals,
            ["<"] = CToken.EOperation.Less,
            [">"] = CToken.EOperation.Bigger,
            ["<="] = CToken.EOperation.Lesseqv,
            [">="] = CToken.EOperation.Bigeqv,
            ["."] = CToken.EOperation.Point,
            [".."] = CToken.EOperation.TwoPoint,
            [","] = CToken.EOperation.Comma,
            [";"] = CToken.EOperation.Semicolon,
            [":"] = CToken.EOperation.Colon,
            [":="] = CToken.EOperation.Assignment,
            ["@"] = CToken.EOperation.At,
            ["'"] = CToken.EOperation.Quote,
            ["{"] = CToken.EOperation.LeftBrace,
            ["}"] = CToken.EOperation.RightBrace,
            ["["] = CToken.EOperation.LeftSqBracket,
            ["]"] = CToken.EOperation.RightSqBracket,
            ["("] = CToken.EOperation.LeftBracket,
            [")"] = CToken.EOperation.RightBracket,
            ["#"] = CToken.EOperation.Grid,
            ["$"] = CToken.EOperation.Dollar,
            ["^"] = CToken.EOperation.Lid,
            ["//"] = CToken.EOperation.DoubleSlash,
            ["abs"] = CToken.EOperation.Abs,
            ["and"] = CToken.EOperation.And,
            ["array"] = CToken.EOperation.Array,
            ["begin"] = CToken.EOperation.Begin,
            ["case"] = CToken.EOperation.Case,
            ["const"] = CToken.EOperation.Const,
            ["div"] = CToken.EOperation.Div,
            ["do"] = CToken.EOperation.Do,
            ["downto"] = CToken.EOperation.Downto,
            ["else"] = CToken.EOperation.Else,
            ["end"] = CToken.EOperation.End,
            ["file"] = CToken.EOperation.File,
            ["for"] = CToken.EOperation.For,
            ["function"] = CToken.EOperation.Function,
            ["goto"] = CToken.EOperation.Goto,
            ["if"] = CToken.EOperation.If,
            ["in"] = CToken.EOperation.In,
            ["label"] = CToken.EOperation.Label,
            ["mod"] = CToken.EOperation.Mod,
            ["nil"] = CToken.EOperation.Nil,
            ["not"] = CToken.EOperation.Not,
            ["of"] = CToken.EOperation.Of,
            ["or"] = CToken.EOperation.Or,
            ["packed"] = CToken.EOperation.Packed,
            ["procedure"] = CToken.EOperation.Procedure,
            ["program"] = CToken.EOperation.Program,
            ["read"] = CToken.EOperation.Read,
            ["readln"] = CToken.EOperation.Readln,
            ["record"] = CToken.EOperation.Record,
            ["repeat"] = CToken.EOperation.Repeat,
            ["set"] = CToken.EOperation.Set,
            ["then"] = CToken.EOperation.Then,
            ["to"] = CToken.EOperation.To,
            ["type"] = CToken.EOperation.Type,
            ["until"] = CToken.EOperation.Until,
            ["uses"] = CToken.EOperation.Uses,
            ["var"] = CToken.EOperation.Var,
            ["while"] = CToken.EOperation.While,
            ["with"] = CToken.EOperation.With,
            ["write"] = CToken.EOperation.Write,
            ["writeln"] = CToken.EOperation.Writeln,
            ["integer"] = CToken.EOperation.Integer,
            ["float"] = CToken.EOperation.Float,
            ["string"] = CToken.EOperation.String
        };
        static List<char> controlChar = new List<char>() { '\n', '\t', '\r', ' ', '\0' };

        int pos; // позиция в тексте
        string buf; // пос-ть символов, потенциально образующующих лексему
        char curChar; // текущий рассматриваемый символ
        char nextChar; // следующий рассматриваемый символ
        StreamReader sr; // ввод символов из файл
        CToken curToken; // текущая сформированная лексема


        public CIO(string filename)
        {
            sr = new StreamReader(filename);
            buf = "";
            curChar = nextChar = ' ';
        }

        private void AddToBuffer()
        {
            pos++;
            /* смена текущего символа */
            curChar = nextChar;
            /* считать новый символ */
            int readChar = sr.Read();
            /* если символ не был считан, то достигнут конец файла */
            nextChar = readChar >= 0 ? (char)readChar : '\0';
            /* добавить текущий рассматриваемый элемент в буфер */
            buf += curChar.ToString().ToLower();
        }

        private void SearchCurLexem(Predicate<char> condition)
        {
            while (condition(nextChar))
                AddToBuffer();
        }

        public CToken GetNextToken()
        {
            if (nextChar == '\0') return null;

            /* получить новый символ */
            AddToBuffer();

            /* символы \n, \t, \r, ' ' */
            if (controlChar.Contains(curChar))
            {
                buf = "";
                return GetNextToken();
            }

            /* символ является цифрой */
            if (Char.IsDigit(curChar))
            {
                /* пока в тексте встречается цифра или символ '.', но лишь однажды */
                SearchCurLexem(c => !controlChar.Contains(c) && (char.IsDigit(c) || (char.IsDigit(curChar) && c == '.')));

                string digit = buf;
                /* если встретились две точки подряд */
                if (nextChar == '.')
                {
                    digit = buf.Substring(0, buf.Length - 1);
                    buf = buf[buf.Length - 1].ToString();
                }
                else
                    buf = "";
                return curToken = digit.Contains('.') ? new ConstValueToken(float.Parse(digit.Replace('.', ','))) : new ConstValueToken(int.Parse(digit));
            }

            /* символ является буквой или нижним подчёркиванием */
            else if (Char.IsLetter(curChar) || curChar == '_')
            {
                /* пока в тексте встречается цифра или буква */
                SearchCurLexem(c => !controlChar.Contains(c) && (char.IsLetterOrDigit(c) || c == '_'));

                /* поиск идентификатор в словаре ключевых слов */
                if (operationMap.ContainsKey(buf))
                    curToken = new OperationToken(operationMap[buf], buf);
                else
                    curToken = new IdentifierToken(buf);
            }
            /* символ является кавычкой */
            else if (curChar == '\'')
            {
                AddToBuffer();
                SearchCurLexem(c => c != '\0' && curChar != '\'');
                curToken = new ConstValueToken(buf);
            }

            /* символ является операцией или оператором */
            else if (operationMap.ContainsKey(buf))
            {
                /* пока добавление следующего символа образует составной оператор */
                SearchCurLexem(c => !controlChar.Contains(c) && operationMap.ContainsKey(buf + c));
                curToken = new OperationToken(operationMap[buf], buf);

                /* обработать блок с комментариями */
                if (curChar == '{')
                {
                    SearchCurLexem(c => c != '\0' && c != '}');
                    AddToBuffer();
                    buf = "";
                    return GetNextToken();
                }
            }
            /* очистить буфер и вернуть сформированный токен */
            buf = "";
            return curToken;
        }
    }
}