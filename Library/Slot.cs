using System;


namespace Library
{

    // a Slot that represents the availability of the user to participate in the meeting
    [Serializable]
    public class Slot
    {
        private MeetingLocation location;
        private String date;
	    public Slot(MeetingLocation location, String date)
	    {
            this.location = location;
            this.date = date;
	    }
        public MeetingLocation GetMeetingLocation()
        {
            return location;
        }
        public String GetDate()
        {
            return date;
        }

        override public String ToString()
        {
            return location.getName() + ", " + date;
        }
    }
}
