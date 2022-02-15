using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Call CreateRedirectionRulesFile() function to create the rules of redirection 
            CreateRedirectionRulesFile();
            //Start server
            string filePath = @"RedirectionRules.txt";
            // 1) Make server object on port 1000
            Server HTTPServer = new Server(1000, filePath);
            // 2) Start Server
            HTTPServer.StartServer();
        }

        static void CreateRedirectionRulesFile()
        {
            // Create file named redirectionRules.txt
            // each line in the file specify a redirection rule
            // example: "aboutus.html,aboutus2.html"
            // means that when making request to aboustus.html,, it redirects me to aboutus2
            string path = @"RedirectionRules.txt";
            if (File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter fw = File.CreateText(path))
                {
                    fw.WriteLine(@"aboutus.html, aboutus2.html");
                    fw.WriteLine(@"random.html, main.html");
                    fw.WriteLine(@"oldpage.html, aboutus.html");
                    fw.WriteLine(@"far.html, oldpage.html");
                }
            }
        }
    }
}
