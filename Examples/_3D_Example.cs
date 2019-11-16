﻿using FmodAudio;
using System;
using System.Numerics;

namespace Examples
{
    using Base;

    public class _3DExample : Example
    {
        const int InterfaceUpdateTime = 50;
        const float DistanceFactor = 1.0f;
        
        private readonly char[] ss = "|.............<1>......................<2>.......|".ToCharArray();

        private float T = 0.0f;
        private Vector3 LastPos;
        private Vector3 ListenerPos = new Vector3() { Z = -1.0f * DistanceFactor };
        private Vector3 Up = new Vector3(0, 1, 0), Forward = new Vector3(0, 0, 1);

        private Sound s1, s2, s3;
        private Channel c1, c2, c3;
        private bool autoMove = true;

        public override void Initialize()
        {
            //Creates the FmodSystem object
            base.Initialize();

            //System object Initialization
            System.Init(32);

            //Set the distance Units (Meters/Feet etc)
            System.Set3DSettings(1.0f, DistanceFactor, 1.0f);

            //Load some sounds
            float min = 0.5f * DistanceFactor, max = 5000.0f * DistanceFactor;

            s1 = System.CreateSound(MediaPath("drumloop.wav"), Mode._3D | Mode.Loop_Normal);
            s1.Set3DMinMaxDistance(min, max);

            s2 = System.CreateSound(MediaPath("jaguar.wav"), Mode._3D | Mode.Loop_Normal);
            s2.Set3DMinMaxDistance(min, max);

            s3 = System.CreateSound(MediaPath("swish.wav"), Mode._2D);

            //Play sounds at certain positions
            Vector3 pos = default, vel = default;

            pos.X = -10.0f * DistanceFactor;

            c1 = System.PlaySound(s1, paused: true);
            c1.Set3DAttributes(in pos, in vel, default);
            c1.Paused = false;

            pos.X = 15.0f * DistanceFactor;

            c2 = System.PlaySound(s2, paused: true);
            c2.Set3DAttributes(in pos, in vel, default);
            c2.Paused = false;
        }

        public override void Run()
        {
            char[] s = new char[ss.Length];

            //Main Loop
            do
            {
                OnUpdate();

                if (!this.Commands.IsEmpty)
                {
                    while (this.Commands.TryDequeue(out Button button))
                    {
                        switch (button)
                        {
                            case Button.Action1:
                                c1.Paused = !c1.Paused;
                                break;
                            case Button.Action2:
                                c2.Paused = !c2.Paused;
                                break;
                            case Button.Action3:
                                if (c3 == null || !c3.IsPlaying)
                                    c3 = System.PlaySound(s3);
                                break;
                            case Button.More:
                                autoMove = !autoMove;
                                break;
                            case Button.Quit:
                                goto BreakLoop;
                        }
                    }
                }

                // ==========================================================================================
                // UPDATE THE LISTENER
                // ==========================================================================================

                if (autoMove)
                {
                    ListenerPos.X = MathF.Sin(this.T * 0.05f) * 24 * DistanceFactor;
                }

                var vel = (ListenerPos - LastPos) * new Vector3(1000f / InterfaceUpdateTime);

                LastPos = ListenerPos;

                System.Set3DListenerAttributes(0, in ListenerPos, in vel, in Forward, in Up);

                this.T += 30f / InterfaceUpdateTime;

                System.Update();

                ss.AsSpan().CopyTo(s);
                s[(int)(ListenerPos.X / DistanceFactor) + 25] = 'L';

                DrawText("==================================================");
                DrawText("3D Example");
                DrawText("Copyright (c) Firelight Technologies 2004-2018.");
                DrawText("==================================================");
                DrawText();
                DrawText("Press 1 to toggle sound 1 (16bit Mono 3D)");
                DrawText("Press 2 to toggle sound 2 (8bit Mono 3D)");
                DrawText("Press 3 to play a sound (16bit Stereo 2D)");
                DrawText("Press Space to toggle listener auto movement");
                DrawText();
                DrawText(s);
                
                Sleep(InterfaceUpdateTime - 1);
            } while (true);

            BreakLoop:
            return;
        }

        public override void Dispose()
        {
            s1?.Dispose();
            s2?.Dispose();
            s3?.Dispose();

            base.Dispose();
        }

        public override string Title => "Fmod 3D Example";
    }
}
