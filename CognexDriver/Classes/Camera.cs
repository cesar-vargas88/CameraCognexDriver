using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CognexDriver
{
    class Camera
    {
        private string  localIP;
        private int     localPort;
        private string  cameraIP;
        private int     cameraPort;
        private int     timeOut;
        private string  user;
        private string  password;
        private string  command;
        private string  data;
        private bool    working;
        private string  serverResponse;

        TCPClient tcpClient;
        TCPServer tcpServer;

        public Camera(string LocalIP, int LocalPort, string CameraIP, int CameraPort, int TimeOut)
        {
            localIP     = LocalIP;
            localPort   = LocalPort;
            cameraIP    = CameraIP;
            cameraPort  = CameraPort;
            timeOut     = TimeOut;

            user            = "";
            password        = "";
            command         = "";
            data            = "";
            working         = false;
            serverResponse  = "";
        }

        public string Trigger(string User, string Password, string Command)
        {
            if (!working)
            {
                try
                {
                    working = true;

                    Ping pingSender = new Ping();
                    PingReply reply = pingSender.Send(cameraIP, 400);

                    if (reply.Status == IPStatus.Success)
                    {
                        user     = User;
                        password = Password;
                        command  = Command;

                        serverResponse = "NO_DATA";

                        Thread threadTcpServer = new Thread(new ThreadStart(thTcpServer));
                        threadTcpServer.IsBackground = true;
                        threadTcpServer.Start();

                        Thread.Sleep(1);

                        Thread threadTcpClient = new Thread(new ThreadStart(thTcpClient));
                        threadTcpClient.IsBackground = true;
                        threadTcpClient.Start();
                        Thread.Sleep(1);

                        int TimeOut = 0;

                        while (serverResponse == "NO_DATA" && TimeOut < timeOut)
                        {
                            TimeOut++;
                            Thread.Sleep(1);
                        }

                        working = false;

                        return serverResponse;
                    } 
                    else
                    {
                        working = false;

                        return "NO_CONNECTION";
                    }
                }
                catch (Exception e)
                {
                    working = false;
                    return "PROCESS_FAIL";
                }
            }
            else
                return "TASK_RUNNING";
        }

        private void thTcpClient()
        {
            try
            {
                Task t = Task.Run(() =>
                {
                    try
                    {
                        tcpClient = new TCPClient(cameraIP, cameraPort, timeOut);
                        tcpClient.Connect();
                        Thread.Sleep(5);
                        data = tcpClient.ReceiveMessage();

                        if (data.Contains("User: "))
                        {
                            tcpClient.SendMessage(user);
                            data = tcpClient.ReceiveMessage();

                            if (data.Contains("Password: "))
                            {
                                tcpClient.SendMessage(password);
                                data = tcpClient.ReceiveMessage();

                                if (data.Contains("User Logged In"))
                                {
                                    tcpClient.SendMessage(command);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            tcpClient.Disconnect();
                        }
                        catch (Exception ee)
                        {
                        }
                    }
                });

                TimeSpan ts = TimeSpan.FromMilliseconds(timeOut);

                tcpClient.Disconnect();
            }
            catch (Exception e)
            {
                try
                {
                    tcpClient.Disconnect();
                }
                catch (Exception ee)
                {
                }
            }
        }
        private void thTcpServer()
        {
            try
            {
                Task t = Task.Run(() => 
                {
                    try
                    {
                        tcpServer = new TCPServer(localIP, localPort);
                        tcpServer.Initialize();
                        tcpServer.CreateConnection();
                        serverResponse = tcpServer.Listenning();
                    }
                    catch(Exception e)
                    {
                        try
                        {
                            tcpServer.DestroyConnection();
                        }
                        catch (Exception ee)
                        {
                        }
                    }
                });

                TimeSpan ts = TimeSpan.FromMilliseconds(timeOut);

                if (!t.Wait(ts))
                    serverResponse = "NO_RESPONSE";

                tcpServer.DestroyConnection();
            }
            catch (Exception e)
            {
                try
                {
                   tcpServer.DestroyConnection();
                }
                catch (Exception ee)
                {
                }
            }
        }
    }
}
