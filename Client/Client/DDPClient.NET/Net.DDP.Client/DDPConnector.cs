using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.ClientEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket4Net;
using MySurfaceApplication;

namespace Net.DDP.Client
{
    internal class DDPConnector
    {
        private WebSocket _socket;
        private string _url = string.Empty;
        private readonly IClient _client;
        private string _version;
        Logger info = new Logger("DDPConnector.log");

        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        public DDPConnector(IClient client, string version)
        {
            _client = client;
            _version = version;
        }

        public void Connect(string url)
        {
            //_url = string.Format("ws://{0}/websocket", url);
            _url = "ws://" + url + "/websocket";

            _socket = new WebSocket(_url);
            _socket.Opened += _socket_Opened;
            _socket.Error += _socket_Error;
            _socket.Closed += _socket_Closed;
            _socket.MessageReceived += socket_MessageReceived;
            _socket.Open();

            // Wait until meteor server responds
            // Meteor DDP documentation: https://github.com/meteor/meteor/blob/master/packages/livedata/DDP.md
            // The server may send an initial message which is a JSON object lacking a msg key. If so, the client should ignore it.
            // The client does not have to wait for this message. (The message was once used to help implement Meteor's hot code reload
            // feature; it is now only included to force old clients to update).
            _autoResetEvent.WaitOne(5000);
        }

        void _socket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine(e);
        }

        void _socket_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception);
        }

        public void Close()
        {
            _socket.Close();
        }

        public void Send(string message)
        {
            _socket.Send(message);
        }

        void _socket_Opened(object sender, EventArgs e)
        {
            var message = string.Format("{{\"msg\":\"connect\",\"version\":\"{0}\",\"support\":[\"1\",\"pre2\",\"pre1\"]}}", _version);
            info.log(message);

            Send(message);
        }

        void socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            _client.AddItem(e.Message);

            _autoResetEvent.Set();
        }
    }
}
