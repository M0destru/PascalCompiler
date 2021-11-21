using System;
using System.IO;
using System.Collections.Generic;

namespace PascalCompiler
{
    class Program
    {
        public static void Main()
        {
            CCompiler сompiler = new CCompiler("./prog.pas", "./output.txt");
        }
    }
}

