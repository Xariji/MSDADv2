using System;
using System.Collections.Generic;
using System.Text;

namespace Library
{
    [Serializable]
    public class MeetingProposal
    {
        public enum Status
        {
            Open, 
            Closed,
            Cancelled
        }
        private int id;
        private Status status; 
        private User coordinator;
        private String topic;
        private int minParticipants;
        private List<Slot> slots;
        private List<User> invitees;
        private List<MeetingRecord> meetingRecs;
        private List<User> meetingParticipants;

        public MeetingProposal(int id, User coordinator, string topic, int minParticipants, List<Slot> slots, List<User> invitees )
        {
            this.id = id;
            this.status = Status.Open;
            this.coordinator = coordinator;
            this.topic = topic;
            this.minParticipants = minParticipants;
            this.slots = slots;
            this.invitees = invitees;
            this.meetingRecs = new List<MeetingRecord>();
            this.meetingParticipants = new List<User>();
        }
        // should we create the Meeting record here or just give the meetingRecord already created?
        //also not sure if the slots can be more than one
        public void addMeetingRec(User user, List<Slot> slots)
        {
            MeetingRecord mr = new MeetingRecord(user, slots);
            this.meetingRecs.Add(mr);
        }

        // Just to make sure if a user can join this meeting proposal
        public Boolean canJoin(User user)
        {
            if(status != Status.Open)
            {
                return false;
            }
            Boolean canJoin = false;
            if(this.invitees.Count == 0)
            {
                canJoin = true;
            }
            else if (this.invitees.Contains(user))
            {
                canJoin = true;
            }

            return canJoin;
        }

        override public string ToString()
        {
            StringBuilder fs = new StringBuilder();
            fs.Append("(ID: " + id + "| Topic: " + topic + "| Minimum Participants: " + minParticipants + "| Slots: ");
            foreach(Slot slot in slots)
            {
                fs.Append(slot.ToString() + "; ");
            }
            fs.Append("| Invitees: ");
            foreach (User invitee in invitees)
            {
                fs.Append(invitee.getName() + ", ");
            }
            fs.Append(")");
            return fs.ToString();
        }

        public int getMPId()
        {
            return id;
        }

        public List<Slot> getSlots()
        {
            return slots;
        }

        public int getMinParticipants()
        {
            return minParticipants;
        }

        public void setStatus(Status newStatus)
        {
            this.status = newStatus;
        }

        public Status getStatus()
        {
            return status;
        }
        public List<MeetingRecord> GetMeetingRecords()
        {
            return meetingRecs;
        }

        public String getMPTopic()
        {
            return topic;
        }

        public List<User> GetMeetingParticipants()
        {
            return meetingParticipants;
        }

        public void addUserToMeetingParticipants(User u)
        {
            meetingParticipants.Add(u);
        }
    }
}

