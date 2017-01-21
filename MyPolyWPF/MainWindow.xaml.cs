using MyPolymtl.Polymtl.Schedules;
using MyPolyWPF.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyPolyWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Pages.ScheduleView myScheduleView_;
        Pages.CourseView courseView_;
        Pages.Login loginView_;
        Credential credential;
        public MainWindow()
        {
            InitializeComponent();
            loginView_ = new Pages.Login(new Action<Credential>(LoginCallBack));
            myScheduleView_ = new Pages.ScheduleView();
            myScheduleView_.ViewCourse += MyScheduleView__ViewCourse;

            frame.Content=(loginView_);
        }

        private void MyScheduleView__ViewCourse(object sender, Pages.CourseViewEventArg e)
        {
            courseView_ = courseView_ ?? new Pages.CourseView();
            courseView_.DataContext = e.Course;
            frame.Navigate(courseView_);
        }
        private void LoginCallBack(Credential credential)
        {
            this.credential = credential;
            LoadSchedule(credential);
            frame.Navigate(myScheduleView_);

        }
        public async void LoadSchedule(Credential credential)
        {
            
            var mypoly = new MyPolymtl.Polymtl.Web.WebExtracter(credential.Username, credential.Password, credential.Birthdate);
            Schedule task = null;
            if (MessageBox.Show("Get schedule from local storage?\nIf you select \"no\","+
                                "local storage will be updated by current semester schedule from moodle","Schedule", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                 task = ReadSchedule();


            if (task == null) 
            {
                task = await mypoly.GetMySchedule();
                SaveSchedule(task);
            }
            myScheduleView_.SetSchedule(task);
        }
        private void SaveSchedule(Schedule schedule)
        {
            BinaryFormatter bin = new BinaryFormatter();

            using (var sw = new StreamWriter("schedule.bin"))
                bin.Serialize(sw.BaseStream, schedule);

        }
        private Schedule ReadSchedule()
        {
            
            BinaryFormatter bin = new BinaryFormatter();
            Schedule schedule = null;
            using (var sr = new StreamReader("schedule.bin"))
                schedule = bin.Deserialize(sr.BaseStream) as Schedule;

            return schedule;
        }

    }
}
