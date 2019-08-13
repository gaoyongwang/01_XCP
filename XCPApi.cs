using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECAN;

namespace ECANXCP
{
    public class XCPApi
    {
        public UInt32 DeviceType;// 设备类型号，USBCAN I 选择3，USBCAN II 选择4
        public UInt32 DeviceIndex;// 设备索引号，当只有一个设备时，索引号为0，有两个时可以为0或1
        public UInt32 CANIndex;// 第几路CAN，即对应卡的CAN通道号，CAN0为0，CAN1为1
        public UInt32 MasterID;// 主设备ID
        public UInt32 SlaveID;// 从设备ID
        INIT_CONFIG init_config = new INIT_CONFIG();// 创建CAN初始化配置结构体
        CAN_OBJ FrameInfo = new CAN_OBJ();// 报文帧
        FILTER_RECORD FilterRecord = new FILTER_RECORD();// 定义CAN滤波器的滤波范围

        UInt32 NumOfFrameTX;// 实际发送帧数量
        UInt32 NumOfFrameRX;// 接收帧数量

        /// <summary>
        /// 定义错误码
        /// </summary>
        public enum TXCPResult : uint
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

        public XCPApi()
        {
            this.DeviceType = 3;
            this.DeviceIndex = 0;
            this.CANIndex = 0;
            this.MasterID = 0x7FB;
            this.SlaveID = 0x7FC;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="Baudrate">波特率</param>
        /// <param name="BoardInfo">设备信息</param>
        /// <returns></returns>
        public bool GcCanInitialize(string Baudrate, out BOARD_INFO BoardInfo)
        {
            BoardInfo.hw_Version = 0x00;
            BoardInfo.fw_Version = 0x00;
            BoardInfo.dr_Version = 0x00;
            BoardInfo.in_Version = 0x00;
            BoardInfo.irq_Num = 0x00;
            BoardInfo.can_Num = 0x00;
            BoardInfo.str_Serial_Num = new byte[] { 0x00 };
            BoardInfo.str_hw_Type = new byte[] { 0x00 };
            BoardInfo.Reserved = new ushort[] { 0x00 };

            // 滤波设置
            init_config.AccCode = ((UInt32)(SlaveID) << 21);
            init_config.AccMask = 0x001FFFFF;
            // 初始化配置滤波使能
            init_config.Filter = 1;

            FilterRecord.ExtFrame = 0;
            FilterRecord.Start = MasterID;
            FilterRecord.End = SlaveID;

            // 初始化配置波特率
            switch (Baudrate)
            {
                case "1000K":

                    init_config.Timing0 = 0;
                    init_config.Timing1 = 0x14;
                    break;
                case "800K":

                    init_config.Timing0 = 0;
                    init_config.Timing1 = 0x16;
                    break;
                case "666K":

                    init_config.Timing0 = 0x80;
                    init_config.Timing1 = 0xb6;
                    break;
                case "500K":

                    init_config.Timing0 = 0;
                    init_config.Timing1 = 0x1c;
                    break;
                case "400K":

                    init_config.Timing0 = 0x80;
                    init_config.Timing1 = 0xfa;
                    break;
                case "250K":

                    init_config.Timing0 = 0x01;
                    init_config.Timing1 = 0x1c;
                    break;
                case "200K":

                    init_config.Timing0 = 0x81;
                    init_config.Timing1 = 0xfa;
                    break;
                case "125K":

                    init_config.Timing0 = 0x03;
                    init_config.Timing1 = 0x1c;
                    break;
                case "100K":

                    init_config.Timing0 = 0x04;
                    init_config.Timing1 = 0x1c;
                    break;
                case "80K":

                    init_config.Timing0 = 0x83;
                    init_config.Timing1 = 0xff;
                    break;
                case "50K":

                    init_config.Timing0 = 0x09;
                    init_config.Timing1 = 0x1c;
                    break;

            }

            // 初始化配置为正常模式
            init_config.Mode = 0;

            // 打开CAN
            if (ECANDLL.OpenDevice(DeviceType, DeviceIndex, CANIndex) == ECAN.ECANStatus.STATUS_OK)
            {
                // 初始化CAN
                if (ECANDLL.InitCAN(DeviceType, DeviceIndex, CANIndex, ref init_config) == ECAN.ECANStatus.STATUS_OK)
                {
                    // 设置CAN 滤波器的滤波范围 实测不起作用
                    if (ECANDLL.SetReference(DeviceType, DeviceIndex, CANIndex, 1, ref FilterRecord) == ECAN.ECANStatus.STATUS_OK)
                    {
                        // 启动USBCAN设备的通道0
                        if (ECANDLL.StartCAN(DeviceType, DeviceIndex, CANIndex) == ECAN.ECANStatus.STATUS_OK)
                        {
                            if (ECANDLL.ReadBoardInfo(DeviceType, DeviceIndex, out BoardInfo) == ECAN.ECANStatus.STATUS_OK)
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
                    ECANDLL.CloseDevice(DeviceType, DeviceIndex);
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
            if (ECANDLL.CloseDevice(DeviceType, DeviceIndex) == ECAN.ECANStatus.STATUS_OK)
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
        /// <param name="Mode">模式</param>
        /// <param name="CtoBuffer">返回数据</param>
        /// <param name="CtoBufferLength">返回数据长度</param>
        /// <returns></returns>
        public TXCPResult XCP_Connect(byte Mode, out byte[] CtoBuffer, UInt16 CtoBufferLength)
        {
            #region Packet Identifier:CMD Command:CONNECT
            // 报文帧ID为
            FrameInfo.ID = MasterID;
            // 发送帧类型 正常发送
            FrameInfo.SendType = 0;
            // 是否为远程帧 0为数据帧
            FrameInfo.RemoteFlag = 0;
            // 是否为扩展帧 0为标准帧，11位帧ID
            FrameInfo.ExternFlag = 0;
            // 数据长度
            FrameInfo.DataLen = 2;
            // CAN报文的数据
            FrameInfo.data = new byte[8];
            // 系统保留
            //CANObj.Reserved = new byte[3];

            FrameInfo.data[0] = 0xFF;
            FrameInfo.data[1] = Mode;

            if ((NumOfFrameTX = ECANDLL.Transmit(DeviceType, DeviceIndex, CANIndex, FrameInfo, 1)) != 1)// 如果发送命令失败
            {
                CtoBuffer = new byte[CtoBufferLength];
                // Function not available / Operation not implemented
                return TXCPResult.XCP_ERR_NOT_IMPLEMENTED;
            }
            #endregion

            #region 读取应答数据
            for (int i = 0; i <= 5; i++)// 尝试读取5次
            {
                if ((NumOfFrameRX = ECANDLL.GetReceiveNum(DeviceType, DeviceIndex, CANIndex)) > 0)// 如果收到数据帧
                {
                    NumOfFrameRX = ECANDLL.Receive(DeviceType, DeviceIndex, CANIndex, out FrameInfo, 1, 10);
                    if (FrameInfo.data[0] == 0xFF)// Packet Identifier:RES
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        // Acknowledge / no error
                        return TXCPResult.XCP_ERR_OK;
                    }
                    else if (FrameInfo.data[0] == 0xFE)// Packet Identifier:ERR
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        #region Error Code
                        switch (FrameInfo.data[1])// 返回 Error Code
                        {
                            case 0:
                                return TXCPResult.XCP_ERR_CMD_SYNCH;
                            case 0x10:
                                return TXCPResult.XCP_ERR_CMD_BUSY;
                            case 0x11:
                                return TXCPResult.XCP_ERR_DAQ_ACTIVE;
                            case 0x12:
                                return TXCPResult.XCP_ERR_PGM_ACTIVE;
                            case 0x20:
                                return TXCPResult.XCP_ERR_CMD_UNKNOWN;
                            case 0x21:
                                return TXCPResult.XCP_ERR_CMD_SYNTAX;
                            case 0x22:
                                return TXCPResult.XCP_ERR_OUT_OF_RANGE;
                            case 0x23:
                                return TXCPResult.XCP_ERR_WRITE_PROTECTED;
                            case 0x24:
                                return TXCPResult.XCP_ERR_ACCESS_DENIED;
                            case 0x25:
                                return TXCPResult.XCP_ERR_ACCESS_DENIED;
                            case 0x26:
                                return TXCPResult.XCP_ERR_PAGE_NOT_VALID;
                            case 0x27:
                                return TXCPResult.XCP_ERR_MODE_NOT_VALID;
                            case 0x28:
                                return TXCPResult.XCP_ERR_SEGMENT_NOT_VALID;
                            case 0x29:
                                return TXCPResult.XCP_ERR_SEQUENCE;
                            case 0x2A:
                                return TXCPResult.XCP_ERR_DAQ_CONFIG;
                            case 0x30:
                                return TXCPResult.XCP_ERR_MEMORY_OVERFLOW;
                            case 0x31:
                                return TXCPResult.XCP_ERR_GENERIC;
                            case 0x32:
                                return TXCPResult.XCP_ERR_VERIFY;
                            case 0x33:
                                return TXCPResult.XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE;
                            default:
                                return TXCPResult.XCP_ERR_INVALID_PARAMETER;
                        }
                        #endregion
                    }
                    else
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        // Invalid parameter value
                        return TXCPResult.XCP_ERR_INVALID_PARAMETER;
                    }
                }
                else//如果没有数据帧，等待1ms
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            // 尝试5次，超时
            CtoBuffer = new byte[CtoBufferLength];
            // A timeout was reached by calling a function synchronously
            return TXCPResult.XCP_ERR_INTERNAL_TIMEOUT;
            #endregion
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="CtoBuffer">返回数据</param>
        /// <param name="CtoBufferLength">返回数据长度</param>
        /// <returns></returns>
        public TXCPResult XCP_Disconnect(out byte[] CtoBuffer, UInt16 CtoBufferLength)
        {
            #region Packet Identifier:CMD Command:DISCONNECT
            // 报文帧ID为
            FrameInfo.ID = MasterID;
            // 发送帧类型 正常发送
            FrameInfo.SendType = 0;
            // 是否为远程帧 0为数据帧
            FrameInfo.RemoteFlag = 0;
            // 是否为扩展帧 0为标准帧，11位帧ID
            FrameInfo.ExternFlag = 0;
            // 数据长度
            FrameInfo.DataLen = 1;
            // CAN报文的数据
            FrameInfo.data = new byte[8];
            // 系统保留
            //CANObj.Reserved = new byte[3];

            FrameInfo.data[0] = 0xFE;

            if ((NumOfFrameTX = ECANDLL.Transmit(DeviceType, DeviceIndex, CANIndex, FrameInfo, 1)) != 1)// 如果发送命令失败
            {
                CtoBuffer = new byte[CtoBufferLength];
                // Function not available / Operation not implemented
                return TXCPResult.XCP_ERR_NOT_IMPLEMENTED;
            }
            #endregion

            #region 读取应答数据
            for (int i = 0; i <= 5; i++)// 尝试读取5次
            {
                if ((NumOfFrameRX = ECANDLL.GetReceiveNum(DeviceType, DeviceIndex, CANIndex)) > 0)// 如果收到数据帧
                {
                    NumOfFrameRX = ECANDLL.Receive(DeviceType, DeviceIndex, CANIndex, out FrameInfo, 1, 10);
                    if (FrameInfo.data[0] == 0xFF)// Packet Identifier:RES
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        // Acknowledge / no error
                        return TXCPResult.XCP_ERR_OK;
                    }
                    else if (FrameInfo.data[0] == 0xFE)// Packet Identifier:ERR
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        #region Error Code
                        switch (FrameInfo.data[1])// 返回 Error Code
                        {
                            case 0:
                                return TXCPResult.XCP_ERR_CMD_SYNCH;
                            case 0x10:
                                return TXCPResult.XCP_ERR_CMD_BUSY;
                            case 0x11:
                                return TXCPResult.XCP_ERR_DAQ_ACTIVE;
                            case 0x12:
                                return TXCPResult.XCP_ERR_PGM_ACTIVE;
                            case 0x20:
                                return TXCPResult.XCP_ERR_CMD_UNKNOWN;
                            case 0x21:
                                return TXCPResult.XCP_ERR_CMD_SYNTAX;
                            case 0x22:
                                return TXCPResult.XCP_ERR_OUT_OF_RANGE;
                            case 0x23:
                                return TXCPResult.XCP_ERR_WRITE_PROTECTED;
                            case 0x24:
                                return TXCPResult.XCP_ERR_ACCESS_DENIED;
                            case 0x25:
                                return TXCPResult.XCP_ERR_ACCESS_DENIED;
                            case 0x26:
                                return TXCPResult.XCP_ERR_PAGE_NOT_VALID;
                            case 0x27:
                                return TXCPResult.XCP_ERR_MODE_NOT_VALID;
                            case 0x28:
                                return TXCPResult.XCP_ERR_SEGMENT_NOT_VALID;
                            case 0x29:
                                return TXCPResult.XCP_ERR_SEQUENCE;
                            case 0x2A:
                                return TXCPResult.XCP_ERR_DAQ_CONFIG;
                            case 0x30:
                                return TXCPResult.XCP_ERR_MEMORY_OVERFLOW;
                            case 0x31:
                                return TXCPResult.XCP_ERR_GENERIC;
                            case 0x32:
                                return TXCPResult.XCP_ERR_VERIFY;
                            case 0x33:
                                return TXCPResult.XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE;
                            default:
                                return TXCPResult.XCP_ERR_INVALID_PARAMETER;
                        }
                        #endregion
                    }
                    else
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        // Invalid parameter value
                        return TXCPResult.XCP_ERR_INVALID_PARAMETER;
                    }
                }
                else//如果没有数据帧，等待1ms
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            // 尝试5次，超时
            CtoBuffer = new byte[CtoBufferLength];
            // A timeout was reached by calling a function synchronously
            return TXCPResult.XCP_ERR_INTERNAL_TIMEOUT;
            #endregion
        }

        /// <summary>
        /// 设置地址
        /// </summary>
        /// <param name="AddrExtension">扩展地址</param>
        /// <param name="Addr">地址</param>
        /// <param name="CtoBuffer">返回数据</param>
        /// <param name="CtoBufferLength">返回数据长度</param>
        /// <returns></returns>
        public TXCPResult XCP_SetMemoryTransferAddress(byte AddrExtension, UInt32 Addr, out byte[] CtoBuffer, UInt16 CtoBufferLength)
        {
            #region Packet Identifier:CMD Command:SET_MTA
            // 报文帧ID为
            FrameInfo.ID = MasterID;
            // 发送帧类型 正常发送
            FrameInfo.SendType = 0;
            // 是否为远程帧 0为数据帧
            FrameInfo.RemoteFlag = 0;
            // 是否为扩展帧 0为标准帧，11位帧ID
            FrameInfo.ExternFlag = 0;
            // 数据长度
            FrameInfo.DataLen = 8;
            // CAN报文的数据
            FrameInfo.data = new byte[8];
            // 系统保留
            //CANObj.Reserved = new byte[3];

            FrameInfo.data[0] = 0xF6;
            FrameInfo.data[3] = AddrExtension;
            Array.Copy(BitConverter.GetBytes(Addr), 0, FrameInfo.data, 4, 4);// 将Addr转换成数组并复制到FrameInfo的后四个元素

            if ((NumOfFrameTX = ECANDLL.Transmit(DeviceType, DeviceIndex, CANIndex, FrameInfo, 1)) != 1)// 如果发送命令失败
            {
                CtoBuffer = new byte[CtoBufferLength];
                // Function not available / Operation not implemented
                return TXCPResult.XCP_ERR_NOT_IMPLEMENTED;
            }
            #endregion

            #region 读取应答数据
            for (int i = 0; i <= 5; i++)// 尝试读取5次
            {
                if ((NumOfFrameRX = ECANDLL.GetReceiveNum(DeviceType, DeviceIndex, CANIndex)) > 0)// 如果收到数据帧
                {
                    NumOfFrameRX = ECANDLL.Receive(DeviceType, DeviceIndex, CANIndex, out FrameInfo, 1, 10);
                    if (FrameInfo.data[0] == 0xFF)// Packet Identifier:RES
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        // Acknowledge / no error
                        return TXCPResult.XCP_ERR_OK;
                    }
                    else if (FrameInfo.data[0] == 0xFE)// Packet Identifier:ERR
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        #region Error Code
                        switch (FrameInfo.data[1])// 返回 Error Code
                        {
                            case 0:
                                return TXCPResult.XCP_ERR_CMD_SYNCH;
                            case 0x10:
                                return TXCPResult.XCP_ERR_CMD_BUSY;
                            case 0x11:
                                return TXCPResult.XCP_ERR_DAQ_ACTIVE;
                            case 0x12:
                                return TXCPResult.XCP_ERR_PGM_ACTIVE;
                            case 0x20:
                                return TXCPResult.XCP_ERR_CMD_UNKNOWN;
                            case 0x21:
                                return TXCPResult.XCP_ERR_CMD_SYNTAX;
                            case 0x22:
                                return TXCPResult.XCP_ERR_OUT_OF_RANGE;
                            case 0x23:
                                return TXCPResult.XCP_ERR_WRITE_PROTECTED;
                            case 0x24:
                                return TXCPResult.XCP_ERR_ACCESS_DENIED;
                            case 0x25:
                                return TXCPResult.XCP_ERR_ACCESS_DENIED;
                            case 0x26:
                                return TXCPResult.XCP_ERR_PAGE_NOT_VALID;
                            case 0x27:
                                return TXCPResult.XCP_ERR_MODE_NOT_VALID;
                            case 0x28:
                                return TXCPResult.XCP_ERR_SEGMENT_NOT_VALID;
                            case 0x29:
                                return TXCPResult.XCP_ERR_SEQUENCE;
                            case 0x2A:
                                return TXCPResult.XCP_ERR_DAQ_CONFIG;
                            case 0x30:
                                return TXCPResult.XCP_ERR_MEMORY_OVERFLOW;
                            case 0x31:
                                return TXCPResult.XCP_ERR_GENERIC;
                            case 0x32:
                                return TXCPResult.XCP_ERR_VERIFY;
                            case 0x33:
                                return TXCPResult.XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE;
                            default:
                                return TXCPResult.XCP_ERR_INVALID_PARAMETER;
                        }
                        #endregion
                    }
                    else
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        // Invalid parameter value
                        return TXCPResult.XCP_ERR_INVALID_PARAMETER;
                    }
                }
                else//如果没有数据帧，等待1ms
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            // 尝试5次，超时
            CtoBuffer = new byte[CtoBufferLength];
            // A timeout was reached by calling a function synchronously
            return TXCPResult.XCP_ERR_INTERNAL_TIMEOUT;
            #endregion
        }

        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="AddrExtension">扩展地址</param>
        /// <param name="Addr">地址</param>
        /// <param name="CtoBuffer">返回数据</param>
        /// <param name="CtoBufferLength">返回数据长度</param>
        /// <returns></returns>
        public TXCPResult XCP_ShortUpload(byte AddrExtension, UInt32 Addr, out byte[] CtoBuffer, UInt16 CtoBufferLength)
        {
            #region Packet Identifier:CMD Command:SHORT_UPLOAD
            // 报文帧ID为
            FrameInfo.ID = MasterID;
            // 发送帧类型 正常发送
            FrameInfo.SendType = 0;
            // 是否为远程帧 0为数据帧
            FrameInfo.RemoteFlag = 0;
            // 是否为扩展帧 0为标准帧，11位帧ID
            FrameInfo.ExternFlag = 0;
            // 数据长度
            FrameInfo.DataLen = 8;
            // CAN报文的数据
            FrameInfo.data = new byte[8];
            // 系统保留
            //CANObj.Reserved = new byte[3];

            FrameInfo.data[0] = 0xF4;
            FrameInfo.data[1] = (byte)BitConverter.GetBytes(Addr).Length;
            FrameInfo.data[3] = AddrExtension;
            Array.Copy(BitConverter.GetBytes(Addr), 0, FrameInfo.data, 4, 4);// 将Addr转换成数组并复制到FrameInfo的后四个元素

            if ((NumOfFrameTX = ECANDLL.Transmit(DeviceType, DeviceIndex, CANIndex, FrameInfo, 1)) != 1)// 如果发送命令失败
            {
                CtoBuffer = new byte[CtoBufferLength];
                // Function not available / Operation not implemented
                return TXCPResult.XCP_ERR_NOT_IMPLEMENTED;
            }
            #endregion

            #region 读取应答数据
            for (int i = 0; i <= 5; i++)// 尝试读取5次
            {
                if ((NumOfFrameRX = ECANDLL.GetReceiveNum(DeviceType, DeviceIndex, CANIndex)) > 0)// 如果收到数据帧
                {
                    NumOfFrameRX = ECANDLL.Receive(DeviceType, DeviceIndex, CANIndex, out FrameInfo, 1, 10);
                    if (FrameInfo.data[0] == 0xFF)// Packet Identifier:RES
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        // Acknowledge / no error
                        return TXCPResult.XCP_ERR_OK;
                    }
                    else if (FrameInfo.data[0] == 0xFE)// Packet Identifier:ERR
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        #region Error Code
                        switch (FrameInfo.data[1])// 返回 Error Code
                        {
                            case 0:
                                return TXCPResult.XCP_ERR_CMD_SYNCH;
                            case 0x10:
                                return TXCPResult.XCP_ERR_CMD_BUSY;
                            case 0x11:
                                return TXCPResult.XCP_ERR_DAQ_ACTIVE;
                            case 0x12:
                                return TXCPResult.XCP_ERR_PGM_ACTIVE;
                            case 0x20:
                                return TXCPResult.XCP_ERR_CMD_UNKNOWN;
                            case 0x21:
                                return TXCPResult.XCP_ERR_CMD_SYNTAX;
                            case 0x22:
                                return TXCPResult.XCP_ERR_OUT_OF_RANGE;
                            case 0x23:
                                return TXCPResult.XCP_ERR_WRITE_PROTECTED;
                            case 0x24:
                                return TXCPResult.XCP_ERR_ACCESS_DENIED;
                            case 0x25:
                                return TXCPResult.XCP_ERR_ACCESS_DENIED;
                            case 0x26:
                                return TXCPResult.XCP_ERR_PAGE_NOT_VALID;
                            case 0x27:
                                return TXCPResult.XCP_ERR_MODE_NOT_VALID;
                            case 0x28:
                                return TXCPResult.XCP_ERR_SEGMENT_NOT_VALID;
                            case 0x29:
                                return TXCPResult.XCP_ERR_SEQUENCE;
                            case 0x2A:
                                return TXCPResult.XCP_ERR_DAQ_CONFIG;
                            case 0x30:
                                return TXCPResult.XCP_ERR_MEMORY_OVERFLOW;
                            case 0x31:
                                return TXCPResult.XCP_ERR_GENERIC;
                            case 0x32:
                                return TXCPResult.XCP_ERR_VERIFY;
                            case 0x33:
                                return TXCPResult.XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE;
                            default:
                                return TXCPResult.XCP_ERR_INVALID_PARAMETER;
                        }
                        #endregion
                    }
                    else
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        // Invalid parameter value
                        return TXCPResult.XCP_ERR_INVALID_PARAMETER;
                    }
                }
                else//如果没有数据帧，等待1ms
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            // 尝试5次，超时
            CtoBuffer = new byte[CtoBufferLength];
            // A timeout was reached by calling a function synchronously
            return TXCPResult.XCP_ERR_INTERNAL_TIMEOUT;
            #endregion
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="Data">下载的数据</param>
        /// <param name="CtoBuffer">返回数据</param>
        /// <param name="CtoBufferLength">返回数据长度</param>
        /// <returns></returns>
        public TXCPResult XCP_Download(byte[] Data, out byte[] CtoBuffer, UInt16 CtoBufferLength)
        {
            #region Packet Identifier:CMD Command:DOWNLOAD
            // 报文帧ID为
            FrameInfo.ID = MasterID;
            // 发送帧类型 正常发送
            FrameInfo.SendType = 0;
            // 是否为远程帧 0为数据帧
            FrameInfo.RemoteFlag = 0;
            // 是否为扩展帧 0为标准帧，11位帧ID
            FrameInfo.ExternFlag = 0;
            // 数据长度
            FrameInfo.DataLen = (byte)(Data.Length + 2);
            // CAN报文的数据
            FrameInfo.data = new byte[8];
            // 系统保留
            //CANObj.Reserved = new byte[3];

            FrameInfo.data[0] = 0xF0;
            FrameInfo.data[1] = (byte)Data.Length;
            Array.Copy(Data, 0, FrameInfo.data, 2, Data.Length);// 将Data复制到FrameInfo

            if ((NumOfFrameTX = ECANDLL.Transmit(DeviceType, DeviceIndex, CANIndex, FrameInfo, 1)) != 1)// 如果发送命令失败
            {
                CtoBuffer = new byte[CtoBufferLength];
                // Function not available / Operation not implemented
                return TXCPResult.XCP_ERR_NOT_IMPLEMENTED;
            }
            #endregion

            #region 读取应答数据
            for (int i = 0; i <= 5; i++)// 尝试读取5次
            {
                if ((NumOfFrameRX = ECANDLL.GetReceiveNum(DeviceType, DeviceIndex, CANIndex)) > 0)// 如果收到数据帧
                {
                    NumOfFrameRX = ECANDLL.Receive(DeviceType, DeviceIndex, CANIndex, out FrameInfo, 1, 10);
                    if (FrameInfo.data[0] == 0xFF)// Packet Identifier:RES
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        // Acknowledge / no error
                        return TXCPResult.XCP_ERR_OK;
                    }
                    else if (FrameInfo.data[0] == 0xFE)// Packet Identifier:ERR
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        #region Error Code
                        switch (FrameInfo.data[1])// 返回 Error Code
                        {
                            case 0:
                                return TXCPResult.XCP_ERR_CMD_SYNCH;
                            case 0x10:
                                return TXCPResult.XCP_ERR_CMD_BUSY;
                            case 0x11:
                                return TXCPResult.XCP_ERR_DAQ_ACTIVE;
                            case 0x12:
                                return TXCPResult.XCP_ERR_PGM_ACTIVE;
                            case 0x20:
                                return TXCPResult.XCP_ERR_CMD_UNKNOWN;
                            case 0x21:
                                return TXCPResult.XCP_ERR_CMD_SYNTAX;
                            case 0x22:
                                return TXCPResult.XCP_ERR_OUT_OF_RANGE;
                            case 0x23:
                                return TXCPResult.XCP_ERR_WRITE_PROTECTED;
                            case 0x24:
                                return TXCPResult.XCP_ERR_ACCESS_DENIED;
                            case 0x25:
                                return TXCPResult.XCP_ERR_ACCESS_DENIED;
                            case 0x26:
                                return TXCPResult.XCP_ERR_PAGE_NOT_VALID;
                            case 0x27:
                                return TXCPResult.XCP_ERR_MODE_NOT_VALID;
                            case 0x28:
                                return TXCPResult.XCP_ERR_SEGMENT_NOT_VALID;
                            case 0x29:
                                return TXCPResult.XCP_ERR_SEQUENCE;
                            case 0x2A:
                                return TXCPResult.XCP_ERR_DAQ_CONFIG;
                            case 0x30:
                                return TXCPResult.XCP_ERR_MEMORY_OVERFLOW;
                            case 0x31:
                                return TXCPResult.XCP_ERR_GENERIC;
                            case 0x32:
                                return TXCPResult.XCP_ERR_VERIFY;
                            case 0x33:
                                return TXCPResult.XCP_ERR_RESOURCE_TEMPORARY_NOT_ACCESSIBLE;
                            default:
                                return TXCPResult.XCP_ERR_INVALID_PARAMETER;
                        }
                        #endregion
                    }
                    else
                    {
                        CtoBuffer = FrameInfo.data;// 回传接收到的数据
                        // Invalid parameter value
                        return TXCPResult.XCP_ERR_INVALID_PARAMETER;
                    }
                }
                else//如果没有数据帧，等待1ms
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            // 尝试5次，超时
            CtoBuffer = new byte[CtoBufferLength];
            // A timeout was reached by calling a function synchronously
            return TXCPResult.XCP_ERR_INTERNAL_TIMEOUT;
            #endregion
        }
    }
}
