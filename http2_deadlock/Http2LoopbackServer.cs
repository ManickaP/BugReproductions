// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace http2_deadlock
{
    public class Http2LoopbackServer : IDisposable
    {
        private IPAddress _address;
        private Socket _listenSocket;
        private Uri _uri;

        public IPAddress Address => _address;
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        public Http2LoopbackServer(IPAddress address = null)
        {
            _address ??= IPAddress.Loopback;
            _listenSocket = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(new IPEndPoint(_address, 5001));
            _listenSocket.Listen(1);
        }

        public async Task<Http2LoopbackConnection> AcceptConnectionAsync()
        {
            Socket connectionSocket = await _listenSocket.AcceptAsync().ConfigureAwait(false);

            var stream = new NetworkStream(connectionSocket, ownsSocket: true);
            return await Http2LoopbackConnection.CreateAsync(connectionSocket, stream).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (_listenSocket != null)
            {
                _listenSocket.Dispose();
                _listenSocket = null;
            }
        }
    }

    public enum ProtocolErrors
    {
        NO_ERROR = 0x0,
        PROTOCOL_ERROR = 0x1,
        INTERNAL_ERROR = 0x2,
        FLOW_CONTROL_ERROR = 0x3,
        SETTINGS_TIMEOUT = 0x4,
        STREAM_CLOSED = 0x5,
        FRAME_SIZE_ERROR = 0x6,
        REFUSED_STREAM = 0x7,
        CANCEL = 0x8,
        COMPRESSION_ERROR = 0x9,
        CONNECT_ERROR = 0xa,
        ENHANCE_YOUR_CALM = 0xb,
        INADEQUATE_SECURITY = 0xc,
        HTTP_1_1_REQUIRED = 0xd
    }
}
