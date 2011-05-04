// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using LibUsbDotNet;
using CHDKPTP;
using CHDKPTPRemote;

namespace chdk_ptp_test
{
    public partial class Form1 : Form
    {
        private bool connected = false;
        private CHDKPTPDevice connected_device = null;
        private Session session = null;
        private Bitmap live_image = null;
        private StreamWriter Log;
        private int display_width, display_height;

        private void refresh_camera_list()
        {
            LogLine("refreshing camera list...");
            devicecombobox.Items.Clear();
            devicecombobox.Text = "<select a device>";

            try
            {
                foreach (CHDKPTPDevice dev in Session.ListDevices(false))
                {
                    LogLine("found device: " + dev.Name + (dev.CHDKSupported ? " (CHDK PTP supported)" : dev.PTPSupported ? " (PTP supported)" : ""));
                    if (dev.PTPSupported && !dev.CHDKSupported && dev.CHDKVersionMajor != -1)
                        LogLine("CHDK version: " + dev.CHDKVersionMajor + "." + dev.CHDKVersionMinor);
                    if (dev.CHDKSupported)
                        devicecombobox.Items.Add(dev);
                }
                LogLine("done.");
            }
            catch (Exception ex)
            {
                LogLine("exception: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                MessageBox.Show("could not open PTP session: " + ex.Message + "\n\n" + ex.StackTrace.ToString());
            }
        }

        private void LogLine(string s)
        {
            Log.WriteLine(s);
            Log.Flush();
        }

        private void UsbDevice_UsbErrorEvent(object sender, UsbError e)
        {
            LogLine("usb error: " + e.ToString());
            MessageBox.Show("UsbError: " + e.ToString());
        }

        public Form1()
        {
            InitializeComponent();
            Log = File.AppendText("chdk_ptp_test.log");
            LogLine("=== program started ===");
            UsbDevice.UsbErrorEvent += new EventHandler<UsbError>(UsbDevice_UsbErrorEvent);
        }

        ~Form1()
        {
            LogLine("closing...");
            UsbDevice.Exit();
            LogLine("=== program ended ===");
        }

        private void refreshbutton_Click(object sender, EventArgs e)
        {
            refresh_camera_list();
        }

        private void connectbutton_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                MessageBox.Show("Already opened a device!", "Error");
                return;
            }

            if (devicecombobox.SelectedItem == null)
            {
                MessageBox.Show("No device selected!", "Error");
                return;
            }

            connected_device = devicecombobox.SelectedItem as CHDKPTPDevice;

            LogLine("opening device: " + connected_device.Name);
            try
            {
                session = new Session(connected_device);
                session.Connect();
            }
            catch (Exception ex)
            {
                LogLine("exception: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                connected = false;
                connected_device = null;
                session = null;
                MessageBox.Show("could not open PTP session: " + ex.Message + "\n\n" + ex.StackTrace.ToString());
                return;
            }
            LogLine("connected.");
            connected = true;
            statuslabel.Text = "Connected to: " + connected_device.ToString();

            try
            {
                LogLine("getting display dimensions...");
                session.GetDisplayDimensions(out display_width, out display_height);
                LogLine("got "+display_width+"x"+display_height);
            }
            catch (Exception ex)
            {
                LogLine("exception: " + ex.Message + Environment.NewLine + ex.StackTrace);
                MessageBox.Show("could not get display dimensions: " + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        private void disconnectbutton_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                LogLine("closing connection...");
                try
                {
                    session.Disconnect();
                }
                catch (Exception ex)
                {
                    LogLine("exception: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                }

                LogLine("closed.");
                statuslabel.Text = "Not connected";
                connected_device = null;
                connected = false;
            }
        }

        private void getimagebutton_Click(object sender, EventArgs e)
        {
            if (!connected)
                return;

            LogLine("getting live image...");
            try
            {
                if (live_image == null)
                {
                    live_image = session.GetLiveImage();
                }
                else
                {
                    session.GetLiveImage(live_image);
                }
            }
            catch (Exception ex)
            {
                LogLine("exception: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                MessageBox.Show("could not get live image: " + ex.Message + "\n\n" + ex.StackTrace.ToString());
                return;
            }

            Invalidate();
            LogLine("done.");
        }

        private void recordbutton_Click(object sender, EventArgs e)
        {
            if (!connected)
                return;

            LogLine("switching to record mode...");
            try
            {
                session.ExecuteScript("switch_mode_usb(1)");
            }
            catch (Exception ex)
            {
                LogLine("exception: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                MessageBox.Show("could not switch to record mode: " + ex.Message + "\n\n" + ex.StackTrace.ToString());
                return;
            }
            LogLine("done.");
        }

        private void playbackbutton_Click(object sender, EventArgs e)
        {
            if (!connected)
                return;

            LogLine("switching to playback mode...");
            try
            {
                session.ExecuteScript("switch_mode_usb(0)");
            }
            catch (Exception ex)
            {
                LogLine("exception: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                MessageBox.Show("could not switch to playback mode: " + ex.Message + "\n\n" + ex.StackTrace.ToString());
                return;
            }
            LogLine("done.");
        }

        private void shutdownbutton_Click(object sender, EventArgs e)
        {
            LogLine("shutting camera down... (may result in exceptions due to loss of connection)");
            try
            {
                session.ExecuteScript("shut_down()");
            }
            catch (Exception ex)
            {
                LogLine("exception: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
            disconnectbutton.PerformClick();
            LogLine("shut down complete.");
        }

        private void execbutton_Click(object sender, EventArgs e)
        {
            LogLine("executing script: " + scriptedit.Text);
            try
            {
                object r = session.ExecuteScript(scriptedit.Text);
                if (r == null)
                {
                    outputlabel.Text = "(none)";
                }
                else if (r.GetType() == typeof(bool))
                {
                    outputlabel.Text = r.ToString();
                }
                else if (r.GetType() == typeof(int))
                {
                    outputlabel.Text = r.ToString();
                }
                else if (r.GetType() == typeof(string))
                {
                    outputlabel.Text = (string)r;
                }
                else
                {
                    outputlabel.Text = "(unsupported type)";
                }
            }
            catch (Exception ex)
            {
                LogLine("exception: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                outputlabel.Text = ex.Message;
            }
            LogLine("done.");
        }

        private void scriptedit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n')
            {
                execbutton.PerformClick();
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (live_image != null)
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(live_image, getimagebutton.Left, getimagebutton.Bottom + 10, display_width, display_height);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: Load doesn't seem the right place as we don't get usb error messages here
            refresh_camera_list();
        }

    }
}
