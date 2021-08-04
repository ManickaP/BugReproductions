using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;

namespace issue_38494
{
    class Program
    {
        static void Main(string[] args)
        {
            var d1 = new Dictionary<string, string>();
            var d2 = new Dictionary<string, string?>();
            var d3 = new List<KeyValuePair<string?, string?>>();
            var d4 = new List<KeyValuePair<string, string?>>();
            var d5 = new List<KeyValuePair<string, string>>();
            FormUrlEncodedContent(d1);
            FormUrlEncodedContent(d2);
            FormUrlEncodedContent(d3);
            FormUrlEncodedContent(d4);
            FormUrlEncodedContent(d5);
            FormUrlEncodedContent(null);
            FormUrlEncodedContent2(d1);
            FormUrlEncodedContent2(d2);
            FormUrlEncodedContent2(d3);
            FormUrlEncodedContent2(d4);
            FormUrlEncodedContent2(d5);
            FormUrlEncodedContent2(null);
            FormUrlEncodedContent3(d1);
            FormUrlEncodedContent3(d2);
            FormUrlEncodedContent3(d3);
            FormUrlEncodedContent3(d4);
            FormUrlEncodedContent3(d5);
            FormUrlEncodedContent3(null);
        }
        #nullable disable
        public static void FormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection){}

        #nullable restore
        public static void FormUrlEncodedContent2(IEnumerable<KeyValuePair<string, string>> nameValueCollection){}
        public static void FormUrlEncodedContent3(
            IEnumerable<KeyValuePair<
#nullable disable
            string, string
#nullable restore
            >> nameValueCollection){}

    }
}
