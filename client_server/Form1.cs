﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleTCP;

enum command:int { GET_PROJECTS=666} ;


namespace client_server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private int current_command=0;
        private int messagecount = 0;

        SimpleTcpClient Client;
        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            try
            {
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
            Client.DataReceived += Client_DataReceived;
        }

        // the function below is handed over to the client object
        // and acts as EventHandler,  see SimpleTcpClient.cs
        // The client object knows nothing about the txtStatus box, 
        // 
        private void Client_DataReceived(object sender, SimpleTCP.Message e)
        {
            messagecount++;

            void p(){
                txtClientStatus.Text += messagecount.ToString()+e.MessageString+"\r\n";
            };
            void q()
            {
                String[] projects = e.MessageString.Split((char) 0x13);
                int i = 0;
                while (projects[i] != "(none)")
                {
                    listBoxProjects.Items.Add(projects[i]);
                    i++;
                }
            };


            txtClientStatus.Invoke((MethodInvoker)p);

            if(current_command == (int)command.GET_PROJECTS)
            {
                listBoxProjects.Invoke((MethodInvoker)q);


            }


        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SimpleTCP.Message newmessage= Client.WriteLineAndGetReply(txtMessage.Text, TimeSpan.FromMilliseconds(1));                  
        }


  

        private void button1_Click(object sender, EventArgs e)
        {
            SimpleTCP.Message newmessage = Client.WriteLineAndGetReply("get projects", TimeSpan.FromMilliseconds(1));
            current_command = (int) command.GET_PROJECTS;
        }


        
    }
}
