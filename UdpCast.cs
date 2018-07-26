using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace HPSocketCS
{
    public class UdpCastEvent
    {
        public delegate HandleResult OnAcceptEventHandler(IntPtr socket, UdpCast sender);

        public delegate HandleResult OnPrepareConnectEventHandler(UdpCast sender, IntPtr socket);

        public delegate HandleResult OnConnectEventHandler(UdpCast sender);

        public delegate HandleResult OnSendEventHandler(UdpCast sender, byte[] bytes);

        public delegate HandleResult OnReceiveEventHandler(UdpCast sender, byte[] bytes);

        public delegate HandleResult OnPointerDataReceiveEventHandler(UdpCast sender, IntPtr pData, int length);

        public delegate HandleResult OnCloseEventHandler(UdpCast sender, SocketOperation enOperation, int errorCode);

        public delegate HandleResult OnHandShakeEventHandler(UdpCast sender);
    }

    public class UdpCast : ConnectionExtra
    {
        protected IntPtr _pCast = IntPtr.Zero;

        protected IntPtr pCast
        {
            get
            {
                return _pCast;
            }

            set
            {
                _pCast = value;
            }
        }

        protected IntPtr pListener = IntPtr.Zero;

        /// <summary>
        /// 服务器ip
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// 本地绑定端口
        /// </summary>
        public string BindAddress { get; set; }

        public int TTL { get; set; }

        public bool ReuseAddr { get; set; }

        public bool IPLoop { get; set; }

        protected bool IsCreate = false;
        private ConnectionExtra ExtraData = new ConnectionExtra();
        /****************************************************/

        /// <summary>
        /// 连接到达事件
        /// </summary>
        public event UdpCastEvent.OnAcceptEventHandler OnAccept;

        /// <summary>
        /// 准备连接了事件
        /// </summary>
        public event UdpCastEvent.OnPrepareConnectEventHandler OnPrepareConnect;

        /// <summary>
        /// 连接事件
        /// </summary>
        public event UdpCastEvent.OnConnectEventHandler OnConnect;

        /// <summary>
        /// 数据发送事件
        /// </summary>
        public event UdpCastEvent.OnSendEventHandler OnSend;

        /// <summary>
        /// 数据到达事件
        /// </summary>
        public event UdpCastEvent.OnReceiveEventHandler OnReceive;

        /// <summary>
        /// 数据到达事件(指针数据)
        /// </summary>
        public event UdpCastEvent.OnPointerDataReceiveEventHandler OnPointerDataReceive;

        /// <summary>
        /// 连接关闭事件
        /// </summary>
        public event UdpCastEvent.OnCloseEventHandler OnClose;

        /// <summary>
        /// 握手事件
        /// </summary>
        public event UdpCastEvent.OnHandShakeEventHandler OnHandShake;

        public UdpCast()
        {
            CreateListener();
        }

        ~UdpCast()
        {
            Destroy();
        }

        /// <summary> 创建socket监听&服务组件 </summary> <returns></returns>
        protected virtual bool CreateListener()
        {
            if (IsCreate == true || pListener != IntPtr.Zero || pCast != IntPtr.Zero)
            {
                return false;
            }

            pListener = Sdk.Create_HP_UdpCastListener();
            if (pListener == IntPtr.Zero)
            {
                return false;
            }

            pCast = Sdk.Create_HP_UdpCast(pListener);
            if (pCast == IntPtr.Zero)
            {
                return false;
            }

            IsCreate = true;

            return true;
        }

        /// <summary>
        /// 终止服务并释放资源
        /// </summary>
        public virtual void Destroy()
        {
            Stop();

            if (pCast != IntPtr.Zero)
            {
                Sdk.Destroy_HP_UdpCast(pCast);
                pCast = IntPtr.Zero;
            }
            if (pListener != IntPtr.Zero)
            {
                Sdk.Destroy_HP_UdpCastListener(pListener);
                pListener = IntPtr.Zero;
            }

            IsCreate = false;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Start()
        {
            if (IsCreate == false)
            {
                return false;
            }
            if (IsStarted == true)
            {
                return false;
            }

            Sdk.HP_UdpCast_SetMultiCastLoop(pCast, this.IPLoop);
            Sdk.HP_UdpCast_SetReuseAddress(pCast, this.ReuseAddr);
            Sdk.HP_UdpCast_SetMultiCastTtl(pCast, this.TTL);

            SetCallback();

            return Sdk.HP_UdpCast_Start(pCast, IpAddress, Port, false, BindAddress);
        }

        /// <summary>
        /// 停止通讯组件
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            if (pCast == IntPtr.Zero)
            {
                return true;
            }

            return Sdk.HP_UdpCast_Stop(pCast);
        }

        #region send

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(byte[] bytes, int size)
        {
            return Sdk.HP_UdpCast_Send(pCast, bytes, size);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(IntPtr bufferPtr, int size)
        {
            return Sdk.HP_UdpCast_Send(pCast, bufferPtr, size);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send<T>(T obj)
        {
            byte[] buffer = StructureToByte<T>(obj);
            return Send(buffer, buffer.Length);
        }

        /// <summary>
        /// 序列化对象后发送数据,序列化对象所属类必须标记[Serializable]
        /// </summary>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool SendBySerializable(object obj)
        {
            byte[] buffer = ObjectToBytes(obj);
            return Send(buffer, buffer.Length);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset">针对bytes的偏移</param>
        /// <param name="size">发多大</param>
        /// <returns></returns>
        public bool Send(byte[] bytes, int offset, int size)
        {
            return Sdk.HP_UdpCast_SendPart(pCast, bytes, size, offset);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bufferPtr"></param>
        /// <param name="offset">针对bufferPtr的偏移</param>
        /// <param name="size">发多大</param>
        /// <returns></returns>
        public bool Send(IntPtr bufferPtr, int offset, int size)
        {
            return Sdk.HP_Client_SendPart(pCast, bufferPtr, size, offset);
        }

        #endregion send

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version
        {
            get
            {
                return Sdk.GetHPSocketVersion();
            }
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                IntPtr ptr = Sdk.HP_UdpCast_GetLastErrorDesc(pCast);
                string desc = Marshal.PtrToStringUni(ptr);
                return desc;
            }
        }

        /// <summary>
        /// 获取错误码
        /// </summary>
        public SocketError ErrorCode
        {
            get
            {
                return Sdk.HP_UdpCast_GetLastError(pCast);
            }
        }

        /*
        #pragma comment(linker, "/EXPORT:HP_UdpCast_GetCastMode=_HP_UdpCast_GetCastMode@4")
        #pragma comment(linker, "/EXPORT:HP_UdpCast_GetMaxDatagramSize=_HP_UdpCast_GetMaxDatagramSize@4")
        #pragma comment(linker, "/EXPORT:HP_UdpCast_GetMultiCastTtl=_HP_UdpCast_GetMultiCastTtl@4")
        #pragma comment(linker, "/EXPORT:HP_UdpCast_GetRemoteAddress=_HP_UdpCast_GetRemoteAddress@16")
        #pragma comment(linker, "/EXPORT:HP_UdpCast_IsMultiCastLoop=_HP_UdpCast_IsMultiCastLoop@4")
        #pragma comment(linker, "/EXPORT:HP_UdpCast_IsReuseAddress=_HP_UdpCast_IsReuseAddress@4")
        #pragma comment(linker, "/EXPORT:HP_UdpCast_SetCastMode=_HP_UdpCast_SetCastMode@8")
        #pragma comment(linker, "/EXPORT:HP_UdpCast_SetMaxDatagramSize=_HP_UdpCast_SetMaxDatagramSize@8")
        #pragma comment(linker, "/EXPORT:HP_UdpCast_SetMultiCastLoop=_HP_UdpCast_SetMultiCastLoop@8")
        #pragma comment(linker, "/EXPORT:HP_UdpCast_SetMultiCastTtl=_HP_UdpCast_SetMultiCastTtl@8")
        #pragma comment(linker, "/EXPORT:HP_UdpCast_SetReuseAddress=_HP_UdpCast_SetReuseAddress@8")
        */
        ///////////////////////////////////////////////////////////////////////////////////////

        // 是否启动
        public bool IsStarted
        {
            get
            {
                if (pCast == IntPtr.Zero)
                {
                    return false;
                }
                return Sdk.HP_UdpCast_HasStarted(pCast);
            }
        }

        /// <summary>
        /// 根据错误码返回错误信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetSocketErrorDesc(SocketError code)
        {
            IntPtr ptr = Sdk.HP_GetSocketErrorDesc(code);
            string desc = Marshal.PtrToStringUni(ptr);
            return desc;
        }

        /// <summary>
        /// 获取某个连接的远程地址信息
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool GetRemoteAddress(IntPtr connId, ref string ip, ref ushort port)
        {
            int ipLength = 40;

            StringBuilder sb = new StringBuilder(ipLength);

            bool ret = Sdk.HP_UdpCast_GetRemoteAddress(pCast, sb, ref ipLength, ref port) && ipLength > 0;
            if (ret == true)
            {
                ip = sb.ToString();
            }

            return ret;
        }

        /// <summary>
        /// 获取该组件对象的连接Id
        /// </summary>
        public IntPtr ConnectionId
        {
            get
            {
                return Sdk.HP_UdpCast_GetConnectionID(pCast);
            }
        }

        /// <summary>
        /// 名称：发送小文件 描述：向指定连接发送 4096 KB 以下的小文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="head">头部附加数据,可以为null</param>
        /// <param name="tail">尾部附加数据,可以为null</param>
        /// <returns>TRUE.成功,FALSE.失败，可通过 SYSGetLastError() 获取 Windows 错误代码</returns>
        public bool SendSmallFile<T1, T2>(string filePath, T1 head, T2 tail)
        {
            byte[] headBuffer = null;
            if (head != null)
            {
                headBuffer = StructureToByte<T1>(head);
            }

            byte[] tailBuffer = null;
            if (tail != null)
            {
                tailBuffer = StructureToByte<T2>(tail);
            }
            return SendSmallFile(filePath, headBuffer, tailBuffer);
        }

        /// <summary>
        /// 设置传播模式（组播或广播）
        /// </summary>
        /// <param name="mode"></param>
        public void SetCastMode(CastMode mode)
        {
            Sdk.HP_UdpCast_SetCastMode(pCast, mode);
        }

        #region callback

        ///////////////////////////////////////////////////////////////////////////////////////

        protected Sdk.OnAccept _OnAccept = null;
        protected Sdk.OnPrepareConnect _OnPrepareConnect = null;
        protected Sdk.OnConnect _OnConnect = null;
        protected Sdk.OnReceive _OnReceive = null;
        protected Sdk.OnSend _OnSend = null;
        protected Sdk.OnClose _OnClose = null;
        protected Sdk.OnHandShake _OnHandShake = null;

        /// <summary>
        /// 设置回调函数
        /// </summary>
        protected virtual void SetCallback()
        {
            // 设置 Socket 监听器回调函数
            _OnAccept = new Sdk.OnAccept(SDK_OnAccept);
            _OnPrepareConnect = new Sdk.OnPrepareConnect(SDK_OnPrepareConnect);
            _OnConnect = new Sdk.OnConnect(SDK_OnConnect);
            _OnSend = new Sdk.OnSend(SDK_OnSend);
            _OnReceive = new Sdk.OnReceive(SDK_OnReceive);
            _OnClose = new Sdk.OnClose(SDK_OnClose);
            _OnHandShake = new Sdk.OnHandShake(SDK_OnHandShake);

            Sdk.HP_Set_FN_Client_OnPrepareConnect(pListener, _OnPrepareConnect);
            Sdk.HP_Set_FN_Client_OnConnect(pListener, _OnConnect);
            Sdk.HP_Set_FN_Client_OnSend(pListener, _OnSend);
            Sdk.HP_Set_FN_Client_OnReceive(pListener, _OnReceive);
            Sdk.HP_Set_FN_Client_OnClose(pListener, _OnClose);
            Sdk.HP_Set_FN_Client_OnHandShake(pListener, _OnHandShake);
        }

        protected HandleResult SDK_OnAccept(IntPtr pSender, IntPtr connId, IntPtr socket)
        {
            if (OnAccept != null)
            {
                return OnAccept(connId, this);
            }

            return HandleResult.Ignore;
        }

        protected HandleResult SDK_OnPrepareConnect(IntPtr pSender, IntPtr connId, IntPtr socket)
        {
            if (OnPrepareConnect != null)
            {
                return OnPrepareConnect(this, socket);
            }
            return HandleResult.Ignore;
        }

        protected HandleResult SDK_OnConnect(IntPtr pSender, IntPtr connId)
        {
            if (OnConnect != null)
            {
                return OnConnect(this);
            }
            return HandleResult.Ignore;
        }

        protected HandleResult SDK_OnSend(IntPtr pSender, IntPtr connId, IntPtr pData, int length)
        {
            if (OnSend != null)
            {
                byte[] bytes = new byte[length];
                Marshal.Copy(pData, bytes, 0, length);
                return OnSend(this, bytes);
            }
            return HandleResult.Ignore;
        }

        protected HandleResult SDK_OnReceive(IntPtr pSender, IntPtr connId, IntPtr pData, int length)
        {
            if (OnPointerDataReceive != null)
            {
                return OnPointerDataReceive(this, pData, length);
            }
            else if (OnReceive != null)
            {
                byte[] bytes = new byte[length];
                Marshal.Copy(pData, bytes, 0, length);
                return OnReceive(this, bytes);
            }
            return HandleResult.Ignore;
        }

        protected HandleResult SDK_OnClose(IntPtr pSender, IntPtr connId, SocketOperation enOperation, int errorCode)
        {
            if (OnClose != null)
            {
                return OnClose(this, enOperation, errorCode);
            }
            return HandleResult.Ignore;
        }

        protected HandleResult SDK_OnHandShake(IntPtr pSender, IntPtr connId)
        {
            if (OnHandShake != null)
            {
                return OnHandShake(this);
            }
            return HandleResult.Ignore;
        }

        ///////////////////////////////////////////////////////////////////////////

        #endregion callback

        #region other

        /// <summary>
        /// 获取系统返回的错误码
        /// </summary>
        public int SYSGetLastError()
        {
            return Sdk.SYS_GetLastError();
        }

        /// <summary>
        /// 调用系统的 ::WSAGetLastError() 方法获取通信错误代码
        /// </summary>
        public int SYSWSAGetLastError()
        {
            return Sdk.SYS_WSAGetLastError();
        }

        /// <summary>
        /// 调用系统的 setsockopt()
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="level"></param>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public int SYS_SetSocketOption(IntPtr sock, int level, int name, IntPtr val, int len)
        {
            return Sdk.SYS_SetSocketOption(sock, level, name, val, len);
        }

        /// <summary>
        /// 调用系统的 getsockopt()
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="level"></param>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public int SYSGetSocketOption(IntPtr sock, int level, int name, IntPtr val, ref int len)
        {
            return Sdk.SYS_GetSocketOption(sock, level, name, val, ref len);
        }

        /// <summary>
        /// 调用系统的 ioctlsocket()
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="cmd"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public int SYSIoctlSocket(IntPtr sock, long cmd, IntPtr arg)
        {
            return Sdk.SYS_IoctlSocket(sock, cmd, arg);
        }

        /// <summary>
        /// 调用系统的 ::WSAIoctl()
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="dwIoControlCode"></param>
        /// <param name="lpvInBuffer"></param>
        /// <param name="cbInBuffer"></param>
        /// <param name="lpvOutBuffer"></param>
        /// <param name="cbOutBuffer"></param>
        /// <param name="lpcbBytesReturned"></param>
        /// <returns></returns>
        public int SYS_WSAIoctl(IntPtr sock, uint dwIoControlCode, IntPtr lpvInBuffer, uint cbInBuffer,
                                              IntPtr lpvOutBuffer, uint cbOutBuffer, uint lpcbBytesReturned)
        {
            return Sdk.SYS_WSAIoctl(sock, dwIoControlCode, lpvInBuffer, cbInBuffer,
                                            lpvOutBuffer, cbOutBuffer, lpcbBytesReturned);
        }

        /// <summary>
        /// 由结构体转换为byte数组
        /// </summary>
        public byte[] StructureToByte<T>(T structure)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            IntPtr bufferIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, bufferIntPtr, true);
                Marshal.Copy(bufferIntPtr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferIntPtr);
            }
            return buffer;
        }

        /// <summary>
        /// 由byte数组转换为结构体
        /// </summary>
        public T ByteToStructure<T>(byte[] dataBuffer)
        {
            object structure = null;
            int size = Marshal.SizeOf(typeof(T));
            IntPtr allocIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(dataBuffer, 0, allocIntPtr, size);
                structure = Marshal.PtrToStructure(allocIntPtr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(allocIntPtr);
            }
            return (T)structure;
        }

        /// <summary>
        /// 对象序列化成byte[]
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public byte[] ObjectToBytes(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.GetBuffer();
            }
        }

        /// <summary>
        /// byte[]序列化成对象
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public object BytesToObject(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                IFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// byte[]转结构体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public T BytesToStruct<T>(byte[] bytes)
        {
            Type strcutType = typeof(T);
            int size = Marshal.SizeOf(strcutType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return (T)Marshal.PtrToStructure(buffer, strcutType);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        #endregion other
    }
}