using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBeatTracker
{
    void SetStartTime(double time);
    void Reset(double time);
    void Stop();

    double BPM
    { get; set; }

    double TimeBetweenBeats
    {
        get;
    }

    int NumPendingBeats
    {
        get;
    }
    void ResetBeatCount();

    double GetTotalElapsed();
    double GetElapsedFromLastTick();
    double GetTimeFromPrevTick();
    double GetTimeForTick(int tick);
    double GetRemainingTimeToTick(int tick);
    double GetTimeToNextTick();

}