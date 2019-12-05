namespace PuppetMaster
{
    partial class PMForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TerminateAll = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.unfreezeID = new System.Windows.Forms.TextBox();
            this.Unfreeze = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.freezeID = new System.Windows.Forms.TextBox();
            this.Freeze = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.crashID = new System.Windows.Forms.TextBox();
            this.Crash = new System.Windows.Forms.Button();
            this.getStatus = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.roomName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.roomCapacity = new System.Windows.Forms.TextBox();
            this.addRoom = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.clientURL = new System.Windows.Forms.TextBox();
            this.serverURL = new System.Windows.Forms.TextBox();
            this.addClient = new System.Windows.Forms.Button();
            this.username = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.addMaxDelay = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.addURL = new System.Windows.Forms.TextBox();
            this.addMaxFaults = new System.Windows.Forms.TextBox();
            this.addMinDelay = new System.Windows.Forms.TextBox();
            this.addServerButton = new System.Windows.Forms.Button();
            this.addServerId = new System.Windows.Forms.TextBox();
            this.selectScript = new System.Windows.Forms.Button();
            this.puppiScript = new System.Windows.Forms.Button();
            this.locationNameNew = new System.Windows.Forms.TextBox();
            this.locationName = new System.Windows.Forms.ComboBox();
            this.labelRoomAdd = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // TerminateAll
            // 
            this.TerminateAll.Location = new System.Drawing.Point(950, 498);
            this.TerminateAll.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TerminateAll.Name = "TerminateAll";
            this.TerminateAll.Size = new System.Drawing.Size(182, 103);
            this.TerminateAll.TabIndex = 22;
            this.TerminateAll.Text = "Terminate All";
            this.TerminateAll.UseVisualStyleBackColor = true;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(1004, 429);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(76, 20);
            this.label15.TabIndex = 39;
            this.label15.Text = "Server ID";
            // 
            // unfreezeID
            // 
            this.unfreezeID.Location = new System.Drawing.Point(950, 458);
            this.unfreezeID.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.unfreezeID.Name = "unfreezeID";
            this.unfreezeID.Size = new System.Drawing.Size(182, 26);
            this.unfreezeID.TabIndex = 19;
            // 
            // Unfreeze
            // 
            this.Unfreeze.Location = new System.Drawing.Point(723, 452);
            this.Unfreeze.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Unfreeze.Name = "Unfreeze";
            this.Unfreeze.Size = new System.Drawing.Size(201, 35);
            this.Unfreeze.TabIndex = 20;
            this.Unfreeze.Text = "Unfreeze Server";
            this.Unfreeze.UseVisualStyleBackColor = true;
            this.Unfreeze.Click += new System.EventHandler(this.Unfreeze_Click);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(1004, 362);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(76, 20);
            this.label14.TabIndex = 38;
            this.label14.Text = "Server ID";
            // 
            // freezeID
            // 
            this.freezeID.Location = new System.Drawing.Point(950, 389);
            this.freezeID.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.freezeID.Name = "freezeID";
            this.freezeID.Size = new System.Drawing.Size(182, 26);
            this.freezeID.TabIndex = 17;
            // 
            // Freeze
            // 
            this.Freeze.Location = new System.Drawing.Point(723, 386);
            this.Freeze.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Freeze.Name = "Freeze";
            this.Freeze.Size = new System.Drawing.Size(201, 35);
            this.Freeze.TabIndex = 18;
            this.Freeze.Text = "Freeze Server";
            this.Freeze.UseVisualStyleBackColor = true;
            this.Freeze.Click += new System.EventHandler(this.Freeze_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(1004, 292);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(76, 20);
            this.label13.TabIndex = 37;
            this.label13.Text = "Server ID";
            // 
            // crashID
            // 
            this.crashID.Location = new System.Drawing.Point(950, 323);
            this.crashID.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.crashID.Name = "crashID";
            this.crashID.Size = new System.Drawing.Size(182, 26);
            this.crashID.TabIndex = 15;
            // 
            // Crash
            // 
            this.Crash.Location = new System.Drawing.Point(723, 318);
            this.Crash.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Crash.Name = "Crash";
            this.Crash.Size = new System.Drawing.Size(201, 35);
            this.Crash.TabIndex = 16;
            this.Crash.Text = "Crash Server";
            this.Crash.UseVisualStyleBackColor = true;
            this.Crash.Click += new System.EventHandler(this.button5_Click);
            // 
            // getStatus
            // 
            this.getStatus.Location = new System.Drawing.Point(723, 532);
            this.getStatus.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.getStatus.Name = "getStatus";
            this.getStatus.Size = new System.Drawing.Size(204, 35);
            this.getStatus.TabIndex = 21;
            this.getStatus.Text = "Get Status";
            this.getStatus.UseVisualStyleBackColor = true;
            this.getStatus.Click += new System.EventHandler(this.getStatus_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(602, 234);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(98, 20);
            this.label12.TabIndex = 36;
            this.label12.Text = "Room Name";
            // 
            // roomName
            // 
            this.roomName.Location = new System.Drawing.Point(560, 258);
            this.roomName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.roomName.Name = "roomName";
            this.roomName.Size = new System.Drawing.Size(182, 26);
            this.roomName.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(422, 235);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 20);
            this.label6.TabIndex = 35;
            this.label6.Text = "Capacity";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(226, 229);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(70, 20);
            this.label11.TabIndex = 34;
            this.label11.Text = "Location";
            // 
            // roomCapacity
            // 
            this.roomCapacity.Location = new System.Drawing.Point(364, 260);
            this.roomCapacity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.roomCapacity.Name = "roomCapacity";
            this.roomCapacity.Size = new System.Drawing.Size(182, 26);
            this.roomCapacity.TabIndex = 12;
            // 
            // addRoom
            // 
            this.addRoom.Location = new System.Drawing.Point(24, 255);
            this.addRoom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addRoom.Name = "addRoom";
            this.addRoom.Size = new System.Drawing.Size(124, 35);
            this.addRoom.TabIndex = 14;
            this.addRoom.Text = "Add Room";
            this.addRoom.UseVisualStyleBackColor = true;
            this.addRoom.Click += new System.EventHandler(this.addRoom_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(807, 142);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 20);
            this.label7.TabIndex = 33;
            this.label7.Text = "Script File";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(604, 142);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(92, 20);
            this.label8.TabIndex = 32;
            this.label8.Text = "Server URL";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(412, 142);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(86, 20);
            this.label9.TabIndex = 31;
            this.label9.Text = "Client URL";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(220, 137);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(83, 20);
            this.label10.TabIndex = 30;
            this.label10.Text = "Username";
            // 
            // clientURL
            // 
            this.clientURL.Location = new System.Drawing.Point(364, 166);
            this.clientURL.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.clientURL.Name = "clientURL";
            this.clientURL.Size = new System.Drawing.Size(182, 26);
            this.clientURL.TabIndex = 7;
            // 
            // serverURL
            // 
            this.serverURL.Location = new System.Drawing.Point(560, 166);
            this.serverURL.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.serverURL.Name = "serverURL";
            this.serverURL.Size = new System.Drawing.Size(182, 26);
            this.serverURL.TabIndex = 8;
            this.serverURL.Text = "tcp://localhost:8001/mcm";
            // 
            // addClient
            // 
            this.addClient.Location = new System.Drawing.Point(24, 162);
            this.addClient.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addClient.Name = "addClient";
            this.addClient.Size = new System.Drawing.Size(124, 35);
            this.addClient.TabIndex = 10;
            this.addClient.Text = "Add Client";
            this.addClient.UseVisualStyleBackColor = true;
            this.addClient.Click += new System.EventHandler(this.addClient_Click);
            // 
            // username
            // 
            this.username.Location = new System.Drawing.Point(170, 166);
            this.username.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(182, 26);
            this.username.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(998, 29);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 20);
            this.label5.TabIndex = 29;
            this.label5.Text = "MAX Delay";
            // 
            // addMaxDelay
            // 
            this.addMaxDelay.Location = new System.Drawing.Point(950, 57);
            this.addMaxDelay.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addMaxDelay.Name = "addMaxDelay";
            this.addMaxDelay.Size = new System.Drawing.Size(182, 26);
            this.addMaxDelay.TabIndex = 4;
            this.addMaxDelay.Text = "6";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(806, 34);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 20);
            this.label4.TabIndex = 28;
            this.label4.Text = "MIN Delay";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(604, 34);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 20);
            this.label3.TabIndex = 27;
            this.label3.Text = "MAX Faults";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(435, 34);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 20);
            this.label2.TabIndex = 26;
            this.label2.Text = "URL";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(224, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 20);
            this.label1.TabIndex = 25;
            this.label1.Text = "Server ID";
            // 
            // addURL
            // 
            this.addURL.Location = new System.Drawing.Point(364, 58);
            this.addURL.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addURL.Name = "addURL";
            this.addURL.Size = new System.Drawing.Size(182, 26);
            this.addURL.TabIndex = 1;
            // 
            // addMaxFaults
            // 
            this.addMaxFaults.Location = new System.Drawing.Point(560, 58);
            this.addMaxFaults.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addMaxFaults.Name = "addMaxFaults";
            this.addMaxFaults.Size = new System.Drawing.Size(182, 26);
            this.addMaxFaults.TabIndex = 2;
            this.addMaxFaults.Text = "4";
            // 
            // addMinDelay
            // 
            this.addMinDelay.Location = new System.Drawing.Point(754, 58);
            this.addMinDelay.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addMinDelay.Name = "addMinDelay";
            this.addMinDelay.Size = new System.Drawing.Size(182, 26);
            this.addMinDelay.TabIndex = 3;
            this.addMinDelay.Text = "5";
            // 
            // addServerButton
            // 
            this.addServerButton.Location = new System.Drawing.Point(24, 54);
            this.addServerButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addServerButton.Name = "addServerButton";
            this.addServerButton.Size = new System.Drawing.Size(124, 35);
            this.addServerButton.TabIndex = 5;
            this.addServerButton.Text = "Add Server";
            this.addServerButton.UseVisualStyleBackColor = true;
            this.addServerButton.Click += new System.EventHandler(this.addServerButton_Click);
            // 
            // addServerId
            // 
            this.addServerId.Location = new System.Drawing.Point(170, 58);
            this.addServerId.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addServerId.Name = "addServerId";
            this.addServerId.Size = new System.Drawing.Size(182, 26);
            this.addServerId.TabIndex = 0;
            // 
            // selectScript
            // 
            this.selectScript.Location = new System.Drawing.Point(772, 162);
            this.selectScript.Name = "selectScript";
            this.selectScript.Size = new System.Drawing.Size(148, 35);
            this.selectScript.TabIndex = 40;
            this.selectScript.Text = "Select script";
            this.selectScript.UseVisualStyleBackColor = true;
            this.selectScript.Click += new System.EventHandler(this.selectScript_Click);
            // 
            // puppiScript
            // 
            this.puppiScript.Location = new System.Drawing.Point(16, 452);
            this.puppiScript.Name = "puppiScript";
            this.puppiScript.Size = new System.Drawing.Size(240, 88);
            this.puppiScript.TabIndex = 41;
            this.puppiScript.Text = "Select PuppetMaster Script";
            this.puppiScript.UseVisualStyleBackColor = true;
            this.puppiScript.Click += new System.EventHandler(this.puppiScript_Click);
            // 
            // locationNameNew
            // 
            this.locationNameNew.Location = new System.Drawing.Point(170, 304);
            this.locationNameNew.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.locationNameNew.Name = "locationNameNew";
            this.locationNameNew.Size = new System.Drawing.Size(182, 26);
            this.locationNameNew.TabIndex = 11;
            this.locationNameNew.Text = "Name of location...";
            this.locationNameNew.Enter += new System.EventHandler(this.locationNameNew_Enter);
            this.locationNameNew.Leave += new System.EventHandler(this.locationNameNew_Leave);
            // 
            // locationName
            // 
            this.locationName.FormattingEnabled = true;
            this.locationName.Location = new System.Drawing.Point(170, 258);
            this.locationName.Name = "locationName";
            this.locationName.Size = new System.Drawing.Size(182, 28);
            this.locationName.TabIndex = 42;
            this.locationName.SelectedIndexChanged += new System.EventHandler(this.locationName_SelectedIndexChanged);
            // 
            // labelRoomAdd
            // 
            this.labelRoomAdd.AutoSize = true;
            this.labelRoomAdd.Location = new System.Drawing.Point(166, 350);
            this.labelRoomAdd.Name = "labelRoomAdd";
            this.labelRoomAdd.Size = new System.Drawing.Size(206, 20);
            this.labelRoomAdd.TabIndex = 43;
            this.labelRoomAdd.Text = "Label is renamed on startup";
            // 
            // PMForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1146, 631);
            this.Controls.Add(this.labelRoomAdd);
            this.Controls.Add(this.locationName);
            this.Controls.Add(this.puppiScript);
            this.Controls.Add(this.selectScript);
            this.Controls.Add(this.TerminateAll);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.unfreezeID);
            this.Controls.Add(this.Unfreeze);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.freezeID);
            this.Controls.Add(this.Freeze);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.crashID);
            this.Controls.Add(this.Crash);
            this.Controls.Add(this.getStatus);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.roomName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.roomCapacity);
            this.Controls.Add(this.addRoom);
            this.Controls.Add(this.locationNameNew);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.clientURL);
            this.Controls.Add(this.serverURL);
            this.Controls.Add(this.addClient);
            this.Controls.Add(this.username);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.addMaxDelay);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.addURL);
            this.Controls.Add(this.addMaxFaults);
            this.Controls.Add(this.addMinDelay);
            this.Controls.Add(this.addServerButton);
            this.Controls.Add(this.addServerId);
            this.Name = "PMForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button TerminateAll;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox unfreezeID;
        private System.Windows.Forms.Button Unfreeze;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox freezeID;
        private System.Windows.Forms.Button Freeze;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox crashID;
        private System.Windows.Forms.Button Crash;
        private System.Windows.Forms.Button getStatus;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox roomName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox roomCapacity;
        private System.Windows.Forms.Button addRoom;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox clientURL;
        private System.Windows.Forms.TextBox serverURL;
        private System.Windows.Forms.Button addClient;
        private System.Windows.Forms.TextBox username;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox addMaxDelay;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox addURL;
        private System.Windows.Forms.TextBox addMaxFaults;
        private System.Windows.Forms.TextBox addMinDelay;
        private System.Windows.Forms.Button addServerButton;
        private System.Windows.Forms.TextBox addServerId;
        private System.Windows.Forms.Button selectScript;
        private System.Windows.Forms.Button puppiScript;
        private System.Windows.Forms.TextBox locationNameNew;
        private System.Windows.Forms.ComboBox locationName;
        private System.Windows.Forms.Label labelRoomAdd;
    }
}

