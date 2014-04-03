using System.Collections.Generic;
using System.IO;
using CodeTrip.Utils.ConfigFileChanger.Extensions;

namespace CodeTrip.Utils.ConfigFileChanger.InstructionLoaders
{
    public class DevBuildInstructionSetLoader : IInstructionSetLoader
    {
        private readonly ProjectType _projectType;
        private readonly string _rootDirectory;
        private readonly string _configuration;

        public DevBuildInstructionSetLoader(ProjectType projectType, string rootDirectory, string configuration)
        {
            _projectType = projectType;
            _rootDirectory = rootDirectory;
            _configuration = configuration;
        }

        public IEnumerable<FileChangeInstructionSet> Load()
        {
            return new[]
            {
                new FileChangeInstructionSet
                {
                    SourceFile = GetFileLocation(@"config\data.connections.config"),
                    DestFile = GetFileLocation(@"config\data.connections.dev.config"),
                    ChangeInstructions = new[] {new SetDbServerToEnvVariable()}
                }
            };
        }


        private string GetFileLocation(string relativeLocation)
        {
            switch (_projectType)
            {
                case ProjectType.Test:
                case ProjectType.Console:
                    return Path.Combine(Path.Combine(_rootDirectory, @"bin\{0}".FormatWith(_configuration)), relativeLocation);
                default:
                    return Path.Combine(Path.Combine(_rootDirectory, "bin"), relativeLocation);
            }
        }
    }
}