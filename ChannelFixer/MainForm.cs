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

        string MainChannelName="";
        int MainChannelNumber = 0;
        string PrimaryNIC_IP = "";
        string BackupNIC_IP = "";

        enum NICType {primary, backup};

        public MainForm(string configFile)
        {
            InitializeComponent();

            try
            {
                MbeServ = new MBEServer();

                if (ReadConfigFile(configFile))
                {
                    CloseTimer.Interval = 100;
                    CloseTimer.Enabled = true;
                    return;
                }
                
                GetChannels();
                ChannelIPFix();
                RefreshTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\nПриложение будет закрыто.");
                CloseTimer.Interval = 100;
                CloseTimer.Enabled = true;
            }
        }

        private bool ReadConfigFile(string configFile)
        {
            StreamReader FileIn;
            string[] settings = new string[3];
            int i = 0;

            try
            {
                FileIn = new StreamReader(configFile);
            }
            catch (IOException exc)
            {
                MessageBox.Show("Ошибка открытия конфигурационного файла!\n" + exc.Message);
                return true;
            }

            try
            {
                while (!FileIn.EndOfStream)
                {
                    settings[i] += FileIn.ReadLine();
                    if (FileIn.EndOfStream && (i < 2))
                    {
                        MessageBox.Show("Файл настроек должен состоять минимум из 3-х строк!");
                        return true;
                    }
                    if (i == 2)
                    {
                        MainChannelName = settings[0];
                        PrimaryNIC_IP = settings[1];
                        BackupNIC_IP = settings[2];
                        break;
                    }
                    i++;
                }
            }
            catch (IOException exc)
            {
                MessageBox.Show("Ошибка чтения файла:\n" + exc.Message);
            }
            finally
            {
                FileIn.Close();
            }
            return false;
        }

        //Считывание доступных каналов
        private void GetChannels()
        {
            NumChannels = MbeServ.GetChannels(ref ptrChannelHandles, ref ptrChannelNames);
            ChannelHandles = (object[])ptrChannelHandles;
            ChannelNames = (object[])ptrChannelNames;
        }

        //Проверка ip-канала и исправление его в случае необходимости
        private void ChannelIPFix()
        {
            int i = 0;
            bool bCheck = false;

            foreach(object channel in ChannelNames)
            {
                if (MainChannelName.Equals(channel))
                {
                    bCheck = true;
                    MainChannelNumber = i;
                    break;
                }
                i++;
            }

            if (bCheck)
            {
                MbeServ.GetPropertyData((int)ChannelHandles[MainChannelNumber], (object)"PrimaryInterfaceDesc", ref ptrPropertyData);
                if (!Convert.ToString(ptrPropertyData).Contains(PrimaryNIC_IP))
                    SetChannelIP(NICType.primary);

                MbeServ.GetPropertyData((int)ChannelHandles[MainChannelNumber], (object)"BackupInterfaceDesc", ref ptrPropertyData);
                if (!Convert.ToString(ptrPropertyData).Contains(BackupNIC_IP))
                    SetChannelIP(NICType.backup);

                MbeServ.FileSave();
                HideTimer.Interval = 5000;
                HideTimer.Enabled = true;
            }
            else
            {
                MessageBox.Show("Заданный канал не найден в списке каналов!");
                CloseTimer.Interval = 100;
                CloseTimer.Enabled = true;
                return;
            }            
        }

        private void SetChannelIP(NICType ntype)
        {
            string sNICType;
            string sNICDesc;
            string sNIC_IP;

            if (ntype == NICType.primary)
            {
                sNICType = "PrimaryNetworkInterface";
                sNICDesc = "PrimaryInterfaceDesc";
                sNIC_IP = PrimaryNIC_IP;
            }
            else
            {
                sNICType = "BackupNetworkInterface";
                sNICDesc = "BackupInterfaceDesc";
                sNIC_IP = BackupNIC_IP;
            }

            for (int i = 0; i < 128; i++)
            {
                MbeServ.SetPropertyData((int)ChannelHandles[MainChannelNumber], (object)sNICType, i);

                MbeServ.GetPropertyData((int)ChannelHandles[MainChannelNumber], (object)sNICDesc, ref ptrPropertyData);

                if (Convert.ToString(ptrPropertyData).Contains(sNIC_IP))
                    break;
            }
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {           
            try
            {
                ChannelIPFix();
            }
            catch (Exception ex)
            {
                RefreshTimer.Enabled = false;
                MessageBox.Show(ex.Message);
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
