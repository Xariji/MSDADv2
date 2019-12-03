using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.Remoting;


namespace Server
{
    public class ServerCli : MarshalByRefObject, ISchedulingServer
    {

        List<MeetingProposal> meetingProposals;
        List<MeetingLocation> meetingLocations;
        List<IClient> clientsList;

        int currMPId;
        SchedulingServer server;

        //Freeze and unfreeze
        private bool isFrozen;
        private int frozenRequests;

        private readonly Random seedRandom = new Random();

        private readonly EventWaitHandle frozenRequestsHandler;
        private readonly EventWaitHandle handler;

        private int IncrementFrozenRequests() { return Interlocked.Increment(ref this.frozenRequests); }
        private int DecrementFrozenRequests() { return Interlocked.Decrement(ref this.frozenRequests); }

        public ServerCli(SchedulingServer server)
        {
            meetingProposals = new List<MeetingProposal>();
            meetingLocations = new List<MeetingLocation>();
            MeetingLocation ml = new MeetingLocation("Lisboa");
            ml.addRoom(new MeetingRoom("Room-C1", 5));
            meetingLocations.Add(ml);
            ml = new MeetingLocation("Porto");
            meetingLocations.Add(ml);
            clientsList = new List<IClient>();
            currMPId = 0;
            this.server = server;


            //freezing
            this.isFrozen = false;
            this.handler = new EventWaitHandle(false, EventResetMode.ManualReset);
            this.frozenRequestsHandler = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        private Message Register(string url) { 

            IClient client = (IClient)Activator.GetObject(typeof(IClient), url);
            clientsList.Add(client);

            Message mess = new Message(true, server.getBackupServer(), "conected to Server " + server.GetId());
            Console.WriteLine("Client " + client.getUser().getName() + " connected.");
            return mess;
        }


    private Message AddMeetingProposal(String topic, int minParticipants,
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
        if (invitees.Length > 0)
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
        foreach (MeetingProposal meetingprop in meetingProposals)
        {
            if (meetingprop.getMPTopic() == topic)
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
                    message = "Meeting " + mp.getMPTopic() + " created successfully.";

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
        return new Message(finalExists, mp, message);
    }



    private Message AddUserToProposal(String meetingTopic, string username, string[] slots)
    {
        IClient ic = findClient(username);
        User user = ic.getUser();

        List<Slot> slotsList = new List<Slot>();
        foreach (string slotUnformat in slots)
        {
            string[] slotFormat = slotUnformat.Split(
               new[] { ";" },
               StringSplitOptions.None);
            MeetingLocation meetingLocation = null;
            foreach (MeetingLocation ml in GetAvailableMeetingLocations())
            {
                if (ml.getName() == slotFormat[0])
                {
                    meetingLocation = ml;
                    slotsList.Add(new Slot(meetingLocation, slotFormat[1]));
                }
            }


        }

        foreach (MeetingProposal mp in meetingProposals)
        {
            if (mp.getMPTopic() == meetingTopic)
            {
                if (mp.canJoin(user))
                {
                    mp.addMeetingRec(user, slotsList);
                    user.addActiveMP(mp);
                    Console.WriteLine("User " + user.getName() + " joined meeting " + meetingTopic);
                }
                else
                {
                    Console.WriteLine("User " + user.getName() + " failed joining meeting " + meetingTopic + ". Meeting is restricted.");
                }
                //ID of MeetingProposal found: return 1
                ic.setUser(user);
                return new Message(mp.canJoin(user), 1, "");
            }
        }
        //No ID of MeetingProposal found: return 0
        ic.setUser(user);
        return new Message(false, 0, "User " + user.getName() + " failed joining meeting " + meetingTopic + ". Topic not found.");

    }

    private void AddMeetingLocation(string location)
    {
        MeetingLocation ml = new MeetingLocation(location);
        meetingLocations.Add(ml);
    }

    private Message CloseMeetingProposal(String meetingTopic, string username)
    {
        IClient ic = findClient(username);
        User user = ic.getUser();
        foreach (MeetingProposal mp in meetingProposals)
        {
            if (mp.getMPTopic() == meetingTopic)
            {
                if (mp.getStatus() == MeetingProposal.Status.Open)
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
                        return new Message(true, dm.Item2, "Meeting closed successfully.");
                    }
                    else
                    {
                        mp.setStatus(MeetingProposal.Status.Cancelled);
                        Console.WriteLine("---Meeting cancelled.");
                        ic.setUser(user);
                        return new Message(true, null, "Meeting cancelled.");
                    }
                }
            }
        }
        ic.setUser(user);


        return new Message(true, null, "Meeting to close not found.");
    }

    // decide meeting place and location based on the users that are going to attend
    // as well as the location and dates they chose

    // with this in the meeting locations we have to check if the rooms is gonna be occupied for the day
    // and if its not we have to record the unavailability
    private Tuple<Boolean, String> DecideMeeting(MeetingProposal mp)
    {
        //Identify the most popular slot
        List<Tuple<Slot, int>> popularSlots = new List<Tuple<Slot, int>>();
        List<User> lu = new List<User>();
        for (int i = 0; i < mp.getSlots().Count(); i++)
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
                        popularSlots[i] = new Tuple<Slot, int>(popularSlots[i].Item1, popularSlots[i].Item2 + 1);
                    }
                }
            }
        }
        popularSlots.OrderBy(x => x.Item2).ToList();
        //Evaluate if Location is free
        foreach (Tuple<Slot, int> tuple in popularSlots)
        {
            MeetingLocation location = tuple.Item1.GetMeetingLocation();
            String time = tuple.Item1.GetDate();
            foreach (MeetingLocation ml in meetingLocations)
            {
                foreach (MeetingRoom mr in ml.GetMeetingRooms())
                {
                    //If meeting room is free in desired location
                    if (!mr.isBooked(time))
                    {
                        //If number of participants is equal or larger than the minimum number of participants specified in the proposal
                        if (lu.Count >= mp.getMinParticipants())
                        {
                            //If number of registered users is bigger than room capacity
                            if (lu.Count > mr.GetCapacity())
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
                            return Tuple.Create(true, "Room " + mr.GetName() + " in " + ml.getName() + " booked on " + time);
                        }
                    }
                }
            }
        }
        return Tuple.Create(false, "");
    }

    private int getCurrMPId()
    {
        return currMPId;
    }

    private List<MeetingProposal> getJoinableMP(String username)
    {
        IClient ic = findClient(username);
        User user = ic.getUser();
        List<MeetingProposal> mps = new List<MeetingProposal>();
        foreach (MeetingProposal mp in meetingProposals)
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
        foreach (MeetingLocation loc in meetingLocations)
        {
            if (loc.getName().Equals(location))
            {
                loc.addRoom(new MeetingRoom(room, capacity));
            }
        }
    }
    private List<MeetingRoom> GetAvailableMeetingRooms()
    {
        List<MeetingRoom> mrs = new List<MeetingRoom>();
        foreach (MeetingLocation ml in meetingLocations)
        {
            mrs.AddRange(ml.GetMeetingRooms());
        }
        return mrs;
    }

    private List<MeetingLocation> GetAvailableMeetingLocations()
    {
        return meetingLocations;
    }

    //should be called on client-side each time an user is updated 
    private IClient findClient(String username)
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

    private Message GetServerId()
    {
        return new Message(true, null, server.GetId());
    }

    public void freeze()
    {

            Console.WriteLine("This server is frozen");
            frozenRequests = 0;
            isFrozen = true;
    }

    public void unfreeze() {

            Console.WriteLine("This server unfroze");

            isFrozen = false;
            this.handler.Set();
            this.handler.Reset();
        }


    // this has to work for every request
    // should we reply something to the client or just do it
    // this handles multi-threading
    public Message Response(String request, List<String> args)//Request request)
    {
        Console.WriteLine("Request");

        int delay = seedRandom.Next(server.getMinDelay(), server.getMaxDelay());

        Thread.Sleep(delay);

        Message mess;

        if (isFrozen)
        {

            this.IncrementFrozenRequests();
            while (this.isFrozen)
            {
                    Console.WriteLine("bolas");
                    this.handler.WaitOne();
            }
            Console.WriteLine("carambolas");
            mess = requestHandle(request, args);

            this.DecrementFrozenRequests();
            this.frozenRequestsHandler.Set();
            this.frozenRequestsHandler.Reset();
        }
        else
        {
            while (this.frozenRequests > 0)
            {
                this.frozenRequestsHandler.WaitOne();
            }

            mess = requestHandle(request, args);
        }

        return mess;
    }

    public Message requestHandle(String request, List<String> args)
    {

        Message mess;
        if (request == "Register") // register
        {
            mess = Register(args[0]);
        }
        else if (request == "CloseMeetingProposal") // close
        {
            mess = CloseMeetingProposal(args[0], args[1]);
        }
        else if (request == "AddMeetingProposal") //create
        {
            String username = args[0];
            String topic = args[1];
            int minPart = Int32.Parse(args[2]);
            int slotsSize = Int32.Parse(args[3]);
            String[] slots = new string[slotsSize];
            for (int i = 0; i < slotsSize; i++)
            {
                slots[i] = args[4 + i];
            }
            int inviteesSize = Int32.Parse(args[4 + slotsSize]);
            String[] invitees = new string[inviteesSize];
            for (int i = 0; i < inviteesSize; i++)
            {
                invitees[i] = args[4 + slotsSize + i];
            }
            mess = AddMeetingProposal(topic, minPart, slots, invitees, username);
        }
        else if (request == "AddUserToProposal") //join 
        {
            string[] slots = args[3].Split(' ');
            mess = AddUserToProposal(args[0], args[1], slots); 
        }
        else if (request == "GetServerId") //get serverID 
        {
                mess = GetServerId();
        }
        else if (request == "RemoveServerFromView") //get serverID
        {
            mess = removeServerFromView(args);
        }
        else if(request == "GetSharedClientsList"){
            mess = getSharedClientsList();
        }
        else
        {
            mess = new Message(false, null, "Operation not supported by the Server");
        }

        return mess;
    }
    public void initializeView(String serverid, String serverurl)
    {
        server.updateView("add", serverid, serverurl);
    }

        public void addServerToView(String serverid, String serverurl)
        {
            //prevent deadlock
            if (!server.getView().ContainsKey(serverid))
            {
                lock (server)
                {
                    Console.WriteLine("-------- BEGIN VIEW UPDATE --------");
                    server.updateView("add", serverid, serverurl);
                    Console.WriteLine("Server added: " + serverid + " @ " + serverurl);
                    String backupInfo = server.getBackupServer()[0];
                    for (int i = 1; i < server.getBackupServer().Length; i++)
                    {
                        backupInfo += ", " + server.getBackupServer()[i];
                    }
                    Console.WriteLine("New Backup-URL: " + backupInfo);
                    if (!server.getBackupServer().Equals(server.getURL()))
                    {
                        ServerCli bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), server.getBackupServer()[0]);
                        bscli.addServerToView(serverid, serverurl);
                    }
                    //send command to update backup server to clients
                    foreach (IClient client in clientsList)
                    {
                        client.setBackupServerURL(server.getBackupServer());
                    }
                    Console.WriteLine("Backup server in " + clientsList.Count + " client(s) updated.");
                    Console.WriteLine("--------- END VIEW UPDATE ---------");
                }

            }
        }

        public Message removeServerFromView(List<String> serverurls)
        {
            foreach(String serverurl in serverurls)
            {
                String serverid = server.getView().Keys[server.getView().IndexOfValue(serverurl)];
            
                //prevent deadlock
                if (server.getView().ContainsKey(serverid))
                {
                    lock (server)
                    {
                        Console.WriteLine("-------- BEGIN VIEW UPDATE --------");
                        server.updateView("remove", serverid, serverurl);
                        Console.WriteLine("Server removed: " + serverid + " @ " + serverurl);
                        String backupInfo = server.getBackupServer()[0];
                        for (int i = 1; i < server.getBackupServer().Length; i++)
                        {
                            backupInfo += ", " + server.getBackupServer()[i];
                        }
                        Console.WriteLine("New Backup-URL: " + backupInfo);

                        if (!server.getBackupServer()[0].Equals(server.getURL()))
                        {
                            tryConnectToBackup(0);

                            void tryConnectToBackup(int indexBackupUpdate)
                            {
                                try
                                {
                                    ServerCli bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), server.getBackupServer()[indexBackupUpdate]);
                                    bscli.addServerToView(serverid, serverurl);
                                }
                                catch (Exception e)
                                {
                                    if (indexBackupUpdate + 1 < server.getBackupServer().Length)
                                    {
                                        tryConnectToBackup(indexBackupUpdate + 1);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error: No Backup-server reachable!");
                                    }
                                }
                            }
                        }
                        //send command to update backup server to clients
                        foreach (IClient client in clientsList)
                        {
                            client.setBackupServerURL(server.getBackupServer());
                        }
                        Console.WriteLine("Backup server in " + clientsList.Count + " client(s) updated.");
                        Console.WriteLine("--------- END VIEW UPDATE ---------");
                    }

                }
            }
            return new Message(true, null, "");
        }

        //Get client urls from the server
        public List<string> getClientsList(){

            List<string> list = new List<string>();

            //Gather all the clients registered on this server
            foreach(IClient c in clientsList){
                list.Add(c.getClientURL());
            }
            return list;
        }

        //Get client urls from the server plus one alive from backup
        public Message getSharedClientsList(){

            List<string> list = null;
            List<string> auxList = null;
            int indexBackupUpdate = 0;
            ServerCli bscli = null;

            //get servers clients list
            list = getClientsList();

            //find a active backup server
            if (!server.getBackupServer()[0].Equals(server.getURL()))
            {
                try
                {
                    bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), server.getBackupServer()[indexBackupUpdate]);  
                }
                catch (Exception e)
                {
                    if(indexBackupUpdate + 1 < server.getBackupServer().Length)
                    {
                        bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), server.getBackupServer()[indexBackupUpdate + 1]);
                    }
                }
            }

            if(bscli != null){
                //get the clients list from the found backup server
                auxList = bscli.getClientsList();
                if(auxList.Count() != 0){
                    list.Add(auxList[0]);
                }
            }

            return new Message(true, list, "");

        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
    }
