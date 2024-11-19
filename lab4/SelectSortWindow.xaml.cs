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
                AddColoredText("Начало сортировки" + "\n", Colors.DarkGreen);
                for (int i = 0; i < array.Length - 1; i++)
                {
                    int minIndex = i;
                    AddColoredText("Предположим, что текущий индекс - минимальный" + "\n", Colors.Orange);
                    await Task.Delay(delayMilliseconds);
                    AddColoredText("Ищем минимальный элемент в оставшейся части массива. Если найденый элемент меньше текущего, делаем его индекском минимального" + "\n", Colors.Orange);

                    for (int j = i + 1; j < array.Length; j++)
                    {
                        AddColoredText("Сравниваются элементы под номерами " + j + " и " + minIndex + "\n", Colors.Black);
                        if (array[j] < array[minIndex])
                        {
                            AddColoredText("Теперь " + j + "-ый элемент является минимальным" + "\n", Colors.Black);
                            minIndex = j;
                        }
                    }
                    await Task.Delay(delayMilliseconds);
                    AddColoredText("Нашли минимальный элемент" + "\n", Colors.Orange);

                    await Task.Delay(delayMilliseconds);
                    AddColoredText("Меняем местами текущий элемент и минимальный" + "\n", Colors.Orange);
                    AddColoredText("Проверяем, не сменился ли минимальный индекс" + "\n", Colors.Black);
                    if (minIndex != i)
                    {
                        AddColoredText("Минимальный индекс сменился, меняем элементы" + i + " и " + minIndex + "местами" + "\n", Colors.Black);
                        int temp = array[i];
                        array[i] = array[minIndex];
                        array[minIndex] = temp;
                    }
                    await Task.Delay(delayMilliseconds);
                    AddColoredText("Повторяем для следующего элемента" + "\n", Colors.Orange);
                }
                await Task.Delay(delayMilliseconds);
                AddColoredText("Массив отсортирован" + "\n", Colors.DarkGreen);
            }
            else
                AddColoredText("Массив не сгенерирован" + "\n", Colors.Red);
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
    }
}
