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

        ISoundEffectControl fx;


        public Loop(ISoundEngine _engine, string _filePath)
        {
            engine = _engine;
            filePath = _filePath;

            ISound l = engine.Play2D(filePath, true, true, StreamMode.AutoDetect, true);
            if (l == null)
            {
                throw new Exception("Could not load " + filePath);
            }
            sound = l;
            fx = sound.SoundEffectControl;
        }

        internal void prepareForDestruction()
        {
            if (sound != null)
            {
                sound.Stop();
                sound.Dispose();
            }
        }

        internal void setEffect(SoundEffect effect, float value)
        {
            switch (effect)
            {
                case SoundEffect.Gargle: // OK
                    if (value == 0)
                    {
                        fx.DisableGargleSoundEffect();
                        break;
                    }
                    int rateHz = (int)(1000 * value);
                    bool sinusWaveForm = true;
                    fx.EnableGargleSoundEffect(rateHz, sinusWaveForm);
                    break;
                case SoundEffect.Echo: // OK
                    if (value == 0)
                    {
                        fx.DisableEchoSoundEffect();
                        break;
                    }
                    float fWetDryMix = 100;
                    float fFeedback = 90 * value;
                    float fLeftDelay = 250;
                    float fRightDelay = 250;
                    int IPanDelay = 0;
                    fx.EnableEchoSoundEffect(fWetDryMix, fFeedback, fLeftDelay, fRightDelay, IPanDelay);
                    break;
                case SoundEffect.Volume: // OK
                    sound.Volume = value;
                    break;
                case SoundEffect.WavesReverb: // OK
                    if (value == 0)
                    {
                        fx.DisableWavesReverbSoundEffect();
                        break;
                    }
                    float fInGain = 0;
                    float fReverbMix = 0 * value;
                    float fReverbTime = 0.001f + value * (3000f - 0.001f);
                    float fHighFreqRTRatio = value;
                    fx.EnableWavesReverbSoundEffect(fInGain, fReverbMix, fReverbTime, fHighFreqRTRatio);
                    break;
                case SoundEffect.Flanger: // 0K
                    if (value == 0)
                    {
                        fx.DisableFlangerSoundEffect();
                        break;
                    }
                    float fWetDryMixF = 60;
                    float fDepth = value * 100;
                    float fFeedbackF = -80;
                    float fFrequency = 0.1f;
                    bool bTriangle = true;
                    float fDelay = 2;
                    int iPhase = 0;
                    fx.EnableFlangerSoundEffect(fWetDryMixF, fDepth, fFeedbackF, fFrequency, bTriangle, fDelay, iPhase);
                    break;
                case SoundEffect.Chorus:
                    if (value == 0)
                    {
                        fx.DisableChorusSoundEffect();
                        break;
                    }
                    float fWetDryMixC = 60;
                    float fDepthC = value * 100;
                    float fFeedbackC = -50;
                    float fFrequencyC = 05f;
                    bool sinusWaveFormC = true;
                    float fDelayC = 2;
                    int iPhaseC = 0;
                    fx.EnableChorusSoundEffect(fWetDryMixC, fDepthC, fFeedbackC, fFrequencyC, sinusWaveFormC, fDelayC, iPhaseC);
                    break;
            }
        }

        internal void setState(bool playing)
        {
            //sound.PlayPosition = 0;
            sound.Paused = !playing;
        }
    }
}