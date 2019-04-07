using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace CFGParser
{
    internal class GrammarDocument
    {
        public readonly string Path;
        public readonly XmlDocument Content;

        private GrammarDocument() { }

        public GrammarDocument(string path, XmlDocument content)
        {
            Path = path;
            Content = content;
        }
    }

    class GrammarDocumentGroup
    {
        const string SchemaNameSpace = "http://www.simpleparser.com/Grammar";
        public static string SchemaPath = null;

        List<GrammarDocument> _items;

        internal IReadOnlyCollection<GrammarDocument> Items => _items;

        public bool LoadWithValidation(string folder, ValidationEventHandler eventHandler)
        {
            if (SchemaPath == null)
            {
                return false;
            }

            bool passSchemaValidation = true;

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                ValidationType = ValidationType.Schema,
            };

            settings.Schemas.Add(SchemaNameSpace, SchemaPath);
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += (sender, e) =>
            {
                passSchemaValidation = false;
                eventHandler?.Invoke(sender, e);
            };

            var documents = new List<GrammarDocument>();
            foreach (string path in Directory.EnumerateFiles(folder).Where(x => x.EndsWith(".xml")))
            {
                var document = new XmlDocument();
                using (XmlReader reader = XmlReader.Create(path, settings))
                {
                    document.Load(reader);
                }
                documents.Add(new GrammarDocument(path, document));
            }

            if (passSchemaValidation)
            {
                _items = documents;
            }

            return passSchemaValidation;
        }
    }
}
