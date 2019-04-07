using System;

namespace CFGParser
{
    internal struct SymbolId
    {
        readonly int _value;
        private SymbolId(int value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is SymbolId)
            {
                return _value == ((SymbolId)obj)._value;
            }
            else
            {
                return false;
            }
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
