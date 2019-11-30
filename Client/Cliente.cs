﻿using System;
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

        private String username;
        private String cURL;
        private String sURL;
        private String script;
        private String[] sURLBackup;

        private List<MeetingProposal> myProposals;

        //Usage: put as args: <username> <scriptPath>

        public Cliente(String username, String cURL, String sURL, String script)
        {
            this.username = username;
            this.cURL = cURL;
            this.sURL = sURL;
            this.script = script;
            this.myProposals = new List<MeetingProposal>();
        }
        public void start() {

            Uri myUri = new Uri(this.cURL);
            //in progress still
            Console.WriteLine("Cliente " + myUri.Port + " started");

            // error : says that the channel has already bin created with the name 'tcp'
            TcpChannel channel = new TcpChannel(myUri.Port);
            ChannelServices.RegisterChannel(channel, false);

            ClientServ cs = new ClientServ(this);
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

            ClientServ cs = new ClientServ(cli);
            RemotingServices.Marshal(cs, "cc", typeof(ClientServ));

            server = (ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURL);

            List<String> arg = new List<String>();
            arg.Add(cURL);
            Message mess = server.Response("Register", arg);
            sURLBackup = Array.ConvertAll((object[])mess.getObj(), Convert.ToString);
            Console.WriteLine("Cliente " + new Uri(cURL).Port + " (" + username + ") " + mess.getMessage());

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
         * Creates a proposal
         */
        private void CreateProposal(String topic, int minParticipants, String[] slots, String[] invitees)
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
                    this.myProposals.Add((MeetingProposal) output.getObj()); // receives the created MP and adds it we later need to add to the proposals the ones we were invited to
                    Console.WriteLine("Proposal created with success");
                }
                else
                {
                    Console.WriteLine(output.getMessage());
                }
                //ShareProposal(mp);
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
        private void ShareProposal(MeetingProposal mp)
        {
            // We will have to share the proposal among Clientes using Peer-to-Peer or another broadcast
            // algorithm
        }

        private void Participate(String meetingTopic, String[] slots)
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

            try
            {
                Message output = server.Response("AddUserToProposal", args);
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
            }
            catch (Exception e)
            {
                if(connectToBackup(0, new List<string>()))
                {
                    Participate(meetingTopic, slots);
                }
            }
        }

        private void CloseProposal(String meetingTopic)
        {
            //List<String> output = server.CloseMeetingProposal(meetingTopic, GetName());
            List<String> args = new List<String>();
            args.Add(GetName());

            try
            {
                Message output = server.Response("CloseMeetingProposal", args);
                List<String> messages = (List<String>)output.getObj();

                foreach (String s in messages)
                {
                    Console.WriteLine(s);
                }
            }
            catch (Exception e)
            {
                if (connectToBackup(0, new List<string>()))
                {
                    CloseProposal(meetingTopic);
                }
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

        private Boolean connectToBackup(int index, List<String> args)
        {
            Console.WriteLine("Connection to Server lost. Trying to reconnect...");
            try
            {
                args.Add(sURL);
                sURL = sURLBackup[index];
                server = (ISchedulingServer)Activator.GetObject(typeof(ISchedulingServer), sURLBackup[index]);
                server.Response("RemoveServerFromView", args).getMessage();

                List<String> arg = new List<String>();
                arg.Add(cURL);
                Message mess = server.Response("Register", arg);
                sURLBackup = Array.ConvertAll((object[])mess.getObj(), Convert.ToString);
                Console.WriteLine("Cliente " + new Uri(cURL).Port + " (" + username + ") " + mess.getMessage());

                return true;
            }
            catch(Exception e)
            {
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
            
                        foreach (MeetingProposal proposal in this.myProposals)
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
