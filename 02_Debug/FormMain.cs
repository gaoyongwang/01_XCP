using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ECAN;
using ECANXCP;

namespace WindowsFormsApplication
{
    public partial class FormMain : Form
    {
        BOARD_INFO m_BoardInfo = new BOARD_INFO();
        XCPApi xcpapi = new XCPApi();
        byte[] MsgTemp = new byte[8];

        ECANXCP.XCPApi.TXCPResult result;
        
        public FormMain()
        {
            InitializeComponent();
            xcpapi.MasterID = 0x7FB;
            xcpapi.SlaveID = 0x7FC;
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="strMsg"></param>
        public void PrintLog(string strMsg)
        {
            textBox_Log.AppendText(strMsg + "\n");
            textBox_Log.ScrollToCaret();
        }

        private void Init_Click(object sender, EventArgs e)
        {
            if (xcpapi.GcCanInitialize("500K", out m_BoardInfo))
            {
                PrintLog("硬件版本号：" + m_BoardInfo.hw_Version.ToString());
                PrintLog("固件版本号：" + m_BoardInfo.fw_Version.ToString());
                PrintLog("驱动程序版本号：" + m_BoardInfo.dr_Version.ToString());
                PrintLog("接口库版本号：" + m_BoardInfo.in_Version.ToString());
                PrintLog("中断号：" + m_BoardInfo.irq_Num.ToString());
                PrintLog("CAN通道数：" + m_BoardInfo.can_Num.ToString());
                PrintLog("设备序列号：" + System.Text.Encoding.Default.GetString(m_BoardInfo.str_Serial_Num));
                PrintLog("\n");
                PrintLog("硬件类型：" + System.Text.Encoding.Default.GetString(m_BoardInfo.str_hw_Type));
                PrintLog("\n");
            }
            else
            {
                PrintLog("Initialize error!");
            }
        }

        private void DeInit_Click(object sender, EventArgs e)
        {
            if (xcpapi.GcCanUnInitialize())
            {
                PrintLog("UnInitialize success!");
            }
            else
            {
                PrintLog("UnInitialize error!");
            }
            timer_Read.Enabled = false;
        }

        private void timer_Read_Tick(object sender, EventArgs e)
        {
            result = xcpapi.XCP_ShortUpload(0x00, 0xB00000C0, out MsgTemp, 0x08);
            PrintLog(BitConverter.ToSingle(MsgTemp, 1).ToString());

            result = xcpapi.XCP_ShortUpload(0x00, 0x700119B8, out MsgTemp, 0x08);
            PrintLog(BitConverter.ToSingle(MsgTemp, 1).ToString());

            result = xcpapi.XCP_ShortUpload(0x00, 0x50004394, out MsgTemp, 0x08);
            PrintLog(BitConverter.ToSingle(MsgTemp, 1).ToString());
        }

        private void button_Connect_Click(object sender, EventArgs e)
        {
            result = xcpapi.XCP_Connect(0x00, out MsgTemp, 0x08);
            PrintLog(BitConverter.ToString(MsgTemp));
        }

        private void button_Disconnect_Click(object sender, EventArgs e)
        {
            result = xcpapi.XCP_Disconnect(out MsgTemp, 0x08);
            PrintLog(BitConverter.ToString(MsgTemp));

            timer_Read.Enabled = false;
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            timer_Read.Enabled = true;
        }

        private void numericUpDown_UV_ValueChanged(object sender, EventArgs e)
        {
            result = xcpapi.XCP_SetMemoryTransferAddress(0x00, 0x50004048, out MsgTemp, 0x08);
            result = xcpapi.XCP_Download(BitConverter.GetBytes(Convert.ToSingle(numericUpDown_UV.Value)), out MsgTemp, 0x08);
            result = xcpapi.XCP_ShortUpload(0x00, 0x50004048, out MsgTemp, 0x08);
            PrintLog(BitConverter.ToSingle(MsgTemp, 1).ToString());
        }
    }
}
