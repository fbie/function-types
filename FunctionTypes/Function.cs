// -*- c-basic-offset: 4; indent-tabs-mode: nil-*-
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FunctionTypes
{
    public class Function
    {
        /// <summary>
        ///   Convert between Funcalc and .Net types
        /// </summary>
        private static class TypeConversion {
            public readonly static Type VALUE_T = typeof(Value);

            static readonly Dictionary<Type, Type> typeMap = new Dictionary<Type, Type>
                {
                    { typeof(double), typeof(NumberValue) },
                    { typeof(string), typeof(TextValue) }
                };

            /// <summary>
            ///   Maps a .Net type to a Funcalc type.
            /// </summary>
            public static Type ToFuncalc(Type t)
            {
                if (t.BaseType == VALUE_T)
                    return t;
                return typeMap[t];
            }

            static readonly Dictionary<Type, MethodInfo> dotNetToFuncalc = new Dictionary<Type, MethodInfo>
                {
                    { typeof(double), VALUE_T.GetMethod("AsDouble") },
                    { typeof(string), VALUE_T.GetMethod("AsString") },
                };

            /// <summary>
            ///   Returns the MethodInfo for a .Net to Funcalc type
            ///   conversion.
            /// </summary>
            public static MethodInfo FuncalcConversion(Type t)
            {
                return dotNetToFuncalc[t];
            }
        }

        readonly Delegate func; // The actual function.
        readonly Type[] parameters; // Actual type parameters.
        readonly Type[] funcalcParameters; // Funcalc type parameters.
        readonly Type returns;
        readonly Func<Value[], Value> call; // The generated call that performs all type conversions.

        public Function(Delegate func)
        {
            this.func = func;
            MethodInfo meta = func.Method;
            parameters = meta.GetParameters().Select(pi => pi.ParameterType).ToArray();
            funcalcParameters = parameters.Select(TypeConversion.ToFuncalc).ToArray();
            returns = meta.ReturnType;
            call = GenerateCall(parameters, meta);
        }

        public Type[] FuncalcParameters
        {
            get { return funcalcParameters; }
        }

        public Type[] Parameters
        {
            get { return parameters; }
        }

        public Type FuncalcReturns
        {
            get { return TypeConversion.ToFuncalc(returns); }
        }

        public Type Returns
        {
            get { return returns; }
        }

        /// <summary>
        ///   Invoke the built-in function, performing all type checks and conversions.
        /// </summary>
        public Value Call(Value[] vs)
        {
            if (vs.Length != FuncalcParameters.Length)
                return ErrorValue.ArgNum;
            // The following lines could be generated to use 'is' instead.
            for (int i = 0; i < vs.Length; ++i) {
                Value v = vs[i];
                if (v is ErrorValue)
                    return v;
                if (v.GetType() != funcalcParameters[i])
                    return ErrorValue.ArgType;
            }
            return call(vs);
        }

        /// <summary>
        ///   Generates a function that performs all type conversions,
        ///   assuming that the types are correct. Called once per
        ///   registered function.
        /// </summary>
        private static Func<Value[], Value> GenerateCall(Type[] parameters, MethodInfo meta)
        {
            ParameterExpression vs = Expression.Parameter(typeof(Value[]), "vs");
            Expression[] casts = new Expression[parameters.Length];
            // Convert types after conversion rules, if necessary.
            for (int i = 0; i < casts.Length; ++i) {
                Expression v = Expression.ArrayIndex(vs, Expression.Constant(i, typeof(int)));
                if (parameters[i].BaseType == TypeConversion.VALUE_T)
                    casts[i] = Expression.Convert(v, parameters[i]);
                else
                    casts[i] = Expression.Call(v, TypeConversion.FuncalcConversion(parameters[i]));
            }
            Expression call = Expression.Call(meta, casts);
            return Expression.Lambda(call, vs).Compile() as Func<Value[], Value>;
        }

        /// <summary>
        ///   True if the complete signatures match.
        /// </summary>
        public bool SignaturesMatch(Function other)
        {
            return Returns == other.Returns && Parameters.Zip(other.Parameters, (l, r) => l == r).Aggregate((v0, v1) => v0 && v1);
        }

        /// <summary>
        ///   True if the return type of this matches the single
        ///   parameter of other.
        /// </summary>
        public bool TypesMatch(Function other)
        {
            return other.Parameters.Length == 1 && other.Parameters[0] == Returns;
        }

        /// <summary>
        ///   Concatenate this with andThen and return a new function
        ///   instance that performs no intermediate typechecks.
        /// </summary>
        public Function Concat(Function andThen)
        {
            if (TypesMatch(andThen))
                return new Function(Delegate.Combine(func, andThen.func));
            else
                throw new Exception("Incompatible function signatures!");
        }
    }

}
