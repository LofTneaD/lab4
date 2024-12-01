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
    /// Логика взаимодействия для BubbleSortWindow.xaml
    /// </summary>
    public partial class BubbleSortWindow : Window
    {
        public BubbleSortWindow()
        {
            InitializeComponent();
        }

        int[] array;
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
                AddColoredText($"Сгенерирован массив с длиной: {input}\n", Colors.DarkGreen);
                array = MakeMassive(Int32.Parse(input));
                DisplayBars();
            }

            GenerateMassiveInputBox.Clear();
            GenerateMassiveInputPanel.Visibility = Visibility.Collapsed;
        }

        private async void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (array != null)
            {
                AddColoredText("Начало сортировки\n", Colors.DarkGreen);

                for (int i = 0; i < array.Length - 1; i++)
                {
                    for (int j = 0; j < array.Length - i - 1; j++)
                    {
                        // Подсветить сравниваемые элементы
                        HighlightBars(j, j + 1, Colors.Red);
                        await Task.Delay(delayMilliseconds);

                        AddColoredText($"Сравниваем элементы {j} и {j + 1}\n", Colors.Orange);

                        if (array[j] > array[j + 1])
                        {
                            await Task.Delay(delayMilliseconds);

                            AddColoredText($"Меняем элементы {j} и {j + 1} местами\n", Colors.Black);

                            // Поменять местами элементы
                            (array[j], array[j + 1]) = (array[j + 1], array[j]);

                            // Обновить визуализацию
                            SwapBars(j, j + 1);
                        }

                        // Снять подсветку
                        HighlightBars(j, j + 1, Colors.Blue);
                    }
                    await Task.Delay(delayMilliseconds);
                    AddColoredText("Повторяем для следующего элемента\n", Colors.Orange);
                }

                AddColoredText("Массив отсортирован\n", Colors.DarkGreen);
            }
            else
            {
                AddColoredText("Массив не сгенерирован\n", Colors.Red);
            }
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
            if (array != null)
            {
                AddColoredText("Элементы массива:\n", Colors.Black);
                foreach (int i in array)
                {
                    AddColoredText(i + "\n", Colors.Black);
                }
            }
            else
            {
                AddColoredText("Массив не сгенерирован\n", Colors.Red);
            }
        }

        public static int[] MakeMassive(int n)
        {
            Random random = new Random();
            int[] array = new int[n];

            for (int j = 0; j < array.Length; j++)
            {
                array[j] = random.Next(1, 100); // Ограничение на значения массива для визуализации
            }

            return array;
        }

        private void DisplayBars()
        {
            VisualizationCanvas.Children.Clear();

            double barWidth = VisualizationCanvas.ActualWidth / array.Length;
            double canvasHeight = VisualizationCanvas.ActualHeight;

            for (int i = 0; i < array.Length; i++)
            {
                Rectangle bar = new Rectangle
                {
                    Width = barWidth - 2,
                    Height = (canvasHeight / 100) * array[i],
                    Fill = new SolidColorBrush(Colors.Blue)
                };

                Canvas.SetLeft(bar, i * barWidth);
                Canvas.SetBottom(bar, 0);
                VisualizationCanvas.Children.Add(bar);
            }
        }

        private void SwapBars(int index1, int index2)
        {
            // Получаем ссылки на столбики
            var bar1 = (Rectangle)VisualizationCanvas.Children[index1];
            var bar2 = (Rectangle)VisualizationCanvas.Children[index2];

            // Сохраняем текущие позиции
            double left1 = Canvas.GetLeft(bar1);
            double left2 = Canvas.GetLeft(bar2);

            // Меняем визуальные позиции на Canvas
            Canvas.SetLeft(bar1, left2);
            Canvas.SetLeft(bar2, left1);

            // Удаляем элементы в порядке убывания индексов, чтобы избежать смещения
            VisualizationCanvas.Children.RemoveAt(Math.Max(index1, index2));
            VisualizationCanvas.Children.RemoveAt(Math.Min(index1, index2));

            // Вставляем элементы обратно в правильном порядке
            if (index1 < index2)
            {
                VisualizationCanvas.Children.Insert(index1, bar2);
                VisualizationCanvas.Children.Insert(index2, bar1);
            }
            else
            {
                VisualizationCanvas.Children.Insert(index2, bar1);
                VisualizationCanvas.Children.Insert(index1, bar2);
            }
        }

        private void HighlightBars(int index1, int index2, Color color)
        {
            var bar1 = (Rectangle)VisualizationCanvas.Children[index1];
            var bar2 = (Rectangle)VisualizationCanvas.Children[index2];

            bar1.Fill = new SolidColorBrush(color);
            bar2.Fill = new SolidColorBrush(color);
        }

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
    }
}
