using System;

namespace TsetlinCSharp
{
    public class MultiClassTsetlinMachine
    {
        private int _classes;
        private TsetlinMachine[] _tsetlinMachines;

        public MultiClassTsetlinMachine(int classes = Constants.CLASSES)
        {
            _classes = classes;
            _tsetlinMachines = new TsetlinMachine[classes];

            Initialize();
        }

        public void Initialize()
        {
            for (int i = 0; i < _classes; i++)
            {
                _tsetlinMachines[i] = new TsetlinMachine();
            }
        }

        /********************************************/
        /*** Evaluate the Trained Tsetlin Machine ***/
        /********************************************/
        public float Evaluate(int[][] X, int[] y, int numberOfExamples)
        {
            int errors;
            int max_class;
            int max_class_sum;

            errors = 0;

            for (int l = 0; l < numberOfExamples; l++)
            {
                /******************************************/
                /*** Identify Class with Largest Output ***/
                /******************************************/

                max_class_sum = _tsetlinMachines[0].Score(X[l]);
                max_class = 0;

                for (int i = 1; i < _classes; i++)
                {
                    int class_sum = _tsetlinMachines[i].Score(X[l]);
                    if (max_class_sum < class_sum)
                    {
                        max_class_sum = class_sum;
                        max_class = i;
                    }
                }

                if (max_class != y[l])
                {
                    errors += 1;
                }
            }

            return 1.0f - (1.0f * errors / numberOfExamples);
        }

        /******************************************/
        /*** Online Training of Tsetlin Machine ***/
        /******************************************/

        // The Tsetlin Machine can be trained incrementally, one training example at a time.
        // Use this method directly for online and incremental training.

        public void Update(int[] X, int targetClass, float s)
        {
            _tsetlinMachines[targetClass].Update(X, 1, s);

            Random rand = new Random();

            // Randomly pick one of the other classes, for pairwise learning of class output 
            int negative_target_class = (int)(_classes * rand.NextDouble() / double.MaxValue);

            while (negative_target_class == targetClass)
            {
                negative_target_class = (int)(_classes * rand.NextDouble() / double.MaxValue);
            }

            _tsetlinMachines[negative_target_class].Update(X, 0, s);
        }

        /**********************************************/
        /*** Batch Mode Training of Tsetlin Machine ***/
        /**********************************************/

        public void Fit(int[][] X, int[] y, int numberOfExamples, int epochs, float s)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                // Add shuffling here...		
                for (int i = 0; i < numberOfExamples; i++)
                {
                    Update(X[i], y[i], s);
                }
            }
        }

    }
}
