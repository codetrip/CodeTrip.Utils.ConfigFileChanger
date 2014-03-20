using System;
using System.Collections.Generic;
using System.Diagnostics;
using CodeTrip.Utils.ConfigFileChanger.Exceptions;
using CodeTrip.Utils.ConfigFileChanger.Extensions;
using CodeTrip.Utils.ConfigFileChanger.InstructionLoaders;
using CodeTrip.Utils.ConfigFileChanger.InstructionParsers;
using NDesk.Options;

namespace CodeTrip.Utils.ConfigFileChanger
{
    public class UsageException : Exception
    {
        
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                Console.WriteLine(Environment.CommandLine);

                var modeCmdLineArgs = new ModeCmdLineArgs();

                var otherArgs = new OptionSet
                {
                    {"M|Mode=", v => modeCmdLineArgs.Mode = (Mode) Enum.Parse(typeof (Mode), v, true)},
                    {"D|Debug", v => modeCmdLineArgs.Debug = v != null},
                }.Parse(args);
                
                if (modeCmdLineArgs.Debug)
                    Debugger.Break();

                IInstructionSetLoader instructionSetLoader;

                switch (modeCmdLineArgs.Mode)
                {
                    case Mode.Dev:
                        instructionSetLoader = GetDevInstructionSetLoader(otherArgs);
                        break;
                    case Mode.Deploy:
                        instructionSetLoader = GetDeploymentInstructionSetLoader(otherArgs);
                        break;
                    default:
                        throw new CodeTripUnexpectedValueException("Mode", modeCmdLineArgs.Mode.ToString());
                }

                var instructionSets = instructionSetLoader.Load();

                foreach (var instructionSet in instructionSets)
                {
                    Console.WriteLine("Source file:{0}; Dest file:{1}".FormatWith(instructionSet.SourceFile,
                                                                                  instructionSet.DestFile));

                    foreach (var instruction in instructionSet.ChangeInstructions)
                        instruction.SpawnChanger().Change(instructionSet.SourceFile, instructionSet.DestFile);
                }

            }
            catch (UsageException)
            {
                return;
            }
        }

        private static IInstructionSetLoader GetDevInstructionSetLoader(IEnumerable<string> args)
        {
            var devBuildCmdLineArgs = new DevBuildCmdLineArgs();

            new OptionSet
            {
                {
                    "ProjectType=",
                    v => devBuildCmdLineArgs.ProjectType = (ProjectType) Enum.Parse(typeof (ProjectType), v, true)
                },
                {"Dir|Directory=", v => devBuildCmdLineArgs.Directory = v},
                {"Config|Configuration=", v => devBuildCmdLineArgs.Configuration = v},
            }.Parse(args);

            if (devBuildCmdLineArgs.Configuration == null || devBuildCmdLineArgs.Directory == null)
                throw new UsageException();

            return new DevBuildInstructionSetLoader(devBuildCmdLineArgs.ProjectType, devBuildCmdLineArgs.Directory,
                                                  devBuildCmdLineArgs.Configuration);
        }

        private static IInstructionSetLoader GetDeploymentInstructionSetLoader(IEnumerable<string> args)
        {
            Console.WriteLine("Parsing deployment instruction cmd line...");
            var deployCmdLineArgs = new DeployCmdLineArgs();

            new OptionSet()
            {
                {"A|AllInSameFile=", v => deployCmdLineArgs.AllInSameFile = v != null},
                {"Env=", v => deployCmdLineArgs.Environment = v},
                {"Inst=", v => deployCmdLineArgs.InstructionsFilesLocation = v},
                {"Config=", v => deployCmdLineArgs.ConfigFilesLocation = v},
            }.Parse(args);

            if (deployCmdLineArgs.Environment == null || deployCmdLineArgs.InstructionsFilesLocation == null || deployCmdLineArgs.ConfigFilesLocation == null)
                throw new UsageException();

            var stringReplacementHistory = new StringReplacementHistory();
            var parser = deployCmdLineArgs.AllInSameFile
                             ? (IInstructionParser) new AllEnvironmentsInOneFileXmlParser("*.inst",
                                                                     deployCmdLineArgs.Environment,
                                                                     stringReplacementHistory)
                             : new FilePerEnvironmentXmlParser(deployCmdLineArgs.Environment + ".inst");

            var loader = new PerEnvironmentInstructionsSetLoader(parser.Parse(deployCmdLineArgs));

            return loader;

        }
    }

    public class ModeCmdLineArgs
    {
        public Mode Mode;
        public bool Debug;
    }

    public enum Mode
    {
        Dev,
        Deploy
    }

    public class DevBuildCmdLineArgs 
    {
        public ProjectType ProjectType;
        public string Configuration;
        public string Directory;
    }

    public class DeployCmdLineArgs : ModeCmdLineArgs
    {
        public bool AllInSameFile;

        public string Environment;

        public string InstructionsFilesLocation;

        public string ConfigFilesLocation;
    }
    
    public enum ProjectType
    {
        Test,
        Console,
        Web
    }
}
