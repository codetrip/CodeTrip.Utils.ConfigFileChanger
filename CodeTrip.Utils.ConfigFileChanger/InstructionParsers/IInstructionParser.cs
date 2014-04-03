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

    public class EnvironmentAttributeComparer : IComparer<EnvironmentAttribute>
    {
        public int Compare(EnvironmentAttribute x, EnvironmentAttribute y)
        {
            var compareLength = y.EnvironmentMatches.Length.CompareTo(x.EnvironmentMatches.Length);
            if (compareLength != 0)
                return compareLength;

            if (x.EnvironmentMatches.Contains("theRest") && !y.EnvironmentMatches.Contains("theRest"))
                return 1;

            if (!x.EnvironmentMatches.Contains("theRest") && y.EnvironmentMatches.Contains("theRest"))
                return -1;

            return x.EnvironmentMatches.Select((t, i) => t.CompareTo(y.EnvironmentMatches[i])).FirstOrDefault(compareI => compareI != 0);
        }
    }

    public class EnvironmentAttribute 
    {
        public EnvironmentAttribute(XmlAttribute xmlAttribute)
        {
            EnvironmentMatches = xmlAttribute.Name.Split('.');
            Value = xmlAttribute.Value;
        }

        public string[] EnvironmentMatches { get; set; }
        public string Value { get; set; }
    }

    public class AllEnvironmentsInOneFileXmlParser : XmlParser
    {
        private readonly string[] _environments;

        public AllEnvironmentsInOneFileXmlParser(string fileMatch, string[] environments, StringReplacementHistory history)
            : base(fileMatch)
        {
            _environments = environments;
            _history = history;
        }

        private readonly StringReplacementHistory _history;

        protected override IEnumerable<IChangeInstruction> GetReplacementInstructions(XmlElement replacementsNode)
        {
            foreach (var replacementNode in replacementsNode.OfType<XmlElement>())
            {
                var bestMatch = GetMatch(replacementNode);

                if (bestMatch != null)
                {
                    var instruction = new TextChangeInstruction("$({0})".FormatWith(replacementNode.Name),
                        _history.ReplaceFromHistory(bestMatch.Value));
                    _history.AddInstruction(instruction);
                    yield return instruction;
                }
            }
        }

        private EnvironmentAttribute GetMatch(XmlElement node)
        {
            var envAttrs = node.Attributes.Cast<XmlAttribute>()
                .Where(a => a.Name != "xpath")
                .Select(a => new EnvironmentAttribute(a));

            var bestMatch = envAttrs
                .OrderBy(a => a, new EnvironmentAttributeComparer())
                .FirstOrDefault(a => a.EnvironmentMatches.All(m => _environments.Contains(m)))
                            ??
                            envAttrs.FirstOrDefault(
                                a => a.EnvironmentMatches.Length == 1 && a.EnvironmentMatches[0] == "theRest");

            return bestMatch;

        }

        protected override IEnumerable<IChangeInstruction> GetXmlChangeInstructions(XmlElement fileNode)
        {
            foreach (var propertyNode in fileNode.OfType<XmlElement>())
            {
                var bestMatch = GetMatch(propertyNode);
                
                if (bestMatch != null)
                    yield return new XmlChangeInstruction(GetXPath(propertyNode), bestMatch.Value);
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