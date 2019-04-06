using System;
using System.Collections.Generic;
using System.Linq;

namespace CFGParser
{
    internal interface ISymbol
    {
        string Name { get; }
        SymbolId Id { get; }
    }

    internal struct TerminalSymbol : ISymbol
    {
        public string Name { get; set; }
        public SymbolId Id { get; private set; }
        public readonly string Input;
        public readonly string Output;

        public TerminalSymbol(SymbolId id, string input, string output, string name = null)
        {
            Id = id;
            Name = name;
            Input = input;
            Output = output;
        }

        public bool MatchInput(string str)
        {
            return str.StartsWith(Input);
        }

        public bool MatchInput(string str, int startIndex)
        {
            return str.Substring(startIndex).StartsWith(Input);
        }
    }

    internal struct NonTerminalSymbol : ISymbol
    {
        readonly List<Production> _productions;

        public string Name { get; set; }
        public SymbolId Id { get; private set; }
        public IReadOnlyCollection<Production> Productions => _productions;
        public bool IsValid => _productions.Count != 0;

        public NonTerminalSymbol(SymbolId id, string name = null)
        {
            Id = id;
            Name = name;
            _productions = new List<Production>();
        }

        public void AddProduction(IReadOnlyList<SymbolId> body)
        {
            _productions.Add(new Production(this, body));
        }
    }
}
