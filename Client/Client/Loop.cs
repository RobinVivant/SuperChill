using IrrKlang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySurfaceApplication
{
    class Loop
    {
        ISoundEngine engine;
        ISound sound;
        private string filePath;

        public Loop(ISoundEngine _engine, string _filePath, bool active)
        {
            engine = _engine;
            filePath = _filePath;
        }

        internal void play()
        {
            sound.Paused = false;
        }

        internal void stop()
        {
            ISound l = engine.Play2D(filePath, false, true);
            if (l == null)
            {
                throw new Exception("Could not load " + filePath);
            }
            if (sound != null)
            {
                sound.Dispose();
            }
            sound = l;
            sound.Paused = true;
            sound.PlayPosition = 0;
        }
    }
}
