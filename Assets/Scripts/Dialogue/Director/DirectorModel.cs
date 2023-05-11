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
 *  MODIFICATIONS TO THIS MODEL
 *  [] Director Data (OUTSIDE OF CLASS
 *      - serves to hold the Dirichlet priors
 *      - samples basically mean,
 *          - if we have n events that occurred, we have an array of numbers or ints (the event ids) representing all these events
 *          - given we have n events that occurred and only talking to one character, each character has 1 trait each, and thus
 *          the sample for our traits has length of n, all of which are the same trait.
 *          - same above for relationship.
 *          - this is also what number of samples represent (length of events known)
 *  [] LearnParameters
 *      [/] From Uniform
 *          - Called at the beginning.
 *          - Parameters:
 *              - int[] knownEvents
 *              - int[] charTrait       // a single trait
 *              - int[] currRel         // with char
 *              - int[] dialogue        // dialogue that we know is already said???
 *          - return DirectorData
 *      [] (NOT SURE IF NEEDED) Inferring Posteriors
 *          - Reference: LearnParameters() the non uniform version
 *          - parameters
 *              - yung sa prev check, PLUS:
 *              - Dirichlet probKnownEvents
 *              - Dirichlet probTrait
 *              - Dirichlet probRelStatus
 *          - return value DirectorData
 *  [] Infer Dialogue
 *      - method params:
 *          - int[] knownEvents
 *          - int[] charTrait       // a single trait
 *          - int[] currRel         // with char
 *          - int[] dialogue        // dialogue that we know is already said???
 *          - DirectorData priors
 *      - Reference: ProbRain method in tutorial
 *      - STEPS
 *          - set our observed values from the int[] arrays
 *          - Infer priors & set it
 *              - traits
 *              - relstatus
 *              - knownevents
 *          - Clear dialogue observed value
 *          - Infer the posterior of dialogue.
 *      
 *      [] How to call in Director class
 *          
 * 
 */
public class DirectorModel
{
    private const double LINE_IS_SAID_WEIGHT_T = 0.2;
    private const double LINE_IS_SAID_WEIGHT_F = 1.0;

    private const string DLINE_DISTRIBUTION_PATH = "Data/XML/Dialogue/lineCPT.xml";

    private int TotalEventCount;
    private int TotalTraitCount;
    private int TotalRelCount;
    private int TotalDialogueCount;
    
    // infer
    protected InferenceEngine engine = new InferenceEngine();

    // we always ded
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
    public DirectorModel(int totalEvents, int totalTraits, int totalDialogue, int totalRelStatus)
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
    /// Called before any interaction.
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

        if(importedDLineDist == null)
        {
            // Dialogue
            // relcount goes first because it's the outermost value
            //UNIFORM IF WE DON'T HAVE A PREDEFINED DISTRIBUTION
            Dirichlet[] parent1 = Enumerable.Repeat(Dirichlet.Uniform(TotalDialogueCount), TotalRelCount).ToArray();
            Dirichlet[][] parent2 = Enumerable.Repeat(parent1, TotalTraitCount).ToArray();
            data.dialogueProb = Enumerable.Repeat(parent2, TotalEventCount).ToArray();
        }
        else
        {
            data.dialogueProb = importedDLineDist;
        }

        return data;
    }


    /// <summary>
    /// returns a directordata that is based on our previously acquired posterior
    /// We call this every time BEFORE we get a new line.
    /// </summary>
    /// <returns></returns>
    public DirectorData SetData()
    {

        // setting our probabilities in director data
        // if all of the posteriors are null (meaning 1st tym), we set the parents to uniform
        if(ProbPost_Events == null && ProbPost_Traits == null && ProbPost_RelStatus == null)
        {
            return UniformDirectorData();
        }

        // otherwise, we set the posteriors
        return new DirectorData
        {
            eventsProb = ProbPost_Events,
            traitsProb = ProbPost_Traits,
            relProb = ProbPost_RelStatus
        };
    }

    /// <summary>
    /// Called during runtime / in-game, we deserialize our line distribution xml and place it into our importedDLineDist variable.
    /// This will be used in our initial setting of data
    /// </summary>
    public void Start()
    {
        DataContractSerializer serializer = new DataContractSerializer(typeof(Dirichlet), new DataContractSerializerSettings { DataContractResolver = new InferDataContractResolver() });
        
        using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(new FileStream(DLINE_DISTRIBUTION_PATH, FileMode.Open), new XmlDictionaryReaderQuotas()))
        {
            // deserialize/ read the distribution
            importedDLineDist = (Dirichlet[][][])serializer.ReadObject(reader);

            // test write???
        }
    }

    #endregion

    #region INFERENCE

    /// <summary>
    /// We learn our cpt values given known outcomes or samples -- our line db
    /// </summary>
    /// <param name="dialogueSample"></param>
    /// <param name="eventSample"></param>
    /// <param name="traitSample"></param>
    /// <param name="relSample"></param>
    /// <param name="priors"></param>
    /// <returns></returns>
    public Dirichlet[][][] LearnDialogueCPT(
        int[] dialogueSample,
        int[] eventSample,
        int[] traitSample,
        int[] relSample,
        DirectorData priors)
    {
        NumOfCases.ObservedValue = dialogueSample.Length;

        // setting our observed
        Dialogue.ObservedValue = dialogueSample;
        Events.ObservedValue = eventSample;
        Traits.ObservedValue = traitSample;
        RelStatus.ObservedValue = relSample;

        // set our priors
        CPTPrior_Dialogue.ObservedValue = priors.dialogueProb;
        ProbPrior_Events.ObservedValue = priors.eventsProb;
        ProbPrior_Traits.ObservedValue = priors.traitsProb;
        ProbPrior_RelStatus.ObservedValue = priors.relProb;

        // make inference
        return engine.Infer<Dirichlet[][][]>(Dialogue);
    }

    /// <summary>
    /// We infer the probabilities of the parents given the data we know
    /// </summary>
    /// <param name="knownEvents"> All events that have occurred globally, in map, and in memory. </param>
    /// <param name="knownTraits"> The trait of the character, same value across whole arr </param>
    /// <param name="knownRel"> Rel of player with character, same value across all elements of arr </param>
    /// <param name="priors"> DirectorData that we have set as uniform or acquired from recent posteriors </param>
    /// <returns></returns>
    public void LearnParameters(int[] knownEvents, int[] knownTraits, int[] knownRel, DirectorData priors)
    {
        NumOfCases.ObservedValue = knownEvents.Length;

        // setting our parent observed values
        Events.ObservedValue = knownEvents;

        if (knownTraits == null)
        {
            // clear the traits
            Traits.ClearObservedValue();
        }
        else
        {
            Traits.ObservedValue = knownTraits;
        }

        RelStatus.ObservedValue = knownRel;

        // setting the priors
        ProbPrior_Events.ObservedValue = priors.eventsProb;
        ProbPrior_Traits.ObservedValue = priors.traitsProb;
        ProbPrior_RelStatus.ObservedValue = priors.relProb;
        CPTPrior_Dialogue.ObservedValue = priors.dialogueProb;

        // inferring the posteriors
        ProbPost_Events = engine.Infer<Dirichlet>(Prob_Events);
        ProbPost_Traits = engine.Infer<Dirichlet>(Prob_Traits);
        ProbPost_RelStatus = engine.Infer<Dirichlet>(Prob_RelStatus);
    }

    /// <summary>
    /// Infers the appropriate dialogue line (the probabilities)
    /// </summary>
    public Discrete[] InferDialogueFromPosteriors(
        int[] events,
        int[] traits,
        int[] rels,
        DirectorData priors)
    {
        // we first learn the posteriors of events, traits, and relationships to use them as priors in inferring dialouge
        LearnParameters(events, traits, rels, priors);

        Dialogue.ClearObservedValue();

        ProbPrior_Events.ObservedValue = ProbPost_Events;
        ProbPrior_Traits.ObservedValue = ProbPost_Traits;
        ProbPrior_RelStatus.ObservedValue = ProbPost_RelStatus;
        // no need to set cptprior_dialogue here bc it's done in learnParameters.

        // inference
        var dialogueInference = engine.Infer<Discrete[]>(Dialogue);
        Debug.Log("inferred: " + dialogueInference.Length);

        return dialogueInference;
    }

    /// <summary>
    /// After making an inference, we convert the discrete into a list of doubles (probabilities) to process in util func
    /// </summary>
    /// <param name="events"></param>
    /// <param name="traits"></param>
    /// <param name="rels"></param>
    /// <param name="priors"></param>
    /// <returns></returns>
    public List<double> GetDialogueProbabilities(
        int[] events,
        int[] traits,
        int[] rels,
        DirectorData priors)
    {
        // get inference
        Discrete[] dialogueInference = InferDialogueFromPosteriors(events, traits, rels, priors);

        // from discrete[], we get the probability as a double
        List<double> probs = new List<double>();
        for (int i = 0; i < dialogueInference.Length; i++)
        {
            // get the probability of the dialogue represented by i index
            probs.Add(dialogueInference[i].GetProbs()[i]);
        }

        return probs;
    }

    #endregion

    #region UTILITY

    public int GetProperWeight(DialogueLine dl, int mood)
    {
        // return neutral
        if (mood >= (int)MoodThreshold.GOOD)
        {
            return dl.posWeight;
        }
        else if (mood <= (int)MoodThreshold.BAD)
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
    public double ComputeExpectedUtility(int dialogueKey, double probability, Dictionary<string, float> topicList, int mood)
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
         */
        // get probability with the related topics.
        foreach (string topic in lineContainer.relatedTopics)
        {
            // accessing the current topic in the topic relevance tracker and mul with the prob value of the line
            utilVals.Add(probability * (double)topicList[topic]);
        }

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
    /// Returns the index of the line with the highest utility.
    /// </summary>
    /// <returns></returns>
    public int LineWithBestUtil(List<double> probabilities, Dictionary<string, float> topicList, int mood)
    {
        double highestUtil = 0;
        int bestDialogue = -1;

        Debug.Log("Probabilities acquired: " + probabilities.Count);
        Debug.Log("Number of lines total: " + TotalDialogueCount);

        for(int i=0;i<probabilities.Count; i++)
        {
            Debug.Log($"Line {i}, probability: {probabilities[i]}");
            /*
             *  WE ADD THE FF UTILITIES
             *  - u(the related topic of a given line i)
             *  - u(mood weight of given line i)
             *  - u(line is said?)
             */
            // we add each utility:
            double computedUtility = ComputeExpectedUtility(i, probabilities[i], topicList, mood);
            
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
                    bestDialogue = i;
                    highestUtil = computedUtility;
                }
            }
        }

        if (bestDialogue == -1)
        {
            // something went wrong
            Debug.LogWarning("Not able to find a best dialogue. Returning default.");
            return 0;
        }

        return bestDialogue;
    }
    #endregion

    #region LINE SELECTION

    /// <summary>
    /// Selecting NPC line
    /// </summary>
    /// <returns></returns>
    public int SelectNPCLine(
        int[] knownEvents,
        int knownTrait,
        int knownRel,
        DirectorData data,
        Dictionary<string, float> topicList,
        int currentMood)
    {
        // testing if we passed correct info:
        Debug.Log($"num of known events: {knownEvents.Length}\n" +
            $"trait: {knownTrait}\n" +
            $"relationship: {knownRel}");

        // array of the same observations, with same length as known events
        int[] traitsArr = new int[knownEvents.Length];
        int[] relArr = new int[knownEvents.Length];

        for(int i = 0; i < knownEvents.Length; i++)
        {
            // populate the array
            if (knownTrait != -1)
            {
                traitsArr[i] = knownTrait;
            }
            relArr[i] = knownRel;
        }
        
        return LineWithBestUtil(
            GetDialogueProbabilities(knownEvents, traitsArr, relArr, data),
            topicList,
            currentMood);
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
    /// <returns></returns>
    public int[] SelectPlayerLines(
        int[] knownEvents,
        int knownTrait,
        int knownRel,
        DirectorData data,
        Dictionary<string, float> topicList,
        int currentMood)
    {

        // testing if we passed correct info:
        Debug.Log($"num of known events: {knownEvents.Length}\n" +
            $"trait: {knownTrait}\n" +
            $"relationship: {knownRel}");

        // array of the same observations, with same length as known events
        int[] traitsArr = new int[knownEvents.Length];
        int[] relArr = new int[knownEvents.Length];

        for (int i = 0; i < knownEvents.Length; i++)
        {
            // populate the array
            // known trait is optional -- some characters have no traits
            if (knownTrait != -1)
            {
                // if knowntrait is not empty (-1) then we set it to whatever we passed.
                traitsArr[i] = knownTrait;
            }
            relArr[i] = knownRel;
        }

        // we infer our posteriors first.
        List<double> linePosteriors = GetDialogueProbabilities(knownEvents, traitsArr, relArr, data);

        int[] best3 = new int[3];
        for(int i = 0; i < 3; i++)
        {
            int best = LineWithBestUtil(linePosteriors, topicList, currentMood);

            // we add best to our best 3
            best3[i] = best;

            // we "delete" the posterior of the best, by setting it to 0 -- which means that all calc will result in this being 0
            linePosteriors[best] = 0;
        }

        return best3;
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
