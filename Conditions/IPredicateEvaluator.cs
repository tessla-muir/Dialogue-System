using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public interface IPredicateEvaluator
    {
        bool? Evaluate(PredicateType predicate, string[] parameters);
    }
}