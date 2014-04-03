using System;
using System.IO;
using System.Xml;

namespace CodeTrip.Utils.ConfigFileChanger
{
    public interface IFileChanger
    {
        void Change(string configFile, string destinationFile);
    }

    public abstract class XmlFileChanger : IFileChanger
    {
        public void Change(string configFile, string destinationFile)
        {
            var xmlDocument = new XmlDocument();
            if (configFile == Constants.NOFILEFOUND || !File.Exists(configFile))
                return;

            xmlDocument.Load(configFile);

            ChangeXmlFile(xmlDocument);

            xmlDocument.Save(destinationFile);
        }
        
        protected abstract void ChangeXmlFile(XmlDocument xmlDocument);
    }
}