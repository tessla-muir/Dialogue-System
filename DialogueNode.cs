using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.Dialogue
{
    public class DialogueNode : ScriptableObject
    {
        [SerializeField] bool isPlayerSpeaking = false;
        [SerializeField] string text;
        [SerializeField] List<string> children = new List<string>();
        [SerializeField] Rect rect = new Rect(20, 20, 200, 100);

        // Actions
        [SerializeField] List<DialogueAction> onEnterActions = new List<DialogueAction>();
        [SerializeField] List<DialogueAction> onExitActions = new List<DialogueAction>();
        [SerializeField] Condition condition;

        public Rect GetRect()
        {
            return rect;
        }

        public string GetText() 
        {
            return text;
        }

        public List<string> GetChildren()
        {
            return children;
        }

        public bool IsPlayerSpeaking()
        {
            return isPlayerSpeaking;
        }

        public List<DialogueAction> GetOnEnterActions()
        {
            return onEnterActions;
        }

        public List<DialogueAction> GetOnExitActions()
        {
            return onExitActions;
        }

        public bool CheckCondition(IEnumerable<IPredicateEvaluator> evaluators)
        {
            if (evaluators.Count() == 0)
            {
                return true;
            }
            return condition.Check(evaluators);
        }

#if UNITY_EDITOR
        public void SetRectPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move Dialogue Node");
            rect.position = newPosition;
        }

        public void SetText(string newText)
        {
            if (newText != text)
            {
                Undo.RecordObject(this, "Update Dialogue Text");
                text = newText;
            }
        }

        public void RemoveChild(string childID)
        {
            Undo.RecordObject(this, "Remove Dialogue Link");
            children.Remove(childID);
        }

        public void AddChild(string childID)
        {
            Undo.RecordObject(this, "Add Dialogue Link");
            children.Add(childID);
        }

        public void SetIsPlayerSpeaking(bool val)
        {
            Undo.RecordObject(this, "Change Dialogue Speaking");
            isPlayerSpeaking = val;
        }
#endif
    }
}