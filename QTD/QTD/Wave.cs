using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALib;

namespace QTD
{
    public struct WaveSpawnHelper
    {
        public WayPoint startWP;
        public List<BaseRunner> Runners;

        public WaveSpawnHelper(int wpID, params BaseRunner[] runners)
        {
            startWP = Level.Instance.WayPoints.Find(w => w.ID == wpID);
            Runners = new List<BaseRunner>();
            Runners.AddRange(runners);
        }
    }

    public class Wave
    {
        int WaveNr;

        public List<WaveSpawnHelper> WaveRunners = new List<WaveSpawnHelper>();
        SimpleTimer RunnerSpawnDelayer;
        bool IsSpawning = false;
        public int TimeUntilNextWaveInMS;
        bool IsDoneSpawning;
        
        public Wave(int waveNr, int spawnDelayInMS)
        {
            WaveNr = waveNr;
            RunnerSpawnDelayer = new SimpleTimer(spawnDelayInMS);
        }

        public void StartSpawn()
        {
            IsSpawning = true;
        }

        public void Update(GameTime gameTime)
        {
            if (IsSpawning && !IsDoneSpawning)
            {
                RunnerSpawnDelayer.Update(gameTime);
                if (RunnerSpawnDelayer.IsDone)
                {
                    for (int i = 0; i < WaveRunners.Count; i++)
                    {
                        BaseRunner runner = WaveRunners[i].Runners.Last();
                        Level.Instance.ActiveRunners.Add(runner);
                        WaveRunners[i].Runners.Remove(runner);
                        runner.SetLocation(WaveRunners[i].startWP);

                        if (WaveRunners[i].Runners.Count == 0)
                        {
                            WaveRunners.RemoveAt(i);
                            i--;
                        }
                    }

                    if (WaveRunners.Count == 0)
                        IsDoneSpawning = true;
                    else
                        RunnerSpawnDelayer.Reset();
                }
            }
        }
    }
}