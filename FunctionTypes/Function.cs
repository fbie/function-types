// -*- c-basic-offset: 4; indent-tabs-mode: nil-*-
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FunctionTypes
{
    public class Function
    {
        readonly Delegate func;
        readonly MethodInfo meta;
        readonly Type[] parameters;
        readonly Type returns;
        readonly Func<Value[], Value> call;

        public Function(Delegate func)
        {
            this.func = func;
            this.meta = func.Method;
            parameters = this.meta.GetParameters().Select(pi => pi.ParameterType).ToArray();
            returns = this.meta.ReturnType;
            call = MakeCall();
        }

        public Type[] Parameters
        {
            get { return parameters; }
        }

        public Type Returns
        {
            get { return returns; }
        }

        public Value Call(Value[] vs)
        {
            if (vs.Length != Parameters.Length)
                return ErrorValue.ArgNum;
            // The following lines could be generated to use 'is' instead.
            for (int i = 0; i < vs.Length; ++i) {
                Value v = vs[i];
                if (v is ErrorValue)
                    return v;
                if (v.GetType() != parameters[i])
                    return ErrorValue.ArgType;
            }
            return call(vs);
        }

        // Called once per registered function.
        private Func<Value[], Value> MakeCall()
        {
            ParameterExpression values = Expression.Parameter(typeof(Value[]), "vs");
            Expression[] casts = new Expression[parameters.Length];
            for (int i = 0; i < casts.Length; ++i)
                casts[i] = Expression.Convert(Expression.ArrayIndex(values, Expression.Constant(i, typeof(int))), parameters[i]);
            Expression call = Expression.Call(meta, casts);
            return Expression.Lambda(call, values).Compile() as Func<Value[], Value>;
        }

        public bool MatchTypes(Function other)
        {
            return Returns == other.Returns && Parameters.Zip(other.Parameters, (l, r) => l == r).Aggregate((v0, v1) => v0 && v1);
        }

        public Function Concat(Function andThen)
        {
            if (MatchTypes(andThen))
                return new Function(Delegate.Combine(func, andThen.func));
            else
                throw new Exception("Incompatible function signatures!");
        }
    }
}
