//
// CSharp.cs
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

using System;
using System.IO;
using System.Linq.Expressions;

namespace Mono.Linq.Expressions {

    public class TemplateWriter : CSharpWriter {

        public TemplateWriter(IFormatter formatter) : base(formatter) { }

        public override void Write(LambdaExpression expression) {
            VisitTemplateSignature(expression);
            Indent();
            Indent();
            VisitLambdaBody(expression);
            Dedent();
            Dedent();
            WriteToken(";");
        }

        private void VisitTemplateSignature(LambdaExpression node) {
            VisitParameters(node);
            WriteSpace();
            WriteToken("=>");
            WriteLine();
        }

    }
    
    public static class CSharp {

        public static string ToCSharpCode(this Expression self) {
            if (self == null) {
                throw new ArgumentNullException(nameof(self));
            }

            return ToCode(writer => writer.Write(self));
        }

        public static string ToCSharpCode(this LambdaExpression self) {
            if (self == null) {
                throw new ArgumentNullException(nameof(self));
            }

            return ToCode(writer => writer.Write(self));
        }

        public static string ToTemplateBodyFunction(this LambdaExpression self) {
            if (self == null) {
                throw new ArgumentNullException(nameof(self));
            }

            return ToLambda(writer => writer.Write(self));
        }

        static string ToLambda(Action<TemplateWriter> writer) {
            var @string = new StringWriter();
            var csharp = new TemplateWriter(new TextFormatter(@string));

            writer(csharp);

            return @string.ToString();
        }
        
        static string ToCode(Action<CSharpWriter> writer) {
            var @string = new StringWriter();
            var csharp = new CSharpWriter(new TextFormatter(@string));

            writer(csharp);

            return @string.ToString();
        }

    }

}