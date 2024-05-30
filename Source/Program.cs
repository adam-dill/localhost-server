using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace LocalHostServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set up the server
            string baseDirectory = Directory.GetCurrentDirectory();
            string[] entryFiles = { "index.html", "start.html", "default.html" }; // Add more entry file names if needed
            string uri = "http://localhost:8080/";

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(uri);
            listener.Start();

            Console.WriteLine("Server started at " + uri);

            // Open the default web browser to the server URL
            OpenBrowser(uri);

            Console.WriteLine("Press 'q' to stop the server...");

            // Handle requests in a separate thread
            ThreadPool.QueueUserWorkItem((state) =>
            {
                while (listener.IsListening)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    // Try serving each entry file until one is found
                    bool fileServed = false;
                    foreach (string entryFile in entryFiles)
                    {
                        string filePath = Path.Combine(baseDirectory, entryFile);
                        if (File.Exists(filePath))
                        {
                            ServeFile(response, filePath);
                            fileServed = true;
                            break;
                        }
                    }

                    if (!fileServed)
                    {
                        // If none of the entry files exist, respond with a 404 Not Found error
                        response.StatusCode = 404;
                        response.Close();
                    }
                }
            });

            // Wait for 'q' key press to stop the server
            while (Console.ReadKey(true).KeyChar != 'q') { }

            // Stop the server
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }

        // Function to serve a file to the client
        static void ServeFile(HttpListenerResponse response, string filePath)
        {
            try
            {
                // Read the file content
                byte[] fileBytes = File.ReadAllBytes(filePath);

                // Set the response content type
                response.ContentType = "text/html";

                // Write the file content to the response stream
                response.ContentLength64 = fileBytes.Length;
                response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
            }
            catch (Exception ex)
            {
                // If an error occurs, respond with a 500 Internal Server Error
                Console.WriteLine("Error serving file: " + ex.Message);
                response.StatusCode = 500;
            }
            finally
            {
                // Close the response stream
                response.Close();
            }
        }

        // Function to open the default web browser with the specified URL
        static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening browser: " + ex.Message);
            }
        }
    }
}
