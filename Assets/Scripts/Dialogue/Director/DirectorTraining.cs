using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectorTraining : DirectorModel
{
    private Variable<int> EventsOccurredLen;
    private Variable<int> TraitsPresentLen;

    // training data
    private VariableArray<int> EventsOccurred;  // list of all events that have occurred by index
    private VariableArray<int> TraitsPresent;
    private Variable<int> RelStatusCurrent;

    public DirectorTraining(int totalEvents, int totalTraits, int totalDialogue, int totalRelStatus) 
        : base(totalEvents, totalTraits, totalDialogue, totalRelStatus)
    {
        // aside from creating our base model, we have to set stuff for the director training

        // initializing the attribs with no values
        EventsOccurredLen = Variable.New<int>();
        TraitsPresentLen = Variable.New<int>();

        // create a range for the events occurred.
        Range eoRange = new Range(EventsOccurredLen);
        Range tRange = new Range(TraitsPresentLen);

        // initialize the actual training data
        EventsOccurred = Variable.Array<int>(eoRange);      // each element in the training data is an event
        TraitsPresent = Variable.Array<int>(tRange);

        // setting the probabilities or distributions of the events
        EventsOccurred[eoRange] = Variable.Discrete(Prob_Events).ForEach(eoRange);
        // distribution of the traits present or observed; applied into the probability of traits
        TraitsPresent[tRange] = Variable.Discrete(Prob_Traits).ForEach(tRange);
        
        // setting distrib of relationship
        RelStatusCurrent = Variable.Discrete(Prob_RelStatus);

    }

    /// <summary>
    /// Infers new direction data
    /// </summary>
    /// <param name="allEventsOccurred"></param>
    /// <param name="allTraitsPresent"></param>
    /// <param name="relStatus"></param>
    /// <returns></returns>
    public DirectorData InferPredictionPriors(int[] allEventsOccurred, int[] allTraitsPresent, int relStatus)
    {
        DirectorData posteriors = new DirectorData();

        // set the lengths
        EventsOccurredLen.ObservedValue = allEventsOccurred.Length;
        TraitsPresentLen.ObservedValue = allTraitsPresent.Length;

        // training data set
        EventsOccurred.ObservedValue = allEventsOccurred;
        TraitsPresent.ObservedValue = allTraitsPresent;
        RelStatusCurrent.ObservedValue = relStatus;

        // make inferences given our probabiloty tables.
        // note that prior to this we have already set our priors in DirectorData.
        posteriors.eventsProb = engine.Infer<Dirichlet>(Prob_Events);
        posteriors.traitsProb = engine.Infer<Dirichlet>(Prob_Traits);
        posteriors.relProb = engine.Infer<Dirichlet>(Prob_RelStatus);
        posteriors.dialogueProb = engine.Infer<Dirichlet[][][]>(CPT_Dialogue);

        return posteriors;
    }

}
