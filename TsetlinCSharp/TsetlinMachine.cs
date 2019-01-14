using System;
using System.Runtime.CompilerServices;

namespace TsetlinCSharp
{
    public class TsetlinMachine
    {
        private int _threshold;
        private int _clauses;
        private int _numberOfStates;
        private bool _boostTruePositiveFeedback;
        private bool _predict;
        private bool _update;

        public int[][][] TaState;
        public int[] ClauseOutput;
        public int[] FeedbackToClauses;

        public int Features { get; }

        public TsetlinMachine(
            int threshold = Constants.THRESHOLD,
            int features = Constants.FEATURES,
            int clauses = Constants.CLAUSES,
            int numberOfStates = Constants.NUMBER_OF_STATES,
            bool boostTruePositiveFeedback = Constants.BOOST_TRUE_POSITIVE_FEEDBACK,
            bool predict = Constants.PREDICT,
            bool update = Constants.UPDATE)
        {
            _threshold = threshold;
            Features = features;
            _clauses = clauses;
            _numberOfStates = numberOfStates;
            _boostTruePositiveFeedback = boostTruePositiveFeedback;
            _predict = predict;
            _update = update;

            Initialize();
        }


        #region Private Methods

        private void Initialize()
        {
            var state = new int[_clauses][][];

            for (var i = 0; i <= state.Length; i++)
            {
                var features = new int[Features][];
                state[i] = features;

                for (var j = 0; j <= features.Length; j++)
                {
                    state[i][j] = new int[2];
                }
            }

            TaState = state;
            ClauseOutput = new int[_clauses];
            FeedbackToClauses = new int[_clauses];

            Random rand = new Random();

            for (int j = 0; j < _clauses; j++)
            {
                for (int k = 0; k < Features; k++)
                {
                    if (rand.NextDouble() / double.MaxValue <= 0.5)
                    {
                        TaState[j][k][0] = _numberOfStates;
                        TaState[j][k][1] = _numberOfStates + 1;
                    }
                    else
                    {
                        TaState[j][k][0] = _numberOfStates + 1;
                        TaState[j][k][1] = _numberOfStates; // Deviation, should be random
                    }
                }
            }
        }


        /* Translates automata state to action */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Action(int state)
        {
            return state > _numberOfStates;
        }

        /* Calculate the output of each clause using the actions of each Tsetline Automaton. */
        /* Output is stored an internal output array. */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculateClauseOutput(int[] X, bool predict)
        {
            int j, k;
            bool action_include, action_include_negated;
            bool all_exclude;

            for (j = 0; j < _clauses; j++)
            {
                ClauseOutput[j] = 1;
                all_exclude = true;

                for (k = 0; k < Features; k++)
                {
                    action_include = Action(TaState[j][k][0]);
                    action_include_negated = Action(TaState[j][k][1]);

                    all_exclude = all_exclude && !(action_include || action_include_negated);

                    if ((action_include && X[k] == 0) || (action_include_negated && X[k] == 1))
                    {
                        ClauseOutput[j] = 0;
                        break;
                    }
                }

                ClauseOutput[j] = Convert.ToInt32(Convert.ToBoolean(ClauseOutput[j]) && !(predict == _predict && all_exclude));
            }
        }

        /* Sum up the votes for each class (this is the multiclass version of the Tsetlin Machine) */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int SumUpClassVotes()
        {
            int class_sum = 0;
            for (int j = 0; j < _clauses; j++)
            {
                int sign = 1 - 2 * (j & 1);
                class_sum += ClauseOutput[j] * sign;
            }

            class_sum = (class_sum > _threshold) ? _threshold : class_sum;
            class_sum = (class_sum < -_threshold) ? -_threshold : class_sum;

            return class_sum;
        }

        /*************************************************/
        /*** Type I Feedback (Combats False Negatives) ***/
        /*************************************************/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TypeIFeedback(int[] X, int j, float s)
        {
            Random rand = new Random();

            if (ClauseOutput[j] == 0)
            {
                for (int k = 0; k < Features; k++)
                {
                    TaState[j][k][0] -= Convert.ToInt32((TaState[j][k][0] > 1) && (rand.NextDouble() / double.MaxValue <= 1.0 / s));

                    TaState[j][k][1] -= Convert.ToInt32((TaState[j][k][1] > 1) && (rand.NextDouble() / double.MaxValue <= 1.0 / s));
                }
            }
            else if (ClauseOutput[j] == 1)
            {
                for (int k = 0; k < Features; k++)
                {
                    if (X[k] == 1)
                    {
                        TaState[j][k][0] += Convert.ToInt32((TaState[j][k][0] < _numberOfStates * 2) && (_boostTruePositiveFeedback || rand.NextDouble() / double.MaxValue <= (s - 1) / s));

                        TaState[j][k][1] -= Convert.ToInt32((TaState[j][k][1] > 1) && (rand.NextDouble() / double.MaxValue <= 1.0 / s));
                    }
                    else if (X[k] == 0)
                    {
                        TaState[j][k][1] += Convert.ToInt32((TaState[j][k][1] < _numberOfStates * 2) && (_boostTruePositiveFeedback || rand.NextDouble() / double.MaxValue <= (s - 1) / s));

                        TaState[j][k][0] -= Convert.ToInt32((TaState[j][k][0] > 1) && (rand.NextDouble() / double.MaxValue <= 1.0 / s));
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TypeIIFeedback(int[] X, int j)
        {
            bool action_include;
            bool action_include_negated;

            if (ClauseOutput[j] == 1)
            {
                for (int k = 0; k < Features; k++)
                {
                    action_include = Action(TaState[j][k][0]);
                    action_include_negated = Action(TaState[j][k][1]);

                    TaState[j][k][0] += Convert.ToInt32((action_include == false && TaState[j][k][0] < (_numberOfStates * 2)) && (X[k] == 0));
                    TaState[j][k][1] += Convert.ToInt32((action_include_negated == false && TaState[j][k][1] < (_numberOfStates * 2)) && (X[k] == 1));
                }
            }
        }

        #endregion

        /******************************************/
        /*** Online Training of Tsetlin Machine ***/
        /******************************************/

        // The Tsetlin Machine can be trained incrementally, one training example at a time.
        // Use this method directly for online and incremental training.
        public void Update(int[] X, int target, float s)
        {
            CalculateClauseOutput(X, _update);

            /***************************/
            /*** Sum up Clause Votes ***/
            /***************************/

            int class_sum = SumUpClassVotes();

            /*************************************/
            /*** Calculate Feedback to Clauses ***/
            /*************************************/
            Random rand = new Random();

            // Calculate feedback to clauses
            for (int j = 0; j < _clauses; j++)
            {
                FeedbackToClauses[j] = ((2 * target) - 1) * (1 - 2 * (j & 1)) * Convert.ToInt32(rand.NextDouble() / double.MaxValue <= 1.0 / (_threshold * 2) * (_threshold + (1 - 2 * target) * class_sum));
            }

            /*********************************/
            /*** Train Individual Automata ***/
            /*********************************/

            for (int j = 0; j < _clauses; j++)
            {
                if (FeedbackToClauses[j] > 0)
                {
                    TypeIFeedback(X, j, s);
                }
                else if (FeedbackToClauses[j] < 0)
                {
                    TypeIIFeedback(X, j);
                }
            }
        }

        public int Score(int[] X)
        {
            /*******************************/
            /*** Calculate Clause Output ***/
            /*******************************/

            CalculateClauseOutput(X, _predict);

            /***************************/
            /*** Sum up Clause Votes ***/
            /***************************/

            return SumUpClassVotes();
        }

        /* Get the state of a specific automaton, indexed by clause, feature, and automaton type (include/include negated). */
        public int GetState(int clause, int feature, int automaton_type)
        {
            return TaState[clause][feature][automaton_type];
        }
    }
}

