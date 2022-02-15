using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPServer
{
    public enum RequestMethod
    {
        GET,
        POST,
        HEAD
    }

    public enum HTTPVersion
    {
        HTTP10,
        HTTP11,
        HTTP09
    }

    class Request
    {
        string requestString;       //stores the request as 1 string
        string[] receivedRequest;   //stores all lines in the request
        string[] requestLines;      //stores the 3 keywords in request line
        Dictionary<string, string> headerLines;
        string contentLines;        //stores content lines for post
        RequestMethod method;
        public string relativeURI;

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }
        
        public RequestMethod Method
        {
            get { return method; }
        }

        HTTPVersion httpVersion;


        public Request(string requestString)
        {
            this.requestString = requestString;
        }
        /// <summary>
        /// Parses the request string and loads the request line, header lines and content, returns false if there is a parsing error
        /// </summary>
        /// <returns>True if parsing succeeds, false otherwise.</returns>
        public bool ParseRequest()
        {
            headerLines = new Dictionary<string, string>();
            // request line, heder lines, blank lines, content
            //parse the receivedRequest using the \r\n delimeter   
            string[] separatingStrings = { "\r\n" };
            receivedRequest = requestString.Split(separatingStrings, System.StringSplitOptions.None);
            int n = receivedRequest.Length;

            // check that there is atleast 3 lines: Request line, Host Header, Blank line (usually 4 lines with the last empty line for empty content)
            if (n < 3) 
            {
                //throw 400 bad request error
                return false;
            }

            // Parse Request line
            // method, uri, http-version
            if (!ParseRequestLine())
            {
                return false;
            }

            //bonus
            method = (RequestMethod)Enum.Parse(typeof(RequestMethod), requestLines[0]);

            relativeURI = requestLines[1];

            //parsing http version to enum
            string versionNum = requestLines[2].Split('/')[1];
            string version;
            if (versionNum != "")
            {
                version = "HTTP" + (Convert.ToDouble(versionNum) * 10).ToString();
            }
            else
            {
                version = "HTTP09";
            }

            if (version != "HTTP11" && version != "HTTP10" && version != "HTTP09")
            {
                return false;
            }

            httpVersion = (HTTPVersion)Enum.Parse(typeof(HTTPVersion), version);

            // Validate blank line exists
            if (!ValidateBlankLine(n))
            {
                return false;
            }

            // Load header lines into HeaderLines dictionary
            LoadHeaderLines(n);

            if (!ValidateIsURI(relativeURI))
            {
                return false;
            }

            return true;
        }

        private bool ParseRequestLine()
        {
            requestLines = receivedRequest[0].Split(' ');
            if (requestLines.Length == 3)
                return true;
            return false;
        }

        private bool ValidateIsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.Relative);
        }

        private void LoadHeaderLines(int size)
        {
            string[] hSeparator = { ": " };
            for (int i = 1; i < size - 2; i++)
            {
                string[] parsedHeaderLine = receivedRequest[i].Split(hSeparator, System.StringSplitOptions.None);
                headerLines.Add(parsedHeaderLine[0], parsedHeaderLine[1]);
            }
        }

        private bool ValidateBlankLine(int size)
        {
            if (receivedRequest[size - 2] != "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
