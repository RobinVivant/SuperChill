﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MySurfaceApplication;

namespace Net.DDP.Client
{
    public class DDPClient : IClient
    {
        Logger info = new Logger("DDPClient.log");
        private DDPConnector _connector;
        private int _uniqueId;
        private ResultQueue _queueHandler;

        public DDPClient(IDataSubscriber subscriber)
        {
            this._connector = new DDPConnector(this, "1");
            this._queueHandler = new ResultQueue(subscriber);
            _uniqueId = 1;
        }

        public void AddItem(string jsonItem)
        {
            _queueHandler.AddItem(jsonItem);
        }

        public void Connect(string url)
        {
            _connector.Connect(url);
        }

        public void Pong()
        {
            string message = string.Format("\"msg\": \"pong\"");
            message = "{" + message + "}";
            _connector.Send(message);
        }

        public void Update(string methodName, params string[] args)
        {
            string message = string.Format("\"msg\": \"method\",\"method\": \"{0}\",\"params\": [{1}],\"id\": \"{2}\"", methodName, this.CreateSpecialJSonArray(args), this.NextId().ToString());
            message = "{" + message + "}";
            info.log(message);
            _connector.Send(message);
        }

        public void Call(string methodName, params string[] args)
        {
            string message = string.Format("\"msg\": \"method\",\"method\": \"{0}\",\"params\": [{1}],\"id\": \"{2}\"", methodName, this.CreateJSonArray(args), this.NextId().ToString());
            message = "{" + message + "}";
            info.log(message);
            _connector.Send(message);
        }

        public int Subscribe(string subscribeTo, params string[] args)
        {
            string message = string.Format("\"msg\": \"sub\",\"name\": \"{0}\",\"params\": [{1}],\"id\": \"{2}\"", subscribeTo, CreateJSonArray(args), NextId());
            message = "{" + message + "}";
            info.log(message);
            _connector.Send(message);            

            return GetCurrentRequestId();
        }

        private string CreateJSonArray(params string[] args)
        {
            if (args == null)
                return string.Empty;

            StringBuilder argumentBuilder = new StringBuilder();
            string delimiter = string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                argumentBuilder.Append(delimiter);
                argumentBuilder.Append(string.Format("\"{0}\"", args[i]));
                delimiter = ",";
            }

            return argumentBuilder.ToString();
        }

        private string CreateSpecialJSonArray(params string[] args)
        {
            if (args == null)
                return string.Empty;

            StringBuilder argumentBuilder = new StringBuilder();
            string delimiter = string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                argumentBuilder.Append(delimiter);
                argumentBuilder.Append(string.Format("{0}", args[i]));
                delimiter = ",";
            }

            return argumentBuilder.ToString();
        }
        private int NextId()
        {
            return _uniqueId++;
        }

        public int GetCurrentRequestId()
        {
            return _uniqueId;
        }

    }
}
