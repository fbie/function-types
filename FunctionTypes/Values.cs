// -*- c-basic-offset: 4; indent-tabs-mode: nil-*-

using System.Collections.Generic;

namespace FunctionTypes
{
    public abstract class Value
    {
        public virtual double AsDouble()
        {
            return double.NaN;
        }

        public virtual string AsString()
        {
            return "";
        }
    }

    public class NumberValue : Value
    {
        public readonly double value;

        public NumberValue(double n)
        {
            value = n;
        }

        public override double AsDouble()
        {
            return value;
        }
    }

    public class TextValue : Value
    {
        public readonly string value;

        public TextValue(string text)
        {
            value = text;
        }

        public override string AsString()
        {
            return value;
        }
    }

    public class ErrorValue : Value
    {
        public readonly double errorNan;

        private ErrorValue(double errorNan)
        {
            this.errorNan = errorNan;
        }

        public static readonly ErrorValue ArgType = new ErrorValue(double.NaN);
        public static readonly ErrorValue ArgNum = new ErrorValue(double.NaN);
        public static readonly ErrorValue ArgNull = new ErrorValue(double.NaN);
    }

}
