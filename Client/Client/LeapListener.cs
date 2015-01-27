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

        public delegate void onHandOrientationChange(float pitch, float roll, float yaw);

        private Object thisLock = new Object();
        public onHandOrientationChange OnHandOrientationChange;

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

            this.OnHandOrientationChange(direction.Pitch, direction.Roll, direction.Yaw);
        }
    }

    class Program
    {

        static Object thisLock = new Object();

        static void handle(float pitch, float roll, float yaw)
        {
            lock (thisLock)
            {
                Console.WriteLine("pitch : " + pitch + " roll : " + roll + " yaw : " + yaw);
            }
        }

        static void lol(string[] args)
        {
            // Create a sample listener and controller
            LeapListener listener = new LeapListener();
            Controller controller = new Controller();

            listener.OnHandOrientationChange += new LeapListener.onHandOrientationChange(handle);

            // Have the sample listener receive events from the controller
            controller.AddListener(listener);

            // Keep this process running until Enter is pressed
            Console.WriteLine("Press Enter to quit...");
            Console.ReadLine();

            // Remove the sample listener when done
            controller.RemoveListener(listener);
            controller.Dispose();
        }
    }
}
