using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;
using Library;

namespace Server
{

    public class SchedulingServer
    {
       
        String id;
        String URL;
        int port;
        int maxFaults;
        int minDelay;
        int maxDelay;
        SortedList<String, String> view;

        public SchedulingServer(String id, String URL, int maxFaults, int minDelay, int maxDelay)
        {
            this.id = id;
            this.URL = URL;
            this.maxFaults = maxFaults;
            this.minDelay = minDelay;
            this.maxDelay = maxDelay;
        }

        public void start() {

            Uri myUri = new Uri(URL);
            Console.WriteLine("Server " + myUri.Port + " started");

            TcpChannel channel = new TcpChannel(myUri.Port);
            ChannelServices.RegisterChannel(channel, false);

            SchedulingServer server = new SchedulingServer(id, URL, maxFaults, minDelay, maxDelay);
            ServerCli mo = new ServerCli(server);
            RemotingServices.Marshal(mo, "mcm", typeof(ServerCli));
            Console.WriteLine("Server " + this.id +" started");
            System.Console.ReadLine();

        }

        public static void Main(string[] args)
        {
            /*
            TcpChannel channel = new TcpChannel(8888);
            ChannelServices.RegisterChannel(channel, false);

            ServerCli mo = new ServerCli();
            RemotingServices.Marshal(mo, "mcm", typeof(ServerCli));
            Console.WriteLine("Server started");
            System.Console.ReadLine();
            */

            string[] vs = args[0].Split(
                   new[] { "'" },
                   StringSplitOptions.None);

            String id = vs[0];
            String URL = vs[1];
            int maxFaults = Int32.Parse(vs[2]);
            int minDelay = Int32.Parse(vs[3]);
            int maxDelay = Int32.Parse(vs[4]);

            Uri myUri = new Uri(URL);
            System.Console.WriteLine("Server " + myUri.Port + " started");

            TcpChannel channel = new TcpChannel(myUri.Port);
            ChannelServices.RegisterChannel(channel, false);

            SchedulingServer server = new SchedulingServer(id, URL, maxFaults, minDelay, maxDelay);
            ServerCli mo = new ServerCli(server);
            RemotingServices.Marshal(mo, "mcm", typeof(ServerCli));

            HttpChannel channel1 = new HttpChannel(myUri.Port+1000);
            ChannelServices.RegisterChannel(channel1, false);

            PuppetServer ps = new PuppetServer(mo); //testing this
            RemotingServices.Marshal(ps, "ps", typeof(PuppetServer)); // testing this

            Console.WriteLine("Server " + id + " started");
            System.Console.ReadLine();

        }

        public String GetId()
        {
            return id;
        }

        public String getBackupServer()
        {
            if(view.IndexOfKey(id) < view.Count - 1)
            {
                return view.Values[view.IndexOfKey(id) + 1];
            } else
            {
                return view.Values[0];
            }
            
        }

        public SortedList<String, String> getView()
        {
            return view;
        }

        public void updateView(String action, String serverid, String serverurl)
        {
            lock (view)
            {
                switch (action)
                {
                    case "add":
                        view.Add(serverid, serverurl);
                        break;
                    case "remove":
                        view.Remove(serverid);
                        break;
                }
            }
        }

    }

}
