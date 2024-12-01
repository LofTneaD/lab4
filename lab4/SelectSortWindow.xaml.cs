using System;
using System.Collections;
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
using System.Threading.Tasks;

namespace lab4
{
    /// <summary>
    /// Логика взаимодействия для SelectSortWindow.xaml
    /// </summary>
    public partial class SelectSortWindow : Window
    {
        public SelectSortWindow()
        {
            InitializeComponent();
        }

        int[] array;
        private int delayMilliseconds = 100; // Значение задержки по умолчанию
                                             
        private void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            delayMilliseconds = (int)DelaySlider.Value;
            DelayLabel.Text = $"Задержка: {delayMilliseconds} мс";
        }

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
                AddColoredText("Сгенерирован массив с длинной : " + input + "\n", Colors.DarkGreen);

                array = MakeMassive(Int32.Parse(input));

                // Отображаем столбики сразу после генерации массива
                UpdateVisualization();
            }

            GenerateMassiveInputBox.Clear();
            GenerateMassiveInputPanel.Visibility = Visibility.Collapsed;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Document.Blocks.Clear();
        }


        private async void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (array != null)
            {
                AddColoredText("Начало сортировки\n", Colors.DarkGreen);
                UpdateVisualization();

                for (int i = 0; i < array.Length - 1; i++)
                {
                    int minIndex = i;
                    AddColoredText($"Итерация {i + 1}: Предположим, что элемент с индексом {i} ({array[i]}) минимальный\n", Colors.Orange);

                    for (int j = i + 1; j < array.Length; j++)
                    {
                        // Подсветка сравниваемых столбиков
                        HighlightBars(i, j, Brushes.Red);
                        AddColoredText($"Сравниваем элемент с индексом {j} ({array[j]}) и текущий минимальный элемент {minIndex} ({array[minIndex]})\n", Colors.Black);

                        await Task.Delay(delayMilliseconds);

                        if (array[j] < array[minIndex])
                        {
                            minIndex = j;
                            AddColoredText($"Найден новый минимальный элемент: индекс {minIndex}, значение {array[minIndex]}\n", Colors.Blue);
                        }

                        // Снять подсветку
                        HighlightBars(i, j, Brushes.Blue);
                    }

                    if (minIndex != i)
                    {
                        // Меняем местами элементы
                        AddColoredText($"Меняем местами элемент с индексом {i} ({array[i]}) и минимальный элемент с индексом {minIndex} ({array[minIndex]})\n", Colors.Green);

                        SwapBars(i, minIndex);
                        await Task.Delay(delayMilliseconds);

                        int temp = array[i];
                        array[i] = array[minIndex];
                        array[minIndex] = temp;

                        UpdateVisualization();
                    }

                    AddColoredText($"Итерация {i + 1} завершена. Массив после итерации: {string.Join(", ", array)}\n", Colors.Orange);
                }

                AddColoredText("Сортировка завершена! Итоговый массив: " + string.Join(", ", array) + "\n", Colors.DarkGreen);
            }
            else
            {
                AddColoredText("Ошибка: Массив не сгенерирован\n", Colors.Red);
            }
        }


        private void PrintMassiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (array != null)
            {
                AddColoredText("Элементы массива:" + "\n", Colors.Black);
                foreach (int i in array)
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

            // Берем первый параграф, если он уже существует, иначе создаем новый
            if (LogBox.Document.Blocks.FirstBlock is Paragraph paragraph)
            {
                paragraph.Inlines.Add(coloredRun);
            }
            else
            {
                paragraph = new Paragraph(coloredRun)
                {
                    Margin = new Thickness(0) // Убираем отступы
                };
                LogBox.Document.Blocks.Add(paragraph);
            }        
        }
        private void UpdateVisualization()
        {
            VisualizationCanvas.Children.Clear();
            if (array != null && array.Length > 0)
            {
                double canvasWidth = VisualizationCanvas.ActualWidth;
                double canvasHeight = VisualizationCanvas.ActualHeight;
                double barWidth = canvasWidth / array.Length;
                int maxValue = array.Max();

                for (int i = 0; i < array.Length; i++)
                {
                    double barHeight = (array[i] / (double)maxValue) * canvasHeight;
                    Rectangle bar = new Rectangle
                    {
                        Width = barWidth - 2, // Зазор между столбиками
                        Height = barHeight,
                        Fill = Brushes.Blue
                    };
                    Canvas.SetLeft(bar, i * barWidth);
                    Canvas.SetBottom(bar, 0);
                    VisualizationCanvas.Children.Add(bar);
                }
            }
        }
        private void HighlightBars(int index1, int index2, Brush color)
        {
            if (VisualizationCanvas.Children[index1] is Rectangle bar1 &&
                VisualizationCanvas.Children[index2] is Rectangle bar2)
            {
                bar1.Fill = color;
                bar2.Fill = color;
            }
        }

        private void SwapBars(int index1, int index2)
        {
            // Получаем ссылки на столбики
            var bar1 = (Rectangle)VisualizationCanvas.Children[index1];
            var bar2 = (Rectangle)VisualizationCanvas.Children[index2];

            // Получаем текущие координаты столбиков
            double left1 = Canvas.GetLeft(bar1);
            double left2 = Canvas.GetLeft(bar2);

            // Меняем визуальные координаты столбиков
            Canvas.SetLeft(bar1, left2);
            Canvas.SetLeft(bar2, left1);
        }
    }
}
