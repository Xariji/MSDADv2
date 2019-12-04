using System;
using System.Collections.Generic;

namespace Library
{
    [Serializable]
    public class MeetingLocation
    {

        List<MeetingRoom> meetRooms;
        String nameLocation;

	    public MeetingLocation(String nameLocation)
	    {
            this.nameLocation = nameLocation;
            this.meetRooms = new List<MeetingRoom>();

	    }


        public void addRoom(MeetingRoom meetRoom)
        {
            meetRooms.Add(meetRoom);
        }

        public List<MeetingRoom> GetMeetingRooms()
        {
            return meetRooms;
        }

        public String getName()
        {
            return nameLocation;
        }

        public int getRoomsCount()
        {
            return meetRooms.Count;
        }
    }
}
