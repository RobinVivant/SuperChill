using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp._01.HelloWorld
{
    class LeapListener : Listener
    {

        public delegate void onHandChange(float pitch, float yaw, float yPos);

        private Object thisLock = new Object();
        public onHandChange OnHandSpatialPropertiesChange;

        private void SafeWriteLine(String line)
        {
            lock (thisLock)
            {
                Console.WriteLine(line);
            }
        }

        public override void OnInit(Controller controller)
        {
            SafeWriteLine("Initialized");
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");

        }

        public override void OnDisconnect(Controller controller)
        {
            //Note: not dispatched when running in a debugger.
            SafeWriteLine("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");
        }

        public override void OnFrame(Controller controller)
        {
            Leap.Frame frame = controller.Frame();

            if (frame.Hands.Count == 0)
                return;

            Leap.Vector direction = frame.Hands.First().Direction;
            Leap.Vector position = frame.Hands.First().PalmPosition;

            this.OnHandSpatialPropertiesChange(direction.Pitch, direction.Yaw, position.y);
        }
    }
}
