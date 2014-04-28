using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PngClipperCore;
using System.IO;

namespace PngClipper
{
    public partial class MainForm : Form
    {

        internal class NameConverter:PngClipperCore.NameConverter
        {
            public String FormatName(Match match, String outPath)
            {
                return outPath + "\\" + match.Groups[1];
            }

            public String GetPattern()
            {
                return @"(\d+)\.png";
            }
       }

        public MainForm()
        {
            InitializeComponent();
            nc_ = new NameConverter();
            pc_ = new PngCutter(this.nc_);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CenterToScreen();
            if( !Directory.Exists(GetWorkingLocation()))
            {
                MessageBox.Show("No inputs!","Error");
                Close();
            }
            else
            {
                UpdateList(pc_.GetNameList(GetWorkingLocation(), GetWorkingLocation()), false);
            }
        }

        private void UpdateList(Dictionary<String, String> dc, bool isChecked)
        {
            listView1.Items.Clear();
            foreach (String s in dc.Keys)
            {
                ListViewItem lvi = listView1.Items.Add(PngCutter.StripFileName(s));
                lvi.SubItems.Add(PngCutter.StripFileName(dc[s]));
                if (isChecked)
                {
                    lvi.SubItems.Add("OK");
                }
            }
            listView1.Update();
        }

        private PngClipperCore.NameConverter GetSearchingPattern()
        {
            return new NameConverter();
        }

        private String GetWorkingLocation()
        {
            return Application.StartupPath + @"\input";
        }

        private String GetDeltaFileName()
        {
            return "dti.txt";
        }

        private void processToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateList(pc_.GetNameList(GetWorkingLocation(), GetWorkingLocation()), false);
            Dictionary<String, String> dc = pc_.ProcessTarget(GetWorkingLocation(), GetWorkingLocation(), GetDeltaFileName());
            UpdateList(dc, true);
            MessageBox.Show("Done");
        }

        private readonly PngClipperCore.NameConverter nc_;
        private PngCutter pc_;
       
    }
}
