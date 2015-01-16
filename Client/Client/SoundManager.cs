using System;
using IrrKlang;

namespace MySurfaceApplication
{
    public class SoundManager
    {
        public const int BPM = 120;

        ISoundEngine engine;

        // <identifier, Loop>
        System.Collections.Hashtable loops;

        // Lists of loop identifiers to delete and deactivate at next step
        System.Collections.ArrayList deletedLoops;
        System.Collections.ArrayList activeLoops;

        System.Timers.Timer timer;

        public SoundManager()
        {
            engine = new ISoundEngine();
            loops = new System.Collections.Hashtable();
            deletedLoops = new System.Collections.ArrayList();
            activeLoops = new System.Collections.ArrayList();

            timer = new System.Timers.Timer(4 * 1000 * BPM / 60);
            timer.Elapsed += endOfLoop;
        }

        public string addLoop(string id, string filePath, bool active)
        {
            loops.Add(id, new Loop(engine, "../.." + filePath, active));
            if (active)
            {
                activeLoops.Add(id);
                if (activeLoops.Count == 1)
                {
                    timer.Stop();
                    endOfLoop();
                }
            }
            return id;
        }

        public void removeLoop(string id)
        {
            deletedLoops.Add(id);
        }



        internal void toggleLoop(string loopId)
        {
            if (activeLoops.Contains(loopId))
            {
                deactivateLoop(loopId);
            }
            else if (loops.ContainsKey(loopId))
            {
                activateLoop(loopId);
            }

            printList(activeLoops);
        }

        void activateLoop(string loopId)
        {
            activeLoops.Add(loopId);
            if (activeLoops.Count == 1)
            {
                timer.Stop();
                endOfLoop();
            }
        }


        public void deactivateLoop(string id)
        {
            activeLoops.Remove(id);
        }







        void endOfLoop(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.Out.WriteLine("loop");
            endOfLoop();
        }

        void endOfLoop()
        {
            timer.Enabled = true;
            foreach (int identifier in deletedLoops)
            {
                loops.Remove(identifier);
                activeLoops.Remove(identifier);
            }
            deletedLoops.Clear();
            foreach (string id in activeLoops)
            {
                Loop l = (Loop)loops[id];
                l.stop();
                l.play();
            }
        }








        private static void printList(System.Collections.ArrayList list)
        {
            string r = "[";
            foreach (Object element in list)
            {
                r += element + ", ";
            }
            Console.Out.WriteLine(r + "]");
        }
    }
}