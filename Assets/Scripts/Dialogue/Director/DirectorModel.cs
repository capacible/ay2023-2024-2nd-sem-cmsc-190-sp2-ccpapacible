using Microsoft.ML.Probabilistic;
using Microsoft.ML.Probabilistic.Algorithms;
using Microsoft.ML.Probabilistic.Collections;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Math;
using Microsoft.ML.Probabilistic.Models;
using Microsoft.ML.Probabilistic.Models.Attributes;
using Microsoft.ML.Probabilistic.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;

// BASE CLASS

/*
 *  CURRENT ISSUES:
 *      When making inference of dialogue, we expect ALL dialogue lines/possible outcomes to be acquired. However
 *      we just get one in Discrete[], why?
 *      - it's because the return value of discrete[] is really just the one talaga.
 *          - getProbs() is ALL THE PROBABILITIES
 *          - the ith element in getProbs() is the discrete value/dialogue id.
 * 
 */
public class DirectorModel
{
    private const double LINE_IS_SAID_WEIGHT_T = 0.5;
    private const double LINE_IS_SAID_WEIGHT_F = 1.0;

    private const double LINE_HARD_MIN_PROB = 0; // previously 0.00475
    
    private static readonly string DLINE_DISTRIBUTION_PATH = "XMLs/Dialogue/lineCPT";
    private static readonly string EVENTS_DISTRIBUTION_PATH = "XMLs/Dialogue/eventCPT";
    private static readonly string TRAITS_DISTRIBUTION_PATH = "XMLs/Dialogue/traitCPT";
    private static readonly string RELS_DISTRIBUTION_PATH = "XMLs/Dialogue/relCPT";
    public static readonly string MODEL_FOLDER = $"{Application.dataPath}/Models";

    private int TotalEventCount;
    private int TotalTraitCount;
    private int TotalRelCount;
    private int TotalDialogueCount;

    //private string debugProbStr = "";
    private string modelFile;
    private string metaFile;

    // infer
    protected InferenceEngine engine = new InferenceEngine();

    private IGeneratedAlgorithm iaEventsRelKnown;
    private IGeneratedAlgorithm iaTraitsRelKnown;
    private IGeneratedAlgorithm iaAllKnown;
    private IGeneratedAlgorithm iaEventsOnly;
    private IGeneratedAlgorithm iaTraitsOnly;
    private IGeneratedAlgorithm iaRelOnly;
    
    protected Variable<int> NumOfCases;

    /*
     * RANGES
     */
    protected Range EventsRange;
    protected Range TraitsRange;
    protected Range RelStatusRange;
    protected Range DialogueRange;

    /*
     * RANDOM VARIABLE REPRESENTATIONS
     *      one element of the array represents one outcome
     */

    // PARENTS
    protected VariableArray<int> Events;
    protected VariableArray<int> Traits;
    protected VariableArray<int> RelStatus;

    // CHILDREN (depth = 1)
    protected VariableArray<int> Dialogue;

    /*
     * PARAMETER OR CPT REPRESENTATIONS
     *      in vector:
     *          indices represent the outcome,
     *          elements represent the probability
     */

    // PARENTS
    protected Variable<Vector> Prob_Events;
    protected Variable<Vector> Prob_Traits;
    protected Variable<Vector> Prob_RelStatus;

    // CHILDREN
    protected VariableArray<VariableArray<VariableArray<Vector>, Vector[][]>, Vector[][][]> CPT_Dialogue;

    /*
     * DISTRIBUTIONS
     */
    protected Variable<Dirichlet> ProbPrior_Events;
    protected Variable<Dirichlet> ProbPrior_Traits;
    protected Variable<Dirichlet> ProbPrior_RelStatus;

    protected VariableArray<VariableArray<VariableArray<Dirichlet>, Dirichlet[][]>, Dirichlet[][][]> CPTPrior_Dialogue;

    /*
     *  POSTERIORS
     */
    private Dirichlet ProbPost_Events;
    private Dirichlet ProbPost_Traits;
    private Dirichlet ProbPost_RelStatus;
    private Dirichlet[][][] defaultDialoguePriors;


    #region INITIALIZATION
    /// <summary>
    /// Creating and instantiating the model
    /// </summary>
    public DirectorModel(int totalEvents, int totalTraits, int totalDialogue, int totalRelStatus, IAlgorithm algorithm, string modelName="DialogueDirector")
    {

        engine.Algorithm = algorithm;
        engine.ModelName = modelName;
        // set location of generated source code
        engine.Compiler.GeneratedSourceFolder = MODEL_FOLDER;

        // our totals
        TotalDialogueCount = totalDialogue;
        TotalEventCount = totalEvents;
        TotalTraitCount = totalTraits;
        TotalRelCount = totalRelStatus;

        /*
         * CREATING THE RANGES
         */

        // CREATE NUMBER OF CASES RANGE TO USE DUN SA RANDOM VARIABLE INITIALIZATION MISMO
        NumOfCases = Variable.New<int>().Named("NumOfCases");
        // number of known cases -- usually the highest number of unique elements, the events
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
    
        // initialize the variable cpt_dialogue conditioned on the prior dialogue
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
        Dialogue = AddDialogueNodeFrmParents(N, Events, Traits, RelStatus, CPT_Dialogue);
       
    }
    

    /// <summary>
    /// Connects child node from three parents
    /// </summary>
    /// <param name="dimension">Range of innermost variable</param>
    /// <param name="events">Events randvar node</param>
    /// <param name="traits">Traits randvar node</param>
    /// <param name="rels">Relationship randvar node</param>
    /// <param name="cptDialogue">Cpt of the dialogue (vector form)</param>
    /// <returns></returns>
    public VariableArray<int> AddDialogueNodeFrmParents(
        Range dimension,
        VariableArray<int> events,
        VariableArray<int> traits,
        VariableArray<int> rels,
        VariableArray<VariableArray<VariableArray<Vector>, Vector[][]>, Vector[][][]> cptDialogue)
    {
        //var child = Variable.Observed<int>(null, dimension).Named("DialogueChild");
        
        var child = Variable.Array<int>(dimension).Named("DialogueChild");
               
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
    
    /// <summary>
    /// Called during runtime / in-game, we deserialize the necessary cpt-related files to be used as distributions 
    /// This will be used in our initial setting of data
    /// </summary>
    public void Start()
    {
        // setting observed values.
        /*
         *  THERE ARE DIFFERENT TYPES OF POSSIBLE COMPILED ALGORITHMS, CONSIDERING THE VARIOUS TYPES OF AVAILABLE OR KNOWN DATA.
         */

        // create the algo for this one
        /*
        iaEventsRelKnown = null;
        iaAllKnown = null;
        iaTraitsRelKnown = null;*/   
        
        iaEventsRelKnown = new Models.DialogueDirector_EP();
        iaTraitsRelKnown = new Models.DialogueDirector0_EP();
        iaAllKnown = new Models.DialogueDirector1_EP();

        iaEventsOnly = new Models.DialogueDirector2_EP();
        iaTraitsOnly = new Models.DialogueDirector3_EP();
        iaRelOnly = new Models.DialogueDirector4_EP();
        
        
        Dirichlet eventProbs = Dirichlet.Uniform(TotalEventCount);
        Dirichlet traitProbs = Dirichlet.Uniform(TotalTraitCount);
        Dirichlet relProbs = Dirichlet.Uniform(TotalRelCount);

        // cpt of events
        iaEventsRelKnown.SetObservedValue(ProbPrior_Events.NameInGeneratedCode, eventProbs);
        iaTraitsRelKnown.SetObservedValue(ProbPrior_Events.NameInGeneratedCode, eventProbs);
        iaAllKnown.SetObservedValue(ProbPrior_Events.NameInGeneratedCode, eventProbs);

        iaEventsOnly.SetObservedValue(ProbPrior_Events.NameInGeneratedCode, eventProbs);
        iaTraitsOnly.SetObservedValue(ProbPrior_Events.NameInGeneratedCode, eventProbs);
        iaRelOnly.SetObservedValue(ProbPrior_Events.NameInGeneratedCode, eventProbs);

        // cpt of traits
        iaEventsRelKnown.SetObservedValue(ProbPrior_Traits.NameInGeneratedCode, traitProbs);
        iaTraitsRelKnown.SetObservedValue(ProbPrior_Traits.NameInGeneratedCode, traitProbs);
        iaAllKnown.SetObservedValue(ProbPrior_Traits.NameInGeneratedCode, traitProbs);

        iaEventsOnly.SetObservedValue(ProbPrior_Traits.NameInGeneratedCode, traitProbs);
        iaTraitsOnly.SetObservedValue(ProbPrior_Traits.NameInGeneratedCode, traitProbs);
        iaRelOnly.SetObservedValue(ProbPrior_Traits.NameInGeneratedCode, traitProbs);

        // cpt of rels
        iaEventsRelKnown.SetObservedValue(ProbPrior_RelStatus.NameInGeneratedCode, relProbs);
        iaTraitsRelKnown.SetObservedValue(ProbPrior_RelStatus.NameInGeneratedCode, relProbs);
        iaAllKnown.SetObservedValue(ProbPrior_RelStatus.NameInGeneratedCode, relProbs);

        iaEventsOnly.SetObservedValue(ProbPrior_RelStatus.NameInGeneratedCode, relProbs);
        iaTraitsOnly.SetObservedValue(ProbPrior_RelStatus.NameInGeneratedCode, relProbs);
        iaRelOnly.SetObservedValue(ProbPrior_RelStatus.NameInGeneratedCode, relProbs);

        Debug.Log("All inferences algo loaded successfully");

    }

    #endregion
    #region BEFORE-RUNNING

    public void GenerateAlgorithms(int gameActive, int noneStr)
    {
        modelFile = Path.Combine(Application.dataPath, engine.ModelName);// delete meta file
        string meta = modelFile.Split('.')[0];
        metaFile = meta;
                
        InferenceEngine.DefaultEngine.ShowFactorGraph = true;
        engine.SaveFactorGraphToFolder = "Assets/Models";

        CPTPrior_Dialogue.ObservedValue = new Dirichlet[TotalEventCount][][];
        ProbPrior_Events.ObservedValue = Dirichlet.Uniform(TotalEventCount);
        ProbPrior_Traits.ObservedValue = Dirichlet.Uniform(TotalTraitCount);
        ProbPrior_RelStatus.ObservedValue = Dirichlet.Uniform(TotalRelCount);
        
        /*
         *  THERE ARE DIFFERENT TYPES OF POSSIBLE COMPILED ALGORITHMS, CONSIDERING THE VARIOUS TYPES OF AVAILABLE OR KNOWN DATA.
         */

        int[] events = new int[] { };
        int[] traits = new int[] {  };
        int[] rels = new int[] {  };
        NumOfCases.ObservedValue = 0;

        Debug.Log("Getting the inference algorithms...");



        SetPreInferenceObservations(events, null, rels);    // we set the observed value
        // create the algo for this one
        iaEventsRelKnown = engine.GetCompiledInferenceAlgorithm(Dialogue, CPT_Dialogue);

        // for traits only
        SetPreInferenceObservations(null, traits, rels);
        iaTraitsRelKnown = engine.GetCompiledInferenceAlgorithm(Dialogue, CPT_Dialogue);

        // for all applicable
        SetPreInferenceObservations(events, traits, rels);
        iaAllKnown = engine.GetCompiledInferenceAlgorithm(Dialogue, CPT_Dialogue);

        // setting for: events only, traits only, rels only
        SetPreInferenceObservations(events, null, null);
        iaEventsOnly = engine.GetCompiledInferenceAlgorithm(Dialogue, CPT_Dialogue);

        SetPreInferenceObservations(null, traits, null);
        iaTraitsOnly = engine.GetCompiledInferenceAlgorithm(Dialogue, CPT_Dialogue);

        SetPreInferenceObservations(null, null, rels);
        iaRelOnly = engine.GetCompiledInferenceAlgorithm(Dialogue, CPT_Dialogue);

        Debug.Log("All inferences algo loaded successfully");
    }
    
    public T DeserializeCPTGivenFullPath<T>(string path)
    {
        DataContractSerializer serializer = new DataContractSerializer(typeof(T), new DataContractSerializerSettings { DataContractResolver = new InferDataContractResolver() });

        using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(new FileStream(path, FileMode.Open), new XmlDictionaryReaderQuotas()))
        {
            // deserialize/ read the distribution
            return (T)serializer.ReadObject(reader);
        }
    }

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

        InferenceEngine.DefaultEngine.ShowFactorGraph = true;
        engine.SaveFactorGraphToFolder = "Assets/Models";

        SetObservations(dialogueSample, eventSample, traitSample, relSample);

        // set our priors
        CPTPrior_Dialogue.ObservedValue = priors.dialogueProb;
        ProbPrior_Events.ObservedValue = priors.eventsProb;
        ProbPrior_Traits.ObservedValue = priors.traitsProb;
        ProbPrior_RelStatus.ObservedValue = priors.relProb;

        defaultDialoguePriors = engine.Infer<Dirichlet[][][]>(CPT_Dialogue);
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
            dialogueProb = defaultDialoguePriors
        };
    }
    #endregion


    #region DEPRECATED (leaving in here for reference)


    /// <summary>
    /// Returns a list of probavbilities for each dialogue in the CPT -- using Infer(); which recompiles and creates a new assembly.
    /// Only use this as last resort, if IGeneratedAlgorithm implementation does not work.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="traits"></param>
    /// <param name="rels"></param>
    /// <param name="priors"></param>
    /// <returns></returns>
    public List<double> GetDialogueProbabilities(
        int[] events,
        int[] traits,
        int[] rels)
    {
        // we set the observed values
        SetPreInferenceObservations(events, traits, rels);

        Dialogue.ClearObservedValue();
        
        // inference
        var inferenceMultipleCase = engine.Infer<Discrete[]>(Dialogue);
                
        // averaging the probabilities of the cases we have inferred
        // each case has probabilities for each dialogue line.
        List<double> avgProbs = new List<double>();
        
        for(int i=0; i < TotalDialogueCount; i++)
        {
            List<double> probOfCurrentDialogue = new List<double>();
            // we get the ith element of each Discrete.GetProbs()
            inferenceMultipleCase.ForEach(c => probOfCurrentDialogue.Add(c.GetProbs()[i]));

            // average this.
            avgProbs.Add(probOfCurrentDialogue.Average());
        }


        Debug.Log("inferred: " + inferenceMultipleCase.Length);
        Debug.Log("probabilities: " + avgProbs.Count);

        return avgProbs;
    }

    #endregion

    #region IN GAME INFERENCE

    /// <summary>
    /// Sets our observations given optional events, traits, and relationships.
    ///     This gets run in Start() while we initialize our inference algorithms.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="traits"></param>
    /// <param name="rels"></param>
    public void SetPreInferenceObservations(
        int[] events,
        int[] traits,
        int[] rels)
    {
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
            Debug.Log("traits array is null");
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
    /// Setting the observed variables in the selected iGeneratedAlgorithm given the laman of the observation arrays (events, traits, and rels)
    /// then returns the marginals that are computed.
    /// 
    /// This is used every time there is a change in relationship, event, trait, etcetera, of the character to update
    /// their priors.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="traits"></param>
    /// <param name="rels"></param>
    /// <returns></returns>
    public void UpdateSpeakerDialogueProbs(
        int[] events,
        int[] traits,
        int[] rels,
        ref List<double> dialogueProbability,
        ref Dirichlet[][][] dialoguePrior)
    {
        bool knowEv = false, knowTrait = false, knowRel = false;
        if(events!=null)
        {
            Debug.Log("EVENT VALUE TRUE");
            // we set the flag that we know the event data.
            knowEv = true;
        }

        if(traits!=null)
        {
            Debug.Log("TRAITS VALUE TRUE");
            // set flag of known trait
            knowTrait = true;
        }

        if(rels!=null)
        {
            Debug.Log("RELS VALUE TRUE");
            knowRel = true;
        }

        // if our passed priors are null, we use the default
        Dirichlet[][][] usePrior;
        if (dialoguePrior != null)
        {
            usePrior = dialoguePrior;
        }
        else
        {
            Debug.LogError("WE HAVE NO PRIORS");
            return;
        }

        // set observed considering w/c are true.
        if(knowEv && knowRel && knowTrait)
        {
            iaAllKnown.SetObservedValue(NumOfCases.NameInGeneratedCode, events.Length);
            iaAllKnown.SetObservedValue(Events.NameInGeneratedCode, events);
            iaAllKnown.SetObservedValue(Traits.NameInGeneratedCode, traits);
            iaAllKnown.SetObservedValue(RelStatus.NameInGeneratedCode, rels);
            iaAllKnown.SetObservedValue(CPTPrior_Dialogue.NameInGeneratedCode, usePrior);

            // update and get probability
            iaAllKnown.Execute(1);

            dialoguePrior = iaAllKnown.Marginal<Dirichlet[][][]>(CPT_Dialogue.NameInGeneratedCode);
            var result = iaAllKnown.Marginal<Discrete[]>(Dialogue.NameInGeneratedCode);

            // replace probability table of the table we pass from speaker
            dialogueProbability = result[0].GetProbs().ToList();
        }
        else if(knowEv && knowRel)
        {
            iaEventsRelKnown.SetObservedValue(NumOfCases.NameInGeneratedCode, events.Length);
            iaEventsRelKnown.SetObservedValue(Events.NameInGeneratedCode, events);
            iaEventsRelKnown.SetObservedValue(RelStatus.NameInGeneratedCode, rels);
            iaEventsRelKnown.SetObservedValue(CPTPrior_Dialogue.NameInGeneratedCode, usePrior);

            // update and get probability
            iaEventsRelKnown.Execute(1);

            dialoguePrior = iaEventsRelKnown.Marginal<Dirichlet[][][]>(CPT_Dialogue.NameInGeneratedCode);
            var result = iaEventsRelKnown.Marginal<Discrete[]>(Dialogue.NameInGeneratedCode);

            // replace probability table of the table we pass from speaker
            dialogueProbability = result[0].GetProbs().ToList();
        }
        else if (knowEv)
        {
            Debug.Log("updating given EVENT");

            iaEventsOnly.SetObservedValue(NumOfCases.NameInGeneratedCode, events.Length);
            iaEventsOnly.SetObservedValue(Events.NameInGeneratedCode, events);
            iaEventsOnly.SetObservedValue(CPTPrior_Dialogue.NameInGeneratedCode, usePrior);

            // update and get probability
            iaEventsOnly.Execute(1);

            // update the prior
            dialoguePrior = iaEventsOnly.Marginal<Dirichlet[][][]>(CPT_Dialogue.NameInGeneratedCode);
            var result = iaEventsOnly.Marginal<Discrete[]>(Dialogue.NameInGeneratedCode);

            // replace probability table
            dialogueProbability = result[0].GetProbs().ToList();
        }
        else if (knowTrait)
        {
            Debug.Log("updating given TRAIT");

            iaTraitsOnly.SetObservedValue(NumOfCases.NameInGeneratedCode, traits.Length);
            iaTraitsOnly.SetObservedValue(Traits.NameInGeneratedCode, traits);
            iaTraitsOnly.SetObservedValue(CPTPrior_Dialogue.NameInGeneratedCode, usePrior);

            // update and get probability
            iaTraitsOnly.Execute(1);

            dialoguePrior = iaTraitsOnly.Marginal<Dirichlet[][][]>(CPT_Dialogue.NameInGeneratedCode);
            var result = iaTraitsOnly.Marginal<Discrete[]>(Dialogue.NameInGeneratedCode);

            // replace probability table
            dialogueProbability = result[0].GetProbs().ToList();
        }
        else if (knowRel)
        {
            Debug.Log("updating given rel");

            iaRelOnly.SetObservedValue(NumOfCases.NameInGeneratedCode, rels.Length);
            iaRelOnly.SetObservedValue(RelStatus.NameInGeneratedCode, rels);
            iaRelOnly.SetObservedValue(CPTPrior_Dialogue.NameInGeneratedCode, usePrior);

            // update and get probability
            iaRelOnly.Execute(1);

            dialoguePrior = iaRelOnly.Marginal<Dirichlet[][][]>(CPT_Dialogue.NameInGeneratedCode);
            var result = iaRelOnly.Marginal<Discrete[]>(Dialogue.NameInGeneratedCode);
 

            // replace probability table
            dialogueProbability = result[0].GetProbs().ToList();
        }

        Debug.Log("avg of probabilities (checking if panay 0) is " + dialogueProbability.Average());
    }
    #endregion

    #region UTILITY

    public double GetProperWeight(DialogueLine dl, int mood)
    {
        // return neutral
        if (mood >= (int)DirectorConstants.MoodThreshold.GOOD)
        {
            return dl.posWeight - 0.75; // 1.25
        }
        else if (mood <= (int)DirectorConstants.MoodThreshold.BAD)
        {
            return dl.negWeight - 0.75; // 1.25
        }

        // no weight
        return 1;
    }

    /// <summary>
    /// Computes the expected utility of one dialogue line
    /// </summary>
    /// <param name="lineProbability">
    ///     DialogueLine                => key
    ///     float / probabiloty of line => value
    /// </param>
    /// <returns></returns>
    public double ComputeExpectedUtility(int dialogueKey, double probability, Dictionary<string, double> topicList, int mood)
    {
        // we get the line in our db
        DialogueLine lineContainer = Director.LineDB[dialogueKey];

        // if EndConversation is CLOSED but the current line's topic is EndConversation topic only, return 0 expected utility.
        if(lineContainer.relatedTopics.Count()==1 && 
            lineContainer.relatedTopics[0]==DirectorConstants.TOPIC_END_CONVO &&
            topicList[DirectorConstants.TOPIC_END_CONVO] == DirectorConstants.TOPIC_RELEVANCE_CLOSE)
        {
            return 0;
        }

        // if StartConversation is NOT prio but the current line's topic is StartConversation topic only, return 0 expected utility.
        if (lineContainer.relatedTopics.Count() == 1 &&
            lineContainer.relatedTopics[0] == DirectorConstants.TOPIC_START_CONVO &&
            topicList[DirectorConstants.TOPIC_START_CONVO] != DirectorConstants.TOPIC_RELEVANCE_PRIO)
        {
            return 0;
        }

        if(topicList[DirectorConstants.TOPIC_START_CONVO] == DirectorConstants.TOPIC_RELEVANCE_PRIO &&
            !lineContainer.relatedTopics.Contains(DirectorConstants.TOPIC_START_CONVO))
        {
            return 0;
        }

        /*
         *  TONE WEIGHT CALCULATION
         */
        // we create a list to keep track of each p(line) * utility
        List<double> utilVals = new List<double>()
            {
                // probability of DLine * weight of line
                probability * GetProperWeight(lineContainer, mood)
            };

        /*
         *  TOPIC RELEVANCE CALCULATION
         *      we multiply the initial overall relevance with the relevance of the topic.
         *      If there are multipe topics with the same x1 relevance, they just have x1 relevance pa rin.
         */

        double overallRelevance = 1;

        if (lineContainer.relatedTopics == null)
        {
            overallRelevance = overallRelevance * topicList[DirectorConstants.NONE_STR];
        }
        else
        {
            // get probability with the related topics.
            foreach (string topic in lineContainer.relatedTopics)
            {
                if (topic == "")
                {
                    overallRelevance = overallRelevance * topicList[DirectorConstants.NONE_STR];
                }
                else
                {
                    overallRelevance = overallRelevance * topicList[topic];
                }
            }
        }

        // add to utility the modified probability considering the relevance of its related topics
        utilVals.Add(probability * overallRelevance);

        /*
         *  LINE IS SAID CALCULATION
         */

        if (lineContainer.isSaid)
        {
            // it's already said, we multiple line probability with is said true weight
            utilVals.Add(probability * LINE_IS_SAID_WEIGHT_T);
        }
        else
        {
            utilVals.Add(probability * LINE_IS_SAID_WEIGHT_F);
        }

        return utilVals.Sum();
    }

    /// <summary>
    /// Filters lines depending on current active npc -- npc is talking, the receiver is going to be no_receiver
    /// </summary>
    /// <param name="probabilities"></param>
    /// <returns></returns>
    public List<double> FilterLines(List<double> probabilities, string receiver="no_receiver", string currentMap="", string currentSpeaker="player")
    {
        // if our receiver is no_receiver (default), it means that any_receiver is also not valid.

        Debug.Log("Filtering setting the probabilities of dialogue lines to 0 if receiver is not: " + receiver);
        foreach(KeyValuePair<int, DialogueLine> line in Director.LineDB)
        {
            // filter out speaker id
            if(line.Value.speakerId != currentSpeaker)
            {
                Debug.Log("line " + line.Key + " is filtered out, current speaker is " + currentSpeaker +" and not " + line.Value.speakerId);
                probabilities[line.Key] = 0;
            }

            // filters by current map, assuming that the current line has any location restrictions.
            if(currentMap!="" && line.Value.locations.Length > 0 && !line.Value.locations.Contains(currentMap))
            {
                probabilities[line.Key] = 0;
                Debug.Log("The line: ( " + line.Value.dialogue + " ) has prob 0 because the location " + currentMap + "is not in its allowed locations");
            }

            // assuming that our line isn't no_receiver (meaning it's an npc line), we consider both receiver value and
            // any receiver to be a valid option.
            if(receiver != "no_receiver" && (line.Value.receiver != receiver && line.Value.receiver != "any_receiver"))
            {
                //Debug.Log("the receiver of this line: " + line.Value.receiver);
                //Debug.Log("filtering those that aren't " + receiver + " or any_receiver");
                //Debug.Log("filtered the line: " + line.Value.dialogue);
                // if the receiver isn't the same, we set the probability to 0.
                probabilities[line.Key] = 0;
            }
            else if(receiver == "no_receiver" && line.Value.receiver != receiver)
            {
                //Debug.Log("the receiver of this line: " + line.Value.receiver + " it's invalid, for this situation");
                probabilities[line.Key] = 0;
            }
            else
            {
                Debug.Log("line: " + line.Key + " is not filtered out");
            }
        }

        return probabilities;
    }

    /// <summary>
    /// Returns the index of the line with the highest utility.
    /// </summary>
    /// <returns></returns>
    public KeyValuePair<int, double> LineWithBestUtil(
        List<double> probabilities, 
        Dictionary<string, double> topicList, 
        int mood, 
        string currentMap,
        string currentSpeaker,
        string receiver = "no_receiver")
    {
        double highestUtil = 0;
        int bestDialogue = -1;

        Debug.Log("average of line probabilities in util function: " + probabilities.Average());

        // filter
        List<double> filtered_probs = FilterLines(probabilities, receiver, currentMap, currentSpeaker);

        Debug.Log("Probabilities acquired: " + probabilities.Count);
        Debug.Log("Number of lines total: " + TotalDialogueCount);
        
        
        for(int i=0;i<probabilities.Count; i++)
        {
            
            Debug.Log($"Line {i}, probability: {probabilities[i]}");
            Debug.Log("Line proper: " + Director.LineDB[i].dialogue);
            /*
             *  WE ADD THE FF UTILITIES
             *  - u(the related topic of a given line i)
             *  - u(mood weight of given line i)
             *  - u(line is said?)
             */
            // we add each utility:
            double computedUtility = ComputeExpectedUtility(i, probabilities[i], topicList, mood);
           Debug.Log("Computed util for line " + i + " is: " + computedUtility);
            
            // comparing best dialogue
            if(bestDialogue == -1)
            {
                // this is for if no dialogue is found yet
                bestDialogue = i;
                highestUtil = computedUtility;
            }
            else
            {
                // if highest utility is lower than our computed value, then we replace the dialogue and the highest utility
                if (highestUtil < computedUtility)
                {
                    Debug.Log("Computed util for line " + i + " is: " + computedUtility + " and greater than former util: "+highestUtil);
                    bestDialogue = i;
                    highestUtil = computedUtility;
                }
            }
        }

        if (bestDialogue == -1)
        {
            // something went wrong
            Debug.LogWarning("Not able to find a best dialogue. Returning default.");
            return new KeyValuePair<int, double>(0, 0.0);
        }

        Debug.Log("Highest utility is: " + highestUtil + " the line is: "+bestDialogue);
        /*
        debugProbStr += $"SELECTED LINE: {bestDialogue}\n" +
            $"ORIGINAL PROBABILITY:{probabilities[bestDialogue]}\n" +
            $"UTILITY: {highestUtil}\n" +
            $"=================\n";

        EventHandler.Instance.UpdateDebugDisplay(new string[] { debugProbStr });*/

        return new KeyValuePair<int, double>(bestDialogue, highestUtil);
    }
    #endregion

    #region LINE SELECTION

    /// <summary>
    /// Selecting NPC line
    /// </summary>
    /// <returns></returns>
    public int SelectNPCLine(
        int knownTrait,         // the trait is optional, not all chars have it -- an optional trait is -1
        int knownRel,           //  not optional.
        Dictionary<string, double> topicList,
        int currentMood,
        string map,                 //  current map we are in
        string activeArchetype,     // the current active speaker (speaker id form)
        List<double> probsToUse     // the acquired probability of the npc after running DialogueProbabilities()
        )
    {
        
        return LineWithBestUtil(
            new List<double>(probsToUse),
            topicList,
            currentMood,
            map,
            activeArchetype).Key;
    }

    /// <summary>
    /// Selecting player line from our knowledge.
    /// </summary>
    /// <param name="knownEvents"></param>
    /// <param name="knownTrait"></param>
    /// <param name="knownRel"></param>
    /// <param name="data"></param>
    /// <param name="topicList"></param>
    /// <param name="currentMood"></param>
    /// <param name="receiverArchetype">the ARCHETYPE of the active npc</param>
    /// <returns></returns>
    public int[] SelectPlayerLines(
        int knownTrait,
        int knownRel,
        Dictionary<string, double> topicList,
        int currentMood,                        // mood value
        string map,                             // map or scene we are in
        string receiverArchetype,                // speaker archetype of who the player is talking to
        List<double> probsToUse                 // the probability to use after using DialogueProbability()
        )
    {
        double minProb = 0.0;

        // create a deep copy of the probability to use
        List<double> linePosteriors = new List<double>(probsToUse);

        Debug.Log("average line probabilities: " + linePosteriors.Average());

        // get the top 3 lines
        Dictionary<int, double> best3 = new Dictionary<int, double>();
        for(int i = 0; i < 3; i++)
        {
            Debug.Log("getting line number: " + i);

            KeyValuePair<int, double> best = LineWithBestUtil(linePosteriors, topicList, currentMood, map, DirectorConstants.PLAYER_STR, receiverArchetype);

            // we base our minimum probability on our first "best" line
            if (i == 0)
            {
                minProb = best.Value - (best.Value * 0.25);
            }

            best3.Add(best.Key, best.Value);

            // we "delete" the posterior of the best, by setting it to 0 -- which means that all calc will result in this being 0
            linePosteriors[best.Key] = 0;
        }
        
        // return all lines greater than the minimum probability.
        // we also have to consider the line minimum aside from the minimum based on the first best value
        // if the line's probability is less than the minimum probability calculated from the 1st best and the line itself is less than our hard minimum, we
        // won't show it.
        return best3.Keys.Where(id => best3.Values.Where(prob => prob >= minProb && prob >= LINE_HARD_MIN_PROB).Contains(best3[id])).ToArray();
    }


    #endregion


}

// DATA FOR THE MODEL (non variable)
public class DirectorData
{
    // used for priors / recent posteriors
    public Dirichlet eventsProb;
    public Dirichlet traitsProb;
    public Dirichlet relProb;
    // dialogue line
    public Dirichlet[][][] dialogueProb;
    
}
