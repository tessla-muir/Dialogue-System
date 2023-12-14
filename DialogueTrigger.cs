using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private Trigger[] triggers;
        public IEnumerable<Trigger> Triggers => triggers;

        [Serializable]
        public class Trigger
        {
            [SerializeField] private DialogueAction action;
            [SerializeField] private UnityEvent onTrigger;

            public void TriggerAction(DialogueAction actionToTrigger)
            {
                if (actionToTrigger == action)
                {
                    onTrigger?.Invoke();
                }
            }
        }
    }
}
