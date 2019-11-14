using System;
using System.Collections.Generic;


namespace Library
{

    [Serializable]
    public class MeetingRoom
{

    private int capacity;
    private String name;
    private List<String> dates;

	public MeetingRoom(String name, int capacity)
    {
        this.name = name;
        this.capacity = capacity;
        dates = new List<string>();
    }

        public String GetName()
        {
            return name;
        }
	
    //When a room becomes booked the date becomes unavailable
    public void book(String date)
    {
        dates.Add(date);
    }

    //just to check if the room is booked on that day
    public Boolean isBooked(String date)
    {
        return dates.Contains(date);
    }

        public int GetCapacity()
        {
            return capacity;
        }
}
}
