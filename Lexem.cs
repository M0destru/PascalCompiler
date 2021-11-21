using System;
using System.Collections.Generic;

namespace PascalCompiler
{
    public enum ETokenType
    {
        Identifier = 1, 
        Const,
        Operation
    }

    public enum EOperation
    {
        Program = 3,
        Semicolon, // ;
        Point, // .
        Equals, // =
        Var,
        Comma, // ,
        Colon, // :
        Begin,
        End,
        If,
        While,
        Assignment, // :=
        RightBracket, // )
        Then,
        Do,
        Plus, // +
        Min, // -
        Mul, // *
        Division, // /
        NotEquals, // <>
        Less, // <
        Bigger, // >
        Lesseqv, // <=
        Bigeqv, // >=
        TwoPoint, // ..
        At, // @
        Quote, // '
        LeftBrace, // {
        RightBrace, // }
        LeftSqBracket, // [
        RightSqBracket, // ]
        LeftBracket, // (
        Grid, // #
        Dollar, // $
        Lid, // ^
        Abs,
        And,
        Array,
        Case,
        Const,
        Div,
        Downto,
        Else,
        File,
        For,
        Function,
        Goto,
        In,
        Label,
        Mod,
        Nil,
        Not,
        Of,
        Or,
        Packed,
        Procedure,
        Read,
        Readln,
        Record,
        Repeat,
        Set,
        To,
        Type,
        Until,
        Uses,
        With,
        Write,
        Writeln
    }

    public enum EValueType
    {
        Integer,
        Real,
        String
    };

    public enum EErrorType
    {
        errExpectedIdent = 1,
        errExpectedConst,
        errExpectedProgram,
        errExpectedSemicolon,
        errExpectedPoint,
        errExpectedEquals,
        errExpectedVar,
        errExpectedComma,
        errExpectedColon,
        errExpectedBegin,
        errExpectedEnd,
        errExpectedIf,
        errExpectedWhile,
        errExpectedAssignment,
        errExpectedBracketEnd,
        errExpectedThen,
        errExpectedDo,
        errUnknownLexem,
        errEOF,
        errMissingQuote,
        errInIntegerConst,
        errInRealConst

    }

    class Error : Exception
    {
        static Dictionary<EErrorType, string> errMap = new Dictionary<EErrorType, string>
        {
            [EErrorType.errExpectedIdent] = "Ident expected",
            [EErrorType.errExpectedConst] = "Const expected",
            [EErrorType.errExpectedProgram] = "'program' expected",
            [EErrorType.errExpectedSemicolon] = "';' expected",
            [EErrorType.errExpectedPoint] = "'.' expected",
            [EErrorType.errExpectedEquals] = "'=' expected",
            [EErrorType.errExpectedVar] = "'var' expected",
            [EErrorType.errExpectedComma] = "',' expected",
            [EErrorType.errExpectedColon] = "':' expected",
            [EErrorType.errExpectedBegin] = "'begin' expected",
            [EErrorType.errExpectedEnd] = "'end' expected",
            [EErrorType.errExpectedIf] = "'if' expected",
            [EErrorType.errExpectedWhile] = "'while' expected",
            [EErrorType.errExpectedAssignment] = "':=' expected",
            [EErrorType.errExpectedBracketEnd] = "')' expected",
            [EErrorType.errExpectedThen] = "'then' expected",
            [EErrorType.errExpectedDo] = "'do' expected",
            [EErrorType.errUnknownLexem] = "Illegal character",
            [EErrorType.errEOF] = "Unexpected end of file",
            [EErrorType.errMissingQuote] = "String constant exceeds line",
            [EErrorType.errInIntegerConst] = "Error in integer constant",
            [EErrorType.errInRealConst] = "Error in real constant",
        };

        public int Line { get; set; }
        public int Col { get; set; }
        public EErrorType ErrorType { get; set; }

        public Error(int line, int col, EErrorType errType)
        {
            Line = line;
            Col = col;
            ErrorType = errType;
        }

        public Error(int line, int col, ETokenType tt)
        {
            Line = line;
            Col = col;
            ErrorType = (EErrorType)(int)tt;
        }

        public Error (int line, int col, EOperation expectedOp)
        {
            Line = line;
            Col = col;
            ErrorType = (EErrorType)(int)expectedOp;
        }

        public override string ToString()
        {
            string errMsg = "";
            errMsg = errMsg.PadLeft(Col+5);
            errMsg += $"^\n****[Error] Code { (int)ErrorType }: { errMap[ErrorType] }****";
            return errMsg;
        }
    }

    abstract class CToken
    {
        public static Dictionary<string, EOperation> operationMap = new Dictionary<string, EOperation>
        {
            ["+"] = EOperation.Plus,
            ["-"] = EOperation.Min,
            ["*"] = EOperation.Mul,
            ["/"] = EOperation.Div,
            ["="] = EOperation.Equals,
            ["<>"] = EOperation.NotEquals,
            ["<"] = EOperation.Less,
            [">"] = EOperation.Bigger,
            ["<="] = EOperation.Lesseqv,
            [">="] = EOperation.Bigeqv,
            ["."] = EOperation.Point,
            [".."] = EOperation.TwoPoint,
            [","] = EOperation.Comma,
            [";"] = EOperation.Semicolon,
            [":"] = EOperation.Colon,
            [":="] = EOperation.Assignment,
            ["@"] = EOperation.At,
            ["'"] = EOperation.Quote,
            ["{"] = EOperation.LeftBrace,
            ["}"] = EOperation.RightBrace,
            ["["] = EOperation.LeftSqBracket,
            ["]"] = EOperation.RightSqBracket,
            ["("] = EOperation.LeftBracket,
            [")"] = EOperation.RightBracket,
            ["#"] = EOperation.Grid,
            ["$"] = EOperation.Dollar,
            ["^"] = EOperation.Lid,
            ["abs"] = EOperation.Abs,
            ["and"] = EOperation.And,
            ["array"] = EOperation.Array,
            ["begin"] = EOperation.Begin,
            ["case"] = EOperation.Case,
            ["const"] = EOperation.Const,
            ["div"] = EOperation.Div,
            ["do"] = EOperation.Do,
            ["downto"] = EOperation.Downto,
            ["else"] = EOperation.Else,
            ["end"] = EOperation.End,
            ["file"] = EOperation.File,
            ["for"] = EOperation.For,
            ["function"] = EOperation.Function,
            ["goto"] = EOperation.Goto,
            ["if"] = EOperation.If,
            ["in"] = EOperation.In,
            ["label"] = EOperation.Label,
            ["mod"] = EOperation.Mod,
            ["nil"] = EOperation.Nil,
            ["not"] = EOperation.Not,
            ["of"] = EOperation.Of,
            ["or"] = EOperation.Or,
            ["packed"] = EOperation.Packed,
            ["procedure"] = EOperation.Procedure,
            ["program"] = EOperation.Program,
            ["read"] = EOperation.Read,
            ["readln"] = EOperation.Readln,
            ["record"] = EOperation.Record,
            ["repeat"] = EOperation.Repeat,
            ["set"] = EOperation.Set,
            ["then"] = EOperation.Then,
            ["to"] = EOperation.To,
            ["type"] = EOperation.Type,
            ["until"] = EOperation.Until,
            ["uses"] = EOperation.Uses,
            ["var"] = EOperation.Var,
            ["while"] = EOperation.While,
            ["with"] = EOperation.With,
            ["write"] = EOperation.Write,
            ["writeln"] = EOperation.Writeln
        };

        public ETokenType TokenType { get; set; }

    }

    class OperationToken : CToken
    {
        public EOperation OperType { get; set; }
        public string Oper { get; set; }

        public OperationToken(EOperation operType, string oper)
        {
            TokenType = ETokenType.Operation;
            OperType = operType;
            Oper = oper;
        }

        public override string ToString()
        {
            return $"{Oper}" ;
        }
    }

    class IdentifierToken : CToken
    {

        public string IdentifierName { get; set; }

        public IdentifierToken(string name)
        {
            TokenType = ETokenType.Identifier;
            IdentifierName = name;
        }

        public override string ToString()
        {
            return $"{IdentifierName}";
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

        public ConstValueToken(double value)
        {
            TokenType = ETokenType.Const;
            ConstVal = new RealVariant(value);
        }

        public ConstValueToken(string value)
        {
            TokenType = ETokenType.Const;
            ConstVal = new StringVariant(value);
        }

        public override string ToString()
        {
            return $"{ConstVal} ";
        }
    }

    abstract class CVariant
    {
        public EValueType ValueType { get; set; }
    }

    class IntegerVariant : CVariant
    {
        public int IntegerValue { get; set; }
        public IntegerVariant(int value)
        {
            ValueType = EValueType.Integer;
            IntegerValue = value;
        }


        public override string ToString()
        {
            return $"{IntegerValue}";
        }
    }

    class RealVariant : CVariant
    {
        public double RealValue { get; set; }

        public RealVariant(double value)
        {
            ValueType = EValueType.Real;
            RealValue = value;
        }

        public override string ToString()
        {
            return $"{RealValue}";
        }
    }

    class StringVariant : CVariant
    {
        public string StringValue { get; set; }

        public StringVariant(string value)
        {
            ValueType = EValueType.String;
            StringValue = value;
        }

        public override string ToString()
        {
            return $"{StringValue}";
        }
    }
}
