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

        public String encodeSOAP()
        {
            String s = nameLocation;
            foreach (MeetingRoom mr in meetRooms)
            {
                s += " " + mr.GetName() + ";" + mr.GetCapacity() + "|";
                foreach (String date in mr.getDates())
                {
                    s += date + ";";
                }
                s = s.Remove(s.Length - 1);
            }
            return s;
        }

        public void decodeSOAP(String s)
        {
            String[] args = s.Split(' ');
            nameLocation = args[0];
            for(int i=1; i<args.Length; i++)
            {
                String[] args2 = args[i].Split('|');
                String[] args3a = args2[0].Split(';');
                
                MeetingRoom mr = new MeetingRoom(args3a[0], Int32.Parse(args3a[1]));
                if(args2.Length > 1)
                {
                    String[] args3b = args2[1].Split(';');
                    List<String> dates = new List<String>(args3b);
                    mr.setDates(dates);
                }
                meetRooms.Add(mr);
            }
        }
    }
}
