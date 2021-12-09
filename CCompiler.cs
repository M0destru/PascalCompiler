using System.Collections.Generic;
using System.IO;

namespace PascalCompiler
{
    /* компилятор */
    class CCompiler
    {
        StreamReader reader;
        StreamWriter writer;
        CLexer lexer;
        CErrorManager errManager;
        CToken curToken;

        /* допустимые типы */
        Dictionary<string, CType> availableTypes;
        /* список переменных */
        Dictionary<string, CVariable> variables;

        public CCompiler(string inFileName, string outFileName)
        {
            reader = new StreamReader(inFileName);
            writer = new StreamWriter(outFileName);
            errManager = new CErrorManager(writer);
            lexer = new CLexer(reader, writer);
            availableTypes = new Dictionary<string, CType>
            {
                ["integer"] = new CIntType(),
                ["real"] = new CRealType(),
                ["string"] = new CStringType(),
                ["boolean"] = new CBooleanType(),
                ["unknown"] = new CUnknownType()
            };
            variables = new Dictionary<string, CVariable>();
            GetNextToken();
            Programma();
            errManager.PrintNumOfErrors();

            /* закрытие файлов */
            reader.Close();
            writer.Close();
        }

        /* неявное преобразование значения одного типа в значение другого типа */
        private CType TypeCasting(CType left, CType right)
        {
            if (left.isDerivedFrom(right))
                return left;
            if (right.isDerivedFrom(left))
                return right;
            return availableTypes["unknown"];
        }

        /* операнды операций отношения */
        private CType CompareOperands(CType left, CType right)
        {
            /* типы совпадают или левый операнд приводИм к правому операнду и наоборот */
            if (left.isDerivedFrom(right) || right.isDerivedFrom(left))
                return availableTypes["boolean"];

            /* типы неприводимы друг к другу */
            errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errTypeMismatch));
            return availableTypes["unknown"];
        }

        /* операнды аддитивных операций */
        private CType AdditiveOperands(CType left, EOperation op, CType right)
        {
            /* оба типа оказались неизвестными */
            if (left.Type == EValueType.Unknown && right.Type == EValueType.Unknown)
                return left;

            /* типы совпадают или левый операнд приводИм к правому операнду и наоборот */
            if (left.isDerivedFrom(right) || right.isDerivedFrom(left))
            {
                /* привести к одному типу */
                CType castType = TypeCasting(left, right);

                if (castType.Type == EValueType.Integer || castType.Type == EValueType.Real)
                    return castType;

                if (castType.Type == EValueType.String && op == EOperation.Plus)
                    return castType;

                if (castType.Type == EValueType.Boolean && op == EOperation.Or)
                    return castType;
            }
            /* типы неприводимы друг к другу/недопустимая операция с типом */
            errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errTypeMismatch));
            return availableTypes["unknown"];
        }

        /* операнды мультипликативных операций */
        private CType MultiplyOperands(CType left, EOperation op, CType right)
        {
            /* оба типа оказались неизвестными */
            if (left.Type == EValueType.Unknown && right.Type == EValueType.Unknown)
                return left;

            /* типы совпадают или левый операнд приводИм к правому операнду и наоборот */
            if (left.isDerivedFrom(right) || right.isDerivedFrom(left))
            {
                /* привести к одному типу */
                CType castType = TypeCasting(left, right);

                if (castType.Type == EValueType.Integer)
                {
                    if (op == EOperation.Division)
                        return availableTypes["real"];
                    return castType;
                }

                if (castType.Type == EValueType.Real && op != EOperation.Div && op != EOperation.Mod)
                    return castType;

                if (castType.Type == EValueType.Boolean && op == EOperation.And)
                    return castType;
            }
            /* типы неприводимы друг к другу/недопустимая операция с типом */
            errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errTypeMismatch));
            return availableTypes["unknown"];
        }

        /* получить следующий токен */
        private void GetNextToken()
        {
            try
            {
                curToken = lexer.GetNextToken();
            }
            catch (CompilerError err)
            {
                errManager.AddError(err);
                GetNextToken();
            }
        }

        /* проверить, что текущий символ идентификатор или константа */
        private void Accept(ETokenType expectedSymbol)
        {
            if (curToken == null)
                errManager.AddError(new CompilerError(lexer.line, lexer.col, expectedSymbol), true);

            if (curToken.TokenType == expectedSymbol)
                GetNextToken();
            else
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, expectedSymbol), true);
        }

        /* проверить что текущий символ совпадает с ожидаемым символом */
        private void Accept(EOperation expectedSymbol)
        {
            if (curToken == null)
                errManager.AddError(new CompilerError(lexer.line, lexer.col, expectedSymbol), true);

            if (curToken.TokenType == ETokenType.Operation && ((OperationToken)curToken).OperType == expectedSymbol)
                GetNextToken();
            else
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, expectedSymbol), true);
        }

        /* проверить что текущий символ является операцией из переданного списка */
        bool IsOperation(List<EOperation> ops)
        {
            if (curToken == null || curToken.TokenType != ETokenType.Operation) return false;

            foreach (var op in ops)
            {
                if (((OperationToken)curToken).OperType == op)
                    return true;
            }
            return false;
        }

        /* проверить что тип текущего токена совпадает с ожидаемым */
        bool IsIdentOrConst(List<ETokenType> tTypes)
        {
            if (curToken == null) return false;

            foreach (var types in tTypes)
            {
                if (curToken.TokenType == types)
                    return true;
            }
            return false;
        }

        /* пропустить символы пока не встретится один из ожидаемых символов или null */
        void SkipTo(List<ETokenType> idents, List<EOperation> opers)
        {
            while (curToken != null && !IsOperation(opers) && !IsIdentOrConst(idents))
                GetNextToken();
        }

        /* программа */
        void Programma()
        {
            try
            {
                Accept(EOperation.Program);
                Accept(ETokenType.Identifier);
                Accept(EOperation.Semicolon);
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { }, new List<EOperation> { EOperation.Var, EOperation.Begin });
            }

            Block();

            try
            {
                Accept(EOperation.Point);
            }
            catch
            {
                SkipTo(new List<ETokenType> { }, new List<EOperation> { EOperation.Point });
            }
        }

        /* блок */
        void Block()
        {

            VarPart();
            try
            {
                StatementPart();
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { }, new List<EOperation> { EOperation.End, EOperation.Point });
                if (IsOperation(new List<EOperation> { EOperation.End }))
                    GetNextToken();
            }
        }

        /* раздел переменных */
        void VarPart()
        {
            if (IsOperation(new List<EOperation>() { EOperation.Var }))
            {
                GetNextToken();
                do
                {
                    try
                    {
                        SimilarTypeVarPart();
                        Accept(EOperation.Semicolon);
                    }
                    catch (CompilerError)
                    {
                        SkipTo(new List<ETokenType> { ETokenType.Identifier }, new List<EOperation> { EOperation.Semicolon, EOperation.Begin });
                        if (IsOperation(new List<EOperation> { EOperation.Semicolon }))
                            GetNextToken();
                    }

                } while (IsIdentOrConst(new List<ETokenType>() { ETokenType.Identifier }));
            }
        }

        /* описание однотипных переменных */
        void SimilarTypeVarPart()
        {
            /* создать вспомогательный список для добавления новых идентификаторов */
            List<string> tempListVars = new List<string>();

            try
            {
                IdentifierToken ident = curToken as IdentifierToken; 
                Accept(ETokenType.Identifier);
                /* проверить, что идентификатор не объявлен ранее */
                if (tempListVars.Contains(ident.IdentifierName) || variables.ContainsKey(ident.IdentifierName))
                    errManager.AddError(new CompilerError(ident.Line, ident.Col, EErrorType.errDuplicateIdent));
                else
                    tempListVars.Add(ident.IdentifierName);
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { }, new List<EOperation> { EOperation.Comma, EOperation.Semicolon, EOperation.Begin });
            }

            while (IsOperation(new List<EOperation>() { EOperation.Comma }))
            {
                GetNextToken();
                try
                {
                    IdentifierToken ident = curToken as IdentifierToken;
                    Accept(ETokenType.Identifier);
                    /* проверить, что идентификатор не объявлен ранее */
                    if (tempListVars.Contains(ident.IdentifierName) || variables.ContainsKey(ident.IdentifierName))
                        errManager.AddError(new CompilerError(ident.Line, ident.Col, EErrorType.errDuplicateIdent));
                    else
                        tempListVars.Add(ident.IdentifierName);
                }
                catch (CompilerError)
                {
                    SkipTo(new List<ETokenType> { }, new List<EOperation> { EOperation.Comma, EOperation.Semicolon, EOperation.Begin });
                }
            }

            try
            {
                Accept(EOperation.Colon);
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { ETokenType.Identifier }, new List<EOperation> { EOperation.Colon, EOperation.Semicolon, EOperation.Begin });
                if (IsOperation(new List<EOperation> { EOperation.Colon }))
                    GetNextToken();
            }

            IdentifierToken type = curToken as IdentifierToken;
            Accept(ETokenType.Identifier);
            /* внести информацию о идентификаторах в область видимости */
            foreach (var varName in tempListVars)
            {
                try
                {
                    variables.Add(varName, new CVariable(varName, availableTypes[type.IdentifierName]));
                }
                catch
                {
                    errManager.AddError(new CompilerError(type.Line, type.Col, EErrorType.errInType));
                    variables.Add(varName, new CVariable(varName, availableTypes["unknown"]));
                }
            }
        }

        /* раздел операторов */
        void StatementPart()
        {
            CompoundStatement();
        }

        /* составной оператор */
        void CompoundStatement()
        {
            try
            {
                Accept(EOperation.Begin);
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { }, new List<EOperation> { EOperation.Begin, EOperation.End });
                if (IsOperation(new List<EOperation> { EOperation.Begin }))
                    GetNextToken();
            }

            try
            {
                Statement();
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { }, new List<EOperation> { EOperation.Semicolon, EOperation.End });
            }

            while (IsOperation(new List<EOperation>() { EOperation.Semicolon }))
            {
                GetNextToken();
                try
                {
                    Statement();
                }
                catch (CompilerError)
                {
                    SkipTo(new List<ETokenType> { }, new List<EOperation> { EOperation.Semicolon, EOperation.End });
                }
            }

            Accept(EOperation.End);
        }

        /* оператор */
        void Statement()
        {
            if (IsIdentOrConst(new List<ETokenType> { ETokenType.Identifier }))
                AssignmentStatement();

            else if (IsOperation(new List<EOperation> { EOperation.Begin }))
                CompoundStatement();

            else if (IsOperation(new List<EOperation> { EOperation.If }))
                IfStatement();

            else if (IsOperation(new List<EOperation> { EOperation.While }))
                LoopStatement();

            else if (curToken != null && !IsOperation(new List<EOperation> {  EOperation.End }))
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errInStatement), true);
        }

        /* оператор присваивания */
        void AssignmentStatement()
        {
            CType left;
            if (variables.ContainsKey(((IdentifierToken)curToken).IdentifierName))
                left = variables[((IdentifierToken)curToken).IdentifierName].Type;
            else
            {
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errUnknownIdent));
                left = availableTypes["unknown"];
            }
            GetNextToken();
            Accept(EOperation.Assignment);
            var right = Expression();
            /* если правая часть выражения не приводима к левой */
            if (!left.isDerivedFrom(right) && left.Type != EValueType.Unknown)
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errTypeMismatch));
        }

        /* выражение */
        CType Expression()
        {
            var left = SimpleExpression();
            if (IsOperation(new List<EOperation>() { EOperation.Equals, EOperation.NotEquals, EOperation.Less, 
                EOperation.Lesseqv, EOperation.Bigger, EOperation.Bigeqv }))
            {
                GetNextToken();
                var right = SimpleExpression();
                left = CompareOperands(left, right);
            }
            return left;
        }

        /* простое выражение */
        CType SimpleExpression()
        {
            bool sign = false;
            if (IsOperation(new List<EOperation>() { EOperation.Plus, EOperation.Min }))
            {
                sign = true;
                GetNextToken();
            }
            var left = Term();
            if (sign && (left.Type == EValueType.String || left.Type == EValueType.Boolean))
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errTypeMismatch));
            while (IsOperation(new List<EOperation>() { EOperation.Plus, EOperation.Min, EOperation.Or }))
            {
                var operation = ((OperationToken)curToken).OperType;
                GetNextToken();
                var right = Term();
                left = AdditiveOperands(left, operation, right);
            }
            return left;
        }

        /* слагаемое */
        CType Term()
        {
            var left = Factor();
            while (IsOperation(new List<EOperation>() { EOperation.Mul, EOperation.Division, EOperation.Mod, EOperation.Div, EOperation.And }))
            {
                var operation = ((OperationToken)curToken).OperType;
                GetNextToken();
                var right = Factor();
                left = MultiplyOperands(left, operation, right);
            }
            return left;
        }

        /* множитель */
        CType Factor ()
        {
            CType expr = availableTypes["unknown"]; // тип выражения неизвестен

            if (IsIdentOrConst(new List<ETokenType>() { ETokenType.Const }))
            {
                switch (((ConstValueToken)curToken).ConstVal.ValueType)
                {
                    case EValueType.Integer:
                        expr = availableTypes["integer"];
                        break;
                    case EValueType.Real:
                        expr = availableTypes["real"];
                        break;
                    case EValueType.String:
                        expr = availableTypes["string"];
                        break;
                    case EValueType.Boolean:
                        expr = availableTypes["boolean"];
                        break;
                }
                GetNextToken();
            }

            else if (IsIdentOrConst(new List<ETokenType>() { ETokenType.Identifier }))
            {
                if (variables.ContainsKey(((IdentifierToken)curToken).IdentifierName))
                    expr = variables[((IdentifierToken)curToken).IdentifierName].Type;
                else
                    errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errUnknownIdent));
                GetNextToken();
            }

            else if (IsOperation(new List<EOperation>() { EOperation.LeftBracket }))
            {
                GetNextToken();
                expr = Expression();
                Accept(EOperation.RightBracket);
            }

            else if (IsOperation(new List<EOperation>() { EOperation.Not }))
            {
                GetNextToken();
                expr = Factor();
                if (expr.Type != EValueType.Boolean && expr.Type != EValueType.Unknown)
                {
                    errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errTypeMismatch));
                    expr = availableTypes["unknown"];
                }
            }

            /* синтаксическая ошибка */
            else
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errSyntaxError), true);

            return expr;
        }

        /* условной оператор */
        void IfStatement()
        {
            GetNextToken();
            try
            {
                var expr = Expression();
                if (!expr.isDerivedFrom(availableTypes["boolean"]) && expr.Type != EValueType.Unknown)
                    errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errInLogicExpr));
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { ETokenType.Identifier }, new List<EOperation>
                    { EOperation.Then, EOperation.Begin, EOperation.If, EOperation.While, EOperation.Semicolon, EOperation.End });
            }

            try
            {
                Accept(EOperation.Then);
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { ETokenType.Identifier }, new List<EOperation> 
                    { EOperation.Then, EOperation.Begin, EOperation.If, EOperation.While, EOperation.Semicolon, EOperation.End });
                if (IsOperation(new List<EOperation> { EOperation.Then })) 
                    GetNextToken();
            }

            Statement();
            if (IsOperation(new List<EOperation>() { EOperation.Else }))
            {
                GetNextToken();
                Statement();
            }
        }

        /* оператор цикла с предусловием */
        void LoopStatement()
        {
            GetNextToken();
            try
            {
                var expr = Expression();
                if (!expr.isDerivedFrom(availableTypes["boolean"]) && expr.Type != EValueType.Unknown)
                    errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errInLogicExpr));
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { ETokenType.Identifier }, new List<EOperation>
                    { EOperation.Do, EOperation.Begin, EOperation.If, EOperation.While, EOperation.Semicolon, EOperation.End });
            }

            try
            {
                Accept(EOperation.Do);
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { ETokenType.Identifier }, new List<EOperation>
                    { EOperation.Do, EOperation.Begin, EOperation.If, EOperation.While, EOperation.Semicolon, EOperation.End });
                if (IsOperation(new List<EOperation> { EOperation.Do}))
                    GetNextToken();
            }

            Statement();

        }
    }
}
