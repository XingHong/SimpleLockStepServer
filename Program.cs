using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LockStep
{
    class Program
    {
		static Hashtable m_sockets;
		static int m_palyerId = 1;
		static void Main(string[] args)
		{
			m_sockets = new Hashtable();

			Socket serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			IPAddress ip = IPAddress.Any;
			IPEndPoint point = new IPEndPoint(ip, 2333);
			//socket绑定监听地址
			serverSocket.Bind(point);
			Console.WriteLine("Listen Success:" + ip);
			//设置同时连接个数
			serverSocket.Listen(10);

			//利用线程后台执行监听,否则程序会假死
			Thread thread = new Thread(Listen);
			thread.IsBackground = true;
			thread.Start(serverSocket);

			Console.Read();
		}

		/// <summary>
		/// 监听连接
		/// </summary>
		/// <param name="o"></param>
		static void Listen(object o)
		{
			var serverSocket = o as Socket;
			while (true)
			{
				//等待连接并且创建一个负责通讯的socket
				var send = serverSocket.Accept();
				//获取链接的IP地址
				var sendIpoint = send.RemoteEndPoint.ToString();
				Console.WriteLine($"{sendIpoint}Connection");
				//开启一个新线程不停接收消息
				Thread thread = new Thread(Recive);
				thread.IsBackground = true;
				thread.Start(send);

				//给对应的玩家编号
				m_sockets.Add(serverSocket, m_palyerId++);
			}
		}

		/// <summary>
		/// 接收消息
		/// </summary>
		/// <param name="o"></param>
		static void Recive(object o)
		{
			var send = o as Socket;
			while (true)
			{
				//获取发送过来的消息容器
				byte[] buffer = new byte[2048];
				var effective = send.Receive(buffer);

				Console.WriteLine("effective: " + effective);

				//表示客户端关闭，要退出循环
				if (effective == 0)
				{
					break;
				}
				if (buffer[0] == 1)			//1代表收到操作消息
				{
					byte[] tmp = new byte[4];
					Array.Copy(buffer, 1, tmp, 0, 4);
					int frameId = BitConverter.ToInt32(tmp, 0);
					Array.Copy(buffer, 5, tmp, 0, 4);
					Console.WriteLine("frameId: " + frameId);
					int op = BitConverter.ToInt32(tmp, 0);
					Console.WriteLine("op: " + op);
					send.Send(buffer, effective, SocketFlags.None);
				}
                Console.WriteLine("send info to client");
            }
		}
	}
}
