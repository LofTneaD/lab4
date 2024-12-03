using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace lab4;

public partial class ExternalSortApp : Window
{
    private List<Dictionary<string, string>> tableData = new List<Dictionary<string, string>>();
    private string inputFilePath = "";
    private string outputFilePath = "";
    
    public ExternalSortApp()
    {
        InitializeComponent();
    }

    private void SelectFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            inputFilePath = openFileDialog.FileName;
            FilePathTextBox.Text = inputFilePath;
            LoadDataFromFile();
        }
    }

    private void LoadDataFromFile()
    {
        if (!File.Exists(inputFilePath)) return;

        tableData.Clear();
        var lines = File.ReadAllLines(inputFilePath);
        if (lines.Length == 0) return;

        var headers = lines[0].Split(',');
        foreach (var line in lines.Skip(1))
        {
            var values = line.Split(',');
            var record = new Dictionary<string, string>();
            for (int i = 0; i < headers.Length; i++)
            {
                record[headers[i]] = values[i];
            }
            tableData.Add(record);
        }

        // Обновляем ComboBox с атрибутами (столбцами)
        SortAttributeComboBox.ItemsSource = headers;
        SortAttributeComboBox.SelectedIndex = 0;
    }
    
    private async Task DisplaySortingStepsAsync(
        List<List<Dictionary<string, string>>> blocks,
        int delay,
        string attribute = null,
        List<List<Dictionary<string, string>>> previousBlocks = null)
    {
        SortingStepsListBox.Items.Add("========= Новый шаг ========");

        for (int i = 0; i < blocks.Count; i++)
        {
            SortingStepsListBox.Items.Add($"Блок {i + 1}:");

            for (int j = 0; j < blocks[i].Count; j++)
            {
                string recordString = string.Join(", ", blocks[i][j].Values);

                if (previousBlocks != null && i < previousBlocks.Count && j < previousBlocks[i].Count &&
                    !blocks[i][j][attribute].Equals(previousBlocks[i][j][attribute]))
                {
                    // Добавляем цветовую метку, например, через добавление текста в [].
                    recordString = $"[Изменено] {recordString}";
                }

                SortingStepsListBox.Items.Add(recordString);
            }
        }

        SortingStepsListBox.ScrollIntoView(SortingStepsListBox.Items[^1]);
        await Task.Delay(delay);
    }




    private async void SortButton_Click(object sender, RoutedEventArgs e)
    {
        string selectedAttribute = SortAttributeComboBox.SelectedItem.ToString();
        string sortDirection = ((ComboBoxItem)SortDirectionComboBox.SelectedItem).Content.ToString();
        bool ascending = sortDirection == "По возрастанию";

        if (!int.TryParse(DelayTextBox.Text, out int delay))
        {
            MessageBox.Show("Введите корректное значение задержки.");
            return;
        }

        string selectedMethod = ((ComboBoxItem)SortMethodComboBox.SelectedItem).Tag.ToString();

        switch (selectedMethod)
        {
            case "NaturalMerge":
                await NaturalMergeSortAsync(selectedAttribute, ascending, delay);
                break;
            case "DirectSort":
                tableData = await DirectSortAsync(tableData, selectedAttribute, ascending, delay);
                MessageBox.Show("Прямая сортировка завершена!");
                break;
            case "MultiWayMerge":
                await MultiWayMergeSortAsync(selectedAttribute, ascending, delay);
                MessageBox.Show("Многопутевое слияние завершено!");
                break;

            default:
                MessageBox.Show("Выберите метод сортировки.");
                break;
        }
    }

    
    private async Task<List<Dictionary<string, string>>> NaturalMergeSortImplementationAsync(
        List<Dictionary<string, string>> data,
        string attribute,
        bool ascending,
        int delay)
    {
        bool isNumeric = long.TryParse(data.First()[attribute], out _);
        List<List<Dictionary<string, string>>> blocks = new();
        List<Dictionary<string, string>> currentBlock = new();

        for (int i = 0; i < data.Count; i++)
        {
            if (i > 0)
            {
                bool isCurrentLess = isNumeric
                    ? ascending
                        ? Convert.ToInt64(data[i - 1][attribute]) > Convert.ToInt64(data[i][attribute])
                        : Convert.ToInt64(data[i - 1][attribute]) < Convert.ToInt64(data[i][attribute])
                    : ascending
                        ? string.Compare(data[i - 1][attribute], data[i][attribute]) > 0
                        : string.Compare(data[i - 1][attribute], data[i][attribute]) < 0;

                if (isCurrentLess)
                {
                    blocks.Add(currentBlock);
                    currentBlock = new();
                }
            }
            currentBlock.Add(data[i]);
        }

        if (currentBlock.Count > 0)
            blocks.Add(currentBlock);

        List<List<Dictionary<string, string>>> previousBlocks = null;

        // Показ начальных блоков
        await DisplaySortingStepsAsync(blocks, delay, attribute);

        // Выполняем слияние
        while (blocks.Count > 1)
        {
            previousBlocks = new List<List<Dictionary<string, string>>>(blocks.Select(block => new List<Dictionary<string, string>>(block)));
            blocks = MultiWayMerge(blocks, attribute, isNumeric, ascending);
            await DisplaySortingStepsAsync(blocks, delay, attribute, previousBlocks);
        }

        return blocks.First();
    }



    private async Task NaturalMergeSortAsync(string attribute, bool ascending, int delay)
    {
        tableData = await NaturalMergeSortImplementationAsync(tableData, attribute, ascending, delay);
        MessageBox.Show("Естественное слияние завершено!");
    }
    
    private async Task MultiWayMergeSortAsync(string attribute, bool ascending, int delay)
    {
        tableData = await NaturalMergeSortImplementationAsync(tableData, attribute, ascending, delay);
        MessageBox.Show("Многопутевое слияние завершено!");
    }
    
    private async Task<List<Dictionary<string, string>>> DirectSortAsync(List<Dictionary<string, string>> data, string attribute, bool ascending, int delay)
    {
        bool isNumeric = long.TryParse(data.First()[attribute], out _);

        for (int i = 1; i < data.Count; i++)
        {
            var current = data[i];
            int j = i - 1;

            while (j >= 0)
            {
                bool isSwapNeeded = isNumeric
                    ? ascending
                        ? Convert.ToInt64(data[j][attribute]) > Convert.ToInt64(current[attribute])
                        : Convert.ToInt64(data[j][attribute]) < Convert.ToInt64(current[attribute])
                    : ascending
                        ? string.Compare(data[j][attribute], current[attribute]) > 0
                        : string.Compare(data[j][attribute], current[attribute]) < 0;

                if (!isSwapNeeded) break;

                data[j + 1] = data[j];
                j--;
            }
            data[j + 1] = current;

            // Обновляем отображение после каждого шага
            await DisplaySortingStepsAsync(new List<List<Dictionary<string, string>>> { data }, delay);
        }

        return data;
    }
    

    private List<List<Dictionary<string, string>>> MultiWayMerge(
        List<List<Dictionary<string, string>>> blocks,
        string attribute,
        bool isNumeric,
        bool ascending)
    {
        var priorityQueue = new SortedDictionary<
            (string, int), Dictionary<string, string>>(new ComparerForMerge(isNumeric, ascending));

        // Индексы текущих элементов для каждого блока
        var indices = new int[blocks.Count];

        // Инициализация очереди
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].Count > 0)
            {
                var key = (blocks[i][0][attribute], i);
                priorityQueue.Add(key, blocks[i][0]);
            }
        }

        var merged = new List<Dictionary<string, string>>();

        while (priorityQueue.Count > 0)
        {
            // Извлечение минимального элемента
            var firstKey = priorityQueue.First().Key;
            var currentRecord = priorityQueue.First().Value;
            priorityQueue.Remove(firstKey);

            merged.Add(currentRecord);

            int blockIndex = firstKey.Item2;
            indices[blockIndex]++;

            // Если в блоке остались элементы, добавляем следующий элемент в очередь
            if (indices[blockIndex] < blocks[blockIndex].Count)
            {
                var nextKey = (blocks[blockIndex][indices[blockIndex]][attribute], blockIndex);
                priorityQueue.Add(nextKey, blocks[blockIndex][indices[blockIndex]]);
            }
        }

        return new List<List<Dictionary<string, string>>> { merged };
    }
    
    private class ComparerForMerge : IComparer<(string, int)>
    {
        private readonly bool isNumeric;
        private readonly bool ascending;

        public ComparerForMerge(bool isNumeric, bool ascending)
        {
            this.isNumeric = isNumeric;
            this.ascending = ascending;
        }

        public int Compare((string, int) x, (string, int) y)
        {
            int comparison;
            if (isNumeric)
            {
                comparison = Convert.ToInt64(x.Item1).CompareTo(Convert.ToInt64(y.Item1));
            }
            else
            {
                comparison = string.Compare(x.Item1, y.Item1, StringComparison.Ordinal);
            }

            return ascending ? comparison : -comparison;
        }
    }

    

    private void SaveResultButton_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            outputFilePath = saveFileDialog.FileName;
            OutputFileTextBox.Text = outputFilePath;
            SaveDataToFile();
        }
    }

    private void SaveDataToFile()
    {
        var headers = tableData.First().Keys.ToArray();
        var lines = new List<string> { string.Join(",", headers) };

        foreach (var record in tableData)
        {
            lines.Add(string.Join(",", record.Values));
        }

        File.WriteAllLines(outputFilePath, lines);
        MessageBox.Show("Результаты сохранены в файл!");
    }
}
