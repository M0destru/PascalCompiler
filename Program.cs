using System;

namespace PascalCompiler
{
    class Program
    {
        public static void Main()
        {
            CIO io = new CIO("./main.pas");
            CToken cur = null;
            do
            {
                cur = io.GetNextToken();
                if (cur != null)
                    Console.WriteLine(cur.ToString());
            } while (cur != null);
        }
    }
}

