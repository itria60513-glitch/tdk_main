using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Management;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Threading;
using EFEM.DataCenter;
using LogUtility;

namespace EFEM.FileUtilities
{
    public class FileUtility : AbstractFileUtilities
    {
        private string WorkPath = ConstVC.FilePath.ConfigFolder;
        private XmlDocument xdOInfo, xdSerial, xdTcpip, xdSysConfig;
        private static AbstractFileUtilities _instance = null;

        private Dictionary<string, TCPConfig> _tCPConfig = new Dictionary<string, TCPConfig>();
        private Dictionary<string, RS232Config> _rs232Config = new Dictionary<string, RS232Config>();
        List<(string, int)> _commList = new List<(string, int)>();
        LogUtilityClient log = null;
        private bool disposed = false;

        public static AbstractFileUtilities GetUniqueInstance()
        {
            if (_instance == null)
                _instance = new FileUtility();

            return _instance;
        }

        private FileUtility()
        {
            string error = "";
            XmlTextReader xtr;
            log = new LogUtilityClient();

            try
            {
                //First check whether EFEMConfig.xml is damaged 
                string efemConfigName = WorkPath + ConstVC.FilePath.EFEMConfig;
                string efemConfigNewName = WorkPath + ConstVC.FilePath.EFEMConfig + ".new";
                string efemConfigOldName = WorkPath + ConstVC.FilePath.EFEMConfig + ".old";

                if (File.Exists(efemConfigName))
                {
                    if (File.Exists(efemConfigNewName))
                    {
                        //EFEMConfig.xml.new is damaged step 1
                        File.Delete(efemConfigNewName);
                    }
                    else if (File.Exists(efemConfigOldName))
                    {
                        //Error deleting file step 4.
                        File.Delete(efemConfigOldName);
                    }
                }
                else if (File.Exists(efemConfigNewName) && File.Exists(efemConfigOldName))
                {
                    //Error renaming file step 2-3.
                    File.Move(efemConfigOldName, efemConfigName);
                    File.Delete(efemConfigNewName);
                }
                else throw new Exception(string.Format("Cannot recovery {0}.", ConstVC.FilePath.EFEMConfig));

                try
                {
                    bool folderExists = Directory.Exists(ConstVC.FilePath.AutoBackupFolder);
                    if (!folderExists)
                        Directory.CreateDirectory(ConstVC.FilePath.AutoBackupFolder);
                }
                catch
                {

                }

                //Double Check
                xtr = new XmlTextReader(efemConfigName);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                xdSysConfig = new XmlDocument();
                xdSysConfig.Load(xtr);
                xtr.Close();
                if (xdSysConfig.DocumentElement == null)
                    throw new Exception("Missing Root Element.");

                //Load EFEMConfig.xml
                if (File.Exists(WorkPath + ConstVC.FilePath.EFEMConfigAutoBackup))
                {
                    DateTime backupTime = File.GetLastWriteTime(WorkPath + ConstVC.FilePath.EFEMConfigAutoBackup);
                    TimeSpan period = DateTime.Now - backupTime;
                    if (period.Days > 7)
                    {
                        string file1 = WorkPath + ConstVC.FilePath.EFEMConfigAutoBackup;
                        try
                        {
                            File.Copy(WorkPath + ConstVC.FilePath.EFEMConfig, file1, true);
                            File.SetLastWriteTime(file1, DateTime.Now);
                        }
                        catch (Exception ex) { MessageBox.Show("Cannot copy " + file1); }
                    }

                    //backup every time when EFEM GUI restart
                    try
                    {
                        string file2 = ConstVC.FilePath.AutoBackupFolder + ConstVC.FilePath.EFEMConfigAutoBackup + "." + DateTime.Now.Ticks.ToString();
                        //File.SetAttributes(file2, FileAttributes.Normal);
                        File.Copy(WorkPath + ConstVC.FilePath.EFEMConfig, file2, true);
                        File.SetLastWriteTime(file2, DateTime.Now);
                    }
                    catch (Exception ex) 
                    { 
                        MessageBox.Show("Fail to auto-backup " + ConstVC.FilePath.EFEMConfig + " Reason: " + ex.Message); 
                    }
                }
                else
                {
                    try
                    {
                        string file3 = WorkPath + ConstVC.FilePath.EFEMConfigAutoBackup;

                        File.Copy(WorkPath + ConstVC.FilePath.EFEMConfig, file3, true);
                        File.SetLastWriteTime(WorkPath + ConstVC.FilePath.EFEMConfigAutoBackup, DateTime.Now);
                    }
                    catch (Exception ex) { }
                    try
                    {
                        string file4 = ConstVC.FilePath.EFEMConfigAutoBackup + "." + DateTime.Now.Ticks.ToString();
                        File.Copy(WorkPath + ConstVC.FilePath.EFEMConfig, file4, true);
                        File.SetLastWriteTime(file4, DateTime.Now);
                    }
                    catch (Exception ex) { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                error += string.Format(string.Format("There is an error while loading {0}. System cannot recovery EFEMConfig.xml. Please check the files.\r\n" + ex.Message + ex.StackTrace, ConstVC.FilePath.EFEMConfig));
            }

            try
            {
                // Load TCPIP.xml
                xtr = new XmlTextReader(WorkPath + ConstVC.FilePath.TCPIP);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                xdTcpip = new XmlDocument();
                xdTcpip.Load(xtr);
                xtr.Close();
            }
            catch (Exception ex)
            {
                error += string.Format("There is an error while loading TCPIP.xml. Please check the file. \r\n" + ex.Message + ex.StackTrace);
                throw new ApplicationException(error);
            }
            try
            {
                // Load RS232.xml
                xtr = new XmlTextReader(WorkPath + ConstVC.FilePath.RS232);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                xdSerial = new XmlDocument();
                xdSerial.Load(xtr);
                xtr.Close();
            }
            catch (Exception ex)
            {
                error += string.Format("There is an error while loading RS232.xml. Please check the file. \r\n" + ex.Message + ex.StackTrace);
                throw new ApplicationException(error);
            }

            try
            {
                xtr = new XmlTextReader(WorkPath + ConstVC.FilePath.EFEMObjectInfo);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                xdOInfo = new XmlDocument();
                xdOInfo.Load(xtr);
                xtr.Close();
            }
            catch (Exception ex)
            {
                error += string.Format("There is an error while loading ObjectInfo.xml. Please check the file. \r\n" + ex.Message + ex.StackTrace);
                throw new ApplicationException(error);
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
            }
            disposed = true;
        }

        ~FileUtility()
        {
            Dispose(false);
        }

        #region Inherited Functions
        public ArrayList InstantiateObjects()
        {
            ArrayList al = new ArrayList();
            FileInfo info = new FileInfo(WorkPath + ConstVC.FilePath.EFEMConfig);
            if (info.IsReadOnly)
                al.Add(ConstVC.FilePath.EFEMConfig + " is read-only.");
            info = new FileInfo(WorkPath + "TCPIP.xml");
            if (info.IsReadOnly)
                al.Add("TCPIP.xml is read-only.");
            info = new FileInfo(WorkPath + "RS232.xml");
            if (info.IsReadOnly)
                al.Add("RS232.xml is read-only.");
            if (al.Count > 0)
                return al;
            else
                return null;
        }
        public ArrayList EstablishCommunications() { return null; }
        public ArrayList DownloadParameters() { return null; }
        public ArrayList Initialize() { return null; }
        #endregion

        #region System Config
        private object _sysconfigLocker = new object();
        int iBufferingCount = 0;
        private bool stopWriting = false;

        public string GetPrivateProfileString(string section, string key)
        {
            Monitor.Enter(_sysconfigLocker);
            try
            {
                XmlDocument xd = xdSysConfig;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        if (xnodSection.Name.ToLower() == section.ToLower())
                            break;
                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                    {
                        return null;
                    }

                    xnodSection = xnodSection.FirstChild;
                    while (xnodSection != null)
                    {
                        if (xnodSection.NodeType == XmlNodeType.Comment)
                        {
                            xnodSection = xnodSection.NextSibling;
                            continue;
                        }

                        XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                        if (mapAttributes == null || mapAttributes.Count != 2)
                        {
                            return null;
                        }

                        Hashtable ht = new Hashtable();
                        for (int i = 0; i < 2; i++)
                        {
                            XmlNode xnodAtt = mapAttributes.Item(i);
                            ht.Add(xnodAtt.Name.ToLower(), xnodAtt.Value);
                        }
                        if (!(ht.ContainsKey("key") && ht.ContainsKey("value")))
                        {
                            return null;
                        }
                        if (ht["key"].ToString().ToLower() == key.ToLower())
                            return ht["value"].ToString();

                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                    {

                    }
                    return null;
                }
                else
                {

                }
                return null;
            }
            finally
            {
                Monitor.Exit(_sysconfigLocker);
            }
        }
        public void WritePrivateProfileString(string section, string key, string val)
        {
            if (stopWriting) return;
            Monitor.Enter(_sysconfigLocker);
            try
            {
                XmlDocument xd = xdSysConfig;
                XmlNode root = xd.DocumentElement;
                XmlNode xnodSection;
                XmlNode xnodEntry;
                if (root.HasChildNodes)
                {
                    xnodSection = root.FirstChild;
                    while (xnodSection != null)
                    {
                        if (xnodSection.Name.ToLower() == section.ToLower())
                            break;
                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                    {
                        string str = string.Format("Unable to find the section [{0}] in EFEMConfig.xml.", section);
                        throw new ApplicationException(str);
                    }

                    xnodEntry = xnodSection.FirstChild;
                    while (xnodEntry != null)
                    {
                        //To prevent exception
                        if (xnodEntry.NodeType == XmlNodeType.Comment)
                        {
                            xnodEntry = xnodEntry.NextSibling;
                            continue;
                        }

                        XmlAttributeCollection attributes = xnodEntry.Attributes;
                        if (attributes == null || attributes.Count != 2)
                        {
                            string str = string.Format("Wrong fromat of EFEMConfig.xml. Attributes collection is empty or more than 2.");
                            throw new ApplicationException(str);
                        }

                        Hashtable ht = new Hashtable();
                        for (int i = 0; i < 2; i++)
                        {
                            XmlNode xnodAtt = attributes.Item(i);
                            ht.Add(xnodAtt.Name.ToLower(), xnodAtt.Value);
                        }
                        if (!ht.ContainsKey("key"))
                        {
                            string str = string.Format("Wrong fromat of EFEMConfig.xml. Wrong attributes collection. Cannot find attribute->key.");
                            throw new ApplicationException(str);
                        }
                        if (!ht.ContainsKey("value"))
                        {
                            string str = string.Format("Wrong fromat of EFEMConfig.xml. Wrong attributes collection. Cannot find attribute->value.");
                            throw new ApplicationException(str);
                        }

                        if (ht["key"].ToString().ToLower() == key.ToLower())
                        {
                            ht["value"] = val;
                            XmlAttribute attValue = xd.CreateAttribute("value");
                            attValue.Value = ht["value"].ToString();
                            attributes.SetNamedItem(attValue);
                            try
                            {
                                iBufferingCount++;
                                if (iBufferingCount >= 20)
                                {
                                    iBufferingCount = 0;
                                    XmlTextWriter xtw = new XmlTextWriter(WorkPath + ConstVC.FilePath.EFEMConfig + ".new", System.Text.Encoding.UTF8);
                                    xtw.Formatting = Formatting.Indented;
                                    xd.WriteTo(xtw);
                                    xtw.Flush();
                                    xtw.Close();

                                    File.Move(WorkPath + ConstVC.FilePath.EFEMConfig, WorkPath + ConstVC.FilePath.EFEMConfig + ".old");
                                    File.Move(WorkPath + ConstVC.FilePath.EFEMConfig + ".new", WorkPath + ConstVC.FilePath.EFEMConfig);
                                    File.Delete(WorkPath + ConstVC.FilePath.EFEMConfig + ".old");
                                }
                                return;
                            }
                            catch (Exception ee)
                            {
                                string msg = ee.Message;
                                return;
                            }
                        }

                        xnodEntry = xnodEntry.NextSibling;
                    }
                    if (xnodEntry == null)
                    {
                        iBufferingCount++;
                        //insert new element
                        XmlElement elem = xd.CreateElement(section + "Entry");
                        elem.SetAttribute("key", key);
                        elem.SetAttribute("value", val);
                        xnodSection.AppendChild(elem);
                        try
                        {
                            XmlTextWriter xtw = new XmlTextWriter(WorkPath + ConstVC.FilePath.EFEMConfig + ".new", System.Text.Encoding.UTF8);
                            xtw.Formatting = Formatting.Indented;
                            xd.WriteTo(xtw);
                            xtw.Flush();
                            xtw.Close();

                            File.Move(WorkPath + ConstVC.FilePath.EFEMConfig, WorkPath + ConstVC.FilePath.EFEMConfig + ".old");
                            File.Move(WorkPath + ConstVC.FilePath.EFEMConfig + ".new", WorkPath + ConstVC.FilePath.EFEMConfig);
                            File.Delete(WorkPath + ConstVC.FilePath.EFEMConfig + ".old");
                        }
                        catch (Exception ee)
                        {
                            string msg = ee.Message;
                            return;
                        }
                    }
                    return;
                }
                else
                {
                    string str = string.Format("Unable to find the section [{0}] in EFEMConfig.xml.", section);
                    throw new ApplicationException(str);
                }
            }
            finally
            {
                Monitor.Exit(_sysconfigLocker);
            }
        }
        public Hashtable GetEFEMConfigDictionary(string section)
        {
            Monitor.Enter(_sysconfigLocker);
            try
            {
                XmlDocument xd = xdSysConfig;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                Hashtable htable = new Hashtable();
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        if (xnodSection.Name.ToLower() == section.ToLower())
                            break;
                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                    {
                        string str = string.Format("Unable to find the section [{0}] in EFEMConfig.xml.", section);
                        throw new ApplicationException(str);
                    }

                    xnodSection = xnodSection.FirstChild;
                    while (xnodSection != null)
                    {
                        //To prevent exception
                        if (xnodSection.NodeType == XmlNodeType.Comment)
                        {
                            xnodSection = xnodSection.NextSibling;
                            continue;
                        }

                        XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                        if (mapAttributes == null || mapAttributes.Count != 2)
                        {
                            string str = string.Format("Wrong fromat of EFEMConfig.xml. Attributes collection is empty or more than 2.");
                            throw new ApplicationException(str);
                        }
                        Hashtable ht = new Hashtable();
                        for (int i = 0; i < 2; i++)
                        {
                            XmlNode xnodAtt = mapAttributes.Item(i);
                            ht.Add(xnodAtt.Name.ToLower(), xnodAtt.Value);
                        }
                        if (!ht.ContainsKey("key"))
                        {
                            string str = string.Format("Wrong fromat of EFEMConfig.xml. Wrong attributes collection. Cannot find attribute->key.");
                            throw new ApplicationException(str);
                        }
                        if (!ht.ContainsKey("value"))
                        {
                            string str = string.Format("Wrong fromat of EFEMConfig.xml. Wrong attributes collection. Cannot find attribute->value.");
                            throw new ApplicationException(str);
                        }
                        htable[ht["key"].ToString()] = ht["value"].ToString();

                        xnodSection = xnodSection.NextSibling;
                    }
                    return htable;
                }
                else
                {
                    string str = string.Format("Unable to find the section [{0}] in EFEMConfig.xml.", section);
                    throw new ApplicationException(str);
                }
            }
            finally
            {
                Monitor.Exit(_sysconfigLocker);
            }
        }
        public void FlushEFEMConfig()
        {
            if (stopWriting) return;
            Monitor.Enter(_sysconfigLocker);
            try
            {
                if (iBufferingCount > 0)
                {
                    iBufferingCount = 0;

                    /////////////safe write file method
                    XmlTextWriter xtw = new XmlTextWriter(WorkPath + ConstVC.FilePath.EFEMConfig + ".new", System.Text.Encoding.UTF8);
                    xtw.Formatting = Formatting.Indented;
                    xdSysConfig.WriteTo(xtw);
                    xtw.Flush();
                    xtw.Close();

                    File.Move(WorkPath + ConstVC.FilePath.EFEMConfig, WorkPath + ConstVC.FilePath.EFEMConfig + ".old");
                    File.Move(WorkPath + ConstVC.FilePath.EFEMConfig + ".new", WorkPath + ConstVC.FilePath.EFEMConfig);
                    File.Delete(WorkPath + ConstVC.FilePath.EFEMConfig + ".old");
                }
            }
            finally
            {
                Monitor.Exit(_sysconfigLocker);
            }
        }
        public void CloseAllFile()
        {
            stopWriting = true;
            System.Threading.Thread.Sleep(500);
        }
        #endregion

        #region Communication

        public List<(string,int)> CommunicationLoad()
        {
            string str = string.Empty;
            XmlDocument xdCom = XMLFileLoad(WorkPath + "Communication.xml");
            XmlNode commTypeNode = xdCom.SelectSingleNode("Communications");
            if (commTypeNode != null)
            {
                foreach (XmlNode child in commTypeNode.ChildNodes)
                {
                    if (child.Name.Equals("Communication"))
                    {
                        var element = (XmlElement)child;
                        string key = element.GetAttribute("key");
                        int commIndex = int.Parse(element.GetAttribute("CommunicationIndex"));
                        if (commIndex == 1)
                        {
                            string ip = element.GetAttribute("ip");
                            string port = element.GetAttribute("port");
                            TCPConfig comm = new TCPConfig(ip, port);
                            _tCPConfig[key] = comm;
                            _commList.Add((key, commIndex));
                        }
                        else if (commIndex == 0)
                        {
                            int port = int.Parse(element.GetAttribute("port"));
                            int baud = int.Parse(element.GetAttribute("baud"));
                            int parity = int.Parse(element.GetAttribute("parity"));
                            int databits = int.Parse(element.GetAttribute("databits"));
                            int stopbits = int.Parse(element.GetAttribute("stopbits"));
                            RS232Config comm = new RS232Config(port, baud, parity, databits, stopbits);
                            _rs232Config[key] = comm;
                            _commList.Add((key, commIndex));
                        }
                        else
                        {
                            str = key + " : Wrong Value of Communication Setting.";
                            log.WriteLog("TDK", str);
                            throw new ApplicationException(str);
                        }
                    }
                    else
                    {
                        str = "Wrong Format of Communication config in Communication.xml.";
                        log.WriteLog("TDK", str);
                        throw new ApplicationException(str);
                    }
                }
            }
            else
            {
                str = "No Communication config in Communication.xml.";
                log.WriteLog("TDK", str);
                throw new ApplicationException(str);
            }

            str = "Config Load Success.";
            log.WriteLog("TDK", str);
            return _commList;
        }

        public List<(string,int)> GetCommList()
        {
            return _commList;
        }

        #endregion

        #region Tcpip Port Config

        public TCPConfig GetTCPSetting(string key)
        {
            if (_tCPConfig.ContainsKey(key))
            {
                return _tCPConfig[key];
            }
            else
            {
                return new TCPConfig(string.Empty, string.Empty);
            }
        }

        public void TCPConfigSave(string key, string ipAddress, string port)
        {
            XmlDocument xdCom = XMLFileLoad(WorkPath + "Communication.xml");
            XmlNode commNode = xdCom.SelectSingleNode("Communications");
            if (commNode != null)
            {
                string str = string.Empty;
                XmlElement target = null;
                foreach (XmlNode child in commNode.ChildNodes)
                {
                    if (child.NodeType != XmlNodeType.Element) continue;

                    var element = (XmlElement)child;
                    if (element.GetAttribute("key") == key)
                    {
                        target = element;
                        target.SetAttribute("CommunicationIndex", "0");
                        target.SetAttribute("ip", ipAddress);
                        target.SetAttribute("port", port);
                        break;
                    }
                }

                XmlTextWriter xtwNew = new XmlTextWriter(WorkPath + "Communication.xml", System.Text.Encoding.UTF8);
                xtwNew.Formatting = Formatting.Indented;
                xdCom.WriteTo(xtwNew);
                xtwNew.Flush();
                xtwNew.Close();

                TCPConfigApply(key, ipAddress, port);

                str = "Config Save Success.";
                log.WriteLog("TDK", str);
            }
            else
            {
                string str = "No Communication config in Communication.xml.";
                log.WriteLog("TDK", str);
                throw new ApplicationException(str);
            }
        }

        public void TCPConfigApply(string key, string ipAddress, string port)
        {
            _tCPConfig[key].Ip = ipAddress;
            _tCPConfig[key].Port = port;
        }
        #endregion

        #region Serial Port Config

        public RS232Config GetSerialSetting(string key)
        {
            if (_rs232Config.ContainsKey(key))
            {
                return _rs232Config[key];
            }
            else
            {
                return new RS232Config(-1, -1, -1, -1, -1);
            }
        }

        public void SerialPortConfigSave(string key, int port, int baud, int parity, int databits, int stopbits)
        {
            string str = string.Empty;
            XmlDocument xdCom = XMLFileLoad(WorkPath + "Communication.xml");
            XmlNode rs232Node = xdCom.SelectSingleNode("Communications");
            if (rs232Node != null)
            {
                XmlElement target = null;
                foreach (XmlNode child in rs232Node.ChildNodes)
                {
                    if (child.NodeType != XmlNodeType.Element) continue;

                    var element = (XmlElement)child;
                    if (element.GetAttribute("key") == key)
                    {
                        target = element;
                        target.SetAttribute("CommunicationIndex", "1");
                        target.SetAttribute("port", port.ToString());
                        target.SetAttribute("baud", baud.ToString());
                        target.SetAttribute("parity", parity.ToString());
                        target.SetAttribute("databits", databits.ToString());
                        target.SetAttribute("stopbits", stopbits.ToString());
                        break;
                    }
                }


                XmlTextWriter xtwNew = new XmlTextWriter(WorkPath + "RS.xml", System.Text.Encoding.UTF8);
                xtwNew.Formatting = Formatting.Indented;
                xdCom.WriteTo(xtwNew);
                xtwNew.Flush();
                xtwNew.Close();

                SerialConfigApply(key, port, baud, parity, databits, stopbits);

                str = "Config Save Success.";
                log.WriteLog("TDK", str);
            }
            else
            {
                str = "No Serial port config in RS.xml.";
                log.WriteLog("TDK", str);
                throw new ApplicationException(str);
            }
        }
        public void SerialConfigApply(string key, int port, int baud, int parity, int databits, int stopbits)
        {
            _rs232Config[key].Port = port;
            _rs232Config[key].Baud = baud;
            _rs232Config[key].Parity = parity;
            _rs232Config[key].DataBits = databits;
            _rs232Config[key].StopBits = stopbits;
        }

        #endregion

        #region Object Info
        private object _objectLocker = new object();
        public UInt16 GetObjectID(string key)
        {
            Monitor.Enter(_objectLocker);
            try
            {
                XmlDocument xd = xdOInfo;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                        if (mapAttributes == null || mapAttributes.Count != 2)
                        {
                            string str = string.Format("Wrong fromat of object [{0}] information in ObjectInfo.xml.", key);
                            return 0;
                        }

                        Hashtable ht = new Hashtable();
                        for (int i = 0; i < 2; i++)
                        {
                            XmlNode xnodAtt = mapAttributes.Item(i);
                            ht.Add(xnodAtt.Name, xnodAtt.Value);
                        }
                        if (!(ht.ContainsKey("key") && ht.ContainsKey("id")))
                        {
                            string str = string.Format("Wrong fromat of object [{0}] information in ObjectInfo.xml.", key);
                            return 0;
                        }
                        if (ht["key"].ToString() == key)
                            return Convert.ToUInt16(ht["id"].ToString());
                        //end case

                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                    {
                        string str = string.Format("Unable to find the object [{0}] information in ObjectInfo.xml.", key);
                        return 0;
                    }
                    return 0;
                }
                else
                {
                    string str = string.Format("Unable to find the node [{0}] in ObjectInfo.xml.", key);
                    return 0;
                }
            }
            finally
            {
                Monitor.Exit(_objectLocker);
            }
        }

        public string GetObjectName(UInt16 ID)
        {
            Monitor.Enter(_objectLocker);
            try
            {
                XmlDocument xd = xdOInfo;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                        if (mapAttributes == null || mapAttributes.Count != 2)
                        {
                            string str = string.Format("Wrong fromat of object [{0}] information in ObjectInfo.xml.", ID);
                            return "";
                        }

                        Hashtable ht = new Hashtable();
                        for (int i = 0; i < 2; i++)
                        {
                            XmlNode xnodAtt = mapAttributes.Item(i);
                            ht.Add(xnodAtt.Name, xnodAtt.Value);
                        }
                        if (!(ht.ContainsKey("key") && ht.ContainsKey("id")))
                        {
                            string str = string.Format("Wrong fromat of object [{0}] information in ObjectInfo.xml.", ID);
                            return "";
                        }
                        if (ht["id"].ToString() == ID.ToString())
                            return ht["key"].ToString();

                        xnodSection = xnodSection.NextSibling;
                    }
                    if (xnodSection == null)
                    {
                        string str = string.Format("Unable to find the object [{0}] information in ObjectInfo.xml.", ID);
                        return "";
                    }
                    return "";
                }
                else
                {
                    string str = string.Format("Unable to find the node [{0}] in ObjectInfo.xml.", ID);
                    return "";
                }
            }
            finally
            {
                Monitor.Exit(_objectLocker);
            }
        }

        public Hashtable GetObjectDictionary()
        {
            Monitor.Enter(_objectLocker);
            try
            {
                XmlDocument xd = xdOInfo;
                XmlNode xnod = xd.DocumentElement;
                XmlNode xnodSection;
                Hashtable dictionary = new Hashtable();
                if (xnod.HasChildNodes)
                {
                    xnodSection = xnod.FirstChild;
                    while (xnodSection != null)
                    {
                        XmlNamedNodeMap mapAttributes = xnodSection.Attributes;
                        if (mapAttributes == null || mapAttributes.Count != 2)
                        {
                            string str = string.Format("Wrong fromat of ObjectInfo.xml.");
                            throw new ApplicationException(str);
                        }
                        Hashtable ht = new Hashtable();
                        for (int i = 0; i < 2; i++)
                        {
                            XmlNode xnodAtt = mapAttributes.Item(i);
                            ht.Add(xnodAtt.Name, xnodAtt.Value);
                        }
                        if (!(ht.ContainsKey("key") && ht.ContainsKey("id")))
                        {
                            string str = string.Format("Wrong fromat of ObjectInfo.xml.");
                            throw new ApplicationException(str);
                        }
                        dictionary[ht["id"].ToString()] = ht["key"].ToString();

                        xnodSection = xnodSection.NextSibling;
                    }

                    return dictionary;
                }
                else
                {
                    string str = string.Format("No any node in ObjectInfo.xml.");
                    throw new ApplicationException(str);
                }
            }
            finally
            {
                Monitor.Exit(_objectLocker);
            }
        }
        #endregion

        #region UserInfo
        private string userfile = ConstVC.FilePath.ConfigFolder + ConstVC.FilePath.User;
        private XmlDocument document = new XmlDocument();
        private UserXml db = null;
        private ArrayList userList = new ArrayList();

        public event WriteUserLogDel OnWriteUserLogRequired;

        private void WriteUserDataLog(string message)
        {
            if (OnWriteUserLogRequired != null)
            {
                OnWriteUserLogRequired(message);
            }
        }

        public ArrayList GetUserLoginData()
        {
            string msg = "";
            try
            {
                if (File.Exists(userfile))
                {
                    document.Load(userfile);
                    userList = this.getUserNamePassword();
                    return userList;
                }
                else
                {
                    msg = "User Control Table is Not exist!!";
                    return null;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }
        }

        private ArrayList getUserNamePassword()
        {
            ArrayList list = new ArrayList();
            // match user name and password 
            foreach (XmlNode node in document.GetElementsByTagName("HMIUser"))
            {
                UserInfo user = new UserInfo(null, null, null, false);
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "name")
                    {
                        user.username = attr.Value;
                        user.userExist = true;
                    }
                    else if (attr.Name == "password")
                    {
                        user.password = attr.Value;
                    }
                    else if (attr.Name == "type")
                    {
                        user.type = attr.Value;
                    }
                }
                list.Add(user);
            }
            return list;
        }

        private UserInfo getSingleUser(string username)
        {
            foreach (UserInfo info in userList)
            {
                if (info.username == username)
                    return info;
            }
            return null;
        }

        public string[] GetTypeList()
        {
            string str = "";
            foreach (XmlNode node in document.GetElementsByTagName("HMIType"))
            {
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "type")
                    {
                        str = attr.Value;
                    }

                }
            }
            return str.Split(',');
        }

        public bool AddUser(UserInfo info)
        {
            foreach (UserInfo user in userList)
            {
                if (user.username == info.username)
                    return false;
            }

            // Add a new user.
            XmlElement newHMIUser = document.CreateElement("HMIUser");
            XmlAttribute newUser = document.CreateAttribute("name");
            newUser.Value = info.username;
            newHMIUser.Attributes.Append(newUser);

            XmlAttribute newPw = document.CreateAttribute("password");
            newPw.Value = info.password;
            newHMIUser.Attributes.Append(newPw);

            XmlAttribute newType = document.CreateAttribute("type");
            newType.Value = info.type;
            newHMIUser.Attributes.Append(newType);

            document.DocumentElement.AppendChild(newHMIUser);

            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(userfile, null);
            writer.Formatting = Formatting.Indented;
            document.Save(writer);
            writer.Close();

            this.userList.Add(info);
            return true;

        }

        public bool ChangePassword(UserInfo info)
        {
            // find the user.
            XmlElement newHMIUser = document.CreateElement("HMIUser");
            bool found = false;
            foreach (XmlNode node in document.GetElementsByTagName("HMIUser"))
            {
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "name")
                    {
                        if (attr.Value == info.username)
                        {
                            found = true;
                        }
                    }
                    else if (attr.Name == "password")
                    {
                        if (found)
                        {
                            attr.Value = info.password;
                            break;
                        }
                    }
                }
                if (found)
                {
                    break;
                }
            }
            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(userfile, null);
            writer.Formatting = Formatting.Indented;
            document.Save(writer);
            writer.Close();

            //Update userList
            foreach (UserInfo info1 in userList)
            {
                if (info1.username == info.username)
                    info1.password = info.password;
            }

            return found;

        }

        public bool DeleteUser(string user)
        {
            // match user name and password 
            bool found = false;
            XmlNode root = document.DocumentElement;
            foreach (XmlNode node in document.GetElementsByTagName("HMIUser"))
            {
                foreach (XmlNode attr in node.Attributes)
                {
                    if (attr.Name == "name")
                    {
                        if (attr.Value == user)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                {
                    root.RemoveChild(node);
                    this.userList.Remove(getSingleUser(user));
                    break;
                }
            }
            // Save the document to a file and auto-indent the output.
            XmlTextWriter writer = new XmlTextWriter(userfile, null);
            writer.Formatting = Formatting.Indented;
            document.Save(writer);
            writer.Close();

            return found;
        }

        public bool UserDataQuery(Hashtable commandTable, ref Hashtable returnTable)
        {
            try
            {
                if (commandTable[UserDataQueryCommand.Command] != null)
                {
                    string userName = "";
                    string password = "";
                    string loginType = "";
                    string returnMessage = "";
                    string logMsg = "";
                    string adminPW = "";
                    string appOwner = "";
                    UserDataQueryCommand command = UserDataQueryCommand.GetUserType;
                    if (commandTable[UserDataQueryCommand.Command] != null)
                    {
                        command = (UserDataQueryCommand)commandTable[UserDataQueryCommand.Command];
                    }
                    if (commandTable[UserDataKey.ApplicationOwner] != null)
                    {
                        appOwner = commandTable[UserDataKey.ApplicationOwner].ToString();
                    }

                    switch (command)
                    {
                        case UserDataQueryCommand.Login:
                            {
                                //get username/pasword
                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                if (commandTable[UserDataKey.Password] != null)
                                {
                                    password = commandTable[UserDataKey.Password].ToString();
                                }
                                if (userName != "" && password != "")
                                {
                                    //verify login data
                                    bool b = this.CheckLoginType(userName, password, ref loginType, ref returnMessage);
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    if (b)
                                    {
                                        logMsg = string.Format("[UserLogin] LoginId=>{0} ,LoginType=>{1}", userName, loginType);
                                        WriteUserDataLog(logMsg);
                                        returnTable[UserDataKey.LoginType] = loginType;
                                        return true;
                                    }
                                    else
                                    {
                                        returnTable[UserDataKey.LoginType] = "";
                                        return false;
                                    }
                                }
                                else
                                {
                                    string msg = "";
                                    if (userName == "")
                                        msg = "UserName is empty.";
                                    if (password == "")
                                        msg += "Password is empty.";

                                    returnTable[UserDataKey.ReturnMessage] = msg;
                                    returnTable[UserDataKey.LoginType] = "";
                                    return false;
                                }
                            }
                        case UserDataQueryCommand.Logout:
                            {
                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                logMsg = string.Format("[UserLogout] LogoutId=>{0}", userName);
                                WriteUserDataLog(logMsg);
                                break;
                            }
                        case UserDataQueryCommand.AddUser:
                            {
                                //get username/pasword

                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                if (commandTable[UserDataKey.Password] != null)
                                {
                                    password = commandTable[UserDataKey.Password].ToString();
                                }
                                if (commandTable[UserDataKey.LoginType] != null)
                                {
                                    loginType = commandTable[UserDataKey.LoginType].ToString();
                                }
                                if (commandTable[UserDataKey.AdminPassword] != null)
                                {
                                    adminPW = commandTable[UserDataKey.AdminPassword].ToString();
                                }
                                if (userName != "" && password != "" && loginType != "")
                                {
                                    #region check and add user

                                    bool b = this.CheckAndAddUser(userName, password, adminPW, loginType, ref returnMessage);
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    if (b)
                                    {
                                        logMsg = string.Format("[AddUser] AddNewUserId=>{0};UserType=>{1}", userName, loginType);
                                        WriteUserDataLog(logMsg);
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    string msg = "";
                                    if (userName == "")
                                        msg = "UserName is empty.";
                                    if (password == "")
                                        msg += "Password is empty.";
                                    if (loginType == "")
                                        msg += "loginType is not assigned.";

                                    returnTable[UserDataKey.ReturnMessage] = msg;
                                    return false;
                                }
                            }
                        case UserDataQueryCommand.RemoveUser:
                            {
                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                if (commandTable[UserDataKey.AdminPassword] != null)
                                {
                                    adminPW = commandTable[UserDataKey.AdminPassword].ToString();
                                }
                                if (userName != "" && adminPW != "")
                                {
                                    #region RemoveUser

                                    if (userName.ToLower() == "admin")
                                    {
                                        returnTable[UserDataKey.ReturnMessage] = "Admin account(admin) cannot be deleted!";
                                        return false;
                                    }
                                    bool b = this.DeleteUser(userName, adminPW, ref returnMessage);
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    if (b)
                                    {
                                        logMsg = string.Format("[RemoveUser:{0}] DeleteUser:UserId=>{1}", appOwner, userName);
                                        WriteUserDataLog(logMsg);
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    string msg = "";
                                    if (userName == "")
                                        msg = "UserName is empty.";
                                    if (adminPW == "")
                                        msg += "adminPW is empty.";

                                    returnTable[UserDataKey.ReturnMessage] = msg;
                                    return false;
                                }
                            }
                        case UserDataQueryCommand.ChangePassword:
                            {
                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                if (commandTable[UserDataKey.Password] != null)
                                {
                                    password = commandTable[UserDataKey.Password].ToString();
                                }
                                string newPassowrd = "";
                                if (commandTable[UserDataKey.NewPassword] != null)
                                {
                                    newPassowrd = commandTable[UserDataKey.NewPassword].ToString();
                                }
                                if (userName != "" && password != "" && newPassowrd != "")
                                {
                                    #region change password

                                    bool b = this.ChangePassword(userName, password, newPassowrd, ref returnMessage);
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    if (b)
                                    {
                                        logMsg = string.Format("[ChangePassword] UserId=>{0}", userName);
                                        WriteUserDataLog(logMsg);
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    string msg = "";
                                    if (userName == "")
                                        msg = "UserName is empty.";
                                    if (password == "")
                                        msg += "Password is empty.";
                                    if (newPassowrd == "")
                                        msg += "newPassowrd is empty.";

                                    returnTable[UserDataKey.ReturnMessage] = msg;
                                    return false;
                                }
                            }
                        case UserDataQueryCommand.GetUserType:
                            {
                                string[] typeList = null;
                                if (this.GetUserTypeList(ref typeList, ref returnMessage))
                                {
                                    returnTable[UserDataKey.TypeList] = typeList;
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    return true;
                                }
                                else
                                {
                                    string str = "";
                                    returnTable[UserDataKey.TypeList] = str.Split(',');
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    return false;
                                }
                            }
                        case UserDataQueryCommand.VerifyUserType:
                            {
                                //get username/pasword
                                if (commandTable[UserDataKey.UserName] != null)
                                {
                                    userName = commandTable[UserDataKey.UserName].ToString();
                                }
                                if (commandTable[UserDataKey.Password] != null)
                                {
                                    password = commandTable[UserDataKey.Password].ToString();
                                }
                                if (userName != "" && password != "")
                                {
                                    //verify login data
                                    //string type = this.Check();
                                    bool b = this.CheckLoginType(userName, password, ref loginType, ref returnMessage);
                                    returnTable[UserDataKey.ReturnMessage] = returnMessage;
                                    if (b)
                                    {
                                        logMsg = string.Format("[VerifyUserType] LoginId=>{0} ,LoginType=>{1}", userName, loginType);
                                        WriteUserDataLog(logMsg);
                                        returnTable[UserDataKey.LoginType] = loginType;
                                        return true;
                                    }
                                    else
                                    {
                                        returnTable[UserDataKey.LoginType] = "";
                                        return false;
                                    }
                                }
                                else
                                {
                                    string msg = "";
                                    if (userName == "")
                                        msg = "UserName is empty.";
                                    if (password == "")
                                        msg += "Password is empty.";

                                    returnTable[UserDataKey.ReturnMessage] = msg;
                                    returnTable[UserDataKey.LoginType] = "";
                                    return false;
                                }
                            }
                    }
                }
            }
            catch (Exception e)
            {
                WriteUserDataLog("Exception occured in UserDataQuery(). Reason: " + e.Message + e.StackTrace);
            }
            return false;
        }

        private bool GetUserTypeList(ref string[] typeList, ref string returnMessage)
        {

            try
            {
                db = new UserXml(userfile);
                if (db.ParseOk)
                {
                    typeList = db.GetTypeList();
                    return true;
                }
                else
                {
                    string str = "";
                    typeList = str.Split(',');
                    returnMessage = "User control table is not exist!";
                    return false;
                }
            }
            catch (Exception ex)
            {
                string str = "";
                typeList = str.Split(',');
                returnMessage = ex.Message;
                return false;
            }
        }
        private bool CheckLoginType(string username, string password, ref string loginType, ref string returnMessage)
        {
            if (username.ToUpper() == "SWRD" && password == "00000000")
            {
                loginType = "SWRD";
                return true;
            }

            //get db
            db = new UserXml(userfile);
            if (db.ParseOk)
            {
                string input_pass = "";
                string input_user = username;
                // ** check user name / password
                UserInfo info = db.getUserNamePassword(input_user);
                input_pass = db.DoHash(info.type, info.username, password);

                // read from XML file
                if (!info.userExist)
                {
                    returnMessage = "UserName does not exist !";
                    return false;
                }
                if (info.password != input_pass)
                {
                    returnMessage = "Wrong Passowrd ! ";
                    return false;
                }

                loginType = info.type;
                return true;
            }
            else
            {
                returnMessage = "User control table is not exist!";
                return false;
            }
        }
        private bool CheckAndAddUser(string username, string password, string adminPassowrd, string loginType, ref string returnMessage)
        {
            //check user name
            //get db
            db = new UserXml(userfile);
            if (db.ParseOk)
            {
                UserInfo info1 = db.getUserNamePassword(username);
                if (info1.userExist || username.ToUpper() == "SWRD")
                {
                    returnMessage = "User Name already exist!";
                    return false;
                }
                //check admin password
                UserInfo info2 = db.getUserNamePassword("admin");
                string input_pass = db.DoHash(info2.type, info2.username, adminPassowrd);
                if (info2.password != input_pass)
                {
                    returnMessage = "Wrong admin password !";
                    return false;
                }

                // ok to add user here!
                string hash_pw = db.DoHash(loginType, username, password);
                if (db.AddUser(new UserInfo(username, hash_pw, loginType, false)))
                {
                    returnMessage = "Add User success!";
                    return true;
                }
                else
                {
                    returnMessage = "Add User Fail!";
                    return false;
                }
            }
            else
            {
                returnMessage = "User control table is not exist!";
                return false;
            }
        }
        private bool ChangePassword(string username, string oldpassword, string newPassowrd, ref string returnMessage)
        {
            db = new UserXml(userfile);
            if (db.ParseOk)
            {
                // check user name
                UserInfo info1 = db.getUserNamePassword(username);
                if (!info1.userExist)
                {
                    returnMessage = "User Name does not exist!";
                    return false;
                }

                // check user password
                string input_pass = db.DoHash(info1.type, username, oldpassword);
                if (info1.password != input_pass)
                {
                    returnMessage = "Wrong user password !";
                    return false;
                }

                // ok to change password here!
                string newpw = db.DoHash(info1.type, username, newPassowrd);
                if (db.ChangePassword(new UserInfo(username, newpw, info1.type, false)))
                {
                    returnMessage = "Change password for " + username + " success!";
                    return true;
                }
                else
                {
                    returnMessage = "Change password Fail!";
                    return false;
                }
            }
            else
            {
                returnMessage = "User control table is not exist!";
                return false;
            }
        }
        public bool DeleteUser(string username, string adminPassword, ref string returnMessage)
        {
            // deltet user
            db = new UserXml(userfile);
            if (db.ParseOk)
            {
                // check user name
                UserInfo info1 = db.getUserNamePassword(username);
                if (!info1.userExist)
                {
                    returnMessage = "User Name does not exist!";
                    return false;
                }
                // check admin password
                UserInfo info2 = db.getUserNamePassword("admin");
                string input_pass = db.DoHash(info2.type, info2.username, adminPassword);
                if (info2.password != input_pass)
                {
                    returnMessage = "Wrong admin password !";
                    return false;
                }

                // ok to delete user here!
                if (db.DeleteUser(username))
                {
                    returnMessage = "Delete success!";
                    return true;
                }
                else
                {
                    returnMessage = "Delete Fail!";
                    return false;
                }
            }
            else
            {
                returnMessage = "User control table is not exist!";
                return false;
            }
        }
        #endregion



        #region XML file handle
        public string ReadXMLFile(string filename, ref string xmlDoc)
        {
            try
            {
                XmlTextReader xtr = new XmlTextReader(WorkPath + filename);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                XmlDocument xmlobj = new XmlDocument();
                xmlobj.Load(xtr);
                xtr.Close();
                xmlDoc = StaticFileUtilities.SerializeXML(xmlobj);
                return "";
            }
            catch (Exception ex)
            {
                xmlDoc = null;
                return ex.Message;
            }
        }

        public void ResetToDefaultValue(string Type, string key)
        {
            try
            {
                switch (Type)
                {
                    case "TCPIP":
                        TCPConfigSave(key, "127.0.0.1", "88");
                        break;
                    case "RS232":
                        SerialPortConfigSave(key, 1, 9600,0 ,5 ,1);
                        break;
                    case "LoadPortActor":

                        break;

                }
            }
            catch
            {

            }
        }

        public XmlDocument XMLFileLoad(string filename)
        {
            if (!File.Exists(filename))
            {
                string str = filename + " is not exist.";
                log.WriteLog("TDK", str);
                throw new FileNotFoundException("No such file", filename);
            }

            XmlTextReader xtr = new XmlTextReader(filename);
            xtr.WhitespaceHandling = WhitespaceHandling.None;
            XmlDocument xd = new XmlDocument();
            xd.Load(xtr);
            xtr.Close();
            return xd;
        }

        public string WriteXMLFile(string filename, string xmlDoc)
        {
            try
            {
                XmlTextWriter writer = new XmlTextWriter(WorkPath + filename, null);
                writer.Formatting = Formatting.Indented;
                XmlDocument xmlobj = StaticFileUtilities.DeserializeXml(xmlDoc);
                xmlobj.Save(writer);
                writer.Close();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        #endregion
    }

    public class StaticFileUtilities
    {
        public static string SerializeXML(XmlDocument xml)
        {
            try
            {
                XmlSerializer xmlSer = new XmlSerializer(xml.GetType());
                StringWriter sWriter = new StringWriter();
                xmlSer.Serialize(sWriter, xml);
                return sWriter.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static XmlDocument DeserializeXml(string xml)
        {
            try
            {
                XmlDocument obj = new XmlDocument();
                XmlSerializer xmlSer = new XmlSerializer(obj.GetType());
                StringReader sReader = new StringReader(xml);
                XmlDocument retXml = (XmlDocument)xmlSer.Deserialize(sReader);
                return retXml;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
