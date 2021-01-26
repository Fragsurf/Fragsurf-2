//
// TextFormatter.cs
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
using System.Text;

namespace Mono.Linq.Expressions {

	public class TextFormatter : IFormatter {

		readonly TextWriter writer;

		bool write_indent;
		int indent;

		public TextFormatter (TextWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			this.writer = writer;
		}

		void WriteIndent ()
		{
			if (!write_indent)
				return;

			for (int i = 0; i < indent; i++)
				writer.Write ("    ");
		}

		public void Write (string str)
		{
			WriteIndent ();
			writer.Write (str);
			write_indent = false;
		}
		
		public void Write (char c)
		{
			WriteIndent ();
			writer.Write (c);
			write_indent = false;
		}
		
		public void WriteLine ()
		{
			writer.WriteLine ();
			write_indent = true;
		}

		public void WriteSpace ()
		{
			Write (" ");
		}

		public void WriteToken (string token)
		{
			Write (token);
		}

		public void WriteToken (char token)
		{
			Write (token);
		}
		
		public void WriteKeyword (string keyword)
		{
			Write (keyword);
		}

		public void WriteLiteral (string literal)
		{
			Write (literal);
		}

		public void WriteReference (string value, object reference)
		{
			Write (value);
		}

		public void WriteIdentifier (string value, object identifier)
		{
			Write (value);
		}

		public void Indent ()
		{
			indent++;
		}

		public void Dedent ()
		{
			indent--;
		}

		public StringBuilder GetStringBuilder() {
			return (writer as StringWriter).GetStringBuilder();
		}

	}
}
