using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

[Serializable]
public struct Template
{
    public string templateName;
    public VisualTreeAsset asset;
}

[Serializable]
public struct CharacterScript
{

}

[Serializable]
public struct Dialogue
{
    public string title;
    public string character;
    public string text;
    public string nextTitle;
    public List<Choice> choices;

    public Dialogue(string title, string character, string text, string nextTitle, List<Choice> choices)
    {
        this.title = title;
        this.character = character;
        this.text = text;
        this.nextTitle = nextTitle;
        this.choices = choices;
    }
}

[Serializable]
public struct Choice
{
    public string text;
    public string nextDialogueTitle;

    public Choice(string text, string nextDialogueTitle)
    {
        this.text = text;
        this.nextDialogueTitle = nextDialogueTitle;
    }
}

