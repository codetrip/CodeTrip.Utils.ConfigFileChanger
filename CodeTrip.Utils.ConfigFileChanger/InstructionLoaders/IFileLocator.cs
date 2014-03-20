using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeTrip.Utils.ConfigFileChanger.Extensions;

namespace CodeTrip.Utils.ConfigFileChanger.InstructionLoaders
{
    /// <summary>
    /// Gets the files to change.
    /// </summary>
    public interface IFileLocator
    {
        IEnumerable<string> Locate();
    }

    public class PatternMatchingConfigFilesLocator : IFileLocator
    {
        private readonly string _rootDirectory;
        private readonly string[] _ancestorDirectories;
        private readonly string[] _matches;

        public PatternMatchingConfigFilesLocator(string rootDirectory, string[] ancestorDirectories, string[] matches)
        {
            _rootDirectory = rootDirectory;
            _ancestorDirectories = ancestorDirectories;
            _matches = matches;
        }

        public IEnumerable<string> Locate()
        {
            return _matches.SelectMany(match => Directory.GetFiles(_rootDirectory, match, SearchOption.AllDirectories))
                .Where(f => _ancestorDirectories.Length == 0 || 
                    _ancestorDirectories.Any(d => f.IsNullOrEmpty() || (Path.GetDirectoryName(f) + @"\").ToUpper().Contains(@"\{0}\".FormatWith(d.ToUpper()))));
        }
    }
}