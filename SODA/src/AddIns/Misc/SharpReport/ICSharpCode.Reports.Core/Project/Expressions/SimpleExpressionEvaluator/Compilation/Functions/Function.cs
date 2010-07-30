#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion

using System.Collections.Generic;
using SimpleExpressionEvaluator.Compilation;
using SimpleExpressionEvaluator.Evaluation;

namespace SimpleExpressionEvaluator.Compilation.Functions
{
    /// <summary>
    /// Summary description for Function.
    /// </summary>
    [NodeType(ExpressionNodeType.Function)]
    public abstract class Function<T> : FunctionBase<T>
    {
        protected Function(): this(ExpressionNodeType.Function)
        {
        }

        protected Function(ExpressionNodeType nodeType):base(nodeType)
        {
            Arguments = new List<IExpression>();
        }

        public override T Evaluate(IExpressionContext context)
        {
            var args = new object[Arguments.Count];
            for (int i = 0; i < args.Length; i++)
                args[i] = Arguments[i].Evaluate(context);
            return EvaluateFunction(args);
        }

        protected abstract T EvaluateFunction(params object[] args);

    }
}