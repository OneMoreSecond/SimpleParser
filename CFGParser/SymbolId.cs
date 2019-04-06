using System;
using System.Collections.Generic;
using System.Text;

namespace CFGParser
{
    internal struct SymbolId
    {
        readonly int _value;
        private SymbolId(int value)
        {
            _value = value;
        }
        internal class Generator
        {
            int _nextValue = 0;

            public SymbolId Next()
            {
                if (_nextValue == int.MaxValue)
                {
                    throw new Exception("Too many symbols");
                }
                return new SymbolId(_nextValue++);
            }
        }
    }

}
