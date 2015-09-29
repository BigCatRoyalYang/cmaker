// **********************************************
// YHGenomics Inc. Production
// Date       : 2015-09-23
// Author     : Shubo Yang
// Description: Help Building CMake File
// **********************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CMaker
{
    class Program
    {
        public const string PROJECTNAME = "project";
        public const string FILETYPE = "ft";
        public const string OUT = "out";
        public const string COMPILER = "compiler";
        public const string FLAG = "flag";
        public const string DEBUG_FLAG = "debug";
        public const string AUTO = "auto";
        public const string LIBS = "libs";
        public const string LINKFLAG = "linkflag";

        const string CMakeFileDirectoryName = "cmakebuild";

        public static StringBuilder OutputData = new StringBuilder();
        public static Dictionary<string, string> Settings = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            //modified by Ke Yang 20150929
            if (NeedHelp(args))
            {
                ShowHelp();
                return; 
            }

            //Settings[PROJECTNAME]
            HardLevel( args );
            //DefaultValue();

            ReadArrengment(args);

            if (!Settings.ContainsKey(PROJECTNAME) || string.IsNullOrEmpty(Settings[PROJECTNAME]))
            {
                ShowHelp();
                return;
            }

            //ProjectName = args[0];
            //FileType = args[1].Split(',');
            //GenerateType = args[2];

            OutputData.AppendLine(string.Format("project({0})", Settings[PROJECTNAME]));
            var files = ScanFiles(System.IO.Directory.GetCurrentDirectory());

            if(files.Count>0)
                OutputData.AppendLine(string.Format("set(SRC_LIST {0})", string.Join(" ",files.ToArray())));

            if (Settings[OUT] == "exe")
            {
                OutputData.AppendLine(string.Format("add_executable({0} {1})", Settings[PROJECTNAME], "${SRC_LIST}")); 
            }
            else if(Settings[OUT] == "lib")
            {
                OutputData.AppendLine(string.Format("add_library({0} {1})", Settings[PROJECTNAME], "${SRC_LIST}"));
            } 

            if(Settings.ContainsKey(COMPILER) && !string.IsNullOrEmpty(Settings[COMPILER]))
                OutputData.AppendLine(string.Format("SET (CMAKE_CXX_COMPILER \"{0}\")", Settings[COMPILER]));
            if (Settings.ContainsKey(FLAG) && !string.IsNullOrEmpty(Settings[FLAG]))
                OutputData.AppendLine(string.Format("SET (CMAKE_CXX_FLAGS \"{0}\")", Settings[FLAG]));
            if (Settings.ContainsKey(DEBUG_FLAG) && !string.IsNullOrEmpty(Settings[DEBUG_FLAG]))
            {
                OutputData.AppendLine(string.Format("SET (CMAKE_BUILD_TYPE Debug)"));
                //Addjust by Ke Yang
                if (Settings[DEBUG_FLAG] != "maraton")
                {
                    OutputData.AppendLine(string.Format("SET (CMAKE_CXX_FLAGS_DEBUG \"{0}\")", Settings[DEBUG_FLAG]));
                }
            }

            if (Settings.ContainsKey(LINKFLAG) && !string.IsNullOrEmpty(Settings[LINKFLAG]))
            {

                OutputData.AppendLine(string.Format("SET (CMAKE_EXE_LINKER_FLAGS \"{0}\")", Settings[LINKFLAG]));
            }


            if (Settings.ContainsKey(LIBS) && !string.IsNullOrEmpty(Settings[LIBS]))
            {
                var libsArray = Settings[LIBS].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                OutputData.AppendLine(string.Format("target_link_libraries({0} {1})", Settings[PROJECTNAME], string.Join(" ", libsArray)));
            }
            Console.WriteLine("Creating Directory:" + Directory.GetCurrentDirectory() + "/"+ CMakeFileDirectoryName);
            System.IO.Directory.CreateDirectory(Directory.GetCurrentDirectory()+ "/"+ CMakeFileDirectoryName);
            string make_directory = Directory.GetCurrentDirectory() + "/"+ CMakeFileDirectoryName+"/";
            Console.WriteLine("Creating CMakeFile:" + make_directory + "CMakeLists.txt");
            System.IO.File.WriteAllText(Path.Combine(make_directory, "CMakeLists.txt"), OutputData.ToString());

            if (Settings.ContainsKey(AUTO) && !string.IsNullOrEmpty(Settings[AUTO]) && Settings[AUTO]=="true")
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.WorkingDirectory = make_directory;
                psi.FileName = "cmake";
                psi.Arguments = "./";

                Process.Start(psi).WaitForExit();

                psi = new ProcessStartInfo();
                psi.WorkingDirectory = make_directory;
                psi.FileName = "make";
                Process.Start(psi);
            }
        }

        private static void HardLevel(string[] args)
        {
            if (args.Length == 2 && args[1] == "--maraton")
            {
                HardDefaultValue();
            }
            else
            {
                DefaultValue();
            }

        }

        private static bool NeedHelp(string[] args)
        {
            bool result = false;
            if (args.Length == 0)
            {
                result = true;
            }
            else if (args.Length == 1)
            {
                if (args[0] == "-h" || args[0] == "--help")
                {
                    result = true;
                }
            }
            return result;
        }

        static void ShowHelp()
        {
            Console.WriteLine("CMaker");
            Console.WriteLine("YHGenomics Inc. Production");
            Console.WriteLine("CMaker project:name [options]");
            Console.WriteLine("options:");
            Console.WriteLine("       ft:*.h,*.cpp(default)");
            Console.WriteLine("       out:exe(default) - support exe,lib");
            Console.WriteLine("       compiler:/usr/bin/clang(default) - support gcc,g++");
            Console.WriteLine("       flag:-Wall --std=c++11(default)");
            Console.WriteLine("       debug:[null](default) - support -g");
            Console.WriteLine("       auto:false(default) - support -g : auto invoke cmake and make");
            Console.WriteLine("       libs:[null] - support libxxx.o,libyyy.o");
            Console.WriteLine("       --maraton     try it :) ");
        }

        /// <summary>
        /// add by Ke Yang 20150929
        /// to use C++11 in a critical level easily
        /// </summary>
        static void HardDefaultValue()
        {
            Settings[FILETYPE] = "*.cpp,*.h";
            Settings[OUT] = "exe";
            Settings[COMPILER] = "/usr/bin/clang++";
            Settings[FLAG] = "-std=c++11 -stdlib=libc++ -Werror -Weverything -Wno-deprecated-declarations -Wno-disabled-macro-expansion -Wno-float-equal -Wno-c++98-compat -Wno-c++98-compat-pedantic -Wno-global-constructors -Wno-exit-time-destructors -Wno-missing-prototypes -Wno-padded -Wno-old-style-cast";
            Settings[DEBUG_FLAG] = "maraton";
            Settings[AUTO] = "false";
            Settings[LIBS] = "";
            Settings[LINKFLAG] = "-lc++ -lc++abi";
        }

        static void DefaultValue()
        {
            Settings[FILETYPE] = "*.cpp,*.h";
            Settings[OUT] = "exe";
            Settings[COMPILER] = "/usr/bin/clang++";
            Settings[FLAG] = "-Wall -std=c++11";
            Settings[DEBUG_FLAG] = "";
            Settings[AUTO] = "false";
            Settings[LIBS] = "";
        }
        static void ReadArrengment(string[] args)
        {
            foreach (var item in args)
            { 
                var kv = item.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length != 2)
                {
                    continue;
                }
                if (!Settings.ContainsKey(kv[0]))
                {
                    Settings.Add(kv[0], null);
                }
                Settings[kv[0]] = kv[1];
            }
        }
        static List<string> ScanFiles(string directory)
        {
            List<string> ret = new List<string>();
            Console.WriteLine("Scaning Directory:"+ directory);
            var types = Settings[FILETYPE].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in types)
            {
                var files = System.IO.Directory.GetFiles(directory, item);
                foreach (var f in files)
                {
                    Console.WriteLine("Add File:" + f);
                    ret.Add(f);
                }
            }

            var dirs = System.IO.Directory.GetDirectories(directory);
            foreach (var item in dirs)
            {
                ret.AddRange(ScanFiles(item));
            }

            return ret;
        }
    }
}


