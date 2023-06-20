using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Math;
using Microsoft.ML.Probabilistic.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DirectorTrainingModel : DirectorBase
{

    private Dirichlet ProbPost_Events;
    private Dirichlet ProbPost_Traits;
    private Dirichlet ProbPost_RelStatus;
    private Dirichlet[][][] ProbPost_Dialogue;

    #region INITIALIZATION
    /// <summary>
    /// Creating and instantiating the model
    /// </summary>
    public DirectorTrainingModel(int totalEvents, int totalTraits, int totalDialogue, int totalRelStatus)
    {
        // set location of generated source code
        engine.Compiler.GeneratedSourceFolder = @"Assets/";

        // our totals
        TotalDialogueCount = totalDialogue;
        TotalEventCount = totalEvents;
        TotalTraitCount = totalTraits;
        TotalRelCount = totalRelStatus;

        /*
         * CREATING THE RANGES
         */

        // CREATE NUMBER OF CASES RANGE TO USE DUN SA RANDOM VARIABLE INITIALIZATION MISMO
        // number of known cases -- usually the highest number of unique elements, the events
        NumOfCases = Variable.New<int>().Named("NumberOfCases");
        Range N = new Range(NumOfCases).Named("NCases");

        // possible outcomes or parameters
        EventsRange = new Range(totalEvents).Named("Events");
        TraitsRange = new Range(totalTraits).Named("Traits");
        RelStatusRange = new Range(totalRelStatus).Named("Rel");
        DialogueRange = new Range(totalDialogue).Named("Dialogue");

        /*
         * CREATE PRIORS AND CPT TABLES
         *      > create the priors based on our decided distribution
         *      > create random variable parameters based on the priors
         */
         

        // EVENTS        
        ProbPrior_Events = Variable.New<Dirichlet>().Named("EventsPriors");
        Prob_Events = Variable<Vector>.Random(ProbPrior_Events).Named("PossibleEvents");
        Prob_Events.SetValueRange(EventsRange); // sets the length of vector

        // TRAITS
        ProbPrior_Traits = Variable.New<Dirichlet>().Named("TraitsPriors");
        Prob_Traits = Variable<Vector>.Random(ProbPrior_Traits).Named("PossibleTraits");
        Prob_Traits.SetValueRange(TraitsRange);

        // RELSTATUS
        ProbPrior_RelStatus = Variable.New<Dirichlet>().Named("RelStatusPriors");
        Prob_RelStatus = Variable<Vector>.Random(ProbPrior_RelStatus).Named("PossibleRel");
        Prob_RelStatus.SetValueRange(RelStatusRange);


        /*
            CREATING DIALOGUE CONDITIONED ON ALL 3 PARENTS  

             hopefully idiot proof explanation for future me:
             we basically nested variable types from our # of parents (in dialogue case, 3 parents) up to 1 parent.
             we created (on the innermost) a variablearray for just 1 parent; from that we create a variable array from 2 parent
             then created a variablearray from 3 parent.
             remember that this <VariableArray<VariableArray<Dirichlet>, Dirichlet[][]>, Dirichlet[][][]> is the type we declared up above

         */

        // initialize the CPT
        CPTPrior_Dialogue = Variable.Array<VariableArray<VariableArray<Dirichlet>, Dirichlet[][]>, Dirichlet[][][]>(Variable.Array<VariableArray<Dirichlet>, Dirichlet[][]>(Variable.Array<Dirichlet>(RelStatusRange), TraitsRange), EventsRange).Named("DialogueCPTPrior");
        
        // initialize the variable
        CPT_Dialogue = Variable.Array<VariableArray<VariableArray<Vector>, Vector[][]>, Vector[][][]>(Variable.Array<VariableArray<Vector>, Vector[][]>(Variable.Array<Vector>(RelStatusRange), TraitsRange), EventsRange).Named("DialogueCPT");
        // create a random variable for all row/cols of the cpt dialogue
        CPT_Dialogue[EventsRange][TraitsRange][RelStatusRange] = Variable<Vector>.Random(CPTPrior_Dialogue[EventsRange][TraitsRange][RelStatusRange]);
        // the values accepted by cpt dialogue is the range of dialogue 
        CPT_Dialogue.SetValueRange(DialogueRange);


        /*
         * CREATING THE PRIMARY VARIABLES...
         */
        // parents
        // we sort of associate the outcomes (lefthand side) with their probabilities (right hand side, prob_varname) by
        // setting their variable probabilities to be of the discrete type
        Events = Variable.Array<int>(N).Named("AllEvents");
        Events[N] = Variable.Discrete(Prob_Events).ForEach(N);

        Traits = Variable.Array<int>(N).Named("AllTraits");
        Traits[N] = Variable.Discrete(Prob_Traits).ForEach(N);

        RelStatus = Variable.Array<int>(N).Named("AllRels");
        RelStatus[N] = Variable.Discrete(Prob_RelStatus).ForEach(N);

        // children
        Dialogue = AddDialogueNodeFrmParents(Events, Traits, RelStatus, CPT_Dialogue);

    }

    /// <summary>
    /// Connects child node from three parents
    /// </summary>
    /// <param name="events">Events randvar node</param>
    /// <param name="traits">Traits randvar node</param>
    /// <param name="rels">Relationship randvar node</param>
    /// <param name="cptDialogue">Cpt of the dialogue (vector form)</param>
    /// <returns></returns>
    public static VariableArray<int> AddDialogueNodeFrmParents(
        VariableArray<int> events,
        VariableArray<int> traits,
        VariableArray<int> rels,
        VariableArray<VariableArray<VariableArray<Vector>, Vector[][]>, Vector[][][]> cptDialogue)
    {
        // the dimension of our outer 
        var dimension = rels.Range;

        var child = Variable.Array<int>(dimension);

        using (Variable.ForEach(dimension))
        using (Variable.Switch(rels[dimension]))
        using (Variable.Switch(traits[dimension]))
        using (Variable.Switch(events[dimension]))
        {
            // each instance of variable in child of range dimension will be associated w/ each variable in events/traits/rels
            child[dimension] = Variable.Discrete(cptDialogue[events[dimension]][traits[dimension]][rels[dimension]]);
        }

        return child;
    }

    /// <summary>
    /// Returns director area where the distribution is uniform 
    /// </summary>
    /// <returns>
    ///     DirectorData of uniform type; or directordata where all but dline probability is uniform
    /// </returns>
    public DirectorData UniformDirectorData()
    {
        DirectorData data = new DirectorData
        {
            // set recent data as uniform
            eventsProb = Dirichlet.Uniform(TotalEventCount),
            traitsProb = Dirichlet.Uniform(TotalTraitCount),
            relProb = Dirichlet.Uniform(TotalRelCount)
        };


        // Dialogue
        // relcount goes first because it's the outermost value
        //UNIFORM IF WE DON'T HAVE A PREDEFINED DISTRIBUTION
        Dirichlet[] parent1 = Enumerable.Repeat(Dirichlet.Uniform(TotalDialogueCount), TotalRelCount).ToArray();
        Dirichlet[][] parent2 = Enumerable.Repeat(parent1, TotalTraitCount).ToArray();
        data.dialogueProb = Enumerable.Repeat(parent2, TotalEventCount).ToArray();

        return data;
    }
    #endregion

    #region INFERENCE

    /// <summary>
    /// set the observed values of ALL our variables.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="traits"></param>
    /// <param name="rels"></param>
    public void SetObservations(
        int[] dialogue,
        int[] events,
        int[] traits,
        int[] rels)
    {
        NumOfCases.ObservedValue = dialogue.Length;
        Dialogue.ObservedValue = dialogue;

        if (events == null)
        {
            Events.ClearObservedValue();
        }
        else
        {
            // setting our parent observed values
            Events.ObservedValue = events;
        }

        if (traits == null)
        {
            // clear the traits
            Traits.ClearObservedValue();
        }
        else
        {
            Traits.ObservedValue = traits;
        }

        if (rels == null)
        {
            RelStatus.ClearObservedValue();
        }
        else
        {
            RelStatus.ObservedValue = rels;
        }
    }

    /// <summary>
    /// Learn the posteriors of each variable given our samples + known priors
    /// </summary>
    /// <param name="dialogueSample"></param>
    /// <param name="eventSample"></param>
    /// <param name="traitSample"></param>
    /// <param name="relSample"></param>
    /// <param name="priors"></param>
    /// <returns></returns>
    public void Learn(
        int[] dialogueSample,
        int[] eventSample,
        int[] traitSample,
        int[] relSample,
        DirectorData priors)
    {
        SetObservations(dialogueSample, eventSample, traitSample, relSample);

        // set our priors
        CPTPrior_Dialogue.ObservedValue = priors.dialogueProb;
        ProbPrior_Events.ObservedValue = priors.eventsProb;
        ProbPrior_Traits.ObservedValue = priors.traitsProb;
        ProbPrior_RelStatus.ObservedValue = priors.relProb;

        ProbPost_Dialogue = engine.Infer<Dirichlet[][][]>(CPT_Dialogue);
        ProbPost_Events = engine.Infer<Dirichlet>(Prob_Events);
        ProbPost_Traits = engine.Infer<Dirichlet>(Prob_Traits);
        ProbPost_RelStatus = engine.Infer<Dirichlet>(Prob_RelStatus);

    }


    /// <summary>
    /// we return a director data based on our posteriors.
    /// </summary>
    /// <returns></returns>
    public DirectorData DataFromPosteriors()
    {
        return new DirectorData
        {
            eventsProb = ProbPost_Events,
            traitsProb = ProbPost_Traits,
            relProb = ProbPost_RelStatus,
            dialogueProb = ProbPost_Dialogue
        };
    }
    #endregion
}
