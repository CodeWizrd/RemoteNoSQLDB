///////////////////////////////////////////////////////////////
// PersistEngine.cs - Read/Write from/to XML to/from DB      //
//                    Requirement 5 of Project #2            //
// Ver 1.0                                                   //
// Application: DB Reads/Writes for CSE681-SMA, Project#2    //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Dell Inspiron 7520, Core-i7, Windows 10      //
// Author:      Sampath T Janardhan (508899838), SU          //
//              (315) 664-8206, storagar@syr.edu             //
// Source:      Jim Fawcett, CST 4-187, Syracuse University  //
//              (315) 443-3948, jfawcett@twcny.rr.com        //
///////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package persists DB contents to an XML file, and 
 * augments an XML file contents to DB.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   DBElement.cs, DBEngine, UtilityExtensions.cs 
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.0 : 08 Oct 15
 * - first release
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using System.Xml.Linq;
using System.IO;

namespace NoSqlDB

{
    public static class PersistEngine
    {
        //----< Persist Enumerable DB to XML file >----------------------------------------
        public static void persist<Key, Value, Data, T>(this DBEngine<Key, Value> dbStore, string filename) where Data : IEnumerable<T>
        {
            XDocument xDocument = PersistToXML<Key, Value, Data, T>(dbStore);
            xDocument.Save(filename);
        }

        //----< Convert Enumerable DB to XML file format >---------------------------------
        public static XDocument PersistToXML<Key, Value, Data, T>(this DBEngine<Key, Value> dbStore) where Data : IEnumerable<T>
        {
            XDocument xDocument = new XDocument();
            xDocument.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            xDocument.Add(new XComment("Persisting DB to XML at " + DateTime.Now.ToString()));
            XElement root = new XElement("NoSqlDb");
            root.Add(new XElement("KeyType", typeof(Key).ToString()));
            root.Add(new XElement("PayloadType", typeof(Data).ToString()));

            foreach (Key key in dbStore.Keys())
            {
                XElement dbEntry = new XElement("DBEntry");
                root.Add(dbEntry);
                dbEntry.Add(new XElement("Key", key.ToString()));
                Value element;
                dbStore.getValue(key, out element);
                DBElement<Key, Data> dbElement = element as DBElement<Key, Data>;

                XElement xmlElement = new XElement("element");
                xmlElement.Add(new XElement("name", dbElement.name));
                xmlElement.Add(new XElement("descr", dbElement.descr));
                xmlElement.Add(new XElement("timestamp", dbElement.timeStamp));

                XElement children = new XElement("children");
                if (dbElement.children.Count > 0)
                {
                    foreach (Key child in dbElement.children)
                    {
                        children.Add(new XElement("key", child.ToString()));
                    }
                }
                xmlElement.Add(children);

                XElement payload = new XElement("payload");
                Data data = dbElement.payload;
                foreach (T item in data)
                {
                    payload.Add(new XElement("item", item.ToString()));
                }

                xmlElement.Add(payload);
                dbEntry.Add(xmlElement);
            }
            xDocument.Add(root);
            return xDocument;
        }

        //----< Convert Enumerable DB to XML format >--------------------------------------
        public static XDocument ToXML<Key, Value, Data, T>(this DBEngine<Key, Value> dbStore) where Data : IEnumerable<T>
        {
            XDocument xDocument = new XDocument();
            XElement root = new XElement("NoSqlDb");

            foreach (Key key in dbStore.Keys())
            {
                XElement dbEntry = new XElement("DBEntry");
                root.Add(dbEntry);
                dbEntry.Add(new XElement("Key", key.ToString()));
                Value element;
                dbStore.getValue(key, out element);
                DBElement<Key, Data> dbElement = element as DBElement<Key, Data>;

                XElement xmlElement = new XElement("element");
                xmlElement.Add(new XElement("name", dbElement.name));
                xmlElement.Add(new XElement("descr", dbElement.descr));
                xmlElement.Add(new XElement("timestamp", dbElement.timeStamp));

                XElement children = new XElement("children");
                if (dbElement.children.Count > 0)
                {
                    foreach (Key child in dbElement.children)
                    {
                        children.Add(new XElement("key", child.ToString()));
                    }
                }
                xmlElement.Add(children);

                XElement payload = new XElement("payload");
                Data data = dbElement.payload;
                foreach (T item in data)
                {
                    payload.Add(new XElement("item", item.ToString()));
                }

                xmlElement.Add(payload);
                dbEntry.Add(xmlElement);
            }
            xDocument.Add(root);
            return xDocument;
        }

        //----< Persist primitive type DB to XML file >-------------------------------------
        public static void persist<Key, Value, Data>(this DBEngine<Key, Value> dbStore, string filename)
        {
            XDocument xDocument = PersistToXML<Key, Value, Data>(dbStore);
            xDocument.Save(filename);
            dbStore.clear();
        }

        //----< Convert primitive DB to XML file format >----------------------------------
        public static XDocument PersistToXML<Key, Value, Data>(this DBEngine<Key, Value> dbStore)
        {
            XDocument xDocument = new XDocument();
            xDocument.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            xDocument.Add(new XComment("Persisting DB to XML at " + DateTime.Now.ToString()));
            XElement root = new XElement("NoSqlDb");
            root.Add(new XElement("KeyType", typeof(Key).ToString()));
            root.Add(new XElement("PayloadType", typeof(Data).ToString()));

            foreach (Key key in dbStore.Keys())
            {
                XElement dbEntry = new XElement("DBEntry");
                root.Add(dbEntry);
                dbEntry.Add(new XElement("Key", key.ToString()));
                Value element;
                dbStore.getValue(key, out element);
                DBElement<Key, Data> dbElement = element as DBElement<Key, Data>;

                XElement xmlElement = new XElement("element");
                xmlElement.Add(new XElement("name", dbElement.name));
                xmlElement.Add(new XElement("descr", dbElement.descr));
                xmlElement.Add(new XElement("timestamp", dbElement.timeStamp));

                XElement children = new XElement("children");
                if (dbElement.children.Count > 0)
                {
                    foreach (Key child in dbElement.children)
                    {
                        children.Add(new XElement("key", child.ToString()));
                    }
                }
                xmlElement.Add(children);
                XElement payload = new XElement("payload");
                payload.Add(new XElement("item", dbElement.payload.ToString()));
                xmlElement.Add(payload);
                dbEntry.Add(xmlElement);
            }
            xDocument.Add(root);
            return xDocument;
        }

        //----< Convert primitive DB to XML format >---------------------------------------
        public static XDocument ToXML<Key, Value, Data>(this DBEngine<Key, Value> dbStore)
        {
            XDocument xDocument = new XDocument();
            XElement root = new XElement("NoSqlDb");

            foreach (Key key in dbStore.Keys())
            {
                XElement dbEntry = new XElement("DBEntry");
                root.Add(dbEntry);
                dbEntry.Add(new XElement("Key", key.ToString()));
                Value element;
                dbStore.getValue(key, out element);
                DBElement<Key, Data> dbElement = element as DBElement<Key, Data>;

                XElement xmlElement = new XElement("element");
                xmlElement.Add(new XElement("name", dbElement.name));
                xmlElement.Add(new XElement("descr", dbElement.descr));
                xmlElement.Add(new XElement("timestamp", dbElement.timeStamp));

                XElement children = new XElement("children");
                if (dbElement.children.Count > 0)
                {
                    foreach (Key child in dbElement.children)
                    {
                        children.Add(new XElement("key", child.ToString()));
                    }
                }
                xmlElement.Add(children);
                XElement payload = new XElement("payload");
                payload.Add(new XElement("item", dbElement.payload.ToString()));
                xmlElement.Add(payload);
                dbEntry.Add(xmlElement);
            }
            xDocument.Add(root);
            return xDocument;
        }

        //----< Augment Enumerable type DB to XML file >------------------------------------
        public static void augment<Key, Value, Data, T>(this DBEngine<Key, Value> db, string filename) where Data : IEnumerable<T>
        {
            if (!File.Exists(filename)) {  "File does not exist!".error(); return;  }
            XDocument xDocument = null;
            xDocument = XDocument.Load(filename);
            PersistEngine.FromXML<Key, Value, Data, T>(db, xDocument);
        }

        //----< Parse Enumerable type DB from XML format >----------------------------------
        public static void FromXML<Key, Value, Data, T>(this DBEngine<Key, Value> db, XDocument xDocument) where Data : IEnumerable<T>
        {
            XElement noSqlDB = xDocument.Root;
            string keytype = noSqlDB.Element("KeyType").Value; string payloadtype = noSqlDB.Element("PayloadType").Value;
            if (!(typeof(Key).ToString() == keytype && typeof(Data).ToString() == payloadtype))
            {
                WriteLine(); "The persisted DB does not match the current DB!!".error(); WriteLine();
                WriteLine("  Current DB: \n    Key Type: {0} \n    Payload Type: {1}", typeof(Key), typeof(Data));
                WriteLine("  DB to be read: \n    Key Type: {0} \n    Payload Type: {1}", keytype, payloadtype);
            } else
            {
                IEnumerable<XElement> dbEntries = xDocument.Root.Elements("DBEntry");
                foreach (XElement dbEntry in dbEntries)
                {
                    string temp = dbEntry.Element("Key").Value;
                    Key key = (Key)Convert.ChangeType(temp, typeof(Key));
                    DBElement<Key, Data> element = new DBElement<Key, Data>();
                    XElement elem = dbEntry.Element("element");
                    element.name = elem.Element("name").Value;
                    element.descr = elem.Element("descr").Value;
                    string time = elem.Element("timestamp").Value;
                    element.timeStamp = (DateTime)Convert.ChangeType(time, typeof(DateTime));
                    if (elem.Element("children") != null)
                    {
                        IEnumerable<XElement> childKeys = elem.Element("children").Elements("key");
                        if (childKeys.Count() > 0)
                        {
                            foreach (XElement child in childKeys)
                            {
                                temp = child.Element("key").Value;
                                Key childKey = (Key)Convert.ChangeType(temp, typeof(Key));
                                element.children.Add(childKey);
                            }
                        }
                    }
                    IEnumerable<XElement> items = elem.Element("payload").Elements("item");
                    List<T> payload = new List<T>();
                    foreach (XElement item in items)
                    {
                        temp = item.Value;
                        T payloadItem = (T)Convert.ChangeType(temp, typeof(T));
                        payload.Add(payloadItem);
                    }
                    Data payloadData = (Data)Convert.ChangeType(payload, typeof(Data));
                    element.payload = payloadData;
                    Value value = (Value)Convert.ChangeType(element, typeof(Value));
                    db.insert(key, value);
                }
            }
        }

        //----< Augment primitive type DB to XML file >-------------------------------------
        public static void augment<Key, Value, Data>(this DBEngine<Key, Value> db, string filename)
        {
            if (!File.Exists(filename)) {  "File does not exist!".error();  return;  }
            XDocument xDocument = null;
            xDocument = XDocument.Load(filename);
            PersistEngine.FromXML<Key, Value, Data>(db, xDocument);
        }

        //----< Parse primitive type DB from XML format >-----------------------------------
        public static void FromXML<Key, Value, Data>(this DBEngine<Key, Value> db, XDocument xDocument)
        {
            XElement noSqlDB = xDocument.Root;
            string keytype = noSqlDB.Element("KeyType").Value;
            string payloadtype = noSqlDB.Element("PayloadType").Value;

            if (!(typeof(Key).ToString() == keytype && typeof(Data).ToString() == payloadtype))
            {
                WriteLine(); "The persisted DB does not match the current DB!!".error();
                WriteLine();
                WriteLine("  Current DB: \n    Key Type: {0} \n    Payload Type: {1}", typeof(Key), typeof(Data));
                WriteLine("  DB to be read: \n    Key Type: {0} \n    Payload Type: {1}", keytype, payloadtype);
            }
            else
            {
                IEnumerable<XElement> dbEntries = xDocument.Root.Elements("DBEntry");
                foreach (XElement dbEntry in dbEntries)
                {
                    string temp = dbEntry.Element("Key").Value;
                    Key key = (Key)Convert.ChangeType(temp, typeof(Key));
                    DBElement<Key, Data> element = new DBElement<Key, Data>();
                    XElement elem = dbEntry.Element("element");
                    element.name = elem.Element("name").Value;
                    element.descr = elem.Element("descr").Value;
                    string time = elem.Element("timestamp").Value;
                    element.timeStamp = (DateTime)Convert.ChangeType(time, typeof(DateTime));

                    if (elem.Element("children") != null)
                    {
                        IEnumerable<XElement> childKeys = elem.Element("children").Elements("key");
                        if (childKeys.Count() > 0)
                        {
                            foreach (XElement child in childKeys)
                            {
                                temp = child.Value;
                                Key childKey = (Key)Convert.ChangeType(temp, typeof(Key));
                                element.children.Add(childKey);
                            }
                        }
                    }
                    XElement payloadElem = elem.Element("payload");
                    temp = payloadElem.Element("item").Value;
                    Data payload = (Data)Convert.ChangeType(temp, typeof(Data));
                    element.payload = payload;

                    Value value = (Value)Convert.ChangeType(element, typeof(Value));
                    db.insert(key, value);
                }
            }
        }
    }

#if (PersistEngineTest) 

    public class PersistEngineTest
    {
        private DBEngine<int, DBElement<int, string>> db = new DBEngine<int, DBElement<int, string>>();
        static void Main(string[] args)
        {
            PersistEngineTest test = new PersistEngineTest();

            "Creating & Populating DB".title2();
            test.TestCreate();
            test.TestAdd(1);
            test.TestAdd(2);
            test.TestAdd(4);
            test.TestAdd(3);
            test.TestAdd(5);
            test.TestAdd(15);
            test.TestRemove(6);
            test.TestRemove(4);
            WriteLine();


            "Demonstrating DB Persist".title2();
            test.TestPersist();
            WriteLine();

            "Demonstrating DB Augment".title2();
            test.TestAugment();

        }

        //----< Demonstrate DB Persists >------------------------------------
        void TestPersist()
        {
            "Persisting DB contents to persistFile.xml".body();
            PersistEngine.persist<int, DBElement<int, string>, string>(db, "persistFile.xml");
            "Clearing DB contents after persist".body();

        }

        //----< Demonstrate DB Augments >------------------------------------
        void TestAugment()
        {
            "Augmenting DB contents from persistFileT.xml".body();
            PersistEngine.augment<int, DBElement<int, string>, string>(db, "persistFileT.xml");
            WriteLine();


            "Augmenting DB contents from persistFile.xml".body();
            PersistEngine.augment<int, DBElement<int, string>, string>(db, "persistFile.xml");
            WriteLine();
        }

        //----< Create & Populate DB >--------------------------------------
        void TestCreate()
        {
            "Demonstrating Requirement #2 (DB <int, string>)".title();
            DBElement<int, string> elem = new DBElement<int, string>();
            elem.name = "element";
            elem.descr = "test element";
            elem.timeStamp = DateTime.Now;
            elem.children.AddRange(new List<int> { 2, 3 });
            elem.payload = "elem's payload";
            "Adding below element with Key = 1".title2();
            db.insert(1, elem);
            WriteLine();
        }

        //----< Add contents to DB >-----------------------------------------
        void TestAdd(int key)
        {
            string str = "Adding element with key " + key.ToString();
            str.title2();
            DBElement<int, string> elem = new DBElement<int, string>();
            elem.name = "Element" + key.ToString();
            elem.descr = "Description of " + key.ToString();
            elem.timeStamp = DateTime.Now;
            elem.children.AddRange(new List<int> { key - 1, key + 1, key + 2 });
            elem.payload = "Element " + key.ToString() + "'s payload";
            bool success = db.insert(key, elem);
            if (success)
            {
                "Element added successfully".success();
            }
            else "Key already exists".error();
            WriteLine();
        }

        //----< Remove contents from DB >------------------------------------
        void TestRemove(int key)
        {
            string str = "Removing element with key " + key.ToString();
            str.title2();
            bool success = db.remove(key);
            if (success)
            {
                "Element removed successfully".success();
            }
            else "Key does not exist".error();
            WriteLine();
        }

    }
#endif

}
