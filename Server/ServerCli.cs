using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.Remoting;
using System.Threading.Tasks;

namespace Server
{
    public class ServerCli : MarshalByRefObject, ISchedulingServer
    {

        List<MeetingProposal> meetingProposals;
        List<MeetingProposal>[] meetingProposalsBackup;
        List<MeetingLocation> meetingLocations;
        List<IClient> clientsList;

        //Handle closes
        Dictionary<int , string> closes = new Dictionary<int, string>();
        private int request = 1;
        private int seq = 0;
        private static int timeout = 10000;


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
            meetingProposalsBackup = new List<MeetingProposal>[0];
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

            //Update local client list on all Clients
            foreach(IClient ic in clientsList)
            {
                ic.updateLocalClients();
            }

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
                    
                    mp = new MeetingProposal(user, topic, minParticipants, slotsList, users);
                    meetingProposals.Add(mp);
                    user.addMyMP(mp);
                    currMPId++;
                    Console.WriteLine("Meeting " + mp.getMPTopic() + " created successfully.");
                    message = "Meeting " + mp.getMPTopic() + " created successfully.";
                    updateBackupProposals();
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
                Monitor.Enter(mp);
                if (mp.canJoin(user))
                {
                    mp.addMeetingRec(user, slotsList);
                    user.addActiveMP(mp);
                    Console.WriteLine("User " + user.getName() + " joined meeting " + meetingTopic);
                    updateBackupProposals();
                    
                    }
                else
                {
                    Console.WriteLine("User " + user.getName() + " failed joining meeting " + meetingTopic + ". Meeting is restricted.");
                }
                Monitor.Pulse(mp);
                Monitor.Exit(mp);
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
                Console.WriteLine("Vou fechar a minha");
        IClient ic = findClient(username);
        User user = ic.getUser();
                            Console.WriteLine("Vou fechar a minha");
        foreach (MeetingProposal mp in meetingProposals)
        {
            if (mp.getMPTopic() == meetingTopic)
            {
                    Monitor.Enter(mp);
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
                        updateBackupProposals();
                        Monitor.Pulse(mp);
                        Monitor.Exit(mp);
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
                else if (mp.getStatus() == MeetingProposal.Status.Closed)
                    {
                        Monitor.Pulse(mp);
                        Monitor.Exit(mp);

                        return new Message(true, null, "Meeting already closed.");
                    }
                else
                {
                        Monitor.Pulse(mp);
                        Monitor.Exit(mp);
                        return new Message(true, null, "Meeting cancelled.");
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
                            
                            //Book room on all servers
                             bookRoom(mr, time);
                             
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

    private void bookRoom(MeetingRoom mr, string time)
    {
        lock (meetingLocations)
        {
            foreach(MeetingLocation ml in meetingLocations)
            {
                foreach(MeetingRoom room in ml.GetMeetingRooms())
                {
                    if(room.GetName() == mr.GetName())
                    {
                        if (room.isBooked(time))
                        {
                            return;
                        }
                        else
                        {
                            room.book(time);
                        }
                    }
                }
            }
            if (!server.getBackupServer().Equals(server.getURL()))
            {
                ServerCli bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), server.getBackupServer()[0]);
                bscli.bookRoom(mr, time);
            }
        }
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
        IClient result = null;

        //look for the client on the server
        foreach (IClient ic in clientsList)
        {
            if (ic.getUser().getName().Equals(username))
            {
                result = ic;
            }
        }

        //look for the client on the other servers
        if(result == null){
            string[] backupList = server.getBackupServer();
            foreach(string servURL in backupList){
                ServerCli bscli = null;
                List<IClient> cliList = null;
                if(!servURL.Equals(server.getURL())){
                    try
                    {                        
                        bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), servURL);
                        cliList = bscli.getClientsList();
                        foreach(IClient c in cliList){
                            if(c.getUser().getName().Equals(username)){
                                result = c;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }

        return result;
    }

    private Message GetServerId()
    {
        Console.WriteLine("toma la o serverID");
        return new Message(true, server.GetId(), "");
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
    
    public void status()
        {
            foreach ( IClient ic in clientsList) 
            {
                ic.status();
            }

            if (isFrozen)
            {
                Console.WriteLine("Server: " + server.GetId() + " is online and frozen.");
            }
            else
            {
                Console.WriteLine("Server: " + server.GetId() + " is online and unfrozen.");
            
            }
            Console.WriteLine();
            Console.WriteLine("The backup server(s) are : ");

            foreach(KeyValuePair<string,string> entry in server.getView())
            {
                Console.WriteLine("ID: " + entry.Key + " URL: " + entry.Value);
            }
            Console.WriteLine();
            Console.WriteLine("The following server(s) are dead: ");

            foreach (KeyValuePair<string, string> entry in server.getCrashed())
            {
                Console.WriteLine("ID: " + entry.Key + " URL: " + entry.Value);
            }

        }
    // this has to work for every request
    // this handles multi-threading
    public Message Response(String request, List<String> args)//Request request)
    {
        int delay = seedRandom.Next(server.getMinDelay(), server.getMaxDelay());

        Thread.Sleep(delay);

        Message mess;

        if (isFrozen)
        {

            this.IncrementFrozenRequests();
            while (this.isFrozen)
            {
                    this.handler.WaitOne();
            }

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
            mess = closeRequest(args[0], args[1]); //CloseMeetingProposal(args[0], args[1]);        
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
            string[] slots = args[2].Split(' ');
                foreach (String s in slots) Console.WriteLine(s);
            Console.WriteLine("HERRRRREEEEEEEEEEEEEEEEE");
            mess = AddUserToProposal(args[0], args[1], slots); 
        }
        else if (request == "GetServerId") //get serverID 
        {
                mess = GetServerId();
        }
        else if (request == "RemoveServerFromView") //get serverID
        {
            mess = removeServerFromView(args);
            recreateBackupProposals();
        }
        else if(request == "GetSharedClientsList"){
            mess = getSharedClientsList();
        }
        else if(request == "GetMeetingProposalURL"){
            mess = GetMeetingProposalURL(args[0]);
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
                    //recreate BackupProposals
                    recreateBackupProposals();

                    Console.WriteLine("Backup server in " + clientsList.Count + " client(s) updated.");
                    Console.WriteLine("--------- END VIEW UPDATE ---------");
                }

            }
        }

        public Message removeServerFromView(List<String> serverurls)
        {
            foreach(String serverurl in serverurls)
            {
                if (server.getView().IndexOfValue(serverurl) != -1)
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
                                Console.WriteLine("Checkpoint 1");
                                tryConnectToBackup(0, serverurls);

                                void tryConnectToBackup(int indexBackupUpdate, List<String> args)
                                {
                                    try
                                    {
                                        Console.WriteLine("Checkpoint 2");
                                        ServerCli bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), server.getBackupServer()[indexBackupUpdate]);
                                        Console.WriteLine("Checkpoint 3");
                                        bscli.removeServerFromView(args);
                                        Console.WriteLine("Checkpoint 4");
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Checkpoint 6");
                                        if (indexBackupUpdate + 1 < server.getBackupServer().Length)
                                        {
                                            Console.WriteLine("Checkpoint 7");
                                            args.Add(server.getBackupServer()[indexBackupUpdate]);
                                            Console.WriteLine("Checkpoint 8");
                                            tryConnectToBackup(indexBackupUpdate + 1, args);
                                            Console.WriteLine("Checkpoint 9");
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

                
            }
            for(int i=0; i<serverurls.Count; i++)
            {
                meetingProposals.AddRange(meetingProposalsBackup[i]);
            }
            return new Message(true, null, "");
        }

        //Get client urls from the server
        public List<string> getClientsURLList(){

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
            ServerCli bscli = null;

            //get servers clients list
            list = getClientsURLList();
            string[] backupList = server.getBackupServer();
            //find a active backup server with active clients
            for(int i=0; i < backupList.Length; i++){
                if (!server.getBackupServer()[0].Equals(server.getURL()))
                {
                    try
                    {
                        bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), backupList[i]);
                    }
                    catch (Exception e)
                    {
                        if(i + 1 < backupList.Length)
                        {
                            bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), backupList[i + 1]);
                        }
                    }
                    if(bscli != null){
                    //get the clients list from the found backup server
                        auxList = bscli.getClientsURLList();
                        if(auxList.Count() != 0){
                            list.Add(auxList[0]);
                        }
                    }
                }
            }
       
            return new Message(true, list, "");

        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void updateLocations(String mls)
        {
            List<MeetingLocation> mlsList = new List<MeetingLocation>();

            String[] args = mls.Split('#');
            foreach (String str in args)
            {
                MeetingLocation ml = new MeetingLocation("");
                ml.decodeSOAP(str);
                mlsList.Add(ml);
            }
            meetingLocations = mlsList;
            Console.WriteLine("Meeting locations updated.");
        }

        public void recreateBackupProposals()
        {
            Console.WriteLine("Recreation of proposal backups started");
            foreach (String url in server.getView().Values)
            {
                try
                {
                    ServerCli bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), url);
                    bscli.createNewMPBackup();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Excpetion recreateBackupProposals: " + e);
                }
            }
            updateBackupProposals();
            Console.WriteLine("Recreation of proposal backups finished");
        }

        public void updateBackupProposals()
        {
            foreach (String url in server.getView().Values)
            {
                try
                {
                    ServerCli bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), url);
                    bscli.fillMPBackup();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Excpetion updateBackupProposals: " + e.Message + " on url: " + url);

                }
            }
        }

        public void createNewMPBackup()
        {
            try
            {
                lock (meetingProposalsBackup)
                {
                    meetingProposalsBackup = new List<MeetingProposal>[server.getBackupServer().Length];
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        public void fillMPBackup()
        {
            Console.WriteLine("Update of proposal backups started");
            for (int i = 0; i < server.getBackupServer().Length; i++)
            {
                try
                {
                    ServerCli bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), server.getBackupServer()[i]);
                    bscli.setMPBackup(i, meetingProposals);
                }
                catch(Exception e)
                {
                    removeServerFromView(new List<String>() { server.getBackupServer()[i] });
                    recreateBackupProposals();
                    fillMPBackup();
                }
                    
            }
            Console.WriteLine("Update of proposal backups finished");
        }

        public void setMPBackup(int index, List<MeetingProposal> mpList)
        {
            lock (meetingProposalsBackup)
            {
                if (meetingProposalsBackup.Length > 0)
                {
                    meetingProposalsBackup[index] = mpList;
                }
            }
        }

        public Message getClientURLs()
        {
            List<String> clientURLs = new List<String>();
            foreach(IClient ic in clientsList)
            {
                clientURLs.Add(ic.getClientURL());
            }
            return new Message(true, clientURLs, "");
        }

        public Message GetMeetingProposalURL(string mpTopic){

            //look if the server that has the mp is this one
            foreach(MeetingProposal serMP in meetingProposals){
                if(serMP.getMPTopic().Equals(mpTopic)){
                    return new Message(true, server.getURL(), "");
                }
            }

            //look for the proposal on another servers
            string[] backupList = server.getBackupServer();
            string result = null;
            foreach(string serv in backupList){
                ServerCli bscli = null;
                List<MeetingProposal> bserMPList = null;
                if(!serv.Equals(server.getURL())){
                    try
                    {                        
                        bscli = (ServerCli)Activator.GetObject(typeof(ServerCli), serv);
                        bserMPList = bscli.getServerMeetingProposals();
                        foreach(MeetingProposal m in bserMPList){
                            if(m.getMPTopic().Equals(mpTopic)){
                                result = serv;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            if(result == null){
                return new Message(false, null, "Meeting proposal not found!");
            } else{
                return new Message(true, result, "");
            }
        }

        public List<IClient> getClientsList(){
            return clientsList;
        }

        public List<MeetingProposal> getServerMeetingProposals(){
            return meetingProposals;
        }

        public Message closeRequest(string topic, string username)
        {
            string primary = server.getView().Values[0];
            ServerCli serv = (ServerCli)Activator.GetObject(typeof(ServerCli), primary);

            Message mess;// = serv.sequence(server.getURL(), topic, username);

            Task<Message> task = Task<Message>.Factory.StartNew(() => serv.sequence(server.getURL(), topic, username));
            bool done = task.Wait(timeout);
            if (done)
            {
                mess = task.Result;
            }
            else
            {
                mess = new Message(false, null, "Server to close timedout abort operation");
            }
            return mess;
        }

        public Message sequence(string url, string topic, string username)
        {
            Message mess = null;

            if(closes.ContainsValue(url + " " + topic + " " + username))
            {
                seq++;
                closes.Add(seq, url + " " + topic + " " + username);

                Monitor.Enter(request);
                closes.TryGetValue(request, out string str);

                string[] data = str.Split(' ');

                ServerCli serv = (ServerCli)Activator.GetObject(typeof(ServerCli), data[0]);

                Task<Message> task = Task<Message>.Factory.StartNew(() => serv.CloseMeetingProposal(data[1], data[2]));
                bool done = task.Wait(timeout);

                if (done)
                {
                    mess = task.Result;
                }

                else
                {
                    mess = new Message(false, null, "Server to close timedout abort operation");
                }

                request++;
                Monitor.Pulse(request);
                Monitor.Exit(request);
            }
            else
            {
                mess = new Message(false, null, "Duplicated request");

            }


            return mess;
        }


        /*A process wishing to TO-multicast a message m to group g attaches a unique identifier id(m) to it.
        The messages for g are sent to the sequencer for g, sequencer(g), as well as to the
        members of g. (The sequencer may be chosen to be a member of g.) 
        The process
        sequencer(g) maintains a group-specific sequence number sg, which it uses to assign
        increasing and consecutive sequence numbers to the messages that it B-delivers.
        It announces the sequence numbers by B-multicasting order messages to g(see Figure
        15.13 for the details).
        A message will remain in the hold-back queue indefinitely until it can be TO delivered according 
        to the corresponding sequence number.Since the sequence numbers
        are well defined (by the sequencer), the criterion for total ordering is met.
        Furthermore,if the processes use a FIFO-ordered variant of B-multicast, then the totally ordered
        multicast is also causally ordered. We leave the reader to show this.
                public string closeRequest(string id)
                {
                    ServerCli serv; // we gonna make this the primary server;
                    foreach(String url in server.getView().Values)
                    {
                        serv = (ServerCli)Activator.GetObject(typeof(ServerCli), url);
                        serv.receiveSeq(id);
                    }
                    serv = (ServerCli)Activator.GetObject(typeof(ServerCli), primary);
                    serv.sequencer(id);
                    return null;
                }
                public string receiveSeq(String id)
                {
                    closes.Add(id);
                    return null;
                }
                public string shouldGo(String id, int S)
                {
                    if(S == request)
                    {
                    }
                    return null;
                }
                public bool sequencer(string id)
                {
                    seq = seq + 1;
                    return false;
                }
                */


    }
}
