﻿using System;

namespace HPSocketCS
{
    public class TcpPullServer : TcpServer
    {
        private new Sdk.OnPullReceive _OnReceive = null;

        /// <summary>
        /// 数据到达事件
        /// </summary>
        public new event TcpPullServerEvent.OnReceiveEventHandler OnReceive;

        /// <summary>
        /// 终止服务并释放资源
        /// </summary>
        public override void Destroy()
        {
            Stop();

            if (pServer != IntPtr.Zero)
            {
                Sdk.Destroy_HP_TcpPullServer(pServer);
                pServer = IntPtr.Zero;
            }
            if (pListener != IntPtr.Zero)
            {
                Sdk.Destroy_HP_TcpPullServerListener(pListener);
                pListener = IntPtr.Zero;
            }
            IsCreate = false;
        }

        /// <summary>
        /// 抓取数据 用户通过该方法从 Socket 组件中抓取数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="pBuffer"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public FetchResult Fetch(IntPtr connId, IntPtr pBuffer, int size)
        {
            return Sdk.HP_TcpPullServer_Fetch(pServer, connId, pBuffer, size);
        }

        /// <summary>
        /// 抓取数据 用户通过该方法从 Socket 组件中抓取数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="pBuffer"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public FetchResult Peek(IntPtr connId, IntPtr pBuffer, int size)
        {
            return Sdk.HP_TcpPullServer_Peek(pServer, connId, pBuffer, size);
        }

        /// <summary> 创建socket监听&服务组件 </summary> <returns></returns>
        protected override bool CreateListener()
        {
            if (IsCreate == true || pListener != IntPtr.Zero || pServer != IntPtr.Zero)
            {
                return false;
            }

            pListener = Sdk.Create_HP_TcpPullServerListener();
            if (pListener == IntPtr.Zero)
            {
                return false;
            }

            pServer = Sdk.Create_HP_TcpPullServer(pListener);
            if (pServer == IntPtr.Zero)
            {
                return false;
            }

            IsCreate = true;

            return true;
        }

        protected HandleResult SDK_OnReceive(IntPtr pSender, IntPtr connId, int length)
        {
            if (OnReceive != null)
            {
                return OnReceive(connId, length);
            }
            return HandleResult.Ignore;
        }

        protected override void SetCallback()
        {
            _OnReceive = new Sdk.OnPullReceive(SDK_OnReceive);
            Sdk.HP_Set_FN_Server_OnPullReceive(pListener, _OnReceive);
            base.SetCallback();
        }
    }

    public class TcpPullServerEvent
    {
        public delegate HandleResult OnReceiveEventHandler(IntPtr connId, int length);
    }
}