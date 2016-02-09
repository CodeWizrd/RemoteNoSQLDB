/////////////////////////////////////////////////////////////////////////
// MessageMaker.cs - Construct ICommService Messages                   //
// Ver 1.1                                                             //
// Application: Demonstration for CSE681-SMA, Project#2                //
// Language:    C#, ver 6.0, Visual Studio 2015                        //
// Platform:    Dell Inspiron 7520, Core-i7, Windows 10                //
// Author:      Sampath T Janardhan (508899838), SU                    //
//              (315) 664-8206, storagar@syr.edu                       //
// Source:      Jim Fawcett, CST 4-187, Syracuse University            //
//              (315) 443-3948, jfawcett@twcny.rr.com                  //
/////////////////////////////////////////////////////////////////////////
/*
 * Purpose:
 *----------
 * This is a package for application specific message construction
 *
 * Additions to C# Console Wizard generated code:
 * - references to ICommService and Utilities
 */
/*
 * Maintenance History:
 * --------------------
 * ver 1.1 : 17 Nov 2015
 * - Added make<xxx>Request and make<xxx>Result methods
 * ver 1.0 : 29 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RemoteNoSqlDB
{
    public class MessageMaker
    {
        public static int msgCount { get; set; } = 0;
        public Message makeMessage(string fromUrl, string toUrl)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.content = String.Format("\n  message #{0}", ++msgCount);
            return msg;
        }

        public Message makeQuery(string fromUrl, string toUrl, int type, string str)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 0;
            //msg.content = String.Format("\n  Query: {0}", str);
            msg.content = String.Format("<query>\n<querystring>{0}</querystring>\n<querytype>{1}</querytype>\n</query>", str, type);
            return msg;
        }

        public Message makeQueryReult(Message msg, string value)
        {
            Message result = new Message();
            result.fromUrl = msg.toUrl;
            result.toUrl = msg.fromUrl;
            result.type = 0;
            result.messageID = msg.messageID;

            XElement root = new XElement("queryResult");
            XElement content = XElement.Parse(value);
            root.Add(content);

            result.content = root.ToString();
            return result;
        }

        public Message makeQueryReult(Message msg, List<string> keys)
        {
            Message result = new Message();
            result.fromUrl = msg.toUrl;
            result.toUrl = msg.fromUrl;
            result.type = 0;
            result.messageID = msg.messageID;

            XElement root = new XElement("queryResult");
            if (keys.Count() > 0)
            {
                foreach (string key in keys)
                {
                    XElement keyX = new XElement("Key"); keyX.Value = key;
                    root.Add(keyX);
                }
            }

            result.content = root.ToString();
            return result;
        }

        public Message makeDisplayRequest(string fromUrl, string toUrl)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 5;
            msg.content = "<displayRequest/>";
            return msg;
        }

        public Message makeDisplayResult(Message msg, string dbContents)
        {
            Message result = new Message();
            result.fromUrl = msg.toUrl;
            result.toUrl = msg.fromUrl;
            result.type = 5;
            result.messageID = msg.messageID;

            XElement root = new XElement("displayResult");
            root.Add(XElement.Parse(dbContents));

            result.content = root.ToString();
            return result;
        }

        public Message makeAddRequest(string fromUrl, string toUrl, string key, string name, string descr, string payload, 
            List<string> children)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 1;

            XElement root = new XElement("addRequest");
            root.Add(new XElement("Key", key));
            root.Add(new XElement("name", name));
            root.Add(new XElement("descr", descr));

            if (children.Count > 0)
            {
                XElement childrenX = new XElement("children");
                foreach (string child in children)
                {
                    childrenX.Add(new XElement("child", child));
                }
                root.Add(childrenX);
            }

            root.Add(new XElement("Payload", payload));
            msg.content = root.ToString();
            return msg;
        }

        public Message makeAddRequest(string fromUrl, string toUrl, int client)
        {            
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID;
            msg.type = 1;
            int append = (client * 100) + msg.messageID + 1;

            string key = append.ToString();
            string name = "Name of " + append;
            string descr = "Descr of " + append;
            string payload = "Payload of " + append;
            List<string> children = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                children.Add((append + i).ToString());
            }
            return makeAddRequest(fromUrl, toUrl, key, name, descr, payload, children);
        }

        public Message makeAddResult(Message msg, bool success)
        {
            Message result = new Message();
            result.fromUrl = msg.toUrl;
            result.toUrl = msg.fromUrl;
            result.type = 1;
            result.messageID = msg.messageID;
            string body;
            if (success) body = "Action Completed";
            else body = "Action Failed";

            XElement root = new XElement("addResult", body);

            result.content = root.ToString();
            return result;
        }

        public Message makeRmvRequest(string fromUrl, string toUrl, string key)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 2;

            XElement root = new XElement("remvRequest");
            root.Add(new XElement("Key", key));
            msg.content = root.ToString();
            return msg;
        }

        public Message makeRmvRequest(string fromUrl, string toUrl, int client)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 2;
            int append = client * 100 + msg.messageID - 6;

            XElement root = new XElement("remvRequest");
            root.Add(new XElement("Key", append));
            msg.content = root.ToString();
            return msg;
        }

        public Message makeRmvResult(Message msg, bool success)
        {
            Message result = new Message();
            result.fromUrl = msg.toUrl;
            result.toUrl = msg.fromUrl;
            result.type = 2;
            result.messageID = msg.messageID;
            string body;
            if (success) body = "Action Completed";
            else body = "Action Failed";
            result.content = String.Format("<remvRequest>{0}</remvRequest>", body);
            return result;
        }

        public Message makeValueRequest(string fromUrl, string toUrl, string key)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 3;
            msg.content = String.Format("<valueRequest>\n<Key>{0}</Key>\n</valueRequest>", key);
            return msg;
        }

        public Message makeValueResult(Message msg, string value)
        {
            Message result = new Message();
            result.fromUrl = msg.toUrl;
            result.toUrl = msg.fromUrl;
            result.type = 3;
            result.messageID = msg.messageID;
            result.content = String.Format("<valueResult>\n{0}\n</valueResult>", value);
            return result;
        }

        public Message makeEditRequest(string fromUrl, string toUrl, string key, string name, string descr, string payload, List<string> children)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 4;
            string content = String.Format("<editRequest>\n<Key>{0}</Key>\n<name>{1}</name>\n<descr>{2}</descr>\n", key, name, descr);
            if (children.Count > 0)
            {
                content += "<children>\n";
                foreach (string child in children)
                {
                    content += String.Format("<child>{0}</child>\n", child);
                }
                content += "</children>\n";
            }
            content += String.Format("<Payload>{0}</Payload>", payload);
            content += "</editRequest>\n";
            msg.content = content;
            return msg;
        }

        public Message makeEditRequest(string fromUrl, string toUrl, int client)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 4;

            int append = client * 100 + msg.messageID - 5;
            string content = String.Format("<editRequest>\n<Key>{0}</Key>\n<name>{1}</name>\n<descr>{2}</descr>\n",
                    append, "Edited Name of " + append, "Edited Descr of " + append);

            content += "<children>\n";
            for (int i = 0; i < 3; i++)
            {
                content += String.Format("<child>{0}</child>\n", append + 2*i);
            }
            content += "</children>\n";

            content += String.Format("<Payload>{0}</Payload>", "Edited Payload of " + append);
            content += "</editRequest>\n";
            msg.content = content;

            return msg;
        }

        public Message makeEditResult(Message msg, bool success)
        {
            Message result = new Message();
            result.fromUrl = msg.toUrl;
            result.toUrl = msg.fromUrl;
            result.type = 4;
            result.messageID = msg.messageID;
            string body;
            if (success) body = "Action Completed";
            else body = "Action Failed";
            result.content = String.Format("<editResult>{0}</editResult>", body);
            return result;
        }

        public Message makePersistRequest(string fromUrl, string toUrl, string filename)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 6;
            msg.content = String.Format("<PersistRequest>\n<File>{0}</File>\n</PersistRequest>", filename);
            return msg;
        }
        
        public Message makePersistRequest(string fromUrl, string toUrl, string filename, int client)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 6;
            msg.content = String.Format("<PersistRequest>\n<File>{0}</File>\n</PersistRequest>", filename + client.ToString() + ".xml");
            return msg;
        }

        public Message makePersistResult(Message msg, bool success)
        {
            Message result = new Message();
            result.fromUrl = msg.toUrl;
            result.toUrl = msg.fromUrl;
            result.type = 6;
            result.messageID = msg.messageID;
            string body;
            if (success) body = "Action Completed";
            else body = "Action Failed";
            result.content = String.Format("<PersistResult>{0}</PersistResult>",body);
            return result;
        }

        public Message makeRestoreRequest(string fromUrl, string toUrl, string filename)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.messageID = Message.MessageID++;
            msg.type = 7;
            msg.content = String.Format("<RestoreRequest>\n<File>{0}</File>\n</RestoreRequest>", filename);
            return msg;
        }

        public Message makeRestoreRequest(string fromUrl, string toUrl, string filename, int client)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.type = 7;
            msg.messageID = Message.MessageID++;
            msg.content = String.Format("<RestoreRequest>\n<File>{0}</File>\n</RestoreRequest>", filename + client.ToString() + ".xml");
            return msg;
        }

        public Message makeRestoreResult(Message msg, bool success)
        {
            Message result = new Message();
            result.fromUrl = msg.toUrl;
            result.toUrl = msg.fromUrl;
            result.type = 7;
            result.messageID = msg.messageID;
            string body;
            if (success) body = "Action Completed";
            else body = "Action Failed";
            result.content = String.Format("<RestoreRequest>{0}</RestoreRequest>", body);
            return result;
        }

        public Message makeTypeRequest(string fromUrl, string toUrl)
        {
            Message msg = new Message();
            msg.fromUrl = fromUrl;
            msg.toUrl = toUrl;
            msg.type = 8;
            msg.messageID = Message.MessageID++;
            msg.content = "<TypeRequest/>";
            return msg;
        }

        public Message makeTypeResult(Message msg, string keytype, string valuetype)
        {
            Message result = new Message();
            result.fromUrl = msg.toUrl;
            result.toUrl = msg.fromUrl;
            result.type = 8;
            result.messageID = msg.messageID;
            result.content = String.Format("<TypeResult>\n<KeyType>{0}</KeyType>\n<ValueType>{1}</ValueType>\n</TypeResult>", keytype, valuetype);
            return result;
        }


#if (TEST_MESSAGEMAKER)
        static void Main(string[] args)
    {
        MessageMaker mm = new MessageMaker();
        Message msg = mm.makeMessage("fromFoo", "toBar");
        Utilities.showMessage(msg);
        Console.Write("\n\n");
    }
    #endif
    }
}
