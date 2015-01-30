using System;
using IrrKlang;
using CSharp._01.HelloWorld;
using MySurfaceApplication;


public enum SoundEffect { Volume, Echo, WavesReverb, Gargle, Flanger, Chorus };

public class SoundManager
{
    public const int BPM = 120;

    ISoundEngine engine;

    // <identifier, Loop>
    System.Collections.Hashtable loops;
    System.Collections.Hashtable loopStates;

    // Lists of loop identifiers to delete and deactivate at next step
    System.Collections.ArrayList deletedLoops;

    int activeCount;
    System.Timers.Timer timer;

    public SoundManager()
    {
        engine = new ISoundEngine();
        loops = new System.Collections.Hashtable();
        loopStates = new System.Collections.Hashtable();
        deletedLoops = new System.Collections.ArrayList();

        activeCount = 0;

        timer = new System.Timers.Timer(4 * 1000 * BPM / 60);
        timer.Elapsed += endOfLoop;
    }

    public void addLoop(string id, string filePath, bool active)
    {
        loops.Add(id, new Loop(engine, filePath));
        loopStates.Add(id, active);
        if (activeCount == 0)
        {
            endOfLoop();
        }
    }

    public void removeLoop(string id)
    {
        deletedLoops.Add(id);
    }



    internal void toggleLoop(string loopId)
    {
        loopStates[loopId] = !(bool)loopStates[loopId];
    }



    public void setEffectOnLoop(string loopId, SoundEffect effect, float value)
    {
        ((Loop)loops[loopId]).setEffect(effect, value);
    }




    void endOfLoop(object sender, System.Timers.ElapsedEventArgs e)
    {
        Console.Out.WriteLine("loop");
        endOfLoop();
    }

    void endOfLoop()
    {
        timer.Enabled = true;
        foreach (string loopId in deletedLoops)
        {
            ((Loop)loops[loopId]).prepareForDestruction();
            loops.Remove(loopId);
            if ((bool)loopStates[loopId]) activeCount--;
            loopStates.Remove(loopId);
        }
        deletedLoops.Clear();
        foreach (string loopId in loops.Keys)
        {
            ((Loop)loops[loopId]).setState((bool)loopStates[loopId]);
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
