using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace Consumer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
 

        public MainWindow()
        {
            InitializeComponent();

           

        }
        public string Text { get; private set; }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Integration.WindowsFormsHost host =
       new System.Windows.Forms.Integration.WindowsFormsHost();

            // Create the MaskedTextBox control.
            PictureBox mtbDate = new PictureBox();

            // Assign the MaskedTextBox control as the host control's child.
            host.Child = mtbDate;

            // Add the interop host control to the Grid
            // control's collection of child controls.
            mtbDate.Width = 1000;
            mtbDate.Height = 700;
            this.Top.Children.Add(host);

            var port = int.Parse(ConfigurationManager.AppSettings.Get("port"));
            var client = new UdpClient(port);

            while (true)
            {
                var data = await client.ReceiveAsync();
                using (var ms = new MemoryStream(data.Buffer))
                {
                    mtbDate.Image = new Bitmap(ms);
                }
                Text = $"Bytes received: {data.Buffer.Length * sizeof(byte)}";
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            System.Windows.MessageBox.Show(string.Join("\n", host.AddressList.
                Where(i => i.AddressFamily == AddressFamily.InterNetwork)
                .Select(i => i.ToString())));
        }

    }
}

