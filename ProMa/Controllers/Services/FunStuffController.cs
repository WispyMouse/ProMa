using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ProMa.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace ProMa.Controllers
{
	public class FunStuffController : Controller
    {
		private static List<string> sentenceEnders = new List<string>() { ".", "!", "?", "\\r\\n" };

		[HttpPost]
		public string MarkovPostedNote([FromForm]int noteTypeId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			Dictionary<string, List<string>> MarkovTree = new Dictionary<string, List<string>>();

			MarkovTree.Add("_start", new List<string>());
			MarkovTree.Add("_end", new List<string>());

			List<string> notes = new List<string>();

			using (ProMaDB scope = new ProMaDB())
			{
				notes = scope.PostedNotes.Where(x => (x.NoteTypeId == noteTypeId || noteTypeId == -1) && x.UserId == user.UserId).Where(x => x.Active == true).Select(x => x.NoteText).ToList();
			}

			foreach (string curNote in notes)
			{
				AddPhraseToMarkovChain(curNote, MarkovTree);
			}

			if (MarkovTree["_start"].Count() == 0)
			{
				return "(no notes?)";
			}

			string phrase = "";

			Random rand = new Random();
			string nextWord = MarkovTree["_start"][rand.Next(MarkovTree["_start"].Count())];
			phrase += UpperCaseFirstLetter(nextWord);

			if (MarkovTree.ContainsKey(nextWord))
			{
				phrase += " ";
			}

			while (MarkovTree.ContainsKey(nextWord))
			{
				string dictionaryKey = GetDictionaryKeyForWord(nextWord);

				nextWord = MarkovTree[dictionaryKey][rand.Next(MarkovTree[nextWord].Count())];

				string useThisWord = nextWord;

				phrase += useThisWord;

				if (MarkovTree.ContainsKey(nextWord))
				{
					phrase += " ";
				}
			}

			return phrase;
		}

		void AddPhraseToMarkovChain(string phrase, Dictionary<string, List<string>> MarkovTree)
		{
			// lowercase the phrase if it's not all caps; this could be an acronym, or just the word "I"
			if (phrase != phrase.ToUpper())
			{
				phrase = phrase.ToLower();
			}

			phrase = Regex.Replace(phrase, @"(<br/>|<br>|\(|\)|""|'|\[\[[^>]*\]\])", " ");

			int firstEnderIndex = -1;
			int firstEnderLength = -1;

			for (int ii = 0; ii <= phrase.Length - 1; ii++)
			{
				foreach (string curEnder in sentenceEnders)
				{
					if (phrase.Substring(ii).StartsWith(curEnder))
					{
						if (firstEnderIndex != -1)
						{
							AddPhraseToMarkovChain(phrase.Substring(0, firstEnderIndex), MarkovTree);

							if (phrase.Length > firstEnderIndex + firstEnderLength + curEnder.Length)
							{
								string remainingString = phrase.Substring(firstEnderIndex, phrase.Length - firstEnderIndex);

								AddPhraseToMarkovChain(remainingString, MarkovTree);
							}

							return;
						}
						else
						{
							firstEnderIndex = ii + curEnder.Length;
							firstEnderLength = curEnder.Length;
						}
					}
				}
			}

			bool isStoppedProperly = false;
			foreach (string curEnder in sentenceEnders)
			{
				if (phrase == curEnder)
					return;

				if (phrase.EndsWith(curEnder))
				{
					isStoppedProperly = true;
					break;
				}
			}

			if (!isStoppedProperly)
			{
				phrase = phrase.Trim() + ".";
			}

			List<string> words = phrase.Split(new char[] { ' ' }).ToList();

			// let's remove all 'words' that are just spaces
			for (int ii = words.Count - 1; ii >= 0; ii--)
			{
				string curWord = words[ii].Trim();

				if (string.IsNullOrEmpty(curWord))
				{
					words.RemoveAt(ii);
				}
			}

			for (int ii = 0; ii < words.Count; ii++)
			{
				string curWord = words[ii];

				curWord = curWord.Trim();

				if (!string.IsNullOrEmpty(curWord))
				{
					if (ii == 0 && !MarkovTree["_start"].Contains(curWord)) // add to the list of sentence starters
					{
						MarkovTree["_start"].Add(curWord);
					}

					if (ii == words.Count - 1) // the last word has nothing after it...
					{
						MarkovTree["_end"].Add(curWord);
					}
					else
					{
						string dictionaryKey = GetDictionaryKeyForWord(curWord);

						if (!MarkovTree.ContainsKey(dictionaryKey))
						{
							MarkovTree.Add(dictionaryKey, new List<string>());
						}

						string nextWord = words[ii + 1];
						if (!MarkovTree[dictionaryKey].Contains(nextWord))
						{
							MarkovTree[dictionaryKey].Add(nextWord);
						}
					}
				}
			}
		}

		string GetDictionaryKeyForWord(string word)
		{
			if (word.StartsWith("[[image"))
			{
				return "_image";
			}
			else if (word.StartsWith("[["))
			{
				return "_link";
			}

			return word;
		}

		string UpperCaseFirstLetter(string word)
		{
			return Regex.Replace(word, "(.*?)\\w.+", "$1") + Regex.Replace(word, ".*?(\\w).+", "$1").ToUpper() + Regex.Replace(word, ".*?\\w(.+)", "$1");
		}
	}
}
