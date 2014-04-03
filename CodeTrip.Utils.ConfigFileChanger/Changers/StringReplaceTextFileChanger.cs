using System;
using System.Collections.Generic;
using System.IO;
using CodeTrip.Utils.ConfigFileChanger.Changers;
using CodeTrip.Utils.ConfigFileChanger.InstructionLoaders;
using System.Linq;

namespace CodeTrip.Utils.ConfigFileChanger
{
    public class StringReplaceTextFileChanger : IFileChanger
    {
        private readonly TextChangeInstruction _instruction;

        public StringReplaceTextFileChanger(TextChangeInstruction instruction)
        {
            _instruction = instruction;
        }

        public void Change(string configFile, string destinationFile)
        {
            string output;
            
            Console.WriteLine(_instruction);
            if (configFile == Constants.NOFILEFOUND)
                return;

            using (var sr = new StreamReader(configFile))
            {
                string configFileContents = sr.ReadToEnd();
                output = configFileContents.Replace(_instruction.WhatToReplace, _instruction.Replacement);
            }

            using (var sw = new StreamWriter(destinationFile))
            {
                sw.Write(output);
            }
        }
    }
}