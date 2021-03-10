using Xunit;
using ChatClientLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClientLibrary.Tests
{
    public class FunctionsTests
    {
        [Fact()]
        public void IsPasswordOKTest()
        {

            Functions functions = new Functions();
            string hashPassword = "0GNW9RYxUDJ3zCNUNHjwHlRw1GZWxP3HoMnweVNFDXPg2Xma";
            string password = "qwe";
            Assert.True(functions.IsPasswordOK(hashPassword, password));
        }

        [Fact()]
        public void IsPasswordOKTest1()
        {
            Functions functions = new Functions();
            string hashPassword = "0GNW9RYxUDJ3zCNUNHjwHlRw1GZWxP3HoMnweVNFDXPg2Xma";
            string password = "qqwe";
            Assert.Throws<UnauthorizedAccessException>(() =>
                {
                    functions.IsPasswordOK(hashPassword, password);
                });
        }
    }
}