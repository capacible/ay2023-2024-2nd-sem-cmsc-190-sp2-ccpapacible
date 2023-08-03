using Microsoft.ML.Probabilistic;
using Microsoft.ML.Probabilistic.Collections;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Math;
using Microsoft.ML.Probabilistic.Models;
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
    private const double LINE_IS_SAID_WEIGHT_T = -1.0;
    private const double LINE_IS_SAID_WEIGHT_F = 1.0;

    private const double LINE_HARD_MIN_PROB = 0.005;
    
    private static readonly string DLINE_DISTRIBUTION_PATH = "XMLs/Dialogue/lineCPT";
    private static readonly string EVENTS_DISTRIBUTION_PATH = Application.dataPath + "/Data/XML/Dialogue/eventCPT.xml";
    private static readonly string TRAITS_DISTRIBUTION_PATH = Application.dataPath + "/Data/XML/Dialogue/traitCPT.xml";
    private static readonly string RELS_DISTRIBUTION_PATH = Application.dataPath + "/Data/XML/Dialogue/relCPT.xml";
    private static readonly string MODEL_FOLDER = $"{Application.dataPath}/Models";

    private int TotalEventCount;
    private int TotalTraitCount;
    private int TotalRelCount;
    private int TotalDialogueCount;

    //private string debugProbStr = "";
    private string modelFile;
    private string metaFile;

    // infer
    protected InferenceEngine engine = new InferenceEngine();
    private IGeneratedAlgorithm ia;

    private IGeneratedAlgorithm iaEventsRelKnown;
    private IGeneratedAlgorithm iaTraitsRelKnown;
    private IGeneratedAlgorithm iaAllKnown;
    
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
    private Dirichlet[][][] ProbPost_Dialogue;

    private Dirichlet[][][] importedDLineDist;


    #region INITIALIZATION
    /// <summary>
    /// Creating and instantiating the model
    /// </summary>
    public DirectorModel(int totalEvents, int totalTraits, int totalDialogue, int totalRelStatus, string modelName="DialogueDirector")
    {
        engine.ModelName = modelName;
        // set location of generated source code
        //engine.Compiler.GeneratedSourceFolder = MODEL_FOLDER;

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
        modelFile = Path.Combine(Application.dataPath, engine.ModelName);// delete meta file
        string meta = modelFile.Split('.')[0];
        metaFile = meta;

        CPTPrior_Dialogue.ObservedValue = DeserializeCPT<Dirichlet[][][]>(DLINE_DISTRIBUTION_PATH);
        ProbPrior_Events.ObservedValue = Dirichlet.Uniform(TotalEventCount);
        ProbPrior_Traits.ObservedValue = Dirichlet.Uniform(TotalTraitCount);
        ProbPrior_RelStatus.ObservedValue = Dirichlet.Uniform(TotalRelCount);

        // setting observed values.
        /*
         *  THERE ARE DIFFERENT TYPES OF POSSIBLE COMPILED ALGORITHMS, CONSIDERING THE VARIOUS TYPES OF AVAILABLE OR KNOWN DATA.
         */
         
        int[] events = new int[] { Director.NumKeyLookUp(DirectorConstants.GAME_IS_ACTIVE, fromEvents: true) };
        int[] traits = new int[] { Director.NumKeyLookUp(DirectorConstants.NONE_STR, fromTraits: true) };
        int[] rels = new int[] { (int)DirectorConstants.REL_STATUS_NUMS.NEUTRAL };
        NumOfCases.ObservedValue = 1;

        Debug.Log("Getting the inference algorithms...");

        SetPreInferenceObservations(events, null, rels);    // we set the observed value
        // create the algo for this one
        iaEventsRelKnown = engine.GetCompiledInferenceAlgorithm(Dialogue);

        // for traits only
        SetPreInferenceObservations(null, traits, rels);
        iaTraitsRelKnown = engine.GetCompiledInferenceAlgorithm(Dialogue);

        // for all applicable
        SetPreInferenceObservations(events, traits, rels);
        iaAllKnown = engine.GetCompiledInferenceAlgorithm(Dialogue);

        Debug.Log("All inferences algo loaded successfully");

    }
    
    /// <summary>
    /// Deserializes an XML file in a given path
    /// </summary>
    /// <typeparam name="T"> type to deserialize into </typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T DeserializeCPT<T>(string path)
    {
        if(path.Contains("CPT") || path.Contains(".xml"))
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T), new DataContractSerializerSettings { DataContractResolver = new InferDataContractResolver() });

            TextAsset cpt = (TextAsset) Resources.Load(path);

            using (var reader = XmlReader.Create(new StringReader(cpt.text)))
            {
                // deserialize/ read the distribution
                return (T)serializer.ReadObject(reader);
            }
        }

        Debug.LogWarning("Invalid path.");
        return default;
    }

    #endregion
    #region BEFORE-RUNNING

    public void GenerateAlgorithms(int gameActive, int noneStr)
    {
        modelFile = Path.Combine(Application.dataPath, engine.ModelName);// delete meta file
        string meta = modelFile.Split('.')[0];
        metaFile = meta;
        string path = "Assets/Resources/XMLs/Dialogue/lineCPT";

        DataContractSerializer serializer = new DataContractSerializer(typeof(Dirichlet[][][]), new DataContractSerializerSettings { DataContractResolver = new InferDataContractResolver() });

        using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(new FileStream(path, FileMode.Open), new XmlDictionaryReaderQuotas()))
        {
            // deserialize/ read the distribution
            CPTPrior_Dialogue.ObservedValue = (Dirichlet[][][])serializer.ReadObject(reader);
        }

        ProbPrior_Events.ObservedValue = Dirichlet.Uniform(TotalEventCount);
        ProbPrior_Traits.ObservedValue = Dirichlet.Uniform(TotalTraitCount);
        ProbPrior_RelStatus.ObservedValue = Dirichlet.Uniform(TotalRelCount);

        // setting observed values.
        /*
         *  THERE ARE DIFFERENT TYPES OF POSSIBLE COMPILED ALGORITHMS, CONSIDERING THE VARIOUS TYPES OF AVAILABLE OR KNOWN DATA.
         */

        int[] events = new int[] { gameActive };
        int[] traits = new int[] { noneStr };
        int[] rels = new int[] { (int)DirectorConstants.REL_STATUS_NUMS.NEUTRAL };
        NumOfCases.ObservedValue = 1;

        Debug.Log("Getting the inference algorithms...");

        SetPreInferenceObservations(events, null, rels);    // we set the observed value
        // create the algo for this one
        iaEventsRelKnown = engine.GetCompiledInferenceAlgorithm(Dialogue);

        // for traits only
        SetPreInferenceObservations(null, traits, rels);
        iaTraitsRelKnown = engine.GetCompiledInferenceAlgorithm(Dialogue);

        // for all applicable
        SetPreInferenceObservations(events, traits, rels);
        iaAllKnown = engine.GetCompiledInferenceAlgorithm(Dialogue);

        Debug.Log("All inferences algo loaded successfully");
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
    /// </summary>
    /// <param name="events"></param>
    /// <param name="traits"></param>
    /// <param name="rels"></param>
    /// <returns></returns>
    public List<double> DialogueProbabilities(
        int[] events,
        int[] traits,
        int[] rels)
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

        // set observed considering w/c are true.
        if(knowEv && knowTrait && knowRel)
        {
            iaAllKnown.Reset();
            // use iaAll
            iaAllKnown.SetObservedValue(NumOfCases.NameInGeneratedCode, events.Length);
            iaAllKnown.SetObservedValue(Events.NameInGeneratedCode, events);
            iaAllKnown.SetObservedValue(Traits.NameInGeneratedCode, traits);
            iaAllKnown.SetObservedValue(RelStatus.NameInGeneratedCode, rels);

            // update and get prob
            iaAllKnown.Update(1);
            var result = iaAllKnown.Marginal<Discrete[]>(Dialogue.NameInGeneratedCode);

            return result[0].GetProbs().ToList();
        }
        else if(knowEv)
        {
            iaEventsRelKnown.Reset();
            // for only event and rel known
            iaEventsRelKnown.SetObservedValue(NumOfCases.NameInGeneratedCode, events.Length);
            iaEventsRelKnown.SetObservedValue(Events.NameInGeneratedCode, events);
            iaEventsRelKnown.SetObservedValue(RelStatus.NameInGeneratedCode, rels);

            // update and get prob
            iaEventsRelKnown.Update(1);
            var result = iaEventsRelKnown.Marginal<Discrete[]>(Dialogue.NameInGeneratedCode);

            return result[0].GetProbs().ToList();
        }
        else if (knowTrait)
        {
            iaTraitsRelKnown.Reset();

            iaTraitsRelKnown.SetObservedValue(NumOfCases.NameInGeneratedCode, traits.Length);
            iaTraitsRelKnown.SetObservedValue(Traits.NameInGeneratedCode, traits);
            iaTraitsRelKnown.SetObservedValue(RelStatus.NameInGeneratedCode, rels);

            // update and get prob
            iaTraitsRelKnown.Update(1);
            var result = iaTraitsRelKnown.Marginal<Discrete[]>(Dialogue.NameInGeneratedCode);

            return result[0].GetProbs().ToList();
        }

        Debug.LogError("OBSERVED VALUE ERROR: all observed values are null");
        return new List<double>() { 0.0 };
    }

    #endregion

    #region UTILITY

    public int GetProperWeight(DialogueLine dl, int mood)
    {
        // return neutral
        if (mood >= (int)DirectorConstants.MoodThreshold.GOOD)
        {
            return dl.posWeight;
        }
        else if (mood <= (int)DirectorConstants.MoodThreshold.BAD)
        {
            return dl.negWeight;
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
                Debug.Log("the receiver of this line: " + line.Value.receiver);
                Debug.Log("filtering those that aren't " + receiver + " or any_receiver");
                Debug.Log("filtered the line: " + line.Value.dialogue);
                // if the receiver isn't the same, we set the probability to 0.
                probabilities[line.Key] = 0;
            }
            else if(receiver == "no_receiver" && line.Value.receiver != receiver)
            {
                Debug.Log("the receiver of this line: " + line.Value.receiver + " it's invalid, for this situation");
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

        // filter
        probabilities = FilterLines(probabilities, receiver, currentMap, currentSpeaker);

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

        Debug.Log("Highest utility is: " + highestUtil);
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
        int[] knownEvents,
        int knownTrait,         // the trait is optional, not all chars have it -- an optional trait is -1
        int knownRel,           //  not optional.
        Dictionary<string, double> topicList,
        int currentMood,
        string map,                 //  current map we are in
        string activeArchetype     // the current active speaker (speaker id form)
        )
    {

        //debugProbStr = "====== NPC ======\n";
        //NumOfCases.ClearObservedValue();

        // default values if known events is empty
        int knownEventsCount = 1;
        // here we check if known events is null or empty; if not, we modify our count on known events to be used in populating the traits and rels array.
        if (knownEvents != null)
        {
            knownEventsCount = knownEvents.Length;
            //NumOfCases.ObservedValue = knownEvents.Length;

            // testing if we passed correct info:S
            Debug.Log($"num of known events: {knownEvents.Length}\n" +
                $"trait: {knownTrait}\n" +
                $"relationship: {knownRel}");

        }

        // array of the same observations, with same length as known events
        int[] traitsArr = new int[knownEventsCount];
        int[] relArr = new int[knownEventsCount];

        if (knownTrait == -1)
        {
            traitsArr = null;
        }

        for(int i = 0; i < knownEventsCount; i++)
        {
            // populate the array; if the trait isn't a known trait or negative we set traitsarr to be the key for "none"
            if (traitsArr != null)
            {
                traitsArr[i] = knownTrait;
            }

            relArr[i] = knownRel;
        }
        
        return LineWithBestUtil(
            DialogueProbabilities(knownEvents, traitsArr, relArr),
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
        int[] knownEvents,
        int knownTrait,
        int knownRel,
        Dictionary<string, double> topicList,
        int currentMood,                        // mood value
        string map,                             // map or scene we are in
        string receiverArchetype                // speaker archetype of who the player is talking to
        )
    {
        double minProb = 0.0;
        //debugProbStr += "====== PLAYER ======\n";

        //NumOfCases.ObservedValue = knownEvents.Length;

        // testing if we passed correct info:
        Debug.Log($"num of known events: {knownEvents.Length}\n" +
            $"trait: {knownTrait}\n" +
            $"relationship: {knownRel}");

        // array of the same observations, with same length as known events
        int[] traitsArr = new int[knownEvents.Length];
        int[] relArr = new int[knownEvents.Length];

        if (knownTrait == -1)
        {
            traitsArr = null;
        }


        for (int i = 0; i < knownEvents.Length; i++)
        {
            // populate the array
            // known trait is optional -- some characters have no traits
            if (traitsArr != null)
            {
                // if knowntrait is not empty (-1) then we set it to whatever we passed.
                traitsArr[i] = knownTrait;
            }
            relArr[i] = knownRel;
        }

        // we infer our posteriors first.
        List<double> linePosteriors = DialogueProbabilities(knownEvents, traitsArr, relArr);

        // get the top 3 lines
        Dictionary<int, double> best3 = new Dictionary<int, double>();
        for(int i = 0; i < 3; i++)
        {
            KeyValuePair<int, double> best = LineWithBestUtil(linePosteriors, topicList, currentMood, map, DirectorConstants.PLAYER_STR, receiverArchetype);

            // we base our minimum probability on our first "best" line
            if (i == 0)
            {
                minProb = best.Value - (best.Value * 0.75);
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
