///////////////////////////////////////////////////////////////
// DBElement.cs - Define element for noSQL database          //
// Ver 1.1                                                   //
// Application: Demonstration for CSE687-OOD, Project#2      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Dell XPS2700, Core-i7, Windows 10            //
// Author:      Jim Fawcett, CST 4-187, Syracuse University  //
//              (315) 443-3948, jfawcett@twcny.rr.com        //
///////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package implements the DBElement<Key, Data> type, used by 
 * DBEngine<key, Value> where Value is DBElement<Key, Data>.
 *
 * The DBElement<Key, Data> state consists of metadata and an
 * instance of the Data type.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: DBElement.cs, UtilityExtensions.cs
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.1 : 24 Sep 15
 * - removed extension methods, removed tests from test stub
 * - Testing now  uses DBEngineTest.cs
 * ver 1.0 : 13 Sep 15
 * - first release
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Console;

namespace NoSqlDB
{
  /////////////////////////////////////////////////////////////////////
  // DBElement<Key, Data> class
  // - Instances of this class are the "values" in our key/value 
  //   noSQL database.
  // - Key and Data are unspecified classes, to be supplied by the
  //   application that uses the noSQL database.
  //   See the teststub below for examples of use.

    public class DBElement<Key, Data>
    {
        public string name { get; set; }          // metadata    |
        public string descr { get; set; }         // metadata    |
        public DateTime timeStamp { get; set; }   // metadata   value
        public List<Key> children { get; set; }   // metadata    |
        public Data payload { get; set; }         // data        |

        //public constructor
        public DBElement(string Name = "unnamed", string Descr = "undescribed")
        {
            name = Name;
            descr = Descr;
            timeStamp = DateTime.Now;
            children = new List<Key>();
        }

        public string ToXml()
        {
            string xml = "";

            XElement xmlElement = new XElement("element");
            xmlElement.Add(new XElement("name", name));
            xmlElement.Add(new XElement("descr", descr));
            xmlElement.Add(new XElement("timestamp", timeStamp));

            XElement childrenX = new XElement("children");
            if (children.Count > 0)
            {
                foreach (Key child in children)
                {
                    childrenX.Add(new XElement("key", child.ToString()));
                }
            }
            xmlElement.Add(childrenX);

            XElement payloadX = new XElement("payload", payload.ToString());
            xmlElement.Add(payloadX);

            xml = xmlElement.ToString();
            xml = xml.Remove(0, 9); // remove <element>
            xml = xml.Remove(xml.Length - 10, 10); // remove </element>
            return xml;
        }

        public string ToXml<Data1, T>() where Data1 : IEnumerable<T>, Data
        {
            string xml = "";

            XElement xmlElement = new XElement("element");
            xmlElement.Add(new XElement("name", name));
            xmlElement.Add(new XElement("descr", descr));
            xmlElement.Add(new XElement("timestamp", timeStamp));

            XElement childrenX = new XElement("children");
            if (children.Count > 0)
            {
                foreach (Key child in children)
                {
                    childrenX.Add(new XElement("key", child.ToString()));
                }
            }
            xmlElement.Add(childrenX);

            XElement payloadX = new XElement("payload");
            Data1 payload1 = (Data1)payload;
            foreach (T item in payload1)
            {
                payloadX.Add(new XElement("item", payload1.ToString()));
            }
            xmlElement.Add(payloadX);

            xml = xmlElement.ToString();
            return xml;
        }

        public string PayloadToString()
        {
            string pload = (string)Convert.ChangeType(payload, typeof(string));
            return pload;//.ToString();
        }

        public string PayloadToString<Data1, T>() where Data1 : IEnumerable<T>, Data
        {
            Data1 enumPayload = (Data1)payload;
            string content = enumPayload.ElementAt(0).ToString();

            if (enumPayload.Count() > 1)
            {
                for (int i = 1; i < enumPayload.Count(); i++)
                {
                    content += ", " + enumPayload.ElementAt(i).ToString();
                }
            }
            return content;
        }


    }

#if (TEST_DBELEMENT)


  class TestDBElement
  {
    static void Main(string[] args)
    {
      "Testing DBElement Package".title('=');
      WriteLine();

      Write("\n  All testing of DBElement class moved to DBElementTest package.");
      Write("\n  This allow use of DBExtensions package without circular dependencies.");

      Write("\n\n");
    }
  }
#endif
}
