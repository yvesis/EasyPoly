using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
namespace MyPolymtl.Polymtl.Schedules
{
    [Serializable]
    public class Schedule
    {
        private List<Day> Days { get; } = new List<Day>();
        public IEnumerable<Day> GetDays()
        {
            return Days;
        }
        public void AddDay(Day day, bool merge = false)
        {
            var d2 = Days.TakeWhile(d => d.DayName == day.DayName).FirstOrDefault();

            if (d2 != null)
            {
                if (merge)
                {
                    // foreach (var h in day)
                    d2?.Merge(day);

                    return;
                }
            }
            else
                Days.Add(day);

        }
        public void AddDays(IEnumerable<Day> days, bool merge =false)
        {
            days.ToList().ForEach(d => AddDay(d, merge));
        }
    }
    [Serializable]
    public class Course:IEqualityComparer<Course>
    {
        public string Name { get; }
        public string Initials { get; }
        public int Credits { get; }
        public List<CourseLabs> Labs { get; } = new List<CourseLabs>();
        public List<CourseMaterial> Materials { get; } = new List<CourseMaterial>();
        public List<CourseChapter> Chapters { get; } = new List<CourseChapter>();

        public Course(string name, string initial, int credits)
        {
            Name = name;
            Initials = initial;
            Credits = credits;
        }
        public override string ToString() => $"{Initials} {Name} {Credits} Credits";

        public bool Equals(Course x, Course y)
        {
            return string.Compare(x.Initials, y.Initials, true) == 0;
        }

        public int GetHashCode(Course obj)
        {
            return obj.GetHashCode();
        }
    }
    [Serializable]
    public class CourseLabs
    {
        public string Initials { get; }
        public double Note { get; set; }
        public double Weight { get; set; }
    }
    [Serializable]
    public class CourseMaterial
    {
        public object Content { get; }
        public Uri ContentUri { get; }
        public CourseContentType ContentType { get; }
    }
    public enum CourseContentType
    {
        Embended,Link
    }
    [Serializable]
    public class CourseChapter
    {

    }
    [Serializable]
    public class Day:IEnumerable<List<Hour>>
    {

        private Dictionary<int, List<Hour>> Hours { get; } = new Dictionary<int, List<Hour>>();
        public string DayName { get; set; }
        public IEnumerator<List<Hour>> GetEnumerator()
        {
            return Hours.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Hours.GetEnumerator();
        }
        public void AddHour(Hour h)
        {
            if (!Hours.ContainsKey(h.Time.Hours))
            {
                Hours[h.Time.Hours] = new List<Hour>();
                Hours[h.Time.Hours].Add(h);
                return;
            }
            if (h.Courses.Count > 0)
                Hours[h.Time.Hours].Add(h);
        }
        public Hour GetHour(TimeSpan time)
        {
            List<Hour> h = null;
            Hours.TryGetValue(time.Hours, out h);
            return h.Where(t => t.Time == time).FirstOrDefault();
        }
        public void Merge(Day d)
        {
            foreach(var h in d.Hours)
            {
                List<Hour> hour = null;
                Hours.TryGetValue(h.Key, out hour);
                if (hour == null)
                    Hours[h.Key] = h.Value;
                else
                {
                    hour.AddRange(h.Value);
                }
            }
        }
        public IEnumerable<List<Hour>> GetHours() => Hours.Values;
        public override string ToString() => DayName;
    }
    [Serializable]
    public class Hour
    {
        public TimeSpan Time { get; set; } = TimeSpan.FromHours(8.5);
        public List<Course> Courses { get; } = new List<Course>();
        public string Room { get; set; }
    }
}
