using System.Collections.Generic;
using CodeTrip.Utils.ConfigFileChanger.Changers;
using CodeTrip.Utils.ConfigFileChanger.Extensions;

namespace CodeTrip.Utils.ConfigFileChanger.InstructionLoaders
{
    public abstract class ChangeInstruction : IChangeInstruction
    {
        public abstract IFileChanger SpawnChanger();
    }

    public interface IChangeInstruction
    {
        IFileChanger SpawnChanger();
    }

    public class FileChangeInstruction
    {
        public FileChangeInstruction(IEnumerable<IChangeInstruction> changeInstructions, IFileLocator fileLocator)
        {
            ChangeInstructions = changeInstructions;
            FileLocator = fileLocator;
        }
        
        public IEnumerable<IChangeInstruction> ChangeInstructions { get; set; }
        public IEnumerable<string> FilesToChange()
        {
            return FileLocator.Locate();
        }
        public IFileLocator FileLocator { get; set; }
    }

    public class TextChangeInstruction : ChangeInstruction
    {
        public TextChangeInstruction(string whatToReplace, string replacement)
        {
            WhatToReplace = whatToReplace;
            Replacement = replacement;
        }

        public string WhatToReplace { get; private set; }
        public string Replacement { get; private set; }

        public override string ToString()
        {
            return "{0} => {1}".FormatWith(WhatToReplace, Replacement);
        }

        public override IFileChanger SpawnChanger()
        {
            return new StringReplaceTextFileChanger(this);
        }
    }

    public class XmlChangeInstruction : ChangeInstruction
    {
        public XmlChangeInstruction(string xPath, string value)
        {
            XPath = xPath;
            Value = value;
        }

        public string XPath { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return "{0} => {1}".FormatWith(XPath, Value);
        }

        public override IFileChanger SpawnChanger()
        {
            return new SetInnerXml(this);
        }
    }
}