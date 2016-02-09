/////////////////////////////////////////////////////////////////////////
// MainWindows.xaml.cs - CommService GUI Client                        //
//                     - Composite client - read + write + Perf. Anal. //
// Ver 2.1                                                             //
// Application: Demonstration for CSE681-SMA, Project #4               //
// Language:    C#, ver 6.0, Visual Studio 2015                        //
// Platform:    Dell Inspiron 7520, Core-i7, Windows 10                //
// Author:      Sampath T Janardhan (508899838), SU                    //
//              (315) 664-8206, storagar@syr.edu                       //
// Source:      Jim Fawcett, CST 4-187, Syracuse University            //
//              (315) 443-3948, jfawcett@twcny.rr.com                  //
/////////////////////////////////////////////////////////////////////////

/*
 * Additions to C# WPF Wizard generated code:
 * - Added reference to ICommService, Sender, Receiver, MakeMessage, Utilities
 * - Added using Project4Starter
 *
 * Note:
 * - This client receives and sends messages.
 */
/*
 * Plans:
 * - Add message decoding and NoSqlDb calls in performanceServiceAction.
 * - Provide requirements testing in requirementsServiceAction, perhaps
 *   used in a console client application separate from Performance 
 *   Testing GUI.
 */
/*
 * Maintenance History:
 * --------------------
 * ver 2.1 : 19 Nov 2015
 * - completed design for Project #4 submission
 * ver 2.0 : 29 Oct 2015
 * - changed Xaml to achieve more fluid design
 *   by embedding controls in grid columns as well as rows
 * - added derived sender, overridding notification methods
 *   to put notifications in status textbox
 * - added use of MessageMaker in send_click
 * ver 1.0 : 25 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using System.Xml.Linq;
using System.IO;
using RemoteNoSqlDB;

namespace WpfApplication1
{
    public partial class MainWindow : Window
    {
        static bool firstConnect = true;
        static Receiver rcvr = null;
        static wpfSender sndr = null;
        string localAddress = "localhost";
        string localPort = "8089";
        string remoteAddress = "localhost";
        string remotePort = "8080";
        int querySelectedInt = 3;
        static int readPort = 8090;
        static int writePort = 9090;
        static int readClientCount = 1;
        static int writeClientCount = 1;
        MessageParser parser = new MessageParser();

        /////////////////////////////////////////////////////////////////////
        // nested class wpfSender used to override Sender message handling
        // - routes messages to status textbox
        public class wpfSender : Sender
        {
            TextBox lStat_ = null;  // reference to UIs local status textbox
            System.Windows.Threading.Dispatcher dispatcher_ = null;

            public wpfSender(TextBox lStat, System.Windows.Threading.Dispatcher dispatcher)
            {
                dispatcher_ = dispatcher;  // use to send results action to main UI thread
                lStat_ = lStat;
            }
            public override void sendMsgNotify(string msg)
            {
                Action act = () => { lStat_.Text = msg; };
                dispatcher_.Invoke(act);
            }
            public override void sendExceptionNotify(Exception ex, string msg = "")
            {
                Action act = () => { lStat_.Text = ex.Message; };
                dispatcher_.Invoke(act);
            }
            public override void sendAttemptNotify(int attemptNumber)
            {
                Action act = null;
                act = () => { lStat_.Text = String.Format("attempt to send #{0}", attemptNumber); };
                dispatcher_.Invoke(act);
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            lAddr.Text = localAddress;
            lPort.Text = localPort;
            rAddr.Text = remoteAddress;
            rPort.Text = remotePort;
            Title = "Prototype WPF Client";
            send.IsEnabled = false;
            deactivateButtons();
        }

        //----< trim off leading and trailing white space >------------------
        string trim(string msg)
        {
            StringBuilder sb = new StringBuilder(msg);
            for(int i=0; i<sb.Length; ++i)
            if (sb[i] == '\n')
                sb.Remove(i,1);
            return sb.ToString().Trim();
        }

        //----< indirectly used by child receive thread to post results >----
        public void postRcvMsg(string content)
        {
            TextBlock item = new TextBlock();
            item.Text = trim(content);
            item.FontSize = 16;
            rcvmsgs.Items.Insert(0, item);
            displayRes(content);
            //processResponse(content);
        }

        //----< process displays in other tabs >-----------------------------
        public void displayRes(string content)
        {
            //string content = msg.content;
            if (tabControl.SelectedIndex == 6 && content.Contains("<displayResult>"))
            {
                if (dispDB.Items.Count > 0) dispDB.Items.Clear();
                content = content.Remove(0, 15);
                content = content.Remove(content.Length - 16, 16);
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 16;
                dispDB.Items.Insert(0, item);
            }
            else if (tabControl.SelectedIndex == 4 && content.Contains("<queryResult>"))
            {
                if (rcvqres.Items.Count > 0) rcvqres.Items.Clear();
                content = content.Remove(0, 13);
                content = content.Remove(content.Length - 13, 13);
                int pos = 0, pos2 = 0; List<int> keys = new List<int>();
                string str = "";
                if (content.Length > 8) str = content.Substring(0, 8);
                if (str.Contains("<Key>"))
                {
                    pos = content.IndexOf("<Key>", pos);
                    while (pos > -1)
                    {
                        pos2 = content.IndexOf("</Key>", pos);
                        string key = content.Substring(pos + 6, pos2 - (pos + 6) - 1);
                        keys.Add(int.Parse(key));
                        pos += 6;
                        pos = content.IndexOf("<Key>", pos2);
                    }
                    pos = 0;
                    foreach (int key in keys)
                    {
                        TextBlock item = new TextBlock();
                        item.Text = trim(key.ToString());
                        item.FontSize = 16;
                        rcvqres.Items.Insert(pos++, item);
                    }
                }
                else
                {
                    TextBlock item = new TextBlock();
                    item.Text = trim(content);
                    item.FontSize = 16;
                    rcvqres.Items.Insert(0, item);
                }
            }
        }

        //----< indirectly used by child receive thread to post results >----
        public void processRcvMsg(Message msg)
        {
            postRcvMsg( msg.content);

            processResponse(msg);
        }

        //----< process response messages to sent requests >-----------------
        public void processResponse(Message msg)
        {
            if (msg.type == 3) ProcessGetValueRes(msg);
            else if (msg.type == 1) ProcessAddResult(msg);
            else if (msg.type == 2) ProcessRmvResult(msg);
            else if (msg.type == 4) ProcessEditResult(msg);
            else if (msg.type == 6) ProcessPrstResult(msg);
            else if (msg.type == 7) ProcessRstrResult(msg);
            else if (msg.type == 8) ProcessTypeResult(msg);
        }

        //----< Process results of Edit Request >----------------------------
        private void ProcessGetValueRes(Message msg)
        {
            string name, descr, children, payload;
            bool success;
            parser.decodeValueResult(msg, out name, out descr, out children, out payload, out success);
            if (success)
            {
                keyInput.IsEnabled = false;
                nameInput.Text = name;
                descInput.Text = descr;
                childInput.Text = children;
                dataInput.Text = payload;
            }
            else
            {
                keyInput.IsEnabled = true;
                keyInput.Text = "";
                statusbarItem.Text = "Key does not exist!";
                nameInput.Text = "";
                descInput.Text = "";
                childInput.Text = "";
                dataInput.Text = "";
            }
        }

        //----< Process results of Add Request >-----------------------------
        private void ProcessAddResult(Message msg)
        {
            bool success = false;
            parser.decodeAddResult(msg, out success);
            if (success) statusbarItem.Text = "Key/Value Added";
            else statusbarItem.Text = "Add operation Failed!";
        }

        //----< Process results of Remove Request >--------------------------
        private void ProcessRmvResult(Message msg)
        {
            bool success = false;
            parser.decodeRmvResult(msg, out success);
            if (success) statusbarItem.Text = "Key/Value Removed";
            else statusbarItem.Text = "Remove operation Failed!";
        }

        //----< Process results of Persist Request >-------------------------
        private void ProcessPrstResult(Message msg)
        {
            bool success = false;
            parser.decodePersistResult(msg, out success);
            if (success) statusbarItem.Text = "Persisted to " + filenameInput.Text + ".xml";
            else statusbarItem.Text = "Persist Failed!";
        }

        //----< Process results of Restore Request >-------------------------
        private void ProcessRstrResult(Message msg)
        {
            bool success = false;
            parser.decodeRestoreResult(msg, out success);
            if (success) statusbarItem.Text = "Restored from " + filenameInput.Text + ".xml";
            else statusbarItem.Text = "DB Restore Failed!";
        }

        //----< Process results of Edit Request >----------------------------
        private void ProcessEditResult(Message msg)
        {
            bool success = false;
            parser.decodeEditResult(msg, out success);
            if (success) statusbarItem.Text = "Edits saved successfully";
            else statusbarItem.Text = "Could not save edits!";
        }

        private void ProcessTypeResult(Message msg)
        {
            string keytype, valuetype;
            parser.decodeTypeResult(msg, out keytype, out valuetype);
            keyTypeInfo.Text = keytype;
            valueTypeInfo.Text = valuetype;
            keyTypeInfo2.Text = keytype;
            valueTypeInfo2.Text = valuetype;
        }

        //----< used by main thread >----------------------------------------
        public void postSndMsg(string content)
        {
            TextBlock item = new TextBlock();
            item.Text = trim(content);
            item.FontSize = 16;
            sndmsgs.Items.Insert(0, item);
        }

        //----< get Receiver and Sender running >----------------------------
        void setupChannel()
        {
            rcvr = new Receiver(localPort, localAddress);
            Action serviceAction = () =>
            {
            try
            {
                Message rmsg = null;
                while (true)
                {
                        rmsg = rcvr.getMessage();
                        //Action act = () => { postRcvMsg(rmsg.content); };
                        Action act = () => { processRcvMsg(rmsg); };
                        Dispatcher.Invoke(act, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch(Exception ex)
            {
                Action act = () => { lStat.Text = ex.Message; };
                Dispatcher.Invoke(act);
            }
            };
            if (rcvr.StartService())
            {
            rcvr.doService(serviceAction);
            }

            sndr = new wpfSender(lStat, this.Dispatcher);
        }

        private void activateButtons()
        {
            addReadClient.IsEnabled = true;
            addWriteClient.IsEnabled = true;
            getperfresults.IsEnabled = true;
            retreiveKey.IsEnabled = true;
            rmvKey.IsEnabled = true;
            reset.IsEnabled = true;
            addKey.IsEnabled = true;
            savKey.IsEnabled = true;
            getQueryRes.IsEnabled = true;
            persist.IsEnabled = true;
            restore.IsEnabled = true;
            showdb.IsEnabled = true;
        }

        private void deactivateButtons()
        {
            addReadClient.IsEnabled = false;
            addWriteClient.IsEnabled = false;
            getperfresults.IsEnabled = false;
            retreiveKey.IsEnabled = false;
            rmvKey.IsEnabled = false;
            reset.IsEnabled = false;
            addKey.IsEnabled = false;
            savKey.IsEnabled = false;
            getQueryRes.IsEnabled = false;
            persist.IsEnabled = false;
            restore.IsEnabled = false;
            showdb.IsEnabled = false;
        }

        //----< set up channel after entering ports and addresses >----------
        private void start_Click(object sender, RoutedEventArgs e)
        {
            localPort = lPort.Text;
            localAddress = lAddr.Text;
            remoteAddress = rAddr.Text;
            remotePort = rPort.Text;

            if (firstConnect)
            {
            firstConnect = false;
            if (rcvr != null)
                rcvr.shutDown();
            setupChannel();
            }
            rStat.Text = "connect setup";
            send.IsEnabled = true;
            connect.IsEnabled = false;
            lPort.IsEnabled = false;
            lAddr.IsEnabled = false;

            activateButtons();

            sendTyprReq();
        }

        private void sendTyprReq()
        {
            MessageMaker maker = new MessageMaker();

            Message msg = maker.makeTypeRequest(
                Utilities.makeUrl(lAddr.Text, lPort.Text),
                Utilities.makeUrl(rAddr.Text, rPort.Text)
            );
            if (sndr.sendMessage(msg))
            {
                string content = msg.content;
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 16;
                sndmsgs.Items.Insert(0, item);
            }
        }

        //----< send a demonstraton message >--------------------------------
        private void send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region
                /////////////////////////////////////////////////////
                // This commented code was put here to allow
                // user to change local port and address after
                // the channel was started.  
                // It does what is intended, but would throw 
                // if the new port is assigned a slot that
                // is in use or has been used since the
                // TCP tables were last updated.
                // if (!localPort.Equals(lPort.Text))
                // {
                //   localAddress = rcvr.address = lAddr.Text;
                //   localPort = rcvr.port = lPort.Text;
                //   rcvr.shutDown();
                //   setupChannel();
                // }
                #endregion
                if (!remoteAddress.Equals(rAddr.Text) || !remotePort.Equals(rPort.Text))
                {
                    remoteAddress = rAddr.Text; remotePort = rPort.Text;
                }
                MessageMaker maker = new MessageMaker();
                Message msg = maker.makeMessage(
                    Utilities.makeUrl(lAddr.Text, lPort.Text), 
                    Utilities.makeUrl(rAddr.Text, rPort.Text) );
                lStat.Text = "sending to" + msg.toUrl;
                sndr.localUrl = msg.fromUrl;
                sndr.remoteUrl = msg.toUrl;
                lStat.Text = "attempting to connect";
                if (sndr.sendMessage(msg)) lStat.Text = "connected";
                else lStat.Text = "connect failed";
                postSndMsg(msg.content);
            }
            catch(Exception ex)
            {
                lStat.Text = ex.Message;
            }
        }

        //----< send a demonstraton message >--------------------------------
        private void addKey_Click(object sender, RoutedEventArgs e)
        {
            MessageMaker maker = new MessageMaker();

            string childrn = childInput.Text.ToString();
            List<string> children = extractChildren(childrn);

            Message msg = maker.makeAddRequest(
                Utilities.makeUrl(lAddr.Text, lPort.Text),
                Utilities.makeUrl(rAddr.Text, rPort.Text),
                keyInput.Text.ToString(),
                nameInput.Text.ToString(),
                descInput.Text.ToString(),
                dataInput.Text.ToString(),
                children
            );
            if (sndr.sendMessage(msg))
            {
                string content = msg.content;
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 16;
                sndmsgs.Items.Insert(0, item);
            }
            
        }

        private List<string> extractChildren(string csv)
        {
            List<string> children = new List<string>();
            string substr = "";
            for (int i = 0; i < csv.Length; i++)
            {
                if (csv.ElementAt(i) == ',')
                {
                    children.Add(substr);
                    substr = "";
                } else
                {
                    substr += csv.ElementAt(i);
                }
            }
            children.Add(substr);

            foreach (string child in children)
            {
                trim(child);
            }

            return children;
        }

        //----< send a remve key request >-----------------------------------
        private void rmvKey_Click(object sender, RoutedEventArgs e)
        {
            MessageMaker maker = new MessageMaker();

            Message msg = maker.makeRmvRequest(
                Utilities.makeUrl(lAddr.Text, lPort.Text),
                Utilities.makeUrl(rAddr.Text, rPort.Text),
                keyInput.Text.ToString()
            );
            if (sndr.sendMessage(msg))
            {
                string content = msg.content;
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 16;
                sndmsgs.Items.Insert(0, item);
            }
        }

        private void rtrvKey_Click(object sender, RoutedEventArgs e)
        {
            MessageMaker maker = new MessageMaker();

            Message msg = maker.makeValueRequest(
                Utilities.makeUrl(lAddr.Text, lPort.Text),
                Utilities.makeUrl(rAddr.Text, rPort.Text),
                keyInput.Text.ToString()
            );

            if (sndr.sendMessage(msg))
            {
                string content = msg.content;
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 16;
                sndmsgs.Items.Insert(0, item);
            }

        }

        private void savKey_Click(object sender, RoutedEventArgs e)
        {
            MessageMaker maker = new MessageMaker();

            string childrn = childInput.Text.ToString();
            List<string> children = extractChildren(childrn);

            Message msg = maker.makeEditRequest(
                Utilities.makeUrl(lAddr.Text, lPort.Text),
                Utilities.makeUrl(rAddr.Text, rPort.Text),
                keyInput.Text.ToString(),
                nameInput.Text.ToString(),
                descInput.Text.ToString(),
                dataInput.Text.ToString(),
                children
                );

            if (sndr.sendMessage(msg))
            {
                string content = msg.content;
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 16;
                sndmsgs.Items.Insert(0, item);
            }
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            keyInput.IsEnabled = true;
            keyInput.Text = "";
            nameInput.Text = "";
            descInput.Text = "";
            childInput.Text = "";
            dataInput.Text = "";
        }

        private void showDB_Click(object sender, RoutedEventArgs e)
        {
            MessageMaker maker = new MessageMaker();

            Message msg = maker.makeDisplayRequest(
                Utilities.makeUrl(lAddr.Text, lPort.Text),
                Utilities.makeUrl(rAddr.Text, rPort.Text)                
            );
            if (sndr.sendMessage(msg))
            {
                string content = msg.content;
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 16;
                sndmsgs.Items.Insert(0, item);
            }
        }

        //----< Get Query Results >------------------------------------------
        private void get_query_Click(object sender, RoutedEventArgs e)
        {
            MessageMaker maker = new MessageMaker();

            Message msg = maker.makeQuery(
                Utilities.makeUrl(lAddr.Text, lPort.Text),
                Utilities.makeUrl(rAddr.Text, rPort.Text),
                querySelectedInt, queryText.Text
            );
            if (sndr.sendMessage(msg))
            {
                string content = msg.content;
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 16;
                sndmsgs.Items.Insert(0, item);
            }
        }

        //----< Listener for query list selection >--------------------------
        private void queryselection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            querySelectedInt = (sender as ListView).SelectedIndex;
        }

        //----< Listener for query list selection >--------------------------
        private void addReadClient_Click(object sender, RoutedEventArgs e)
        {
            addRdClient();
        }

        public void addRdClient()
        {
            bool success = startProcess("../../../ReadClient/bin/debug/Client.exe", true);
            if (success) statusbarItem.Text = "Read Client created";
            else statusbarItem.Text = "Unable to create Read client at this time!";
        }

        //----< Listener for query list selection >--------------------------
        private void addWriteClient_Click(object sender, RoutedEventArgs e)
        {
            addWrClient();
        }

        public void addWrClient()
        {
            bool success = startProcess("../../../WriteClient/bin/debug/Client2.exe", false);
            if (success) statusbarItem.Text = "Read Client created";
            else statusbarItem.Text = "Unable to create Read client at this time!";
        }

        public bool startProcess(string process, bool read)
        {
            process = System.IO.Path.GetFullPath(process);
            int clientCount, port;
            if (read)
            {
                clientCount = readClientCount++;
                port = readPort++;
            }
            else
            {
                clientCount = writeClientCount++;
                port = writePort++;
            }
                
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = process,
                Arguments = clientCount.ToString() + " /L http://localhost:" + port.ToString() + "/CommService",
                // set UseShellExecute to true to see child console, false hides console
                UseShellExecute = false
            };
            try
            {
                Process p = Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
        }

        // --<  >--
        private void persist_Click(object sender, RoutedEventArgs e)
        {
            string filename = filenameInput.Text;
            MessageMaker maker = new MessageMaker();
            Message msg = maker.makePersistRequest(
                Utilities.makeUrl(lAddr.Text, lPort.Text),
                Utilities.makeUrl(rAddr.Text, rPort.Text),
                filename + ".xml"
            );
            if (sndr.sendMessage(msg))
            {
                string content = msg.content;
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 16;
                sndmsgs.Items.Insert(0, item);
            }
        }

        private void restore_Click(object sender, RoutedEventArgs e)
        {
            string filename = filenameInput.Text;
            MessageMaker maker = new MessageMaker();
            Message msg = maker.makeRestoreRequest(
                Utilities.makeUrl(lAddr.Text, lPort.Text),
                Utilities.makeUrl(rAddr.Text, rPort.Text),
                filename + ".xml"
            );
            if (sndr.sendMessage(msg))
            {
                string content = msg.content;
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 16;
                sndmsgs.Items.Insert(0, item);
            }
        }


        private void getReadPerf(XElement clientsX)
        {
            ulong total = 0, count = 0;
            double average = 0;
            IEnumerable<XElement> clients = clientsX.Elements("Client");
            if (clients.Count() > 0)
            {
                foreach (XElement client in clients)
                {
                    if (client.Attribute("type").Value.ToString() == "read")
                    {
                        count++;
                        total += ulong.Parse(client.Value.ToString());
                    }
                }
            }
            if (total == 0)
            {
                readlatency.Text = "No data available";
            } else
            {
                average = (double)total / count;
                average = Math.Round(average, 2);
                readlatency.Text = average.ToString() + " microseconds";
            }
        }

        private void getWritePerf(XElement clientsX)
        {
            ulong total = 0, count = 0;
            double average = 0;
            IEnumerable<XElement> clients = clientsX.Elements("Client");
            if (clients.Count() > 0)
            {
                foreach (XElement client in clients)
                {
                    string attribute = client.Attribute("type").Value.ToString();
                    if ( attribute == "write")
                    {
                        count++;
                        total += ulong.Parse(client.Value.ToString());
                    }
                }
            }
            if (total == 0)
            {
                writelatency.Text = "No data available";
            }
            else
            {
                average = (double)total / count;
                average = Math.Round(average, 2);
                writelatency.Text = average.ToString() + " microseconds";
            }
        }

        private void getServerPerf(XElement server)
        {
            double total = 0, average = 0;
            int count = 0;
            IEnumerable<XElement> snaps = server.Elements("Snapshot");

            if (snaps.Count() > 0)
            {
                foreach (XElement snap in snaps)
                {
                    count++;
                    total += double.Parse(snap.Value.ToString());
                }
            }
            if (total == 0)
            {
                throughput.Text = "No data available";
            }
            else
            {
                average = total / count;
                average = Math.Round(average, 2);
                throughput.Text = (average*10).ToString() + " operations every 10 milliseconds";
            }
        }

        //
        private void getPerf_Click(object sender, RoutedEventArgs e)
        {
            string filename = "../../../PerfAnalysis.xml";
            int test = 0;
            while (test < 50)
            {
                test++;
                try
                {
                    if (File.Exists(filename))
                    {
                        XElement perf = null;
                        perf = XElement.Load(filename);

                        getReadPerf(perf.Element("Clients"));
                        getWritePerf(perf.Element("Clients"));
                        getServerPerf(perf.Element("Server"));

                        break;
                    }
                }
                catch (IOException)
                {
                    //Console.Write("{0}", e.ToString());
                    Thread.Sleep(450);
                }
                catch (Exception)
                {
                    //Console.Write("{0}", e.ToString());
                    Thread.Sleep(450);
                }
            }
        }
    }
}
