using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DTCommandPalette {
	public class CommandManager {
		// PRAGMA MARK - Constructors
		public void AddCommand(ICommand command) {
			loadedObjects_.Add(command);
		}

		public void AddLoader(ICommandLoader loader) {
			// Add all objects from loaded into _loadedObjects
			ICommand[] objects = loader.Load();
			loadedObjects_.AddRange(objects);
		}

		// PRAGMA MARK - Public Interface
		public ICommand[] ObjectsSortedByMatch(string input) {
			List<ICommand> objectsCopy = new List<ICommand>(loadedObjects_);

			Dictionary<string, double> cachedDistances = new Dictionary<string, double>();
			foreach (ICommand command in objectsCopy) {
				cachedDistances[command.DisplayTitle] = ScoreFor(command, input);
			}

			objectsCopy.Sort(delegate (ICommand objA, ICommand objB) {
				double distanceA = cachedDistances[objA.DisplayTitle];
				double distanceB = cachedDistances[objB.DisplayTitle];
				return distanceA.CompareTo(distanceB);
			});

			return objectsCopy.ToArray();
		}

		public double ScoreFor(ICommand command, string input) {
			string inputLowercase = input.ToLower();

			string displayTitle = Path.GetFileNameWithoutExtension(command.DisplayTitle);
			string displayTitleLowercase = displayTitle.ToLower();

			float editDistance = ComparisonUtil.EditDistance(displayTitleLowercase, inputLowercase);

			string longestCommonSubstring = ComparisonUtil.LongestCommonSubstring(displayTitleLowercase, inputLowercase);
			float substringLength = longestCommonSubstring.Length;
			float substringIndex = displayTitleLowercase.IndexOf(longestCommonSubstring);

			double score = 0;
			score += 0.05f * editDistance;
			score += 2.0f * -substringLength;
			score += substringIndex;
			score -= command.SortingPriority;

			return score;
		}


		// PRAGMA MARK - Internal
		private List<ICommand> loadedObjects_ = new List<ICommand>();
	}
}
