using System;
using System.IO;
using System.Collections.Generic;

namespace PascalCompiler
{
    enum EErrorType
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
        errInRealConst,
        errInStatement,
        errDuplicateIdent,
        errUnknownIdent,
        errInType,
        errTypeMismatch,
        errInLogicExpr
    }

    class CompilerError : Exception
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
            [EErrorType.errInStatement] = "Error in statement",
            [EErrorType.errDuplicateIdent] = "Duplicate identifier",
            [EErrorType.errUnknownIdent] = "Unknown identifier",
            [EErrorType.errInType] = "Error in type definiton",
            [EErrorType.errTypeMismatch] = "Type mismatch",
            [EErrorType.errInLogicExpr] = "Expression type must be boolean"
        };

        public int Line { get; set; }
        public int Col { get; set; }
        public EErrorType ErrorType { get; set; }

        public CompilerError(int line, int col, EErrorType errType)
        {
            Line = line;
            Col = col;
            ErrorType = errType;
        }

        public CompilerError(int line, int col, ETokenType tt)
        {
            Line = line;
            Col = col;
            ErrorType = (EErrorType)(int)tt;
        }

        public CompilerError(int line, int col, EOperation expectedOp)
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

    /* менеджер ошибок - отвечает за хранение и вывод ошибок в файл */
    class CErrorManager
    {
        List<CompilerError> errors;
        StreamWriter writer;

        public CErrorManager(StreamWriter writer)
        {
            errors = new List<CompilerError>();
            this.writer = writer;
        }

        /* добавить новую ошибку */
        public void AddError(CompilerError err, bool throwErr = false)
        {
            errors.Add(err);
            writer.WriteLine(err);
            if (throwErr)
                throw err;
        }

        /* вернуть число обнаруженных ошибок */
        public void PrintNumOfErrors()
        {
            writer.WriteLine ($"\nКоличество ошибок: {errors.Count}");
        }

    }
}
