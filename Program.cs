using System;
using System.IO;
using System.Collections.Generic;

namespace PascalCompiler
{
    class Program
    {
        public static StreamWriter sw;

        public static void Main()
        {
            sw = new StreamWriter("./output.txt");

            Lexer io = new Lexer("./main.pas");
            CToken cur = null;
            List<Error> errorLst = new List<Error>();
            do
            {
                try
                {
                    cur = io.GetNextToken();
                }
                catch (Error err)
                {
                    errorLst.Add(err);
                    sw.WriteLine(err);
                    continue;
                }
                if (cur != null)
                    Console.Write(cur.ToString());
            } while (cur != null);

            if (errorLst.Count > 0)
                sw.WriteLine($"\nTotal Errors: {errorLst.Count}");
            else
                sw.WriteLine("\nNo Errors");

            sw.Close();
        }
    }
}

