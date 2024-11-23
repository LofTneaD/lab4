using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;

namespace lab4;

public partial class TextSorting : Window
{
    public TextSorting()
    {
        InitializeComponent();
    }
    // Merge Sort
        private List<string> MergeSort(List<string> words)
        {
            if (words.Count <= 1) return words;

            int mid = words.Count / 2;
            var left = MergeSort(words.GetRange(0, mid));
            var right = MergeSort(words.GetRange(mid, words.Count - mid));

            return Merge(left, right);
        }

        private List<string> Merge(List<string> left, List<string> right)
        {
            List<string> result = new();
            int i = 0, j = 0;

            while (i < left.Count && j < right.Count)
            {
                if (string.Compare(left[i], right[j], StringComparison.Ordinal) <= 0)
                {
                    result.Add(left[i]);
                    i++;
                }
                else
                {
                    result.Add(right[j]);
                    j++;
                }
            }

            result.AddRange(left.Skip(i));
            result.AddRange(right.Skip(j));
            return result;
        }

        // Radix Sort
        private List<string> RadixSort(List<string> words)
        {
            int maxLength = words.Max(w => w.Length);
            for (int i = maxLength - 1; i >= 0; i--)
            {
                var buckets = new List<string>[256];
                for (int j = 0; j < 256; j++) buckets[j] = new List<string>();

                foreach (var word in words)
                {
                    int charIndex = i < word.Length ? word[i] : 0;
                    buckets[charIndex].Add(word);
                }

                words = buckets.SelectMany(b => b).ToList();
            }

            return words;
        }

        // Count word frequencies
        private Dictionary<string, int> CountWords(List<string> sortedWords)
        {
            Dictionary<string, int> wordCounts = new();
            foreach (var word in sortedWords)
            {
                if (wordCounts.ContainsKey(word))
                    wordCounts[word]++;
                else
                    wordCounts[word] = 1;
            }
            return wordCounts;
        }

        // Event handlers for sorting
        private void SortMerge_Click(object sender, RoutedEventArgs e)
        {
            ProcessTextAndSort(MergeSort);
        }

        private void SortRadix_Click(object sender, RoutedEventArgs e)
        {
            ProcessTextAndSort(RadixSort);
        }

        // Process input, sort, and display results
        private void ProcessTextAndSort(Func<List<string>, List<string>> sortAlgorithm)
        {
            string inputText = InputTextBox.Text;
            if (string.IsNullOrWhiteSpace(inputText))
            {
                MessageBox.Show("Please enter some text.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Split text into words
            var words = Regex.Split(inputText, @"\W+").Where(w => !string.IsNullOrEmpty(w)).ToList();

            // Measure sorting time
            Stopwatch stopwatch = Stopwatch.StartNew();
            var sortedWords = sortAlgorithm(words);
            stopwatch.Stop();

            // Count word frequencies
            var wordCounts = CountWords(sortedWords);

            // Display results
            OutputTextBox.Text = string.Join("\n", wordCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            TimingResultsGrid.ItemsSource = new[]
            {
                new { Algorithm = sortAlgorithm.Method.Name, Time = stopwatch.ElapsedMilliseconds + " ms" }
            };
        }
}