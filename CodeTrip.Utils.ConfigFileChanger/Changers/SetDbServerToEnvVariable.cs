using System;
using System.Data.Common;
using System.Xml;
using CodeTrip.Utils.ConfigFileChanger.Changers;
using CodeTrip.Utils.ConfigFileChanger.InstructionLoaders;
using CodeTrip.Utils.ConfigFileChanger.Extensions;

namespace CodeTrip.Utils.ConfigFileChanger
{
    public class SetDbServerToEnvVariable : ReplaceAttributeValue, IChangeInstruction
    {
        public SetDbServerToEnvVariable() : base("//add[@connectionString]", "connectionString")
        {
            
        }

        protected override string GetReplacementValue(string currentValue, XmlElement element, XmlDocument document)
        {
            Console.WriteLine("CurentValue is " + currentValue);
            string envVariable = Environment.GetEnvironmentVariable("DbServer");
            Console.WriteLine("DbServer environment variable is "+ envVariable);
            if (envVariable.IsNullOrEmpty())
                return currentValue;

            var csb = new DbConnectionStringBuilder { ConnectionString = currentValue };

            csb["Data Source"] = envVariable;

            return csb.ConnectionString;
        }

        public IFileChanger SpawnChanger()
        {
            return this;
        }
    }
}