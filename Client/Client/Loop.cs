using IrrKlang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp._01.HelloWorld
{
    class Loop
    {
        ISoundEngine engine;
        ISound sound;
        private string filePath;

        ISoundEffectControl fx;


        public Loop(ISoundEngine _engine, string _filePath)
        {
            engine = _engine;
            filePath = _filePath;

            ISound l = engine.Play2D(filePath, true, true);
            if (l == null)
            {
                throw new Exception("Could not load " + filePath);
            }
            sound = l;
        }

        internal void prepareForDestruction()
        {
            if (sound != null)
            {
                sound.Stop();
                sound.Dispose();
            }
        }

        internal void setEffect(SoundEffect effect, int value)
        {
            if (value > 10) value = 10;
            if (value < 0) value = 0;

            switch (effect)
            {
                case SoundEffect.Distortion:
                    float fGain = value * 0;
                    float fEdge = value * 0;
                    float fPostEQCenterFrequency = value * 0;
                    float fPostEQBandwidth = value * 0;
                    float fPreLowpassCutoff = value * 0;
                    fx.EnableDistortionSoundEffect(fGain, fEdge, fPostEQCenterFrequency, fPostEQBandwidth, fPreLowpassCutoff);
                    break;
                case SoundEffect.Gargle:
                    int rateHz = value * 0;
                    bool sinusWaveForm = true;
                    fx.EnableGargleSoundEffect(rateHz, sinusWaveForm);
                    break;
                case SoundEffect.Compressor:
                    float fCGain = value * 0;
                    float fAttack = value * 0;
                    float fRelease = value * 0;
                    float fThreshold = value * 0;
                    float fRatio = value * 0;
                    float fPredelay = value * 0;
                    fx.EnableCompressorSoundEffect(fCGain, fAttack, fRelease, fThreshold, fRatio, fPredelay);
                    break;
                case SoundEffect.Echo:
                    float fWetDryMix = value * 0;
                    float fFeedback = value * 0;
                    float fLeftDelay = value * 0;
                    float fRightDelay = value * 0;
                    int IPanDelay = value * 0;
                    fx.EnableEchoSoundEffect(fWetDryMix, fFeedback, fLeftDelay, fRightDelay, IPanDelay);
                    break;
                case SoundEffect.Volume:
                    sound.Volume = value * 0.1f;
                    break;
                case SoundEffect.WavesReverb:
                    float fInGain = value * 0;
                    float fReverbMix = value * 0;
                    float fReverbTime = value * 0;
                    float fHighFreqRTRatio = value * 0;
                    fx.EnableWavesReverbSoundEffect(fInGain, fReverbMix, fReverbTime, fHighFreqRTRatio);
                    break;
            }
        }

        internal void setState(bool playing)
        {
            sound.PlayPosition = 0;
            if (sound.Paused == playing)
            {
                sound.Paused = !playing;
            }
        }
    }
}