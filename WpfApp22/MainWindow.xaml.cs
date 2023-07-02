using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp22
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        CancellationTokenSource? cts;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "(*.txt)|*.txt";
            fileDialog.ShowDialog();
            filetxtbx.Text= fileDialog.FileName??default;
 
        }


        public void Encryption(string file,string key)
        {
            cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            using (FileStream fs=new FileStream(file,FileMode.Open,FileAccess.ReadWrite))
            {
                int len = 1;
                var buffer=new byte[len];
                Dispatcher.Invoke(() =>
                {
                    progress.Maximum = fs.Length;
                });
                for (int i = 0; i < fs.Length; i++)
                {

                    if (cts.IsCancellationRequested)
                    {
                        for (int k = i-1; k >=0; k--)
                        {
                            fs.Position = k;
                            fs.Read(buffer, 0, buffer.Length);
                            fs.Position -= len;
                            byte.TryParse((buffer[0] ^ key[k % key.Length]).ToString(), out buffer[0]);
                            fs.Write(buffer, 0, len);
                            Dispatcher.Invoke(() =>
                            {
                                progress.Value = k;
                            });
                            Thread.Sleep(30);
                        }
                        MessageBox.Show("Encrypt Cancelled");
                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        progress.Value = i + 1;
                    });

                    fs.Read(buffer, 0, buffer.Length);
                    fs.Position-=len;
                    byte.TryParse((buffer[0] ^ key[i % key.Length]).ToString(), out buffer[0]);
                    fs.Write(buffer, 0, len);
                    Thread.Sleep(30);
                }
                MessageBox.Show("Encrypt Process Succeed");
            }
        }



        private void startbtn_Click(object sender, RoutedEventArgs e)
        {
            string file = filetxtbx.Text;
            string key = passwordtxtbx.Text;
            if (!File.Exists(file))
            {
                MessageBox.Show("File path was not found");
                return;
            }
            if (key == "")
            {
                MessageBox.Show("Enter key");
                return;
            }
            if (cts!=null) return;
            ThreadPool.QueueUserWorkItem(f =>
            {
                Encryption(file, key);
                Dispatcher.Invoke(() =>
                {
                    cts = null;
                    filetxtbx.Text = default;
                    passwordtxtbx.Text = default;
                    progress.Value = 0;
                });
            });
        }

        private void Cancelbtn_Click(object sender, RoutedEventArgs e)
        {
            if(cts?.IsCancellationRequested==false)cts.Cancel();
        }
    }


}

