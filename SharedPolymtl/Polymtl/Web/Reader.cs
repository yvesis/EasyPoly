using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MyPolymtl.Polymtl.Web
{
    public class Reader
    {
        public string XmlContent { get;}
        public Reader(string xmlcontent)
        {
            XmlContent = xmlcontent;
        }
        public IEnumerable<XmlValue> ReadAllNameValues(string tag)
        {
            int i = XmlContent.IndexOf(tag);
            do
            {
                var i2 = XmlContent.IndexOf(">", i + 1);
                var content = XmlContent.Substring(i, i2 - i + 1);
                var name = ReadAttribute(content, "name");
                var value = ReadAttribute(content, "value");
                yield return new XmlValue(name,value);
            } while ((i = XmlContent.IndexOf(tag, i + 1)) > -1);


            //return null;
        }
        private string ReadAttribute(string content,string name)
        {
            int i = content.IndexOf(name);
            if (i == -1) return null;
            int i2 = content.IndexOf("\"", i);
            if (i2 == -1) return null;

            int i3 = content.IndexOf("\"", i2 + 1);
            if (i3 == -1) return null;

            return content.Substring(i2 + 1, i3 - i2 - 1);

        }
        public IEnumerable<XmlValue> ReadAll(string tag = null)
        {
            var regcomms = new Regex("<!--.*?-->", RegexOptions.Singleline);
            var test = regcomms.IsMatch(XmlContent);
            var a = regcomms.Replace(XmlContent, " ");
            //var nocomms = string.Join(" ",regcomms.Split(XmlContent));
            var regex = new Regex(tag);
            var results = regex.Split(a);
            for (int i = 1; i < results.Length; i++)
                yield return new XmlValue(tag, $"{results[i].Replace("<br>", "<br/>")}");


        }

    }
    public class XmlValue
    {
        public string Tag { get; }
        public string Value { get; }
        public XmlValue(string tag, string value)
        {
            Tag = tag;
            Value = value;
        }
    }
}
