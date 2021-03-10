using Xunit;
using DataLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary.Tests
{

    class UserModel
    {
        public int Id { get; set; }
        public string NickName { get; set; }
        public string PasswordHash { get; set; }
        public bool IsOnline { get; set; }
    }
    public class DataAccessTests
    {
        async public void LoadDataTest()
        {
            private List<UserModel> user;
            private string connectionString = "";
    
            IDataAccess dataAccess = new DataAccess();
            string sql = $"select * from myusers where NickName like @NickName";
        user = await dataAccess.LoadData<UserModel, dynamic>(sql, new {NickName = obj.Login}, connString);
        }
    }
}
