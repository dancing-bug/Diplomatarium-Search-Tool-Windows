using CorpusSearchEngine.CustomElements;
using CorpusSearchEngine.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace CorpusSearchEngine
{
    public partial class MainWindow : Window
    {
        int cnt = 1;
        string word = string.Empty;
        List<string> words = new List<string>();
        bool isSearching = false;
        int wordsCount = 0;
        int bufferSize = 1024 * 1024; // 1 MB buffer size
        SearchMethods searchMethod = SearchMethods.StreamBlocks;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            StartSearch();
        }

        private void searchTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartSearch();
            }

        }

        async void StartSearch()
        {
            if (!isSearching)
            {
                isSearching = true;

                words = searchTextbox.Text.Split(',').ToList();
                canvasStackPanel.Children.Clear();

                await Task.Run(() => SearchForWords(words, searchMethod));
            }
        }

        private List<int> GetWordIndexesWithStreamBlocks(string path,string word)
        {
            IEnumerable<int> firstCollection = FindWordIndexesInFile(path, word, bufferSize);
            IEnumerable<int> secondCollection = FindWordIndexesInFile(path, word, bufferSize+83);
            List<int> mergedCollection = firstCollection.Concat(secondCollection).Distinct().OrderBy(x => x).ToList();
            wordsCount = mergedCollection.Count();

            return mergedCollection;
        }
        private IEnumerable<int> FindWordIndexesInFile(string filePath, string word, int bufferSize)
        {
            int wordsNumber = 0;

            using (var fileStream = File.OpenRead(filePath))
            using (var streamReader = new StreamReader(fileStream))
            {
                char[] buffer = new char[bufferSize];
                int charsRead;

                int offset = 0;
                while ((charsRead = streamReader.ReadBlock(buffer, 0, bufferSize)) > 0)
                {
                    string chunk = new string(buffer, 0, charsRead);

                    int index = 0;
                    while (index < charsRead)
                    {
                        index = chunk.IndexOf(word, index, StringComparison.Ordinal);

                        if (index == -1)
                            break;

                        yield return offset + index; //absolute index
                        wordsNumber++;
                        index += word.Length;
                    }

                    offset += charsRead;
                }
            }
        }

        private string ReadFile(string path)
        {
            using (var reader = File.OpenText(path))
            {
                return reader.ReadToEnd();
            }
        }

        public void UpdateResultText(string text)
        {
            resultText.Text = text;
        }

        private void AddNewEntry(string bind, string number, string content)
        {
            canvasStackPanel.Children.Add(new TextTab { bind = bind, number = number, content = content});
        }

        public void AddNewEntry(string bind, string number, string[] content)
        {
            canvasStackPanel.Children.Add(new TextTab { bind = bind, number = number, highlightedContent = content});
        }

        public void AddNewExpander(string word)
        {
            canvasStackPanel.Children.Add(new CustomExpander { word = word , isExpanded = false });
        }

        private string[] DivideText(string text, string word)
        {
            int index = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);

            if (index == -1)
            {
                // Word not found in the text
                return new string[] { text, string.Empty, string.Empty };
            }

            string[] dividedParts = new string[3];

            dividedParts[0] = text.Substring(0, index);
            dividedParts[1] = text.Substring(index, word.Length);
            dividedParts[2] = text.Substring(index + word.Length);

            return dividedParts;
        }

        private string ReplaceLineBreaks(string input)
        {
            input = input.Replace("<br>", "\n");
            input = System.Text.RegularExpressions.Regex.Replace(input, "<.{0,20}>", "");

            /*input = input.Replace("<td>", "");
            input = input.Replace("<tr>", "");
            input = input.Replace("<b>", "");
            input = input.Replace("<i>", "");
            input = input.Replace("<font size=\"-1\">", "");
            input = input.Replace("<font>", "");
            input = input.Replace("<a>", "");
            input = input.Replace("<p>", "");
            input = input.Replace("</td>", "");
            input = input.Replace("</tr>", "");
            input = input.Replace("</b>", "");
            input = input.Replace("</i>", "");
            input = input.Replace("</font size=\"-1\">", "");
            input = input.Replace("</font>", "");
            input = input.Replace("</a>", "");
            input = input.Replace("</p>", "");
            input = input.Replace("<table>", "");
            input = input.Replace("</table>", "");
            input = input.Replace("<sup>", "");
            input = input.Replace("</sup>", "");
            input = input.Replace("<TD>", "");
            input = input.Replace("</TD>", "");
            input = input.Replace("<TR>", "");
            input = input.Replace("</TR>", "");
            input = input.Replace("<FONT size=\"-1\">", "");
            input = input.Replace("</FONT>", "");
            input = input.Replace("<tr valign=\"top\">", "");
            input = input.Replace("<TR valign=\"top\">", "");*/

            return input;
        }

        private List<int> GetWordIndexesWithIndexOf(string text, string word)
        {
            List<int> foundWords = new List<int>();
            int lastIndex = 0;
            wordsCount = 0;

            while (lastIndex != -1)
            {

                lastIndex = text.IndexOf(word, lastIndex);

                if (lastIndex != -1)
                {
                    foundWords.Add(lastIndex);
                    lastIndex += 1;
                    wordsCount++;
                }
                Dispatcher.Invoke(() => UpdateResultText("Znalezionych słów: " + wordsCount.ToString()));
            }
            return foundWords;
        }

        private List<int[]> FindBindNumberStartEndIndexes(string text, List<int> wordIndexes)
        {
            List<int[]> bindNumberStartEndIndexes = new List<int[]>();
            int foundWordIndex;

            for (int i = 0; i < wordIndexes.Count; i++)
            {
                foundWordIndex = wordIndexes[i];
                int[] tmp = {   text.LastIndexOf("<b>Nummer:</b>", foundWordIndex),
                                text.LastIndexOf("<FONT", foundWordIndex),
                                text.LastIndexOf("Brevtekst", foundWordIndex),
                                text.IndexOf("</p>", foundWordIndex) };
                bindNumberStartEndIndexes.Add(tmp);
            }

            return bindNumberStartEndIndexes;
        }

        private List<int[]> RemoveDuplicates(List<int[]> list)
        {
            HashSet<string> set = new HashSet<string>();
            List<int[]> uniqueList = new List<int[]>();

            foreach (int[] array in list)
            {
                // Convert the array to a string representation
                string arrayStr = "[" + string.Join(",", array) + "]";

                // Check if the set does not contain the string representation
                if (!set.Contains(arrayStr))
                {
                    set.Add(arrayStr); // Add it to the set to mark it as seen
                    uniqueList.Add(array); // Add the original array to the unique list
                }
            }

            return uniqueList;
        }

        private void CreateTabsFromText(string text, string word, List<int[]> bindNumberStartEndIndexes, CustomExpander expander)
        {
            int numberIndex, bindIndex, textStartIndex, textEndIndex;
            string numberValue, bindValue, textValue;

            for (int i = 0; i < bindNumberStartEndIndexes.Count; i++)
            {
                numberIndex = bindNumberStartEndIndexes[i][0] + 14;
                bindIndex = bindNumberStartEndIndexes[i][1] + 16;
                textStartIndex = bindNumberStartEndIndexes[i][2];
                textEndIndex = bindNumberStartEndIndexes[i][3];

                numberValue = text.Substring(numberIndex, 7);
                bindValue = text.Substring(bindIndex, 4);
                textValue = text.Substring(textStartIndex, textEndIndex - textStartIndex);

                numberValue = System.Text.RegularExpressions.Regex.Replace(numberValue, "[^0-9]", "");

                textValue = ReplaceLineBreaks(textValue);
                string[] dividedText = DivideText(textValue, word);

                //Dispatcher.Invoke(() => AddNewEntry("Bind: " + bindValue, "Number: " + numberValue, dividedText));
                Dispatcher.Invoke(() => expander.AddNewEntry("Bind: " + bindValue, "Number: " + numberValue, dividedText));
            }
        }

        public void SearchForWord(string word, SearchMethods searchMethod)
        {
            List<int> foundWords = new List<int>();
            List<int[]> bindNumberStartEndIndexes = new List<int[]>();

            string path = "Texts/DIPLOMATARIUM.html"; 
            string textContent = File.ReadAllText(path);


            Dispatcher.Invoke(() => AddNewExpander(word));

            if (searchMethod == SearchMethods.StreamBlocks)
            {
                foundWords = GetWordIndexesWithStreamBlocks(path, word);
            }
            else if (searchMethod == SearchMethods.IndexOf)
            {
                foundWords = GetWordIndexesWithIndexOf(textContent, word);
            }
            Dispatcher.Invoke(() => UpdateResultText("Zapisywanie słów"));

            bindNumberStartEndIndexes = FindBindNumberStartEndIndexes(textContent, foundWords);
            Dispatcher.Invoke(() => UpdateResultText("Generowanie etykiet"));

            List<int[]> filteredBindNumberStartEndIndexes = bindNumberStartEndIndexes.Distinct().ToList(); //RemoveDuplicates(bindNumberStartEndIndexes);

            //CustomExpander expander = new CustomExpander();
            //Dispatcher.Invoke(() => expander = canvasStackPanel.Children.OfType<CustomExpander>().LastOrDefault());

            Dispatcher.InvokeAsync(() =>
            {
                // Find the CustomExpander in your UI hierarchy
                CustomExpander expander = canvasStackPanel.Children.OfType<CustomExpander>().LastOrDefault();

                if (expander != null)
                {
                    // Create and add TextTab to the CustomExpander
                    CreateTabsFromText(textContent, word, filteredBindNumberStartEndIndexes, canvasStackPanel.Children.OfType<CustomExpander>().LastOrDefault());
                }
                else
                {
                    // Handle the case where no CustomExpander was found
                    // You might want to create a new CustomExpander, add it to the canvasStackPanel, and then call AddTextTab
                }
            });

            //CreateTabsFromText(textContent, word, filteredBindNumberStartEndIndexes, canvasStackPanel.Children.OfType<CustomExpander>().LastOrDefault());
            Dispatcher.Invoke(() => UpdateResultText("Znalezione teksty: " + wordsCount.ToString()));

        }

        public void SearchForWords(List<string> words, SearchMethods searchMethod)
        {
            for (int i = 0; i < words.Count; i++) {
                SearchForWord(words[i], searchMethod);
            }

            isSearching = false;
        }
    }
}
