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
            this.view = new SortedList<String, String>();
        }

        public void start() {

            Uri myUri = new Uri(URL);

            TcpChannel channel = new TcpChannel(myUri.Port);
            ChannelServices.RegisterChannel(channel, false);

            SchedulingServer server = new SchedulingServer(id, URL, maxFaults, minDelay, maxDelay);
            ServerCli mo = new ServerCli(server);
            RemotingServices.Marshal(mo, "mcm", typeof(ServerCli));
            Console.WriteLine("Server " + this.id +" started on Port " + myUri.Port);
            System.Console.ReadLine();

        }

        public static void Main(string[] args)
        {

            string[] vs = args[0].Split(
                   new[] { "'" },
                   StringSplitOptions.None);

            String id = vs[0];
            String URL = vs[1];
            int maxFaults = Int32.Parse(vs[2]);
            int minDelay = Int32.Parse(vs[3]);
            int maxDelay = Int32.Parse(vs[4]);

            Uri myUri = new Uri(URL);

            TcpChannel channel = new TcpChannel(myUri.Port);
            ChannelServices.RegisterChannel(channel, false);

            SchedulingServer server = new SchedulingServer(id, URL, maxFaults, minDelay, maxDelay);
            ServerCli mo = new ServerCli(server);
            RemotingServices.Marshal(mo, "mcm", typeof(ServerCli));

            HttpChannel channel1 = new HttpChannel(myUri.Port+1000);
            ChannelServices.RegisterChannel(channel1, false);

            PuppetServer ps = new PuppetServer(mo); //testing this
            RemotingServices.Marshal(ps, "ps", typeof(PuppetServer)); // testing this

            Console.WriteLine("Server " + id + " started on Port " + myUri.Port);
            System.Console.ReadLine();

        }

        public string GetId()
        {
            return id;
        }

        public int getMinDelay()
        {
            return minDelay;
        }

        public int getMaxDelay()
        {
            return maxDelay;
        }

        public int getMaxFaults()
        {
            return maxFaults;
        }

        public String getURL()
        {
            return URL;
        }

        public String[] getBackupServer()
        {
            if(view.Count < 3)
            {
                if (view.IndexOfKey(id) < view.Count - 1)
                {
                    return new String[]{view.Values[view.IndexOfKey(id) + 1]};
                }
                else
                {
                    return new String[]{view.Values[0]};
                }
            }
            else
            {
                String[] result = new String[view.Count / 2 + 1];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = view.Values[((view.IndexOfKey(id) + i + 1) % view.Count + view.Count) % view.Count];
                }
                return result;
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
