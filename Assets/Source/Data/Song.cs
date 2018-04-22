using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public enum BeatType: int
{
    Default = 0,
    BonusMine,
    BonusGas,
    BonusPopulation,
    BonusDamage,
    Path
}

public struct BeatEntry
{
    public int beat;
    public int duration;
    public BeatType type;

    public BeatEntry(int _beat, int _duration, BeatType _type)
    {
        beat = _beat;
        duration = _duration;
        type = BeatType.Default;
    }
}

[System.Serializable]
public class Song
{
    public AudioClip songResource;
    public int bpm;
    public TextAsset songSheet;

    public List<BeatEntry> beats = new List<BeatEntry>();

    public void LoadSheet()
    {
        string[] lines = Regex.Split(songSheet.text, "\n|\r|\r\n");
        foreach (string l in lines)
        {
            string[] tokens = l.Split(' ');
            if (tokens.Length != 3)
            {
                continue;
            }
            BeatEntry b = new BeatEntry();
            System.Int32.TryParse(tokens[0], out b.beat);
            System.Int32.TryParse(tokens[1], out b.duration);
            int type;
            System.Int32.TryParse(tokens[2], out type);
            b.type = (BeatType)type;
            beats.Add(b);
        }
    }

   public bool GetBeat(int idx, out BeatEntry entry)
    {
        bool valid = idx >= 0 && idx < beats.Count;
        if (valid)
        {
            entry = beats[idx];
        }
        else
        {
            entry = new BeatEntry(-1, 0, BeatType.Default);
        }
        return valid;
    }

    public List<BeatEntry> GetBeatsInRange(double elapsedTime, float incomingThreshold, float pastThreshold = -1.0f) // -1: Reuse,  0: Not added.
    {
        List<BeatEntry> result = new List<BeatEntry>();
        foreach(BeatEntry entry in beats)
        {
            double time = entry.beat * 60 / bpm;
            double delta = time - elapsedTime;
            // Max range:
            double minThreshold = pastThreshold < 0.0f
                ? incomingThreshold
                : Mathf.Approximately(pastThreshold, 0.0f)
                    ? elapsedTime
                    : pastThreshold;
            if (delta > incomingThreshold && delta < minThreshold) // Not there yet
                continue;
            result.Add(entry);
        }
        return result;
    }

    public List<BeatEntry> GetUpcomingBeats(int limit, int start = 0)
    {
        List<BeatEntry> result = new List<BeatEntry>();
        foreach (BeatEntry beat in beats)
        {
            if (beat.beat >= start && beat.beat <= start + limit)
            {
                result.Add(beat);
            }
        }
        return result;
    }

    public List<int> GetBeats()
    {
        List<int> beatResult = new List<int>();
        foreach(BeatEntry beat in beats)
        {
            beatResult.Add(beat.beat);
        }
        return beatResult;
    }
}
