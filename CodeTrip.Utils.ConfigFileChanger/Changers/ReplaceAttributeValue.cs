using System.Xml;
using CodeTrip.Utils.ConfigFileChanger.Changers;

namespace CodeTrip.Utils.ConfigFileChanger
{
    public abstract class ReplaceAttributeValue : XmlFileChanger
    {
        private readonly string _elementXpath;
        private readonly string _attributeName;

        protected ReplaceAttributeValue(string elementXpath, string attributeName)
        {
            _elementXpath = elementXpath;
            _attributeName = attributeName;
        }

        protected abstract string GetReplacementValue(string currentValue, XmlElement element, XmlDocument document);

        protected override void ChangeXmlFile(XmlDocument configFile)
        {
            foreach (XmlElement element in configFile.SelectNodes(_elementXpath))
            {
                string attributeValue = element.GetAttribute(_attributeName);

                element.SetAttribute(_attributeName, GetReplacementValue(attributeValue, element, configFile));
            }
        }
    }
}