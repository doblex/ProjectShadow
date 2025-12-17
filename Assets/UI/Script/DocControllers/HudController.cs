using System;
using UnityEngine.UIElements;

public class HudController : BaseDocController
{
    Button pauseButton;

    Button ability1;
    Button ability2;
    Button ability3;
    Button ability4;

    Label Task;

    protected override bool SetComponents()
    {
        bool bInit = base.SetComponents();

        pauseButton = Root.Q<Button>("Pause");
        pauseButton.clicked += OnPause;

        ability1 = Root.Q<Button>("Ab1");
        ability1.clicked += Ability1;

        ability2 = Root.Q<Button>("Ab2");
        ability2.clicked += Ability2;

        ability3 = Root.Q<Button>("Ab3");
        ability3.clicked += Ability3;

        ability4 = Root.Q<Button>("Ab4");
        ability4.clicked += Ability4;

        Task = Root.Q<Label>("Task");

        return bInit;
    }

    public void AddTask(string text)
    { 
        Task.text = text;
    }

    private void Ability4()
    {
        ActionManager.Instance.OnAbility(4);
    }

    private void Ability3()
    {
        ActionManager.Instance.OnAbility(3);
    }

    private void Ability2()
    {
        ActionManager.Instance.OnAbility(2);
    }

    private void Ability1()
    {
        ActionManager.Instance.OnAbility(1);
    }

    private void OnPause()
    {
        ActionManager.Instance.OnPause();
    }
}