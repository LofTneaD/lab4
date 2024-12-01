using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace lab4
{
    /// <summary>
    /// Логика взаимодействия для TimSortWindow.xaml
    /// </summary>
    public partial class TimSortWindow : Window
    {
        public TimSortWindow()
        {
            InitializeComponent();
        }

        int[] ourArray;
        private int delayMilliseconds = 100; // Значение задержки по умолчанию

        private void GenerateMassiveButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateMassiveInputPanel.Visibility = Visibility.Visible;
            GenerateMassiveInputBox.Focus();
        }

        private void ConfirmGenerateMassiveButton_Click(object sender, RoutedEventArgs e)
        {
            string input = GenerateMassiveInputBox.Text.Trim();

            if (!string.IsNullOrEmpty(input))
            {
                AddColoredText("Сгенерирован массив с длиной: " + input + "\n", Colors.DarkGreen);
                ourArray = MakeMassive(Int32.Parse(input));
                DisplayBars(); // Отображаем начальный массив
            }
            GenerateMassiveInputBox.Clear();
            GenerateMassiveInputPanel.Visibility = Visibility.Collapsed;
        }

        private async void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (ourArray != null)
            {
                AddColoredText("Начало сортировки" + "\n", Colors.DarkGreen);
                await Task.Delay(delayMilliseconds);
                AddColoredText("Задаём минимальный размер подмассивов (RUN), которые будут сортироваться с помощью сортировки вставками." + "\n", Colors.Orange);
                await Task.Delay(delayMilliseconds);
                await TimSort(ourArray, ourArray.Length);  // Обновлено: вызываем метод с асинхронной задержкой
            }
            else
                AddColoredText("Массив не сгенерирован" + "\n", Colors.Red);
        }

        private void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            delayMilliseconds = (int)DelaySlider.Value;
            DelayLabel.Text = $"Задержка: {delayMilliseconds} мс";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Document.Blocks.Clear();
        }

        private void PrintMassiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ourArray != null)
            {
                AddColoredText("Элементы массива:" + "\n", Colors.Black);
                foreach (int i in ourArray)
                {
                    AddColoredText(i + "\n", Colors.Black);
                }
            }
            else
                AddColoredText("Массив не сгенерирован" + "\n", Colors.Red);
        }

        public static int[] MakeMassive(int n)
        {
            Random random = new Random();
            int[] array = new int[n];

            for (int j = 0; j < array.Length; j++)
            {
                array[j] = random.Next();
            }
            return array;
        }

        // Метод добавления текста с указанным цветом
        private void AddColoredText(string text, Color color)
        {
            Run coloredRun = new Run(text)
            {
                Foreground = new SolidColorBrush(color)
            };

            if (LogBox.Document.Blocks.FirstBlock is Paragraph paragraph)
            {
                paragraph.Inlines.Add(coloredRun);
            }
            else
            {
                paragraph = new Paragraph(coloredRun)
                {
                    Margin = new Thickness(0)
                };
                LogBox.Document.Blocks.Add(paragraph);
            }
        }

        const int RUN = 32; // Минимальный размер руна

        public async Task TimSort(int[] array, int n)
        {
            AddColoredText("1. Разбиваем массив на подмассивы длиной RUN для сортировки с помощью сортировки вставками." + "\n", Colors.Orange);

            // Сортируем все руны (подмассивы)
            for (int i = 0; i < n; i += RUN)
            {
                AddColoredText($"Сортируем подмассив с индекса {i} до {Math.Min(i + RUN - 1, n - 1)} с помощью сортировки вставками." + "\n", Colors.Orange);
                await InsertionSort(array, i, Math.Min(i + RUN - 1, n - 1));
                DisplayBars(); // Обновляем визуализацию после сортировки вставками
                await Task.Delay(delayMilliseconds);  // Добавляем задержку после каждой операции сортировки
            }

            await Task.Delay(delayMilliseconds);
            AddColoredText("2. Объединяем отсортированные руны с помощью сортировки слиянием." + "\n", Colors.Orange);

            // Объединяем руны с использованием сортировки слиянием
            int size = RUN;
            while (size < n)
            {
                AddColoredText($"Слияние блоков с размером {size}." + "\n", Colors.Purple);
                for (int left = 0; left < n; left += 2 * size)
                {
                    int mid = Math.Min(left + size - 1, n - 1);
                    int right = Math.Min((left + 2 * size - 1), (n - 1));
                    if (mid < right)
                    {
                        await Merge(array, left, mid, right); // Делаем слияние с задержкой
                        DisplayBars(); // Обновляем визуализацию после слияния
                        await Task.Delay(delayMilliseconds);  // Задержка после слияния
                    }
                }
                size *= 2;  // Удваиваем размер блока
            }

            // Финальный вывод
            AddColoredText("Сортировка завершена!" + "\n", Colors.Green);
            await Task.Delay(delayMilliseconds);
        }

        // Сортировка вставками
        public async Task InsertionSort(int[] array, int left, int right)
        {
            AddColoredText($"Сортировка вставками подмассива с индексами от {left} до {right}." + "\n", Colors.Orange);
            for (int i = left + 1; i <= right; i++)
            {
                await Task.Delay(delayMilliseconds);
                int temp = array[i];
                int j = i - 1;

                while (j >= left && array[j] > temp)
                {
                    array[j + 1] = array[j];
                    j--;
                }
                array[j + 1] = temp;

                DisplayBars(i, j); // Обновляем визуализацию после вставки, подсвечивая активные элементы
            }
        }

        // Слияние двух подмассивов
        public async Task Merge(int[] array, int left, int mid, int right)
        {
            AddColoredText($"Слияние подмассивов: от {left} до {mid} и от {mid + 1} до {right}." + "\n", Colors.Green);
            int len1 = mid - left + 1;
            int len2 = right - mid;
            int[] leftArray = new int[len1];
            int[] rightArray = new int[len2];

            for (int x = 0; x < len1; x++)
                leftArray[x] = array[left + x];
            for (int x = 0; x < len2; x++)
                rightArray[x] = array[mid + 1 + x];

            int i = 0, j = 0, k = left;
            while (i < len1 && j < len2)
            {
                if (leftArray[i] <= rightArray[j])
                {
                    array[k] = leftArray[i];
                    i++;
                }
                else
                {
                    array[k] = rightArray[j];
                    j++;
                }
                k++;
            }

            while (i < len1)
            {
                array[k] = leftArray[i];
                i++;
                k++;
            }

            while (j < len2)
            {
                array[k] = rightArray[j];
                j++;
                k++;
            }

            DisplayBars(); // Обновляем визуализацию после слияния
        }

        // Визуализация с подсветкой активных элементов
        private void DisplayBars(int highlightedIndex1 = -1, int highlightedIndex2 = -1)
        {
            // Очищаем Canvas перед отрисовкой новых столбиков
            VisualizationCanvas.Children.Clear();

            double barWidth = VisualizationCanvas.ActualWidth / ourArray.Length;  // Ширина одного столбика
            double canvasHeight = VisualizationCanvas.ActualHeight;  // Высота Canvas

            // Находим максимальное значение в массиве для нормализации высоты столбиков
            int maxValue = ourArray.Max();

            // Перебираем все элементы массива для отрисовки
            for (int i = 0; i < ourArray.Length; i++)
            {
                // Создаем новый прямоугольник (столбик)
                Rectangle bar = new Rectangle
                {
                    Width = barWidth - 2,  // Добавляем небольшой отступ между столбиками
                    Height = (canvasHeight / maxValue) * ourArray[i],  // Высота столбика пропорциональна значению
                    Fill = (i == highlightedIndex1 || i == highlightedIndex2) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Blue)  // Подсветка активных элементов
                };

                // Устанавливаем позицию столбика на Canvas
                Canvas.SetLeft(bar, i * barWidth);  // Позиция по горизонтали
                Canvas.SetTop(bar, canvasHeight - bar.Height);  // Позиция по вертикали (снизу)

                // Добавляем столбик на Canvas
                VisualizationCanvas.Children.Add(bar);
            }
        }
    }

}
