using System;
using System.IO;
using System.Collections.Generic;

namespace PascalCompiler
{
    class Program
    {
        public static void Main()
        {
            CCompiler сompiler = new CCompiler("./prog2.pas", "./output.txt");
        }
    }
}

