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
        public enum Flavor
        {
            Debug,
            Release,
            Unknown
        }

        public class Result
        {
            public string FullName;

            public Flavor Flavor;

            public ProcessorArchitecture TargetPlatform;

            public string TargetFramework;

            public string ErrorMessage;

            public bool HasError;

            public Result(
                string fullName, 
                Flavor flavor, 
                ProcessorArchitecture targetPlatform, 
                string targetFramework, 
                string errorMsg
                )

            {
                this.FullName = fullName;
                this.Flavor = flavor;
                this.TargetPlatform = targetPlatform;
                this.TargetFramework = targetFramework;
                if(errorMsg==null)
                {
                    this.HasError = false;
                }
                else
                {
                    this.HasError = true;
                    this.ErrorMessage = errorMsg;
                }
            }
        }

        static Result GetFlavor(string file)
        {
            string fullName;
            Flavor flavor = Flavor.Unknown;
            ProcessorArchitecture targetPlatform;
            string targetFramework;

            AssemblyName assemblyName;
            Assembly assembly;

            //Determine target platform
            try
            {
                assemblyName = AssemblyName.GetAssemblyName(file);
                fullName = assemblyName.FullName;
                targetPlatform = assemblyName.ProcessorArchitecture;
            }
            catch(Exception ex)
            {
                return new Result(
                    "Unknown", 
                    Flavor.Unknown, 
                    ProcessorArchitecture.None, 
                    "Unknown", 
                    string.Format("Determine target platform failed. {0}", ex.Message)
                    );
            }

            //Determine flavor
            try
            {
                assembly = Assembly.Load(assemblyName);
                flavor = Flavor.Release;

                foreach (object attribute in assembly.GetCustomAttributes(false))
                {
                    if (attribute.GetType() == Type.GetType("System.Diagnostics.DebuggableAttribute"))
                    {
                        if(((System.Diagnostics.DebuggableAttribute)attribute).IsJITTrackingEnabled)
                        {
                            flavor = Flavor.Debug;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return new Result(
                    fullName, 
                    Flavor.Unknown, 
                    targetPlatform, 
                    "Unknown", 
                    string.Format("Determine flavor failed. {0}", ex.Message)
                    );
            }

            targetFramework = assembly.ImageRuntimeVersion;
            return new Result(
                fullName, 
                flavor, 
                targetPlatform, 
                targetFramework, 
                null
                );
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
                        string fullName;
                        try
                        {
                            fullName = Path.GetFullPath(file);
                        }
                        catch(Exception ex)
                        {
                            PrintResult(file, new Result(
                                "Unknown", 
                                Flavor.Unknown, 
                                ProcessorArchitecture.None, 
                                "Unknown", 
                                string.Format("Get full path failed. {0}", ex.Message))
                                );

                            continue;
                        }
                        PrintResult(fullName, Program.GetFlavor(fullName));
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
                    FileInfo[] files;

                    try
                    {
                        files = dirInfo.GetFiles(path);
                    }
                    catch
                    {
                        Console.WriteLine("You can ONLY use wildcard under current directory.");
                        continue;
                    }

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

        static void PrintResult(string file, Result result)
        {
            
            Console.WriteLine("Assembly: \t\t{0}", file);
            Console.WriteLine("Full Name: \t\t{0}", result.FullName);
            Console.WriteLine("Configuration:\t\t{0}", result.Flavor);
            Console.WriteLine("Target Platform:\t{0}", result.TargetPlatform.ToString());
            Console.WriteLine("Target Framework:\t{0}", result.TargetFramework);
            if (result.HasError)
            {
                Console.WriteLine("Error Message: \t\t{0}", result.ErrorMessage);
            }
            Console.WriteLine("");
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
