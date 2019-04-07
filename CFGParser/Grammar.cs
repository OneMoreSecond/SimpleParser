using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace CFGParser
{
    partial class Grammar
    {
        Dictionary<SymbolId, Symbol> _symbols;
        NonTerminalSymbol _start;

        public void Parse(string str)
        {
            throw new NotImplementedException();

            //if (_start == null)
            //{
            //    throw new InvalidOperationException("Grammar not loaded");
            //}

            //_start = new Production(null, id);

            //var progresses = new List<ProductionProgress>[str.Length + 1];
            //for (int i = 0; i < progresses.Length; i++)
            //{
            //    progresses[i] = new List<ProductionProgress>();
            //}

            //progresses[0].Add(new ProductionProgress(_start, 0, 0));

            //var histories = new Dictionary<SymbolId, ProductionProgress>[str.Length];
            //for (int i = 0; i < progresses.Length; i++)
            //{
            //    for (int j = 0; j < progresses[i].Count; j++)
            //    {
            //        Production rule = progresses[i][j].Production;
            //        if (progresses[i][j].Progress == rule.Body.Count)
            //        {
            //            // Complete
            //            ;
            //        }
            //    }
            //}
        }
    }
}
