using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace CFGParser
{
    internal abstract class Symbol
    {
        public string Name { get; set; }
        public SymbolId Id { get; protected set; }

        public string GetNameText()
        {
            return (Name ?? Grammar.ReservedCharacter + Id.ToString()).AddQuote();
        }

        abstract public string ToText(Dictionary<SymbolId, Symbol> symbols);
    }

    internal class TerminalSymbol : Symbol
    {
        public readonly string Input;
        public readonly string Output;

        public TerminalSymbol(SymbolId id, string input, string output, string name = null)
        {
            Id = id;
            Name = name;
            Input = input;
            Output = output;
        }

        public override string ToText(Dictionary<SymbolId, Symbol> symbols)
        {
            var builder = new StringBuilder();
            builder.Append(GetNameText());
            builder.Append(": ");
            builder.Append(Input.AddQuote());
            builder.Append(" => ");
            builder.Append(Output.AddQuote());
            return builder.ToString();
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

    internal class NonTerminalSymbol : Symbol
    {
        readonly List<Production> _productions;

        public IReadOnlyCollection<Production> Productions => _productions;
        public bool IsValid => _productions.Count != 0;

        public NonTerminalSymbol(SymbolId id, string name = null)
        {
            Id = id;
            Name = name;
            _productions = new List<Production>();
        }

        public override string ToText(Dictionary<SymbolId, Symbol> symbols)
        {
            var builder = new StringBuilder();
            string nameText = GetNameText();

            builder.Append(nameText);
            builder.Append(" -> ");

            string pad = '\n' + new string(' ', nameText.Length) + "  | ";
            builder.AppendJoin(pad, _productions.Select(x => x.ToText(symbols)));

            return builder.ToString();
        }
        public void AddProduction(IReadOnlyList<SymbolId> body, int[] output_order = null)
        {
            _productions.Add(new Production(this, body, output_order));
        }

        public void AddProduction(SymbolId id)
        {
            _productions.Add(new Production(this, id));
        }
    }
}
