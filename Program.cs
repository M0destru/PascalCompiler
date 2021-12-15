using System;
using System.IO;

namespace PascalCompiler
{
    /* Информация о программах, на которых можно протестировать работу компилятора
     * factorial.pas - вычисление факториала числа
     * fib.pas - вывод чисел Фибоначчи
     * firstlastdigit.pas - перестановка первой и последней цифр заданного числа 
     * operations_str.pas - операции над строками
     * comp_three_dig.pas - нахождение наибольшего числа из трёх
     * divisionZero.pas - деление и целочисленное деление на 0
     * logicoper.pas - операции сравнения двух чисел
     * logicfunc.pas - найти значение логической функции
     * equationsystem.pas - решение системы трёх линейных уравнений с тремя неизвестными
     * tablemult.pas - вывести таблицу умножения до 10
     */

    class Program
    {
        public static void Main()
        {
            CGenerator generator = new CGenerator();
            generator.CompileMsil("C:\\Users\\Rustam-PC\\source\\repos\\Pascal_Compiler\\bin\\Debug\\Тесты\\tablemult.pas", "./output.txt");
        }
    }
}


