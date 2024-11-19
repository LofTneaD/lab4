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
    /// Логика взаимодействия для QuickSortWindow.xaml
    /// </summary>
    public partial class QuickSortWindow : Window
    {
        public QuickSortWindow()
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
                AddColoredText("Сгенерирован массив с длинной : " + input + "\n", Colors.DarkGreen);

                ourArray = MakeMassive(Int32.Parse(input));
            }
            GenerateMassiveInputBox.Clear();
            GenerateMassiveInputPanel.Visibility = Visibility.Collapsed;
        }

        private async void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (ourArray != null)
            {
                Run(ourArray);
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


        public void Run(int[] array)
        {
            AddColoredText("Начало сортировки" + "\n", Colors.DarkGreen);
            algQuickSort(array, 0, array.Length - 1);
        }
        async void algQuickSort(int[] array, int low, int high)
        {
            if (low < high)
            {
                await Task.Delay(delayMilliseconds);
                AddColoredText("Делим массив на две части, и возвращаем индекс элемента, который будет на своём окончательном месте после сортировки" + "\n", Colors.Orange);
                int pivotIndex = Partition(array, low, high);
                await Task.Delay(delayMilliseconds);
                AddColoredText("Рекурсивно сортируем элементы до и после опорного элемента" + "\n", Colors.Orange);                
                algQuickSort(array, low, pivotIndex - 1);
                algQuickSort(array, pivotIndex + 1, high);
            }
        }

        // Метод для разделения массива на части (чтобы найти опорный элемент)
        int Partition(int[] array, int low, int high)
        {
            AddColoredText(high + "-ый элемент - опорный" + "\n", Colors.Orange);
            int pivot = array[high];
            int i = low - 1;
            AddColoredText("Создаем переменную, которая будет отслеживать индекс последнего элемента, меньшего опорного" + "\n", Colors.Orange);
            AddColoredText("Затем проходим по всем элементам массива от индекса " + low + " до " + (high - 1) + "\n", Colors.Orange);
            for (int j = low; j < high; j++)
            {
                AddColoredText("Проверяем является ли текущий элемент меньше опорного" + "\n", Colors.Orange);
                if (array[j] < pivot)
                {
                    AddColoredText("Увеличиваем индекс" + "\n", Colors.Orange);
                    i++;

                    Swap(array, i, j);
                }
            }
            AddColoredText("Ставим опорный элемент на правильное место" + "\n", Colors.Orange);            
            Swap(array, i + 1, high);

            return i + 1;
        }

        async void Swap(int[] array, int i, int j)
        {
            await Task.Delay(delayMilliseconds);
            AddColoredText("Меняем элементы " + i + " и " + j + " местами" + "\n", Colors.Black);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
