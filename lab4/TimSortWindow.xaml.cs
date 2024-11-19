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
                AddColoredText("Начало сортировки" + "\n", Colors.DarkGreen);
                await Task.Delay(delayMilliseconds);
                AddColoredText("Задаём минимальный размер подмассивов(RUN), которые будут сортироваться с помощью сортировки вставками." + "\n", Colors.Orange);
                await Task.Delay(delayMilliseconds);
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




        const int RUN = 32; // Минимальный размер руна
        public async void Run(int[] array)
        {
            TimSort(array, array.Length);
        }

        async void TimSort(int[] array, int n)
        {
            AddColoredText("Разбиваем массив на подмассивы длиной RUN, которые сортируются с использованием сортировки вставками." + "\n", Colors.Orange);             
            for (int i = 0; i < n; i += RUN)
            {
                InsertionSort(array, i, Math.Min(i + RUN - 1, n - 1));
            }
            await Task.Delay(delayMilliseconds);
            AddColoredText("Объединяем отсортированные руны с помощью сортировки слиянием" + "\n", Colors.Orange);            
            for (int size = RUN; size < n; size = 2 * size)
            {
                for (int left = 0; left < n; left += 2 * size)
                {
                    int mid = left + size - 1;
                    int right = Math.Min((left + 2 * size - 1), (n - 1));
                    if (mid < right)
                        Merge(array, left, mid, right);
                }
            }
        }
        // Сортировка вставками
        async void InsertionSort(int[] array, int left, int right)
        {
            for (int i = left + 1; i <= right; i++)
            {
                await Task.Delay(delayMilliseconds);
                AddColoredText("Сохраняем текущий элемент, который нужно вставить в отсортированную часть массива" + "\n", Colors.Orange);
                int temp = array[i];
                await Task.Delay(delayMilliseconds);
                AddColoredText("Помечаем элемент в отсортированной части массива" + "\n", Colors.Orange);
                int j = i - 1;

                await Task.Delay(delayMilliseconds);
                AddColoredText("Ищем позицию для вставки элемента" + "\n", Colors.Orange);
                await Task.Delay(delayMilliseconds);
                AddColoredText("Проверяем два условия: является ли помеченный элемент больше начального индекса подмассива: чтобы не выйти за пределы отсортированной части." +
                    "является ли текущий элемент больше, чем элемент, который нужно вставить. Если да, двигаем его вправо." + "\n", Colors.Orange);
                while (j >= left && array[j] > temp)
                {
                    await Task.Delay(delayMilliseconds);
                    AddColoredText("Сдвигаем текущий элемент на одну позицию вправо, освобождая место для вставки." + "\n", Colors.Orange);
                    array[j + 1] = array[j];
                    j--;
                }
                array[j + 1] = temp;
                await Task.Delay(delayMilliseconds);
                AddColoredText("Вставляем наш элемент." + "\n", Colors.Orange);
            }
        }

        // Слияние двух подмассивов
        async void Merge(int[] array, int left, int mid, int right)
        {
            await Task.Delay(delayMilliseconds);
            AddColoredText("Создаем временные массивы для левой и правой половин" + "\n", Colors.Orange);            
            int len1 = mid - left + 1;
            int len2 = right - mid;
            int[] leftArray = new int[len1];
            int[] rightArray = new int[len2];

            await Task.Delay(delayMilliseconds);
            AddColoredText("Копируем данные во временные массивы" + "\n", Colors.Orange);
            for (int x = 0; x < len1; x++)
                leftArray[x] = array[left + x];
            for (int x = 0; x < len2; x++)
                rightArray[x] = array[mid + 1 + x];

            // Индексы для левой, правой половин и основного массива
            int i = 0, j = 0, k = left;
            await Task.Delay(delayMilliseconds);
            AddColoredText("Сливаем два массива" + "\n", Colors.Orange);
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
            await Task.Delay(delayMilliseconds);
            AddColoredText("Копируем оставшиеся элементы" + "\n", Colors.Orange);
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
        }
    }
}
