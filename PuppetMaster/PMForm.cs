using Client;
using PCS;
using Server;
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
        List<String> urlServers = new List<String>();
        List<String> urlClients = new List<String>();
        String scriptPath = "";
        public PMForm()
        {
            InitializeComponent();
        }

        // We gonna use the PCS here to create a Server
        private void addServerButton_Click(object sender, EventArgs e)
        {
            String serverID = addServerId.Text;
            String URL = addURL.Text;
            int maxFaults = Int32.Parse(addMaxFaults.Text);
            int minDelay = Int32.Parse(addMinDelay.Text);
            int maxDelay = Int32.Parse(addMaxDelay.Text);

            //we need the URL to know in which machine is it
            // If it's not on the same machine as the PuppetMaster we need to check the PCS
            // on the machine we want to call
            PCSService pCs;
            Uri myUri = new Uri(URL);
            IPAddress[] hostIPs = Dns.GetHostAddresses(myUri.Host);



            // get local IP addresses
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            // test if any host IP is a loopback IP or is equal to any local IP


            if (hostIPs.Any(hostIP => IPAddress.IsLoopback(hostIP) || localIPs.Contains(hostIP))) // we can directly create the server because is 
            {
                // we should make an interface for the PuppetMaster to communicate
                // directly with the server

                //SchedulingServer ss = new SchedulingServer(serverID, URL, maxFaults, minDelay, maxDelay);
                //ss.start();             

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Directory.GetCurrentDirectory() + @"..\..\..\..\Server\bin\Debug\Server",
                        Arguments = serverID + "'" + URL + "'" + maxFaults + "'" + minDelay + "'" + maxDelay,

                    }
                };

                process.Start();
                String name = process.ProcessName; // we save the process and the name
            }
            else
            {
                //pCs = (PCS)Activator.GetObject(typeof(PCS), IP + "10000"); //TODO can generate new exception
                // pCs.createServerProcess(serverID, URL, maxFaults, minDelay, maxDelay);
            }

            urlServers.Add(URL);

        }

        private void addClient_Click(object sender, EventArgs e)
        {
            String userName = username.Text;
            String cURL = clientURL.Text;
            String sURL = serverURL.Text;

            //we need the URL to know in which machine is it
            // If it's not on the same machine as the PuppetMaster we need to check the PCS
            // on the machine we want to call
            PCSService pCs;
            Uri myUri = new Uri(cURL);
            IPAddress[] hostIPs = Dns.GetHostAddresses(myUri.Host);



            // get local IP addresses
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            // test if any host IP is a loopback IP or is equal to any local IP


            if (hostIPs.Any(hostIP => IPAddress.IsLoopback(hostIP) || localIPs.Contains(hostIP))) // we can directly create the server because is 
            {
                //Cliente cc = new Cliente(userName, cURL, sURL, script);
                //cc.start();
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
                String name = process.ProcessName; // we save the process and the name
            }
            else
            {
                //pCs = (PCS)Activator.GetObject(typeof(PCS), IP + "10000"); //TODO can generate new exception
                //pCs.createClientProcess(userName, cURL, sURL, script);
            }

            urlClients.Add(cURL);
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
    }
}
