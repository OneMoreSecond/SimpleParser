using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace CFGParser
{

    class SymbolRefNotFoundException : Exception
    {
        public readonly string Path;
        public readonly string SymbolRefName;

        public SymbolRefNotFoundException(string path, string symbolRefName)
            : base($"Symbol reference {symbolRefName} not found ({path})")
        {
            Path = path;
            SymbolRefName = symbolRefName;
        }
    }
    class SymbolRedefinitionException : Exception
    {
        public readonly string Path;
        public readonly string SymbolName;

        public SymbolRedefinitionException(string path, string symbolName)
            : base($"Symbol {symbolName} is defined twice ({path})")
        {
            Path = path;
            SymbolName = symbolName;
        }
    }

    class Grammar
    {
        Dictionary<SymbolId, ISymbol> _symbols;
        Production _startRule;

        public bool IsValid => _symbols != null;

        public void Load(GrammarDocumentGroup documents)
        {
            var idGenerator = new SymbolId.Generator();
            Dictionary<string, SymbolId> symbolRefs = CheckGrammar(documents, idGenerator);
        }

        static private Dictionary<string, SymbolId> CheckGrammar(GrammarDocumentGroup documents, SymbolId.Generator idGenerator)
        {
            var symbolRefs = new Dictionary<string, SymbolId>();
            var exceptions = new List<Exception>();
            bool hasTopSymbol = false;

            void collectSymbolNamesByTagName(GrammarDocument document, string tagName)
            {
                foreach (XmlElement element in document.Content.GetElementsByTagName(tagName))
                {
                    if (element.HasAttribute("name"))
                    {
                        string name = element.Attributes["name"].Value;
                        if (symbolRefs.ContainsKey(name))
                        {
                            exceptions.Add(new SymbolRedefinitionException(document.Path, name));
                        }
                        else
                        {
                            symbolRefs[name] = idGenerator.Next();
                        }
                    }
                    if (element.HasAttribute("top") && XmlConvert.ToBoolean(element.Attributes["top"].Value))
                    {
                        hasTopSymbol = true;
                    }
                }
            }

            foreach (GrammarDocument document in documents.Items)
            {
                collectSymbolNamesByTagName(document, "Terminal");
                collectSymbolNamesByTagName(document, "Symbol");
            }

            if (!hasTopSymbol)
            {
                exceptions.Add(new Exception("Grammar has no top level symbol"));
            }

            foreach (GrammarDocument document in documents.Items)
            {
                foreach (XmlElement element in document.Content.GetElementsByTagName("SymbolRef"))
                {
                    Debug.Assert(element.HasAttribute("name"));
                    
                    string name = element.Attributes["name"].Value;
                    if (!symbolRefs.ContainsKey(name))
                    {
                        exceptions.Add(new SymbolRefNotFoundException(document.Path, name));
                    }
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            return symbolRefs;
        }

        public void Parse(string str)
        {
            throw new NotImplementedException();

            Debug.Assert(IsValid);

            var progresses = new List<ProductionProgress>[str.Length + 1];
            for (int i = 0; i < progresses.Length; i++)
            {
                progresses[i] = new List<ProductionProgress>();
            }

            progresses[0].Add(new ProductionProgress(_startRule, 0, 0));

            var histories = new Dictionary<SymbolId, ProductionProgress>[str.Length];
            for (int i = 0; i < progresses.Length; i++)
            {
                for (int j = 0; j < progresses[i].Count; j++)
                {
                    Production rule = progresses[i][j].Production;
                    if (progresses[i][j].Progress == rule.Body.Count)
                    {
                        // Complete
                        ;
                    }
                }
            }
        }
    }
}
