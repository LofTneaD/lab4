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
        // Найти максимальную длину слова
        int maxLength = words.Max(w => w.Length);

        // Итерируем по разрядам, начиная с последнего символа
        for (int i = maxLength - 1; i >= 0; i--)
        {
            // Создаем корзины (одна для каждого ASCII символа + 1 для "пустых" символов)
            var buckets = new List<string>[257];
            for (int j = 0; j < 257; j++)
            {
                buckets[j] = new List<string>();
            }

            // Распределяем слова по корзинам
            foreach (var word in words)
            {
                // Если индекс выходит за пределы слова, символ считается "нулевым"
                int charIndex = i < word.Length ? word[i] + 1 : 0;

                // Добавляем слово в соответствующую корзину
                buckets[charIndex].Add(word);
            }

            // Объединяем все корзины обратно в список
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

    private List<string> CleanText(string text)
    {
        // Удаляем все лишние символы (оставляем только буквы и цифры)
        var words = Regex.Matches(text.ToLower(), @"\b\w+\b")
            .Cast<Match>()
            .Select(m => m.Value)
            .ToList();
        return words;
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

        // Очищаем текст
        var words = CleanText(inputText);

        if (words.Count == 0)
        {
            MessageBox.Show("No valid words found after cleaning the text.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Замер времени сортировки
        Stopwatch stopwatch = Stopwatch.StartNew();
        var sortedWords = sortAlgorithm(words);
        stopwatch.Stop();

        double elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
        Console.WriteLine($"Radix Sort completed in {elapsedMilliseconds:F3} ms.");


        // Подсчитываем частоту слов
        var wordCounts = CountWords(sortedWords);

        // Выводим результаты
        OutputTextBox.Text = string.Join("\n", wordCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        TimingResultsGrid.ItemsSource = new[]
        {
            new { Algorithm = sortAlgorithm.Method.Name, Time = $"{stopwatch.ElapsedMilliseconds} ms" }
        };
    }
}