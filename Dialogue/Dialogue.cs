using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Dialogue
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 0)]
    public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] List<DialogueNode> nodes = new List<DialogueNode>();
        Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();

        void Awake() 
        {
            OnValidate();
        }

        private void OnValidate()
        {
            nodeLookup.Clear(); // Start with a clean state
            foreach (DialogueNode node in GetAllNodes())
            {
                if (node != null)
                {
                    nodeLookup[node.name] = node;
                }
            }
        }

        public IEnumerable<DialogueNode> GetAllNodes()
        {
            return nodes;
        }

        public DialogueNode GetRootNode()
        {
            return nodes[0];
        }

        // Returns all child nodes for a given parent node
        public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parent)
        {
            // Dictonary for efficency
            foreach (string childID in parent.GetChildren())
            {
                if (nodeLookup.ContainsKey(childID))
                {
                    yield return nodeLookup[childID];
                }
            }
        }

        // Returns all player child nodes for current node
        public IEnumerable<DialogueNode> GetPlayerChildren(DialogueNode currentNode)
        {
            foreach (DialogueNode node in GetAllChildren(currentNode))
            {
                if (node.IsPlayerSpeaking())
                {
                    yield return node;
                }
            }
        }

        // Returns all AI child nodes for current node
        public IEnumerable<DialogueNode> GetAIChildren(DialogueNode currentNode)
        {
            foreach (DialogueNode node in GetAllChildren(currentNode))
            {
                if (!node.IsPlayerSpeaking())
                {
                    yield return node;
                }
            }
        }



#if UNITY_EDITOR
        // Creates a child node for the given parent node
        public void CreateNode(DialogueNode parent)
        {
            DialogueNode newNode = MakeNode(parent);
            newNode.SetIsPlayerSpeaking(!parent.IsPlayerSpeaking());
            Undo.RecordObject(this, "Added Dialogue Node");
            AddNode(newNode);
        }

        // Deletes a given node
        public void DeleteNode(DialogueNode nodeToRemove)
        {
            Undo.RecordObject(this, "Deleted Dialogue Node");
            nodes.Remove(nodeToRemove);
            OnValidate(); // Updates GUI
            CleanChildren(nodeToRemove);
            Undo.DestroyObjectImmediate(nodeToRemove);
        }

        // Remove connection of a given node to children
        public void CleanChildren(DialogueNode nodeToClean)
        {
            foreach (DialogueNode node in GetAllNodes())
            {
                node.RemoveChild(nodeToClean.name);
            }
        }

        
        private static DialogueNode MakeNode(DialogueNode parent)
        {
            DialogueNode newNode = CreateInstance<DialogueNode>();
            newNode.name = System.Guid.NewGuid().ToString();
            Undo.RegisterCreatedObjectUndo(newNode, "Created Dialogue Node");
            if (parent != null)
            {
                parent.AddChild(newNode.name);
                newNode.SetRectPosition(parent.GetRect().position + new Vector2(200, 0));
            }

            return newNode;
        }

        private void AddNode(DialogueNode newNode)
        {
            nodes.Add(newNode);
            OnValidate(); // Updates Bezier curves
        }
#endif

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (nodes.Count == 0)
            {
                DialogueNode newNode = MakeNode(null);
                AddNode(newNode);
            }

            if (AssetDatabase.GetAssetPath(this) != "")
            {
                foreach (DialogueNode node in GetAllNodes())
                {
                    if (AssetDatabase.GetAssetPath(node) == "")
                    {
                        AssetDatabase.AddObjectToAsset(node, this);
                    }
                }
            }
#endif
        }

        public void OnAfterDeserialize()
        {
            // Just needed for interface
        }
    }
}