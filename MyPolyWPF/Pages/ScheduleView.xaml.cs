using MyPolymtl.Polymtl.Schedules;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace MyPolyWPF.Pages
{
    /// <summary>
    /// Interaction logic for ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        public delegate void ViewCourseHandler(object sender, CourseViewEventArg e);
        public event ViewCourseHandler ViewCourse;
        public ScheduleView()
        {
            InitializeComponent();
        }
        public void SetSchedule(Schedule sched)
        {
            //Day heures = new Day { DayName = "Heures" };
            //heures.AddHour(new Hour { Time = TimeSpan.FromHours(8.5) });
            //heures.AddHour(new Hour { Time = TimeSpan.FromHours(9.5) });
            //heures.AddHour(new Hour { Time = TimeSpan.FromHours(10.5) });
            //heures.AddHour(new Hour { Time = TimeSpan.FromHours(11.5) });
            //heures.AddHour(new Hour { Time = TimeSpan.FromHours(12.75) });
            //heures.AddHour(new Hour { Time = TimeSpan.FromHours(13.75) });
            //heures.AddHour(new Hour { Time = TimeSpan.FromHours(14.75) });
            //heures.AddHour(new Hour { Time = TimeSpan.FromHours(15.75) });
            //heures.AddHour(new Hour { Time = TimeSpan.FromHours(16.75) });
            //heures.AddHour(new Hour { Time = TimeSpan.FromHours(17.75) });

            var list = sched.GetDays().ToList();
            //list.Add(heures);
            schedItems.ItemsSource = list;// sched.GetDays();
        }
        /// <summary>
        /// Handles property changed event for the ItemsPerRow property, constructing
        /// the required ItemsPerRow elements on the grid which this property is attached to.
        /// </summary>
        private static void OnItemsPerRowPropertyChanged(DependencyObject d,
                            DependencyPropertyChangedEventArgs e)
        {
            Grid grid = d as Grid;
            int itemsPerRow = (int)e.NewValue;

            // construct the required row definitions
            grid.LayoutUpdated += (s, e2) =>
            {
                var childCount = grid.Children.Count;

                // add the required number of row definitions
                int rowsToAdd = (childCount - grid.RowDefinitions.Count) / itemsPerRow;
                for (int row = 0; row < rowsToAdd; row++)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                }

                // set the row property for each chid
                for (int i = 0; i < childCount; i++)
                {
                    var child = grid.Children[i] as FrameworkElement;
                    Grid.SetRow(child, i / itemsPerRow);
                }
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var param = ((Button)sender).CommandParameter as Course;
            ViewCourse?.Invoke(this, new CourseViewEventArg(param));
        }
    }
    public class DayConverter : MarkupExtension, IValueConverter
    {
        string[] daynames_ = new[] {"Période", "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            var i= daynames_.ToList().IndexOf(value.ToString());
            return i;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    public class HourConverter : MarkupExtension, IValueConverter
    {
        string[] daynames_ = new[] {"Heures", "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            var day = (KeyValuePair < int, List< Hour >>)value  ;
            var i = day.Key - 8;
            return day.Key - 8;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    public class CourseViewEventArg:RoutedEventArgs
    {
        public Course Course { get; }
        public CourseViewEventArg(Course course)
        {
            Course = course;
        }
    }
}
