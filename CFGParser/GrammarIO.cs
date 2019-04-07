using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace CFGParser
{
    class SymbolRefNotFoundException : Exception
    {
        public readonly string Path;
        public readonly string SymbolRefName;

        public SymbolRefNotFoundException(string path, string symbolRefName)
            : base($"Symbol reference {symbolRefName} not found. ({path})")
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
            : base($"Symbol {symbolName} is redefined. ({path})")
        {
            Path = path;
            SymbolName = symbolName;
        }
    }

    class IllegelSymbolNameException : Exception
    {
        public readonly string Path;
        public readonly string SymbolName;
        public readonly string Reason;

        public IllegelSymbolNameException(string path, string symbolName, string reason)
            : base($"Symbol name {symbolName} is illegal. {reason} ({path})")
        {
            Path = path;
            SymbolName = symbolName;
            Reason = reason;
        }
    }

    class IllegalOutputOrderException : Exception
    {
        public readonly string Path;

        public IllegalOutputOrderException(string path)
            : base($"Output order should be a permutation. ({path})")
        {
            Path = path;
        }
    }

    partial class Grammar
    {
        public const char ReservedCharacter = '#';

        Dictionary<string, SymbolId> _symbolRefs;
        SymbolId.Generator _idGenerator;

        public string ToText()
        {
            return string.Join("\n\n", _symbols.Values.Select(x => x.ToText(_symbols)));
        }

        public void Load(GrammarDocumentGroup documentGroup)
        {
            if (documentGroup.Items == null)
            {
                throw new ArgumentException("Grammar documents not loaded", "documentGroup");
            }
            CheckGrammar(documentGroup);
            BuildSymbols(documentGroup);
        }

        private void BuildSymbols(GrammarDocumentGroup documentGroup)
        {
            _symbols = new Dictionary<SymbolId, Symbol>();

            SymbolId startId = _idGenerator.Next();
            _start = new NonTerminalSymbol(startId, ReservedCharacter + "Start");
            _symbols.Add(startId, _start);

            foreach (GrammarDocument document in documentGroup.Items)
            {
                foreach (XmlElement element in document.Content.DocumentElement.ChildNodes)
                {
                    BuildSymbol(element);
                }
            }
        }

        private SymbolId BuildSymbol(XmlElement element)
        {
            string name = element.Attributes["name"]?.Value;

            SymbolId id = name != null ? _symbolRefs[name] : _idGenerator.Next();

            switch(element.Name)
            {
                case "Terminal":
                    Debug.Assert(element.ChildNodes.Count == 2);

                    Debug.Assert(element.FirstChild.Name == "in");
                    string input = element.FirstChild.InnerText;

                    Debug.Assert(element.LastChild.Name == "out");
                    string output = element.LastChild.InnerText;

                    _symbols.Add(id, new TerminalSymbol(id, input, output, name));
                    break;

                case "Symbol":
                    var symbol = new NonTerminalSymbol(id, name);
                    foreach (XmlElement subElement in element.ChildNodes)
                    {
                        switch(subElement.Name)
                        {
                            case "Terminal":
                            case "Symbol":
                                symbol.AddProduction(BuildSymbol(subElement));
                                break;

                            case "SymbolRef":
                                symbol.AddProduction(GetRefId(subElement));
                                break;

                            case "Production":
                                IReadOnlyList<SymbolId> body = BuildProduction(subElement, out int[] output_order);
                                symbol.AddProduction(body, output_order);
                                break;

                            default:
                                Debug.Fail("Unrecognized symbol type" + subElement.Name);
                                break;
                        }
                    }

                    _symbols.Add(id, symbol);
                    break;

                default:
                    Debug.Fail("Unrecognized symbol type" + element.Name);
                    break;
            }

            if (element.HasAttribute("top") && XmlConvert.ToBoolean(element.Attributes["top"].Value))
            {
                _start.AddProduction(id);
            }

            return id;
        }

        private SymbolId GetRefId(XmlElement element)
        {
            Debug.Assert(element.Name == "SymbolRef");

            Debug.Assert(element.HasAttribute("name"));
            string subName = element.Attributes["name"].Value;

            Debug.Assert(_symbolRefs.ContainsKey(subName));
            return _symbolRefs[subName];
        }

        private IReadOnlyList<SymbolId> BuildProduction(XmlElement element, out int[] output_order)
        {
            Debug.Assert(element.Name == "Production");

            if (element.HasAttribute("output_order"))
            {
                output_order = element.Attributes["output_order"].Value.Split(' ').Select(x => int.Parse(x)).ToArray();
            }
            else
            {
                output_order = null;
            }

            var idList = new List<SymbolId>();
            foreach (XmlElement subElement in element.ChildNodes)
            {
                switch (subElement.Name)
                {
                    case "Terminal":
                    case "Symbol":
                        idList.Add(BuildSymbol(subElement));
                        break;

                    case "SymbolRef":
                        idList.Add(GetRefId(subElement));
                        break;

                    default:
                        Debug.Fail("Unrecognized symbol type" + subElement.Name);
                        break;
                }
            }

            return idList;
        }

        private void CheckGrammar(GrammarDocumentGroup documentGroup)
        {
            var exceptions = new List<Exception>();
            foreach (GrammarDocument document in documentGroup.Items)
            {
                foreach (XmlElement element in document.Content.GetElementsByTagName("Production"))
                {
                    if (element.HasAttribute("output_order"))
                    {
                        string[] tokens = element.Attributes["output_order"].Value.Split(' ');
                        if (tokens.Length != element.ChildNodes.Count)
                        {
                            throw new IllegalOutputOrderException(document.Path);
                        }

                        var found = new bool[tokens.Length];
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            if (int.TryParse(tokens[i], out int number) && number < tokens.Length && !found[number])
                            {
                                found[number] = true;
                            }
                            else
                            {
                                throw new IllegalOutputOrderException(document.Path);
                            }
                        }
                    }
                }
            }

            bool hasTopSymbol = false;
            var symbolRefs = new Dictionary<string, SymbolId>();
            var idGenerator = new SymbolId.Generator();
            void checkSymbolsByTagName(string tagName)
            {
                foreach (GrammarDocument document in documentGroup.Items)
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
                                symbolRefs.Add(name, idGenerator.Next());
                            }
                        }
                        if (element.HasAttribute("top") && XmlConvert.ToBoolean(element.Attributes["top"].Value))
                        {
                            hasTopSymbol = true;
                        }
                    }
                }
            }

            checkSymbolsByTagName("Terminal");
            checkSymbolsByTagName("Symbol");

            foreach (GrammarDocument document in documentGroup.Items)
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

            if (!hasTopSymbol)
            {
                exceptions.Add(new Exception("Grammar has no top level symbol"));
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            _symbolRefs = symbolRefs;
            _idGenerator = idGenerator;
        }
    }
}
