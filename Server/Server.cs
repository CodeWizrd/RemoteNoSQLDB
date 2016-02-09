/////////////////////////////////////////////////////////////////////////
// Server.cs - CommService server                                      //
// Ver 2.4                                                             //
// Application: Demonstration for CSE681-SMA, Project #4               //
// Language:    C#, ver 6.0, Visual Studio 2015                        //
// Platform:    Dell Inspiron 7520, Core-i7, Windows 10                //
// Author:      Sampath T Janardhan (508899838), SU                    //
//              (315) 664-8206, storagar@syr.edu                       //
// Source:      Jim Fawcett, CST 4-187, Syracuse University            //
//              (315) 443-3948, jfawcett@twcny.rr.com                  //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - This server now receives and then sends back received messages.
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
 * ver 2.4 : 19 Nov 2015
 * - added processing of client requests
 * - Added Performance analysis (Throughput)
 * ver 2.3 : 29 Oct 2015
 * - added handling of special messages: 
 *   "connection start message", "done", "closeServer"
 * ver 2.2 : 25 Oct 2015
 * - minor changes to display
 * ver 2.1 : 24 Oct 2015
 * - added Sender so Server can echo back messages it receives
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 2.0 : 20 Oct 2015
 * - Defined Receiver and used that to replace almost all of the
 *   original Server's functionality.
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using System.Timers;
using HRTimer;
using NoSqlDB;

namespace RemoteNoSqlDB
{
    using Util = Utilities;
    using DBElem = DBElement<int, string>;
    using DB = DBEngine<int, DBElement<int, string>>;

    class Server
    {
        public MessageMaker maker { get; set; }
        public MessageParser parser { get; set; }
        string address { get; set; } = "localhost";
        string port { get; set; } = "8080";
        DB db;
        HiResTimer perfTimer = new HiResTimer();
        ulong totalTime = 0; int reqCount = 0;

        //----< Default Constructor >--------------------------------------------
        Server()
        {
            SetupDB();
            maker = new MessageMaker();
            parser = new MessageParser();
        }

        //----< Import DB<Key, Value> from file >--------------------------------
        public void SetupDB()
        {
            db = new DB();
            PersistEngine.augment<int, DBElem, string>(db, "persistFile.xml");
        }

        //----< quick way to grab ports and addresses from commandline >---------
        public void ProcessCommandLine(string[] args)
        {
            if (args.Length > 0)
            {
            port = args[0];
            }
            if (args.Length > 1)
            {
            address = args[1];
            }
        }

        //----< process query requests and frame appropriate reply message >-----
        public Message ProcessQuery(Message msg)
        {
            Console.WriteLine("Received Query Request");
            string queryString; int queryType;
            parser.decodeQuery(msg, out queryType, out queryString);

            Message result;
            perfTimer.Start();

            if (queryType == 0)
            {
                string content = QueryOperations(queryString);
                content = "<Value>" + content + "</Value>";
                result = maker.makeQueryReult(msg, content);
            }
            else
            {
                List<string> keys = QueryOperations(queryType, queryString);
                result = maker.makeQueryReult(msg, keys);
            }
            perfTimer.Stop();
            reqCount++;
            totalTime += perfTimer.ElapsedMicroseconds;

            Console.WriteLine("Sent Query Result");
            return result;
        }

        //----< perform query operations on DB and return results >--------------
        public string QueryOperations(string queryString)
        {
            string res = "";
            QueryEngine queryEngine = new QueryEngine();

            DBElem value = QueryEngine.queryValue<int, DBElem, string>(int.Parse(queryString), db);
            if (value != null) res = value.ToXml();
            return res;
        }

        //----< perform query operations on DB and return results >--------------
        public List<string> QueryOperations(int queryType, string queryString)
        {
            List<string> keys = new List<string>();
            QueryEngine queryEngine = new QueryEngine();
            Func<int, bool> q;
            List<int> keyCollection;
            bool r;
            DBFactory<int, DBElem> resDb;

            if (queryType == 1) {
                q = queryEngine.queryChildren<int, DBElem, string>(db);
                r = queryEngine.processChildrenQuery<int, DBElem, string>(q, out keyCollection, int.Parse(queryString), db);
                resDb = queryEngine.queryResults<int, DBElem, string>(r, keyCollection, db);
                ExtractKeysXML<int, DBElem>(resDb, out keys);
            } else 
            {
                switch (queryType)
                {
                    case 2:  q = queryEngine.queryRegEx<int, DBElem, string>(db, queryString);
                        break;
                    case 3:  q = queryEngine.queryMetadata<int, DBElem, string>(queryString, db);
                        break;
                    case 4:  TimeSpan tspan = new TimeSpan(0, 0, 0, 0, 300);
                        DateTime initial = DateTime.Now.Subtract(tspan);
                        q = queryEngine.queryTimestamp<int, DBElem, string>(db, initial);
                        //q = queryEngine.queryTimestamp<int, DBElem, string>(db, initial, final);
                        break;
                    default: q = queryEngine.queryMetadata<int, DBElem, string>(queryString, db);
                        break;
                }
                r = queryEngine.processQuery<int, DBElem>(q, out keyCollection, db);
                resDb = queryEngine.queryResults<int, DBElem, string>(r, keyCollection, db);
                ExtractKeysXML<int, DBElem>(resDb, out keys);
            }
            return keys;
        }

        //----< convert DB of Keys to List<Key> >--------------------------------
        public void ExtractKeysXML<Key, Value>(DBFactory<Key, Value> resDb, out List<string> keyList) 
        {
            List<Key> keys = new List<Key>(resDb.Keys());
            keyList = new List<string>();

            if (keys != null && keys.Count > 0)
            {
                foreach (Key key in keys)
                {
                    keyList.Add(key.ToString());
                }
            }
        }

        //----< Process request to Add key >-------------------------------------
        public Message ProcessAddKey(Message msg)
        {
            Console.WriteLine("Received Request to Add Key/Value Pair");

            int key; List<int> children = new List<int>();
            string keyS; List<string> childrenS;
            string name, descr, payload;
            parser.decodeAddRequest(msg, out keyS, out name, out descr, out payload, out childrenS);
            bool success;
            try
            {
                key = int.Parse(keyS);
                if (childrenS.Count() > 0)
                {
                    foreach (string child in childrenS)
                    {
                        children.Add(int.Parse(child));
                    }
                }

                perfTimer.Start();

                DBElem elem = new DBElem(name, descr);
                elem.children.AddRange(children);
                elem.payload = payload;
                success = db.insert(key, elem);

                perfTimer.Stop();
                reqCount++;
                totalTime += perfTimer.ElapsedMicroseconds;
            } catch (Exception)
            {
                success = false;
            }          

            if (success) Console.WriteLine("Added Key/Value Pair");
            else Console.WriteLine("Add Operation Failed");

            Message result = maker.makeAddResult(msg, success);
            return result;
        }

        //----< Process request to Retrieve key's value >------------------------
        public Message ProcessGetValue(Message msg)
        {

            string keyS;
            parser.decodeValueRequest(msg, out keyS);
            bool success;
            try
            {
                int key = int.Parse(keyS);
                perfTimer.Start();

                DBElem elem = new DBElem();
                success = db.getValue(key, out elem);
                if (success) keyS = SerializeValue<int, string>(elem);
                else keyS = "";

                perfTimer.Stop();
                reqCount++;
                totalTime += perfTimer.ElapsedMicroseconds;
            } catch (Exception)
            {
                success = false;
            }

            Message result = maker.makeValueResult(msg, keyS);
            return result;
        }

        //----< Serialize value of a Key >---------------------------------------
        public string SerializeValue<Key, Value>(DBElement<Key, Value> element)
        {
            string xml = String.Format("<Value>\n<name>{0}</name>\n<descr>{1}</descr>\n<timestamp>{2}</timestamp>\n<payload>{3}</payload>\n",
                element.name, element.descr, element.timeStamp, element.PayloadToString());

            if (element.children.Count > 0)
            {
                xml += "<children>\n";
                foreach (Key child in element.children)
                {
                    xml += String.Format("<child>{0}</child>\n", child.ToString());
                }
                xml += "</children>";
            }
            xml += "</Value>";
            return xml;
        }

        //----< Process request to Edit key >------------------------------------
        public Message ProcessEditKey(Message msg)
        {
            int key; List<int> children = new List<int>();
            string keyS; List<string> childrenS;
            string name, descr, payload;
            parser.decodeAddRequest(msg, out keyS, out name, out descr, out payload, out childrenS);

            key = int.Parse(keyS);
            if (childrenS.Count() > 0)
            {
                foreach (string child in childrenS)
                {
                    children.Add(int.Parse(child));
                }
            }

            perfTimer.Start();

            DBElem elem = new DBElem();
            bool success = db.getValue(key, out elem);

            perfTimer.Stop();
            reqCount++;
            totalTime += perfTimer.ElapsedMicroseconds;

            if (success)
            {
                elem.name = name;
                elem.descr = descr;
                elem.timeStamp = DateTime.Now;
                elem.children.AddRange(children);
                elem.payload = payload;
                Console.WriteLine("Edits saved");
            }
            else Console.WriteLine("Edit Operation Failed. Key doesn't exist!");

            Message result = maker.makeEditResult(msg, success);
            return result;
        }

        //----< Process request to Remove key >----------------------------------
        public Message ProcessRmvKey(Message msg)
        {
            Console.WriteLine("Received Request to Remove Key/Value Pair");
            
            string content = "";
            parser.decodeRmvRequest(msg, out content);
            bool success;
            try
            {
                int key = int.Parse(content);

                perfTimer.Start();

                success = db.remove(key);

                perfTimer.Stop();
                reqCount++;
                totalTime += perfTimer.ElapsedMicroseconds;
            }
            catch (Exception)
            {
                success = false;
            }
            

            if (success) Console.WriteLine("Removed Key/Value Pair");
            else Console.WriteLine("Remove Operation Failed");

            Message result = maker.makeRmvResult(msg, success);
            return result;
        }

        //----< Process request to Display DB >----------------------------------
        private Message ProcessDBDisplay(Message msg)
        {
            Console.WriteLine("Received Request to Retrieve DB");

            perfTimer.Start();

            XDocument xDocument = PersistEngine.ToXML<int, DBElem, string>(db);

            perfTimer.Stop();
            reqCount++;
            totalTime += perfTimer.ElapsedMicroseconds;

            Message result = maker.makeDisplayResult(msg, xDocument.ToString());
            return result;
        }

        //----< Process request to Persist DB>-----------------------------------
        private Message ProcessPersist(Message msg)
        {
            bool success = false;
            string filename;
            parser.decodePersistRequest(msg, out filename);

            perfTimer.Start();

            PersistEngine.persist<int, DBElem, string>(db, filename);
            if (File.Exists(filename)) success = true;
            else success = false;

            perfTimer.Stop();
            reqCount++;
            totalTime += perfTimer.ElapsedMicroseconds;

            Message result = maker.makePersistResult(msg, success);
            return result;
        }

        //----< Process request to Restore DB >----------------------------------
        private Message ProcessRestore(Message msg)
        {
            bool success = false;
            string filename;
            parser.decodeRestoreRequest(msg, out filename);

            perfTimer.Start();

            PersistEngine.augment<int, DBElem, string>(db, filename);
            if (db.Keys().Count() > 0) success = true;
            else success = false;

            perfTimer.Stop();
            reqCount++;
            totalTime += perfTimer.ElapsedMicroseconds;

            Message result = maker.makeRestoreResult(msg, success);
            return result;
        }

        //----< Process request to Retreive DB Type >----------------------------
        private Message ProcessTypeReq(Message msg)
        {
            perfTimer.Start();

            string keytype = typeof(int).ToString();
            string valuetype = typeof(string).ToString();

            perfTimer.Stop();
            reqCount++;
            totalTime += perfTimer.ElapsedMicroseconds;

            Message result = maker.makeTypeResult(msg, keytype, valuetype);
            return result;
        }

        //----< Perform Performance Analysis >-----------------------------------
        private void PerformanceAnalysis()
        {
            double totalsecs = totalTime / 1000;
            double average = reqCount / totalsecs;
            
            if (reqCount > 0)
            {
                if (reqCount == 1) average = 1;
                Console.WriteLine("\n  =====================================================");
                Console.Write("  \n  Performance Summary");
                Console.Write("  \n  Total number of DB requests: {0} ", reqCount);
                Console.Write("  \n  Total time taken to process DB requests: {0} microseconds", totalTime);
                Console.WriteLine("\n  Average Throughput: {0} requests every 10 milliseconds", average*10);
                Console.WriteLine("\n  =====================================================");

                if (! (double.IsInfinity(average) || double.IsNaN(average) ))
                {
                    savePerfAnalysis(average);
                }
                    
            }

            totalTime = 0;
            reqCount = 0;
        }

        //----< Save Performance Analysis to XML >-------------------------------
        private void savePerfAnalysis(double average)
        {
            string filename = "../../../PerfAnalysis.xml";

            if (File.Exists(filename))
            {
                XElement perf = null;
                perf = XElement.Load(filename);
                XElement clients = perf.Element("Server");
                XElement client = new XElement("Snapshot");
                client.SetValue(average.ToString());
                clients.Add(client);
                string filecontent = perf.ToString();
                File.WriteAllText(filename, filecontent);
            }
        }

        //----< Schedule Performance Analysis Computation >----------------------
        private void CalculatePerf()
        {
            System.Timers.Timer schedular = new System.Timers.Timer();
            schedular.Interval = 1000;
            schedular.AutoReset = true;
            schedular.Elapsed += (object source, ElapsedEventArgs e) => 
            {
                PerformanceAnalysis();
            };
            schedular.Enabled = true;
            Thread.Sleep(3000);
        }

        //----< Entry point for Server >-----------------------------------------
        static void Main(string[] args)
        {
            Util.verbose = false;
            Server srvr = new Server();
            srvr.ProcessCommandLine(args);

            Console.Title = "Server";
            Console.Write(String.Format("\n  Starting CommService server listening on port {0}", srvr.port));
            Console.Write("\n ====================================================\n");

            Sender sndr = new Sender(Util.makeUrl(srvr.address, srvr.port));
            Receiver rcvr = new Receiver(srvr.port, srvr.address);

            srvr.CalculatePerf();
            // - serviceAction defines what the server does with received messages
            // - This serviceAction just announces incoming messages 
            Action serviceAction = () =>
            {
                Message msg = null;
                while (true)
                {
                    msg = rcvr.getMessage();   // note use of non-service method to deQ messages
                    Console.Write("\n  Received message:");
                    Console.Write("\n  sender is {0}", msg.fromUrl);
                    Console.Write("\n  Request ID: {0}\n", msg.messageID);
                    Console.Write("\n  content is {0}\n", msg.content);
                    if (msg.content == "connection start message") { Console.Write("\n  client connected\n"); continue; }
                    else if (msg.content == "done") {  Console.Write("\n  client has finished\n");  continue;  }
                    else if (msg.content == "closeServer")  {  Console.Write("received closeServer");  break; }
                    else if (msg.type == 0) sndr.sendMessage(srvr.ProcessQuery(msg));
                    else if (msg.type == 1) sndr.sendMessage(srvr.ProcessAddKey(msg));
                    else if (msg.type == 2) sndr.sendMessage(srvr.ProcessRmvKey(msg));
                    else if (msg.type == 3) sndr.sendMessage(srvr.ProcessGetValue(msg));
                    else if (msg.type == 4) sndr.sendMessage(srvr.ProcessEditKey(msg));
                    else if (msg.type == 5) sndr.sendMessage(srvr.ProcessDBDisplay(msg));
                    else if (msg.type == 6) sndr.sendMessage(srvr.ProcessPersist(msg));
                    else if (msg.type == 7) sndr.sendMessage(srvr.ProcessRestore(msg));
                    else if (msg.type == 8) sndr.sendMessage(srvr.ProcessTypeReq(msg));

                    msg.content = "received " + msg.content + " from " + msg.fromUrl;
                    Util.swapUrls(ref msg);
                }
            };
            if (rcvr.StartService()) rcvr.doService(serviceAction); // This serviceAction is asynchronous so the call doesn't block.
            Util.waitForUser(); 
        }
    }
}
