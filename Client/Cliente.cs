using System;
using Library;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Threading.Tasks;

namespace Client
{
    [Serializable]
    public class Cliente
    {

        private static ISchedulingServer server;
        private static ClientServ cs;

        private static int timeout = 20000;

        private String username;
        private String cURL;
        private String sURL;
        private String script;
        private String[] sURLBackup;
        private List<String> localClients;

        //Usage: put as args: <username> <scriptPath>

        public Cliente(String username, String cURL, String sURL, String script)
        {
            this.username = username;
            this.cURL = cURL;
            this.sURL = sURL;
            this.script = script;
            //this.myProposals = new List<MeetingProposal>();
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
            String script = "" ; 
            String[] sURLBackup;
            List<String> localClients = new List<String>();

            if (args.Length > 1)
            {
                script = args[1];
            }

            cli = new Cliente(username, cURL, sURL, script);
            Uri myUri = new Uri(cURL);

            channel = new TcpChannel(myUri.Port);
            ChannelServices.RegisterChannel(channel, false);

            cs = new ClientServ(cli);

            //RemotingServices.Marshal(cs, "cc", typeof(ClientServ));

            RemotingServices.Marshal(cs, myUri.Segments[1], typeof(ClientServ));
            

            server = (ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURL);

            List<String> arg = new List<String>();
            arg.Add(cURL);
            Message mess = null; // = server.Response("Register", arg); //it should wait but it should be a task!

            try
            {
                Task<Message> task = Task<Message>.Factory.StartNew(() => server.Response("Register", arg));
                task.Wait();
                mess = task.Result;
            }
            catch (Exception e)
            {
                //Should we give here another server for the Client to connect?
                Console.WriteLine("The server you tried to connect unfortunately is not available");
                Console.WriteLine("Please close window and try to connect to another server adress");
                Console.ReadLine();
            }

            sURLBackup = Array.ConvertAll((object[])mess.getObj(), Convert.ToString);
            Console.WriteLine("Cliente " + new Uri(cURL).Port + " (" + username + ") " + mess.getMessage());

            if (args.Length == 1 || args.Length == 2)
            {
                Boolean run = true;
                Console.WriteLine("Hello, " + username + ". Welcome to MSDAD");
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
                    Console.WriteLine("If you wish the script to be run stpe-by-step write YES");
                    String s = Console.ReadLine();
                    if (s.Equals("YES")){
                        foreach (string command in commandList)
                        {
                            cli.ProcessConsoleLine(command);
                            Console.WriteLine("Press Enter to execute next line");
                            Console.ReadLine();
                        }
                    }
                    else
                    {
                        foreach (string command in commandList)
                        {
                            cli.ProcessConsoleLine(command);
                        }
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


        public string GetName()
        {
            return this.username; 
        }


        public string getURL()
        {
            return this.cURL; ;
        }

        public string getSURL()
        {
            return this.sURL;
        }


        /**
         * Lists all the meeting proposals
         */
        public List<MeetingProposal> ListProposals()
        {
            return cs.getMyProp(); 
        }

        /**
         * Creates a proposal
         */
        private void CreateProposal(String topic, int minParticipants, String[] slots, String[] invitees)
        {
            List<String> args = new List<String>();
            args.Add(cs.getUser().getName());
            args.Add(topic);
            args.Add(minParticipants.ToString());
            args.Add(slots.Length.ToString());
            Array.ForEach(slots, args.Add);
            args.Add(invitees.Length.ToString());
            Array.ForEach(invitees, args.Add);
            Message output;
            try
            {
                Task<Message> task = Task<Message>.Factory.StartNew(() => server.Response("AddMeetingProposal", args));
                bool taskCompleted = task.Wait(timeout);

                if (taskCompleted)
                {
                    output = task.Result;

                    //Message output = server.Response("AddMeetingProposal", args);
                    if (output.getSucess())
                    {
                        // receives the created MP and adds it we later need to add to the proposals the ones we were invited to
                        Console.WriteLine("Proposal created with success");
                        ShareProposal((MeetingProposal)output.getObj());
                    }
                    else
                    {
                        Console.WriteLine(output.getMessage());
                    }

                    return;
                }
            }
            
            catch (Exception e) // we should specify the exceptions we get ( Is this the conection Exception ? )
            {
                if (connectToBackup(0, new List<string>()))
                {
                    CreateProposal(topic, minParticipants, slots, invitees);
                }
                return;
            }

                Console.WriteLine("Request: Timeout, abort request.");
        }

        // this can be to share the proposal we created or the redirect a received proposal
        //TODO We have to make all calls to the Server fail proof
        public void ShareProposal(MeetingProposal mp)
        {
            List<String> args = new List<String>();
            List<string> listURLs = (List<string>) server.Response("GetSharedClientsList", args).getObj();


            foreach(string url in listURLs){
                if(url != cURL){               
                    ClientServ c = (ClientServ)Activator.GetObject(typeof(ClientServ), url);
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
                if(m.getMPTopic().Equals(mp.getMPTopic())){
                    found = true;
                }
            }

            if(!found){
                //add the proposal
                cs.getUser().addMyMP(mp);
                //share it with the other clients
                ShareProposal(mp);
            }

        }

        private void Participate(String meetingTopic, String[] slots)
        {

            if (slots == null)
            {
                Console.WriteLine("No slots input");
                return;
            }

            //look for the server that created that meeting proposal
            ISchedulingServer otherServer = findOriginServer(meetingTopic);

            List<String> args = new List<String>();
            args.Add(meetingTopic);
            args.Add(cs.getUser().getName());
            args.Add(string.Join(" ", slots));

            Message output;

            try
            {
                Task<Message> task = Task<Message>.Factory.StartNew(() => otherServer.Response("AddUserToProposal", args));
                bool taskCompleted = task.Wait(timeout);

                if (taskCompleted)
                {
                    output = task.Result;
                    //Message output = server.Response("AddUserToProposal", args);
                    if (output.getSucess())
                    {
                        Console.WriteLine("Meeting " + meetingTopic + " joined successfully.");
                    }
                    else
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
                    return;
                }
            }
            catch (Exception e)
            {
                if (connectToBackup(0, new List<string>()))
                {
                    Participate(meetingTopic, slots);
                }
                return;
            }
            Console.WriteLine("Request: Timeout, abort request.");
        }

        private void CloseProposal(String meetingTopic)
        {

            //look for the server that created that meeting proposal
            ISchedulingServer otherServer = findOriginServer(meetingTopic);

            List<String> args = new List<String>();
            args.Add(meetingTopic);
            args.Add(cs.getUser().getName());
            Message output;
            try
            {
                Task<Message> task = Task<Message>.Factory.StartNew(() => otherServer.Response("CloseMeetingProposal", args));
                bool taskCompleted = task.Wait(timeout);

                if (taskCompleted)
                {
                    output = task.Result;
                    //Message output = server.Response("CloseMeetingProposal", args);
                    List<String> messages = (List<String>)output.getObj();
                }
            }
            catch (Exception e)
            {
                if (connectToBackup(0, new List<string>()))
                {
                    CloseProposal(meetingTopic);
                }
                return;
            }

            Console.WriteLine("Request: Timeout, abort request.");
        }



        public String[] getBackupServerURL()
        {
            return sURLBackup;
        }

        public void setBackupServerURL(String[] urls)
        {
            sURLBackup = urls;
        }

        //TODO we have to make all calls to the server fail proof, both from freezes and crashes
        public Boolean connectToBackup(int index, List<String> args)
        {
            Boolean _return = true;
            Console.WriteLine("Connection to Server lost. Trying to reconnect...");
            try
            {
                args.Add(sURL);
                this.sURL = sURLBackup[index];
                server = (ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURLBackup[index]);
                //server.Response("RemoveServerFromView", args).getMessage();

                Task<Message> task = Task<Message>.Factory.StartNew(() => server.Response("RemoveServerFromView", args));
                task.Wait();

                List<String> arg = new List<String>();
                arg.Add(cURL);
                Message mess; // = server.Response("Register", arg);

                task = Task<Message>.Factory.StartNew(() => server.Response("Register", arg)); // should we send an error here ?
                task.Wait();
                mess = task.Result;

                sURLBackup = Array.ConvertAll((object[])mess.getObj(), Convert.ToString);
                Console.WriteLine("Cliente " + new Uri(cURL).Port + " (" + username + ") " + mess.getMessage());

                //coordinate local clients to reconnect to different backup servers
                int pointer = index + 1 % sURLBackup.Length;
                foreach (string url in localClients)
                {
                    if (url != cURL)
                    {
                        ClientServ c = (ClientServ)Activator.GetObject(typeof(ClientServ), url);
                        c.connectToBackup(pointer % sURLBackup.Length, new List<String>());
                        pointer++;
                    }
                }

                _return = true;
            }
            catch(Exception e)
            {
                try
                {
                    if(index + 1 < sURLBackup.Length)
                    {
                        connectToBackup(index + 1, args);
                    }
                    else
                    {
                        Console.WriteLine("Error: No Backup-server reachable!");
                        _return = false;
                    }
                }
                catch (Exception e2)
                {
                    _return = false;
                }
            }
            return _return;
        }

        public void updateLocalClients()
        {
            //TODO what is this server crashes or wtv
            //localClients = (List<String>)server.Response("getClientURLs", null).getObj();
            Task<object> task = Task<object>.Factory.StartNew(() => server.Response("getClientURLs", null).getObj()); // should we send an error here ?
            task.Wait();
            localClients = (List<String>)task.Result;
        }

        private ISchedulingServer findOriginServer(string meetingTopic){

            ISchedulingServer result = null;
            List<string> auxArgs = new List<string>();
            auxArgs.Add(meetingTopic);
            Message urlMess; // = server.Response("GetMeetingProposalURL", auxArgs); //TODO
            Task<Message> task = Task<Message>.Factory.StartNew(() => server.Response("GetMeetingProposalURL", auxArgs));
            task.Wait();
            urlMess = task.Result;

            if (urlMess.getSucess()){
                result = (ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), (string)urlMess.getObj());
            } else {
                Console.WriteLine(urlMess.getMessage());
            }

            return result;
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
    }
}
