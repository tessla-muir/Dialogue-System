using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Core;

namespace Game.Dialogue
{
    public class PlayerConversant : MonoBehaviour
    {
        [SerializeField] string playerName;
        [SerializeField] Sprite[] playerSprites;
        private Emotion playerMood = Emotion.Neutral;

        [SerializeField] GameObject DialogueUI;
        Dialogue currentDialogue;
        DialogueNode currentNode = null;
        AIConversant currentConversant = null;
        bool isChoosing = false;
        bool hasSingleChoice = false;

        public event Action onConversationUpdated;

        void Awake()
        {
            // Activate the UI for setup
            DialogueUI.SetActive(true);
        }

        // Starts the given dialogue through the AIConversant
        public void StartDialogue(AIConversant newConversant, Dialogue newDialogue)
        {
            DialogueUI.SetActive(true);
            currentConversant = newConversant;
            currentDialogue = newDialogue;
            currentNode = currentDialogue.GetRootNode();
            TriggerEnterActions();
            onConversationUpdated();
        }

        // Stops dialogue, resetting values
        public void Quit()
        {
            currentDialogue = null;
            TriggerExitActions();
            currentNode = null;
            isChoosing = false;
            hasSingleChoice = false;
            currentConversant = null;
            onConversationUpdated();
        }

        public bool IsChoosing()
        {
            return isChoosing;
        }

        public bool HasSingleChoice()
        {
            return hasSingleChoice;
        }

        public string GetText()
        {
            if (currentNode == null)
            {
                return "";
            }

            return currentNode.GetText();
        }

        public bool IsActive()
        {
            return currentDialogue != null;
        }

        public IEnumerable<DialogueNode> GetChoices()
        {
            return FilterOnCondition(currentDialogue.GetPlayerChildren(currentNode));
        }

        // Update chosenNode to the current node and continue dialogue
        public void SelectChoice(DialogueNode chosenNode)
        {
            currentNode = chosenNode;
            TriggerEnterActions();
            isChoosing = false;

            // Skip to next node
            Next();
        }

        // Sets the current node to the first child node
        public void Next()
        {
            DialogueNode[] childNodes = FilterOnCondition(currentDialogue.GetAllChildren(currentNode)).ToArray();

            // Player text
            if (FilterOnCondition(currentDialogue.GetPlayerChildren(currentNode)).Count() == 1)
            {
                hasSingleChoice = true;
                NextText(childNodes);
                return;
            }
            // Player choices
            else if (FilterOnCondition(currentDialogue.GetPlayerChildren(currentNode)).Count() > 0)
            {
                hasSingleChoice = false; // Only needed for two consecutive choice lists
                TriggerExitActions();
                isChoosing = true;
                onConversationUpdated();
                return;
            }

            // AI text
            hasSingleChoice = false;
            NextText(childNodes);
        }

        // Triggers actions and moves to next dialogue node
        private void NextText(DialogueNode[] childNodes)
        {
            TriggerExitActions();
            currentNode = childNodes[0];
            TriggerEnterActions();
            onConversationUpdated();
        }

        // Returns true if the dialogue continues after this node (thus has a child node)
        public bool HasNext()
        {
            if (FilterOnCondition(currentDialogue.GetAllChildren(currentNode)).Count() > 0)
            {
                return true;
            }
            return false;
        }

        // Returns name of conversant for current node
        public string GetConversantName()
        {
            if (isChoosing || hasSingleChoice)
            {
                return playerName;
            }
            else
            {
                return currentConversant.GetName();
            }
        }

        // Returns sprite of conversant for current node
        public Sprite GetConversantSprite()
        {
            if (isChoosing || hasSingleChoice)
            {
                return playerSprites[(int)playerMood];
            }
            else
            {
                return currentConversant.GetConversantSprite();
            }
        }

        public Dialogue GetCurrentDialogue()
        {
            return currentDialogue;
        }

        public void SetMood(Emotion newMood)
        {
            playerMood = newMood;
        }

        public void SetMood(int val)
        {
            playerMood = (Emotion)val;
        }

        // Filters a list of nodes based on the condition of the evaluators
        private IEnumerable<DialogueNode> FilterOnCondition(IEnumerable<DialogueNode> inputNode)
        {
            foreach (var node in inputNode)
            {
                if (node.CheckCondition(GetEvaluators()))
                {
                    yield return node;
                }
            }
        }

        // Returns list of evaluators for the current node
        private IEnumerable<IPredicateEvaluator> GetEvaluators()
        {
            return GetComponents<IPredicateEvaluator>();
        }

        private void TriggerEnterActions()
        {
            if (currentNode != null)
            {
               TriggerAction(currentNode.GetOnEnterActions());
            }
        }

        private void TriggerExitActions()
        {
            if (currentNode != null)
            {
               TriggerAction(currentNode.GetOnExitActions());
            }
        }

        // Triggers the response to a given dialogue action
        private void TriggerAction(IEnumerable<DialogueAction> actions)
        {
            // Gets list of actions for dialogue triggers from current AI Conversant
            // Could add a general list of dialogue triggers
            DialogueTrigger dialogueTrigger = currentConversant.GetComponent<DialogueTrigger>();

            foreach (DialogueAction action in actions)
            {
                if (action == DialogueAction.None) continue;

                foreach (var trigger in dialogueTrigger.Triggers)
                {
                    trigger.TriggerAction(action);
                }
            }
        }
    }
}
