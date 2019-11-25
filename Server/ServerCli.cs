using Library;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public class ServerCli : MarshalByRefObject, ISchedulingServer
    {

        List<MeetingProposal> meetingProposals;
        List<MeetingLocation> meetingLocations;
        List<IClient> clientsList;
        //List<User> userList;
        int currMPId;
        SchedulingServer server;

        public ServerCli(SchedulingServer server)
        {
            meetingProposals = new List<MeetingProposal>();
            meetingLocations = new List<MeetingLocation>();
            MeetingLocation ml = new MeetingLocation("Lisboa");
            ml.addRoom(new MeetingRoom("Room-C1",5));
            meetingLocations.Add(ml);
            ml = new MeetingLocation("Porto");
            meetingLocations.Add(ml);
            clientsList = new List<IClient>();
            //userList = new List<User>();
            currMPId = 0;
            this.server = server;
        }

        public String Register(string url)
        {
            IClient client = (IClient)Activator.GetObject(typeof(IClient), url);
            clientsList.Add(client);
            Console.WriteLine("User " + client.getUser().getName() + " registered.");
            return server.getBackupServer();
        }

        public String GetName()
        {
            return server.GetId();
        }

        public Tuple<Boolean, string> AddMeetingProposal(String topic, int minParticipants, 
            string[] slots, string[] invitees, string username)
        {
            IClient ic = findClient(username);
            User user = ic.getUser();
            //First validate the invitees
            Boolean finalExists = false;
            Boolean usersExists = true;
            string userNotFound = "";
            string message = "";
            List<User> users = new List<User>();
            MeetingProposal mp = null;
            if(invitees.Length > 0)
            {
                for (int i = 0; i < invitees.Length && usersExists; i++)
                {
                    usersExists = false;
                    for (int j = 0; j < clientsList.Count && !usersExists; j++)
                    {
                        if (clientsList[j].getUser().getName().Equals(invitees[i]))
                        {
                            usersExists = true;
                            users.Add(clientsList[j].getUser());
                        }
                    }
                    userNotFound = invitees[i];
                }
            }
            else
            {
                usersExists = false;
            }
            Boolean topicExists = false;
            foreach(MeetingProposal meetingprop in meetingProposals)
            {
                if(meetingprop.getMPTopic() == topic)
                {
                    topicExists = true;
                }
            }
            if (!topicExists)
            {
                if (usersExists || invitees.Length == 0)
                {
                    //if all invitees exists, validate slot list
                    Boolean slotsExists = true;
                    string slotNotFound = "";
                    List<Slot> slotsList = new List<Slot>();
                    if (slots.Length > 0)
                    {
                        for (int x = 0; x < slots.Length && slotsExists; x++)
                        {
                            string[] slotFormat = slots[x].Split(
                               new[] { ";" },
                               StringSplitOptions.None);
                            slotsExists = false;
                            for (int z = 0; z < meetingLocations.Count && !slotsExists; z++)
                            {
                                //validate location
                                if (meetingLocations[z].getName().Equals(slotFormat[0]))
                                {
                                    slotsList.Add(new Slot(meetingLocations[z], slotFormat[1]));
                                    slotsExists = true;
                                }
                            }
                            slotNotFound = slots[x];
                        }
                    }
                    else
                    {
                        slotsExists = false;
                    }
                    if (slotsExists)
                    {
                        finalExists = true;
                        
                        mp = new MeetingProposal(getCurrMPId() + 1, user, topic, minParticipants, slotsList, users);
                        meetingProposals.Add(mp);
                        user.addMyMP(mp);
                        currMPId++;
                        Console.WriteLine("Meeting " + mp.getMPTopic() + " created successfully.");
                    }
                    else
                    {
                        message = "Slot " + slotNotFound + " doesn't exist";
                    }
                }
                else
                {
                    message = "User " + userNotFound + " doesn't exist";
                }
            }
            else
            {
                message = "Meeting with that topic already exists";
            }
            ic.setUser(user);
            return Tuple.Create(finalExists, message);
        }

        //private bool validateRooms(List<Slot> list)
        //{
        //    foreach(MeetingLocation ml in meetingLocations)
        //    {
        //        foreach(Slot slot in list)
        //        {
        //            if(!ml.GetMeetingRooms().Contains(slot.GetMeetingRoom()))
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

        public Tuple<Boolean, int> AddUserToProposal(String meetingTopic, string username, List<Slot> slots)
        {
            IClient ic = findClient(username);
            User user = ic.getUser();
            foreach (MeetingProposal mp in meetingProposals)
            {
                if (mp.getMPTopic() == meetingTopic)
                {
                    if (mp.canJoin(user))
                    {
                        mp.addMeetingRec(user, slots);
                        user.addActiveMP(mp);
                        Console.WriteLine("User " + user.getName() + " joined meeting " + meetingTopic);
                    }
                    else
                    {
                        Console.WriteLine("User " + user.getName() + " failed joining meeting " + meetingTopic + ". Meeting is restricted.");
                    }
                    //ID of MeetingProposal found: return 1
                    ic.setUser(user);
                    return Tuple.Create(mp.canJoin(user), 1);
                }
            }
            //No ID of MeetingProposal found: return 0
            ic.setUser(user);
            Console.WriteLine("User " + user.getName() + " failed joining meeting " + meetingTopic + ". Topic not found.");
            return Tuple.Create(false, 0);
        }

        public void AddMeetingLocation(string location)
        {
            MeetingLocation ml = new MeetingLocation(location);
            meetingLocations.Add(ml);
        }

        public List<String> CloseMeetingProposal(String meetingTopic, string username)
        {
            IClient ic = findClient(username);
            User user = ic.getUser();
            foreach (MeetingProposal mp in meetingProposals)
            {
                if(mp.getMPTopic() == meetingTopic)
                {
                    if(mp.getStatus() == MeetingProposal.Status.Open)
                    {
                        Console.WriteLine("User " + user.getName() + " prompts to close meeting " + meetingTopic);
                        user.removeMyMP(mp);
                        Console.WriteLine("---Meeting removed from user's myMP list.");
                        foreach (IClient ict in clientsList)
                        {
                            ict.getUser().removeActiveMP(mp);
                        }
                        Console.WriteLine("---Meeting removed from all users activeMP list.");
                        Tuple<Boolean, String> dm = DecideMeeting(mp);
                        if (dm.Item1)
                        {
                            mp.setStatus(MeetingProposal.Status.Closed);
                            Console.WriteLine("---Meeting closed successfully.");
                            Console.WriteLine(dm.Item2);
                            ic.setUser(user);
                            return new List<String> { "Meeting closed successfully.", dm.Item2 };
                        }
                        else
                        {
                            mp.setStatus(MeetingProposal.Status.Cancelled);
                            Console.WriteLine("---Meeting cancelled.");
                            ic.setUser(user);
                            return new List<String> { "Meeting cancelled." };
                        }
                    }   
                }
            }
            Console.WriteLine("Meeting to close not found.");
            ic.setUser(user);
            return new List<String> { "Meeting to close not found." };
            //its necessary to signal clients that the proposal is closed or 
            // alternatively just to remove the proposal and deal with the 
            // future client requests 


            // this is gonna call decideMeeting
        }

        // decide meeting place and location based on the users that are going to attend
        // as well as the location and dates they chose

        // with this in the meeting locations we have to check if the rooms is gonna be occupied for the day
        // and if its not we have to record the unavailability
        private Tuple<Boolean,String> DecideMeeting(MeetingProposal mp)
        {
            //Identify the most popular slot
            List<Tuple<Slot, int>> popularSlots = new List<Tuple<Slot, int>>();
            List<User> lu = new List<User>();
            for(int i=0; i<mp.getSlots().Count(); i++)
            {
                popularSlots.Add(Tuple.Create(mp.getSlots()[i], 0));
                foreach (MeetingRecord mr in mp.GetMeetingRecords())
                {
                    if (!lu.Contains(mr.GetUser()))
                    {
                        lu.Add(mr.GetUser());
                    }
                    foreach (Slot s in mr.GetSlots())
                    {
                        if (s.GetDate().Equals(mp.getSlots()[i].GetDate()) && s.GetMeetingLocation().getName().Equals(mp.getSlots()[i].GetMeetingLocation().getName()))
                        {
                            popularSlots[i] = new Tuple<Slot, int>(popularSlots[i].Item1, popularSlots[i].Item2+1);
                        }
                    }
                }
            }
            popularSlots.OrderBy(x => x.Item2).ToList();
            //Evaluate if Location is free
            foreach(Tuple<Slot, int> tuple in popularSlots)
            {
                MeetingLocation location = tuple.Item1.GetMeetingLocation();
                String time = tuple.Item1.GetDate();
                foreach(MeetingLocation ml in meetingLocations)
                {
                    foreach(MeetingRoom mr in ml.GetMeetingRooms())
                    {
                        //If meeting room is free in desired location
                        if (!mr.isBooked(time))
                        {
                            //If number of participants is equal or larger than the minimum number of participants specified in the proposal
                            if (lu.Count >= mp.getMinParticipants())
                            {
                                //If number of registered users is bigger than room capacity
                                if(lu.Count > mr.GetCapacity())
                                {
                                    //Exclude last n users
                                    int noToExclude = lu.Count - mr.GetCapacity();
                                    User[] usersToExclude = new User[noToExclude];
                                    for (int i = 0; i < noToExclude; i++)
                                    {
                                        usersToExclude[i] = lu.ElementAt(lu.Count - 1);
                                    }
                                    for (int i = 0; i < noToExclude; i++)
                                    {
                                        lu.RemoveAll(user => user == usersToExclude[i]);
                                        Console.WriteLine("User " + usersToExclude[i].getName() + " excluded from meeting.");
                                    }
                                }
                                //Book room
                                mr.book(time);
                                //Add users to participants list
                                foreach (IClient ic in clientsList)
                                {
                                    if (lu.Contains(ic.getUser()))
                                    {
                                        mp.addUserToMeetingParticipants(ic.getUser());
                                    }
                                }
                                return Tuple.Create(true,"Room " + mr.GetName() + " in " + ml.getName() + " booked on " + time);
                            }
                        }
                    }
                }
            }
            return Tuple.Create(false,"");
        }

        public int getCurrMPId()
        {
            return currMPId;
        }

        public List<MeetingProposal> getJoinableMP(String username)
        {
            IClient ic = findClient(username);
            User user = ic.getUser();
            List<MeetingProposal> mps = new List<MeetingProposal>();
            foreach(MeetingProposal mp in meetingProposals)
            {
                if (mp.canJoin(user))
                {
                    mps.Add(mp);
                }
            }
            return mps;
        }

        public void AddMeetingRoom(string location, string room, int capacity)
        {
            foreach(MeetingLocation loc in meetingLocations)
            {
                if (loc.getName().Equals(location))
                {
                    loc.addRoom(new MeetingRoom(room, capacity));
                }
            }
        }
        public List<MeetingRoom> GetAvailableMeetingRooms()
        {
            List<MeetingRoom> mrs = new List<MeetingRoom>();
            foreach (MeetingLocation ml in meetingLocations)
            {
                mrs.AddRange(ml.GetMeetingRooms());
            }
            return mrs;
        }

        public List<MeetingLocation> GetAvailableMeetingLocations()
        {
            return meetingLocations;
        }

        //should be called on client-side each time an user is updated 
        public IClient findClient(String username)
        {
            foreach (IClient ic in clientsList)
            {
                if (ic.getUser().getName().Equals(username))
                {
                    return ic;
                }
            }
            return null;
        }

        public String GetServerId()
        {
            return server.GetId();
        }

        public void addServerToView(String serverid, String serverurl)
        {
            lock (server)
            {
                server.updateView("add", serverid, serverurl);
                //TO-DO: Send update command to backup server
                Console.WriteLine("Backup-URL: " + server.getBackupServer());
                ServerCli bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), server.getBackupServer());
                bscli.addServerToView(serverid, serverurl);
                //send command to update backup server to clients
                foreach (IClient client in clientsList)
                {
                    client.setBackupServerURL(server.getBackupServer());
                }
            }  
        }

    }

 
}
