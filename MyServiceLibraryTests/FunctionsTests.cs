using Xunit;
using MyServiceLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyServiceLibrary.Tests
{
    public class FunctionsTests
    {
        [Fact()]
        public void LeaveRoomTest()
        {
            HashSet<string> roomSet = new HashSet<string>();
            roomSet.Add("room");
            Functions functions = new Functions();
            bool result = functions.LeaveRoom("room", roomSet);
            Assert.True(result);
        }

        [Fact()]
        public void IsMemberTest()
        {
            HashSet<string> roomSet = new HashSet<string>();
            roomSet.Add("room");
            Functions functions = new Functions();
            bool result = functions.IsMember("room", roomSet);
            Assert.True(result);
        }
        [Fact()]
        public void IsMemberTest1()
        {
            HashSet<string> roomSet = new HashSet<string>();
            Functions functions = new Functions();
            bool result = functions.IsMember("room", roomSet);
            Assert.False(result);
        }
    }
}