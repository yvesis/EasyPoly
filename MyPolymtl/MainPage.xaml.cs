using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyPolymtl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            LoadSchedule();
        }
        public async void LoadSchedule()
        {
            var mypoly = new Polymtl.Web.WebExtracter("yvngub", "Victoi01", "820427");
            var task = await mypoly.GetMySchedule();
            lbSchedule.ItemsSource = task.GetDays();
            //mypoly.GetMyMoodle();
        }
    }
    public class DayToIntConverter :  IValueConverter
    {
        //private static string days = new string [] { };
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return 0;
            var i = CultureInfo.CurrentCulture.DateTimeFormat.DayNames.ToList().IndexOf(value.ToString());
            return Math.Max(0, i);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            int i = 0;
            int.TryParse(value.ToString(), out i);
            return CultureInfo.CurrentCulture.DateTimeFormat.DayNames[i];
        }
    }
}
