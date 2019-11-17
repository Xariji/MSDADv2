using System;
using Library;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace Client
{
    [Serializable]
    public class Cliente
    {

        private static ISchedulingServer server;

        private User user;
        private String cURL;
        private String sURL;
        private String script;

        //Usage: put as args: <username> <scriptPath>

        public Cliente(String username, String cURL, String sURL, String script)
        {
            this.user = new User(username);
            this.cURL = cURL;
            this.sURL = sURL;
            this.script = script;
        }
        public void start() {

            Uri myUri = new Uri(this.cURL);
            //in progress still
            Console.WriteLine("Cliente " + myUri.Port + " started");

            // error : says that the channel has already bin created with the name 'tcp'
            TcpChannel channel = new TcpChannel(myUri.Port);
            ChannelServices.RegisterChannel(channel, false);

            ClientServ cs = new ClientServ(this.user);
            RemotingServices.Marshal(cs, "cc", typeof(ClientServ));

            server = (ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURL);

        }


        static void Main(string[] args)
        {
            TcpChannel channel;
            Cliente cli;

            string[] vs = args[0].Split(
                    new[] { "'" },
                    StringSplitOptions.None);

            String username = vs[0];
            String cURL = vs[1];
            String sURL = vs[2];
            String script = vs[3];

            cli = new Cliente(username, cURL, sURL, script);
            Uri myUri = new Uri(cURL);

            channel = new TcpChannel(myUri.Port);
            ChannelServices.RegisterChannel(channel, false);

            ClientServ cs = new ClientServ(cli.user);
            RemotingServices.Marshal(cs, "cc", typeof(ClientServ));

            server = (ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURL);

            server.Register(cURL);

            Console.WriteLine("Cliente " + myUri.Port + " (" + username + ") connected to " + server);


            /*
                cli = new Cliente(args[0], "", "", "");
                //connection to the server
                ClientServ Clientserv = new ClientServ(cli.user);
                string url = "tcp://localhost:8000/CC";
                channel = new TcpChannel(8000);
                ChannelServices.RegisterChannel(channel, false);
                RemotingServices.Marshal(Clientserv, "CC", typeof(IClient));

                server = (ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), "tcp://localhost:8888/mcm");
                server.Register(cli.user, url, 8888);

                //identify if normal Cliente
            */

            if (args.Length == 1)
            {
                Boolean run = true;
                Console.WriteLine("Hello, " + cli.GetName() + ". Welcome to MSDAD");
                Console.WriteLine("Type list to list the user's meeting proposals");
                Console.WriteLine("Type create <topic> <# of min participants> <# of slots> <# of invitees> [slots] [invitees] to create a new meeting proposal");
                Console.WriteLine("Type join <meetingTopic> <# of slots> [slots] to join the specific meeting");
                Console.WriteLine("Type close <meetingTopic> to close the specific meeting proposal");
                Console.WriteLine("Type wait <time in milliseconds> to let the Cliente wait");
                Console.WriteLine("Type quit to exit");
                while (run)
                {
                    Console.Write("Insert command: ");
                    string command = Console.ReadLine();
                    if (command.Split(
                        new[] { " " }, StringSplitOptions.None)[0].Equals("quit"))
                    {
                        System.Environment.Exit(1);
                    }
                    else
                    {
                        cli.ProcessConsoleLine(command);
                    }
                }
                //or Cliente-script
            }
            else if (args.Length == 2)
            {
                script = System.IO.File.ReadAllText(args[1]);
                string[] commandList = script.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.None);
                foreach (string command in commandList)
                {
                    cli.ProcessConsoleLine(command);
                }
            }
            else
            {
                System.Console.WriteLine("ERROR: Wrong number of arguments!");
            }

        }


        public String GetName()
        {
            return user.getName();
        }

        /**
         * Lists all the meeting proposals
         */
        public List<MeetingProposal> ListProposals()
        {
            //I think we should return all MP's where a user could join -> return this.user.getMyMP() + this.user.getActiveMP(); //this doesn't work coz its static...
            return server.getJoinableMP(GetName());
        }
         
        /**
         * Creates a proposal
         */
        public void CreateProposal(String topic, int minParticipants, 
            string[] slots, string[] invitees)
        {
            Tuple<Boolean, string> output = server.AddMeetingProposal(topic, minParticipants, slots, invitees, GetName());
            
            if(output.Item1){
                Console.WriteLine("Proposal created with success");
            }
            else
            {
                Console.WriteLine(output.Item2);
            }
            //ShareProposal(mp);
        }

        // this can be to share the proposal we created or the redirect a received proposal
        public void ShareProposal(MeetingProposal mp)
        {
            // We will have to share the proposal among Clientes using Peer-to-Peer or another broadcast
            // algorithm
        }

        public void Participate(String meetingTopic, string[] slots)
        {
            // we will have to signal the server our intent to participate in this meeting
            // should we keep a record on the meetings we are participating in ?
            List<Slot> slotsList = new List<Slot>();
            foreach (string slotUnformat in slots)
            {
                string[] slotFormat = slotUnformat.Split(
                   new[] { ";" },
                   StringSplitOptions.None);
                MeetingLocation meetingLocation = null;
                foreach (MeetingLocation ml in server.GetAvailableMeetingLocations())
                {
                    if (ml.getName() == slotFormat[0])
                    {
                        meetingLocation = ml;
                        slotsList.Add(new Slot(meetingLocation, slotFormat[1]));
                    }
                }
                if (slotsList.Count == 0)
                {
                    Console.WriteLine("Join of Meeting Proposal declined. Invalid location.");
                    return;
                }
                
            }
            Tuple<Boolean, int> output = server.AddUserToProposal(meetingTopic, GetName(), slotsList);
            if (output.Item1)
            {
                Console.WriteLine("Meeting " + meetingTopic + " joined successfully.");
            } else
            {
                switch (output.Item2)
                {
                    case 0:
                        Console.WriteLine("Joing meeting " + meetingTopic + " failed. Topic not found.");
                        break;
                    case 1:
                        Console.WriteLine("Joing meeting " + meetingTopic + " failed. Meeting is restricted.");
                        break;
                }
            }
        }

        public void CloseProposal(String meetingTopic)
        {
            List<String> output = server.CloseMeetingProposal(meetingTopic, GetName());
            foreach(String s in output)
            {
                Console.WriteLine(s);
            }
        }

        public void AddLocation(string location)
        {
            server.AddMeetingLocation(location);
        }

        public void AddRoom(string location, string room, int capacity)
        {
            server.AddMeetingRoom(location, room, capacity);
        }

        private void ProcessConsoleLine(string line)
        {
            string[] commandArgs = line.Split(
                   new[] { " " },
                   StringSplitOptions.None);

            switch (commandArgs[0].ToLower())
            {
                case "list":
                    //list all available meetings
                    List<MeetingProposal> list = ListProposals();
                    foreach (MeetingProposal proposal in list)
                    {
                        System.Console.WriteLine(proposal.ToString());
                    }
                    break;
                case "create":
                    int nSlots = Int32.Parse(commandArgs[3]);
                    int nInvitees = Int32.Parse(commandArgs[4]);
                    string[] slots = new string[nSlots];
                    for (int i = 0; i < nSlots; i++)
                    {
                        slots[i] = commandArgs[5 + i];
                    }
                    string[] invitees = new string[nInvitees];
                    for (int i = 0; i < nInvitees; i++)
                    {
                        invitees[i] = commandArgs[5 + nSlots + i];
                    }
                    CreateProposal(commandArgs[1], Int32.Parse(commandArgs[2]), slots, invitees);
                    break;
                case "join":
                    slots = new string[Int32.Parse(commandArgs[2])];
                    for (int i = 0; i < slots.Length; i++)
                    {
                        slots[i] = commandArgs[3 + i];
                    }
                    Participate(commandArgs[1], slots);
                    break;
                case "close":
                    CloseProposal(commandArgs[1]);
                    break;
                case "wait":
                    System.Threading.Thread.Sleep(Int32.Parse(commandArgs[1]));
                    break;
                case "addLocation":
                    AddLocation(commandArgs[1]);
                    break;
                case "addRoom":
                    AddRoom(commandArgs[1], commandArgs[2], Int32.Parse(commandArgs[3]));
                    break;
                default:
                    System.Console.WriteLine("ERROR: " + commandArgs[0] + " is an unknown command!");
                    break;
            }
        }
    }
}
