using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace CFGParser
{
    internal struct GrammarDocument
    {
        public readonly string Path;
        public readonly XmlDocument Content;

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

        public IReadOnlyCollection<GrammarDocument> Items => _items;

        public bool Load(string folder, ValidationEventHandler eventHandler)
        {
            bool passSchemaValidation = true;

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                ValidationType = ValidationType.Schema,
            };

            if (SchemaPath != null)
            {
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas.Add(SchemaNameSpace, SchemaPath);
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += (sender, e) =>
                {
                    passSchemaValidation = false;
                    eventHandler?.Invoke(sender, e);
                };
            }
            else
            {
                return false;
            }

            _items = new List<GrammarDocument>();
            foreach (string path in Directory.EnumerateFiles(folder))
            {
                if (path.EndsWith(".xml"))
                {
                    var document = new XmlDocument();
                    using (XmlReader reader = XmlReader.Create(path, settings))
                    {
                        document.Load(reader);
                    }
                    _items.Add(new GrammarDocument(path, document));
                }
            }

            if (!passSchemaValidation)
            {
                _items = null;
            }

            return passSchemaValidation;
        }
    }
}
