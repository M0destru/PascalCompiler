using System;
using System.Collections.Generic;
using System.IO;

namespace PascalCompiler
{
    class CCompiler
    {
        StreamReader reader;
        StreamWriter writer;
        Lexer lexer;
        CToken curToken;

        public CCompiler(string inFileName, string outFileName)
        {
            reader = new StreamReader(inFileName);
            writer = new StreamWriter(outFileName);
            lexer = new Lexer(reader, writer);
            try
            {
                GetNextToken();
                Programma();
            }
            catch (Error err)
            {
                writer.WriteLine(err);
            }
            reader.Close();
            writer.Close();
        }

        /* получить следующий токен */
        private void GetNextToken()
        {
            curToken = lexer.GetNextToken();
        }

        /* проверить, что текущий символ идентификатор или константа */
        private void Accept(ETokenType expectedSymbol)
        {
            if (curToken == null) throw new Error(lexer.line, lexer.col - 1, expectedSymbol);

            if (curToken.TokenType == expectedSymbol)
                GetNextToken();
            else
                throw new Error(lexer.line, lexer.col - curToken.ToString().Length, expectedSymbol);
        }

        /* проверить что текущий символ совпадает с ожидаемым символом */
        private void Accept(EOperation expectedSymbol)
        {
            if (curToken == null) throw new Error(lexer.line, lexer.col - 1, expectedSymbol);

            if (curToken.TokenType == ETokenType.Operation && ((OperationToken)curToken).OperType == expectedSymbol)
                GetNextToken();
            else
                throw new Error(lexer.line, lexer.col - curToken.ToString().Length, expectedSymbol);
        }

        /* проверить что текущий символ является операцией из переданного списка */
        bool IsOperation(List<EOperation> ops)
        {
            if (curToken != null && curToken.TokenType == ETokenType.Operation)
            {
                foreach (var op in ops)
                {
                    if (((OperationToken)curToken).OperType == op)
                        return true;
                }
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
            TypePart();
            VarPart();
            StatementPart();
        }

        /* раздел типов */
        void TypePart()
        {
            if (IsOperation(new List<EOperation>() { EOperation.Type }))
            {
                GetNextToken();
                do
                {
                    TypeDefinition();
                    Accept(EOperation.Semicolon);
                }
                while (IsIdentOrConst(new List<ETokenType>() { ETokenType.Identifier }));
            }
        }

        /* определение типа */
        void TypeDefinition()
        {
            GetNextToken();
            Accept(EOperation.Equals);
            Type();
        }

        /* тип */
        void Type()
        {
            SimpleType();
        }

        /* простой тип */
        void SimpleType()
        {
            TypeName();
        }

        /* имя типа */
        void TypeName()
        {
            Accept(ETokenType.Identifier);
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

        /* <описание однотипных переменных> */
        void SimilarTypeVarPart()
        {
            Accept(ETokenType.Identifier);
            while (IsOperation(new List<EOperation>() { EOperation.Comma }))
            {
                GetNextToken();
                Accept(ETokenType.Identifier);
            }
            Accept(EOperation.Colon);
            Type();
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
            if (IsIdentOrConst(new List<ETokenType>() { ETokenType.Identifier }))
            {
                SimpleStatement();
            }
            else if (IsOperation(new List<EOperation>() { EOperation.Begin, EOperation.If, EOperation.While }))
            {
                ComplexStatement();
            }
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
                СompoundStatement();
            else if (IsOperation(new List<EOperation>() { EOperation.If }))
                ConditionalStatement();
            else
                LoopStatement();
        }

        /* оператор присваивания */
        void AssignmentStatement()
        {
            Accept(ETokenType.Identifier);
            Accept(EOperation.Assignment);
            Expression();
        }

        /* выражение */
        void Expression()
        {
            SimpleExpression();
            if (IsOperation(new List<EOperation>() { EOperation.Equals, EOperation.NotEquals, EOperation.Less, 
                EOperation.Lesseqv, EOperation.Bigger, EOperation.Bigeqv, EOperation.In }))
            {
                GetNextToken();
                SimpleExpression();
            }
        }

        /* простое выражение */
        void SimpleExpression()
        {
            if (IsOperation(new List<EOperation>() { EOperation.Plus, EOperation.Min }))
                GetNextToken();
            Term();
            while (IsOperation(new List<EOperation>() { EOperation.Plus, EOperation.Min, EOperation.Or }))
            {
                GetNextToken();
                Term();
            }

        }

        /* слагаемое */
        void Term()
        {
            Factor();
            while (IsOperation(new List<EOperation>() { EOperation.Mul, EOperation.Division, EOperation.Mod, EOperation.Div, EOperation.And }))
            {
                GetNextToken();
                Factor();
            }
        }

        /* множитель */
        void Factor ()
        {
            if (IsIdentOrConst(new List<ETokenType>() { ETokenType.Identifier, ETokenType.Const }) || IsOperation(new List<EOperation>() { EOperation.Nil }) )
                GetNextToken();
            else if (IsOperation(new List<EOperation>() { EOperation.LeftBracket }))
            {
                GetNextToken();
                Expression();
                Accept(EOperation.RightBracket);
            }
            else if (IsOperation(new List<EOperation>() { EOperation.Not }))
            {
                GetNextToken();
                Factor();
            }
        }

        /* составной оператор */
        void СompoundStatement()
        {
            GetNextToken();
            Statement();
            while (IsOperation(new List<EOperation>() { EOperation.Semicolon }))
            {
                GetNextToken();
                Statement();
            }
            Accept(EOperation.End);
        }
        
        /* условной оператор */
        void ConditionalStatement()
        {
            GetNextToken();
            Expression();
            Accept(EOperation.Then);
            Statement();
            if (IsOperation(new List<EOperation>() { EOperation.Else}))
            {
                GetNextToken();
                Statement();
            }
        }

        /* оператор цикла */
        void LoopStatement()
        {
            GetNextToken();
            Expression();
            Accept(EOperation.Do);
            Statement();
        }
    }
}
