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
            lexer = new CLexer(reader, writer);
            errManager = new CErrorManager(writer);
            availableTypes = new Dictionary<string, CType>
            {
                ["integer"] = new CIntType(),
                ["real"] = new CRealType(),
                ["string"] = new CStringType(),
                ["boolean"] = new CBooleanType(),
                ["unknown"] = new CUnknownType()
            };
            variables = new Dictionary<string, CVariable>();
            try
            {
                GetNextToken();
                Programma();
            }
            catch (CompilerError)
            {

            }
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
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, expectedSymbol), true);

            if (curToken.TokenType == expectedSymbol)
                GetNextToken();
            else
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, expectedSymbol), true);
        }

        /* проверить что текущий символ совпадает с ожидаемым символом */
        private void Accept(EOperation expectedSymbol)
        {
            if (curToken == null)
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, expectedSymbol), true);

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

        //void SkipTo (List <ETokenType> idents, List<EOperation> opers)
        //{
        //    while (curToken != null && !IsOperation(opers) && !IsIdentOrConst(idents))
        //        GetNextToken();
        //}

        /* программа */
        void Programma()
        {
            Accept(EOperation.Program);
            Accept(ETokenType.Identifier);
            Accept(EOperation.Semicolon);
            Block();
            Accept(EOperation.Point);
        }

        /* блок */
        void Block()
        {
            VarPart();
            StatementPart();
        }

        /* раздел переменных */
        void VarPart()
        {
            if (IsOperation(new List<EOperation>() { EOperation.Var }))
            {
                GetNextToken();
                do
                {
                    SimilarTypeVarPart();
                    Accept(EOperation.Semicolon);

                } while (IsIdentOrConst(new List<ETokenType>() { ETokenType.Identifier }));
            }
        }

        /* описание однотипных переменных */
        void SimilarTypeVarPart()
        {
            /* создать вспомогательный список для добавления новых идентификаторов */
            List<string> tempListVars = new List<string>();

            if (IsIdentOrConst(new List<ETokenType> { ETokenType.Identifier }))
            {
                string name = ((IdentifierToken)curToken).IdentifierName;
                /* проверить, что идентификатор не объявлен ранее */
                if (tempListVars.Contains(name) || variables.ContainsKey(name))
                    errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errDuplicateIdent));
                else
                    tempListVars.Add(name);
                GetNextToken();
            }
            /* else - пробросить синтаксическую ошибку */

            while (IsOperation(new List<EOperation>() { EOperation.Comma }))
            {
                GetNextToken();
                if (IsIdentOrConst(new List<ETokenType> { ETokenType.Identifier }))
                {
                    string name = ((IdentifierToken)curToken).IdentifierName;
                    /* проверить, что идентификатор не объявлен ранее */
                    if (tempListVars.Contains(name) || variables.ContainsKey(name))
                        errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errDuplicateIdent));
                    else
                        tempListVars.Add(name);
                    GetNextToken();
                }
                /* else - пробросить синтаксическую ошибку */
            }

            Accept(EOperation.Colon);
            if (IsIdentOrConst(new List<ETokenType> { ETokenType.Identifier }))
            {
                /* внести информацию о идентификаторах в область видимости */
                string type = ((IdentifierToken)curToken).IdentifierName;
                foreach (var varName in tempListVars)
                {
                    try
                    {
                        variables.Add(varName, new CVariable(varName, availableTypes[type]));
                    }
                    catch
                    {
                        errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errInType));
                        variables.Add(varName, new CVariable(varName, availableTypes["unknown"]));
                    }
                }
                GetNextToken();
            }
            /* else - пробросить синтаксическую ошибку */
        }

        /* раздел операторов */
        void StatementPart()
        {
            CompoundStatement();
        }

        /* составной оператор */
        void CompoundStatement()
        {
            Accept(EOperation.Begin);
            Statement();
            while (IsOperation(new List<EOperation>() { EOperation.Semicolon }))
            {
                GetNextToken();
                Statement();
            }
            Accept(EOperation.End);
        }

        /* оператор */
        void Statement()
        {
            if (IsIdentOrConst(new List<ETokenType> { ETokenType.Identifier }))
            {
                SimpleStatement();
            }
            else if (IsOperation(new List<EOperation> { EOperation.Begin, EOperation.If, EOperation.While }))
            {
                ComplexStatement();
            }
            else if (!IsOperation(new List<EOperation> { EOperation.End }))
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errInStatement));
        }

        /* простой оператор */
        void SimpleStatement ()
        {
            AssignmentStatement();
        }

        /* сложный оператор */
        void ComplexStatement()
        {
            if (IsOperation(new List<EOperation>() { EOperation.Begin }))
                CompoundStatement();
            else if (IsOperation(new List<EOperation>() { EOperation.If }))
                IfStatement();
            else
                LoopStatement();
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

            return expr;
        }

        /* условной оператор */
        void IfStatement()
        {
            GetNextToken();
            var expr = Expression();
            if (!expr.isDerivedFrom(availableTypes["boolean"]) && expr.Type != EValueType.Unknown)
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errInLogicExpr));
            Accept(EOperation.Then);
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
            var expr = Expression();
            if (!expr.isDerivedFrom(availableTypes["boolean"]) && expr.Type != EValueType.Unknown)
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errInLogicExpr));
            Accept(EOperation.Do);
            Statement();
        }
    }
}
