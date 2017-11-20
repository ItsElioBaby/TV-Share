using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Drawing;

namespace BunnyDesktop
{
    public class DesktopWaiter
    {
        //Socket server_socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

       public Point last_mouse_position = new Point(0, 0);

       List<Point> lpMouse = new List<Point>();

       public Point[] MousePostition { get { return lpMouse.ToArray(); } }

        public async void WaitForInput(Action<Point> callback)
        {
            Point cPos = Cursor.Position;
            last_mouse_position = await Task<Point>.Run<Point>(delegate
            {
                while(true)
                {
                    if (last_mouse_position != cPos)
                    {
                        callback(cPos);
                        return cPos;
                    }
                }
            });
            WaitForInput(callback);
        }
    }
}
