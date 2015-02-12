using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySurfaceApplication
{
    class LeapListener : Listener
    {

        public delegate void onHandVariation(float dPitch, float dX, float dY);
        public delegate void onHandLeaving(bool isPresent);

        private Object thisLock = new Object();
        private bool resetPosition = true;
        private float prevPitch, prevX, prevY;
        public onHandVariation OnHandVariation;
        public onHandLeaving OnHandLeaving;

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

            if (frame.Hands.Count != 1)
            {
                resetPosition = true;
                this.OnHandLeaving(resetPosition);
                return;
            }

            Leap.Hand h = frame.Hands.First();

            Leap.Vector direction = h.Direction;
            Leap.Vector position = h.PalmPosition;

            

            if (resetPosition)
            {
                resetPosition = false;
                this.OnHandLeaving(resetPosition);
            }
            else
            {
                this.OnHandVariation(direction.Pitch - prevPitch, position.x - prevX, position.y - prevY);
            }
            prevPitch = direction.Pitch;
            prevX = position.x;
            prevY = position.y;
        }
    }
}
