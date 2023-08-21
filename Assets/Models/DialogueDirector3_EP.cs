// <auto-generated />
#pragma warning disable 1570, 1591

using System;
using Microsoft.ML.Probabilistic;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Math;
using Microsoft.ML.Probabilistic.Collections;
using Microsoft.ML.Probabilistic.Factors;

namespace Models
{
	/// <summary>
	/// Generated algorithm for performing inference.
	/// </summary>
	/// <remarks>
	/// If you wish to use this class directly, you must perform the following steps:
	/// 1) Create an instance of the class.
	/// 2) Set the value of any externally-set fields e.g. data, priors.
	/// 3) Call the Execute(numberOfIterations) method.
	/// 4) Use the XXXMarginal() methods to retrieve posterior marginals for different variables.
	/// 
	/// Generated by Infer.NET 0.4.2301.301 at 8:03 PM on Monday, August 21, 2023.
	/// </remarks>
	public partial class DialogueDirector3_EP : IGeneratedAlgorithm
	{
		#region Fields
		/// <summary>Field backing the AllTraits property</summary>
		private int[] AllTraits_field;
		/// <summary>True if Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors has executed. Set this to false to force re-execution of Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors</summary>
		public bool Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors_isDone;
		/// <summary>Message to marginal of 'DialogueChild'</summary>
		public DistributionRefArray<Discrete,int> DialogueChild_marginal_F;
		/// <summary>Message to marginal of 'DialogueCPT'</summary>
		public DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]> DialogueCPT_marginal_F;
		/// <summary>Field backing the DialogueCPTPrior property</summary>
		private Dirichlet[][][] DialogueCPTPrior_field;
		/// <summary>Field backing the EventsPriors property</summary>
		private Dirichlet EventsPriors_field;
		/// <summary>Field backing the NumberOfIterationsDone property</summary>
		private int numberOfIterationsDone;
		/// <summary>Field backing the NumOfCases property</summary>
		private int NumOfCases_field;
		/// <summary>Field backing the RelStatusPriors property</summary>
		private Dirichlet RelStatusPriors_field;
		/// <summary>Field backing the TraitsPriors property</summary>
		private Dirichlet TraitsPriors_field;
		#endregion

		#region Properties
		/// <summary>The externally-specified value of 'AllTraits'</summary>
		public int[] AllTraits
		{
			get {
				return this.AllTraits_field;
			}
			set {
				if ((value!=null)&&(value.Length!=this.NumOfCases)) {
					throw new ArgumentException(((("Provided array of length "+value.Length)+" when length ")+this.NumOfCases)+" was expected for variable \'AllTraits\'");
				}
				this.AllTraits_field = value;
				this.numberOfIterationsDone = 0;
				this.Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors_isDone = false;
			}
		}

		/// <summary>The externally-specified value of 'DialogueCPTPrior'</summary>
		public Dirichlet[][][] DialogueCPTPrior
		{
			get {
				return this.DialogueCPTPrior_field;
			}
			set {
				if ((value!=null)&&(value.Length!=215)) {
					throw new ArgumentException(((("Provided array of length "+value.Length)+" when length ")+215)+" was expected for variable \'DialogueCPTPrior\'");
				}
				this.DialogueCPTPrior_field = value;
				this.numberOfIterationsDone = 0;
				this.Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors_isDone = false;
			}
		}

		/// <summary>The externally-specified value of 'EventsPriors'</summary>
		public Dirichlet EventsPriors
		{
			get {
				return this.EventsPriors_field;
			}
			set {
				this.EventsPriors_field = value;
				this.numberOfIterationsDone = 0;
				this.Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors_isDone = false;
			}
		}

		/// <summary>The number of iterations done from the initial state</summary>
		public int NumberOfIterationsDone
		{
			get {
				return this.numberOfIterationsDone;
			}
		}

		/// <summary>The externally-specified value of 'NumOfCases'</summary>
		public int NumOfCases
		{
			get {
				return this.NumOfCases_field;
			}
			set {
				if (this.NumOfCases_field!=value) {
					this.NumOfCases_field = value;
					this.numberOfIterationsDone = 0;
					this.Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors_isDone = false;
				}
			}
		}

		/// <summary>The externally-specified value of 'RelStatusPriors'</summary>
		public Dirichlet RelStatusPriors
		{
			get {
				return this.RelStatusPriors_field;
			}
			set {
				this.RelStatusPriors_field = value;
				this.numberOfIterationsDone = 0;
				this.Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors_isDone = false;
			}
		}

		/// <summary>The externally-specified value of 'TraitsPriors'</summary>
		public Dirichlet TraitsPriors
		{
			get {
				return this.TraitsPriors_field;
			}
			set {
				this.TraitsPriors_field = value;
				this.numberOfIterationsDone = 0;
			}
		}

		#endregion

		#region Methods
		/// <summary>Computations that depend on the observed value of AllTraits and DialogueCPTPrior and EventsPriors and NumOfCases and RelStatusPriors</summary>
		private void Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors()
		{
			if (this.Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors_isDone) {
				return ;
			}
			DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]> DialogueCPT_F;
			// Create array for 'DialogueCPT' Forwards messages.
			DialogueCPT_F = new DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]>(215);
			DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]> DialogueCPT_use_B;
			// Create array for 'DialogueCPT_use' Backwards messages.
			DialogueCPT_use_B = new DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]>(215);
			// Create array for 'DialogueCPT_marginal' Forwards messages.
			this.DialogueCPT_marginal_F = new DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]>(215);
			// Create array for replicates of 'DialogueCPT_itemAllTraits_NCases__F'
			DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>[] DialogueCPT_itemAllTraits_NCases__F = new DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>[215];
			// Create array for replicates of 'DialogueCPT_itemAllTraits_NCases__B'
			DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>[] DialogueCPT_itemAllTraits_NCases__B = new DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>[215];
			// Create array for replicates of 'DialogueCPT_use_F_Events__marginal'
			DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>[] DialogueCPT_use_F_Events__marginal = new DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>[215];
			for(int Events = 0; Events<215; Events++) {
				// Create array for 'DialogueCPT' Forwards messages.
				DialogueCPT_F[Events] = new DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>(14);
				for(int Traits = 0; Traits<14; Traits++) {
					// Create array for 'DialogueCPT' Forwards messages.
					DialogueCPT_F[Events][Traits] = new DistributionRefArray<Dirichlet,Vector>(3);
				}
			}
			for(int Rel = 0; Rel<3; Rel++) {
				for(int Events = 0; Events<215; Events++) {
					for(int Traits = 0; Traits<14; Traits++) {
						DialogueCPT_F[Events][Traits][Rel] = ArrayHelper.MakeUniform<Dirichlet>(this.DialogueCPTPrior[Events][Traits][Rel]);
					}
				}
			}
			for(int Events = 0; Events<215; Events++) {
				// Create array for 'DialogueCPT_marginal' Forwards messages.
				this.DialogueCPT_marginal_F[Events] = new DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>(14);
				// Create array for 'DialogueCPT_use' Backwards messages.
				DialogueCPT_use_B[Events] = new DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>(14);
				for(int Traits = 0; Traits<14; Traits++) {
					// Create array for 'DialogueCPT_use' Backwards messages.
					DialogueCPT_use_B[Events][Traits] = new DistributionRefArray<Dirichlet,Vector>(3);
				}
			}
			for(int Rel = 0; Rel<3; Rel++) {
				for(int Events = 0; Events<215; Events++) {
					for(int Traits = 0; Traits<14; Traits++) {
						DialogueCPT_use_B[Events][Traits][Rel] = ArrayHelper.MakeUniform<Dirichlet>(this.DialogueCPTPrior[Events][Traits][Rel]);
					}
				}
			}
			for(int Events = 0; Events<215; Events++) {
				for(int Traits = 0; Traits<14; Traits++) {
					// Create array for 'DialogueCPT_marginal' Forwards messages.
					this.DialogueCPT_marginal_F[Events][Traits] = new DistributionRefArray<Dirichlet,Vector>(3);
				}
			}
			for(int Rel = 0; Rel<3; Rel++) {
				for(int Events = 0; Events<215; Events++) {
					for(int Traits = 0; Traits<14; Traits++) {
						this.DialogueCPT_marginal_F[Events][Traits][Rel] = ArrayHelper.MakeUniform<Dirichlet>(this.DialogueCPTPrior[Events][Traits][Rel]);
						// Message to 'DialogueCPT_marginal' from Variable factor
						this.DialogueCPT_marginal_F[Events][Traits][Rel] = VariableOp.MarginalAverageConditional<Dirichlet>(DialogueCPT_use_B[Events][Traits][Rel], this.DialogueCPTPrior[Events][Traits][Rel], this.DialogueCPT_marginal_F[Events][Traits][Rel]);
					}
				}
			}
			for(int Events = 0; Events<215; Events++) {
				DialogueCPT_use_F_Events__marginal[Events] = GetItemsOp<Vector[]>.MarginalInit<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>>(DialogueCPT_F[Events]);
			}
			for(int Rel = 0; Rel<3; Rel++) {
				for(int Events = 0; Events<215; Events++) {
					for(int Traits = 0; Traits<14; Traits++) {
						// Message to 'DialogueCPT' from Random factor
						DialogueCPT_F[Events][Traits][Rel] = ArrayHelper.SetTo<Dirichlet>(DialogueCPT_F[Events][Traits][Rel], this.DialogueCPTPrior[Events][Traits][Rel]);
					}
				}
			}
			for(int Events = 0; Events<215; Events++) {
				// Create array for 'DialogueCPT_itemAllTraits_NCases_' Forwards messages.
				DialogueCPT_itemAllTraits_NCases__F[Events] = new DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>(this.NumOfCases);
				// Create array for 'DialogueCPT_itemAllTraits_NCases_' Backwards messages.
				DialogueCPT_itemAllTraits_NCases__B[Events] = new DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>(this.NumOfCases);
				DialogueCPT_use_F_Events__marginal[Events] = GetItemsOp<Vector[]>.Marginal<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,DistributionRefArray<Dirichlet,Vector>>(DialogueCPT_F[Events], DialogueCPT_use_B[Events], DialogueCPT_use_F_Events__marginal[Events]);
			}
			for(int NCases = 0; NCases<this.NumOfCases; NCases++) {
				for(int Events = 0; Events<215; Events++) {
					// Create array for 'DialogueCPT_itemAllTraits_NCases_' Forwards messages.
					DialogueCPT_itemAllTraits_NCases__F[Events][NCases] = new DistributionRefArray<Dirichlet,Vector>(3);
					// Create array for 'DialogueCPT_itemAllTraits_NCases_' Backwards messages.
					DialogueCPT_itemAllTraits_NCases__B[Events][NCases] = new DistributionRefArray<Dirichlet,Vector>(3);
				}
				for(int Rel = 0; Rel<3; Rel++) {
					for(int Events = 0; Events<215; Events++) {
						DialogueCPT_itemAllTraits_NCases__B[Events][NCases][Rel] = ArrayHelper.MakeUniform<Dirichlet>(this.DialogueCPTPrior[Events][this.AllTraits[NCases]][Rel]);
						DialogueCPT_itemAllTraits_NCases__F[Events][NCases][Rel] = ArrayHelper.MakeUniform<Dirichlet>(this.DialogueCPTPrior[Events][this.AllTraits[NCases]][Rel]);
					}
				}
				for(int Events = 0; Events<215; Events++) {
					// Message to 'DialogueCPT_itemAllTraits_NCases_' from GetItems factor
					DialogueCPT_itemAllTraits_NCases__F[Events][NCases] = GetItemsOp<Vector[]>.ItemsAverageConditional<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,DistributionRefArray<Dirichlet,Vector>>(DialogueCPT_itemAllTraits_NCases__B[Events][NCases], DialogueCPT_F[Events], DialogueCPT_use_F_Events__marginal[Events], this.AllTraits, NCases, DialogueCPT_itemAllTraits_NCases__F[Events][NCases]);
				}
			}
			DistributionRefArray<Discrete,int> AllEvents_F;
			// Create array for 'AllEvents' Forwards messages.
			AllEvents_F = new DistributionRefArray<Discrete,int>(this.NumOfCases);
			Dirichlet[] PossibleRel_rep_F;
			// Create array for 'PossibleRel_rep' Forwards messages.
			PossibleRel_rep_F = new Dirichlet[this.NumOfCases];
			Dirichlet[] PossibleRel_rep_B;
			// Create array for 'PossibleRel_rep' Backwards messages.
			PossibleRel_rep_B = new Dirichlet[this.NumOfCases];
			DistributionRefArray<Discrete,int> AllRels_F;
			// Create array for 'AllRels' Forwards messages.
			AllRels_F = new DistributionRefArray<Discrete,int>(this.NumOfCases);
			Dirichlet PossibleRel_rep_F_marginal;
			PossibleRel_rep_F_marginal = ReplicateOp_Divide.MarginalInit<Dirichlet>(this.RelStatusPriors);
			Dirichlet PossibleRel_rep_B_toDef;
			PossibleRel_rep_B_toDef = ReplicateOp_Divide.ToDefInit<Dirichlet>(this.RelStatusPriors);
			PossibleRel_rep_F_marginal = ReplicateOp_Divide.Marginal<Dirichlet>(PossibleRel_rep_B_toDef, this.RelStatusPriors, PossibleRel_rep_F_marginal);
			// Create array for replicates of 'AllEvents_NCases__Rel_selector_uses_F'
			Discrete[][][] AllEvents_NCases__Rel_selector_uses_F = new Discrete[this.NumOfCases][][];
			// Create array for replicates of 'AllRels_NCases__selector_uses_B_toDef'
			Discrete[] AllRels_NCases__selector_uses_B_toDef = new Discrete[this.NumOfCases];
			// Create array for replicates of 'AllRels_NCases__selector_uses_F_marginal'
			Discrete[] AllRels_NCases__selector_uses_F_marginal = new Discrete[this.NumOfCases];
			// Create array for replicates of 'AllRels_NCases__selector_cases_uses_B'
			DistributionStructArray<Bernoulli,bool>[][] AllRels_NCases__selector_cases_uses_B = new DistributionStructArray<Bernoulli,bool>[this.NumOfCases][];
			// Create array for replicates of 'AllRels_NCases__selector_uses_F'
			Discrete[][] AllRels_NCases__selector_uses_F = new Discrete[this.NumOfCases][];
			// Create array for replicates of 'AllRels_NCases__selector_uses_B'
			Discrete[][] AllRels_NCases__selector_uses_B = new Discrete[this.NumOfCases][];
			// Create array for replicates of 'AllRels_NCases__selector_cases_uses_F'
			DistributionStructArray<Bernoulli,bool>[][] AllRels_NCases__selector_cases_uses_F = new DistributionStructArray<Bernoulli,bool>[this.NumOfCases][];
			// Create array for replicates of 'AllRels_NCases__selector_cases_F'
			DistributionStructArray<Bernoulli,bool>[] AllRels_NCases__selector_cases_F = new DistributionStructArray<Bernoulli,bool>[this.NumOfCases];
			// Create array for replicates of 'AllEvents_NCases__Rel_selector_uses_B'
			Discrete[][][] AllEvents_NCases__Rel_selector_uses_B = new Discrete[this.NumOfCases][][];
			// Create array for replicates of 'AllEvents_NCases__Rel_selector_cases_F'
			DistributionStructArray<Bernoulli,bool>[][] AllEvents_NCases__Rel_selector_cases_F = new DistributionStructArray<Bernoulli,bool>[this.NumOfCases][];
			// Create array for replicates of 'AllEvents_NCases__Rel_selector_cases_uses_F'
			DistributionStructArray<Bernoulli,bool>[][][] AllEvents_NCases__Rel_selector_cases_uses_F = new DistributionStructArray<Bernoulli,bool>[this.NumOfCases][][];
			// Create array for replicates of 'AllEvents_NCases__Rel_selector_cases_uses_B'
			DistributionStructArray<Bernoulli,bool>[][][] AllEvents_NCases__Rel_selector_cases_uses_B = new DistributionStructArray<Bernoulli,bool>[this.NumOfCases][][];
			// Create array for replicates of 'DialogueChild_NCases__Rel_F'
			Discrete[][] DialogueChild_NCases__Rel_F = new Discrete[this.NumOfCases][];
			Dirichlet[] PossibleEvents_rep_F;
			// Create array for 'PossibleEvents_rep' Forwards messages.
			PossibleEvents_rep_F = new Dirichlet[this.NumOfCases];
			Dirichlet[] PossibleEvents_rep_B;
			// Create array for 'PossibleEvents_rep' Backwards messages.
			PossibleEvents_rep_B = new Dirichlet[this.NumOfCases];
			Dirichlet PossibleEvents_rep_F_marginal;
			PossibleEvents_rep_F_marginal = ReplicateOp_Divide.MarginalInit<Dirichlet>(this.EventsPriors);
			Dirichlet PossibleEvents_rep_B_toDef;
			PossibleEvents_rep_B_toDef = ReplicateOp_Divide.ToDefInit<Dirichlet>(this.EventsPriors);
			PossibleEvents_rep_F_marginal = ReplicateOp_Divide.Marginal<Dirichlet>(PossibleEvents_rep_B_toDef, this.EventsPriors, PossibleEvents_rep_F_marginal);
			// Create array for replicates of 'DialogueChild_NCases_2_Events_F'
			Discrete[][][] DialogueChild_NCases_2_Events_F = new Discrete[this.NumOfCases][][];
			DistributionRefArray<Discrete,int> DialogueChild_F;
			// Create array for 'DialogueChild' Forwards messages.
			DialogueChild_F = new DistributionRefArray<Discrete,int>(this.NumOfCases);
			// Create array for 'DialogueChild_marginal' Forwards messages.
			this.DialogueChild_marginal_F = new DistributionRefArray<Discrete,int>(this.NumOfCases);
			Discrete DialogueChild_use_B_reduced;
			DialogueChild_use_B_reduced = default(Discrete);
			if (this.NumOfCases>0) {
				DialogueChild_use_B_reduced = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(670));
			}
			for(int NCases = 0; NCases<this.NumOfCases; NCases++) {
				AllEvents_F[NCases] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(215));
				PossibleEvents_rep_B[NCases] = ArrayHelper.MakeUniform<Dirichlet>(this.EventsPriors);
				PossibleEvents_rep_F[NCases] = ArrayHelper.MakeUniform<Dirichlet>(this.EventsPriors);
				// Message to 'PossibleEvents_rep' from Replicate factor
				PossibleEvents_rep_F[NCases] = ReplicateOp_Divide.UsesAverageConditional<Dirichlet>(PossibleEvents_rep_B[NCases], PossibleEvents_rep_F_marginal, NCases, PossibleEvents_rep_F[NCases]);
				// Message to 'AllEvents' from Discrete factor
				AllEvents_F[NCases] = DiscreteFromDirichletOp.SampleAverageConditional(PossibleEvents_rep_F[NCases], AllEvents_F[NCases]);
				AllRels_F[NCases] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(3));
				PossibleRel_rep_B[NCases] = ArrayHelper.MakeUniform<Dirichlet>(this.RelStatusPriors);
				PossibleRel_rep_F[NCases] = ArrayHelper.MakeUniform<Dirichlet>(this.RelStatusPriors);
				// Message to 'PossibleRel_rep' from Replicate factor
				PossibleRel_rep_F[NCases] = ReplicateOp_Divide.UsesAverageConditional<Dirichlet>(PossibleRel_rep_B[NCases], PossibleRel_rep_F_marginal, NCases, PossibleRel_rep_F[NCases]);
				AllRels_NCases__selector_uses_F_marginal[NCases] = ReplicateOp_Divide.MarginalInit<Discrete>(AllRels_F[NCases]);
				AllRels_NCases__selector_uses_B_toDef[NCases] = ReplicateOp_Divide.ToDefInit<Discrete>(AllRels_F[NCases]);
				// Message to 'AllRels' from Discrete factor
				AllRels_F[NCases] = DiscreteFromDirichletOp.SampleAverageConditional(PossibleRel_rep_F[NCases], AllRels_F[NCases]);
				DialogueChild_F[NCases] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(670));
				this.DialogueChild_marginal_F[NCases] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(670));
				// Create array for 'AllRels_NCases__selector_uses' Forwards messages.
				AllRels_NCases__selector_uses_F[NCases] = new Discrete[3];
				// Create array for 'AllRels_NCases__selector_uses' Backwards messages.
				AllRels_NCases__selector_uses_B[NCases] = new Discrete[3];
				AllRels_NCases__selector_uses_B[NCases][0] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(3));
				AllRels_NCases__selector_uses_F[NCases][0] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(3));
				AllRels_NCases__selector_uses_F_marginal[NCases] = ReplicateOp_Divide.Marginal<Discrete>(AllRels_NCases__selector_uses_B_toDef[NCases], AllRels_F[NCases], AllRels_NCases__selector_uses_F_marginal[NCases]);
				// Message to 'AllRels_NCases__selector_uses' from Replicate factor
				AllRels_NCases__selector_uses_F[NCases][0] = ReplicateOp_Divide.UsesAverageConditional<Discrete>(AllRels_NCases__selector_uses_B[NCases][0], AllRels_NCases__selector_uses_F_marginal[NCases], 0, AllRels_NCases__selector_uses_F[NCases][0]);
				// Create array for 'AllRels_NCases__selector_cases' Forwards messages.
				AllRels_NCases__selector_cases_F[NCases] = new DistributionStructArray<Bernoulli,bool>(3);
				for(int Rel = 0; Rel<3; Rel++) {
					AllRels_NCases__selector_cases_F[NCases][Rel] = Bernoulli.Uniform();
					// Message to 'AllRels_NCases__selector_cases' from CasesInt factor
					AllRels_NCases__selector_cases_F[NCases][Rel] = IntCasesOp.CasesAverageConditional(AllRels_NCases__selector_uses_F[NCases][0], Rel);
				}
				// Create array for 'AllRels_NCases__selector_cases_uses' Forwards messages.
				AllRels_NCases__selector_cases_uses_F[NCases] = new DistributionStructArray<Bernoulli,bool>[2];
				// Create array for 'AllRels_NCases__selector_cases_uses' Backwards messages.
				AllRels_NCases__selector_cases_uses_B[NCases] = new DistributionStructArray<Bernoulli,bool>[2];
				// Create array for 'AllRels_NCases__selector_cases_uses' Backwards messages.
				AllRels_NCases__selector_cases_uses_B[NCases][0] = new DistributionStructArray<Bernoulli,bool>(3);
				for(int Rel = 0; Rel<3; Rel++) {
					AllRels_NCases__selector_cases_uses_B[NCases][0][Rel] = Bernoulli.Uniform();
				}
				// Create array for 'AllRels_NCases__selector_cases_uses' Forwards messages.
				AllRels_NCases__selector_cases_uses_F[NCases][1] = new DistributionStructArray<Bernoulli,bool>(3);
				for(int Rel = 0; Rel<3; Rel++) {
					AllRels_NCases__selector_cases_uses_F[NCases][1][Rel] = Bernoulli.Uniform();
				}
				// Message to 'AllRels_NCases__selector_cases_uses' from Replicate factor
				AllRels_NCases__selector_cases_uses_F[NCases][1] = ReplicateOp_NoDivide.UsesAverageConditional<DistributionStructArray<Bernoulli,bool>>(AllRels_NCases__selector_cases_uses_B[NCases], AllRels_NCases__selector_cases_F[NCases], 1, AllRels_NCases__selector_cases_uses_F[NCases][1]);
				// Create array for replicates of 'AllEvents_NCases__Rel_selector_uses_F'
				AllEvents_NCases__Rel_selector_uses_F[NCases] = new Discrete[3][];
				// Create array for replicates of 'AllEvents_NCases__Rel_selector_uses_B'
				AllEvents_NCases__Rel_selector_uses_B[NCases] = new Discrete[3][];
				// Create array for replicates of 'AllEvents_NCases__Rel_selector_cases_F'
				AllEvents_NCases__Rel_selector_cases_F[NCases] = new DistributionStructArray<Bernoulli,bool>[3];
				// Create array for replicates of 'AllEvents_NCases__Rel_selector_cases_uses_F'
				AllEvents_NCases__Rel_selector_cases_uses_F[NCases] = new DistributionStructArray<Bernoulli,bool>[3][];
				// Create array for replicates of 'AllEvents_NCases__Rel_selector_cases_uses_B'
				AllEvents_NCases__Rel_selector_cases_uses_B[NCases] = new DistributionStructArray<Bernoulli,bool>[3][];
				// Create array for replicates of 'DialogueChild_NCases__Rel_F'
				DialogueChild_NCases__Rel_F[NCases] = new Discrete[3];
				// Create array for replicates of 'DialogueChild_NCases_2_Events_F'
				DialogueChild_NCases_2_Events_F[NCases] = new Discrete[3][];
				for(int Rel = 0; Rel<3; Rel++) {
					// Create array for 'AllEvents_NCases__Rel_selector_uses' Forwards messages.
					AllEvents_NCases__Rel_selector_uses_F[NCases][Rel] = new Discrete[2];
					// Create array for 'AllEvents_NCases__Rel_selector_uses' Backwards messages.
					AllEvents_NCases__Rel_selector_uses_B[NCases][Rel] = new Discrete[2];
					AllEvents_NCases__Rel_selector_uses_B[NCases][Rel][1] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(215));
					AllEvents_NCases__Rel_selector_uses_F[NCases][Rel][0] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(215));
					// Message to 'AllEvents_NCases__Rel_selector_uses' from Replicate factor
					AllEvents_NCases__Rel_selector_uses_F[NCases][Rel][0] = ReplicateOp_NoDivide.UsesAverageConditional<Discrete>(AllEvents_NCases__Rel_selector_uses_B[NCases][Rel], AllEvents_F[NCases], 0, AllEvents_NCases__Rel_selector_uses_F[NCases][Rel][0]);
					// Create array for 'AllEvents_NCases__Rel_selector_cases' Forwards messages.
					AllEvents_NCases__Rel_selector_cases_F[NCases][Rel] = new DistributionStructArray<Bernoulli,bool>(215);
					for(int Events = 0; Events<215; Events++) {
						AllEvents_NCases__Rel_selector_cases_F[NCases][Rel][Events] = Bernoulli.Uniform();
						// Message to 'AllEvents_NCases__Rel_selector_cases' from CasesInt factor
						AllEvents_NCases__Rel_selector_cases_F[NCases][Rel][Events] = IntCasesOp.CasesAverageConditional(AllEvents_NCases__Rel_selector_uses_F[NCases][Rel][0], Events);
					}
					// Create array for 'AllEvents_NCases__Rel_selector_cases_uses' Forwards messages.
					AllEvents_NCases__Rel_selector_cases_uses_F[NCases][Rel] = new DistributionStructArray<Bernoulli,bool>[2];
					// Create array for 'AllEvents_NCases__Rel_selector_cases_uses' Backwards messages.
					AllEvents_NCases__Rel_selector_cases_uses_B[NCases][Rel] = new DistributionStructArray<Bernoulli,bool>[2];
					// Create array for 'AllEvents_NCases__Rel_selector_cases_uses' Backwards messages.
					AllEvents_NCases__Rel_selector_cases_uses_B[NCases][Rel][0] = new DistributionStructArray<Bernoulli,bool>(215);
					for(int Events = 0; Events<215; Events++) {
						AllEvents_NCases__Rel_selector_cases_uses_B[NCases][Rel][0][Events] = Bernoulli.Uniform();
					}
					// Create array for 'AllEvents_NCases__Rel_selector_cases_uses' Forwards messages.
					AllEvents_NCases__Rel_selector_cases_uses_F[NCases][Rel][1] = new DistributionStructArray<Bernoulli,bool>(215);
					for(int Events = 0; Events<215; Events++) {
						AllEvents_NCases__Rel_selector_cases_uses_F[NCases][Rel][1][Events] = Bernoulli.Uniform();
					}
					// Message to 'AllEvents_NCases__Rel_selector_cases_uses' from Replicate factor
					AllEvents_NCases__Rel_selector_cases_uses_F[NCases][Rel][1] = ReplicateOp_NoDivide.UsesAverageConditional<DistributionStructArray<Bernoulli,bool>>(AllEvents_NCases__Rel_selector_cases_uses_B[NCases][Rel], AllEvents_NCases__Rel_selector_cases_F[NCases][Rel], 1, AllEvents_NCases__Rel_selector_cases_uses_F[NCases][Rel][1]);
					DialogueChild_NCases__Rel_F[NCases][Rel] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(670));
					// Create array for replicates of 'DialogueChild_NCases_2_Events_F'
					DialogueChild_NCases_2_Events_F[NCases][Rel] = new Discrete[215];
					for(int Events = 0; Events<215; Events++) {
						DialogueChild_NCases_2_Events_F[NCases][Rel][Events] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(670));
						// Message to 'DialogueChild_NCases_2_Events' from Discrete factor
						DialogueChild_NCases_2_Events_F[NCases][Rel][Events] = DiscreteFromDirichletOp.SampleAverageConditional(DialogueCPT_itemAllTraits_NCases__F[Events][NCases][Rel], DialogueChild_NCases_2_Events_F[NCases][Rel][Events]);
					}
					// Message to 'DialogueChild_NCases__Rel' from Exit factor
					DialogueChild_NCases__Rel_F[NCases][Rel] = BeliefPropagationGateExitOp.ExitAverageConditional<Discrete>(AllEvents_NCases__Rel_selector_cases_uses_F[NCases][Rel][1], DialogueChild_NCases_2_Events_F[NCases][Rel], DialogueChild_NCases__Rel_F[NCases][Rel]);
				}
				// Message to 'DialogueChild' from Exit factor
				DialogueChild_F[NCases] = BeliefPropagationGateExitOp.ExitAverageConditional<Discrete>(AllRels_NCases__selector_cases_uses_F[NCases][1], DialogueChild_NCases__Rel_F[NCases], DialogueChild_F[NCases]);
				// Message to 'DialogueChild_marginal' from DerivedVariable factor
				this.DialogueChild_marginal_F[NCases] = DerivedVariableOp.MarginalAverageConditional<Discrete>(DialogueChild_use_B_reduced, DialogueChild_F[NCases], this.DialogueChild_marginal_F[NCases]);
			}
			this.Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors_isDone = true;
		}

		/// <summary>
		/// Returns the marginal distribution for 'DialogueChild' given by the current state of the
		/// message passing algorithm.
		/// </summary>
		/// <returns>The marginal distribution</returns>
		public DistributionRefArray<Discrete,int> DialogueChildMarginal()
		{
			return this.DialogueChild_marginal_F;
		}

		/// <summary>
		/// Returns the marginal distribution for 'DialogueCPT' given by the current state of the
		/// message passing algorithm.
		/// </summary>
		/// <returns>The marginal distribution</returns>
		public DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]> DialogueCPTMarginal()
		{
			return this.DialogueCPT_marginal_F;
		}

		/// <summary>Update all marginals, by iterating message passing the given number of times</summary>
		/// <param name="numberOfIterations">The number of times to iterate each loop</param>
		/// <param name="initialise">If true, messages that initialise loops are reset when observed values change</param>
		private void Execute(int numberOfIterations, bool initialise)
		{
			this.Changed_AllTraits_DialogueCPTPrior_EventsPriors_NumOfCases_RelStatusPriors();
			this.numberOfIterationsDone = numberOfIterations;
		}

		/// <summary>Update all marginals, by iterating message-passing the given number of times</summary>
		/// <param name="numberOfIterations">The total number of iterations that should be executed for the current set of observed values.  If this is more than the number already done, only the extra iterations are done.  If this is less than the number already done, message-passing is restarted from the beginning.  Changing the observed values resets the iteration count to 0.</param>
		public void Execute(int numberOfIterations)
		{
			this.Execute(numberOfIterations, true);
		}

		/// <summary>Get the observed value of the specified variable.</summary>
		/// <param name="variableName">Variable name</param>
		public object GetObservedValue(string variableName)
		{
			if (variableName=="NumOfCases") {
				return this.NumOfCases;
			}
			if (variableName=="EventsPriors") {
				return this.EventsPriors;
			}
			if (variableName=="TraitsPriors") {
				return this.TraitsPriors;
			}
			if (variableName=="RelStatusPriors") {
				return this.RelStatusPriors;
			}
			if (variableName=="DialogueCPTPrior") {
				return this.DialogueCPTPrior;
			}
			if (variableName=="AllTraits") {
				return this.AllTraits;
			}
			throw new ArgumentException("Not an observed variable name: "+variableName);
		}

		/// <summary>Get the marginal distribution (computed up to this point) of a variable</summary>
		/// <param name="variableName">Name of the variable in the generated code</param>
		/// <returns>The marginal distribution computed up to this point</returns>
		/// <remarks>Execute, Update, or Reset must be called first to set the value of the marginal.</remarks>
		public object Marginal(string variableName)
		{
			if (variableName=="DialogueCPT") {
				return this.DialogueCPTMarginal();
			}
			if (variableName=="DialogueChild") {
				return this.DialogueChildMarginal();
			}
			throw new ArgumentException("This class was not built to infer "+variableName);
		}

		/// <summary>Get the marginal distribution (computed up to this point) of a variable, converted to type T</summary>
		/// <typeparam name="T">The distribution type.</typeparam>
		/// <param name="variableName">Name of the variable in the generated code</param>
		/// <returns>The marginal distribution computed up to this point</returns>
		/// <remarks>Execute, Update, or Reset must be called first to set the value of the marginal.</remarks>
		public T Marginal<T>(string variableName)
		{
			return Distribution.ChangeType<T>(this.Marginal(variableName));
		}

		/// <summary>Get the query-specific marginal distribution of a variable.</summary>
		/// <param name="variableName">Name of the variable in the generated code</param>
		/// <param name="query">QueryType name. For example, GibbsSampling answers 'Marginal', 'Samples', and 'Conditionals' queries</param>
		/// <remarks>Execute, Update, or Reset must be called first to set the value of the marginal.</remarks>
		public object Marginal(string variableName, string query)
		{
			if (query=="Marginal") {
				return this.Marginal(variableName);
			}
			throw new ArgumentException(((("This class was not built to infer \'"+variableName)+"\' with query \'")+query)+"\'");
		}

		/// <summary>Get the query-specific marginal distribution of a variable, converted to type T</summary>
		/// <typeparam name="T">The distribution type.</typeparam>
		/// <param name="variableName">Name of the variable in the generated code</param>
		/// <param name="query">QueryType name. For example, GibbsSampling answers 'Marginal', 'Samples', and 'Conditionals' queries</param>
		/// <remarks>Execute, Update, or Reset must be called first to set the value of the marginal.</remarks>
		public T Marginal<T>(string variableName, string query)
		{
			return Distribution.ChangeType<T>(this.Marginal(variableName, query));
		}

		private void OnProgressChanged(ProgressChangedEventArgs e)
		{
			// Make a temporary copy of the event to avoid a race condition
			// if the last subscriber unsubscribes immediately after the null check and before the event is raised.
			EventHandler<ProgressChangedEventArgs> handler = this.ProgressChanged;
			if (handler!=null) {
				handler(this, e);
			}
		}

		/// <summary>Reset all messages to their initial values.  Sets NumberOfIterationsDone to 0.</summary>
		public void Reset()
		{
			this.Execute(0);
		}

		/// <summary>Set the observed value of the specified variable.</summary>
		/// <param name="variableName">Variable name</param>
		/// <param name="value">Observed value</param>
		public void SetObservedValue(string variableName, object value)
		{
			if (variableName=="NumOfCases") {
				this.NumOfCases = (int)value;
				return ;
			}
			if (variableName=="EventsPriors") {
				this.EventsPriors = (Dirichlet)value;
				return ;
			}
			if (variableName=="TraitsPriors") {
				this.TraitsPriors = (Dirichlet)value;
				return ;
			}
			if (variableName=="RelStatusPriors") {
				this.RelStatusPriors = (Dirichlet)value;
				return ;
			}
			if (variableName=="DialogueCPTPrior") {
				this.DialogueCPTPrior = (Dirichlet[][][])value;
				return ;
			}
			if (variableName=="AllTraits") {
				this.AllTraits = (int[])value;
				return ;
			}
			throw new ArgumentException("Not an observed variable name: "+variableName);
		}

		/// <summary>Update all marginals, by iterating message-passing an additional number of times</summary>
		/// <param name="additionalIterations">The number of iterations that should be executed, starting from the current message state.  Messages are not reset, even if observed values have changed.</param>
		public void Update(int additionalIterations)
		{
			this.Execute(checked(this.numberOfIterationsDone+additionalIterations), false);
		}

		#endregion

		#region Events
		/// <summary>Event that is fired when the progress of inference changes, typically at the end of one iteration of the inference algorithm.</summary>
		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
		#endregion

	}

}
