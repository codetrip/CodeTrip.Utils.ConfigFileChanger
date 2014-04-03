using System;
using System.IO;
using System.Xml;
using CodeTrip.Utils.ConfigFileChanger.Changers;
using CodeTrip.Utils.ConfigFileChanger.InstructionLoaders;
using NUnit.Framework;

namespace CodeTrip.Utils.ConfigFileChanger.Tests
{
    [TestFixture]
    public class SetInnerXmlTests
    {
        [Test]
        public void Simple_run_through()
        {
            var doc = new XmlDocument();
            var nsm = new XmlNamespaceManager(doc.NameTable);
            //nsm.AddNamespace("", "http://blah");

            doc.LoadXml("<root xmlns='http://blah'><Property>oldValue</Property></root>");

            Console.WriteLine(doc.InnerXml);

            var sourceLocation = Path.GetTempFileName();
            var destLocation = Path.GetTempFileName();

            doc.Save(sourceLocation);

            new SetInnerXml(new XmlChangeInstruction("//Property", "newValue", "http://blah")).Change(sourceLocation, destLocation);

            doc.Load(destLocation);
            Console.WriteLine(doc.InnerXml);
            Assert.That(doc.SelectSingleNode("//Property").InnerText, Is.EqualTo("newValue"));
        }
    }
}