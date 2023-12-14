using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    [System.Serializable]
    public class Condition
    {
        [SerializeField] Disjunction[] and;

        public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
        {
            foreach (var disjunction in and)
            {
                // All need to be true in AND operations
                if (!disjunction.Check(evaluators))
                {
                    return false;
                }
            }
            return true;
        }


        [System.Serializable]
        class Disjunction
        {
            [SerializeField] Predicate[] or;

            public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
            {
                foreach (var predicate in or)
                {
                    // Only one needs to be true in OR operations
                    if (predicate.Check(evaluators))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [System.Serializable]
        class Predicate
        {
            [SerializeField] PredicateType predicate;
            [SerializeField] string[] parameters;
            [SerializeField] bool negate = false;

            public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
            {
                foreach (var evaluator in evaluators)
                {
                    bool? result = evaluator.Evaluate(predicate, parameters);
                    if (result == null)
                    {
                        continue;
                    }

                    if (result == negate) return false;
                }
                return true;
            }
        }
    }
}
