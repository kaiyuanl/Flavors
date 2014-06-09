//Name:         Flavors
//Author:       Kaiyuan Liang
//Description:  A utility to determine if assemblies were built as Release or Debug

namespace Flavors
{
    using System;
    using System.Reflection;
    using System.IO;

    class Program
    {
        static class Result
        {
            public static string Debug = "Debug";
            public static string Release = "Release";
            public static string Unknown = "Unknown";
        }

        static string GetFlavor(string file)
        {
            try
            {
                file = Path.GetFullPath(file);
                Assembly assembly = Assembly.LoadFile(file);
                foreach (object attribute in assembly.GetCustomAttributes(false))
                {
                    if (attribute.GetType() == Type.GetType("System.Diagnostics.DebuggableAttribute"))
                    {
                        if(((System.Diagnostics.DebuggableAttribute)attribute).IsJITTrackingEnabled)
                        {
                            return Result.Debug;
                        }
                    }
                }
                return Result.Release;
            }
            catch(Exception ex)
            {
                return string.Format("{0} : {1}", Result.Unknown, ex.Message);
            }
        }

        static bool IsDir(string path)
        {
            return Directory.Exists(path);
        }

        static bool IsFile(string path)
        {
            return File.Exists(path);
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Program.Helper();
                return;
            }

            foreach(string path in args)
            {
                if (Program.IsDir(path))
                {
                    Console.WriteLine("-----Directory {0}-----", Path.GetFullPath(path));
                    foreach(string file in Directory.GetFiles(path))
                    {
                        PrintResult(file, GetFlavor(file));
                    }
                    Console.WriteLine("");
                }
                else if (Program.IsFile(path))
                {
                    PrintResult(path, GetFlavor(path));
                }
                else //Wildcard under current directory
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
                    FileInfo[] files = dirInfo.GetFiles(path);
                    if (files.Length == 0)
                    {
                        Console.WriteLine("Cannot find file or directory {0}", path);
                    }
                    else 
                    {
                        foreach(FileInfo fileInfo in files)
                        {
                            PrintResult(fileInfo.FullName, GetFlavor(fileInfo.FullName));
                        }
                    }
                }
            }
        }

        static void PrintResult(string file, string result)
        {
            Console.WriteLine("{0}\t\t\t{1}", Path.GetFullPath(file), result);
        }

        static void Helper()
        {
            Console.WriteLine("Flavors - A utility to determine if assemblies were built as Debug or Release");
            Console.WriteLine("Usage:");
            Console.WriteLine("    Flavors <file name>|<directory name>");
            Console.WriteLine("Examples:");
            Console.WriteLine("    Flavors assembly1.dll    //Determine flavor of assembly1.dll");
            Console.WriteLine("    Flavors assembly1.dll assembly2.dll    //Determine flavors of two assemblies");
            Console.WriteLine("    Flavors D:/dir1    //Determine flavors of all assemblies under dir");
            Console.WriteLine("    Flovors *.dll    //Determine flavors of all .dll files under current directory");
        }
    }
}
