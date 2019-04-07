using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace CFGParser
{
    internal class Production
    {
        readonly SymbolId[] _body;
        readonly int[] _outputOrder;

        public readonly NonTerminalSymbol Symbol;
        public IReadOnlyList<SymbolId> Body => _body;
        public IReadOnlyList<int> OutputOrder => _outputOrder;

        private Production() { }

        public Production(NonTerminalSymbol symbol, IReadOnlyList<SymbolId> body, IReadOnlyList<int> outputOrder)
        {
            Symbol = symbol;
            _body = body.ToArray();
            _outputOrder = outputOrder?.ToArray();
        }

        public Production(NonTerminalSymbol symbol, SymbolId id)
        {
            Symbol = symbol;
            _body = new SymbolId[] { id };
        }

        public string ToText(Dictionary<SymbolId, Symbol> symbols)
        {
            var builder = new StringBuilder();
            builder.AppendJoin(' ', _body.Select(x => symbols[x].GetNameText()));

            if (_outputOrder != null)
            {
                builder.Append(" (output_order: ");
                builder.AppendJoin(' ', _outputOrder.Select(x => x.ToString()));
                builder.Append(")");
            }

            return builder.ToString();
        }
    }

    internal class ProductionProgress
    {
        public readonly Production Production;
        public readonly int Progress;
        public readonly int StartIndex;

        private ProductionProgress() { }

        public ProductionProgress(Production production, int progress, int startIndex)
        {
            Production = production;
            Progress = progress;
            StartIndex = startIndex;
        }
    }
}
