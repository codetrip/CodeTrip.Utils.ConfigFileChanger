using System;
using System.Xml;
using CodeTrip.Utils.ConfigFileChanger.InstructionLoaders;

namespace CodeTrip.Utils.ConfigFileChanger.Changers
{
    public class SetInnerXml : XmlFileChanger
    {
        private readonly XmlChangeInstruction _instruction;
        
        public SetInnerXml(XmlChangeInstruction instruction)
        {
            _instruction = instruction;
        }
        
        protected override void ChangeXmlFile(XmlDocument configFile)
        {
            Console.WriteLine(_instruction);
            foreach (XmlNode element in configFile.SelectNodes(_instruction.XPath))
            {
                element.InnerXml = _instruction.Value;
            }    
        }
    }
}