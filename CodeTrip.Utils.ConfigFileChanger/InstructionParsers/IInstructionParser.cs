using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CodeTrip.Utils.ConfigFileChanger;
using CodeTrip.Utils.ConfigFileChanger.Extensions;
using CodeTrip.Utils.ConfigFileChanger.InstructionLoaders;
using System.Linq;

namespace CodeTrip.Utils.ConfigFileChanger.InstructionParsers
{
    /// <summary>
    /// Takes an instruction file location and turns it into a FileChangeInstruction set.
    /// </summary>
    public interface IInstructionParser
    {
        IEnumerable<FileChangeInstruction> Parse(DeployCmdLineArgs cmdLineArgs);
    }

    public abstract class XmlParser : IInstructionParser
    {
        private readonly string _fileMatch;

        protected XmlParser(string fileMatch)
        {
            _fileMatch = fileMatch;
        }

        public IEnumerable<FileChangeInstruction> Parse(DeployCmdLineArgs cmdLineArgs)
        {
            foreach (string file in Directory.GetFiles(cmdLineArgs.InstructionsFilesLocation, _fileMatch))
            {
                var doc = new XmlDocument();
                doc.Load(file);

                var ret = GetFileChangeInstructions(cmdLineArgs, doc.SelectNodes("//Files/File"), GetXmlChangeInstructions)
                    .Union(GetFileChangeInstructions(cmdLineArgs, doc.SelectNodes("//Replacements"), GetReplacementInstructions));

                foreach (var retItem in ret)
                    yield return retItem;
            }
        }

        private static IEnumerable<FileChangeInstruction> GetFileChangeInstructions(
            DeployCmdLineArgs cmdLineArgs, 
            XmlNodeList elements, 
            Func<XmlElement, IEnumerable<IChangeInstruction>> getInstructions)
        {
            return from XmlElement element in elements 
                   let filenameMatches = (element.HasAttribute("match")
                                            ? element.GetAttribute("match")
                                            : "*.*")
                                            .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries) 
                   let ancestorFolders = element.GetAttribute("ancestor-folder").Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                   select new FileChangeInstruction(
                       getInstructions(element),
                       new PatternMatchingConfigFilesLocator(cmdLineArgs.ConfigFilesLocation, ancestorFolders, filenameMatches));
        }

        protected abstract IEnumerable<IChangeInstruction> GetReplacementInstructions(XmlElement replacementNode);

        protected abstract IEnumerable<IChangeInstruction> GetXmlChangeInstructions(XmlElement fileNode);

        protected string GetXPath(XmlElement pn)
        {
            return pn.HasAttribute("xpath") ? pn.GetAttribute("xpath") : "//" + pn.Name;
        }
    }

    public class AllEnvironmentsInOneFileXmlParser : XmlParser
    {
        private readonly string _environment;

        public AllEnvironmentsInOneFileXmlParser(string fileMatch, string environment, StringReplacementHistory history)
            : base(fileMatch)
        {
            _environment = environment;
            _history = history;
        }

        private readonly StringReplacementHistory _history;

        protected override IEnumerable<IChangeInstruction> GetReplacementInstructions(XmlElement replacementsNode)
        {
            foreach (var replacementNode in replacementsNode.OfType<XmlElement>())
            {
                foreach (var env in new[] { _environment, "theRest" })
                {
                    if (replacementNode.HasAttribute(env))
                    {
                        var instruction = new TextChangeInstruction("$({0})".FormatWith(replacementNode.Name), _history.ReplaceFromHistory(replacementNode.GetAttribute(env)));
                        _history.AddInstruction(instruction);
                        yield return instruction;
                        break;
                    }
                }
            }
        }

        protected override IEnumerable<IChangeInstruction> GetXmlChangeInstructions(XmlElement fileNode)
        {
            foreach (var propertyNode in fileNode.OfType<XmlElement>())
            {
                foreach (var env in new[] {_environment, "theRest"})
                {
                    if (propertyNode.HasAttribute(env))
                    {
                        yield return new XmlChangeInstruction(GetXPath(propertyNode), propertyNode.GetAttribute(env));
                        break;
                    }
                }
            }
        }
    }

    public class FilePerEnvironmentXmlParser : XmlParser
    {
        public FilePerEnvironmentXmlParser(string fileMatch) : base(fileMatch)
        {
        }

        protected override IEnumerable<IChangeInstruction> GetReplacementInstructions(XmlElement replacementNode)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IChangeInstruction> GetXmlChangeInstructions(XmlElement fileNode)
        {
            return from XmlElement propertyNode in fileNode
                   select (IChangeInstruction)new XmlChangeInstruction(GetXPath(propertyNode), propertyNode.GetAttribute("value"));
        }
    }


    public class StringReplacementHistory
    {
        private readonly List<TextChangeInstruction> _history = new List<TextChangeInstruction>();

        public void AddInstruction(TextChangeInstruction instruction)
        {
            _history.Add(instruction);
        }

        public string ReplaceFromHistory(string sInput)
        {
            return _history.Aggregate(sInput, (s, h) => s.Replace(h.WhatToReplace, h.Replacement));
        }
    }
}