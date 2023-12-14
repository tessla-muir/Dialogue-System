using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Dialogue
{
    public enum DialogueAction
    {
        None,
        NameChange,
        NameReset,
        SpriteChange,
        SpriteReset,
        MoodChange,
        MoodReset,
        TextSizeChange,
        TextColorChange,
        TextReset,
        PlaySound
    }
}