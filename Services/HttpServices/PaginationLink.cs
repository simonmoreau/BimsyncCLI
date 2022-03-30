using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BimsyncCLI.Services.HttpServices
{
    public class PaginationLink
    {
        public PaginationLink(string linkValue)
        {
            //<https://api.bimsync.com/v2/projects?pageSize=100&page=2>; rel="next",<https://api.bimsync.com/v2/projects?pageSize=100&page=2>; rel="last"
            string[] values = linkValue.Split(',');

            Regex rx = new Regex(@"(?<=rel="")(.*?)(?="")");

            foreach (string value in values)
            {
                MatchCollection matches = rx.Matches(value);
                if (matches.Count > 0)
                {
                    // Report on each match.
                    foreach (Match match in matches)
                    {
                        GroupCollection groups = match.Groups;
                        string rel = groups[0].Value;

                        switch (rel)
                        {
                            case "next":
                                next = GetUrl(value);
                                break;
                            case "prev":
                                prev = GetUrl(value);
                                break;
                            case "first":
                                first = GetUrl(value);
                                break;
                            case "last":
                                last = GetUrl(value);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private string GetUrl(string value)
        {
            Regex rx = new Regex(@"(?<=<)(.*?)(?=>)");
            MatchCollection matches = rx.Matches(value);
            string url = null;
            if (matches.Count > 0)
            {
                // Report on each match.
                foreach (Match match in matches)
                {
                    GroupCollection groups = match.Groups;
                    url = groups[0].Value;
                    break;
                }
            }

            return url;
        }

        public string next { get; }
        public string prev { get; }
        public string first { get; }
        public string last { get; }
    }

    public class ReturnValue<T>
    {
        public ReturnValue(T value, string next)
        {
            Value = value;
            Next = next;
        }
        public T Value { get; }
        public string Next { get; }
    }

}
