using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace BunnyDesktop
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        TcpListener listener = new TcpListener(IPAddress.Any, 57852);

        NetworkStream nStream = null;

        DateTime lastSnapshot = DateTime.Now;

        public void OnTcpClientConnected(IAsyncResult state)
        {
            TcpListener listener = (TcpListener)state.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(state);

            nStream = client.GetStream();
            using (BinaryWriter bWriter = new BinaryWriter(nStream))
            {
                bWriter.Write(Screen.PrimaryScreen.WorkingArea.Size.Height);
                bWriter.Write(Screen.PrimaryScreen.WorkingArea.Size.Width);
                bWriter.Flush();
            }
            new Thread(WaitForPacket).Start();
        }

        private void WaitForPacket()
        {
            BinaryReader reader = new BinaryReader(nStream);
            while(true)
            {
                if (nStream != null)
                {
                    try
                    {
                        if(DateTime.Now.Subtract(lastSnapshot).Minutes > 3)
                        {
                            MessageBox.Show("Connection Timed Out!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        int header = reader.ReadInt32();
                        if (header == 0xb24ac)  // Is image ?
                        {
                            int size = reader.ReadInt32();
                            byte[] image = reader.ReadBytes(size);
                            MemoryStream memsr = new MemoryStream(image);
                            pictureBox1.Invoke(new Action(delegate
                            {
                                pictureBox1.Image = Image.FromStream(memsr);
                            }));
                            lastSnapshot = DateTime.Now;
                        }
                    }
                    catch (Exception) {
                        MessageBox.Show("Connection Timed Out! #2", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }


        private void OnPictureReceived(object pic)
        {
            pictureBox1.Image = (Bitmap)pic;
        }
        

        public void Start()
        {
            listener.Start();
            listener.BeginAcceptTcpClient(OnTcpClientConnected, listener);
        }

        DesktopWaiter dw = new DesktopWaiter();

        private void Form1_Load(object sender, EventArgs e)
        { 
            dw.WaitForInput(OnPosChanged);
            Start();
        }

        private void OnPosChanged(Point nPoint)
        {
                if(nStream != null)
                {
                    using(MemoryStream memsr = new MemoryStream())
                    {
                        using(BinaryWriter writer = new BinaryWriter(memsr))
                        {
                            writer.Write(0x12);
                            writer.Write(nPoint.X);
                            writer.Write(nPoint.Y);
                            writer.Flush();
                        }
                        byte[] final = memsr.ToArray();
                        nStream.Write(final, 0, final.Length);
                    }
                }
        }

    } 
}
