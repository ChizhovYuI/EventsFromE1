using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace EventsInEkb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string Url = @"http://www.e1.ru/afisha/events/gastroli";

        public MainWindow()
        {
            InitializeComponent();

            startDatePicker.SelectedDate = DateTime.Now;
            endDatePicker.SelectedDate = DateTime.Now;
            LanguageProperty.OverrideMetadata(typeof (FrameworkElement), new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(new CultureInfo("ru-RU").IetfLanguageTag)));
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBoxItem = statusBar.Items[0] as ListBoxItem;
            if (listBoxItem == null) return;
            var ev = dataGrid.SelectedItem as Event;
            listBoxItem.Content = ev == null ? String.Empty : String.Concat("Адрес: ", ev.Address);
        }

        private void siteButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Url);
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            var parser = GetParser();
            if (parser == null)
                return;
            updateButton.IsEnabled = false;
            var task = new Task(GetEvents, parser);
            task.Start();
        }

        private EventsParser GetParser()
        {
            if (startDatePicker.SelectedDate == null || endDatePicker.SelectedDate == null)
                return null;
            var startDate = startDatePicker.SelectedDate.Value;
            var endDate = endDatePicker.SelectedDate.Value;
            if (startDate <= endDate) return new EventsParser(Url, startDate, endDate);
            MessageBox.Show("Некорректный выбор даты", "Ошибка!");
            return null;
        }

        private void GetEvents(object obj)
        {
            var parser = (EventsParser) obj;
            var result = new List<Event>();
            try
            {
                result = parser.GetEventsForSeveralDaysParrallel();
            }
            catch (WebException)
            {
                MessageBox.Show("Нет доступа к интернету или сервер не доступен", "Ошибка!");
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                dataGrid.ItemsSource = result;
                updateButton.IsEnabled = true;
            }));
        }
    }
}
