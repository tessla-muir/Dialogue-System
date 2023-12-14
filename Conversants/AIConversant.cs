using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogue
{
    public class AIConversant : MonoBehaviour
    {
        PlayerConversant playerConversant;
        [SerializeField] Dialogue dialogue = null;
        [SerializeField] string conversantName = "";
        [SerializeField] Sprite[] conversantSprites;
        private Emotion conversantMood = Emotion.Neutral;

        void Start()
        {
            playerConversant = GameObject.FindObjectOfType<PlayerConversant>();
        }

        public string GetName()
        {
            return conversantName;
        }

        public void SetName(string newName)
        {
            conversantName = newName;
        }

        public void SetMood(Emotion newMood)
        {
            conversantMood = newMood; 
        }

        public void SetMood(int val)
        {
            conversantMood = (Emotion) val;
        }

        public Sprite GetConversantSprite()
        {
            return conversantSprites[(int) conversantMood];
        }

        public void StartDialogue()
        {
            playerConversant.StartDialogue(this, dialogue);
        }

        public Sprite GetSprite(int index)
        {
            return conversantSprites[index];
        }
    }
}
