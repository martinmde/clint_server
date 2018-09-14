﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using SimpleTCP;
using EasyEncryption;

enum command:int { GET_PROJECTS=666, AUTHENTICATE, GET_REQUIREMENTS } ;


namespace client_server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private int current_command = 0;
        private int messagecount = 0;
        private String current_project = "";
        private String lasttab = "init";

        SimpleTcpClient Client;
        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            try
            {
                label6.Text = "connected to host";
                label6.ForeColor = System.Drawing.Color.Green;
                Client.Connect(txtHost.Text, Convert.ToInt32(txtPort.Text));
            }
            catch (Exception)
            {
                label6.Text = "ERROR: could not connect to host";
                label6.ForeColor = System.Drawing.Color.Red;
                //throw;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Client = new SimpleTcpClient();
            Client.StringEncoder = Encoding.UTF8;
            Client.DataReceived += Client_DataReceived_init;   // connect, auth, get projects
        }

        // the function below is handed over to the client object
        // and acts as EventHandler,  see SimpleTcpClient.cs
        // The client object knows nothing about the txtStatus box, 
        // 

        private void btnSend_Click(object sender, EventArgs e)
        {
            SimpleTCP.Message newmessage = Client.WriteLineAndGetReply(txtMessage.Text, TimeSpan.FromMilliseconds(1));
            // data is received via event handler Client_DataReceived_phase1/2
            // Client_DataReceived_phase1/2 will handle the reply
        }




        private void button1_Click(object sender, EventArgs e)
        {
            SimpleTCP.Message newmessage = Client.WriteLineAndGetReply("get projects " + txtUsername.Text, TimeSpan.FromMilliseconds(1));
            current_command = (int)command.GET_PROJECTS;
            label3.Text = "click to choose";
        }

        private void btnAuth_Click(object sender, EventArgs e)
        {
            String encrypt_pw = EasyEncryption.SHA.ComputeSHA1Hash(txtPassword.Text);
            txtPassword.Clear(); // clear ASAP
            SimpleTCP.Message newmessage = Client.WriteLineAndGetReply("authenticate " + txtUsername.Text + " " + encrypt_pw, TimeSpan.FromMilliseconds(1));
            current_command = (int)command.AUTHENTICATE;
        }

        private void listBoxProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(listBoxProjects.Items[listBoxProjects.SelectedIndex].ToString(), "choose project", MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                current_project = listBoxProjects.Items[listBoxProjects.SelectedIndex].ToString();
                label3.Text = "project chosen: " + current_project;
            }
            else current_project = "";

        }

        private void labelAuth_TextChanged(object sender, EventArgs e)
        {
            label3.Text = labelAuth.Text;
        }

        /// <summary>
        /// //////////////////////////////////////////////////////////////////


        private void Client_DataReceived_init(object sender, SimpleTCP.Message e)  // connect, auth, get projects
        {
            messagecount++;

            void p()
            {  // will always be executed
                txtClientStatus.Text += messagecount.ToString() + e.MessageString + "\r\n";
            };

            void q() // does get projects  handling
            {
                String[] projects = e.MessageString.Split((char)0x13);
                int i = 0;
                while (projects[i] != "(none)")
                {
                    listBoxProjects.Items.Add(projects[i]);
                    i++;
                }
            };

            void r() // handles  authentification
            {
                labelAuth.Text = "authenticated as " + txtUsername.Text;
            };

            txtClientStatus.Invoke((MethodInvoker)p);

            if (current_command == (int)command.GET_PROJECTS)
            {

                listBoxProjects.Invoke((MethodInvoker)q);

            }

            if (current_command == (int)command.AUTHENTICATE)
            {
                if (e.MessageString.StartsWith("authenticated"))
                    labelAuth.Invoke((MethodInvoker)r);

            }

        }

        /// <summary>
        /// ///////////////////////////////////////////////

        private void Client_DataReceived_work(object sender, SimpleTCP.Message e)   // working
        {
           

            
            void p()
            {
                
                String[] requirements = e.MessageString.Split((char)0x13);
                int i = 0;
                while (requirements[i] != "(none)")
                {
                    checkedListBoxReq.Items.Insert(i, requirements[i] );
                    i++;
                }

                
            };
            if (current_command == (int)command.GET_REQUIREMENTS)
            {
                checkedListBoxReq.Invoke((MethodInvoker)p);
            }

            void q()  // get requirements
            {
                String[] requirements = e.MessageString.Split((char)0x13);
                int i = 0;
                while (requirements[i] != "(none)")
                {
                    textBoxRequirements.Text += requirements[i]+ Environment.NewLine;
                    i++;
                }
            };



            if (current_command == (int)command.GET_REQUIREMENTS)
            {
                textBoxRequirements.Invoke((MethodInvoker)q);

            }

        }  // phase3



        private void Client_DataReceived_debug(object sender, SimpleTCP.Message e)  //debug
        {
            messagecount++;

            void p()
            {
                txtClientStatus.Text += messagecount.ToString() + e.MessageString + "\r\n";
            };

            txtClientStatus.Invoke((MethodInvoker)p);


        } //phase2  debug





        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {

            //MessageBox.Show(tabControl2.SelectedTab.Text);
            if (tabControl2.SelectedTab.Text.StartsWith("init"))
            {

                if (lasttab == "work") Client.DataReceived -= Client_DataReceived_work;
                if (lasttab == "debug") Client.DataReceived -= Client_DataReceived_debug;

                Client.DataReceived += Client_DataReceived_init;
                lasttab = "init";
            }
            if (tabControl2.SelectedTab.Text.StartsWith("debug"))
            {

                if (lasttab == "work") Client.DataReceived -= Client_DataReceived_work;
                if (lasttab == "init") Client.DataReceived -= Client_DataReceived_init;
                Client.DataReceived += Client_DataReceived_debug;
                lasttab = "debug";



            }
            if (tabControl2.SelectedTab.Text.StartsWith("work"))
            {

                if (lasttab == "init") Client.DataReceived -= Client_DataReceived_init;
                if (lasttab == "debug") Client.DataReceived -= Client_DataReceived_debug;
                Client.DataReceived += Client_DataReceived_work;
                lasttab = "work";

                


            }



        }

        private void btnFastStart_Click(object sender, EventArgs e)
        {

            btnConnect_Click(sender, e);
            txtPassword.Text = "1234";
            btnAuth_Click(sender, e);

            Thread.Sleep(10);
            current_project = "bmm580";
            label3.Text = "project chosen: " + current_project;


        }

        private void btnGetReq_Click(object sender, EventArgs e)
        {
            SimpleTCP.Message newmessage = Client.WriteLineAndGetReply("get requirements " + current_project + " " + txtUsername.Text, TimeSpan.FromMilliseconds(1));
            current_command = (int)command.GET_REQUIREMENTS;
        }
    }
}
