using System;
using TsetlinCSharp;

namespace TestlinCSharp.Demo
{
    class Program
    {
        const int NUMBER_OF_EXAMPLES = 5000;

        static void Main(string[] args)
        {
            int[][] X_train = new int[NUMBER_OF_EXAMPLES][];
            int[] y_train = new int[NUMBER_OF_EXAMPLES];

            for (var i = 0; i <= X_train.Length; i++)
            {
                X_train[i] = new int[Constants.FEATURES];
            }

            int[][] X_test = new int[NUMBER_OF_EXAMPLES][];
            int[] y_test = new int[NUMBER_OF_EXAMPLES];

            for (var i = 0; i <= X_test.Length; i++)
            {
                X_test[i] = new int[Constants.FEATURES];
            }

            ReadFiles();

            var mcTsetlinMachine = new MultiClassTsetlinMachine();

            float average = 0.0f;

	        for (var i = 0; i < 1000; i++) 
            {
                mcTsetlinMachine.Initialize();
                TimeSpan start_total = 0;
                mc_tm_fit(mc_tsetlin_machine, X_train, y_train, NUMBER_OF_EXAMPLES, 200, 3.9);
                clock_t end_total = clock();
                double time_used = ((double)(end_total - start_total)) / CLOCKS_PER_SEC;

                printf("EPOCH %d TIME: %f\n", i+1, time_used);
                average += mc_tm_evaluate(mc_tsetlin_machine, X_test, y_test, NUMBER_OF_EXAMPLES);

                printf("Average accuracy: %f\n", average/(i+1));
	        }
        }

        static void ReadFiles()
        {

            string[] lines;

            try
            {
                lines = System.IO.File.ReadAllLines(@".\\NoisyXORTrainingData.txt");

                for (int i = 0; i < NUMBER_OF_EXAMPLES; i++)
                {
                    var line = lines[i];

                    tokens = line.Split(' ');
                    for (int j = 0; j < Constants.FEATURES; j++)
                    {
                        X_train[i][j] = atoi(token);
                        token = strtok(NULL, s);
                    }
                    y_train[i] = atoi(token);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("");
            }

            fp = fopen("NoisyXORTestData.txt", "r");
            if (fp == NULL)
            {
                printf("Error opening\n");
                exit(EXIT_FAILURE);
            }

            for (int i = 0; i < NUMBER_OF_EXAMPLES; i++)
            {
                getline(&line, &len, fp);

                token = strtok(line, s);
                for (int j = 0; j < FEATURES; j++)
                {
                    X_test[i][j] = atoi(token);
                    token = strtok(NULL, s);
                }
                y_test[i] = atoi(token);
            }
        }
    }
}
