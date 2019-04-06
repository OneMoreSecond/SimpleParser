using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Schema;

namespace CFGParser
{
    class Program
    {
        private enum ReturnValue
        {
            Success = 0,
            InvalidArgument = 1,
            InvalidGrammar = 2,
        }
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the grammar folder argument");
                return (int)ReturnValue.InvalidArgument;
            }

            string grammarFolder = args[1];

            GrammarDocumentGroup.SchemaPath = "Schema.xsd";

            var documents = new GrammarDocumentGroup();
            bool success = documents.Load(grammarFolder, (sender, e) =>
            {
                string severity;
                switch (e.Severity)
                {
                    case XmlSeverityType.Error:
                        severity = "Error";
                        break;
                    case XmlSeverityType.Warning:
                        severity = "Warning";
                        break;
                    default:
                        severity = "UnknownSeverityType";
                        break;
                }
                Console.WriteLine(severity + ": " + e.Message + $" ({e.Exception.Source})");
            });

            if (!success)
            {
                Console.WriteLine("Schema validation doesn't pass");
                return (int)ReturnValue.InvalidGrammar;
            }

            var grammar = new Grammar();
            try
            {
                grammar.Load(documents);
            }
            catch(AggregateException pack)
            {
                foreach (Exception e in pack.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
                return (int)ReturnValue.InvalidGrammar;
            }

            return 0;
        }
    }
}
