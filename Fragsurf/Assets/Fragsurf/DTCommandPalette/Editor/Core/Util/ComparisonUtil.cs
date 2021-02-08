using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DTCommandPalette {
	public static class ComparisonUtil {
		/// <summary>
		/// Computes the Levenshtein Edit Distance between two enumerables.
		/// </summary>
		public static int EditDistance<T>(IEnumerable<T> x, IEnumerable<T> y) where T : IEquatable<T> {
			// Validate parameters
			if (x == null) {
				throw new ArgumentNullException("x");
			}

			if (y == null) {
				throw new ArgumentNullException("y");
			}

			// Convert the parameters into IList instances
			// in order to obtain indexing capabilities
			IList<T> first = x as IList<T> ?? new List<T>(x);
			IList<T> second = y as IList<T> ?? new List<T>(y);

			// Get the length of both.  If either is 0, return
			// the length of the other, since that number of insertions
			// would be required.
			int n = first.Count, m = second.Count;
			if (n == 0) {
				return m;
			}

			if (m == 0) {
				return n;
			}

			// Rather than maintain an entire matrix (which would require O(n*m) space),
			// just store the current row and the next row, each of which has a length m+1,
			// so just O(m) space. Initialize the current row.
			int curRow = 0, nextRow = 1;
			int[][] rows = new int[][] { new int[m + 1], new int[m + 1] };
			for (int j = 0; j <= m; ++j) {
				rows[curRow][j] = j;
			}

			// For each virtual row (since we only have physical storage for two)
			for (int i = 1; i <= n; ++i) {
				// Fill in the values in the row
				rows[nextRow][0] = i;
				for (int j = 1; j <= m; ++j) {
					int dist1 = rows[curRow][j] + 1;
					int dist2 = rows[nextRow][j - 1] + 1;
					int dist3 = rows[curRow][j - 1] +
					(first[i - 1].Equals(second[j - 1]) ? 0 : 1);

					rows[nextRow][j] = Math.Min(dist1, Math.Min(dist2, dist3));
				}

				// Swap the current and next rows
				if (curRow == 0) {
					curRow = 1;
					nextRow = 0;
				} else {
					curRow = 0;
					nextRow = 1;
				}
			}

			// Return the computed edit distance
			return rows[curRow][m];
		}

		public static int LongestCommonSubstringLength(string x, string y) {
			if (String.IsNullOrEmpty(x) || String.IsNullOrEmpty(y)) {
				return 0;
			}
			return x.LongestCommonSubstring(y).Length;
		}

		public static string LongestCommonSubstring(this string source, string target) {
			if (String.IsNullOrEmpty(source) || String.IsNullOrEmpty(target)) { return ""; }

			int[,] L = new int[source.Length, target.Length];
			int maximumLength = 0;
			int lastSubsBegin = 0;
			StringBuilder stringBuilder = new StringBuilder();

			for (int i = 0; i < source.Length; i++) {
				for (int j = 0; j < target.Length; j++) {
					if (source[i] != target[j]) {
						L[i, j] = 0;
					} else {
						if ((i == 0) || (j == 0)) {
							L[i, j] = 1;
						} else {
							L[i, j] = 1 + L[i - 1, j - 1];
						}

						if (L[i, j] > maximumLength) {
							maximumLength = L[i, j];
							int thisSubsBegin = i - L[i, j] + 1;
							if (lastSubsBegin == thisSubsBegin) { // if the current LCS is the same as the last time this block ran
								stringBuilder.Append(source[i]);
							} else { // this block resets the string builder if a different LCS is found
								lastSubsBegin = thisSubsBegin;
								stringBuilder.Length = 0; // clear it
								stringBuilder.Append(source.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
							}
						}
					}
				}
			}

			return stringBuilder.ToString();
		}
	}
}
