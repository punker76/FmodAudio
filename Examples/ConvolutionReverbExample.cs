﻿using FmodAudio;
using FmodAudio.DigitalSignalProcessing;
using System;
using System.Runtime.InteropServices;

namespace Examples
{
    using System.Runtime.CompilerServices;
    using Base;
    using FmodAudio.Base;

    public unsafe class ConvolutionReverbExample : Example
    {
        private ChannelGroup reverbGroup, mainGroup;
        private Dsp reverbUnit;
        private Sound sound;

        private short[] IRData;

        float wetVolume = 1f, dryVolume = 1f;

        public ConvolutionReverbExample() : base ("Fmod Convolution Reverb Example")
        {
            RegisterCommand(ConsoleKey.LeftArrow, () => wetVolume = Math.Clamp(wetVolume - 0.05f, 0, 1));
            RegisterCommand(ConsoleKey.RightArrow, () => wetVolume = Math.Clamp(wetVolume + 0.05f, 0, 1));
            RegisterCommand(ConsoleKey.UpArrow, () => dryVolume = Math.Clamp(dryVolume + 0.05f, 0, 1));
            RegisterCommand(ConsoleKey.DownArrow, () => dryVolume = Math.Clamp(dryVolume - 0.05f, 0, 1));
        }

        public override void Initialize()
        {
            base.Initialize();

            System.Init(32);

            reverbGroup = System.CreateChannelGroup("reverb");
            mainGroup = System.CreateChannelGroup("main");

            System.MasterChannelGroup.AddGroup(mainGroup);
            System.MasterChannelGroup.Volume = 0.25f;

            reverbUnit = System.CreateDSPByType(DSPType.ConvolutionReverb);

            reverbGroup.AddDSP(ChannelControlDSPIndex.DSPTail, reverbUnit);

            var tmpsound = System.CreateSound(MediaPath("standrews.wav"), Mode.Default | Mode.OpenOnly);

            tmpsound.GetFormat(out _, out SoundFormat sFormat, out int irSoundChannels, out _);

            if (sFormat != SoundFormat.PCM16)
            {
                Console.WriteLine("Sound file's format is not PCM16.");
                Environment.Exit(-1);
            }

            int irSoundLength = (int)tmpsound.GetLength(TimeUnit.PCM);

            //Allocate a pre-pinned array, tracked by the GC so we have no need to free it ourselves.
            this.IRData = GC.AllocateArray<short>(irSoundLength * irSoundChannels + 1, pinned: true);

            tmpsound.ReadData<short>(IRData);

            const int ReverbParamIR = 0;
            const int ReverbParamDry = 2;

            reverbUnit.SetParameterData(ReverbParamIR, Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(IRData)), (uint)IRData.Length * sizeof(short));
            reverbUnit.SetParameterFloat(ReverbParamDry, -80f);

            tmpsound.Release();

            sound = System.CreateSound(MediaPath("singing.wav"), Mode._3D | Mode.Loop_Normal);
        }

        public override void Run()
        {
            Channel channel = System.PlaySound(sound, mainGroup, true);

            var channelHead = channel.GetDSP(ChannelControlDSPIndex.DspHead);

            DspConnection reverbConnection = reverbUnit.AddInput(channelHead, DSPConnectionType.Send);

            channel.Paused = false;

            do
            {
                OnUpdate();

                System.Update();

                reverbConnection.Mix = wetVolume;
                mainGroup.Volume = dryVolume;

                DrawText("==================================================");
                DrawText("Convolution Example.");
                DrawText("Copyright (c) Firelight Technologies 2004-2018.");
                DrawText("==================================================");
                DrawText("Press Up and Down arrows to change dry mix");
                DrawText("Press Left and Right Arrows to change wet mix");
                DrawText($"Wet mix: {wetVolume}, Dry mix: {dryVolume}");
                DrawText("Press Esc to Quit.");

                Sleep(50);

            }
            while (!ShouldEndExample);
        }

        public override void Dispose()
        {
            if (sound != default)
                sound.Dispose();

            mainGroup?.Dispose();

            if (reverbUnit != default && reverbGroup != null)
            {
                reverbGroup.RemoveDSP(reverbUnit);
                reverbGroup.Dispose();
                reverbUnit.Dispose();
            }
            else
            {
                if (reverbGroup != null) reverbGroup.Dispose();

                if (reverbUnit != default) reverbUnit.Dispose();
            }

            base.Dispose();
        }
    }
}
