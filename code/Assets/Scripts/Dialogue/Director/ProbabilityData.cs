using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbabilityData
{
    public double initialProb;
    public double finalProb;
    public double finalTopicMod;
    public double lineXTopicMod;
    public double lineXToneMod;
    public double lineXisSaidMod;
    public double toneWeightValue;
    public double isSaidWeightUsed;
    public string lineIsSaid;

    public ProbabilityData Clone()
    {
        return new ProbabilityData
        {
            initialProb = initialProb,
            finalProb = finalProb,
            finalTopicMod = finalTopicMod,
            lineXisSaidMod = lineXisSaidMod,
            lineXToneMod = lineXToneMod,
            lineXTopicMod = lineXTopicMod,
            toneWeightValue = toneWeightValue,
            isSaidWeightUsed = isSaidWeightUsed,
            lineIsSaid = lineIsSaid
        };
    }
}
