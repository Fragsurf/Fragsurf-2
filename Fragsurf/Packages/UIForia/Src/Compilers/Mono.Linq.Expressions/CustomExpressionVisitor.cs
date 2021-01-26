//
// CustomExpressionVisitor.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2010 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Linq.Expressions;

namespace Mono.Linq.Expressions {

	public abstract class CustomExpressionVisitor : ExpressionVisitor {

		protected override Expression VisitExtension (Expression node)
		{
			var custom_node = node as CustomExpression;
			if (custom_node != null)
				return Visit (custom_node);

			return base.VisitExtension (node);
		}

		protected Expression Visit (CustomExpression node)
		{
			if (node == null)
				return null;

			return node.Accept (this);
		}

		protected internal virtual Expression VisitDoWhileExpression (DoWhileExpression node)
		{
			return node.Update (
				Visit (node.Test),
				Visit (node.Body),
				node.BreakTarget,
				node.ContinueTarget);
		}

		protected internal virtual Expression VisitForExpression (ForExpression node)
		{
			return node.Update (
				(ParameterExpression) Visit (node.Variable),
				Visit (node.Initializer),
				Visit (node.Test),
				Visit (node.Step),
				Visit (node.Body),
				node.BreakTarget,
				node.ContinueTarget);
		}

		protected internal virtual Expression VisitForEachExpression (ForEachExpression node)
		{
			return node.Update (
				(ParameterExpression) Visit (node.Variable),
				Visit (node.Enumerable),
				Visit (node.Body),
				node.BreakTarget,
				node.ContinueTarget);
		}

		protected internal virtual Expression VisitUsingExpression (UsingExpression node)
		{
			return node.Update (
				(ParameterExpression) Visit (node.Variable),
				Visit (node.Disposable),
				Visit (node.Body));
		}

		protected internal virtual Expression VisitWhileExpression (WhileExpression node)
		{
			return node.Update (
				Visit (node.Test),
				Visit (node.Body),
				node.BreakTarget,
				node.ContinueTarget);
		}
	}
}
