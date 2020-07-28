using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleOf2Cities.Interface;

namespace TaleOf2Cities
{
    public class FileParserReportGenerator : IFileParseReportGenerator

    {
        const string inputFolder = @"..\..\..\Input\";
        const string outPutFolder = @"..\..\..\Output\";
        private static Dictionary<string, int> characterList = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, int> punctuationList = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, int> wordList = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static List<int> searchList = new List<int>();
        private readonly string searchPhrase;
        private static object _lockObject = new object();
        private readonly TaleConfiguration _configuration;

        public FileParserReportGenerator(TaleConfiguration configuration)
        {
            _configuration = configuration;
            if (string.IsNullOrEmpty(_configuration.InputFileName))
                throw new ArgumentNullException(nameof(_configuration.InputFileName));
            searchPhrase = _configuration.SearchKeyWord;
        }

        public void GenerateFileSummary(int topXUsed)
        {

            try
            {
                using (StreamReader sr = new StreamReader(inputFolder + _configuration.InputFileName))
                {
                    string line;
                    int lineCounter = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lineCounter++;
                        Parallel.Invoke(() => GenerateDistinctListOfChar(line), () => GenerateDistinctListOfWordCount(line), () => GenerateListOfWordMatches(line, lineCounter, searchPhrase));
                    }
                    // Sort and write the file 
                    var sortedCharacterList = SortDictionaryToList(characterList, SortOption.Key);
                    var sortedPunctuationList = SortDictionaryToList(punctuationList, SortOption.Value);
                    var sortedWordList = SortDictionaryToList(wordList, SortOption.Key);
                    var sortedWordByWordCountList = SortDictionaryToList(wordList, SortOption.Value);
                    WritetoFile(sortedCharacterList, sortedPunctuationList, sortedWordList, sortedWordByWordCountList, searchList, topXUsed);

                }
            }
            catch (FileNotFoundException fx)
            {
                Console.WriteLine("FIle Not found . Please check the file path:{0}", fx.Message);
            }

        }
        private static void GenerateDistinctListOfChar(string inputLine)
        {
            char[] inputCharacters = inputLine.ToCharArray();
            foreach (var inputchar in inputCharacters)
            {
                string charKey = inputchar.ToString();

                if (Char.IsPunctuation(inputchar))
                {
                    IncrementListCounts(punctuationList, charKey);
                }

                else
                {
                    IncrementListCounts(characterList, charKey);
                }
            }
        }

        private static List<KeyValuePair<string, int>> SortDictionaryToList(Dictionary<string, int> inputDictionary, SortOption sortBy)
        {
            List<KeyValuePair<string, int>> sortedList = new List<KeyValuePair<string, int>>();

            if (sortBy == SortOption.Value)
            {
                foreach (var kvpItem in inputDictionary.OrderBy(wordCount => wordCount.Value))
                {
                    sortedList.Add(new KeyValuePair<string, int>(kvpItem.Key, kvpItem.Value));
                }
            }
            else
            {

                foreach (var kvpItem in inputDictionary.OrderBy(wordCount => wordCount.Key))
                {
                    sortedList.Add(new KeyValuePair<string, int>(kvpItem.Key, kvpItem.Value));
                }
            }
            return sortedList;

        }

        private static void GenerateDistinctListOfWordCount(string inputLine)
        {
            char[] punctuations = { ' ', ',', '.', ':', '\t', '(', ')', '"', '_', '“', '”', '‘', '’', '*', '[', ']', '#', '!', '\r', '\n' };
            var words = inputLine.Split(punctuations, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (IsValidWord(word))
                    IncrementListCounts(wordList, word);
            }
        }


        private static void GenerateListOfWordMatches(string inputLine, int lineNumber, string searchKeywords)
        {
            if (inputLine.Contains(searchKeywords))
            {
                searchList.Add(lineNumber);
            }
        }



        private static void IncrementListCounts(Dictionary<string, int> inputList, string inputKey)
        {

            if (inputList.ContainsKey(inputKey))
            {
                int currentCount = inputList[inputKey];
                currentCount++;
                inputList[inputKey] = currentCount;
            }
            else
            {
                inputList[inputKey] = 1;
            }

        }

        private static bool IsValidWord(string wordInput)
        {
            char[] inputChars = wordInput.ToCharArray();
            for (int wordCounter = 0; wordCounter < inputChars.Length; wordCounter++)
            {
                if (!char.IsLetter(inputChars[wordCounter]))
                    return false;
            }

            return true;
        }
        private void WritetoFile(List<KeyValuePair<string, int>> charList, List<KeyValuePair<string, int>> punctList, List<KeyValuePair<string, int>> alphabeticalWordList, List<KeyValuePair<string, int>> wordCountList, List<int> searchList, int topXUsed)
        {


            try
            {
                using (StreamWriter writer = new StreamWriter(outPutFolder + _configuration.OutputFileNamePrefix))
                {
                    writer.WriteLine("This file contains Summary of Tale of 2 Cities");
                    writer.WriteLine("SC1 Distinct list of all the characters in the file along with their counts.The results should be sorted by character.Begins");
                    writer.WriteLine("Character,Count");
                    foreach (var row in charList)
                        writer.WriteLine(row.Key + "," + row.Value);
                    writer.WriteLine("##################################################################################################################################");
                    writer.WriteLine("SC2 Distinct list of all punctuation in the file along with their counts. The results should be sorted based on punctuation occurrence from most frequent to least frequent..Begins");
                    writer.WriteLine("PunctuationCharacter,Count");
                    for (int listCounter = punctList.Count - 1; listCounter >= 0; listCounter--)
                    {
                        writer.WriteLine(punctList[listCounter].Key + "," + punctList[listCounter].Value);
                    }
                    writer.WriteLine("##################################################################################################################################");

                    writer.WriteLine("SC3 distinct words and their respective counts ordered alphabetically...Begins");
                    writer.WriteLine("Word,Count");
                    foreach (var row in alphabeticalWordList)
                        writer.WriteLine(row.Key + "," + row.Value);
                    writer.WriteLine("##################################################################################################################################");

                    writer.WriteLine("SC4 list of matches with the matching line number based on a given “search request”....Begins");
                    writer.WriteLine("LineNumber");
                    for (int listCounter = 0; listCounter < searchList.Count; listCounter++)
                    {
                        writer.WriteLine(searchList[listCounter]);
                    }
                    writer.WriteLine("##################################################################################################################################");

                    writer.WriteLine("SC5 X used words (with an option to include or exclude conjunctions) and their counts where 'X' is a passed in parameter.Begins");
                    writer.WriteLine("TopxWord,Count");
                    int topCounterLimit = wordCountList.Count - topXUsed;
                    for (int listCounter = wordCountList.Count - 1; listCounter >= topCounterLimit; listCounter--)
                    {
                        writer.WriteLine(wordCountList[listCounter].Key + "," + wordCountList[listCounter].Value);
                    }
                    writer.WriteLine("##################################################################################################################################");

                }
            }
            catch (Exception exp)
            {
                Console.Write(exp.Message);
            }

        }

    }
}
