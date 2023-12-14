using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /*
        List of all predicates available to use in conditions
        Comes from various external systems like quests, inventory, relationships, etc.
        Add/Subtract as needed
    */
    public enum PredicateType
    {
        Select,
        HasQuest,
        HasCompletedQuest,
        HasItem,
        HasMet,
        IsFriend,
        IsEnemy,
        IsInRelationship
    }
}
