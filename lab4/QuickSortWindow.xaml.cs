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
                AddColoredText("Сгенерирован массив с длиной: " + input + "\n", Colors.DarkGreen);
                ourArray = MakeMassive(Int32.Parse(input));
                DrawBars(); // Отображаем начальный массив
            }
            GenerateMassiveInputBox.Clear();
            GenerateMassiveInputPanel.Visibility = Visibility.Collapsed;
        }

        private async void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (ourArray != null)
            {
                await Run(ourArray);
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
            DrawBars(); // Очистка и обновление визуализации
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
                array[j] = random.Next(1, 100); // Ограничиваем диапазон значений
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

        public async Task Run(int[] array)
        {
            AddColoredText("Начало сортировки" + "\n", Colors.DarkGreen);
            await QuickSort(array, 0, array.Length - 1);
            AddColoredText("Сортировка завершена" + "\n", Colors.DarkGreen);
            DrawBars(); // Финальная визуализация
        }

        public async Task QuickSort(int[] array, int low, int high)
        {
            if (low < high)
            {
                // Разбиение массива и получение индекса опорного элемента
                AddColoredText($"Разбиение массива с индексами от {low} до {high}" + "\n", Colors.Black);
                int pivotIndex = await Partition(array, low, high);
                AddColoredText($"После разбиения: опорный элемент на индексе {pivotIndex}" + "\n", Colors.Orange);

                // Рекурсивно сортируем части до и после опорного элемента
                AddColoredText($"Рекурсивная сортировка левой части (от {low} до {pivotIndex})" + "\n", Colors.Orange);
                await QuickSort(array, low, pivotIndex);  // Левая часть
                AddColoredText($"Рекурсивная сортировка правой части (от {pivotIndex + 1} до {high})" + "\n", Colors.Orange);
                await QuickSort(array, pivotIndex + 1, high); // Правая часть
            }
        }

        private async Task<int> Partition(int[] array, int low, int high)
        {
            int pivot = array[low];  // Опорный элемент (первый элемент)
            int i = low - 1;         // Индекс для меньших элементов
            int j = high + 1;        // Индекс для больших элементов
            HighlightBars(low, low, Colors.Green); // Подсветка опорного элемента
            AddColoredText($"Опорный элемент: {pivot}\n", Colors.Orange);

            while (true)
            {
                // Ищем элемент, который больше или равен опорному
                do
                {
                    i++;
                } while (array[i] < pivot);

                // Ищем элемент, который меньше или равен опорному
                do
                {
                    j--;
                } while (array[j] > pivot);

                if (i >= j)
                {
                    // После завершения работы с опорным элементом, подсвечиваем его в синий
                    HighlightBars(low, low, Colors.Blue);
                    AddColoredText($"Опорный элемент {pivot} поставлен на правильное место на индекс {j}\n", Colors.Orange);
                    return j;  // Возвращаем индекс для разделения массива
                }

                // Обмениваем элементы
                await Swap(array, i, j);
                HighlightBars(i, j, Colors.Red); // Подсветка сравниваемых элементов
                await Task.Delay(delayMilliseconds); // Задержка для анимации
            }
        }

        async Task Swap(int[] array, int i, int j)
        {
            await Task.Delay(delayMilliseconds); // Задержка для анимации обмена
            AddColoredText($"Меняем элементы {array[i]} и {array[j]} местами (индексы {i} и {j})\n", Colors.Black);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;

            // Обновляем визуализацию
            HighlightBars(i, j, Colors.Blue); // Подсветка обмена синим цветом
            DrawBars(); // Рисуем обновленный массив
        }

        // Метод для подсветки столбиков
        private void HighlightBars(int index1, int index2, Color color)
        {
            // Убедимся, что индексы в допустимом диапазоне
            if (index1 >= 0 && index1 < ourArray.Length && index2 >= 0 && index2 < ourArray.Length)
            {
                // Подсвечиваем элементы массива
                var rect1 = VisualizationCanvas.Children.OfType<Rectangle>().ElementAt(index1);
                var rect2 = VisualizationCanvas.Children.OfType<Rectangle>().ElementAt(index2);

                rect1.Fill = new SolidColorBrush(color);
                rect2.Fill = new SolidColorBrush(color);
            }
        }

        // Метод для отрисовки столбиков в Canvas
        private void DrawBars()
        {
            VisualizationCanvas.Children.Clear(); // Очищаем Canvas перед отрисовкой

            double width = VisualizationCanvas.ActualWidth / ourArray.Length; // Определяем ширину столбиков
            double maxHeight = VisualizationCanvas.ActualHeight;

            for (int i = 0; i < ourArray.Length; i++)
            {
                // Добавляем небольшое расстояние между столбиками
                var bar = new Rectangle
                {
                    Width = width - 2,  // Уменьшаем ширину для промежутка
                    Height = (ourArray[i] / 100.0) * maxHeight, // Масштабируем высоту столбика
                    Fill = new SolidColorBrush(Colors.Blue)  // Цвет столбиков теперь синий
                };

                Canvas.SetLeft(bar, i * width); // Располагаем столбик по оси X
                Canvas.SetBottom(bar, 0); // Располагаем столбик по оси Y (снизу)

                VisualizationCanvas.Children.Add(bar);
            }
        }
    }


}
