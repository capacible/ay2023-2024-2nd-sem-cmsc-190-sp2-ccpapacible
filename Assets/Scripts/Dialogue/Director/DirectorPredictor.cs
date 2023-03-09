using Microsoft.ML.Probabilistic.Distributions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DirectorPredictor : DirectorModel
{

    public DirectorPredictor(int totalEvents, int totalTraits, int totalDialogue, int totalRelStatus)
        : base(totalEvents, totalTraits, totalDialogue, totalRelStatus)
    {

    }

    public static int GetProperWeight(DialogueLine dl, int mood)
    {
        // return neutral
        if (mood > (int)MoodThreshold.GOOD)
        {
            return dl.posWeight;
        }
        else if (mood < (int)MoodThreshold.BAD)
        {
            return dl.negWeight;
        }

        return dl.neutWeight;
    }

    /// <summary>
    /// Computes the expected utility of one dialogue line
    /// </summary>
    /// <param name="lineProbability">
    ///     DialogueLine                => key
    ///     float / probabiloty of line => value
    /// </param>
    /// <returns></returns>
    public static double ComputeExpectedUtility(KeyValuePair<int, double> lineProbability, Dictionary<string, float> topicList, int mood)
    {
        DialogueLine lineContainer = Director.lineDB[lineProbability.Key];

        // we create a list to keep track of each p(line) * utility
        List<double> utilVals = new List<double>()
            {
                // probability of DLine * weight of line
                lineProbability.Value * GetProperWeight(lineContainer, mood)
            };

        // get probability with the related topics.
        foreach (string topic in lineContainer.relatedTopics)
        {
            // accessing the current topic in the topic relevance tracker and mul with the prob value of the line
            utilVals.Add(lineProbability.Value * (double)topicList[topic]);
        }

        return utilVals.Sum();
    }

    /// <summary>
    /// Utility function. Particularly, checks the topic tracker, and the current mood value and modifies the probability of all
    /// lines
    /// </summary>
    /// <returns></returns>
    public static int UtilFunc(Dictionary<int, double> probabilities, Dictionary<string, float> topicList, int mood)
    {
        double highestUtil = 0;
        int bestDialogue = -1;

        // for every line we are considering w/ probabilities, we get the related topic in the dialogue line
        foreach (KeyValuePair<int, double> probPair in probabilities)
        {
            // we do:
            // summation of each utility
            // u(given related topic/s)
            // u(posweight)

            double value = ComputeExpectedUtility(probPair, topicList, mood);

            // compare
            if (bestDialogue == -1)
            {
                // set bestdialogue to be the current
                bestDialogue = probPair.Key;
                highestUtil = value;
            }
            else
            {
                // if highest utility is lower than our computed value, then we replace the dialogue and the highest utility
                if (highestUtil < value)
                {
                    bestDialogue = probPair.Key;
                    highestUtil = value;
                }
            }
        }

        if (bestDialogue == -1)
        {
            // something went wrong
            Debug.LogError("A best dialogue is not found. Trace back what went wrong.");
        }

        return bestDialogue;
    }

    /// <summary>
    /// Infers the posteriors of the dialogue line given the data we have for priors that was set beforehand.
    /// </summary>
    public Dictionary<int, double> InferLinePosteriors()
    {
        // inference of dialogue.
        var dLinePosteriors = engine.Infer<Discrete[]>(Dialogue);       // this is all the discrete possibilities
                                                                        // we get the doubles by dlinposteriors[index].GetProbs()[index]
        int posteriorLen = dLinePosteriors.Length;

        Dictionary<int, double> lineProbPair = new Dictionary<int, double>();
        for(int i = 0; i < posteriorLen; i++)
        {
            lineProbPair.Add(i, dLinePosteriors[i].GetProbs()[i]);
        }

        return lineProbPair;
    }
    

    public DialogueLine SelectBestNPCLine(Dictionary<string, float> topicsList, int currentMood)
    {
        // infer and pass into util fxn
        return Director.lineDB[UtilFunc( InferLinePosteriors(), topicsList, currentMood )];
    }

    public List<DialogueLine> SelectBestPlayerLines(Dictionary<string, float> topicsList, int currentMood)
    {
        // infer
        Dictionary<int, double> linePosteriors = InferLinePosteriors();
        
        // do util fxn
        List<DialogueLine> top3 = new List<DialogueLine>();
        
        // get three lines
        for(int i = 0; i < 3; i++)
        {
            int toAdd = UtilFunc(linePosteriors, topicsList, currentMood);

            // remove from the dictionary the line we want to add
            linePosteriors.Remove(toAdd);
            top3.Add(Director.lineDB[toAdd]);
        }

        return top3;
    }
}
