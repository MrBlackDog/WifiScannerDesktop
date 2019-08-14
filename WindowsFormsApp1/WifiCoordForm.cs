using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiFi.Scanner
{
    public partial class WifiCoordForm : Form
    {
         
        public WifiCoordForm()
        {           
            InitializeComponent();
        }
        public void UpdateList(List<CoordList> coordLists)
        {
            listView1.Items.Clear();
            foreach (CoordList crl in coordLists)
            {
                foreach (WifiInfo wifiInfos in crl.WifiInfoList)
                {
                    ListViewItem wifiItem = new ListViewItem(new string[] { wifiInfos.SSID, wifiInfos.BSSID,wifiInfos.avglevel.ToString(), wifiInfos.diff.ToString() });
                    listView1.Items.Add(wifiItem);
                }
                listView1.Items.Add("@@@@@@@@@@@@@");
            }
        }


    }
}