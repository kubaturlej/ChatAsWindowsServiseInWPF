namespace ChatClient.Model
{
    class UserModel
    {
        public int Id { get; set; }
        public string NickName { get; set; }
        public string PasswordHash { get; set; }
        public bool IsOnline { get; set; } 
    }
}
