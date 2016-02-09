using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteNoSqlDB
{
    public class MessageParser
    {
        public void decodePersistRequest(Message msg, out string filename)
        {
            XElement doc = XElement.Parse( msg.content);
            filename = doc.Element("File").Value.ToString();
        }

        public void decodeRestoreRequest(Message msg, out string filename)
        {
            XElement doc = XElement.Parse(msg.content);
            filename = doc.Element("File").Value.ToString(); 
        }

        public void decodeAddRequest(Message msg, out string key, out string name, out string descr, out string payload, out List<string> children)
        {
            XElement doc = XElement.Parse(msg.content);
            key = doc.Element("Key").Value.ToString();
            name = doc.Element("name").Value.ToString();
            descr = doc.Element("descr").Value.ToString();
            payload = doc.Element("Payload").Value.ToString();
            IEnumerable<XElement> childrenX = doc.Elements("child");
            children = new List<string>();
            if (childrenX.Count() > 0)
            {
                foreach (XElement child in childrenX)
                {
                    children.Add(child.Value.ToString());
                }
            }
        }

        public void decodeValueRequest(Message msg, out string key)
        {
            XElement doc = XElement.Parse(msg.content);
            key = doc.Element("Key").Value.ToString();
        }

        public void decodeValueResult(Message msg, out string name, out string descr, out string children, out string payload, out bool status)
        {
            XElement doc = XElement.Parse(msg.content).Element("Value"); ;
            if (doc == null)
            {
                name = ""; descr = ""; children = ""; payload = "";
                status = false;
            }
            else
            {
                status = true;
                XElement temp = doc.Element("name");
                name = temp.Value.ToString();
                descr = doc.Element("descr").Value.ToString();
                payload = doc.Element("payload").Value.ToString();
                children = "";
                IEnumerable<XElement> childrenX = doc.Element("children").Elements("child");
                bool flag = true;
                if (childrenX.Count() > 0)
                {
                    foreach (XElement child in childrenX)
                    {
                        if (flag)
                        {
                            children = (child.Value.ToString());
                            flag = false;
                        }
                        else children += ", " + (child.Value.ToString());
                    }
                }
            }
        }

        public void decodeAddResult(Message msg, out bool success)
        {
            XElement doc = XElement.Parse(msg.content);
            if (doc.Value.ToString() == "Action Completed") success = true;
            else success = false;
        }

        public void decodeEditResult(Message msg, out bool success)
        {
            XElement doc = XElement.Parse(msg.content);
            if (doc.Value.ToString() == "Action Completed") success = true;
            else success = false;
        }

        public void decodeRmvRequest(Message msg, out string key)
        {
            XElement doc = XElement.Parse(msg.content);
            key = doc.Element("Key").Value.ToString();
        }

        public void decodeRmvResult(Message msg, out bool success)
        {
            XElement doc = XElement.Parse(msg.content);
            if (doc.Value.ToString() == "Action Completed") success = true;
            else success = false;
        }

        public void decodePersistResult(Message msg, out bool success)
        {
            XElement doc = XElement.Parse(msg.content);
            if (doc.Value.ToString() == "Action Completed") success = true;
            else success = false;
        }

        public void decodeRestoreResult(Message msg, out bool success)
        {
            XElement doc = XElement.Parse(msg.content);
            if (doc.Value.ToString() == "Action Completed") success = true;
            else success = false;
        }

        public void decodeQuery(Message msg, out int type, out string query)
        {
            XElement doc = XElement.Parse(msg.content);
            query = doc.Element("querystring").Value.ToString();
            type = int.Parse(doc.Element("querytype").Value.ToString());
        }

        public void decodeTypeResult(Message msg, out string keytype, out string valuetype)
        {
            XElement doc = XElement.Parse(msg.content);
            keytype = doc.Element("KeyType").Value.ToString();
            valuetype = doc.Element("ValueType").Value.ToString();
        }

    }

#if (TEST_MESSAGEPARSER)
        class Test_MessageParser 
    {

        static void Main(string[] args)
        {
        }
    }
#endif
}