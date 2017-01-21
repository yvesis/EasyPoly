using MyPolymtl.Polymtl.Schedules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MyPolymtl.Polymtl.Web
{
    class WebExtracter
    {
        public string Matricule { get; set; }
        public string Password { get; set; }
        public string DateCode { get; set; }

        public async Task<Schedule> GetMySchedule()
        {
            var datas = new List<KeyValuePair<string, string>>();
            datas.Add(new KeyValuePair<string, string>("code", Matricule));
            datas.Add(new KeyValuePair<string, string>("nip", Password));
            datas.Add(new KeyValuePair<string, string>("naissance", DateCode));

            var form = new FormUrlEncodedContent(datas);
            var str = new StringContent($"code={Matricule}&nip={Password}&naissance={DateCode}", Encoding.UTF8, "application/x-www-form-urlencoded");
            var cookieContainer = new CookieContainer();
            //var httpHandler = new HttpClientHandler { CookieContainer = cookieContainer };
                 HttpClient client = new HttpClient(/*httpHandler*/);
                 client.BaseAddress = new Uri("https://dossieretudiant.polymtl.ca");

            //cookieContainer.Add( client.BaseAddress, new Cookie("cookie", Matricule, "/", client.BaseAddress.Host));

                 var request = new HttpRequestMessage(HttpMethod.Post, "/WebEtudiant7/ValidationServlet");
                 //var byteArray = new UTF8Encoding().GetBytes("<clientid>:<clientsecret>");
                 //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                 request.Content = form;
            //https://dossieretudiant.polymtl.ca/WebEtudiant7/ValidationServlet
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

            }
            var content = await response.Content.ReadAsStringAsync();
            var reader = new Reader(content);
            var inputs = reader.ReadAllNameValues(@"input type=""hidden""");
            var datas2 = new List<KeyValuePair<string, string>>();
            foreach(var xv in inputs)
            {
                datas2.Add(new KeyValuePair<string, string>(xv.Tag, xv.Value));
            }

            var request2 = new HttpRequestMessage(HttpMethod.Post, "/WebEtudiant7/PresentationHorairePersServlet");
            request2.Content = new FormUrlEncodedContent(datas2);
            var response2 = await client.SendAsync(request2);
            var content2 = await response2.Content.ReadAsStringAsync();
            reader = new Reader(content2);
            
            var divs = reader.ReadAll("<div class=\"wrapperPourListeCoursActuels\">");
            var divs2 = reader.ReadAll("<div class=\"row replace-bottom tableListeCoursActuels\">");

            var courses = GetCourses(divs2);
            var days = GetDays(divs,courses.ToList());

            var sch = new Schedule();
            sch.AddDays(days);
            return sch;
        }
        public async void GetMyMoodle()
        {
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("username",Matricule),
                new KeyValuePair<string,string>("password",Password),
            });

            var client = new HttpClient { BaseAddress = new Uri("https://moodle.polymtl.ca") };
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");
            using (var request = new HttpRequestMessage(HttpMethod.Post, "/login/index.php"))
            {
                request.Content = form;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var resp = await client.GetAsync("https://moodle.polymtl.ca/my/");
                var content = await resp.Content.ReadAsStringAsync();

            }
     
            
        }
        public IEnumerable<Course> GetCourses(IEnumerable<XmlValue> contents)
        {
            var list = new List<Course>();
            foreach(var xmlcontent in contents)
            {

                using (var xr = XmlReader.Create(new StringReader($"{xmlcontent.Tag}{xmlcontent.Value}")))
                {
                    List<string> labels = new List<string>();
                    List<string> values = new List<string>();
                    bool isLabel = false;
                    string labelName = "";
                    try
                    {
                        while (xr.Read())
                        {
                            switch (xr.NodeType)
                            {
                                case XmlNodeType.Element:
                                    if (!isLabel)
                                        isLabel = xr.Name == "label";
                                    break;
                                case XmlNodeType.EndElement:
                                    break;
                                case XmlNodeType.Text:
                                    if (isLabel)
                                    {
                                        labelName = xr.Value.ToString();
                                        //labels.Add(xr.Value.ToString());
                                        isLabel = false;
                                    }
                                    else
                                    {
                                        labels.Add(labelName);
                                        values.Add(xr.Value.Trim());
                                    }
                                    break;
                                case XmlNodeType.Attribute:

                                    break;

                            }
                        }
                        var prof = values[2];
                        //values.RemoveAt(2);
                        list.Add(new Course(values.ElementAt(labels.IndexOf("Intitulé")), values.ElementAt(labels.IndexOf("Sigle")), int.Parse(values.ElementAt(labels.IndexOf("Crédits"))))
                        {

                        });

                    }
                    catch (Exception e)
                    {
                        //Debugger.Break();
                    }
                }
            }

            return list;
        }
        public IEnumerable<Day> GetDays(IEnumerable<XmlValue> contents, List<Course> courses)
        {
            Dictionary<string, Day> daysList = new Dictionary<string, Day>();
            foreach (var xmlcontent in contents)
            {

                using (var xr = XmlReader.Create(new StringReader($"{xmlcontent.Tag}{xmlcontent.Value.Replace("<font color=#FF0000>", "").Replace("</font>", "")}")))
                {
                    
                    List<string> labels = new List<string>();
                    List<string> values = new List<string>();
                    bool isLabel = false;
                    string label = "";
                    try
                    {
                        
                        while (xr.Read())
                        {
                           
                            switch (xr.NodeType)
                            {
                                case XmlNodeType.Element:
                                    if (!isLabel)
                                        isLabel = xr.Name == "label";
                                    break;
                                case XmlNodeType.EndElement:
                                    break;
                                case XmlNodeType.Text:
                                    Debug.WriteLine(xr.Value);

                                    if (isLabel)
                                    {
                                        label = xr.Value.ToString();
                                        isLabel = false;
                                    }
                                    else
                                    {
                                        if (label == "Période" && labels.Contains(label))
                                        {
                                            var datas = new Dictionary<string, List<string>>();
                                            foreach (var v in values)
                                            {
                                                var key = labels[values.IndexOf(v)];
                                                if (!string.IsNullOrWhiteSpace(v))
                                                {
                                                    if (!datas.ContainsKey(key))
                                                    {
                                                        datas[key] = new List<string>();
                                                    }
                                                    datas[key].Add(v);

                                                }
                                            }

                                            var days = datas.Select(d =>
                                            {
                                                //if (d.Value.Count > 1) Debugger.Break();
                                                var cours = d.Value.Select(val => courses.Find(c => val.Contains(c.Initials))); /*courses.Find(c => d.Value.First().Contains(c.Initials));*/
                                                if (cours == null) return null;
                                                var ddd = values[labels.IndexOf("Période")].Replace("h", "");
                                                if (ddd.Contains("1745")) Debugger.Break();

                                                var date = DateTime.ParseExact(values[labels.IndexOf("Période")].Replace("h", ""), "HHmm", CultureInfo.InvariantCulture);
                                                var hours = d.Value.Select(val =>
                                                {
                                                        var h = new Hour
                                                        {
                                                            Time = new TimeSpan(date.Hour, date.Minute, 0)
                                                        };
                                                        try
                                                        {
                                                            h.Room = Regex.Split(val, "([A-Z]{1}-[0-9]*)")[1];
                                                            var c = courses.Find(c2 => val.Contains(c2.Initials));
                                                        if (c != null)
                                                            h.Courses.Add(c);
                                                        }
                                                        catch
                                                        {
                                                            //return null;
                                                        };
                                                        return h;
                                                });


                                                var x = new Day { DayName = d.Key };
                                                hours.ToList().ForEach(h => x.AddHour(h));
                                                //var b = x.AddHour(h);
                                                //if (cours.Where(c => c != null).Count() > 1) Debugger.Break();
                                                //h.Courses.AddRange(cours.Where(c => c != null));
                                                return x;
                                            });
                                            
                                            foreach(var day in days.Where(d=> d!= null))
                                            {
                                                Day d = null;
                                                daysList.TryGetValue(day.DayName, out d);
                                                if (d == null)
                                                    daysList[day.DayName] = day;
                                                else
                                                    d.Merge(day);
                                            }
                                            labels.RemoveRange(1, labels.Count - 1);
                                            values.Clear();
                                            values.Add(xr.Value.Trim());

                                        }
                                        else
                                        {
                                            var v = xr.Value.Trim();
                                            if(!string.IsNullOrWhiteSpace(v))
                                            {
                                                //if (v.Contains("4700")) Debugger.Break();
                                                labels.Add(label);
                                                values.Add(v);
                                            }
                                        }
                                    }
                                    break;
                                case XmlNodeType.Attribute:

                                    break;

                            }
                        }
                        //var prof = values[2];
                        //values.RemoveAt(2);
                        //list.Add(new Course(values.ElementAt(labels.IndexOf("Intitulé")), values.ElementAt(labels.IndexOf("Sigle")), int.Parse(values.ElementAt(labels.IndexOf("Crédits"))))
                        //{

                        //});

                    }
                    catch(Exception e) {


                        //Debugger.Break();
                    }
                }
            }

            return daysList.Values.ToList();
        }
        public WebExtracter(string code, string nip, string naiss)
        {
            Matricule = code;
            Password = nip;
            DateCode = naiss;
        }
    }
}
