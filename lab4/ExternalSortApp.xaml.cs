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
    
    private async Task DisplaySortingStepsAsync(List<List<Dictionary<string, string>>> blocks, int delay, string attribute = null, List<List<Dictionary<string, string>>> previousBlocks = null)
    {
        SortingStepsListBox.Items.Add(new RecordDisplay { RecordString = "========= Новый шаг ========" });

        // Добавление информации о блоках
        for (int i = 0; i < blocks.Count; i++)
        {
            SortingStepsListBox.Items.Add(new RecordDisplay { RecordString = $"Блок {i + 1}:" });

            for (int j = 0; j < blocks[i].Count; j++)
            {
                var record = blocks[i][j];
                string recordString = string.Join(", ", record.Values);

                string color = record.ContainsKey("highlighted") ? record["highlighted"] : null;

                SortingStepsListBox.Items.Add(new RecordDisplay
                {
                    RecordString = recordString,
                    Highlighted = color
                });
            }
        }

        // Показать промежуточные изменения блоков
        if (previousBlocks != null)
        {
            for (int i = 0; i < previousBlocks.Count; i++)
            {
                var block = previousBlocks[i];
                SortingStepsListBox.Items.Add(new RecordDisplay { RecordString = $"После слияния блок {i + 1}:" });

                foreach (var record in block)
                {
                    string recordString = string.Join(", ", record.Values);
                    SortingStepsListBox.Items.Add(new RecordDisplay { RecordString = recordString });
                }
            }
        }

        SortingStepsListBox.ScrollIntoView(SortingStepsListBox.Items[^1]);
        await Task.Delay(delay);
    }

    
    private async void SortButton_Click(object sender, RoutedEventArgs e)
    {
        SortingStepsListBox.Items.Clear();
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
                tableData = await MergeSortAsync(tableData, selectedAttribute, ascending, delay);
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
        List<List<Dictionary<string, string>>> blocks = new List<List<Dictionary<string, string>>>();
        List<Dictionary<string, string>> currentBlock = new List<Dictionary<string, string>>();

        // Разбиение данных на блоки
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
                    blocks.Add(new List<Dictionary<string, string>>(currentBlock));
                    currentBlock.Clear(); // Новый блок
                }
            }
            currentBlock.Add(data[i]);
        }

        // Добавляем последний блок
        if (currentBlock.Count > 0)
        {
            blocks.Add(new List<Dictionary<string, string>>(currentBlock));
        }

        // Отображаем начальные блоки
        await DisplaySortingStepsAsync(blocks, delay, attribute);

        // Слияние блоков
        while (blocks.Count > 1)
        {
            // Слияние блоков по одному элементу
            blocks = new List<List<Dictionary<string, string>>>
            {
                new List<Dictionary<string, string>>(await MultiWayMergeAsync(blocks, attribute, isNumeric, ascending, delay))
            };

            // Показать состояние после каждого слияния
            await DisplaySortingStepsAsync(blocks, delay, attribute);
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
    
    private async Task<List<Dictionary<string, string>>> MergeSortAsync(List<Dictionary<string, string>> data, string attribute, bool ascending, int delay)
    {
        bool isNumeric = long.TryParse(data.First()[attribute], out _);

        // Разделение данных на два блока
        var evenBlock = data.Where((_, index) => index % 2 == 0).ToList();
        var oddBlock = data.Where((_, index) => index % 2 != 0).ToList();

        // Показать начальное состояние двух блоков
        await DisplaySortingStepsAsync(new List<List<Dictionary<string, string>>>
        {
            evenBlock,
            oddBlock
        }, delay);

        // Алгоритм слияния двух блоков
        var merged = new List<Dictionary<string, string>>();
        int evenIndex = 0, oddIndex = 0;

        while (evenIndex < evenBlock.Count && oddIndex < oddBlock.Count)
        {
            // Сравнение текущих элементов
            bool isEvenSmaller = isNumeric
                ? ascending
                    ? Convert.ToInt64(evenBlock[evenIndex][attribute]) <= Convert.ToInt64(oddBlock[oddIndex][attribute])
                    : Convert.ToInt64(evenBlock[evenIndex][attribute]) >= Convert.ToInt64(oddBlock[oddIndex][attribute])
                : ascending
                    ? string.Compare(evenBlock[evenIndex][attribute], oddBlock[oddIndex][attribute]) <= 0
                    : string.Compare(evenBlock[evenIndex][attribute], oddBlock[oddIndex][attribute]) >= 0;

            // Добавляем текстовый комментарий в интерфейс
            string comparisonText = $"Сравниваем {evenBlock[evenIndex][attribute]} (четный блок) и {oddBlock[oddIndex][attribute]} (нечетный блок).";
            SortingStepsListBox.Items.Add(new RecordDisplay { RecordString = comparisonText });

            if (isEvenSmaller)
            {
                merged.Add(evenBlock[evenIndex]);
                evenIndex++;
            }
            else
            {
                merged.Add(oddBlock[oddIndex]);
                oddIndex++;
            }

            // Показать состояние после каждого слияния
            await DisplaySortingStepsAsync(new List<List<Dictionary<string, string>>>
            {
                evenBlock.Skip(evenIndex).ToList(),
                oddBlock.Skip(oddIndex).ToList(),
                merged // Промежуточный результат
            }, delay);

            // Добавление комментария после отображения третьего блока
            SortingStepsListBox.Items.Add(new RecordDisplay
            {
                RecordString = $"Текущий результат: {string.Join(", ", merged.Select(item => item[attribute]))}"
            });
        }

        // Добавить оставшиеся элементы
        while (evenIndex < evenBlock.Count)
        {
            merged.Add(evenBlock[evenIndex]);
            SortingStepsListBox.Items.Add(new RecordDisplay
            {
                RecordString = $"Добавляем {evenBlock[evenIndex][attribute]} из четного блока в результат."
            });
            evenIndex++;
        }

        while (oddIndex < oddBlock.Count)
        {
            merged.Add(oddBlock[oddIndex]);
            SortingStepsListBox.Items.Add(new RecordDisplay
            {
                RecordString = $"Добавляем {oddBlock[oddIndex][attribute]} из нечетного блока в результат."
            });
            oddIndex++;
        }

        // Показать финальное состояние
        await DisplaySortingStepsAsync(new List<List<Dictionary<string, string>>>
        {
            new List<Dictionary<string, string>>(), // Четный блок пуст
            new List<Dictionary<string, string>>(), // Нечетный блок пуст
            merged // Финальный результат
        }, delay);

        // Итоговое сообщение
        SortingStepsListBox.Items.Add(new RecordDisplay
        {
            RecordString = $"Слияние завершено. Итоговый результат: {string.Join(", ", merged.Select(item => item[attribute]))}"
        });

        return merged;
    }
    
    public class RecordDisplay
    {
        public string RecordString { get; set; }  // Отображаемая строка
        public string Highlighted { get; set; }  // Цвет выделения ("yellow", "green" или null)
    }
    
    private async Task<List<Dictionary<string, string>>> MultiWayMergeAsync(
    List<List<Dictionary<string, string>>> blocks,
    string attribute,
    bool isNumeric,
    bool ascending,
    int delay)
{
    var merged = new List<Dictionary<string, string>>();
    var indices = new int[blocks.Count];

    while (true)
    {
        var currentElements = new List<(int blockIndex, Dictionary<string, string> record)>();

        // Добавляем текущие элементы из каждого блока
        for (int i = 0; i < blocks.Count; i++)
        {
            if (indices[i] < blocks[i].Count)
            {
                currentElements.Add((i, blocks[i][indices[i]]));
            }
        }

        if (currentElements.Count == 0)
        {
            break; // Если блоки пустые, завершение
        }

        // Сортируем по значению
        var elementToAdd = currentElements
            .OrderBy(e => isNumeric
                ? ascending
                    ? Convert.ToInt64(e.record[attribute])
                    : -Convert.ToInt64(e.record[attribute])
                : ascending
                    ? string.Compare(e.record[attribute], currentElements[0].record[attribute], StringComparison.Ordinal)
                    : -string.Compare(e.record[attribute], currentElements[0].record[attribute], StringComparison.Ordinal))
            .First(); // Берем минимальный элемент

        // Добавляем в результирующий список
        merged.Add(elementToAdd.record);

        // Удаляем добавленный элемент из блока, из которого он был взят
        blocks[elementToAdd.blockIndex].RemoveAt(indices[elementToAdd.blockIndex]);

        // Сдвигаем индекс для блока, из которого был выбран элемент
        indices[elementToAdd.blockIndex]++;

        // Показать состояние после каждого слияния
        await DisplaySortingStepsAsync(blocks, delay, attribute, new List<List<Dictionary<string, string>>> { merged });

        // Отображаем текущий шаг слияния
        string comparisonText = "Сравниваем: ";
        foreach (var elem in currentElements)
        {
            comparisonText += $"{elem.record[attribute]} (из блока {elem.blockIndex + 1}) ";
        }
        SortingStepsListBox.Items.Add(new RecordDisplay { RecordString = comparisonText });

        // Выводим текущий результат
        SortingStepsListBox.Items.Add(new RecordDisplay
        {
            RecordString = $"Текущий блок после слияния: {string.Join(", ", merged.Select(item => item[attribute]))}"
        });

        // Ждем задержку между шагами
        await Task.Delay(delay);
    }

    return merged; // Возвращаем итоговый отсортированный блок
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