using ChatClient.Model;
using DataLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string UserName = "guest";
        private readonly string host = "127.0.0.1";
        private const int port = 8080;
        private TcpClient Client;
        private NetworkStream Stream;
        private IDataAccess _dataAccess;
        private List<UserModel> user;
        private List<UserModel> users;
        private Window myWindow;
        private Thread RecieveThread = null;
        private Thread UpdateViewList = null;
        private bool refreshList = true;


        public MainWindow()
        {
            _dataAccess = new DataAccess();
            InitializeComponent();
            HostTextBox.Text = host;
            DisconnectButton.IsEnabled = false;
            SendMessageButton.IsEnabled = false;
            PortTextBox.Text = port.ToString();
            ChatTexBox.Text = "Welcome to my chat, click connect to start more action\n";

        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SendMessage();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            myWindow = new Window();
            myWindow.Title = "Wybierz";

            myWindow.Width = 450;
            myWindow.Height = 300;

            myWindow.MaxHeight = 500;
            myWindow.MinHeight = 250;

            myWindow.MaxWidth = 500;
            myWindow.MinWidth = 250;

            Grid myGrid = new Grid();
            myGrid.Width = 450;
            myGrid.Height = 200;

            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            myGrid.ColumnDefinitions.Add(colDef1);
            myGrid.ColumnDefinitions.Add(colDef2);
            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();
            myGrid.RowDefinitions.Add(rowDef1);
            myGrid.RowDefinitions.Add(rowDef2);

            Button SignIn = new Button();
            SignIn.Content = "Zaloguj";
            Grid.SetColumnSpan(SignIn, 3);
            Grid.SetRow(SignIn, 0);
            myGrid.Children.Add(SignIn);

            Button SignUp = new Button();
            SignUp.Content = "Zarejestruj";
            Grid.SetColumnSpan(SignUp, 3);
            Grid.SetRow(SignUp, 1);
            myGrid.Children.Add(SignUp);

            myWindow.Content = myGrid;
            myWindow.Show();

            SignIn.Click += this.SignInBtn_Click;
            SignUp.Click += this.SignUpBtn_Click;

        }

        T Cast<T>(object obj, T type) { return (T)obj; }
        async private void SignInBtn_Click(Object sender, EventArgs e)
        {
            object userData = new LoginInput("LogIn").ShowDialog();
            var obj = Cast(userData, new { Login = "", Password = "" });
            string sql = $"select * from myusers where NickName like @NickName";
            string connString = GetConnectionString();
            user = await _dataAccess.LoadData<UserModel, dynamic>(sql, new {NickName = obj.Login}, connString);
            
            if (user.Count == 0)
            {
                ChatTexBox.Text += "Logowanie się nie powiodło ...\n";
            }
            else
            {
                string savedPasswordHash = user[0].PasswordHash;

                if(IsPasswordOK(savedPasswordHash, obj.Password))
                {
                    UserName = user[0].NickName;
                    UserNameTextBox.Text = UserName;
                    sql = $"update myusers set IsOnline = @IsOnline where NickName = @NickName";
                    await _dataAccess.SaveData(sql, new { IsOnline = 1, NickName = UserName }, connString);
                    ChatTexBox.Text = $"Witaj {UserName} :)\n";
                    ChatTexBox.Text += "Możesz wysłać prywatną wiadomość :\nwpisz /pomoc, aby poznać możliwości czatu\n";
                    UpdateViewList = new Thread(new ThreadStart(UpdateLV));
                    UpdateViewList.Start();
                    ConnectToServer();
                    GetUsersOnline();
                }
                else
                {
                    ChatTexBox.Text += "Logowanie się nie powiodło ...\n";
                }
            }
            myWindow.Close();
        }

        private bool IsPasswordOK(string hashPassowrd, string password)
        {
            byte[] hashBytes = Convert.FromBase64String(hashPassowrd);
    
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
        
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;

            return true;
        }

        async private void UpdateLV()
        {
            while(refreshList)
            {
                Thread.Sleep(3000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UsersOnlineList.ItemsSource = null;
                    UsersOnlineList.Items.Clear();
                });
              
                string sql = "select * from myusers";
                string connString = GetConnectionString();
                users = await _dataAccess.LoadData<UserModel, dynamic>(sql, new { }, connString);
                foreach (var u in users)
                {
                    if (u.IsOnline)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UsersOnlineList.Items.Add(u.NickName);
                        });
                    } 
                }
            }
        }

        async private void SignUpBtn_Click(Object sender, EventArgs e)
        {
            object userData = new RegisterInput("Register").ShowDialog();
            var obj = Cast(userData, new { Login = "", Password = "" });

            string PasswordHash =  HashPassword(obj.Password);

            string sql = $"insert into myusers (NickName,PasswordHash, IsOnline) values (@NickName, @PasswordHash, @IsOnline)";
            string connString = GetConnectionString();
            await _dataAccess.SaveData(sql, new { NickName = obj.Login, PasswordHash = PasswordHash, IsOnline = 1 }, connString);

            UserName = obj.Login;
            UserNameTextBox.Text = UserName;
            ChatTexBox.Text = $"Witaj {UserName} :)\n";
            ChatTexBox.Text += "Możesz wysłać prywatną wiadomość:\nwpisz /pomoc, aby poznać możliwości czatu\n";
            ConnectToServer();
            GetUsersOnline();
            UpdateViewList = new Thread(new ThreadStart(UpdateLV));
            UpdateViewList.Start();

            myWindow.Close();
        }

        private string HashPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string savedPasswordHash = Convert.ToBase64String(hashBytes);

            return savedPasswordHash;
        }

        private void ConnectToServer()
        {
            try
            {
                Client = new TcpClient();
                Client.Connect(IPAddress.Parse(host), port);
                Stream = Client.GetStream();

                string Message = UserName;
                UserNameTextBox.Text = UserName;
                byte[] buffer = Encoding.UTF8.GetBytes(Message);
                Stream.Write(buffer, 0, buffer.Length);

                RecieveThread = new Thread(new ThreadStart(RecieveMessage));
                RecieveThread.Start();

                SendMessageButton.IsEnabled = true;
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
            }
            catch(SocketException)
            {
                ChatTexBox.Text += "Server jest wyłączony ...\n";
                SendMessageButton.IsEnabled = false;
            }
        }

        async private void GetUsersOnline()
        {
            string sql = "select * from myusers";
            string connString = GetConnectionString();
            users = await _dataAccess.LoadData<UserModel, dynamic>(sql, new { }, connString);
            foreach (var u in users)
            {
                if (u.IsOnline)
                    UsersOnlineList.Items.Add(u.NickName);
            }

            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
        }

        private string GetConnectionString()
        {
            var settings = ConfigurationManager.ConnectionStrings;

            return settings[0].ConnectionString;
        }

        async private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            SendMessageButton.IsEnabled = false;
            refreshList = false;

            if (UpdateViewList != null)
            {
                UpdateViewList.Abort();
            }
            if (RecieveThread != null)
            {
                RecieveThread.Abort();
            }

            UsersOnlineList.ItemsSource = null;
            UsersOnlineList.Items.Clear();

            string connString = GetConnectionString();
            string sql = $"update myusers set IsOnline = @IsOnline where NickName = @NickName";
            await _dataAccess.SaveData(sql, new { IsOnline = 0, NickName = UserName }, connString);

            Disconnect();
            ChatTexBox.Text += "Zostałeś rozłączony ...\n";
        }

        private void ZamykanieOkna(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DisconnectButton.IsEnabled)
            {
                ChatTexBox.Text += "You have to disconnect first !\n";
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
           DateTime time = DateTime.Now;
           string text = sendTextBox.Text;
           if(!String.IsNullOrEmpty(text))
           {
                string Message;
                if (text.StartsWith("pv") || text.StartsWith("joinRoom") || text.StartsWith("leaveRoom") || text.StartsWith("room"))
                {
                    Message = $"[{time.ToString("HH:mm")}] {UserName}: {text}\n";
                    ChatTexBox.Text += Message;
                    Message = text;
                }
                else if (text.StartsWith("/pomoc"))
                {
                    Message = "Komendy : \npv nickName treść wiadomości - wyślij prywatną widomość\njoinRoom roomName - dołącz do pokoju\nroom roomName treść wiadomości - wyślij wiadomość do ludzi w pokoju\nleaveRoom roomName - opuść pokój\n";
                    ChatTexBox.Text += Message;
                    Message = null;
                }
                else
                {
                    Message = $"[{time.ToString("HH:mm")}] {UserName}: {text}\n";
                    ChatTexBox.Text += Message;
                }
                if (!String.IsNullOrEmpty(Message))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(Message);
                    Stream.Write(buffer, 0, buffer.Length);
                   
                }
                sendTextBox.Text = "";
            }
        }

        private void RecieveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[64];
                    StringBuilder Builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = Stream.Read(buffer, 0, buffer.Length);
                        Builder.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
                    } while (Stream.DataAvailable);

                    string Message = Builder.ToString();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ChatTexBox.Text += Message;
                    });
                }
                catch
                {
                    Stream.Close();
                    Client.Close();
                }
            }
        }

        private void Disconnect()
        {
            if (Stream != null)
            {
                string Message = "UserDisconnect";
                byte[] buffer = Encoding.UTF8.GetBytes(Message);
                Stream.Write(buffer, 0, buffer.Length);
                Stream.Close();
            }
            if (Client != null)
            {
                Client.Close();
            }
        }

        private void UsersOnlineList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         
        }
    }
}
