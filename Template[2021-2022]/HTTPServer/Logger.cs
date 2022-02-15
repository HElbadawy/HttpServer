using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Logger
    {
        public static void LogException(Exception ex)
        {
            // Create log file named log.txt to log exception details in it
            string path = @"log2.txt";
            
                // Create a file to write to.
                using (StreamWriter fw = File.AppendText("log2.txt"))
                {
                    //Datetime:
                    //message:
                    // for each exception write its details associated with datetime 
                    fw.WriteLine("Date Time: " + DateTime.Now.ToString());
                    fw.WriteLine("Message: " + ex.Message);
                    fw.Close();
                }
        } 
        
    }
}
