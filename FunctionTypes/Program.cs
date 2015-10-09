// -*- c-basic-offset: 4; indent-tabs-mode: nil-*-
using System;
using System.Diagnostics;
using System.Text;

namespace FunctionTypes
{
    class MainClass
    {
        public delegate R Nullary<R>() where R : Value;
        public delegate R Unary<V, R>(V v0) where R : Value;
        public delegate R Binary<V, T, R>(V v0, T v1) where R : Value;

        public static NumberValue Add(double n0, double n1)
        {
            return new NumberValue(n0 + n1);
        }

        public static TextValue Repeat(NumberValue n, string t)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = (int)n.value; 0 < i; --i)
                sb.Append(t);
            return new TextValue(sb.ToString());
        }

        public static void Main(string[] args)
        {
            Function add = new Function(new Binary<double, double, NumberValue>(Add));
            Function repeat = new Function(new Binary<NumberValue, string, TextValue>(Repeat));
            Value v0 = add.Call(new Value[]{ new NumberValue(1), new NumberValue(1) });
            Debug.Assert(v0 != null && v0 is NumberValue);
            System.Console.WriteLine("add(1, 1) = " + (v0 as NumberValue).value);
            Value v1 = add.Call(new Value[]{ new NumberValue(1), new TextValue("foo") });
            Debug.Assert(v1 is ErrorValue);
            Value v3 = repeat.Call(new Value[]{});
            Debug.Assert(v3 is ErrorValue);
            Value v4 = repeat.Call(new Value[]{ new NumberValue(14), new TextValue("ab") });
            Debug.Assert(v4 is TextValue);
            System.Console.WriteLine("repeat(14, \"ab\") = " + (v4 as TextValue).value);
        }
    }
}
