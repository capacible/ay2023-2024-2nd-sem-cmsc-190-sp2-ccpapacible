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
	/// Generated by Infer.NET 0.4.2301.301 at 12:03 AM on Sunday, July 16, 2023.
	/// </remarks>
	public partial class DialogueDirector1_EP : IGeneratedAlgorithm
	{
		#region Fields
		/// <summary>Field backing the AllEvents property</summary>
		private int[] AllEvents_field;
		/// <summary>Field backing the AllRels property</summary>
		private int[] AllRels_field;
		/// <summary>Field backing the AllTraits property</summary>
		private int[] AllTraits_field;
		/// <summary>True if Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases has executed. Set this to false to force re-execution of Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases</summary>
		public bool Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases_isDone;
		/// <summary>Message to marginal of 'DialogueChild'</summary>
		public DistributionRefArray<Discrete,int> DialogueChild_marginal_F;
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
		/// <summary>The externally-specified value of 'AllEvents'</summary>
		public int[] AllEvents
		{
			get {
				return this.AllEvents_field;
			}
			set {
				if ((value!=null)&&(value.Length!=this.NumOfCases)) {
					throw new ArgumentException(((("Provided array of length "+value.Length)+" when length ")+this.NumOfCases)+" was expected for variable \'AllEvents\'");
				}
				this.AllEvents_field = value;
				this.numberOfIterationsDone = 0;
				this.Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases_isDone = false;
			}
		}

		/// <summary>The externally-specified value of 'AllRels'</summary>
		public int[] AllRels
		{
			get {
				return this.AllRels_field;
			}
			set {
				if ((value!=null)&&(value.Length!=this.NumOfCases)) {
					throw new ArgumentException(((("Provided array of length "+value.Length)+" when length ")+this.NumOfCases)+" was expected for variable \'AllRels\'");
				}
				this.AllRels_field = value;
				this.numberOfIterationsDone = 0;
				this.Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases_isDone = false;
			}
		}

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
				this.Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases_isDone = false;
			}
		}

		/// <summary>The externally-specified value of 'DialogueCPTPrior'</summary>
		public Dirichlet[][][] DialogueCPTPrior
		{
			get {
				return this.DialogueCPTPrior_field;
			}
			set {
				if ((value!=null)&&(value.Length!=148)) {
					throw new ArgumentException(((("Provided array of length "+value.Length)+" when length ")+148)+" was expected for variable \'DialogueCPTPrior\'");
				}
				this.DialogueCPTPrior_field = value;
				this.numberOfIterationsDone = 0;
				this.Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases_isDone = false;
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
					this.Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases_isDone = false;
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
		/// <summary>Computations that depend on the observed value of AllEvents and AllRels and AllTraits and DialogueCPTPrior and NumOfCases</summary>
		private void Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases()
		{
			if (this.Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases_isDone) {
				return ;
			}
			DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]> DialogueCPT_F;
			// Create array for 'DialogueCPT' Forwards messages.
			DialogueCPT_F = new DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]>(148);
			for(int Events = 0; Events<148; Events++) {
				// Create array for 'DialogueCPT' Forwards messages.
				DialogueCPT_F[Events] = new DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>(14);
				for(int Traits = 0; Traits<14; Traits++) {
					// Create array for 'DialogueCPT' Forwards messages.
					DialogueCPT_F[Events][Traits] = new DistributionRefArray<Dirichlet,Vector>(3);
					for(int Rel = 0; Rel<3; Rel++) {
						DialogueCPT_F[Events][Traits][Rel] = ArrayHelper.MakeUniform<Dirichlet>(this.DialogueCPTPrior[Events][Traits][Rel]);
					}
				}
			}
			DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]> DialogueCPT_use_B;
			// Create array for 'DialogueCPT_use' Backwards messages.
			DialogueCPT_use_B = new DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]>(148);
			for(int Events = 0; Events<148; Events++) {
				// Create array for 'DialogueCPT_use' Backwards messages.
				DialogueCPT_use_B[Events] = new DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>(14);
				for(int Traits = 0; Traits<14; Traits++) {
					// Create array for 'DialogueCPT_use' Backwards messages.
					DialogueCPT_use_B[Events][Traits] = new DistributionRefArray<Dirichlet,Vector>(3);
					for(int Rel = 0; Rel<3; Rel++) {
						DialogueCPT_use_B[Events][Traits][Rel] = ArrayHelper.MakeUniform<Dirichlet>(this.DialogueCPTPrior[Events][Traits][Rel]);
					}
				}
			}
			DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]> DialogueCPT_use_F_marginal;
			DialogueCPT_use_F_marginal = GetItemsFromDeepJaggedOp<Vector>.MarginalInit<DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]>>(DialogueCPT_F);
			for(int Events = 0; Events<148; Events++) {
				for(int Traits = 0; Traits<14; Traits++) {
					for(int Rel = 0; Rel<3; Rel++) {
						// Message to 'DialogueCPT' from Random factor
						DialogueCPT_F[Events][Traits][Rel] = ArrayHelper.SetTo<Dirichlet>(DialogueCPT_F[Events][Traits][Rel], this.DialogueCPTPrior[Events][Traits][Rel]);
					}
				}
			}
			DistributionRefArray<Discrete,int> DialogueChild_F;
			// Create array for 'DialogueChild' Forwards messages.
			DialogueChild_F = new DistributionRefArray<Discrete,int>(this.NumOfCases);
			DistributionRefArray<Dirichlet,Vector> DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases__F;
			// Create array for 'DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases_' Forwards messages.
			DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases__F = new DistributionRefArray<Dirichlet,Vector>(this.NumOfCases);
			DistributionRefArray<Dirichlet,Vector> DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases__B;
			// Create array for 'DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases_' Backwards messages.
			DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases__B = new DistributionRefArray<Dirichlet,Vector>(this.NumOfCases);
			// Create array for 'DialogueChild_marginal' Forwards messages.
			this.DialogueChild_marginal_F = new DistributionRefArray<Discrete,int>(this.NumOfCases);
			DialogueCPT_use_F_marginal = GetItemsFromDeepJaggedOp<Vector>.Marginal<DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]>,DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>>(DialogueCPT_F, DialogueCPT_use_B, DialogueCPT_use_F_marginal);
			Discrete DialogueChild_use_B_reduced;
			DialogueChild_use_B_reduced = default(Discrete);
			if (this.NumOfCases>0) {
				DialogueChild_use_B_reduced = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(606));
			}
			for(int NCases = 0; NCases<this.NumOfCases; NCases++) {
				DialogueChild_F[NCases] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(606));
				DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases__B[NCases] = ArrayHelper.MakeUniform<Dirichlet>(this.DialogueCPTPrior[this.AllEvents[NCases]][this.AllTraits[NCases]][this.AllRels[NCases]]);
				DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases__F[NCases] = ArrayHelper.MakeUniform<Dirichlet>(this.DialogueCPTPrior[this.AllEvents[NCases]][this.AllTraits[NCases]][this.AllRels[NCases]]);
				// Message to 'DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases_' from GetItemsFromDeepJagged factor
				DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases__F[NCases] = GetItemsFromDeepJaggedOp<Vector>.ItemsAverageConditional<DistributionRefArray<DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,Vector[][]>,DistributionRefArray<DistributionRefArray<Dirichlet,Vector>,Vector[]>,DistributionRefArray<Dirichlet,Vector>,Dirichlet>(DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases__B[NCases], DialogueCPT_F, DialogueCPT_use_F_marginal, this.AllEvents, this.AllTraits, this.AllRels, NCases, DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases__F[NCases]);
				this.DialogueChild_marginal_F[NCases] = ArrayHelper.MakeUniform<Discrete>(Discrete.Uniform(606));
				// Message to 'DialogueChild' from Discrete factor
				DialogueChild_F[NCases] = DiscreteFromDirichletOp.SampleAverageConditional(DialogueCPT_itemAllEvents_NCases_AllTraits_NCases_AllRels_NCases__F[NCases], DialogueChild_F[NCases]);
				// Message to 'DialogueChild_marginal' from Variable factor
				this.DialogueChild_marginal_F[NCases] = VariableOp.MarginalAverageConditional<Discrete>(DialogueChild_use_B_reduced, DialogueChild_F[NCases], this.DialogueChild_marginal_F[NCases]);
			}
			this.Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases_isDone = true;
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

		/// <summary>Update all marginals, by iterating message passing the given number of times</summary>
		/// <param name="numberOfIterations">The number of times to iterate each loop</param>
		/// <param name="initialise">If true, messages that initialise loops are reset when observed values change</param>
		private void Execute(int numberOfIterations, bool initialise)
		{
			this.Changed_AllEvents_AllRels_AllTraits_DialogueCPTPrior_NumOfCases();
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
			if (variableName=="AllEvents") {
				return this.AllEvents;
			}
			if (variableName=="AllTraits") {
				return this.AllTraits;
			}
			if (variableName=="AllRels") {
				return this.AllRels;
			}
			throw new ArgumentException("Not an observed variable name: "+variableName);
		}

		/// <summary>Get the marginal distribution (computed up to this point) of a variable</summary>
		/// <param name="variableName">Name of the variable in the generated code</param>
		/// <returns>The marginal distribution computed up to this point</returns>
		/// <remarks>Execute, Update, or Reset must be called first to set the value of the marginal.</remarks>
		public object Marginal(string variableName)
		{
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
			if (variableName=="AllEvents") {
				this.AllEvents = (int[])value;
				return ;
			}
			if (variableName=="AllTraits") {
				this.AllTraits = (int[])value;
				return ;
			}
			if (variableName=="AllRels") {
				this.AllRels = (int[])value;
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
