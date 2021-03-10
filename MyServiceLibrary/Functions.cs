using System;
using System.Collections.Generic;
using System.Text;

namespace MyServiceLibrary
{
    public class Functions
    {
        public void JoinRoom(string roomName, HashSet<string> roomSet)
        {
            roomSet.Add(roomName);
        }

        public bool LeaveRoom(string roomName, HashSet<string> roomSet)
        {
            if (IsMember(roomName, roomSet))
            {
                roomSet.Remove(roomName);
                return true;
            }
            return false;

        }
        public bool IsMember(string roomName, HashSet<string> roomSet)
        {
            return roomSet.Contains(roomName);
        }
    }
}
