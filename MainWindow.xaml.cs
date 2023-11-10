using CorpusSearchEngine.CustomElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CorpusSearchEngine
{
    public partial class MainWindow : Window
    {
        int cnt = 1;
        string word = string.Empty;
        bool isSearching = false;
        int wordsCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        async void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSearching)
            {
                isSearching = true;

                word = searchTextbox.Text;
                canvasStackPanel.Children.Clear();

                await Task.Run(() => SearchForWord(word));
            }
        }

        async void searchTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!isSearching)
                {
                    isSearching = true;

                    word = searchTextbox.Text;
                    canvasStackPanel.Children.Clear();

                    await Task.Run(() => SearchForWord(word));
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

        private void UpdateResultText(string text)
        {
            resultText.Text = text;
        }

        private void AddNewEntry(string bind, string number, string content)
        {
            canvasStackPanel.Children.Add(new TextTab { bind = bind, number = number, content = content});
        }

        private void AddNewEntry(string bind, string number, string[] content)
        {
            canvasStackPanel.Children.Add(new TextTab { bind = bind, number = number, highlightedContent = content});
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

        private ArrayList FindWordIndexesInText(string text, string word)
        {
            ArrayList foundWords = new ArrayList();
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

        private ArrayList FindBindNumberStartEndIndexes(string text, ArrayList wordIndexes)
        {
            ArrayList bindNumberStartEndIndexes = new ArrayList();

            for (int i = 0; i < wordIndexes.Count; i++)
            {
                int foundWordIndex = (int)wordIndexes[i];
                int[] tmp = {   text.LastIndexOf("<b>Nummer:</b>", foundWordIndex),
                                text.LastIndexOf("<FONT", foundWordIndex),
                                text.LastIndexOf("Brevtekst", foundWordIndex),
                                text.IndexOf("</p>", foundWordIndex) };
                bindNumberStartEndIndexes.Add(tmp);
            }

            return bindNumberStartEndIndexes;
        }

        private List<int[]> RemoveDuplicates(ArrayList list)
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

        private void CreateTabsFromText(string text, List<int[]> bindNumberStartEndIndexes)
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

                Dispatcher.Invoke(() => AddNewEntry("Bind: " + bindValue, "Number: " + numberValue, dividedText));
            }
        }

        public void SearchForWord(string word)
        {
            ArrayList foundWords = new ArrayList();
            ArrayList bindNumberStartEndIndexes = new ArrayList();

            string path = "Texts/DIPLOMATARIUM.html"; 
            string textContent = File.ReadAllText(path);

            foundWords = FindWordIndexesInText(textContent, word);
            Dispatcher.Invoke(() => UpdateResultText("Zapisywanie słów"));

            bindNumberStartEndIndexes = FindBindNumberStartEndIndexes(textContent, foundWords);
            Dispatcher.Invoke(() => UpdateResultText("Generowanie etykiet"));

            List<int[]> filteredBindNumberStartEndIndexes = RemoveDuplicates(bindNumberStartEndIndexes);
            CreateTabsFromText(textContent, filteredBindNumberStartEndIndexes);
            Dispatcher.Invoke(() => UpdateResultText("Znalezione teksty: " + wordsCount.ToString()));

            isSearching = false;
        }
    }
}
