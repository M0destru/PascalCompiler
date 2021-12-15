using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace PascalCompiler
{
    /* компилятор */
    class CCompiler
    {
        CLexer lexer;
        CErrorManager errManager;
        CToken curToken;
        Dictionary<string, CType> availableTypes; // допустимые типы
        Dictionary<string, CVariable> variables; // область переменных
        ILGenerator il; // генератор IL-кодов
        StreamReader reader;
        StreamWriter writer;

        public CCompiler(string inFileName, string outFileName, ILGenerator il)
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

            this.il = il;
        }

        /* запустить процесс компиляции */
        public bool RunCompilation ()
        {
            GetNextToken();
            Programma();
            writer.WriteLine($"\nКоличество ошибок: {errManager.GetNumOfErrors()}");
            /* закрытие файлов */
            reader.Close();
            writer.Close();
            return errManager.GetNumOfErrors() == 0;

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
                il.BeginScope();
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
                il.EndScope();
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
                    if (availableTypes[type.IdentifierName].Type == EValueType.Integer)
                    {
                        variables.Add(varName, new CVariable(varName, availableTypes[type.IdentifierName]));
                        if (errManager.GetNumOfErrors() == 0)
                            variables[varName].Lb = il.DeclareLocal(typeof(int));
                    }
                    else if (availableTypes[type.IdentifierName].Type == EValueType.Real)
                    {
                        variables.Add(varName, new CVariable(varName, availableTypes[type.IdentifierName]));
                        if (errManager.GetNumOfErrors() == 0)
                            variables[varName].Lb = il.DeclareLocal(typeof(double));
                    }
                    else if (availableTypes[type.IdentifierName].Type == EValueType.String)
                    {
                        variables.Add(varName, new CVariable(varName, availableTypes[type.IdentifierName]));
                        if (errManager.GetNumOfErrors() == 0)
                            variables[varName].Lb = il.DeclareLocal(typeof(string));
                    }
                    else if (availableTypes[type.IdentifierName].Type == EValueType.Boolean)
                    {
                        variables.Add(varName, new CVariable(varName, availableTypes[type.IdentifierName]));
                        if (errManager.GetNumOfErrors() == 0)
                            variables[varName].Lb = il.DeclareLocal(typeof(bool));
                    }

                }
                catch
                {
                    errManager.AddError(new CompilerError(type.Line, type.Col, EErrorType.errInType));
                    variables.Add(varName, new CVariable(varName, availableTypes["unknown"], null));
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

            else if (IsOperation(new List<EOperation> { EOperation.Writeln, EOperation.Write }))
                OutputStatement();

            else if (curToken != null && !IsOperation(new List<EOperation> { EOperation.End }))
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errInStatement), true);
        }

        /* оператор присваивания */
        void AssignmentStatement()
        {
            CType left;
            IdentifierToken identToken = (IdentifierToken)curToken;
            if (variables.ContainsKey(identToken.IdentifierName))
                left = variables[identToken.IdentifierName].Type;
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
            /* извлечь из стека верхнее значение и поместить в переменную */
            if (errManager.GetNumOfErrors() == 0)
            {
                if (left.Type == EValueType.Real && right.Type == EValueType.Integer)
                    il.Emit(OpCodes.Conv_R8);
                il.Emit(OpCodes.Stloc, variables[identToken.IdentifierName].Lb);
            }
        }

        /* выражение */
        CType Expression()
        {
            var left = SimpleExpression();
            if (IsOperation(new List<EOperation>() { EOperation.Equals, EOperation.NotEquals, EOperation.Less,
                EOperation.Lesseqv, EOperation.Bigger, EOperation.Bigeqv }))
            {
                var operation = ((OperationToken)curToken).OperType;
                GetNextToken();
                var right = SimpleExpression(left);
                left = CompareOperands(left, right);

                if (errManager.GetNumOfErrors() == 0)
                {
                    if (operation == EOperation.Equals || operation == EOperation.NotEquals)
                    {
                        il.Emit(OpCodes.Ceq);
                        if (operation == EOperation.NotEquals)
                        {
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.Sub);
                            il.Emit(OpCodes.Neg);
                        }
                    }
                    else if (operation == EOperation.Less || operation == EOperation.Bigeqv)
                    {
                        il.Emit(OpCodes.Clt);
                        if (operation == EOperation.Bigeqv)
                        {
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.Sub);
                            il.Emit(OpCodes.Neg);
                        }
                    }
                    else if (operation == EOperation.Bigger || operation == EOperation.Lesseqv)
                    {
                        il.Emit(OpCodes.Cgt);
                        if (operation == EOperation.Lesseqv)
                        {
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.Sub);
                            il.Emit(OpCodes.Neg);
                        }
                    }
                }
            }
            return left;
        }

        /* простое выражение */
        CType SimpleExpression(CType prevType = null)
        {
            int sign = 0; // 0: нет знака, 1: +, 2: -
            if (IsOperation(new List<EOperation> { EOperation.Plus, EOperation.Min }))
            {
                if (IsOperation(new List<EOperation> { EOperation.Plus }))
                    sign = 1;
                else
                    sign = 2;
                GetNextToken();
            }
            var left = Term(prevType);
            if (sign > 0 && (left.Type == EValueType.String || left.Type == EValueType.Boolean))
                errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errTypeMismatch));
            /* инверсировать значение, если был знак '-' перед слагаемым */
            if (errManager.GetNumOfErrors() == 0 && sign == 2)
                il.Emit(OpCodes.Neg);
            while (IsOperation(new List<EOperation>() { EOperation.Plus, EOperation.Min, EOperation.Or }))
            {
                var operation = ((OperationToken)curToken).OperType;
                GetNextToken();
                var right = Term(left);
                left = AdditiveOperands(left, operation, right);

                if (errManager.GetNumOfErrors() > 0)
                    continue;

                if (operation == EOperation.Plus)
                {
                    if (left.Type == EValueType.String)
                    {
                        /* конкатенация строк */
                        il.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));
                    }
                    else
                        il.Emit(OpCodes.Add);
                }
                else if (operation == EOperation.Min)
                {
                    il.Emit(OpCodes.Sub);
                }
                else if (operation == EOperation.Or)
                {
                    il.Emit(OpCodes.Or);
                }

            }
            return left;
        }

        /* слагаемое */
        CType Term(CType prevType)
        {
            var left = Factor(prevType);
            while (IsOperation(new List<EOperation>() { EOperation.Mul, EOperation.Division, EOperation.Mod, EOperation.Div, EOperation.And }))
            {
                var operation = ((OperationToken)curToken).OperType;
                GetNextToken();
                var right = operation == EOperation.Division? Factor(left, true): Factor(left);

                left = MultiplyOperands(left, operation, right);

                if (errManager.GetNumOfErrors() > 0)
                    continue;

                if (operation == EOperation.Mul)
                {
                    il.Emit(OpCodes.Mul);
                }
                else if (operation == EOperation.Division || operation == EOperation.Div)
                {
                    il.Emit(OpCodes.Div);
                }
                else if (operation == EOperation.Mod)
                {
                    il.Emit(OpCodes.Rem);
                }
                else if (operation == EOperation.And)
                {
                    il.Emit(OpCodes.And);
                }

            }
            return left;
        }

        /* множитель */
        CType Factor(CType prevType, bool floatDivision = false)
        {
            CType expr = availableTypes["unknown"]; // тип выражения неизвестен

            if (IsIdentOrConst(new List<ETokenType>() { ETokenType.Const }))
            {
                ConstValueToken constToken = (ConstValueToken)curToken;
                switch (constToken.ConstVal.ValueType)
                {
                    case EValueType.Integer:
                        var intConst = (IntegerVariant)constToken.ConstVal;
                        expr = availableTypes["integer"];
                        if (errManager.GetNumOfErrors() == 0)
                        {
                            if (prevType != null && prevType.Type == EValueType.Integer && floatDivision)
                                il.Emit(OpCodes.Conv_R8);
                            il.Emit(OpCodes.Ldc_I4, intConst.IntegerValue);
                            if (prevType != null && (prevType.Type == EValueType.Real || floatDivision))
                                il.Emit(OpCodes.Conv_R8);

                        }
                        break;
                    case EValueType.Real:
                        var realConst = (RealVariant)constToken.ConstVal;
                        expr = availableTypes["real"];
                        if (errManager.GetNumOfErrors() == 0)
                        {
                            if (prevType != null && prevType.Type == EValueType.Integer)
                                il.Emit(OpCodes.Conv_R8);
                            il.Emit(OpCodes.Ldc_R8, realConst.RealValue);
                        }
                        break;
                    case EValueType.String:
                        var stringConst = (StringVariant)constToken.ConstVal;
                        expr = availableTypes["string"];
                        if (errManager.GetNumOfErrors() == 0)
                        {
                            il.Emit(OpCodes.Ldstr, stringConst.StringValue);
                        }
                        break;
                    case EValueType.Boolean:
                        var boolConst = (BooleanVariant)constToken.ConstVal;
                        expr = availableTypes["boolean"];
                        if (errManager.GetNumOfErrors() == 0)
                        {
                            if (!boolConst.BoolValue)
                                il.Emit(OpCodes.Ldc_I4_0);
                            else
                                il.Emit(OpCodes.Ldc_I4_1);
                        }
                        break;
                }
                GetNextToken();
            }

            else if (IsIdentOrConst(new List<ETokenType>() { ETokenType.Identifier }))
            {
                IdentifierToken identToken = (IdentifierToken)curToken;
                if (variables.ContainsKey(identToken.IdentifierName))
                {
                    expr = variables[identToken.IdentifierName].Type;
                    if (errManager.GetNumOfErrors() == 0)
                    {
                        if (prevType != null && prevType.Type == EValueType.Integer && (expr.Type == EValueType.Real || floatDivision))
                        {
                            il.Emit(OpCodes.Conv_R8);
                        }
                        il.Emit(OpCodes.Ldloc, variables[identToken.IdentifierName].Lb);
                        if (prevType != null && (prevType.Type == EValueType.Real || floatDivision) && expr.Type == EValueType.Integer)
                        {
                            il.Emit(OpCodes.Conv_R8);
                        }
                    }
                }
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
                expr = Factor(prevType);
                if (expr.Type != EValueType.Boolean && expr.Type != EValueType.Unknown)
                {
                    errManager.AddError(new CompilerError(curToken.Line, curToken.Col, EErrorType.errTypeMismatch));
                    expr = availableTypes["unknown"];
                }
                else if (errManager.GetNumOfErrors() == 0)
                {
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Sub);
                    il.Emit(OpCodes.Neg);
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

            /* метки */
            Label falseLabel = il.DefineLabel();
            Label trueLabel = il.DefineLabel();
            Label endLabel = il.DefineLabel();

            if (errManager.GetNumOfErrors() == 0)
            {
                il.Emit(OpCodes.Brtrue, trueLabel); // if true
                il.Emit(OpCodes.Br, falseLabel); // if false

                il.MarkLabel(trueLabel);
                Statement();

                il.Emit(OpCodes.Br, endLabel);
                il.MarkLabel(falseLabel);
            }

            if (IsOperation(new List<EOperation>() { EOperation.Else }))
            {
                GetNextToken();
                Statement();
            }

            if (errManager.GetNumOfErrors() == 0)
            {
                il.Emit(OpCodes.Br, endLabel);
                il.MarkLabel(endLabel);
            }
        }

        /* оператор цикла с предусловием */
        void LoopStatement()
        {
            GetNextToken();
            /* метки */
            Label loopCondition = il.DefineLabel();
            Label loopBody = il.DefineLabel();
            Label loopEnd = il.DefineLabel();

            if (errManager.GetNumOfErrors() == 0)
            {
                il.Emit(OpCodes.Br, loopCondition);
                il.MarkLabel(loopCondition);
            }

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
                if (IsOperation(new List<EOperation> { EOperation.Do }))
                    GetNextToken();
            }

            if (errManager.GetNumOfErrors() == 0)
            {
                il.Emit(OpCodes.Brtrue, loopBody); // выражение true
                il.Emit(OpCodes.Br, loopEnd); // выражение false
                il.MarkLabel(loopBody);
                Statement();

                il.Emit(OpCodes.Br, loopCondition);

                il.MarkLabel(loopEnd);
            }

        }
        /* вывод в консоль */
        void OutputStatement()
        {
            string methodName = IsOperation(new List<EOperation> { EOperation.Writeln }) ? "WriteLine" : "Write";
            GetNextToken();
            try
            {
                Accept(EOperation.LeftBracket);
                CType expr = null;
                if (!IsOperation (new List<EOperation> { EOperation.RightBracket }))
                    expr = Expression();
                Accept(EOperation.RightBracket);
                if (errManager.GetNumOfErrors() == 0)
                {
                    if (expr == null)
                        il.Emit(OpCodes.Call, typeof(Console).GetMethod(methodName, new Type[] { }));
                    else
                    {
                        switch (expr.Type)
                        {
                            case EValueType.Integer:
                                il.Emit(OpCodes.Call, typeof(Console).GetMethod(methodName, new Type[] { typeof(int) }));
                                break;
                            case EValueType.Real:
                                il.Emit(OpCodes.Call, typeof(Console).GetMethod(methodName, new Type[] { typeof(double) }));
                                break;
                            case EValueType.String:
                                il.Emit(OpCodes.Call, typeof(Console).GetMethod(methodName, new Type[] { typeof(string) }));
                                break;
                            case EValueType.Boolean:
                                il.Emit(OpCodes.Call, typeof(Console).GetMethod(methodName, new Type[] { typeof(bool) }));
                                break;
                        }
                    }
                }
            }
            catch (CompilerError)
            {
                SkipTo(new List<ETokenType> { ETokenType.Identifier }, new List<EOperation> { EOperation.Semicolon, EOperation.End });
            }
        }
    }
}

