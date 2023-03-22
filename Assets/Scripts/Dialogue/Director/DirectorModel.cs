using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Math;
using Microsoft.ML.Probabilistic.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// BASE CLASS

// TO LEARN:
// add child from one/two parents.
// CPT is used to set 
// MIXTURE MODEL ata yung hinahanap ko for events and traits na may halo halo
public abstract class DirectorModel
{
    
    // infer
    protected InferenceEngine engine = new InferenceEngine();

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

    protected DirectorData recentPosteriors;

    /// <summary>
    /// Creating and instantiating the model
    ///     As the only purpose of this class is to be inherited, the following constructor is abstract.
    /// </summary>
    public DirectorModel(int totalEvents, int totalTraits, int totalDialogue, int totalRelStatus)
    {
        /*
         * CREATING THE RANGES
         */
        EventsRange = new Range(totalEvents);
        TraitsRange = new Range(totalTraits);
        DialogueRange = new Range(totalDialogue);
        RelStatusRange = new Range(totalRelStatus);

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

        // DIALOGUE CONDITIONED ON ALL 3 PARENTS
        CPTPrior_Dialogue = Variable.Array(Variable.Array(Variable.Array<Dirichlet>(EventsRange), TraitsRange), RelStatusRange).Named("DialoguePriors");
        // initialize
        CPT_Dialogue = Variable.Array(Variable.Array(Variable.Array<Vector>(EventsRange), TraitsRange), RelStatusRange).Named("CptDialogue");
        CPT_Dialogue[EventsRange][TraitsRange][RelStatusRange] = Variable<Vector>.Random(CPTPrior_Dialogue[EventsRange][TraitsRange][RelStatusRange]);
        // the vallues accepted of cpt dialogue is the range of dialogue
        CPT_Dialogue.SetValueRange(DialogueRange);


        /*
         * CREATING THE PRIMARY VARIABLES...
         */
        Events = Variable.Array<int>(EventsRange).Named("AllEvents");
        Traits = Variable.Array<int>(TraitsRange).Named("AllTraits");
        RelStatus = Variable.Array<int>(RelStatusRange).Named("AllRels");
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

        var dimension = events.Range;
        
        
        var child = Variable.Array<int>(dimension);

        using (Variable.ForEach(dimension))
        using (Variable.Switch(events[dimension]))
        using (Variable.Switch(traits[dimension]))
        using (Variable.Switch(rels[dimension]))
            // each instance of variable in child of range dimension will be associated w/ each variable in events/traits/rels
            child[dimension] = Variable.Discrete(cptDialogue[events[dimension]][traits[dimension]][rels[dimension]]);

        return child;
    }

    /// <summary>
    /// Sets the priors of our model to be based on data we know or have updated.
    /// </summary>
    /// <param name="d">Director data object</param>
    public virtual void SetDirectorData(DirectorData d)
    {
        ProbPrior_Events.ObservedValue = d.eventsProb;
        ProbPrior_Traits.ObservedValue = d.traitsProb;
        ProbPrior_RelStatus.ObservedValue = d.relProb;

        CPTPrior_Dialogue.ObservedValue = d.dialogueProb;
    }
    
}

// DATA FOR THE MODEL (non variable)
public class DirectorData
{
    // 
    public Dirichlet eventsProb;
    public Dirichlet traitsProb;
    public Dirichlet relProb;
    // dialogue line
    public Dirichlet[][][] dialogueProb;

    /// <summary>
    /// Sets the prior data to be uniform. This is done at the very beginning of the game
    /// </summary>
    /// <param name="allEventCount">Number of total events</param>
    /// <param name="allTraitsCount">Numebr of total traits</param>
    /// <param name="allRelCount">Number of total relationship statuses/types</param>
    /// <param name="allDialogueCount">Number of total lines.</param>
    public static DirectorData SetDataUniform(int allEventCount, int allTraitsCount, int allRelCount, int allDialogueCount)
    {
        DirectorData ret = new DirectorData();

        ret.eventsProb = Dirichlet.Uniform(allEventCount);
        ret.traitsProb = Dirichlet.Uniform(allTraitsCount);
        ret.relProb = Dirichlet.Uniform(allRelCount);

        // Dialogue
        Dirichlet[] parent1 = Enumerable.Repeat(Dirichlet.Uniform(allDialogueCount), allEventCount).ToArray();
        Dirichlet[][] parent2 = Enumerable.Repeat(parent1, allTraitsCount).ToArray();
        ret.dialogueProb = Enumerable.Repeat(parent2, allRelCount).ToArray();

        return ret;
    }
}
