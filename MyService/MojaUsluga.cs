using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace MyService
{
    public class MojaUsluga : ServiceBase
    {
        public const string NazwaUslugi = "MojSerwis";
        private static ServerObject Server;
        private static Thread ListenerThread;

        public MojaUsluga()
        {
            Server = new ServerObject();
            ListenerThread = new Thread(new ThreadStart(Server.Listen));
        }

        protected override void OnStart(string[] args)
        {
            ListenerThread.Start();
        }

        protected override void OnStop()
        {
            Server.Disconnect();
        }

        public class ClientObject
        {
            protected internal string Id { get; private set; }
            protected internal NetworkStream Stream { get; private set; }
            protected internal string UserName;
            protected internal HashSet<string> roomSet = new HashSet<string>();
            TcpClient Client = null;
            ServerObject Server = null;

            public ClientObject(TcpClient client, ServerObject server)
            {
                Id = Guid.NewGuid().ToString();
                Client = client;
                Server = server;
                server.AddConnection(this);
            }

            public void UserInit()
            {
                try
                {
                    Stream = Client.GetStream();
                    string Message = GetMessage();
                    UserName = Message;

                    DateTime time = DateTime.Now;
                    Message = $"[{time.ToString("HH:mm")}] Server: {Message} joined chat\n";
                    Server.BroadcastMessage(Message, this.Id);
                    Console.WriteLine(Message);
                    while (true)
                    {
                        try
                        {
                            Message = GetMessage();
                            if (Message.Equals("UserDisconnect"))
                            {
                                Server.DeleteConnetion(this.Id);
                                DateTime time2 = DateTime.Now;
                                Message = $"[{time2.ToString("HH:mm")}] Server: {UserName} left chat...\n";
                                Console.WriteLine(Message);
                                Server.BroadcastMessage(Message, this.Id);
                                Close();
                                break;
                            }
                            else if (Message.StartsWith("pv"))
                            {
                                string[] parts = Message.Split(' ');

                                var string1 = parts[0];
                                var string2 = parts[1];
                                var theRest = Message
                                        .Replace(string1, "")
                                        .Replace(string2, "");
                                Server.SendPrivateMessage(theRest, string2, this.Id, this.UserName);
                            }
                            else if (Message.StartsWith("joinRoom"))
                            {
                                string[] parts = Message.Split(' ');

                                var string1 = parts[0];
                                var string2 = parts[1];
                                JoinRoom(string2);
                                Server.JoinRoomInfo(Id);
                            }
                            else if (Message.StartsWith("leaveRoom"))
                            {
                                string[] parts = Message.Split(' ');

                                var string1 = parts[0];
                                var string2 = parts[1];
                                DateTime time2 = DateTime.Now;
                                if (LeaveRoom(string2))
                                {
                                    Server.LeaveRoomInfo(Id, $"[{time2.ToString("HH:mm")}] Server: opuściłeś room...\n");
                                }
                                else
                                {
                                    Server.LeaveRoomInfo(Id, $"[{time2.ToString("HH:mm")}] Server: nie jesteś w tym roomie...\n");
                                }
                            }
                            else if (Message.StartsWith("room"))
                            {
                                string[] parts = Message.Split(' ');

                                var string1 = parts[0];
                                var string2 = parts[1];
                                var theRest = Message
                                        .Replace(string1, "")
                                        .Replace(string2, "");
                                Server.RoomMessage(theRest, string2, this.Id);
                            }
                            else
                            {
                                Console.WriteLine(Message);
                                Server.BroadcastMessage(Message, this.Id);
                            }

                        }
                        catch
                        {
                            //Message = String.Format("{0}: leave...", UserName);
                            ////Console.WriteLine(Message);
                            //Server.BroadcastMessage(Message, this.Id);
                            //break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    Server.DeleteConnetion(this.Id);
                    Close();
                }
            }

            public string GetMessage()
            {
                byte[] buffer = new byte[64];
                StringBuilder Builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = Stream.Read(buffer, 0, buffer.Length);
                    Builder.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
                } while (Stream.DataAvailable);

                return Builder.ToString(); ;
            }

            private void JoinRoom(string roomName)
            {
                roomSet.Add(roomName);
            }

            private bool LeaveRoom(string roomName)
            {
                if (IsMember(roomName))
                {
                    roomSet.Remove(roomName);
                    return true;
                }
                return false;

            }

            protected internal bool IsMember(string roomName)
            {
                return roomSet.Contains(roomName);
            }

            protected internal void Close()
            {
                if (Stream != null)
                {
                    Stream.Close();
                }
                if (Client != null)
                {
                    Client.Close();
                }
            }
        }

        public class ServerObject
        {
            private static TcpListener Listener;
            private List<ClientObject> Clients = new List<ClientObject>();

            protected internal void AddConnection(ClientObject clientObject)
            {
                Clients.Add(clientObject);
            }

            protected internal void DeleteConnetion(string id)
            {
                ClientObject Client = Clients.FirstOrDefault(x => x.Id == id);
                if (Client != null)
                {
                    Clients.Remove(Client);
                }
            }

            public void Listen()
            {
                try
                {
                    Listener = new TcpListener(IPAddress.Any, 8080);
                    Listener.Start();
                    Console.WriteLine("Server start. Waiting for connections...");

                    while (true)
                    {
                        TcpClient Client = Listener.AcceptTcpClient();

                        ClientObject clientObject = new ClientObject(Client, this);
                        Thread ClientThread = new Thread(new ThreadStart(clientObject.UserInit));
                        ClientThread.Start();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Disconnect();
                }

            }

            protected internal void BroadcastMessage(string message, string id)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].Id != id)
                    {
                        Clients[i].Stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            protected internal void SendPrivateMessage(string message, string name, string id, string userName)
            {
                DateTime time = DateTime.Now;
                message = $"[{time.ToString("HH:mm")}] Pv from {userName}:{message}\n";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                bool sucess = false;
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].UserName == name)
                    {
                        Clients[i].Stream.Write(buffer, 0, buffer.Length);
                        sucess = true;
                    }
                }
                if (!sucess)
                {
                    buffer = Encoding.UTF8.GetBytes($"[{ time.ToString("HH:mm")}] Server: Nie znaleziono usera\n");
                    for (int i = 0; i < Clients.Count; i++)
                    {
                        if (Clients[i].Id == id)
                        {
                            Clients[i].Stream.Write(buffer, 0, buffer.Length);
                        }
                    }
                }
            }

            protected internal void RoomMessage(string message, string roomName, string id)
            {
                DateTime time = DateTime.Now;
                message = $"[{time.ToString("HH:mm")}] Pv from room {roomName}:{message}\n";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].IsMember(roomName) && Clients[i].Id != id)
                    {
                        Clients[i].Stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            protected internal void LeaveRoomInfo(string id, string message)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].Id == id)
                    {
                        Clients[i].Stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            protected internal void JoinRoomInfo(string id)
            {
                string message = "dołączyłeś do pokoju";
                DateTime time = DateTime.Now;
                message = $"[{time.ToString("HH:mm")}] Server : {message}\n";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].Id == id)
                    {
                        Clients[i].Stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }

            public void Disconnect()
            {
                Listener.Stop();

                for (int i = 0; i < Clients.Count; i++)
                {
                    Clients[i].Close();
                }
                //Environment.Exit(0);
            }
        }

    }
}
