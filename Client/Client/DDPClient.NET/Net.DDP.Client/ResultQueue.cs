using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Net.DDP.Client
{
    internal class ResultQueue
    {
        private static ManualResetEvent _enqueuedEvent;
        private static Thread _workerThread;
        private readonly Queue<string> _jsonItemsQueue;
        private string _currentJsongItem;
        //private readonly JsonDeserializeHelper _serializeHelper;
        private IDataSubscriber _subscriber;

        public ResultQueue(IDataSubscriber subscriber)
        {
            _subscriber = subscriber;

            _jsonItemsQueue = new Queue<string>();
            //_serializeHelper = new JsonDeserializeHelper(subscriber);

            _enqueuedEvent = new ManualResetEvent(false);
            _workerThread = new Thread(PerformDeserilization);
            _workerThread.Start();
        }

        public void AddItem(string jsonItem)
        {
            lock (_jsonItemsQueue)
            {
                _jsonItemsQueue.Enqueue(jsonItem);
                _enqueuedEvent.Set();
            }
            RestartThread();
        }


        private bool Dequeue()
        {
            lock (_jsonItemsQueue)
            {
                if (_jsonItemsQueue.Count > 0)
                {
                    _enqueuedEvent.Reset();
                    _currentJsongItem = _jsonItemsQueue.Dequeue();
                }
                else
                {
                    return false;
                }

                return true;
            }
        }

        public void RestartThread()
        {
            if (_workerThread.ThreadState == ThreadState.Stopped)
            {
                _workerThread.Abort();
                _workerThread = new Thread(PerformDeserilization);
                _workerThread.Start();
            }
        }

        private void PerformDeserilization()
        {
            while (Dequeue())
            {
                _subscriber.DataReceived(_currentJsongItem);
            }
        }
    }
}
