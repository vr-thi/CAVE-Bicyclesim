using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Cave
{
    public class NewNodeConfiguration
    {
        public Vector3 originPosition;
        public Vector3 cameraRoation;
        public Vector3 screenplanePosition;
        public Vector3 screenplaneRotation;
        public Vector3 screenplaneScale;
        public Transform screenPlane;
        public string cameraEye;
        public string ip;
        public int port;
    }

    static class NodeInformation
    {
        private static XmlDocument xmlDocument;

        public static NewNodeConfiguration master;
        public static List<NewNodeConfiguration> slaves;
        public static NewNodeConfiguration own;
        public static bool developmentMode;


        public static void Load()
        {
            slaves = new List<NewNodeConfiguration>();

            string configFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, "node-config.xml");
            string xmlContent = System.IO.File.ReadAllText(configFilePath);

            xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent);

            ReadNodeConfigInformation();
        
        }

        private static void ReadNodeConfigInformation()
        {
            XmlNode nodeConfig = xmlDocument.GetElementsByTagName("config")[0];

            foreach (XmlNode node in nodeConfig.ChildNodes)
            {
                if (node.Name.Equals("slave") || node.Name.Equals("master"))
                {
                    NewNodeConfiguration NewNodeConfiguration = new NewNodeConfiguration();
                    ReadConnectionInfos(node, NewNodeConfiguration);

                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "origin":
                                ReadOrigin(childNode, NewNodeConfiguration);
                                break;

                            case "camera":
                                ReadCamera(childNode, NewNodeConfiguration);
                                break;

                            case "screenplane":
                                ReadScreenplane(childNode, NewNodeConfiguration);
                                break;
                        }
                    }

                    switch (node.Name)
                    {
                        case "master":
                            master = NewNodeConfiguration;
                            break;
                        case "slave":
                            slaves.Add(NewNodeConfiguration);
                            break;
                    }
                }
            }

            if (developmentMode || IsOwnIP(master.ip))
            {
                own = master;

            }
            else
            {
                foreach (NewNodeConfiguration slave in slaves)
                {
                    if (IsOwnIP(slave.ip))
                    {
                        own = slave;
                        //break;
                    }
                }
            }

            if (own == null)
                throw new Exception("Own configuration node could not be found.");
            else
                Debug.Log("Config loaded (" + (master != null ? "1" : "0") + " master, " + slaves.Count + " slaves, own type: " + (IsMaster() ? "master" : "slave") + (developmentMode ? ", Development mode!" : "") + ")");
        }

        private static void ReadConnectionInfos(XmlNode node, NewNodeConfiguration configurationNode)
        {
            configurationNode.ip = node.Attributes["ip"].Value;
            if (node.Attributes["port"] != null)
                configurationNode.port = Convert.ToInt32(node.Attributes["port"].Value);
        }

        private static void ReadScreenplane(XmlNode node, NewNodeConfiguration configurationNode)
        {
            configurationNode.screenplanePosition = ReadVector(node, "position");
            configurationNode.screenplaneRotation = ReadVector(node, "rotation");
            configurationNode.screenplaneScale = ReadVector(node, "scale");
        }

        private static void ReadOrigin(XmlNode node, NewNodeConfiguration configurationNode)
        {
            configurationNode.originPosition = ReadVector(node, "position");
        }

        private static void ReadCamera(XmlNode node, NewNodeConfiguration configurationNode)
        {
            configurationNode.cameraRoation = ReadVector(node, "rotation");
            configurationNode.cameraEye = node.Attributes["eye"].Value;
        }

        private static Vector3 ReadVector(XmlNode node, string childNodeName)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == childNodeName)
                {
                    float x, y, z;
                    Vector3 vec = new Vector3();

                    float.TryParse(childNode.Attributes["x"].Value, out x);
                    float.TryParse(childNode.Attributes["y"].Value, out y);
                    float.TryParse(childNode.Attributes["z"].Value, out z);

                    vec = new Vector3(x, y, z);

                    return vec;
                }
            }
            throw new Exception("No child node with name " + childNodeName + " found.");
        }

        private static bool IsOwnIP(String ip)
        {
            if (ip.Equals("localhost"))
                return true;

            string dnsName = System.Net.Dns.GetHostName();
            if (dnsName == "")
                dnsName = "localhost";
            var host = System.Net.Dns.GetHostEntry(dnsName);
			foreach (var ownIp in host.AddressList)
			{
				if (ownIp.ToString().Equals(ip))
				{
					return true;
				}
			}
			return false;
		}

        public static bool IsMaster()
        {
            return master == own;
        }

        private static void SpawnScreenplane(Transform parent, NewNodeConfiguration configurationNode)
        {
            GameObject screenPlane = new GameObject(configurationNode.ip);
            screenPlane.transform.parent = parent;
            screenPlane.transform.localScale = configurationNode.screenplaneScale;
            screenPlane.transform.eulerAngles = configurationNode.screenplaneRotation;
            screenPlane.transform.localPosition = configurationNode.screenplanePosition;
            configurationNode.screenPlane = screenPlane.transform;
        }

        public static void SpawnScreenplanes(Transform parent)
        {
            SpawnScreenplane(parent, master);
            foreach (NewNodeConfiguration slave in slaves)
            {
                SpawnScreenplane(parent, slave);
            }
        }
    }
}
