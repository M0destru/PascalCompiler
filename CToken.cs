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
        Writeln,
        True,
        False
    }

    public enum EValueType
    {
        Integer,
        Real,
        String,
        Boolean,
        Unknown
    };

   
    abstract class CToken
    {
        public static Dictionary<string, EOperation> operationMap = new Dictionary<string, EOperation>
        {
            ["+"] = EOperation.Plus,
            ["-"] = EOperation.Min,
            ["*"] = EOperation.Mul,
            ["/"] = EOperation.Division,
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
            ["writeln"] = EOperation.Writeln,
            ["true"] = EOperation.True,
            ["false"] = EOperation.False
        };

        public ETokenType TokenType { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }

        public CToken (int lexerLine, int lexerCol)
        {
            Line = lexerLine;
            Col = lexerCol;
        }
    }

    class OperationToken : CToken
    {
        public EOperation OperType { get; set; }
        public string Oper { get; set; }

        public OperationToken(EOperation operType, string oper, int lexerLine, int lexerCol) : base (lexerLine, lexerCol)
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

        public IdentifierToken(string name, int lexerLine, int lexerCol) : base(lexerLine, lexerCol)
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

        public ConstValueToken(int value, int lexerLine, int lexerCol) : base(lexerLine, lexerCol)
        {
            TokenType = ETokenType.Const;
            ConstVal = new IntegerVariant(value);
        }

        public ConstValueToken(double value, int lexerLine, int lexerCol) : base(lexerLine, lexerCol)
        {
            TokenType = ETokenType.Const;
            ConstVal = new RealVariant(value);
        }

        public ConstValueToken(string value, int lexerLine, int lexerCol) : base(lexerLine, lexerCol)
        {
            TokenType = ETokenType.Const;
            ConstVal = new StringVariant(value);
        }

        public ConstValueToken(bool value, int lexerLine, int lexerCol): base(lexerLine, lexerCol)
        {
            TokenType = ETokenType.Const;
            ConstVal = new BooleanVariant(value);
        }

        public override string ToString()
        {
            return $"{ConstVal} ";
        }
    }    
}
