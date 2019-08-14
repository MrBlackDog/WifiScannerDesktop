using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WiFi.Scanner.Classes;
using WindowsFormsApp1.Classes;

namespace WiFi.Scanner
{
    public class WifiInfo
    {
        public string SSID;
        public string BSSID;
        public string Level;
        public int NumBerOfMentions;
        public double avglevel;
        public double diff;
        public WifiInfo()
        {

        }
    }
    public class CoordList
    {
        public int X, Y;
        public List<WifiInfo> WifiInfoList;
    }
    public class Result
    {
        public string SSID;
        public double res;
    }

    public partial class FormScanner : Form
    {
        private string writePath = @"D:\Projects\Wifi Measurements\J200_0,5.txt";
        private WlanClient wlanClient = null;
        private Label IterationsCountLabel;
        private Label label1;
        private System.ComponentModel.IContainer components = null;
        private int IterCount = 0;
        WifiCoordForm newForm;
       // public WifiCoordForm newForm;
        public static int i, j;
        //лист, содержащий все координаты и листом сетей,привязанные к координатам
        public static List<CoordList> CoordListInfo = new List<CoordList>();
        //лист с текущими результатами сканирования
        private List<WifiInfo> CurrentWifiList = new List<WifiInfo>();
        //лист, содержащий минимум разностей сигналов сетей
        public List<Result> MinimumResultList = new List<Result>();
        public FormScanner()
        {
            newForm = new WifiCoordForm();
            newForm.Show();
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            for (i = 0; i < 2; i++)
                for (j = 0; j < 6; j++)
                {
                    ReadFile(i, j);
                }
        }

        //на вход метода приходит лист вафлей
        public void CalculateDiff(List<WifiInfo> wifiInfos)
        {
            newForm.label1.Text = "";
            int NumberOfMatches;
            int MaxMatchesIndex = 0;
            int DiffIndex = 0;
            int MaxDiffIndex = 0;
            for (int i = 0; i < CoordListInfo.Count; i++)
            {
                DiffIndex = 0;
                // Console.WriteLine("//////////////////" + CoordListInfo[i].X + " " + CoordListInfo[i].Y);
                NumberOfMatches = 0;
                foreach (WifiInfo wi in wifiInfos)
                {
                    //if(wi.BSSID != null)
                    if (CoordListInfo[i].WifiInfoList.Exists(x => x.BSSID == wi.BSSID))
                    {
                        //WifiInfo diffInfo = new WifiInfo();
                        NumberOfMatches++;
                        CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(wi.BSSID)).diff =
                         Math.Abs(Math.Round(CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(wi.BSSID)).avglevel - double.Parse(wi.Level.Split('d')[0]), 1));
                        Console.WriteLine(CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(wi.BSSID)).SSID + " " + CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(wi.BSSID)).diff);
                    }
                    //diffInfo.SSID = wi.SSID;
                    //diffInfo.avglevel = res;
                    //DiffList.Add(diffInfo);
                }
                //  Console.WriteLine("Число совпадений:" + NumberOfMatches);
                if (i == 0)
                {
                    MaxMatchesIndex = i;
                    CoordListInfo[MaxMatchesIndex].WifiInfoList.ForEach(delegate (WifiInfo wi)
                    {
                        // MinDiffList.Find(x => x.BSSID.Contains(diff.BSSID)).diff = CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(diff.BSSID)).diff;
                        if ((CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(wi.BSSID)).diff < 3) && (CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(wi.BSSID)).diff != 0))
                            DiffIndex++;
                    });
                    MaxDiffIndex = DiffIndex;
                }
                else
                {
                    CoordListInfo[MaxMatchesIndex].WifiInfoList.ForEach(delegate (WifiInfo wi)
                    {
                        if (CoordListInfo[i].WifiInfoList.Exists(x => x.BSSID == wi.BSSID))
                        {
                            if (CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(wi.BSSID)).diff < CoordListInfo[MaxMatchesIndex].WifiInfoList.Find(x => x.BSSID.Contains(wi.BSSID)).diff)
                            {
                                // MinDiffList.Find(x => x.BSSID.Contains(diff.BSSID)).diff = CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(diff.BSSID)).diff;
                                if ((CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(wi.BSSID)).diff < 3) && (CoordListInfo[i].WifiInfoList.Find(x => x.BSSID.Contains(wi.BSSID)).diff != 0))
                                    DiffIndex++;
                            }
                        }
                    });
                    if (DiffIndex > MaxDiffIndex)
                    {
                        MaxMatchesIndex = i;
                        MaxDiffIndex = DiffIndex;
                    }
                   // Console.WriteLine(DiffIndex);
                }
                newForm.label1.Text += DiffIndex.ToString() + '\n';
            }
            newForm.UpdateList(CoordListInfo);
            label1.Text = CoordListInfo[MaxMatchesIndex].X + " " + CoordListInfo[MaxMatchesIndex].Y;
            // Console.WriteLine(MaxDiffIndex);
            // Console.WriteLine(CoordListInfo[MaxMatchesIndex].X + " " + CoordListInfo[MaxMatchesIndex].Y);           
        }
        //Чтение файла для записи в лист координат
        public static void ReadFile(int x, int y)
        {
            string readPath = @"D:\Projects\AVG Wifi Measurements\J200_" + i + "," + j + "AVG.txt";
            StreamReader sr = new StreamReader(readPath, System.Text.Encoding.Default);
            string[] rawMass = sr.ReadToEnd().Split('\n');
            WifiInfo wifiInfo;
            CoordList coordList = new CoordList();
            coordList.X = x;
            coordList.Y = y;
            coordList.WifiInfoList = new List<WifiInfo>();
            foreach (string CurrRawMass in rawMass)
            {
                string CurrMass = CurrRawMass.Trim('\r');
                wifiInfo = new WifiInfo();
                wifiInfo.SSID = CurrMass.Split('#')[0];
                if (CurrMass.Split('#').Length == 4)
                {
                    wifiInfo.BSSID = CurrRawMass.Split('#')[1];
                    wifiInfo.avglevel = Double.Parse(CurrMass.Split('#')[2].Split('d')[0]);
                    wifiInfo.NumBerOfMentions = Int32.Parse(CurrMass.Split('#')[3]);
                }
                else if (CurrMass.Split('#').Length == 3)
                {
                    wifiInfo.avglevel = Double.Parse(CurrMass.Split('#')[1].Split('d')[0]);
                    wifiInfo.NumBerOfMentions = Int32.Parse(CurrMass.Split('#')[2]);
                }
                coordList.WifiInfoList.Add(wifiInfo);
            }
            CoordListInfo.Add(coordList);
        }
        private void InitializeComponent()
        {
            this.WiFiThemeContainer = new MonoFlat_ThemeContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.IterationsCountLabel = new System.Windows.Forms.Label();
            this.listViewAccessPoints = new System.Windows.Forms.ListView();
            this.ColProfile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColMacAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColQuality = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColSignal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColChannel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColEncryption = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColAuthentication = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.WiFiControlBox = new MonoFlat_ControlBox();
            this.WiFiThemeContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // WiFiThemeContainer
            // 
            this.WiFiThemeContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(41)))), ((int)(((byte)(50)))));
            this.WiFiThemeContainer.Controls.Add(this.label1);
            this.WiFiThemeContainer.Controls.Add(this.IterationsCountLabel);
            this.WiFiThemeContainer.Controls.Add(this.listViewAccessPoints);
            this.WiFiThemeContainer.Controls.Add(this.WiFiControlBox);
            this.WiFiThemeContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WiFiThemeContainer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.WiFiThemeContainer.Location = new System.Drawing.Point(0, 0);
            this.WiFiThemeContainer.Name = "WiFiThemeContainer";
            this.WiFiThemeContainer.Padding = new System.Windows.Forms.Padding(10, 70, 10, 9);
            this.WiFiThemeContainer.RoundCorners = true;
            this.WiFiThemeContainer.Sizable = true;
            this.WiFiThemeContainer.Size = new System.Drawing.Size(704, 490);
            this.WiFiThemeContainer.SmartBounds = true;
            this.WiFiThemeContainer.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WiFiThemeContainer.TabIndex = 0;
            this.WiFiThemeContainer.Text = "WiFi Scanner";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.IterationsCountLabel.BackColor = System.Drawing.Color.White;
            this.IterationsCountLabel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.label1.Location = new System.Drawing.Point(441, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "coords";
            // 
            // IterationsCountLabel
            // 
            this.IterationsCountLabel.AutoSize = true;
            this.IterationsCountLabel.BackColor = System.Drawing.Color.White;
            this.IterationsCountLabel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.IterationsCountLabel.Location = new System.Drawing.Point(138, 25);
            this.IterationsCountLabel.Name = "IterationsCountLabel";
            this.IterationsCountLabel.Size = new System.Drawing.Size(55, 15);
            this.IterationsCountLabel.TabIndex = 2;
            this.IterationsCountLabel.Text = "itercount";
            // 
            // listViewAccessPoints
            // 
            this.listViewAccessPoints.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewAccessPoints.BackColor = System.Drawing.Color.White;
            this.listViewAccessPoints.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColProfile,
            this.ColMacAddress,
            this.ColQuality,
            this.ColSignal,
            this.ColChannel,
            this.ColEncryption,
            this.ColAuthentication});
            this.listViewAccessPoints.FullRowSelect = true;
            this.listViewAccessPoints.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewAccessPoints.Location = new System.Drawing.Point(13, 73);
            this.listViewAccessPoints.MultiSelect = false;
            this.listViewAccessPoints.Name = "listViewAccessPoints";
            this.listViewAccessPoints.Size = new System.Drawing.Size(677, 404);
            this.listViewAccessPoints.TabIndex = 1;
            this.listViewAccessPoints.UseCompatibleStateImageBehavior = false;
            this.listViewAccessPoints.View = System.Windows.Forms.View.Details;
            // 
            // ColProfile
            // 
            this.ColProfile.Tag = "Profile";
            this.ColProfile.Text = "Profile";
            this.ColProfile.Width = 170;
            // 
            // ColMacAddress
            // 
            this.ColMacAddress.Tag = "Mac";
            this.ColMacAddress.Text = "Mac Address";
            this.ColMacAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ColMacAddress.Width = 120;
            // 
            // ColQuality
            // 
            this.ColQuality.Tag = "Quality";
            this.ColQuality.Text = "Quality";
            this.ColQuality.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ColQuality.Width = 50;
            // 
            // ColSignal
            // 
            this.ColSignal.Tag = "Signal";
            this.ColSignal.Text = "Signal";
            this.ColSignal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ColChannel
            // 
            this.ColChannel.Tag = "Channel";
            this.ColChannel.Text = "Channel";
            this.ColChannel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ColEncryption
            // 
            this.ColEncryption.Tag = "Encryption";
            this.ColEncryption.Text = "Encryption";
            this.ColEncryption.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ColEncryption.Width = 70;
            // 
            // ColAuthentication
            // 
            this.ColAuthentication.Tag = "Authentication";
            this.ColAuthentication.Text = "Authentication";
            this.ColAuthentication.Width = 125;
            // 
            // WiFiControlBox
            // 
            this.WiFiControlBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.WiFiControlBox.EnableHoverHighlight = true;
            this.WiFiControlBox.EnableMaximizeButton = false;
            this.WiFiControlBox.EnableMinimizeButton = true;
            this.WiFiControlBox.Location = new System.Drawing.Point(592, 15);
            this.WiFiControlBox.Name = "WiFiControlBox";
            this.WiFiControlBox.Size = new System.Drawing.Size(100, 25);
            this.WiFiControlBox.TabIndex = 0;
            // 
            // FormScanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(704, 490);
            this.Controls.Add(this.WiFiThemeContainer);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(704, 490);
            this.Name = "FormScanner";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WiFi Scanner";
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.Load += new System.EventHandler(this.FormScanner_Load);
            this.Shown += new System.EventHandler(this.FormScanner_Shown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormScanner_KeyUp);
            this.WiFiThemeContainer.ResumeLayout(false);
            this.WiFiThemeContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        private MonoFlat_ThemeContainer WiFiThemeContainer;
        private MonoFlat_ControlBox WiFiControlBox;
        private System.Windows.Forms.ListView listViewAccessPoints;
        private System.Windows.Forms.ColumnHeader ColProfile;
        private System.Windows.Forms.ColumnHeader ColMacAddress;
        private System.Windows.Forms.ColumnHeader ColQuality;
        private System.Windows.Forms.ColumnHeader ColSignal;
        private System.Windows.Forms.ColumnHeader ColChannel;
        private System.Windows.Forms.ColumnHeader ColEncryption;
        private System.Windows.Forms.ColumnHeader ColAuthentication;

        //private FileWriter fileWriter;
        private void FormScanner_Load(object sender, EventArgs e)
        {
            wlanClient = new WlanClient();

            Taskbar.SetState(this.Handle, Taskbar.TaskbarStates.Indeterminate);
        }
        private void FormScanner_KeyUp(object sender, KeyEventArgs e)
        {
            // Display Help About Screen
            if (e.KeyCode == Keys.F1)
            {
                // FormAbout frm = new FormAbout();
                // frm.Show(this);
            }
            // Scan
            if (e.KeyCode == Keys.F5)
            {
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    this.Scan();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, WiFiThemeContainer.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.Cursor = Cursors.Default;
            }
        }
        private void FormScanner_Shown(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                this.ScanRegister();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, WiFiThemeContainer.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.Cursor = Cursors.Default;
        }

        private void ScanRegister()
        {
            listViewAccessPoints.Items.Clear();

            foreach (WlanClient.WlanInterface wlanInterface in wlanClient.Interfaces)
            {
                Wlan.WlanAvailableNetwork[] networks = wlanInterface.GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles |
                                                                                             Wlan.WlanGetAvailableNetworkFlags.IncludeAllManualHiddenProfiles);
                Wlan.WlanBssEntry[] wlanBssEntries = wlanInterface.GetNetworkBssList();

                wlanInterface.WlanNotification +=
                    new WlanClient.WlanInterface.WlanNotificationEventHandler(Wlan_Notification);

                this.NetworkList(networks, wlanBssEntries);
            }
            CalculateDiff(CurrentWifiList);
        }
        private void Scan()
        {
            listViewAccessPoints.Items.Clear();
            CurrentWifiList.Clear();
            foreach (WlanClient.WlanInterface wlanInterface in wlanClient.Interfaces)
            {
                Wlan.WlanAvailableNetwork[] networks = wlanInterface.GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles |
                                                                                              Wlan.WlanGetAvailableNetworkFlags.IncludeAllManualHiddenProfiles);
                Wlan.WlanBssEntry[] wlanBssEntries = wlanInterface.GetNetworkBssList();

                this.NetworkList(networks, wlanBssEntries);
            }
            CalculateDiff(CurrentWifiList);
        }

        private void ResetList(bool reset)
        {
            if (reset) listViewAccessPoints.Items.Clear();
            listViewAccessPoints.Refresh();
        }
        private string GetProfileName(Wlan.Dot11Ssid value)
        {
            return Encoding.ASCII.GetString(value.SSID, 0, (int)value.SSIDLength);
        }
        private string GetMacAddress(Byte[] value)
        {
            var macAddrLen = (uint)value.Length;
            var str = new string[(int)macAddrLen];

            for (int i = 0; i < macAddrLen; i++)
                str[i] += (value[i].ToString("x2").PadLeft(2, '0').ToUpper() + ":").Trim();

            str[str.Length - 1] = str[str.Length - 1].Remove(2, 1);

            return string.Join("", str);
        }
        private int GetChannel(Wlan.WlanBssEntry value)
        {
            int freq = (int)(value.chCenterFrequency / 1000);

            if (freq >= 2412 && freq <= 2484)
                return (freq - 2412) / 5 + 1;
            else if (freq >= 5170 && freq <= 5825)
                return (freq - 5170) / 5 + 34;
            else
                return -1;
        }
        private void ShowProgress(int value, int max, Taskbar.TaskbarStates status)
        {
            if (status == Taskbar.TaskbarStates.NoProgress)
            {
                Taskbar.SetState(this.Handle, Taskbar.TaskbarStates.NoProgress);
                return;
            }

            Taskbar.SetValue(this.Handle, value, max);
            Taskbar.SetState(this.Handle, status);
        }

        private void NetworkList(Wlan.WlanAvailableNetwork[] networks, Wlan.WlanBssEntry[] wlanBssEntries)
        {
            int count = 0;
            IterCount++;
            IterationsCountLabel.Text = IterCount.ToString();
            //WriteToFile(IterCount.ToString());

            foreach (Wlan.WlanAvailableNetwork network in networks)
            {
                Application.DoEvents();

                Wlan.WlanBssEntry entry = (from bs in wlanBssEntries
                                           where GetProfileName(bs.dot11Ssid).Trim() == GetProfileName(network.dot11Ssid).Trim()
                                           select bs).FirstOrDefault<Wlan.WlanBssEntry>();

                ShowProgress((count += 1), networks.Length, Taskbar.TaskbarStates.Normal);
                this.AddToList(network, entry);

                Thread.Sleep(200);
            }
            ShowProgress(0, 0, Taskbar.TaskbarStates.NoProgress);
        }
        private void AddToList(Wlan.WlanAvailableNetwork network, Wlan.WlanBssEntry entry)
        {
            ListViewItem wifiItem = new ListViewItem(this.GetProfileName(network.dot11Ssid));
            // MAC Address
            wifiItem.SubItems.Add(this.GetMacAddress(entry.dot11Bssid));
            // Signal Quality
            wifiItem.SubItems.Add(string.Format("{0}%", network.wlanSignalQuality.ToString()));
            // dBm Value
            wifiItem.SubItems.Add(string.Format("{0}dBm", entry.rssi.ToString()));
            // Channel No
            wifiItem.SubItems.Add(this.GetChannel(entry).ToString());
            // Encryption
            wifiItem.SubItems.Add(network.dot11DefaultCipherAlgorithm.ToString());
            // Authentication
            wifiItem.SubItems.Add(network.dot11DefaultAuthAlgorithm.ToString());

            int range = ((int)network.wlanSignalQuality - 1) / 25;
            wifiItem.ImageIndex = range;

            if (network.dot11DefaultCipherAlgorithm.ToString().Equals("None"))
                wifiItem.BackColor = Color.LimeGreen;
            //запись в файл и добавление в лист на вторую форму
           // WriteToFile(this.GetProfileName(network.dot11Ssid) +"#"+ this.GetMacAddress(entry.dot11Bssid) + "#" + string.Format("{0}dBm", entry.rssi.ToString()));
            WifiInfo wi = new WifiInfo();
            wi.SSID = this.GetProfileName(network.dot11Ssid);
            wi.BSSID = this.GetMacAddress(entry.dot11Bssid);
            wi.Level = entry.rssi.ToString();
            CurrentWifiList.Add(wi);
            listViewAccessPoints.Items.Add(wifiItem);
        }

        public void WriteToFile(string StringToWrite)
        {
            StreamWriter sw = new StreamWriter(writePath, true, System.Text.Encoding.Default);
            sw.WriteLine(StringToWrite);
            sw.Close();
        }

        private void Wlan_Notification(Wlan.WlanNotificationData notifyData)
        {
            switch (notifyData.notificationSource)
            {
                case Wlan.WlanNotificationSource.ACM:
                    switch ((Wlan.WlanNotificationCodeAcm)notifyData.NotificationCode)
                    {
                        case Wlan.WlanNotificationCodeAcm.ScanComplete:

                            this.Cursor = Cursors.WaitCursor;

                            try
                            {
                                this.Scan();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, WiFiThemeContainer.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            this.Cursor = Cursors.Default;

                            break;
                    }
                    break;
            }
        }
    }
}