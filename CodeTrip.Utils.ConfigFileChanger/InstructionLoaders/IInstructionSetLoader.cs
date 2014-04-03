using System.Collections.Generic;
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
}