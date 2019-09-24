using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECAN;

namespace ECANXCP
{
    public class EcanXcpApi
    {
        private UInt32 deviceType;// 设备类型号，USBCAN I 选择3，USBCAN II 选择4
        private UInt32 deviceIndex;// 设备索引号，当只有一个设备时，索引号为0，有两个时可以为0或1
        private UInt32 canIndex;// 第几路CAN，即对应卡的CAN通道号，CAN0为0，CAN1为1
        private string baudrate;// 波特率
        private UInt32 masterID;// 主设备ID
        private UInt32 slaveID;// 从设备ID
        INIT_CONFIG initConfig = new INIT_CONFIG();// 创建CAN初始化配置结构体
        CAN_OBJ frameInfo = new CAN_OBJ();// 报文帧
        FILTER_RECORD filterRecord = new FILTER_RECORD();// 定义CAN滤波器的滤波范围

        private UInt32 numOfFrameTX;// 实际发送帧数量
        private UInt32 numOfFrameRX;// 接收帧数量

        public UInt32 DeviceType { get; set; }
        public UInt32 DeviceIndex { get; set; }
        public UInt32 CanIndex { get; set; }
        public string Baudrate { get; set; }
        public UInt32 MasterID { get; set; }
        public UInt32 SlaveID { get; set; }

        /// <summary>
        /// 定义错误码
        /// </summary>
        public enum EcanXcpResult : uint
        {
            // Codes for not sucessfully executed XCP commands
            //
            /// <summary>
            /// Command processor synchronization
            /// </summary>
            XCP_ERR_CMD_SYNCH = 0,
            /// <summary>
            /// Command was not executed
            /// </summary>
            XCP_ERR_CMD_BUSY = 0x10,
            /// <summary>
            /// Command rejected because DAQ is running
            /// </summary>
            XCP_ERR_DAQ_ACTIVE = 0x11,
            /// <summary>
            /// Command rejected because PGM is running
            /// </summary>
            XCP_ERR_PGM_ACTIVE = 0x12,
            /// <summary>
            /// Unknown command or not implemented optional command
            /// </summary>
            XCP_ERR_CMD_UNKNOWN = 0x20,
            /// <summary>
            /// Command syntax invalid
            /// </summary>
            XCP_ERR_CMD_SYNTAX = 0x21,
            /// <summary>
            /// Command syntax valid but command parameter(s) out of range
            /// </summary>
            XCP_ERR_OUT_OF_RANGE = 0x22,
            /// <summary>
            /// The memory location is write protected
            /// </summary>
            XCP_ERR_WRITE_PROTECTED = 0x23,
            /// <summary>
            /// The memory location is not accessible
            /// </summary>
            XCP_ERR_ACCESS_DENIED = 0x24,
            /// <summary>
            /// Access denied,Seed & Key is required
            /// </summary>
            XCP_ERR_ACCESS_LOCKED = 0x25,
            /// <summary>
            /// Selected page not available
            /// </summary>
            XCP_ERR_PAGE_NOT_VALID = 0x26,
            /// <summary>
            /// Selected page mode not available
            /// </summary>
            XCP_ERR_MODE_NOT_VALID = 0x27,
            /// <summary>
            /// Selected segment not valid
            /// </summary>
            XCP_ERR_SEGMENT_NOT_VALID = 0x28,
            /// <summary>
            /// Sequence error
            /// </summary>
            XCP_ERR_SEQUENCE = 0x29,
            /// <summary>
            /// DAQ configuration not valid
            /// </summary>
            XCP_ERR_DAQ_CONFIG = 0x2A,
            /// <summary>
            /// Memory overflow error
            /// </summary>
            XCP_ERR_MEMORY_OVERFLOW = 0x30,
            /// <summary>
            /// Generic error
            /// </summary>
            XCP_ERR_GENERIC = 0x31,
            /// <summary>
            /// The slave internal program verify routine detects an error
            /// </summary>
            XCP_ERR_VERIFY = 0x32,
            /// <summary>
            /// Access to the requested resource is temporary not possible
            /// </summary>
            XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE = 0x33,

            // API return error codes
            //
            /// <summary>
            /// Acknowledge / no error
            /// </summary>
            XCP_ERR_OK = (1 << 8),
            /// <summary>
            /// Function not available / Operation not implemented
            /// </summary>
            XCP_ERR_NOT_IMPLEMENTED = (2 << 8),
            /// <summary>
            /// Invalid parameter value
            /// </summary>
            XCP_ERR_INVALID_PARAMETER = (3 << 8),
            /// <summary>
            /// The maximum amount of registered Slave channels was reached
            /// </summary>
            XCP_ERR_MAX_CHANNELS = (4 << 8),
            /// <summary>
            /// The given handle is invalid
            /// </summary>
            XCP_ERROR_INVALID_HANDLE = (5 << 8),
            /// <summary>
            /// A timeout was reached by calling a function synchronously
            /// </summary>
            XCP_ERR_INTERNAL_TIMEOUT = (6 << 8),
            /// <summary>
            /// The queue being referred is empty
            /// </summary>
            XCP_ERR_QUEUE_EMPTY = (7 << 8),
            /// <summary>
            /// The size of the given buffer, is not big enough
            /// </summary>
            XCP_ERR_INSUFFICIENT_BUFFER = (8 << 8),

            // Transport protocol error flags
            //
            /// <summary>
            /// Flag for a specific error within the underlying transport channel 
            /// </summary>
            XCP_ERR_TRANSPORT_CHANNEL = 0x80000000
        }

        public EcanXcpApi()
        {
            this.deviceType = 3;
            this.deviceIndex = 0;
            this.canIndex = 0;
            this.baudrate = "500K";
            this.masterID = 0x7FB;
            this.slaveID = 0x7FC;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="baudrate">波特率</param>
        /// <param name="boardInfo">设备信息</param>
        /// <returns></returns>
        public bool GcCanInitialize(out BOARD_INFO boardInfo)
        {
            boardInfo.hw_Version = 0x00;
            boardInfo.fw_Version = 0x00;
            boardInfo.dr_Version = 0x00;
            boardInfo.in_Version = 0x00;
            boardInfo.irq_Num = 0x00;
            boardInfo.can_Num = 0x00;
            boardInfo.str_Serial_Num = new byte[] { 0x00 };
            boardInfo.str_hw_Type = new byte[] { 0x00 };
            boardInfo.Reserved = new ushort[] { 0x00 };

            // 滤波设置
            initConfig.AccCode = ((UInt32)(slaveID) << 21);
            initConfig.AccMask = 0x001FFFFF;
            // 初始化配置滤波使能
            initConfig.Filter = 1;

            filterRecord.ExtFrame = 0;
            filterRecord.Start = masterID;
            filterRecord.End = slaveID;

            // 初始化配置波特率
            switch (baudrate)
            {
                case "1000K":

                    initConfig.Timing0 = 0;
                    initConfig.Timing1 = 0x14;
                    break;
                case "800K":

                    initConfig.Timing0 = 0;
                    initConfig.Timing1 = 0x16;
                    break;
                case "666K":

                    initConfig.Timing0 = 0x80;
                    initConfig.Timing1 = 0xb6;
                    break;
                case "500K":

                    initConfig.Timing0 = 0;
                    initConfig.Timing1 = 0x1c;
                    break;
                case "400K":

                    initConfig.Timing0 = 0x80;
                    initConfig.Timing1 = 0xfa;
                    break;
                case "250K":

                    initConfig.Timing0 = 0x01;
                    initConfig.Timing1 = 0x1c;
                    break;
                case "200K":

                    initConfig.Timing0 = 0x81;
                    initConfig.Timing1 = 0xfa;
                    break;
                case "125K":

                    initConfig.Timing0 = 0x03;
                    initConfig.Timing1 = 0x1c;
                    break;
                case "100K":

                    initConfig.Timing0 = 0x04;
                    initConfig.Timing1 = 0x1c;
                    break;
                case "80K":

                    initConfig.Timing0 = 0x83;
                    initConfig.Timing1 = 0xff;
                    break;
                case "50K":

                    initConfig.Timing0 = 0x09;
                    initConfig.Timing1 = 0x1c;
                    break;

            }

            // 初始化配置为正常模式
            initConfig.Mode = 0;

            // 打开CAN
            if (ECANDLL.OpenDevice(deviceType, deviceIndex, canIndex) == ECAN.ECANStatus.STATUS_OK)
            {
                // 初始化CAN
                if (ECANDLL.InitCAN(deviceType, deviceIndex, canIndex, ref initConfig) == ECAN.ECANStatus.STATUS_OK)
                {
                    // 设置CAN 滤波器的滤波范围 实测不起作用
                    if (ECANDLL.SetReference(deviceType, deviceIndex, canIndex, 1, ref filterRecord) == ECAN.ECANStatus.STATUS_OK)
                    {
                        // 启动USBCAN设备的通道0
                        if (ECANDLL.StartCAN(deviceType, deviceIndex, canIndex) == ECAN.ECANStatus.STATUS_OK)
                        {
                            if (ECANDLL.ReadBoardInfo(deviceType, deviceIndex, out boardInfo) == ECAN.ECANStatus.STATUS_OK)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    ECANDLL.CloseDevice(deviceType, deviceIndex);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 去初始化
        /// </summary>
        /// <returns></returns>
        public bool GcCanUnInitialize()
        {
            // 关闭USBCAN设备的通道1
            if (ECANDLL.CloseDevice(deviceType, deviceIndex) == ECAN.ECANStatus.STATUS_OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="mode">模式</param>
        /// <param name="ctoBuffer">返回数据</param>
        /// <param name="ctoBufferLength">返回数据长度</param>
        /// <returns></returns>
        public EcanXcpResult XCP_Connect(byte mode, out byte[] ctoBuffer, UInt16 ctoBufferLength)
        {
            #region Packet Identifier:CMD Command:CONNECT
            // 报文帧ID为
            frameInfo.ID = masterID;
            // 发送帧类型 正常发送
            frameInfo.SendType = 0;
            // 是否为远程帧 0为数据帧
            frameInfo.RemoteFlag = 0;
            // 是否为扩展帧 0为标准帧，11位帧ID
            frameInfo.ExternFlag = 0;
            // 数据长度
            frameInfo.DataLen = 2;
            // CAN报文的数据
            frameInfo.data = new byte[8];
            // 系统保留
            //CANObj.Reserved = new byte[3];

            frameInfo.data[0] = 0xFF;
            frameInfo.data[1] = mode;

            if ((numOfFrameTX = ECANDLL.Transmit(deviceType, deviceIndex, canIndex, frameInfo, 1)) != 1)// 如果发送命令失败
            {
                ctoBuffer = new byte[ctoBufferLength];
                // Function not available / Operation not implemented
                return EcanXcpResult.XCP_ERR_NOT_IMPLEMENTED;
            }
            #endregion

            #region 读取应答数据
            for (int i = 0; i < 5; i++)// 尝试读取5次
            {
                if ((numOfFrameRX = ECANDLL.GetReceiveNum(deviceType, deviceIndex, canIndex)) > 0)// 如果收到数据帧
                {
                    numOfFrameRX = ECANDLL.Receive(deviceType, DeviceIndex, canIndex, out frameInfo, 1, 10);
                    if (frameInfo.data[0] == 0xFF)// Packet Identifier:RES
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        // Acknowledge / no error
                        return EcanXcpResult.XCP_ERR_OK;
                    }
                    else if (frameInfo.data[0] == 0xFE)// Packet Identifier:ERR
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        #region Error Code
                        switch (frameInfo.data[1])// 返回 Error Code
                        {
                            case 0:
                                return EcanXcpResult.XCP_ERR_CMD_SYNCH;
                            case 0x10:
                                return EcanXcpResult.XCP_ERR_CMD_BUSY;
                            case 0x11:
                                return EcanXcpResult.XCP_ERR_DAQ_ACTIVE;
                            case 0x12:
                                return EcanXcpResult.XCP_ERR_PGM_ACTIVE;
                            case 0x20:
                                return EcanXcpResult.XCP_ERR_CMD_UNKNOWN;
                            case 0x21:
                                return EcanXcpResult.XCP_ERR_CMD_SYNTAX;
                            case 0x22:
                                return EcanXcpResult.XCP_ERR_OUT_OF_RANGE;
                            case 0x23:
                                return EcanXcpResult.XCP_ERR_WRITE_PROTECTED;
                            case 0x24:
                                return EcanXcpResult.XCP_ERR_ACCESS_DENIED;
                            case 0x25:
                                return EcanXcpResult.XCP_ERR_ACCESS_DENIED;
                            case 0x26:
                                return EcanXcpResult.XCP_ERR_PAGE_NOT_VALID;
                            case 0x27:
                                return EcanXcpResult.XCP_ERR_MODE_NOT_VALID;
                            case 0x28:
                                return EcanXcpResult.XCP_ERR_SEGMENT_NOT_VALID;
                            case 0x29:
                                return EcanXcpResult.XCP_ERR_SEQUENCE;
                            case 0x2A:
                                return EcanXcpResult.XCP_ERR_DAQ_CONFIG;
                            case 0x30:
                                return EcanXcpResult.XCP_ERR_MEMORY_OVERFLOW;
                            case 0x31:
                                return EcanXcpResult.XCP_ERR_GENERIC;
                            case 0x32:
                                return EcanXcpResult.XCP_ERR_VERIFY;
                            case 0x33:
                                return EcanXcpResult.XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE;
                            default:
                                return EcanXcpResult.XCP_ERR_INVALID_PARAMETER;
                        }
                        #endregion
                    }
                    else
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        // Invalid parameter value
                        return EcanXcpResult.XCP_ERR_INVALID_PARAMETER;
                    }
                }
                else//如果没有数据帧，等待1ms
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            // 尝试5次，超时
            ctoBuffer = new byte[ctoBufferLength];
            // A timeout was reached by calling a function synchronously
            return EcanXcpResult.XCP_ERR_INTERNAL_TIMEOUT;
            #endregion
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="ctoBuffer">返回数据</param>
        /// <param name="ctoBufferLength">返回数据长度</param>
        /// <returns></returns>
        public EcanXcpResult XCP_Disconnect(out byte[] ctoBuffer, UInt16 ctoBufferLength)
        {
            #region Packet Identifier:CMD Command:DISCONNECT
            // 报文帧ID为
            frameInfo.ID = masterID;
            // 发送帧类型 正常发送
            frameInfo.SendType = 0;
            // 是否为远程帧 0为数据帧
            frameInfo.RemoteFlag = 0;
            // 是否为扩展帧 0为标准帧，11位帧ID
            frameInfo.ExternFlag = 0;
            // 数据长度
            frameInfo.DataLen = 1;
            // CAN报文的数据
            frameInfo.data = new byte[8];
            // 系统保留
            //CANObj.Reserved = new byte[3];

            frameInfo.data[0] = 0xFE;

            if ((numOfFrameTX = ECANDLL.Transmit(deviceType, deviceIndex, canIndex, frameInfo, 1)) != 1)// 如果发送命令失败
            {
                ctoBuffer = new byte[ctoBufferLength];
                // Function not available / Operation not implemented
                return EcanXcpResult.XCP_ERR_NOT_IMPLEMENTED;
            }
            #endregion

            #region 读取应答数据
            for (int i = 0; i < 5; i++)// 尝试读取5次
            {
                if ((numOfFrameRX = ECANDLL.GetReceiveNum(deviceType, deviceIndex, canIndex)) > 0)// 如果收到数据帧
                {
                    numOfFrameRX = ECANDLL.Receive(deviceType, deviceIndex, canIndex, out frameInfo, 1, 10);
                    if (frameInfo.data[0] == 0xFF)// Packet Identifier:RES
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        // Acknowledge / no error
                        return EcanXcpResult.XCP_ERR_OK;
                    }
                    else if (frameInfo.data[0] == 0xFE)// Packet Identifier:ERR
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        #region Error Code
                        switch (frameInfo.data[1])// 返回 Error Code
                        {
                            case 0:
                                return EcanXcpResult.XCP_ERR_CMD_SYNCH;
                            case 0x10:
                                return EcanXcpResult.XCP_ERR_CMD_BUSY;
                            case 0x11:
                                return EcanXcpResult.XCP_ERR_DAQ_ACTIVE;
                            case 0x12:
                                return EcanXcpResult.XCP_ERR_PGM_ACTIVE;
                            case 0x20:
                                return EcanXcpResult.XCP_ERR_CMD_UNKNOWN;
                            case 0x21:
                                return EcanXcpResult.XCP_ERR_CMD_SYNTAX;
                            case 0x22:
                                return EcanXcpResult.XCP_ERR_OUT_OF_RANGE;
                            case 0x23:
                                return EcanXcpResult.XCP_ERR_WRITE_PROTECTED;
                            case 0x24:
                                return EcanXcpResult.XCP_ERR_ACCESS_DENIED;
                            case 0x25:
                                return EcanXcpResult.XCP_ERR_ACCESS_DENIED;
                            case 0x26:
                                return EcanXcpResult.XCP_ERR_PAGE_NOT_VALID;
                            case 0x27:
                                return EcanXcpResult.XCP_ERR_MODE_NOT_VALID;
                            case 0x28:
                                return EcanXcpResult.XCP_ERR_SEGMENT_NOT_VALID;
                            case 0x29:
                                return EcanXcpResult.XCP_ERR_SEQUENCE;
                            case 0x2A:
                                return EcanXcpResult.XCP_ERR_DAQ_CONFIG;
                            case 0x30:
                                return EcanXcpResult.XCP_ERR_MEMORY_OVERFLOW;
                            case 0x31:
                                return EcanXcpResult.XCP_ERR_GENERIC;
                            case 0x32:
                                return EcanXcpResult.XCP_ERR_VERIFY;
                            case 0x33:
                                return EcanXcpResult.XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE;
                            default:
                                return EcanXcpResult.XCP_ERR_INVALID_PARAMETER;
                        }
                        #endregion
                    }
                    else
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        // Invalid parameter value
                        return EcanXcpResult.XCP_ERR_INVALID_PARAMETER;
                    }
                }
                else//如果没有数据帧，等待1ms
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            // 尝试5次，超时
            ctoBuffer = new byte[ctoBufferLength];
            // A timeout was reached by calling a function synchronously
            return EcanXcpResult.XCP_ERR_INTERNAL_TIMEOUT;
            #endregion
        }

        /// <summary>
        /// 设置地址
        /// </summary>
        /// <param name="addrExtension">扩展地址</param>
        /// <param name="addr">地址</param>
        /// <param name="ctoBuffer">返回数据</param>
        /// <param name="ctoBufferLength">返回数据长度</param>
        /// <returns></returns>
        public EcanXcpResult XCP_SetMemoryTransferAddress(byte addrExtension, UInt32 addr, out byte[] ctoBuffer, UInt16 ctoBufferLength)
        {
            #region Packet Identifier:CMD Command:SET_MTA
            // 报文帧ID为
            frameInfo.ID = masterID;
            // 发送帧类型 正常发送
            frameInfo.SendType = 0;
            // 是否为远程帧 0为数据帧
            frameInfo.RemoteFlag = 0;
            // 是否为扩展帧 0为标准帧，11位帧ID
            frameInfo.ExternFlag = 0;
            // 数据长度
            frameInfo.DataLen = 8;
            // CAN报文的数据
            frameInfo.data = new byte[8];
            // 系统保留
            //CANObj.Reserved = new byte[3];

            frameInfo.data[0] = 0xF6;
            frameInfo.data[3] = addrExtension;
            Array.Copy(BitConverter.GetBytes(addr), 0, frameInfo.data, 4, 4);// 将Addr转换成数组并复制到FrameInfo的后四个元素

            if ((numOfFrameTX = ECANDLL.Transmit(deviceType, deviceIndex, canIndex, frameInfo, 1)) != 1)// 如果发送命令失败
            {
                ctoBuffer = new byte[ctoBufferLength];
                // Function not available / Operation not implemented
                return EcanXcpResult.XCP_ERR_NOT_IMPLEMENTED;
            }
            #endregion

            #region 读取应答数据
            for (int i = 0; i < 5; i++)// 尝试读取5次
            {
                if ((numOfFrameRX = ECANDLL.GetReceiveNum(deviceType, deviceIndex, canIndex)) > 0)// 如果收到数据帧
                {
                    numOfFrameRX = ECANDLL.Receive(deviceType, deviceIndex, canIndex, out frameInfo, 1, 10);
                    if (frameInfo.data[0] == 0xFF)// Packet Identifier:RES
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        // Acknowledge / no error
                        return EcanXcpResult.XCP_ERR_OK;
                    }
                    else if (frameInfo.data[0] == 0xFE)// Packet Identifier:ERR
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        #region Error Code
                        switch (frameInfo.data[1])// 返回 Error Code
                        {
                            case 0:
                                return EcanXcpResult.XCP_ERR_CMD_SYNCH;
                            case 0x10:
                                return EcanXcpResult.XCP_ERR_CMD_BUSY;
                            case 0x11:
                                return EcanXcpResult.XCP_ERR_DAQ_ACTIVE;
                            case 0x12:
                                return EcanXcpResult.XCP_ERR_PGM_ACTIVE;
                            case 0x20:
                                return EcanXcpResult.XCP_ERR_CMD_UNKNOWN;
                            case 0x21:
                                return EcanXcpResult.XCP_ERR_CMD_SYNTAX;
                            case 0x22:
                                return EcanXcpResult.XCP_ERR_OUT_OF_RANGE;
                            case 0x23:
                                return EcanXcpResult.XCP_ERR_WRITE_PROTECTED;
                            case 0x24:
                                return EcanXcpResult.XCP_ERR_ACCESS_DENIED;
                            case 0x25:
                                return EcanXcpResult.XCP_ERR_ACCESS_DENIED;
                            case 0x26:
                                return EcanXcpResult.XCP_ERR_PAGE_NOT_VALID;
                            case 0x27:
                                return EcanXcpResult.XCP_ERR_MODE_NOT_VALID;
                            case 0x28:
                                return EcanXcpResult.XCP_ERR_SEGMENT_NOT_VALID;
                            case 0x29:
                                return EcanXcpResult.XCP_ERR_SEQUENCE;
                            case 0x2A:
                                return EcanXcpResult.XCP_ERR_DAQ_CONFIG;
                            case 0x30:
                                return EcanXcpResult.XCP_ERR_MEMORY_OVERFLOW;
                            case 0x31:
                                return EcanXcpResult.XCP_ERR_GENERIC;
                            case 0x32:
                                return EcanXcpResult.XCP_ERR_VERIFY;
                            case 0x33:
                                return EcanXcpResult.XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE;
                            default:
                                return EcanXcpResult.XCP_ERR_INVALID_PARAMETER;
                        }
                        #endregion
                    }
                    else
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        // Invalid parameter value
                        return EcanXcpResult.XCP_ERR_INVALID_PARAMETER;
                    }
                }
                else//如果没有数据帧，等待1ms
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            // 尝试5次，超时
            ctoBuffer = new byte[ctoBufferLength];
            // A timeout was reached by calling a function synchronously
            return EcanXcpResult.XCP_ERR_INTERNAL_TIMEOUT;
            #endregion
        }

        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="addrExtension">扩展地址</param>
        /// <param name="addr">地址</param>
        /// <param name="ctoBuffer">返回数据</param>
        /// <param name="ctoBufferLength">返回数据长度</param>
        /// <returns></returns>
        public EcanXcpResult XCP_ShortUpload(byte addrExtension, UInt32 addr, out byte[] ctoBuffer, UInt16 ctoBufferLength)
        {
            #region Packet Identifier:CMD Command:SHORT_UPLOAD
            // 报文帧ID为
            frameInfo.ID = masterID;
            // 发送帧类型 正常发送
            frameInfo.SendType = 0;
            // 是否为远程帧 0为数据帧
            frameInfo.RemoteFlag = 0;
            // 是否为扩展帧 0为标准帧，11位帧ID
            frameInfo.ExternFlag = 0;
            // 数据长度
            frameInfo.DataLen = 8;
            // CAN报文的数据
            frameInfo.data = new byte[8];
            // 系统保留
            //CANObj.Reserved = new byte[3];

            frameInfo.data[0] = 0xF4;
            frameInfo.data[1] = (byte)BitConverter.GetBytes(addr).Length;
            frameInfo.data[3] = addrExtension;
            Array.Copy(BitConverter.GetBytes(addr), 0, frameInfo.data, 4, 4);// 将Addr转换成数组并复制到FrameInfo的后四个元素

            if ((numOfFrameTX = ECANDLL.Transmit(deviceType, deviceIndex, canIndex, frameInfo, 1)) != 1)// 如果发送命令失败
            {
                ctoBuffer = new byte[ctoBufferLength];
                // Function not available / Operation not implemented
                return EcanXcpResult.XCP_ERR_NOT_IMPLEMENTED;
            }
            #endregion

            #region 读取应答数据
            for (int i = 0; i < 5; i++)// 尝试读取5次
            {
                if ((numOfFrameRX = ECANDLL.GetReceiveNum(deviceType, deviceIndex, canIndex)) > 0)// 如果收到数据帧
                {
                    numOfFrameRX = ECANDLL.Receive(deviceType, deviceIndex, canIndex, out frameInfo, 1, 10);
                    if (frameInfo.data[0] == 0xFF)// Packet Identifier:RES
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        // Acknowledge / no error
                        return EcanXcpResult.XCP_ERR_OK;
                    }
                    else if (frameInfo.data[0] == 0xFE)// Packet Identifier:ERR
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        #region Error Code
                        switch (frameInfo.data[1])// 返回 Error Code
                        {
                            case 0:
                                return EcanXcpResult.XCP_ERR_CMD_SYNCH;
                            case 0x10:
                                return EcanXcpResult.XCP_ERR_CMD_BUSY;
                            case 0x11:
                                return EcanXcpResult.XCP_ERR_DAQ_ACTIVE;
                            case 0x12:
                                return EcanXcpResult.XCP_ERR_PGM_ACTIVE;
                            case 0x20:
                                return EcanXcpResult.XCP_ERR_CMD_UNKNOWN;
                            case 0x21:
                                return EcanXcpResult.XCP_ERR_CMD_SYNTAX;
                            case 0x22:
                                return EcanXcpResult.XCP_ERR_OUT_OF_RANGE;
                            case 0x23:
                                return EcanXcpResult.XCP_ERR_WRITE_PROTECTED;
                            case 0x24:
                                return EcanXcpResult.XCP_ERR_ACCESS_DENIED;
                            case 0x25:
                                return EcanXcpResult.XCP_ERR_ACCESS_DENIED;
                            case 0x26:
                                return EcanXcpResult.XCP_ERR_PAGE_NOT_VALID;
                            case 0x27:
                                return EcanXcpResult.XCP_ERR_MODE_NOT_VALID;
                            case 0x28:
                                return EcanXcpResult.XCP_ERR_SEGMENT_NOT_VALID;
                            case 0x29:
                                return EcanXcpResult.XCP_ERR_SEQUENCE;
                            case 0x2A:
                                return EcanXcpResult.XCP_ERR_DAQ_CONFIG;
                            case 0x30:
                                return EcanXcpResult.XCP_ERR_MEMORY_OVERFLOW;
                            case 0x31:
                                return EcanXcpResult.XCP_ERR_GENERIC;
                            case 0x32:
                                return EcanXcpResult.XCP_ERR_VERIFY;
                            case 0x33:
                                return EcanXcpResult.XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE;
                            default:
                                return EcanXcpResult.XCP_ERR_INVALID_PARAMETER;
                        }
                        #endregion
                    }
                    else
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        // Invalid parameter value
                        return EcanXcpResult.XCP_ERR_INVALID_PARAMETER;
                    }
                }
                else//如果没有数据帧，等待1ms
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            // 尝试5次，超时
            ctoBuffer = new byte[ctoBufferLength];
            // A timeout was reached by calling a function synchronously
            return EcanXcpResult.XCP_ERR_INTERNAL_TIMEOUT;
            #endregion
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="data">下载的数据</param>
        /// <param name="ctoBuffer">返回数据</param>
        /// <param name="ctoBufferLength">返回数据长度</param>
        /// <returns></returns>
        public EcanXcpResult XCP_Download(byte[] data, out byte[] ctoBuffer, UInt16 ctoBufferLength)
        {
            #region Packet Identifier:CMD Command:DOWNLOAD
            // 报文帧ID为
            frameInfo.ID = masterID;
            // 发送帧类型 正常发送
            frameInfo.SendType = 0;
            // 是否为远程帧 0为数据帧
            frameInfo.RemoteFlag = 0;
            // 是否为扩展帧 0为标准帧，11位帧ID
            frameInfo.ExternFlag = 0;
            // 数据长度
            frameInfo.DataLen = (byte)(data.Length + 2);
            // CAN报文的数据
            frameInfo.data = new byte[8];
            // 系统保留
            //CANObj.Reserved = new byte[3];

            frameInfo.data[0] = 0xF0;
            frameInfo.data[1] = (byte)data.Length;
            Array.Copy(data, 0, frameInfo.data, 2, data.Length);// 将Data复制到FrameInfo

            if ((numOfFrameTX = ECANDLL.Transmit(deviceType, deviceIndex, canIndex, frameInfo, 1)) != 1)// 如果发送命令失败
            {
                ctoBuffer = new byte[ctoBufferLength];
                // Function not available / Operation not implemented
                return EcanXcpResult.XCP_ERR_NOT_IMPLEMENTED;
            }
            #endregion

            #region 读取应答数据
            for (int i = 0; i < 5; i++)// 尝试读取5次
            {
                if ((numOfFrameRX = ECANDLL.GetReceiveNum(deviceType, deviceIndex, canIndex)) > 0)// 如果收到数据帧
                {
                    numOfFrameRX = ECANDLL.Receive(deviceType, deviceIndex, canIndex, out frameInfo, 1, 10);
                    if (frameInfo.data[0] == 0xFF)// Packet Identifier:RES
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        // Acknowledge / no error
                        return EcanXcpResult.XCP_ERR_OK;
                    }
                    else if (frameInfo.data[0] == 0xFE)// Packet Identifier:ERR
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        #region Error Code
                        switch (frameInfo.data[1])// 返回 Error Code
                        {
                            case 0:
                                return EcanXcpResult.XCP_ERR_CMD_SYNCH;
                            case 0x10:
                                return EcanXcpResult.XCP_ERR_CMD_BUSY;
                            case 0x11:
                                return EcanXcpResult.XCP_ERR_DAQ_ACTIVE;
                            case 0x12:
                                return EcanXcpResult.XCP_ERR_PGM_ACTIVE;
                            case 0x20:
                                return EcanXcpResult.XCP_ERR_CMD_UNKNOWN;
                            case 0x21:
                                return EcanXcpResult.XCP_ERR_CMD_SYNTAX;
                            case 0x22:
                                return EcanXcpResult.XCP_ERR_OUT_OF_RANGE;
                            case 0x23:
                                return EcanXcpResult.XCP_ERR_WRITE_PROTECTED;
                            case 0x24:
                                return EcanXcpResult.XCP_ERR_ACCESS_DENIED;
                            case 0x25:
                                return EcanXcpResult.XCP_ERR_ACCESS_DENIED;
                            case 0x26:
                                return EcanXcpResult.XCP_ERR_PAGE_NOT_VALID;
                            case 0x27:
                                return EcanXcpResult.XCP_ERR_MODE_NOT_VALID;
                            case 0x28:
                                return EcanXcpResult.XCP_ERR_SEGMENT_NOT_VALID;
                            case 0x29:
                                return EcanXcpResult.XCP_ERR_SEQUENCE;
                            case 0x2A:
                                return EcanXcpResult.XCP_ERR_DAQ_CONFIG;
                            case 0x30:
                                return EcanXcpResult.XCP_ERR_MEMORY_OVERFLOW;
                            case 0x31:
                                return EcanXcpResult.XCP_ERR_GENERIC;
                            case 0x32:
                                return EcanXcpResult.XCP_ERR_VERIFY;
                            case 0x33:
                                return EcanXcpResult.XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE;
                            default:
                                return EcanXcpResult.XCP_ERR_INVALID_PARAMETER;
                        }
                        #endregion
                    }
                    else
                    {
                        ctoBuffer = frameInfo.data;// 回传接收到的数据
                        // Invalid parameter value
                        return EcanXcpResult.XCP_ERR_INVALID_PARAMETER;
                    }
                }
                else//如果没有数据帧，等待1ms
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            // 尝试5次，超时
            ctoBuffer = new byte[ctoBufferLength];
            // A timeout was reached by calling a function synchronously
            return EcanXcpResult.XCP_ERR_INTERNAL_TIMEOUT;
            #endregion
        }
    }
}
