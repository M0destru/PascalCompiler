using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

namespace PascalCompiler
{
    /* генератор */
    class CGenerator
    {
        AppDomain aDomain; // домен приложения
        AssemblyName aName; // имя сборки
        AssemblyBuilder aBuilder; // билдер для сборки
        ModuleBuilder mBuilder; // билдер для модуля
        TypeBuilder tBulider; // билдер типа
        MethodBuilder methodBuilder; // билдер для метода
        ILGenerator il; // генератор IL-кодов
        CCompiler сompiler; // компилятор

        public CGenerator()
        {
            /* получить домен приложения */
            aDomain = Thread.GetDomain();
            /* определить сборку */
            aName = new AssemblyName("Compiler"); 
            aName.Version = new Version("1.0");
            aBuilder = aDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave, Directory.GetCurrentDirectory());
            /* определить модуль и тип */
            mBuilder = aBuilder.DefineDynamicModule(aName.Name, aName.Name + ".exe");
            tBulider = mBuilder.DefineType("Compiler", TypeAttributes.Public);
            /* определить метод */
            methodBuilder = tBulider.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new Type[] { });
            /* создать ILGenerator */
            il = methodBuilder.GetILGenerator();
            /* установить точку входа */
            aBuilder.SetEntryPoint(methodBuilder, PEFileKinds.ConsoleApplication);
        }

        public void CompileMsil(string InFilePath, string OutFilePath)
        {
            сompiler = new CCompiler(InFilePath, OutFilePath, il);
            /* запустить процесс компиляции */
            bool compIsOver = сompiler.RunCompilation();
            il.Emit(OpCodes.Ret);
            /* создать и сохранить сборку, если во время компиляции не было ошибок */
            if (compIsOver)
            {
                tBulider.CreateType();
                aBuilder.Save(aName.Name + ".exe");
            }
            else
            {
                Console.WriteLine($"Ошибки во время компиляции. Информация об ошибках: { Directory.GetCurrentDirectory()}\\output.txt\nНажмите любую клавишу, чтобы закрыть");
                Console.ReadKey();
            }
        }
    }

}
