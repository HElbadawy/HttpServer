using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;
        int portNum;

        public Server(int portNumber, string redirectionMatrixPath)
        {
            //call this.LoadRedirectionRules passing redirectionMatrixPath to it
            LoadRedirectionRules(redirectionMatrixPath);
            portNum = portNumber;
            //initialize this.serverSocket
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endP = new IPEndPoint(IPAddress.Any, portNum);
            serverSocket.Bind(endP);
        }
        public void StartServer()
        {
            // Listen to connections, with large backlog.
            serverSocket.Listen(100);
            Console.WriteLine("Start Listening");
            // Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                Socket clientS = serverSocket.Accept();
                //accept connections and start thread for each accepted connection.
                Thread newThread = new Thread(new ParameterizedThreadStart(HandleConnection));
                newThread.Start(clientS);
            }
        }

        Response returnedResponse;
        public void HandleConnection(object obj)
        {
            // Create client socket 
            Socket client_socket = (Socket)obj;
            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            client_socket.ReceiveTimeout = 0;
            // receive requests in while true until remote client closes the socket.
            int receivedLength;
            while (true)
            {
                try
                {
                    // Receive request
                    byte[] ReceivedRequest = new byte[1024];
                    receivedLength = client_socket.Receive(ReceivedRequest);
                    string data = Encoding.ASCII.GetString(ReceivedRequest);
                    // break the while loop if receivedLen==0
                    if (receivedLength == 0)
                    {
                        break;
                    }
                    // Create a Request object using received request string
                    Request requestObj = new Request(data);
                    // Call HandleRequest Method that returns the response
                    returnedResponse = HandleRequest(requestObj);
                    // Send Response back to client
                    string resp = returnedResponse.ResponseString;
                    client_socket.Send(Encoding.ASCII.GetBytes(resp));
                }
                catch (Exception ex)
                {
                    // log exception using Logger class
                    Logger.LogException(ex);
                }
            }

            // close client socket
            client_socket.Close();
        }

        Response HandleRequest(Request request)
        {
            string content;
            int contentLength;
            StatusCode code;
            try
            {
                throw new Exception();
                //check for bad request 
                bool validReq = request.ParseRequest();
                if (validReq)
                {
                    //map the relativeURI in request to get the physical path of the resource.
                    string PageName;
                    string PhysicalPath;
                    // Retrieve the relative URI substring
                    string[] relativeURi = request.relativeURI.Split('/');
                    PageName = relativeURi[1];
                    PhysicalPath = Configuration.RootPath + '\\' + PageName;
                    //check for redirect
                    for (int i = 0; i < Configuration.RedirectionRules.Count; i++)
                    {
                        if (Configuration.RedirectionRules.Keys.ElementAt(i).ToString() == PageName)
                        {
                            code = StatusCode.Redirect;
                            PageName = Configuration.RedirectionRules.Values.ElementAt(i).ToString();
                            PhysicalPath = GetRedirectionPagePathIFExist(PageName);
                            string Location = "http://localhost:1000/" + PageName;
 
                            content =LoadDefaultPage(Configuration.RedirectionDefaultPageName);
                            contentLength = content.Length;
                            if (request.Method == RequestMethod.HEAD)
                                content = "";

                            returnedResponse = new Response(code, "text/html", content, contentLength, Location);
                            return returnedResponse;
                        }
                    }
                    //check file exists
                    if (!File.Exists(PhysicalPath))
                    {
                        content = LoadDefaultPage(Configuration.NotFoundDefaultPageName);
                        contentLength = content.Length;
                        code = StatusCode.NotFound;
                        PhysicalPath = Configuration.RootPath + '\\' + Configuration.NotFoundDefaultPageName;
                    }
                    //read the physical file
                    else
                    {                     
                        content = LoadDefaultPage(PageName);
                        code = StatusCode.OK;
                    }

                    contentLength = content.Length;
                    if (request.Method == RequestMethod.HEAD)
                        content = "";

                    // Create OK response
                    returnedResponse = new Response(code, "text/html", content, contentLength, PhysicalPath);

                    return returnedResponse;
                }

                code = StatusCode.BadRequest;

                content = LoadDefaultPage(Configuration.BadRequestDefaultPageName);
                contentLength = content.Length;
                if (request.Method == RequestMethod.HEAD)
                    content = "";

                returnedResponse = new Response(code, "text/html", content, contentLength, " ");
                return returnedResponse;
            }
            catch (Exception ex)
            {
                // log exception using Logger class
                Logger.LogException(ex);
                // in case of exception, return Internal Server Error. 
                code = StatusCode.InternalServerError;

                content = LoadDefaultPage(Configuration.InternalErrorDefaultPageName);
                contentLength = content.Length;
                if (request.Method == RequestMethod.HEAD)
                    content = "";

                string physicalPath = Configuration.RootPath + '\\' + Configuration.InternalErrorDefaultPageName;
                returnedResponse = new Response(code, "text/html", content, contentLength, physicalPath);

                return returnedResponse;
            }
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
            for (int i = 0; i < Configuration.RedirectionRules.Count; i++)
            {
                if ('/' + Configuration.RedirectionRules.Keys.ElementAt(i).ToString() == relativePath)
                {
                    string physicalPath = Configuration.RootPath + '\\' + Configuration.RedirectionRules.Keys.ElementAt(i).ToString();
                    return physicalPath;
                }
            }
            return string.Empty;
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            // check if filepath not exist log exception using Logger class and return empty string
             try
             {
                if (File.Exists(filePath))
                {
                    // else read file and return its content
                    string Content = File.ReadAllText(filePath);
                    return Content;
                }
             }
            catch(Exception ex)
             {
                Logger.LogException(ex);
             }
            return string.Empty;
        }

        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                Configuration.RedirectionRules = new Dictionary<string, string>();

                // using the filepath paramter read the redirection rules from file 
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                StreamReader sr = new StreamReader(fs);
                // then fill Configuration.RedirectionRules dictionary 
                while (sr.Peek() != -1)
                {
                    string line = sr.ReadLine();
                    string[] data;
                    string[] separatingStrings = { ", " };
                    data = line.Split(separatingStrings, System.StringSplitOptions.None);
                    if (data[0] == "")
                    {
                        break;
                    }
                    Configuration.RedirectionRules.Add(data[0], data[1]);
                }

                fs.Close();
            }
            catch (Exception ex)
            {
                // log exception using Logger class
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }
    }
}
