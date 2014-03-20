using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeTrip.Utils.ConfigFileChanger;
using CodeTrip.Utils.ConfigFileChanger.Extensions;

namespace CodeTrip.Utils.ConfigFileChanger.InstructionLoaders
{
    public interface IInstructionSetLoader
    {
        IEnumerable<FileChangeInstructionSet> Load();
    }

    public class PerEnvironmentInstructionsSetLoader : IInstructionSetLoader
    {
        private readonly IEnumerable<FileChangeInstruction> _baseInstructions;

        public PerEnvironmentInstructionsSetLoader(IEnumerable<FileChangeInstruction> baseInstructions)
        {
            _baseInstructions = baseInstructions;
        }

        public IEnumerable<FileChangeInstructionSet> Load()
        {
            return from bi in _baseInstructions
                   from f in bi.FilesToChange().OrIfNoneThen(Constants.NOFILEFOUND)
                   select
                       new FileChangeInstructionSet
                           {
                               SourceFile = f,
                               DestFile = f,
                               ChangeInstructions = bi.ChangeInstructions,
                           };
        }
    }

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