using System.Collections.Generic;
using CodeTrip.Utils.ConfigFileChanger.InstructionLoaders;

namespace CodeTrip.Utils.ConfigFileChanger
{
    /// <summary>
    /// Represents a source a destination and all the changers that will 
    /// </summary>
    public class FileChangeInstructionSet
    {
        public string SourceFile { get; set; }
        public string DestFile { get; set; }
        public IEnumerable<IChangeInstruction> ChangeInstructions { get; set; }
    }
}