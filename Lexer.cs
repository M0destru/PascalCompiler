using System;
using System.IO;

namespace PascalCompiler
{
    class Lexer
    {
        public int line; // номер строки
        public int col; // номер символа
        string buf; // считанная строка кода
        char curChar; // текущая литера
        StreamReader reader; // ввод символов из файла
        StreamWriter writer; // вывод текста программы в выходной файл
        CToken curToken; // текущая сформированная лексема

        public Lexer(StreamReader reader, StreamWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
            buf = "";
            line = col = 0;
            GetNextChar();
        }

        /* получить следующую литеру */
        private void GetNextChar()
        {
            /* буфер пуст */
            if (col == buf.Length)
            {
                string str = reader.ReadLine();
                if (str != null)
                {
                    writer.WriteLine($"{line + 1,4}. {str}");
                    buf = str + '\n';
                    line++;
                    col = 0;
                }
                else 
                    buf += "\0";
            }
            curChar = buf[col++];        
        }

        /* откат к предыдущей литере */
        private char GetPrevChar()
        {
            if (col > 0)
                return buf[--col - 1];
            else
                return ' ';
        }

        /* выдать ошибку и перейти к следующей литере */
        private void ThrowError(int errLine, int errCol, EErrorType errType)
        {
            GetNextChar();
            throw new Error(errLine, errCol, errType);
        }

        /* найти конец текущей лексемы */
        private string SearchCurLexem(Predicate<string> condition)
        {
            string curLexem = "";
            do
            {
                curLexem += curChar;
                GetNextChar();
            }
            while (condition(curLexem));
            return curLexem;
        }

        /* получить следующий токен */
        public CToken GetNextToken()
        {
            /* если достигнут конец файла */
            if (curChar == '\0')
            {
                reader.Close();
                return null;
            }

            /* символы пробел и перенос строки */
            if (curChar == ' ' || curChar == '\n')
            {
                GetNextChar();
                return GetNextToken();
            }

            /* блок комментариев */
            if (curChar == '{')
            {
                /* пока не встретилась закрывающая фигурная скобка или символ конца файла */
                SearchCurLexem(lex => curChar != '}' && curChar != '\0');

                if (curChar != '}')
                    ThrowError(line, col - 1, EErrorType.errEOF);
                GetNextChar();
                return GetNextToken();
            }

            /* числовая константа */
            else if (Char.IsDigit(curChar))
            {
                string wholePart = "", fractPart = "";
                /* пока следующая литера является цифрой */
                wholePart = SearchCurLexem(lex => char.IsDigit(curChar));
                /* если текущей литерой является символ '.' */
                if (curChar == '.')
                {
                    /* добавить '.' к вещественной части числа */
                    fractPart += curChar;
                    GetNextChar();
                    /* если текущая литера снова '.', то встретился символ '..' */
                    if (curChar == '.')
                    {
                        curChar = GetPrevChar();
                        fractPart = ""; // целое число, т.е. веществ. части нет
                    }
                    /* разбор вещественной константы */
                    else
                    {
                        /* пока следующая литера является цифрой */
                        fractPart += SearchCurLexem(lex => char.IsDigit(curChar));
                        /* если текущая литера 'e' */
                        if (curChar == 'e' || curChar == 'E')
                        {
                            /* добавить литеру к вещественной части */
                            fractPart += curChar;
                            GetNextChar();
                            /* если литера является знаком '-'/'+' или цифрой */
                            if (curChar == '-' || curChar == '+' || char.IsDigit(curChar))
                                fractPart += SearchCurLexem(lex => char.IsDigit(curChar));
                        }

                        /* проверка считанной вещественной константы */
                        double realNum;
                        if (double.TryParse((wholePart + fractPart).Replace('.', ','), out realNum))
                            curToken = new ConstValueToken(realNum);
                        else if (double.MaxValue < realNum)
                            ThrowError(line, col - (wholePart + fractPart).Length, EErrorType.errInRealConst);
                        else
                            ThrowError(line, col, EErrorType.errUnknownLexem);
                    }
                }
                /* если отсутствует вещественная часть, то разбор целочисленной константы */
                if (fractPart == "")
                {
                    int intNum;
                    if (int.TryParse(wholePart, out intNum))
                        curToken = new ConstValueToken(intNum);
                    else
                        ThrowError(line, col - wholePart.Length, EErrorType.errInIntegerConst);
                }
            }

            /* идентификатор или ключевое слово */
            else if (Char.IsLetter(curChar) || curChar == '_')
            {
                /* пока в тексте встречается цифра/буква/'_' */
                string name = SearchCurLexem(lex => char.IsLetterOrDigit(curChar) || curChar == '_').ToLower();

                /* поиск идентификатора в словаре ключевых слов */
                if (CToken.operationMap.ContainsKey(name))
                    curToken = new OperationToken(CToken.operationMap[name], name);
                else
                    curToken = new IdentifierToken(name);
            }

            /* строковая константа */
            else if (curChar == '\'')
            {
                int startLine = line, startCol = col;
                /* пока в тексте не встретится закрывающая кавычка/символы переноса строки или конца файла */
                string strConst = SearchCurLexem(lex => curChar != '\'' && curChar != '\n');

                /* если не встретилась закрывающая кавычка или длина строковой константы >= 255 */
                if (curChar != '\'' || strConst.Length > 255)
                    ThrowError(startLine, startCol, EErrorType.errEOF);

                curToken = new ConstValueToken(strConst + curChar);
                GetNextChar();
            }

            /* оператор или строка комментариев */
            else if (CToken.operationMap.ContainsKey(curChar.ToString()))
            {
                /* пока добавление следующей литеры приводит к образованию составного оператора */
                string oper = SearchCurLexem(lex => CToken.operationMap.ContainsKey(lex + curChar));
                /* строка комментариев */
                if (oper + curChar == "//")
                {
                    SearchCurLexem(lex => curChar != '\n');
                    return GetNextToken();
                }
                /* оператор */
                curToken = new OperationToken(CToken.operationMap[oper], oper);
            }

            /* неизвестная лексема */
            else
                ThrowError(line, col, EErrorType.errUnknownLexem);

            return curToken;
        }

    }
}