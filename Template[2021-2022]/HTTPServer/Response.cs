using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        NotFound = 404,
        BadRequest = 400,
        Redirect = 301
    }

    class Response
    {
        string responseString;
        public string ResponseString
        {
            get
            {
                return responseString;
            }
        }
        StatusCode code;
        string Location;
        List<string> headerLines = new List<string>();
        public Response(StatusCode code, string contentType, string content, int contentLength, string redirectoinPath)
        {
            this.code = code;
            // Add headlines (Content-Type, Content-Length,Date, [location if there is redirection])
            headerLines.Add(contentType);
            headerLines.Add(contentLength.ToString());
            headerLines.Add(DateTime.Now.ToString());
            // Create the request string
            string Status = GetStatusLine(code);

            if (code == StatusCode.Redirect)
            {
                Location = redirectoinPath;
                headerLines.Add(Location);

                responseString = Status + "\r\n" + "Content-Type:" + headerLines[0] + "\r\n" + "Content-Length:" + headerLines[1] +
                    "\r\n" + "Date-time:" + headerLines[2] + "\r\n" + "Location:" + headerLines[3] + "\r\n" + "\r\n" + content;
            }
            else
            {
                responseString = Status + "\r\n" + "Content-Type:" + headerLines[0] + "\r\n" + "Content-Length:" + headerLines[1] +
                    "\r\n" + "Date-time:" + headerLines[2] + "\r\n" + "\r\n" + content;
            }

        }
        string message;
        private string GetStatusLine(StatusCode code)
        {
            // Create the response status line and return it
            string statusLine = string.Empty;
            if (code == StatusCode.BadRequest)
            {
                message = "Bad Request Error";
            }
            else if (code == StatusCode.InternalServerError)
            {
                message = "Internal Serever Error";
            }
            else if (code == StatusCode.NotFound)
            {
                message = "Not Found Error";
            }
            else if (code == StatusCode.Redirect)
            {
                message = "Redirection Error";
            }
            else if (code == StatusCode.OK)
            {
                message = "OK";
            }
            statusLine = Configuration.ServerHTTPVersion + " " + code.GetHashCode() + " " + message;
            return statusLine;
        }
    }
}
