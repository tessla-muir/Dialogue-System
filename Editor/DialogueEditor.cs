using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Game.Dialogue.Editor
{
    public class DialogueEditor : EditorWindow
    {
        Dialogue selectedDialogue = null;
        [System.NonSerialized] GUIStyle nodeStyle;
        [System.NonSerialized] GUIStyle playerNodeStyle;
        [System.NonSerialized] DialogueNode draggingNode = null;
        [System.NonSerialized] Vector2 draggingOffset;

        [System.NonSerialized] DialogueNode creatingNode = null;
        [System.NonSerialized] DialogueNode deletingNode = null;
        [System.NonSerialized] DialogueNode linkingParentNode = null;

        Vector2 scrollPosition; // Keep position through serialization
        [System.NonSerialized] Vector2 windowSize = new Vector2(600, 600);
        [System.NonSerialized] bool draggingCanvas = false;
        [System.NonSerialized] Vector2 draggingCanvasOffset;

        // Creates dialogue window
        [MenuItem("Window/Dialogue Editor")]
        private static void ShowEditorWindow()
        {
            var window = GetWindow<DialogueEditor>();
            window.titleContent = new GUIContent("Dialogue Editor");
            window.Show();
        }

        // Calls editor if a Dialogue SO is opened
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            // Get object from instance ID
            Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;

            if (dialogue != null)
            {
                ShowEditorWindow();
                return true;
            }
            return false;
        }

        // Called when an object becomes enabled and active
        private void OnEnable() {
            Selection.selectionChanged += OnSelectionChanged;

            // Style nodes
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
            nodeStyle.normal.textColor = Color.white;
            nodeStyle.padding = new RectOffset(20, 20, 20, 20);
            nodeStyle.border = new RectOffset(12, 12, 12, 12);

            playerNodeStyle = new GUIStyle();
            playerNodeStyle.normal.background = EditorGUIUtility.Load("node2") as Texture2D;
            playerNodeStyle.normal.textColor = Color.white;
            playerNodeStyle.padding = new RectOffset(20, 20, 20, 20);
            playerNodeStyle.border = new RectOffset(12, 12, 12, 12);
        }

        // Updates editor if a different Dialogue is selected
        private void OnSelectionChanged()
        {
            Dialogue newDialogue = Selection.activeObject as Dialogue;
            if (newDialogue != null)
            {
                ResetNodes();
                selectedDialogue = newDialogue;
                Repaint(); // Calls OnGUI
            }
        }

        // Reset nodes so you can't link between two different dialogues
        private void ResetNodes()
        {
            draggingNode = null;
            creatingNode = null;
            deletingNode = null;
            linkingParentNode = null;
        }

        private void OnGUI() {
            if (selectedDialogue == null)
            {
                EditorGUILayout.LabelField("No Dialogue Selected.");
            }
            else
            {
                ProcessEvents();
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                GUILayoutUtility.GetRect(windowSize.x, windowSize.y);

                // Draw Nodes after connections so that nodes are always on top of curves
                foreach(var node in selectedDialogue.GetAllNodes())
                {
                    DrawConnections(node);
                }
                foreach(var node in selectedDialogue.GetAllNodes())
                {
                    DrawNode(node);
                }
                EditorGUILayout.EndScrollView();

                // Create node
                if (creatingNode != null)
                {
                    selectedDialogue.CreateNode(creatingNode);
                    creatingNode = null;
                }
                // Delete node
                if (deletingNode != null)
                {
                    selectedDialogue.DeleteNode(deletingNode);
                    deletingNode = null;
                }
            }
        }

        // Draws a given node onto the editor GUI
        private void DrawNode(DialogueNode node)
        {
            // Determine color based on if player is talking
            GUIStyle style = nodeStyle;
            if (node.IsPlayerSpeaking())
            {
                style = playerNodeStyle;
            }

            GUILayout.BeginArea(node.GetRect(), style);
            node.SetText(EditorGUILayout.TextField(node.GetText()));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                // We cannot edit a list while it's being iterated through
                // Signal - we need to make a new child node from the given node later
                creatingNode = node;
            }
            DrawLinkButtons(node);
            if (GUILayout.Button("Del"))
            {
                // We cannot edit a list while it's being iterated through
                // Signal - we need to delete this node later
                deletingNode = node;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        // Determines what version of the link button to draw on a given node
        private void DrawLinkButtons(DialogueNode node)
        {
            if (linkingParentNode == null)
            {
                if (GUILayout.Button("Link"))
                {
                    linkingParentNode = node;
                }
            }
            else if (linkingParentNode == node)
            {
                if (GUILayout.Button("Cancel"))
                {
                    linkingParentNode = null;
                }
            }
            else if (linkingParentNode.GetChildren().Contains(node.name))
            {
                if (GUILayout.Button("Unlink"))
                {
                    linkingParentNode.RemoveChild(node.name);
                    linkingParentNode = null;
                }
            }
            else
            {
                if (GUILayout.Button("Child"))
                {
                    linkingParentNode.AddChild(node.name);
                    linkingParentNode = null;
                }
            }
    
        }

        // Draws Bezier curves between parent and child nodes
        private void DrawConnections(DialogueNode node) 
        {
            float offset = 5f;
            Vector3 startPosition = new Vector2(node.GetRect().xMax - offset, node.GetRect().center.y);

            foreach (DialogueNode child in selectedDialogue.GetAllChildren(node))
            {
                Vector3 endPosition = new Vector2(child.GetRect().xMin + offset, child.GetRect().center.y);
                Vector3 controlPointOffset = new Vector2(100,0);
                controlPointOffset.x *= 0.8f;
                Handles.DrawBezier(startPosition, endPosition, startPosition + controlPointOffset, endPosition - controlPointOffset, Color.white, null, 4f);
            }
        }

        private void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown && draggingNode == null)
            {
                draggingNode = GetNodeAtPoint(Event.current.mousePosition + scrollPosition);
                if (draggingNode != null)
                {
                    // Dragging Node
                    draggingOffset = draggingNode.GetRect().position - Event.current.mousePosition;
                    Selection.activeObject = draggingNode;
                }
                else
                {
                    // Dragging Canvas
                    draggingCanvas = true;
                    draggingCanvasOffset = Event.current.mousePosition + scrollPosition;
                    Selection.activeObject = selectedDialogue;
                }
            }
            else if (Event.current.type == EventType.MouseDrag && draggingNode != null)
            {
                draggingNode.SetRectPosition(Event.current.mousePosition + draggingOffset);
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseDrag && draggingCanvas)
            {
                scrollPosition = draggingCanvasOffset - Event.current.mousePosition;
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseDown && draggingNode != null)
            {
                draggingNode = null;
            }
            else if (Event.current.type == EventType.MouseUp && draggingCanvas)
            {
                draggingCanvas = false;
            }

             AlterWindowSize();
        }

        // Updates window size as needed
        private void AlterWindowSize()
        {
            Vector2 maxSize = new Vector2(0, 0);
            
            foreach (DialogueNode node in selectedDialogue.GetAllNodes())
            {
                if (maxSize.x < node.GetRect().xMax)
                {
                    maxSize.x = node.GetRect().xMax;
                }
                if (maxSize.y < node.GetRect().yMax)
                {
                    maxSize.y = node.GetRect().yMax;
                }
                windowSize = maxSize + new Vector2(200, 200);
            }
        }

        // If available, gets a node at a given mouse position
        private DialogueNode GetNodeAtPoint(Vector2 mousePosition)
        {
            DialogueNode foundNode = null;
            foreach (DialogueNode node in selectedDialogue.GetAllNodes())
            {
                if (node.GetRect().Contains(mousePosition))
                {
                    foundNode = node; // Keeps searching to find the top layer node
                }
            }
            return foundNode;
        }
    }
}

