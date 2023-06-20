using Microsoft.ML.Probabilistic;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Math;
using Microsoft.ML.Probabilistic.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BASE CLASS OF THE DIRECTOR MODEL -- INCLUDES ALL ATTRIBUTES THAT OCCUR IN BOTH TRAINING IMPLEMENTATION AND IN-GAME W/C USES THE TRAINED PRIORS
/// </summary>

public class DirectorBase
{

    protected int TotalEventCount;
    protected int TotalTraitCount;
    protected int TotalRelCount;
    protected int TotalDialogueCount;

    protected string debugProbStr = "";

    // infer
    public InferenceEngine engine = new InferenceEngine();
    public IGeneratedAlgorithm ia;

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
}
