using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MBEDrv;

namespace ChannelFixer
{
    public partial class MainForm : Form
    {
        MBEServer MbeServ;

        object ptrChannelHandles = new object();
        object ptrChannelNames = new object();
        object ptrPropertyData = new object();

        //GetProperties объект не хавает, почему-то только массив объектов хотя бы из одного элемента нужен
        object[] ptrProperties = new object[1];
        object[] ChannelHandles;
        object[] ChannelNames;
       
        int NumChannels;

        private class channelParams
        {
            public string ChannelName { get; set; }
            public string PrimaryNIC_IP { get; set; }
            public string BackupNIC_IP { get; set; }
            public channelParams(string chName, string primIP, string backIP)
            {
                ChannelName = chName;
                PrimaryNIC_IP = primIP;
                BackupNIC_IP = backIP;
            }
        }

        channelParams[] channelsToFix;

        enum NICType {primary, backup};

        public MainForm(string configFile)
        {
            InitializeComponent();

            try
            {
                MbeServ = new MBEServer();

                if (!ReadConfigFile(configFile))
                {
                    CloseTimer.Interval = 5000;
                    CloseTimer.Enabled = true;
                    return;
                }
                
                GetChannels();
                RefreshTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                MainLabel.Text = ex.Message + "\nПриложение будет закрыто.";
                CloseTimer.Interval = 5000;
                CloseTimer.Enabled = true;
            }
        }

        private bool ReadConfigFile(string configFile)
        {
            StreamReader FileIn;
            var settings = new ArrayList();
            int i = 0;
            int chanNum;
            int j;

            try
            {
                FileIn = new StreamReader(configFile);
            }
            catch (IOException exc)
            {
                MainLabel.Text = "Ошибка открытия конфигурационного файла!\n" + exc.Message;
                return false;
            }

            try
            {
                while (!FileIn.EndOfStream)
                {
                    settings.Add(FileIn.ReadLine());
                    if (FileIn.EndOfStream && (i < 2))
                    {
                        MainLabel.Text = "Файл настроек должен состоять минимум из 3-х строк!";
                        return false;
                    }
                    ++i;
                }

                if((i % 3) != 0)
                {
                    MainLabel.Text = "Настройки для каждого канала должны" +
                                    "состоять ровно из 3-х строк!";
                    return false;
                }

                chanNum = i / 3;

                channelsToFix = new channelParams[chanNum];

                for (i = 0; i < chanNum; ++i)
                {
                    j = i * 3;
                    channelsToFix[i] = new channelParams((string)settings[j + 0],
                                                         (string)settings[j + 1],
                                                         (string)settings[j + 2]);
                }
            }
            catch (IOException exc)
            {
                MainLabel.Text = "Ошибка чтения файла:\n" + exc.Message;
            }
            finally
            {
                FileIn.Close();
            }
            return true;
        }

        //Считывание доступных каналов
        private void GetChannels()
        {
            NumChannels = MbeServ.GetChannels(ref ptrChannelHandles, ref ptrChannelNames);
            ChannelHandles = (object[])ptrChannelHandles;
            ChannelNames = (object[])ptrChannelNames;
        }

        //Проверка ip-канала и исправление его в случае необходимости
        private bool ChannelIPFix(channelParams channelToFix)
        {
            int i = 0;
            bool checkName = false;
            bool isPrimIPChanged = false;
            bool isBackIPChanged = false;
            bool noError = true;
            int chanNumber = 0;

            foreach(object chanName in ChannelNames)
            {
                if (channelToFix.ChannelName.Equals(chanName))
                {
                    chanNumber = i;
                    checkName = true;
                    break;
                }
                ++i;
            }

            if (checkName)
            {
                MbeServ.GetPropertyData((int)ChannelHandles[chanNumber], (object)"PrimaryInterfaceDesc", ref ptrPropertyData);
                if (!Convert.ToString(ptrPropertyData).Contains(channelToFix.PrimaryNIC_IP)) 
                {
                    isPrimIPChanged = SetChannelIP(NICType.primary, channelToFix, chanNumber);
                }

                MbeServ.GetPropertyData((int)ChannelHandles[chanNumber], (object)"BackupInterfaceDesc", ref ptrPropertyData);
                if (!Convert.ToString(ptrPropertyData).Contains(channelToFix.BackupNIC_IP))
                {
                    isBackIPChanged = SetChannelIP(NICType.backup, channelToFix, chanNumber);
                }

                if(isPrimIPChanged || isBackIPChanged)
                    MbeServ.FileSave();

                HideTimer.Interval = 5000;
                HideTimer.Enabled = true;
            }
            else
                noError = false;
            return noError;
        }

        private bool SetChannelIP(NICType ntype, channelParams channelToFix,
                                  int channelNumber)
        {
            string sNICType;
            string sNICDesc;
            string sNIC_IP;
            bool isIPChanged = false;

            if (ntype == NICType.primary)
            {
                sNICType = "PrimaryNetworkInterface";
                sNICDesc = "PrimaryInterfaceDesc";
                sNIC_IP = channelToFix.PrimaryNIC_IP;
            }
            else
            {
                sNICType = "BackupNetworkInterface";
                sNICDesc = "BackupInterfaceDesc";
                sNIC_IP = channelToFix.BackupNIC_IP;
            }

            MbeServ.GetPropertyData((int)ChannelHandles[channelNumber], (object)sNICDesc, ref ptrPropertyData);
            if (!Convert.ToString(ptrPropertyData).Contains(sNIC_IP))
            {
                for (int i = 0; i < 128; i++)
                {
                    MbeServ.SetPropertyData((int)ChannelHandles[channelNumber], (object)sNICType, i);
                    MbeServ.GetPropertyData((int)ChannelHandles[channelNumber], (object)sNICDesc, ref ptrPropertyData);

                    if (Convert.ToString(ptrPropertyData).Contains(sNIC_IP))
                    {
                        isIPChanged = true;
                        break;
                    }
                }
            }

            return isIPChanged;
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {           
            foreach(channelParams chToFix in channelsToFix)
                if (!ChannelIPFix(chToFix))
                {
                    RefreshTimer.Enabled = false;
                    CloseTimer.Interval = 5000;
                    CloseTimer.Enabled = true;
                    MainLabel.Text = "Заданный канал не найден в списке каналов!" +
                                     "Приложение будет закрыто." ;
                    break;
                }
        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            this.Hide();
            HideTimer.Enabled = false;
        }

        private void MainForm_Leave(object sender, EventArgs e)
        {
            HideTimer.Interval = 5000;
            HideTimer.Enabled = true;
        }   
    }
}
