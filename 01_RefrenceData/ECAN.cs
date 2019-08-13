using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ECAN
{
    /// <summary>
    /// CAN操作结果
    /// </summary>
    [Flags]
    public enum ECANStatus : uint
    {
        STATUS_ERR = 0x00000,
        STATUS_OK = 0x00001,
    }

    /// <summary>
    /// ECAN 系列接口卡的设备信息。结构体将在ReadBoardInfo函数中被填充
    /// </summary>
    public struct BOARD_INFO
    {
        public ushort hw_Version;       // 硬件版本号，用16进制表示
        public ushort fw_Version;       // 固件版本号，用16进制表示
        public ushort dr_Version;       // 驱动程序版本号，用16进制表示
        public ushort in_Version;       // 接口库版本号，用16进制表示
        public ushort irq_Num;          // 板卡所使用的中断号
        public byte can_Num;            // 表示有几路CAN通道
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] str_Serial_Num;   // 此板卡的序列号
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] str_hw_Type;      // 硬件类型
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] Reserved;       // 系统保留
    }

    /// <summary>
    /// 报文帧的数据结构，在发送函数Transmit和接收函数Receive中被用来传送CAN信息帧
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CAN_OBJ
    {
        public uint ID;             // 报文帧ID
        public uint TimeStamp;      // 接收到信息帧时的时间标识，从CAN控制器初始化开始计时，单位微秒
        public byte TimeFlag;       // 是否使用时间标识，为1时TimeStamp有效，TimeFlag和TimeStamp只在此帧为接收帧时有意义
        public byte SendType;       // 发送帧类型。0为正常发送，1为单次发送（不自动重发），2为自发自收（用于测试CAN卡是否损坏），
                                    // 3为单次自发自收（只发送一次，用于自测试），只在此帧为发送帧时有意义
        public byte RemoteFlag;     // 是否是远程帧。0为数据帧，1为远程帧
        public byte ExternFlag;     // 是否是扩展帧。0为标准帧（11位帧ID），1为扩展帧（29位帧ID）
        public byte DataLen;        // 数据长度DLC(<=8)，即Data的长度
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] data;         // CAN报文的数据。空间受DataLen的约束
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Reserved;     // 系统保留
    }

    /// <summary>
    /// CAN控制器状态信息。结构体将在ReadCanStatus函数中被填充。
    /// </summary>
    public struct CAN_STATUS
    {
        public byte ErrInterrupt;   // 中断记录，读操作会清除
        public byte regMode;        // CAN控制器模式寄存器
        public byte regStatus;      // CAN控制器状态寄存器
        public byte regALCapture;   // CAN控制器仲裁丢失寄存器
        public byte regECCapture;   // CAN控制器错误寄存器
        public byte EWLimit;        // CAN控制器错误警告限制寄存器
        public byte RECounter;      // CAN控制器接收错误寄存器
        public byte TECounter;      // CAN控制器发送错误寄存器
        public uint Reserved;       // 系统保留
    }

    /// <summary>
    /// 用于装载VCI库运行时产生的错误信息。结构体将在ReadErrInfo函数中被填充
    /// </summary>
    public struct ERR_INFO
    {
        public uint ErrCode;            // 错误码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Passive_ErrData;  // 当产生的错误中有消极错误时表示为消极错误的错误标识数据
        public byte ArLost_ErrData;     // 当产生的错误中有仲裁丢失错误时表示为仲裁丢失错误的错误标识数据
    }

    /// <summary>
    /// 初始化CAN的配置。将在InitCan函数中被填充
    /// </summary>
    public struct INIT_CONFIG
    {

        public uint AccCode;    // 验收码。SJA1000的帧过滤验收码
        public uint AccMask;    // 屏蔽码。SJA1000的帧过滤屏蔽码。屏蔽码推荐设置为0xFFFF FFFF，即全部接收
        public uint Reserved;   // 保留
        public byte Filter;     // 滤波使能。0为不使能，1为使能。使能时，请参照SJA1000验收滤波器设置验收码和屏蔽码
        public byte Timing0;    // 波特率定时器0（BTR0）
        public byte Timing1;    // 波特率定时器1（BTR1）
        public byte Mode;       // 模式。0为正常模式，1为只听模式，2为自发自收模式
    }

    /// <summary>
    /// 定义了CAN 滤波器的滤波范围。结构体将在SetReference函数中被填充。
    /// </summary>
    public struct FILTER_RECORD
    {
        public uint ExtFrame;   // 过滤的帧类型标志，为1代表要过滤的为扩展帧，为0代表要过滤的为标准帧
        public uint Start;      // 滤波范围的起始帧ID
        public uint End;        // 滤波范围的结束帧ID
    }

    /// <summary>
    /// CAN设备操作相关函数
    /// </summary>
    public static class ECANDLL
    {
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="DeviceType">设备类型号，USBCAN I 选择3，USBCAN II 选择4</param>
        /// <param name="DeviceIndex">设备索引号，当只有一个设备时，索引号为0，有两个时可以为0或1</param>
        /// <returns>返回1表示操作成功，0表示操作失败</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "OpenDevice")]
        public static extern ECANStatus OpenDevice(
            UInt32 DeviceType,
            UInt32 DeviceIndex,
            UInt32 Reserved);

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <returns>返回1表示操作成功，0表示操作失败</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "CloseDevice")]
        public static extern ECANStatus CloseDevice(
            UInt32 DeviceType, 
            UInt32 DeviceIndex);

        /// <summary>
        /// 初始化指定的CAN通道
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="CANIndex">第几路CAN，即对应卡的CAN通道号，CAN0为0，CAN1为1</param>
        /// <param name="InitConfig">初始化参数结构体</param>
        /// <returns>返回1表示操作成功，0表示操作失败</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "InitCAN")]
        public static extern ECANStatus InitCAN(
            UInt32 DeviceType, 
            UInt32 DeviceIndex,
            UInt32 CANIndex,
            ref INIT_CONFIG InitConfig);

        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="BoardInfo">用来存储设备信息的BOARD_INFO结构指针</param>
        /// <returns>返回1表示操作成功，0表示操作失败</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "ReadBoardInfo")]
        public static extern ECANStatus ReadBoardInfo(
            UInt32 DeviceType,
            UInt32 DeviceIndex,
            out BOARD_INFO BoardInfo);

        /// <summary>
        /// 获取USBCAN分析仪最后一次错误信息
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="CANIndex">CAN通道号</param>
        /// <param name="ErrorInfo">用来存储错误信息的ERR_INFO结构指针</param>
        /// <returns>返回1表示操作成功，0表示操作失败</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "ReadErrInfo")]
        public static extern ECANStatus ReadErrInfo(
            UInt32 DeviceType,
            UInt32 DeviceIndex,
            UInt32 CANIndex,
            out ERR_INFO ErrorInfo);

        /// <summary>
        /// 获取CAN状态
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="CANIndex">CAN通道号</param>
        /// <param name="CANStatus">用来存储CAN状态的CAN_STATUS结构指针</param>
        /// <returns>返回1表示操作成功，0表示操作失败</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "ReadCanStatus")]
        public static extern ECANStatus ReadCanStatus(
            UInt32 DeviceType,
            UInt32 DeviceIndex,
            UInt32 CANIndex,
            out CAN_STATUS CANStatus);

        //[DllImport("ECANVCI64.dll", EntryPoint = "GetReference")]
        //public static extern ECANStatus GetReference(
        //    UInt32 DeviceType,
        //    UInt32 DeviceIndex,
        //    UInt32 CANIndex,
        //    UInt32 RefType);

        /// <summary>
        /// 此函数用以设置设备的相应参数，主要处理不同设备的特定操作
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="CANIndex">CAN通道号</param>
        /// <param name="RefType">参数类型</param>
        /// <param name="FilterRecord">用来存储参数有关数据缓冲区地址首指针</param>
        /// <returns></returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "SetReference")]
        public static extern ECANStatus SetReference(
            UInt32 DeviceType,
            UInt32 DeviceIndex,
            UInt32 CANIndex,
            UInt32 RefType,
            ref FILTER_RECORD FilterRecord);

        /// <summary>
        /// 用以获取指定接收缓冲区中接收到但尚未被读取的帧数量
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="CANIndex">CAN通道号</param>
        /// <returns>返回尚未被读取的帧数</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "GetReceiveNum")]
        public static extern UInt32 GetReceiveNum(
            UInt32 DeviceType,
            UInt32 DeviceIndex,
            UInt32 CANIndex);

        /// <summary>
        /// 用以清空指定CAN通道的缓冲区
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="CANIndex">CAN通道号</param>
        /// <returns>返回1表示操作成功，0表示操作失败</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "ClearBuffer")]
        public static extern ECANStatus ClearBuffer(
            UInt32 DeviceType,
            UInt32 DeviceIndex,
            UInt32 CANIndex);

        /// <summary>
        /// 用以启动USBCAN设备的某一个CAN通道
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="CANIndex">CAN通道号</param>
        /// <returns>返回1表示操作成功，0表示操作失败</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "StartCAN")]
        public static extern ECANStatus StartCAN(
            UInt32 DeviceType, 
            UInt32 DeviceIndex,
            UInt32 CANIndex);

        /// <summary>
        /// 用以复位CAN。如当USBCAN分析仪进入总线关闭状态时，可以调用这个函数
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="CANIndex">CAN通道号</param>
        /// <returns>返回1表示操作成功，0表示操作失败</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "ResetCAN")]
        public static extern ECANStatus ResetCAN(
            UInt32 DeviceType, 
            UInt32 DeviceIndex,
            UInt32 CANIndex);

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="CANIndex">CAN通道号</param>
        /// <param name="Send">要发送的数据帧数组的首指针</param>
        /// <param name="Length">要发送的数据帧数组的长度</param>
        /// <returns>返回实际发送的帧数</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "Transmit")]
        public static extern UInt32 Transmit(
            UInt32 DeviceType,
            UInt32 DeviceIndex,
            UInt32 CANIndex,
            CAN_OBJ Send,
            UInt16 Length);

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="DeviceType">设备类型号</param>
        /// <param name="DeviceIndex">设备索引号</param>
        /// <param name="CANIndex">CAN通道号</param>
        /// <param name="Receive">用来接收的数据帧数组的首指针</param>
        /// <param name="Length">用来接收的数据帧数组的长度</param>
        /// <param name="WaitTime">等待超时时间，以毫秒为单位</param>
        /// <returns>返回实际读取到的帧数。如果返回值为0xFFFFFFFF，则表示读取数据失败</returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "Receive")]
        public static extern UInt32 Receive(
            UInt32 DeviceType,
            UInt32 DeviceIndex,
            UInt32 CANIndex,
            out CAN_OBJ Receive,
            UInt32 Length,
            UInt32 WaitTime);
    }
}
