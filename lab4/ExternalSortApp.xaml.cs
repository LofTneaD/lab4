using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ClosedXML.Excel;

namespace lab4;

public partial class ExternalSortApp : Window
{
    private StringBuilder loger;
    private int delay;
    private SolidColorBrush defaultColor = Brushes.Cornsilk;
    private List<Dictionary<string, string>> table;
    private string[] headers;
    
    public ExternalSortApp()
    {
        InitializeComponent();
        loger = new StringBuilder();
        delay = 450;
    }
    
    private void Logs(string message)
    {
        loger.AppendLine(message);
        LogTextBox.Text = loger.ToString();
        LogTextBox.ScrollToEnd();
    }
    
    private void LoadFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            using (var workbook = new XLWorkbook(openFileDialog.FileName))
            {
                var worksheet = workbook.Worksheet(1);
                headers = worksheet.FirstRowUsed()
                    .CellsUsed()
                    .Select(cell => cell.Value.ToString())
                    .ToArray();

                table = worksheet.RowsUsed()
                    .Skip(1)
                    .Select(row => headers.Zip(row.CellsUsed().Select(cell => cell.Value.ToString()),
                            (header, value) => new { header, value })
                        .ToDictionary(x => x.header, x => x.value))
                    .ToList();
            }

            ExcelColumnComboBox.ItemsSource = headers;
            Logs("файл с данными загружен");
        }
    }
    
    private async void StartSorting_Click(object sender, RoutedEventArgs e)
    {
            
        if (table == null || headers == null || ExcelColumnComboBox.SelectedItem == null)
        {
            Logs("Не выбран файл или колонка для сортировки.");
            return;
        }

        StartSorting.IsEnabled = false;
        string sortKey = ExcelColumnComboBox.SelectedItem.ToString();
        Logs($"Сортировка по колонке: {sortKey}");

        if (DirectMergeSortRadioButton.IsChecked == true)
        {
            await DirectMergeSort(table, sortKey);
        }
        else if (NaturalMergeSortRadioButton.IsChecked == true)
        {
            await NaturalMergeSort(table, sortKey);

        }
        else if (MultiwayMergeSortRadioButton.IsChecked == true)
        {
            await MultiWayMergeSort(table, sortKey);
        }

        Logs("Сортировка завершена.");
        StartSorting.IsEnabled = true;
    }
    
    private void DelayChanging(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        delay = (int)e.NewValue;
        DelayLabel.Content = $"Задержка: {delay} мс"; 
    }
    
    private async Task VisualCondition(List<Dictionary<string, string>> file1, List<Dictionary<string, string>> file2, List<Dictionary<string, string>> file3, List<Dictionary<string, string>> file4, string sortKey, Dictionary<string, string> highlighted2 = null, Dictionary<string, string> highlighted3 = null, Dictionary<string, string> highlighted4 = null, Dictionary<string, string> highlighted1 = null, List<Dictionary<string, string>> highlightedSeries2 = null, List<Dictionary<string, string>> highlightedSeries3 = null, List<Dictionary<string, string>> highlightedSeries4 = null)
    {
        SortCanvas.Children.Clear();
        double canvasWidth = SortCanvas.ActualWidth;
        double blockWidth = canvasWidth / 4;

        DrawingBlock(file1, blockWidth, 0, "1", sortKey, highlighted1, Brushes.Turquoise);
        DrawingBlock(file2, blockWidth, blockWidth, "2", sortKey, highlighted2, Brushes.Turquoise, highlightedSeries2, Brushes.MediumSeaGreen);
        DrawingBlock(file3, blockWidth, 2 * blockWidth, "3", sortKey, highlighted3, Brushes.PapayaWhip, highlightedSeries3, Brushes.BurlyWood);
        DrawingBlock(file4, blockWidth, 3 * blockWidth, "4", sortKey, highlighted4, Brushes.Silver, highlightedSeries4, Brushes.CadetBlue);

        await Task.Delay(delay); 
    }

    private async Task VisualCondition(List<Dictionary<string, string>> file1, List<Dictionary<string, string>> file2, List<Dictionary<string, string>> file3, string sortKey, Dictionary<string, string> highlighted2 = null, Dictionary<string, string> highlighted3 = null, Dictionary<string, string> highlighted1 = null, List<Dictionary<string, string>> highlightedSeries2 = null, List<Dictionary<string, string>> highlightedSeries3 = null)
    {
        SortCanvas.Children.Clear();
        double canvasWidth = SortCanvas.ActualWidth;
        double blockWidth = canvasWidth / 3;

        DrawingBlock(file1, blockWidth, 0, "1", sortKey, highlighted1, Brushes.Turquoise);
        DrawingBlock(file2, blockWidth, blockWidth, "2", sortKey, highlighted2, Brushes.Turquoise, highlightedSeries2, Brushes.MediumSeaGreen);
        DrawingBlock(file3, blockWidth, 2 * blockWidth, "3", sortKey, highlighted3, Brushes.PapayaWhip, highlightedSeries3, Brushes.BurlyWood);
        
        await Task.Delay(delay);
    }
    
    private async Task NaturalMergeSort(List<Dictionary<string, string>> table, string sortKey)
    {
        int n = table.Count;
        int step = 1;

        while (true)
        {
            var file2 = new List<Dictionary<string, string>>();
            var file3 = new List<Dictionary<string, string>>();
            bool writeTo2 = true;

            Logs($"\nШаг {step}: Начинаем разбиение исходного массива на естественные серии.");
            int i = 0;
            while (i < n)
            {
                var series = new List<Dictionary<string, string>>();
                series.Add(table[i]);
                Logs($"Создаём новую серию, добавляем элемент {table[i][sortKey]}");
                Dictionary<string, string> highlighted1 = table[i];
                await VisualCondition(table, file2, file3, sortKey, highlighted1: highlighted1);
                
                while (i + 1 < n && CompareValues(table[i][sortKey], table[i + 1][sortKey]) <= 0)
                {
                    i++;
                    series.Add(table[i]);
                    Logs($"Добавление элемента {table[i][sortKey]} в текущую серию.");
                    highlighted1 = table[i];
                    await VisualCondition(table, file2, file3, sortKey, highlighted1: highlighted1);
                }
                i++;
                
                if (writeTo2)
                {
                    file2.AddRange(series);
                    Logs($"Серия {string.Join(", ", series.Select(row => row[sortKey]))} загружена в файл 2.");
                }
                else
                {
                    file3.AddRange(series);
                    Logs($"Серия {string.Join(", ", series.Select(row => row[sortKey]))} загружена в файл 3.");
                }

                writeTo2 = !writeTo2;

                await VisualCondition(table, file2, file3, sortKey);
            }

            Logs($"\nПосле разбиения:\n2: {string.Join(", ", file2.Select(row => row[sortKey]))}\n3: {string.Join(", ", file3.Select(row => row[sortKey]))}");

            if (file3.Count == 0)
            {
                Logs("Файл 3 пуст. Сортировка завершена.");
                return;
            }

            Logs("\nНачинаем слияние файлов 2 и 3 обратно в файл 1.");
            table.Clear();
            int Index2 = 0, Index3 = 0;

            while (Index2 < file2.Count || Index3 < file3.Count)
            { 
                int End2 = Index2;
                int End3 = Index3;

                if (Index2 < file2.Count)
                {
                    End2++;
                    while (End2 < file2.Count && CompareValues(file2[End2 - 1][sortKey], file2[End2][sortKey]) <= 0)
                    {
                        End2++;
                    }
                }

                if (Index3 < file3.Count)
                {
                    End3++;
                    while (End3 < file3.Count && CompareValues(file3[End3 - 1][sortKey], file3[End3][sortKey]) <= 0)
                    {
                        End3++;
                    }
                }

                Logs($"\nПодготавливаем к слиянию серии из файла 2: {string.Join(", ", file2.GetRange(Index2, End2 - Index2).Select(row => row[sortKey]))}");
                Logs($"Подготавливаем к слиянию серии из файла 3: {string.Join(", ", file3.GetRange(Index3, End3 - Index3).Select(row => row[sortKey]))}");
                await VisualCondition(table, file2, file3, sortKey, highlightedSeries2: file2.GetRange(Index2, End2 - Index2), highlightedSeries3: file3.GetRange(Index3, End3 - Index3));
                
                while (Index2 < End2 || Index3 < End3)
                {
                    Dictionary<string, string> highlighted2 = Index2 < End2 ? file2[Index2] : null;
                    Dictionary<string, string> highlighted3 = Index3 < End3 ? file3[Index3] : null;
                    await VisualCondition(table, file2, file3, sortKey, highlighted2, highlighted3);

                    if (Index2 < End2 && (Index3 >= End3 || CompareValues(file2[Index2][sortKey], file3[Index3][sortKey]) <= 0))
                    {
                        Logs($"Сравнение: {file2[Index2][sortKey]} (из 2) < {file3.ElementAtOrDefault(Index3)?[sortKey]} (из 3): извлекаем {file2[Index2][sortKey]} из 2.");
                        table.Add(file2[Index2]);
                        file2.RemoveAt(Index2);
                        End2--;
                        await VisualCondition(table, file2, file3, sortKey);
                    }
                    else if (Index3 < End3)
                    {
                        Logs($"Сравнение: {file3[Index3][sortKey]} (из 3) <= {file2.ElementAtOrDefault(Index2)?[sortKey]} (из 2): извлекаем {file3[Index3][sortKey]} из 3.");
                        table.Add(file3[Index3]);
                        file3.RemoveAt(Index3);
                        End3--;
                        await VisualCondition(table, file2, file3, sortKey);
                    }
                }
            }

            Logs($"\nПосле слияния: {string.Join(", ", table.Select(row => row[sortKey]))}");
            await VisualCondition(table, file2, file3, sortKey);

            step++;
        }
    }

    private async Task MultiWayMergeSort(List<Dictionary<string, string>> table, string sortKey)
    {
        int n = table.Count;
        int seriesLength = 1;
        int step = 1;

        while (seriesLength < n)
        {
            Logs($"Шаг {step}: Длина цепей = {seriesLength}");
            
            var file2 = new List<Dictionary<string, string>>();
            var file3 = new List<Dictionary<string, string>>();
            var file4 = new List<Dictionary<string, string>>();

            int i = 0;
            while (i < table.Count)
            {
                Logs("Начинаем разбиение массива 1 на файлы 2, 3 и 4.");

                for (int j = 0; j < seriesLength && i < table.Count; j++)
                {
                    if (i < table.Count)
                    {
                        Dictionary<string, string> highlighted1 = table[i];
                        await VisualCondition(table, file2, file3, file4, sortKey, highlighted1: highlighted1);
                        file2.Add(table[i]);
                        Logs($"Добавление элемента {table[i][sortKey]} в файл 2.");
                        table.RemoveAt(i);
                    }
                }

                for (int j = 0; j < seriesLength && i < table.Count; j++)
                {
                    if (i < table.Count)
                    {
                        Dictionary<string, string> highlighted1 = table[i];
                        await VisualCondition(table, file2, file3, file4, sortKey, highlighted1: highlighted1);
                        file3.Add(table[i]);
                        Logs($"Добавление элемента {table[i][sortKey]} в файл 3.");
                        table.RemoveAt(i);
                    }
                }

                for (int j = 0; j < seriesLength && i < table.Count; j++)
                {
                    if (i < table.Count)
                    {
                        Dictionary<string, string> highlighted1 = table[i];
                        await VisualCondition(table, file2, file3, file4, sortKey, highlighted1: highlighted1);
                        file4.Add(table[i]);
                        Logs($"Добавление элемента {table[i][sortKey]} в файл 4.");
                        table.RemoveAt(i);
                    }
                }

                await VisualCondition(table, file2, file3, file4, sortKey);
            }

            Logs($"После разбиения:\n2: {string.Join(", ", file2.Select(row => row[sortKey]))}\n3: {string.Join(", ", file3.Select(row => row[sortKey]))}\n4: {string.Join(", ", file4.Select(row => row[sortKey]))}");
            
            Logs("\nНачало слияния файлов 2, 3 и 4 обратно в файл 1.");
            table.Clear();
            int Index2 = 0, Index3 = 0, Index4 = 0;

            while (Index2 < file2.Count || Index3 < file3.Count || Index4 < file4.Count)
            {
                int End2 = Math.Min(Index2 + seriesLength, file2.Count);
                int End3 = Math.Min(Index3 + seriesLength, file3.Count);
                int End4 = Math.Min(Index4 + seriesLength, file4.Count);
                
                await VisualCondition(table, file2, file3, file4, sortKey, highlightedSeries2: file2.GetRange(Index2, End2 - Index2), highlightedSeries3: file3.GetRange(Index3, End3 - Index3), highlightedSeries4: file4.GetRange(Index4, End4 - Index4));
                
                while (Index2 < End2 || Index3 < End3 || Index4 < End4)
                {
                    Dictionary<string, string> highlighted2 = Index2 < End2 ? file2[Index2] : null;
                    Dictionary<string, string> highlighted3 = Index3 < End3 ? file3[Index3] : null;
                    Dictionary<string, string> highlighted4 = Index4 < End4 ? file4[Index4] : null;
                    await VisualCondition(table, file2, file3, file4, sortKey, highlighted2, highlighted3, highlighted4);

                    if (Index2 < End2 && (Index3 >= End3 || CompareValues(file2[Index2][sortKey], file3[Index3][sortKey]) <= 0) && (Index4 >= End4 || CompareValues(file2[Index2][sortKey], file4[Index4][sortKey]) <= 0))
                    {
                        Logs($"Сравнение: {file2[Index2][sortKey]} (из 2) < {file3.ElementAtOrDefault(Index3)?[sortKey]} (из 3) и {file4.ElementAtOrDefault(Index4)?[sortKey]} (из 4): извлекаем {file2[Index2][sortKey]} из 2.");
                        table.Add(file2[Index2]);
                        file2.RemoveAt(Index2);
                        End2--;
                        await VisualCondition(table, file2, file3, file4, sortKey);
                    }
                    else if (Index3 < End3 && (Index4 >= End4 || CompareValues(file3[Index3][sortKey], file4[Index4][sortKey]) <= 0))
                    {
                        Logs($"Сравнение: {file3[Index3][sortKey]} (из 3) <= {file4.ElementAtOrDefault(Index4)?[sortKey]} (из 4): извлекаем {file3[Index3][sortKey]} из 3.");
                        table.Add(file3[Index3]);
                        file3.RemoveAt(Index3);
                        End3--;
                        await VisualCondition(table, file2, file3, file4, sortKey);
                    }
                    else if (Index4 < End4)
                    {
                        Logs($"извлекаем {file4[Index4][sortKey]} из 4.");
                        table.Add(file4[Index4]);
                        file4.RemoveAt(Index4);
                        End4--;
                        await VisualCondition(table, file2, file3, file4, sortKey);
                    }
                }
            }

            Logs($"\nПосле слияния: {string.Join(", ", table.Select(row => row[sortKey]))}");
            await VisualCondition(table, file2, file3, file4, sortKey);
            
            seriesLength *= 3; 
            step++;
        }
    }

    private async Task DirectMergeSort(List<Dictionary<string, string>> table, string sortKey)
    {
        int n = table.Count;
        int seriesLength = 1;
        int step = 1; 

        
        var file2 = new List<Dictionary<string, string>>();
        var file3 = new List<Dictionary<string, string>>();

        while (seriesLength < n)
        {
            Logs($"\nШаг {step}: Длина цепей = {seriesLength}");
            
            file2.Clear();
            file3.Clear();

            int i = 0;
            while (i < table.Count)
            {
                Logs("\nНачало разбиения массива 1 на файлы 2 и 3.");
                for (int j = 0; j < seriesLength && i < table.Count; j++)
                {
                    Logs($"Подготовка элемента {table[i][sortKey]} в файл 2.");
                    Dictionary<string, string> highlightedA = table[i];
                    await VisualCondition(table, file2, file3, sortKey, highlighted1: highlightedA);

                    file2.Add(table[i]);
                    Logs($"Добавление элемента {table[i][sortKey]} в файл 2 и удаление из файла 1.");
                    table.RemoveAt(i);
                }

                for (int j = 0; j < seriesLength && i < table.Count; j++)
                {
                    Logs($"Подготовка к перемещению {table[i][sortKey]} в файл 3.");
                    Dictionary<string, string> highlightedA = table[i];
                    await VisualCondition(table, file2, file3, sortKey, highlighted1: highlightedA);

                    file3.Add(table[i]);
                    Logs($"Добавление элемента {table[i][sortKey]} в файл 3 и удаление из файла 1.");
                    table.RemoveAt(i);
                }

                Logs("\nТекущее состояние:");
                await VisualCondition(table, file2, file3, sortKey);
            }

            Logs($"\nПосле разбиения:\n2: {string.Join(", ", file2.Select(row => row[sortKey]))}\n3: {string.Join(", ", file3.Select(row => row[sortKey]))}");
            Logs("\nНачинаем слияние файлов 2 и 3 обратно в файл 1.");
            table.Clear();
            int bIndex = 0, cIndex = 0;

            while (bIndex < file2.Count || cIndex < file3.Count)
            {
                int bEnd = Math.Min(bIndex + seriesLength, file2.Count);
                int cEnd = Math.Min(cIndex + seriesLength, file3.Count);

                Logs($"\nПодготавливаем к слиянию серии из файла 2: {string.Join(", ", file2.GetRange(bIndex, bEnd - bIndex).Select(row => row[sortKey]))}");
                Logs($"Подготавливаем к слиянию серии из файла 3: {string.Join(", ", file3.GetRange(cIndex, cEnd - cIndex).Select(row => row[sortKey]))}");
                await VisualCondition(table, file2, file3, sortKey, highlightedSeries2: file2.GetRange(bIndex, bEnd - bIndex), highlightedSeries3: file3.GetRange(cIndex, cEnd - cIndex));

                while (bIndex < bEnd || cIndex < cEnd)
                {
                    Dictionary<string, string> highlightedB = bIndex < bEnd ? file2[bIndex] : null;
                    Dictionary<string, string> highlightedC = cIndex < cEnd ? file3[cIndex] : null;
                    await VisualCondition(table, file2, file3, sortKey, highlightedB, highlightedC);

                    if (bIndex < bEnd && (cIndex >= cEnd || CompareValues(file2[bIndex][sortKey], file3[cIndex][sortKey]) <= 0))
                    {
                        Logs($"Сравнение: {file2[bIndex][sortKey]} (из 2) < {file3.ElementAtOrDefault(cIndex)?[sortKey]} (из 3): извлекаем {file2[bIndex][sortKey]} из 2.");
                        table.Add(file2[bIndex]);
                        file2.RemoveAt(bIndex);
                        bEnd--;
                        await VisualCondition(table, file2, file3, sortKey);
                    }
                    else if (cIndex < cEnd)
                    {
                        Logs($"Сравнение: {file3[cIndex][sortKey]} (из 3) <= {file2.ElementAtOrDefault(bIndex)?[sortKey]} (из 2): извлекаем {file3[cIndex][sortKey]} из 3.");
                        table.Add(file3[cIndex]);
                        file3.RemoveAt(cIndex);
                        cEnd--;
                        await VisualCondition(table, file2, file3, sortKey);
                    }
                }
            }

            Logs($"\nПосле слияния: {string.Join(", ", table.Select(row => row[sortKey]))}");
            await VisualCondition(table, file2, file3, sortKey);
            
            seriesLength *= 2;
            step++;
        }
    }

    private void DrawingBlock(List<Dictionary<string, string>> block, double blockWidth, double offsetX, string label, string sortKey, Dictionary<string, string> highlighted = null, Brush highlightColor = null, List<Dictionary<string, string>> highlightedSeries = null, Brush seriesHighlightColor = null)
    {
        double rectangleHeight = block.Count * 20;

        var rectangle = new Rectangle
        {
            Width = blockWidth - 10,
            Height = rectangleHeight,
            Fill = defaultColor,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        
        var blockLabel = new TextBlock
        {
            Text = label,
            Foreground = Brushes.Black,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center
        };

        Canvas.SetLeft(rectangle, offsetX);
        Canvas.SetTop(rectangle, SortCanvas.ActualHeight - rectangleHeight - 20);
        SortCanvas.Children.Add(rectangle);
        Canvas.SetLeft(blockLabel, offsetX + blockWidth / 2 - 30);
        Canvas.SetTop(blockLabel, SortCanvas.ActualHeight - rectangleHeight - 40);
        SortCanvas.Children.Add(blockLabel);

        for (int j = 0; j < block.Count; j++)
        {
            if (block[j] == null)
            {
                continue;
            }

            string key = block[j][headers[0]];
            string value = block[j][sortKey]; 

            var labelItem = new TextBlock
            {
                Text = $"{key} ({value})",
                Foreground = Brushes.Gray,
                Background = (block[j] == highlighted && highlightColor != null) ? highlightColor :
                             (highlightedSeries != null && highlightedSeries.Contains(block[j]) && seriesHighlightColor != null) ? seriesHighlightColor :
                             Brushes.Azure,
                TextAlignment = TextAlignment.Center,
                Width = blockWidth - 10,
                Height = 20
            };

            Canvas.SetLeft(labelItem, offsetX);
            Canvas.SetTop(labelItem, SortCanvas.ActualHeight - rectangleHeight + j * 20 - 20);
            SortCanvas.Children.Add(labelItem);
        }
    }
    private int CompareValues(string value1, string value2)
    {
        if (double.TryParse(value1, out var number) && double.TryParse(value2, out var num2))
        {
            return number.CompareTo(num2);
        }
        return string.Compare(value1, value2, StringComparison.Ordinal);
    }
}