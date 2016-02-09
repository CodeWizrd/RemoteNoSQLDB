/////////////////////////////////////////////////////////////////////////
// WriteClient.cs - CommService client sends and receives messages     //
//                - specifically deals with Modify DB Requests & Perst //
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
 * Additions to C# Console Wizard generated code:
 * - Added using System.Threading
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - in this incantation the client has Sender and now has Receiver to
 *   retrieve Server echo-back messages.
 * - If you provide command line arguments they should be ordered as:
 *   remotePort, remoteAddress, localPort, localAddress
 */
/*
 * Maintenance History:
 * --------------------
 * ver 2.1 : 29 Oct 2015
 * - fixed bug in processCommandLine(...)
 * - added rcvr.shutdown() and sndr.shutDown() 
 * ver 2.0 : 20 Oct 2015
 * - replaced almost all functionality with a Sender instance
 * - added Receiver to retrieve Server echo messages.
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using HRTimer;

namespace RemoteNoSqlDB
{
    using Util = Utilities;

    ///////////////////////////////////////////////////////////////////////
    // Client class sends and receives messages in this version
    // - commandline format: /L http://localhost:8085/CommService 
    //                       /R http://localhost:8080/CommService
    //   Either one or both may be ommitted

    class WriteClient
    {
        static int WriteClientCount = 0;
        int clientCount, rcvCount = 0;
        Receiver rcvr;
        Sender sndr;
        List<HiResTimer> latencyTimers = new List<HiResTimer>();
        List<ulong> latencyValues = new List<ulong>();
        string localUrl { get; set; } = "http://localhost:9082/CommService";
        string remoteUrl { get; set; } = "http://localhost:8080/CommService";

        //----< Constructor for Read Client >--------------------------------
        WriteClient()
        {
            WriteClient.WriteClientCount++;
            clientCount = WriteClient.WriteClientCount;
        }

        //----< retrieve urls from the CommandLine if there are any >--------
        public void processCommandLine(string[] args)
        {
            if (args.Length == 0)
                return;

            int count = int.Parse(args[0]);
            WriteClient.WriteClientCount = count;
            clientCount = WriteClient.WriteClientCount;

            localUrl = Util.processCommandLineForLocal(args, localUrl);
            remoteUrl = Util.processCommandLineForRemote(args, remoteUrl);
        }

        //----< retrieve and parse contents of xml file >--------------------
        public List<Message> ProcessXML(string filename)
        {
            List<Message> MessageList = new List<Message>();

            XDocument xml = XDocument.Load(filename);
            IEnumerable<XElement> messages = xml.Root.Element("WriteMessages").Elements("Message");
            foreach (XElement message in messages)
            {
                Message msg;
                XElement msgInstance = message.FirstNode as XElement;

                switch (msgInstance.Name.ToString())
                {
                    case "Add":
                        msg = ProcessAdd();
                        break;
                    case "Edit":
                        msg = ProcessEdit();
                        break;
                    case "Remove":
                        msg = ProcessRemove();
                        break;
                    case "Persist":
                        msg = ProcessPersist(msgInstance);
                        break;
                    case "Restore":
                        msg = ProcessRestore(msgInstance);
                        break;
                    default:
                        msg = ProcessShowDB(msgInstance);
                        break;
                }
                MessageList.Add(msg);
            }
            return MessageList;
        }

        //----< Create Message for Add Request >-----------------------------
        private Message ProcessAdd()
        {
            MessageMaker maker = new MessageMaker();

            Message msg = maker.makeAddRequest(
                localUrl, remoteUrl, clientCount
            );

            return msg;
        }

        //----< Create Message for Remove Request >--------------------------
        private Message ProcessRemove()
        {
            MessageMaker maker = new MessageMaker();
            Message msg = maker.makeRmvRequest(
                localUrl, remoteUrl, clientCount
            );

            return msg;
        }

        //----< Create Message for Edit Request >----------------------------
        private Message ProcessEdit()
        {
            MessageMaker maker = new MessageMaker();
            Message msg = maker.makeEditRequest(
                localUrl, remoteUrl, clientCount
            );

            return msg;
        }

        //----< Create Message for Persist Request >-------------------------
        private Message ProcessPersist(XElement persist)
        {
            XElement file = persist.Element("Filename");
            string filename = file.Value.ToString();

            MessageMaker maker = new MessageMaker();
            Message msg = maker.makePersistRequest(
                localUrl, remoteUrl, filename, clientCount
            );

            return msg;
        }

        //----< Create Message for Restore Request >-------------------------
        private Message ProcessRestore(XElement persist)
        {
            XElement file = persist.Element("Filename");
            string filename = file.Value.ToString();

            MessageMaker maker = new MessageMaker();
            Message msg = maker.makeRestoreRequest(
                localUrl, remoteUrl, filename, clientCount
            );

            return msg;
        }

        //----< Create Message for DB Display Request >----------------------
        private Message ProcessShowDB(XElement showDB)
        {
            MessageMaker maker = new MessageMaker();
            Message msg = maker.makeDisplayRequest(
                localUrl,
                remoteUrl
            );
            return msg;
        }

        public Action defaultServiceAction()
        {
            Action serviceAction = () =>
            {
                if (Util.verbose)
                    Console.Write("\n  starting Receiver.defaultServiceAction");
                Message msg = null;
                while (true)
                {
                    msg = rcvr.getMessage();   // note use of non-service method to deQ messages
                    //
                    Console.Write("\n  Received message:");
                    Console.Write("\n  sender is {0}", msg.fromUrl);
                    if (msg.messageID > 0)
                    {
                        HiResTimer timer = latencyTimers.ElementAt(msg.messageID);
                        timer.Stop();
                        latencyValues[msg.messageID] = timer.ElapsedMicroseconds;
                        Console.Write("\n  Request ID: {0}\n", msg.messageID);
                        Console.Write("\n  content is {0}\n", msg.content);
                        Console.Write("\n  Latency is {0} microseconds\n", latencyValues.ElementAt(msg.messageID));
                    }
                    rcvCount++;
                    //serverProcessMessage(msg);
                    if (msg.content == "closeReceiver")
                        break;
                }
            };
            return serviceAction;
        }

        public void performanceAnalysis()
        {
            int i = 1;
            ulong total = 0, average;
            Console.WriteLine("\n  =====================================================");
            Console.Write("  \n Performance Summary");
            Console.WriteLine("\n  =====================================================");
            for (i = 1; i < latencyValues.Count(); i++)
            {
                total += latencyValues.ElementAt(i);
                Console.WriteLine("  Message ID: {0}  \t Latency: {1} microseconds", i, latencyValues.ElementAt(i));
            }
            average = total / (ulong)(i - 2);
            Console.WriteLine("  =====================================================");
            Console.WriteLine("   Average Latency: {0} microseconds", average);
            Console.WriteLine("  =====================================================");
            savePerfAnalysis(average);
        }

        private void savePerfAnalysis(ulong average)
        {
            string filename = "../../../PerfAnalysis.xml";

            int test = 0;
            while (test < 50)
            {
                ++test;
                try
                {
                    if (File.Exists(filename))
                    {
                        XElement perf = null;
                        perf = XElement.Load(filename);
                        XElement clients = perf.Element("Clients");
                        XElement client = new XElement("Client");
                        client.SetAttributeValue("type", "write");
                        client.SetAttributeValue("id", clientCount);
                        client.SetValue(average.ToString());
                        clients.Add(client);
                        string filecontent = perf.ToString();
                        File.WriteAllText(filename, filecontent);
                        break;
                    }
                }
                catch (IOException)
                {
                    //Console.Write("{0}", e.ToString());
                    Thread.Sleep(550);
                }
                catch (Exception)
                {
                    //Console.Write("{0}", e.ToString());
                    Thread.Sleep(550);
                }
            }    
        }

        private bool ProcessCommandArgsAndFile(WriteClient clnt, string[] args)
        {
            Console.Write("\n  starting CommService client");
            Console.Write("\n =============================\n");

            clnt.processCommandLine(args);
            Console.Title = "Write Client #" + clnt.clientCount.ToString();

            string localPort = Util.urlPort(clnt.localUrl);
            string localAddr = Util.urlAddress(clnt.localUrl);
            clnt.rcvr = new Receiver(localPort, localAddr);
            if (clnt.rcvr.StartService())
            {
                clnt.rcvr.doService(clnt.defaultServiceAction());
            }

            clnt.sndr = new Sender(clnt.localUrl);
            Console.Write("\n  sender's url is {0}", clnt.localUrl);
            Console.Write("\n  attempting to connect to {0}\n", clnt.remoteUrl);

            if (!sndr.Connect(clnt.remoteUrl))
            {
                Console.Write("\n  could not connect in {0} attempts", sndr.MaxConnectAttempts);
                sndr.shutdown(); clnt.rcvr.shutDown(); return false;
            }
            return true;
        }

        //----< Entry point into Write Client >------------------------------
        static void Main(string[] args)
        {
            WriteClient clnt = new WriteClient();
            bool success = clnt.ProcessCommandArgsAndFile(clnt, args);
            if (!success) return;

            HiResTimer timer = new HiResTimer(); timer.Start();
            List<Message> messageList = clnt.ProcessXML("MessagePrototype.xml");
            clnt.latencyTimers.Add(new HiResTimer());
            clnt.latencyValues.Add(new ulong());
            foreach (Message message in messageList)
            {
                if (clnt.sndr.sendMessage(message))
                {
                    HiResTimer temp = new HiResTimer();
                    temp.Start(); clnt.latencyTimers.Add(temp);
                    clnt.latencyValues.Add(new ulong());
                    Console.Write("\n  Request ID: {0}\n", message.messageID);
                    Console.WriteLine(message.content);
                }
                else
                {
                    Console.Write("\n  could not connect in {0} attempts", clnt.sndr.MaxConnectAttempts);
                    clnt.sndr.shutdown(); clnt.rcvr.shutDown(); return;
                }
            }
            timer.Stop(); Console.WriteLine("\n  =====================================================");
            Console.WriteLine("\n   Total time to send all the messages: {0} microseconds", timer.ElapsedMicroseconds);
            Console.WriteLine("\n  =====================================================");

            Message msg = new Message();
            msg.fromUrl = clnt.localUrl;
            msg.toUrl = clnt.remoteUrl;
            msg.content = "done";
            clnt.sndr.sendMessage(msg);
            while (true)
            {
                if (clnt.rcvCount == messageList.Count())
                {
                    Thread.Sleep(50);
                    clnt.performanceAnalysis();
                    break;
                }
            }
            Util.waitForUser();
            clnt.rcvr.shutDown();
            clnt.sndr.shutdown();
            Console.Write("\n\n");
        }
    }
    }

