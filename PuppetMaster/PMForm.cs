using Client;
using PCS;
using Server;
using Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class PMForm : Form
    {

        private static int port = 10001;

        Dictionary<String, String> urlServers = new Dictionary<string, string>();

        Dictionary<String, String> urlClients = new Dictionary<string, string>();

        Dictionary<String, Process> sProcesses = new Dictionary<string, Process>();

        private string scriptPath = "";
        private string puppScript = "";



        int serverNo = 1, clientNo = 1;

        List<MeetingLocation> meetingLocations = new List<MeetingLocation>();

        public PMForm()
        {
            InitializeComponent();
            addServerId.Text = "server" + serverNo.ToString().PadLeft(2, '0');
            addURL.Text = "tcp://localhost:80" + serverNo.ToString().PadLeft(2, '0') +"/mcm";
            username.Text = "user" + clientNo.ToString().PadLeft(2, '0');
            clientURL.Text = "tcp://localhost:60" + clientNo.ToString().PadLeft(2, '0') + "/cc";

            locationNameNew.Visible = false;
        }

        // We gonna use the PCS here to create a Server
        private void addServerButton_Click(object sender, EventArgs e)
        {
            String serverID = addServerId.Text;
            String URL = addURL.Text;
            int maxFaults = Int32.Parse(addMaxFaults.Text);
            int minDelay = Int32.Parse(addMinDelay.Text);
            int maxDelay = Int32.Parse(addMaxDelay.Text);

            addServer(serverID, URL, maxFaults, minDelay, maxDelay);
        }

        private void addServer(string serverID, string URL, int maxFaults, int minDelay, int maxDelay) { 

            PCSService pCs;
            Uri myUri = new Uri(URL);
            IPAddress[] hostIPs = Dns.GetHostAddresses(myUri.Host);
            Task task;


            // get local IP addresses
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            // test if any host IP is a loopback IP or is equal to any local IP


            if (hostIPs.Any(hostIP => IPAddress.IsLoopback(hostIP) || localIPs.Contains(hostIP))) // we can directly create the server because is 
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Directory.GetCurrentDirectory() + @"..\..\..\..\Server\bin\Debug\Server",
                        Arguments = serverID + "'" + URL + "'" + maxFaults + "'" + minDelay + "'" + maxDelay,

                    }
                };

                process.Start();
                sProcesses.Add(serverID, process);
            }
            else 
            {
                pCs = (PCSService)Activator.GetObject(typeof(PCSService), myUri.Scheme + Uri.SchemeDelimiter + myUri.Host + ":10000");
                task = Task.Factory.StartNew(() => pCs.createServerProcess(serverID, URL, maxFaults, minDelay, maxDelay));
                task.Wait();

                
            }
            String psURLHost = myUri.Host;
            int psURLPort = myUri.Port + 1000;
            Console.WriteLine("serverID: " + serverID + " URL: " + URL);
            PuppetServer ps = (PuppetServer)Activator.GetObject(typeof(PuppetServer), "http://" + psURLHost + ":" + psURLPort + "/ps");
            foreach(KeyValuePair<String, String> server in urlServers)
            {
                task = Task.Factory.StartNew(() => ps.initializeView(server.Key, server.Value));
                task.Wait();
               
            }

            urlServers.Add(serverID, URL);
            task = Task.Factory.StartNew(() => ps.addServerToView(serverID, URL));
            task.Wait();
            serverNo++;
            addServerId.Text = "server" + serverNo.ToString().PadLeft(2, '0');
            addURL.Text = "tcp://localhost:80" + serverNo.ToString().PadLeft(2, '0') + "/mcm";
        }

        private void addClient_Click(object sender, EventArgs e)
        {
            String userName = username.Text;
            String cURL = clientURL.Text;
            String sURL = serverURL.Text;

            addCli(userName, cURL, sURL);
        }

        private void addCli(string userName, string cURL, string sURL) { 
            PCSService pCs;
            Uri myUri = new Uri(cURL);
            IPAddress[] hostIPs = Dns.GetHostAddresses(myUri.Host);



            // get local IP addresses
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

            // test if any host IP is a loopback IP or is equal to any local IP
            if (hostIPs.Any(hostIP => IPAddress.IsLoopback(hostIP) || localIPs.Contains(hostIP))) // we can directly create the server because is 
            {

                Process process;
                if(scriptPath != "")
                {
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = Directory.GetCurrentDirectory() + @"..\..\..\..\Client\bin\Debug\Client",
                            Arguments = userName + "'" + cURL + "'" + sURL + " " + scriptPath,

                        }
                    };
                }
                else
                {
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = Directory.GetCurrentDirectory() + @"..\..\..\..\Client\bin\Debug\Client",
                            Arguments = userName + "'" + cURL + "'" + sURL,

                        }
                    };
                }
                
                process.Start();

            }
            else // its not local
            {
                pCs = (PCSService)Activator.GetObject(typeof(PCSService), myUri.Scheme + Uri.SchemeDelimiter + myUri.Host + ":10000/pcs");
                Task task = Task.Factory.StartNew(() => pCs.createClientProcess(userName, cURL, sURL, scriptPath));
                task.Wait();

               
            }

            urlClients.Add(userName, cURL);

            clientNo++;
            username.Text = "user" + clientNo.ToString().PadLeft(2, '0');
            clientURL.Text = "tcp://localhost:60" + clientNo.ToString().PadLeft(2, '0') + "/cc";
        }

        private void selectScript_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            DialogResult result = openFileDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK)
            {
                scriptPath = openFileDialog.FileName;
                selectScript.Text = openFileDialog.FileName.Split('\\').Last();
            } else if (result == DialogResult.Cancel)
            {
                scriptPath = "";
                selectScript.Text = "Select script";
            }
        }

        // This method is used to crash a server 
        private void button5_Click(object sender, EventArgs e)
        {
            String serverID = crashID.Text;
            Kill(serverID);
            
        }
        private void Kill(String serverID)
            {
            if (sProcesses.TryGetValue(serverID, out Process pr))
            {
                pr.Kill();

                sProcesses.Remove(serverID);
                urlServers.Remove(serverID);
            }
            else // if it's not local we have to call PCS
            {
                urlServers.TryGetValue(serverID, out string sURL);

                Uri myUri = new Uri(sURL);
                PCSService pCs = (PCSService)Activator.GetObject(typeof(PCSService), myUri.Scheme + Uri.SchemeDelimiter + myUri.Host + ":10000");
                Task task = Task.Factory.StartNew(() => pCs.crashServer(serverID));
                task.Wait();

                ;
            }
        }
        
        private void Freeze_Click(object sender, EventArgs e)
        {
            String servID = freezeID.Text;
            Freez(servID);
        }

        private void Freez(string servID)
        {
            urlServers.TryGetValue(servID, out string url);

            Uri myUri = new Uri(url);

            String psURLHost = myUri.Host;
            int psURLPort = myUri.Port + 1000;

            PuppetServer ps = (PuppetServer)Activator.GetObject(typeof(PuppetServer), "http://" + psURLHost + ":" + psURLPort + "/ps");

            Task task = Task.Factory.StartNew(() => ps.freeze());
            task.Wait();
            
        }

        private void getStatus_Click(object sender, EventArgs e)
        {
            getStat();
        }

        private void getStat()
        {
            foreach (String url in urlServers.Values)
            {
                Uri myUri = new Uri(url);

                String psURLHost = myUri.Host;
                int psURLPort = myUri.Port + 1000;
                PuppetServer ps = (PuppetServer)Activator.GetObject(typeof(PuppetServer), "http://" + psURLHost + ":" + psURLPort + "/ps");
                Task task = Task.Factory.StartNew(() => ps.status());
                task.Wait();
            }
        }
        

        private void Unfreeze_Click(object sender, EventArgs e)
        {
            String servID = unfreezeID.Text;
            Unfreez(servID);

        }
        
        private void Unfreez(string servID)
        {
            urlServers.TryGetValue(servID, out String url);

            Uri myUri = new Uri(url);

            String psURLHost = myUri.Host;
            int psURLPort = myUri.Port + 1000;

            PuppetServer ps = (PuppetServer)Activator.GetObject(typeof(PuppetServer), "http://" + psURLHost + ":" + psURLPort + "/ps");
            Task task = Task.Factory.StartNew(() => ps.unfreeze());
            task.Wait();
        }

        private void locationNameNew_Enter(object sender, EventArgs e)
        {
            if (locationNameNew.Text == "Name of location...")
            {
                locationNameNew.Text = "";
            }
        }

        private void locationNameNew_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(locationNameNew.Text))
            {
                locationNameNew.Text = "Name of location...";
            }   
        }

        private void locationName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(locationName.SelectedItem == "Add location")
            {
                locationNameNew.Visible = true;
            } else
            {
                locationNameNew.Visible = false;
            }
        }

        private void addRoom_Click(object sender, EventArgs e)
        {
            if(locationName.SelectedItem == "Add location")
            {
                MeetingLocation ml = new MeetingLocation(locationNameNew.Text);
                ml.addRoom(new MeetingRoom(roomName.Text, Int32.Parse(roomCapacity.Text)));
                meetingLocations.Add(ml);
            }
            else
            {
                foreach(MeetingLocation ml in meetingLocations)
                {
                    if(ml.getName() == locationName.SelectedItem)
                    {
                        ml.addRoom(new MeetingRoom(roomName.Text, Int32.Parse(roomCapacity.Text)));
                    }
                }
            }
            updateMeetingLocationsOnAllServers();
            locationName.Items.Clear();
            foreach(MeetingLocation ml in meetingLocations)
            {
                locationName.Items.Add(ml.getName() + " (" + ml.getRoomsCount() + ")");
            }
            locationName.Items.Add("Add location");
        }

        private void updateMeetingLocationsOnAllServers()
        {
            foreach (String url in urlServers.Values)
            {
                Uri myUri = new Uri(url);
                String psURLHost = myUri.Host;
                int psURLPort = myUri.Port + 1000;
                PuppetServer ps = (PuppetServer)Activator.GetObject(typeof(PuppetServer), "http://" + psURLHost + ":" + psURLPort + "/ps");
                ps.updateLocations(meetingLocations);
            }
        }

        private void puppiScript_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            DialogResult result = openFileDialog.ShowDialog(); // Show the dialog.

            if (result == DialogResult.OK)
            {
                puppScript = openFileDialog.FileName;
                puppiScript.Text = openFileDialog.FileName.Split('\\').Last();
            }
            else if (result == DialogResult.Cancel)
            {
                puppScript = "";
                selectScript.Text = "Select script";
            }
        }

        private void runPuppiScript_Click(object sender, EventArgs e)
        {
            if (!puppiScript.Equals(""))
            {
                string script = System.IO.File.ReadAllText(puppScript);
                string[] commandList = script.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.None);
                foreach (string command in commandList)
                {
                    ProcessScriptLine(command);
                }
            }
            
        }

        //TODO TEST
        private void ProcessScriptLine(String command)
        {
            string[] commandArgs = command.Split(
                   new[] { " " },
                   StringSplitOptions.None);

            switch (commandArgs[0].ToLower())
            {
               
                case "addroom":        
                    break;
                case "server":
                    addServer(commandArgs[1], commandArgs[2], Int32.Parse(commandArgs[3]), Int32.Parse(commandArgs[4])
                        , Int32.Parse(commandArgs[5]));
                    break;
                case "client":
                    addCli(commandArgs[1], commandArgs[2], commandArgs[3]);
                    break;
                case "status":
                    getStat();
                    break;
                case "wait":
                    System.Threading.Thread.Sleep(Int32.Parse(commandArgs[1]));
                    break;
                case "freeze":
                    Freez(commandArgs[1]);
                    break;
                case "unfreeze":
                    Unfreez(commandArgs[1]);
                    break;
                case "crash":
                    Kill(commandArgs[1]);
                    break;
                default:
                    break;
            }
        }
    }
}
