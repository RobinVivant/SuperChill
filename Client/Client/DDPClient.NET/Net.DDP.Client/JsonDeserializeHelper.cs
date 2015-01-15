using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace Net.DDP.Client
{
    internal class JsonDeserializeHelper
    {
        private readonly IDataSubscriber _subscriber;

        public JsonDeserializeHelper(IDataSubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        internal void Deserialize(string jsonItem)
        {
            JObject jObj = JObject.Parse(jsonItem);
            if (jObj["set"] != null)
            {
                dynamic d = this.GetData(jObj);
                d.type = "sub";
                this._subscriber.DataReceived(d);
            }
            else if (jObj["unset"] != null)
            {
                dynamic entity = new ExpandoObject();
                entity.type = "unset";
                entity.id = jObj["id"].ToString();
                this._subscriber.DataReceived(entity);
            }
            else if (jObj["result"] != null)
            {
                dynamic entity = new ExpandoObject();
                entity.type = "method";
                entity.requestingId = jObj["id"].ToString();
                entity.result = jObj["result"].ToString();
                this._subscriber.DataReceived(entity);
            }
            else if (jObj["msg"] != null)
            {
                var msg = jObj["msg"].ToString();
                switch (msg)
                {
                    case "added":
                        dynamic d = GetData(jObj);
                        d.type = "sub";
                        _subscriber.DataReceived(d);
                        break;
                        break;
                    case "connected":
                        Console.WriteLine();
                        break;
                    default:
                        break;
                }
            }
        }
        private dynamic ParseAdded(JObject json)
        {
            dynamic entity = new ExpandoObject();
            ((IDictionary<string, object>)entity).Add("id", json["id"].ToString());
            entity.collection = json["collection"].ToString();
            JObject tmp = (JObject)json["fields"];
            foreach (var item in tmp)
                ((IDictionary<string, object>)entity).Add(item.Key, item.Value.ToString());

            return entity;
        }

        private dynamic GetData(JObject json)
        {
            dynamic entity = new ExpandoObject();
            ((IDictionary<string, object>)entity).Add("id", json["id"].ToString());
            entity.collection = json["collection"].ToString();
            JObject tmp = (JObject)json["fields"];
            foreach (var item in tmp)
                ((IDictionary<string, object>)entity).Add(item.Key, item.Value.ToString());

            return entity;
        }
    }
}
