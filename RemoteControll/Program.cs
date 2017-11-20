using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace RemoteControll
{
    class Program
    {
        static TcpClient client = new TcpClient();
        static Stream sr = null;

        static void WaitForIt()
        {
            BinaryReader reader = new BinaryReader(sr);
            while (true)
            {
                if (sr != null)
                { 
                    int typ = reader.ReadInt32();
                    if (typ == 0x12)
                    {
                        int x = reader.ReadInt32();
                        int y = reader.ReadInt32();
                        Cursor.Position = ClientToScreen(ClientSize, Screen.PrimaryScreen.WorkingArea.Size, new Point(x, y));
                        Log2.Info("Cursor position has been updated..");
                    }
                }
            }
        }

        static Point ClientToScreen(Size clientArea, Size screenArea, Point clientPoint)
        {
            int hRatio = clientPoint.Y / clientArea.Height;
            int wRatio = clientPoint.X / clientArea.Width;

            Point pScreen = new Point(screenArea.Height * hRatio, screenArea.Width * wRatio);
            return pScreen;
        }

        static void SendPicture()
        {
            while (true)
            {
                byte[] data;
                using (Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                            Screen.PrimaryScreen.Bounds.Y,
                            0, 0,
                            bmp.Size,
                            CopyPixelOperation.SourceCopy);
                    }
                    using (MemoryStream memsr = new MemoryStream())
                    {
                        ((Image)bmp).Save(memsr, System.Drawing.Imaging.ImageFormat.Jpeg);
                        data = memsr.ToArray();
                    }
                }
                using (MemoryStream memsr2 = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(memsr2))
                    {
                        writer.Write(0xb24ac);
                        writer.Write(data.Length);
                        writer.Write(data);
                        writer.Flush();
                    }
                    byte[] final = memsr2.ToArray();
                    
                    sr.Write(final, 0, final.Length);
                }
                Thread.Sleep(10);
            }
        }

        static Size ClientSize;

        static void UpdateClientSize()
        {
            BinaryReader reader = new BinaryReader(sr);
            int h = reader.ReadInt32();
            int w = reader.ReadInt32();

            ClientSize = new Size(w, h);
        }

        static void Main(string[] args)
        {
            Log2.Initialize("Log2.txt", LogLevel.All, false);
            string ip = Console.ReadLine();
            client.Connect(IPAddress.Parse(ip), 57852);
            sr = client.GetStream();
            Log2.Info("Updating client size...");
            UpdateClientSize();
            new Thread(WaitForIt).Start();
            new Thread(SendPicture).Start();
            Log2.Debug("Systems up!");
            while(true)
            {
                Log2.WriteAway();
            }
        }
    }
}
