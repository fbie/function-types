// -*- c-basic-offset: 4; indent-tabs-mode: nil-*-

namespace FunctionTypes
{
    public abstract class Value
    {
    }

    public class NumberValue : Value
    {
        public readonly double value;

        public NumberValue(double n)
        {
            value = n;
        }
    }

    public class TextValue : Value
    {
        public readonly string value;

        public TextValue(string text)
        {
            value = text;
        }
    }

    public class ErrorValue : Value
    {
        private ErrorValue()
        {
        }

        public static readonly ErrorValue ArgType = new ErrorValue();
        public static readonly ErrorValue ArgNum = new ErrorValue();
        public static readonly ErrorValue ArgNull = new ErrorValue();
    }
}
