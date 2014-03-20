using System;
using CodeTrip.Utils.ConfigFileChanger.Extensions;

namespace CodeTrip.Utils.ConfigFileChanger.Exceptions
{
    public class CodeTripUnexpectedValueException : Exception
    {
        public CodeTripUnexpectedValueException(string whatHadTheUnexpectedValue, string whatWasTheUnexpectedValue)
            : base("Cannot handle a {0} value of {1}".FormatWith(whatWasTheUnexpectedValue, whatHadTheUnexpectedValue))
        {
        }
    }
}
