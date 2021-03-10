using ChatClient.Model;
using DataLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChatClient
{
    class RegisterInput
    {
        private Window Box = new Window();
        private FontFamily font = new FontFamily("Tahoma");
        private int FontSize = 24;
        private StackPanel sp1 = new StackPanel();
        private string title = "LogIn";
        private string boxcontent;
        private string errormessage = "Hasła się nie zgadzają";
        private string errortitle = "Error";
        private string okbuttontext = "OK";
        private Brush BoxBackgroundColor = Brushes.Aqua;
        private Brush InputBackgroundColor = Brushes.Gray;
        private bool clicked = false;
        private TextBox input = new TextBox();
        private PasswordBox input2 = new PasswordBox();
        private PasswordBox input3 = new PasswordBox();
        private Button ok = new Button();

            public RegisterInput(string content)
            {
                try
                {
                    boxcontent = content;
                }
                catch { boxcontent = "Error!"; }
                windowdef();
            }


            private void windowdef()
            {
                Box.Height = 300;
                Box.Width = 300;
                Box.MinHeight = 250;
                Box.MaxHeight = 350;
                Box.MinWidth = 250;
                Box.MaxWidth = 350;
                Box.Background = BoxBackgroundColor;
                Box.Title = title;
                Box.Content = sp1;
                Box.Closing += Box_Closing;
                TextBlock content = new TextBlock();
                content.TextWrapping = TextWrapping.Wrap;
                content.Background = null;
                content.HorizontalAlignment = HorizontalAlignment.Center;
                content.Text = boxcontent;
                content.FontFamily = font;
                content.FontSize = FontSize;
                sp1.Children.Add(content);

                Label label1 = new Label();
                label1.Content = "Login";
                label1.HorizontalContentAlignment = HorizontalAlignment.Center;
                sp1.Children.Add(label1);

                input.Background = InputBackgroundColor;
                input.FontFamily = font;
                input.FontSize = FontSize;
                input.HorizontalAlignment = HorizontalAlignment.Center;
                input.MinWidth = 200;
                sp1.Children.Add(input);

                Label label2 = new Label();
                label2.Content = "Password";
                label2.HorizontalContentAlignment = HorizontalAlignment.Center;
                sp1.Children.Add(label2);

                input2.Background = InputBackgroundColor;
                input2.FontFamily = font;
                input2.FontSize = FontSize;
                input2.HorizontalAlignment = HorizontalAlignment.Center;
                input2.MinWidth = 200;
                sp1.Children.Add(input2);

                Label label3 = new Label();
                label3.Content = "Repeat Password";
                label3.HorizontalContentAlignment = HorizontalAlignment.Center;
                sp1.Children.Add(label3);

                input3.Background = InputBackgroundColor;
                input3.FontFamily = font;
                input3.FontSize = FontSize;
                input3.HorizontalAlignment = HorizontalAlignment.Center;
                input3.MinWidth = 200;
                sp1.Children.Add(input3);

                ok.Width = 70;
                ok.Height = 30;
                ok.Click += ok_Click;
                ok.Content = okbuttontext;
                ok.HorizontalAlignment = HorizontalAlignment.Center;
                sp1.Children.Add(ok);

            }

            void Box_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            {
                if (!clicked)
                    e.Cancel = true;
            }


            async void ok_Click(object sender, RoutedEventArgs e)
            {
                DataAccess _dataAccess = new DataAccess();
                clicked = true;
                List<UserModel> user;
                string sql = $"select * from myusers where NickName like @NickName";
                var settings = ConfigurationManager.ConnectionStrings;
                string connString =   settings[0].ConnectionString;
                user = await _dataAccess.LoadData<UserModel, dynamic>(sql, new { NickName = input.Text }, connString);

                if (user.Count != 0)
                {
                    MessageBox.Show("Podany login jest już zajęty.", errortitle);
                }
                else if (input2.Password != input3.Password)
                {
                    MessageBox.Show(errormessage, errortitle);
                }
                else
                {
                    Box.Close();
                }
                clicked = false;
            }

            public object ShowDialog()
            {
                Box.ShowDialog();
                return new { Login = input.Text, Password = input2.Password };
            }
        }
}
