using System;
using System.Collections.Generic;
using System.Linq;

namespace CFGParser
{
    internal struct Production
    {
        readonly SymbolId[] _body;

        public readonly NonTerminalSymbol Symbol;
        public IReadOnlyList<SymbolId> Body => _body;

        public Production(NonTerminalSymbol symbol, IReadOnlyList<SymbolId> body)
        {
            Symbol = symbol;
            _body = body.ToArray();
        }
    }

    internal struct ProductionProgress
    {
        public readonly Production Production;
        public readonly int Progress;
        public readonly int StartIndex;

        public ProductionProgress(Production production, int progress, int startIndex)
        {
            Production = production;
            Progress = progress;
            StartIndex = startIndex;
        }
    }
}
