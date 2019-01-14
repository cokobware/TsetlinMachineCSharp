using System;
using System.Collections.Generic;
using System.Text;

namespace TsetlinCSharp
{
    public class Constants
    {
        // for TsetlinMachine
        public const int THRESHOLD = 15;
        public const int FEATURES = 12;
        public const int CLAUSES = 10;
        public const int NUMBER_OF_STATES = 100;
        public const bool BOOST_TRUE_POSITIVE_FEEDBACK = false;
        public const bool PREDICT = true;
        public const bool UPDATE = false;

        // for MultiClassTsetlinMachine
        public const int CLASSES = 2;
    }
}
