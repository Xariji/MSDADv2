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
        private static ClientServ cs;

        private String username;
        private String cURL;
        private String sURL;
        private String script;
        private String[] sURLBackup;

        //Usage: put as args: <username> <scriptPath>

        public Cliente(String username, String cURL, String sURL, String script)
        {
            this.username = username;
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

            cs = new ClientServ(this);
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
            String script = "";
            String[] sURLBackup;

            if(args.Length > 1)
            {
                script = args[1];
            }

            cli = new Cliente(username, cURL, sURL, script);
            Uri myUri = new Uri(cURL);

            channel = new TcpChannel(myUri.Port);
            ChannelServices.RegisterChannel(channel, false);

            cs = new ClientServ(cli);
            RemotingServices.Marshal(cs, "cc", typeof(ClientServ));

            server = (ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURL);

            List<String> arg = new List<String>();
            arg.Add(cURL);
            Message mess = server.Response("Register", arg);
            Console.WriteLine(mess.getMessage());  

            sURLBackup = Array.ConvertAll((object[]) mess.getObj(), Convert.ToString);
            String backupInfo = ((ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURLBackup[0])).Response("GetServerId", null).getMessage();
            for(int i=1; i<sURLBackup.Length; i++)
            {
                backupInfo += ", " + ((ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURLBackup[i])).Response("GetServerId", null).getMessage();
            }
            Console.WriteLine("Cliente " + myUri.Port + " (" + username + ") connected to " + server.Response("GetServerId", null).getMessage());


            if (args.Length == 1 || args.Length == 2)
            {
                Boolean run = true;
                Console.WriteLine("Hello, " + cli.GetName() + ". Welcome to MSDAD");
                Console.WriteLine("Type list to list the user's meeting proposals");
                Console.WriteLine("Type create <topic> <# of min participants> <# of slots> <# of invitees> [slots] [invitees] to create a new meeting proposal");
                Console.WriteLine("Type join <meetingTopic> <# of slots> [slots] to join the specific meeting");
                Console.WriteLine("Type close <meetingTopic> to close the specific meeting proposal");
                Console.WriteLine("Type wait <time in milliseconds> to let the Cliente wait");
                Console.WriteLine("Type quit to exit");

                //if script client
                if (args.Length == 2)
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
            }
            else
            {
                System.Console.WriteLine("ERROR: Wrong number of arguments!");
            }

        }


        public String GetName()
        {
            return username;
        }

        /**
         * Lists all the meeting proposals
         */
        public List<MeetingProposal> ListProposals()
        {
            return cs.getUser().getMyMP(); 
            // Not working now, we just need to know the proposals we created like locally and the other
                         // that others users tell us
        }
         
        /**
         * Creates a proposal
         */
        public void CreateProposal(String topic, int minParticipants, String[] slots, String[] invitees)
        {
            //Tuple<Boolean, string> output = server.AddMeetingProposal(topic, minParticipants, slots, invitees, GetName());

            List<String> args = new List<String>();
            args.Add(GetName());
            args.Add(topic);
            args.Add(minParticipants.ToString());
            args.Add(slots.Length.ToString());
            Array.ForEach(slots, args.Add);
            args.Add(invitees.Length.ToString());
            Array.ForEach(invitees, args.Add);

            try
            {
                Message output = server.Response("AddMeetingProposal", args);
                if (output.getSucess())
                {
                    Console.WriteLine("Proposal created with success");
                    ShareProposal((MeetingProposal) output.getObj());
                }
                else
                {
                    Console.WriteLine(output.getMessage());
                }
            }
            catch (Exception e)
            {
                if(connectToBackup(0, new List<string>()))
                {
                    CreateProposal(topic, minParticipants, slots, invitees);
                }
            } 
        }

        // this can be to share the proposal we created or the redirect a received proposal
        public void ShareProposal(MeetingProposal mp)
        {
            Console.WriteLine("Share Proposal: " + mp.getMPId());
            List<String> args = new List<String>();
            List<string> listURLs = (List<string>) server.Response("GetSharedClientsList", args).getObj();
            foreach(string url in listURLs){
                if(url != cURL){
                    Console.WriteLine("Connect to client: " + url);
               
                    ClientServ c = (ClientServ)Activator.GetObject(typeof(ClientServ), url);
                    Console.WriteLine("Send proposal");
                    c.receiveProposal(mp);
                }
            }
        }

        public void receiveProposal(MeetingProposal mp){
           Console.WriteLine("Receive proposal");

            Boolean found = false;
            Console.WriteLine(cs.getUser().getName());

            //validate if the client already as the proposal

            foreach(MeetingProposal m in cs.getUser().getMyMP()){
                Console.WriteLine("Procurando");
                if(m.getMPId() == mp.getMPId()){
                    Console.WriteLine("Busted");
                    found = true;
                }
            }
            Console.WriteLine("Validated");

            if(!found){
                Console.WriteLine("Add");
                //add the proposal
                cs.getUser().addMyMP(mp);
                //share it
                Console.WriteLine("Partilhar: "  + mp.getMPId());
                ShareProposal(mp);
            }

        }

        public void Participate(String meetingTopic, String[] slots)
        {

            if (slots == null)
            {
                Console.WriteLine("No slots input");
                return;
            }

            //Tuple<Boolean, int> output = server.AddUserToProposal(meetingTopic, GetName(), slots);

            List<String> args = new List<String>();
            args.Add(meetingTopic);
            args.Add(slots.ToString());
            args.Add(GetName());
            Message output = server.Response("AddUserToProposal", args);

            if (output.getSucess())
            {
                Console.WriteLine("Meeting " + meetingTopic + " joined successfully.");
            } else
            {
                switch (output.getObj())
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
            //List<String> output = server.CloseMeetingProposal(meetingTopic, GetName());
            List<String> args = new List<String>();
            args.Add(GetName());

            Message output = server.Response("CloseMeetingProposal", args);
            List<String> messages = (List<String>) output.getObj();

            foreach (String s in messages)
            {
                Console.WriteLine(s);
            }
        }

 

        public String[] getBackupServerURL()
        {
            return sURLBackup;
        }

        public void setBackupServerURL(String[] urls)
        {
            sURLBackup = urls;
        }

        public Boolean connectToBackup(int index, List<String> args)
        {
            Console.WriteLine("Connection to Server lost. Trying to reconnect...");
            try
            {
                args.Add(sURL);
                server = (ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURLBackup[index]);
                server.Response("RemoveServerFromView", args).getMessage();
                sURL = sURLBackup[index];

                List<String> arg = new List<String>();
                arg.Add(cURL);
                Message mess = server.Response("Register", arg);
                Console.WriteLine(mess.getMessage());

                sURLBackup = Array.ConvertAll((object[])mess.getObj(), Convert.ToString);
                String backupInfo = ((ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURLBackup[0])).Response("GetServerId", null).getMessage();
                for (int i = 1; i < sURLBackup.Length; i++)
                {
                    backupInfo += ", " + ((ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURLBackup[i])).Response("GetServerId", null).getMessage();
                }
                Uri myUri = new Uri(cURL);
                Console.WriteLine("Cliente " + myUri.Port + " (" + username + ") connected to " + server.Response("GetServerId", null).getMessage());
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("Connect BU: " + e.Message);
                foreach(String s in args)
                {
                    Console.WriteLine(s);
                }
                try
                {
                    if(index + 1 < sURLBackup.Length)
                    {
                        connectToBackup(index + 1, args);
                    }
                    else
                    {
                        Console.WriteLine("Error: No Backup-server reachable!");
                        return false;
                    }
                }
                catch (Exception e2)
                {
                    Console.WriteLine("Connect BU2: " + e2.Message);
                    return false;
                }
            }
            return false;
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
                default:
                    System.Console.WriteLine("ERROR: " + commandArgs[0] + " is an unknown command!");
                    break;
            }
        }

        public string getURL(){
            return cURL;
        }
    }
}
