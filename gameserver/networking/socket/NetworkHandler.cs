﻿using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using log4net;
using LoESoft.Core.config;
using static LoESoft.GameServer.networking.Client;
using LoESoft.GameServer.realm;
using LoESoft.Core.models;

namespace LoESoft.GameServer.networking
{
    internal partial class NetworkHandler : IDisposable
    {
        public int BUFFER_SIZE = 0x20000;
        public int MESSAGE_SIZE = 5;

        private static readonly ILog log = LogManager.GetLogger(typeof(NetworkHandler));

        private readonly RealmManager Manager = GameServer.Manager;
        private readonly Client client;
        private readonly ConcurrentQueue<Message> pending = new ConcurrentQueue<Message>();
        private readonly object sendLock = new object();
        private readonly Socket skt;

        private SocketAsyncEventArgs _outgoing;
        private byte[] _outgoingBuff;
        private OutgoingStage _outgoingState = OutgoingStage.Awaiting;

        private SocketAsyncEventArgs _incoming;
        private byte[] _incomingBuff;
        private IncomingStage _incomingState = IncomingStage.Awaiting;

        public NetworkHandler(Client client, Socket skt)
        {
            this.client = client;
            this.skt = skt;
        }

        public void BeginHandling()
        {
            skt.NoDelay = Settings.NETWORKING.DISABLE_NAGLES_ALGORITHM;
            skt.UseOnlyOverlappedIO = true;

            _outgoing = new SocketAsyncEventArgs();
            _outgoing.Completed += ProcessOutgoingMessage;
            _outgoing.UserToken = new OutgoingToken();
            _outgoingBuff = new byte[BUFFER_SIZE];
            _outgoing.SetBuffer(_outgoingBuff, 0, BUFFER_SIZE);

            _incoming = new SocketAsyncEventArgs();
            _incoming.Completed += ProcessIncomingMessage;
            _incoming.UserToken = new IncomingToken();
            _incomingBuff = new byte[BUFFER_SIZE];
            _incoming.SetBuffer(_incomingBuff, 0, BUFFER_SIZE);

            _incomingState = IncomingStage.ReceivingMessage;

            _incoming.SetBuffer(0, 5);

            if (!skt.ReceiveAsync(_incoming))
                ProcessIncomingMessage(null, _incoming);
        }

        private void OnError(Exception ex)
        {
            Log.Error(ex.ToString());

            Manager.TryDisconnect(client, DisconnectReason.SOCKET_ERROR_DETECTED);
        }

        public void Dispose()
        {
            _outgoing.Completed -= ProcessOutgoingMessage;
            _outgoing.Dispose();
            _outgoingBuff = null;

            _incoming.Completed -= ProcessIncomingMessage;
            _incoming.Dispose();
            _incomingBuff = null;
        }
    }
}