using System;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UIElements;

public class HudController : BaseDocController
{
    Button pauseButton;
    string lockedClass = "Locked";

    Button ability1;
    float ab1Cooldown;
    float ab1Timer;

    Button ability2;
    float qty;

    Button ability3;
    float ab3Cooldown;
    float ab3Timer;

    Button ability4;
    float ab4Cooldown;
    float ab4Timer;

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

    public override void ShowDoc(bool show, bool force = false)
    {
        base.ShowDoc(show, force);

        if (show)
        {
            AbilityController ability = FindFirstObjectByType<AbilityController>();
            if (ability != null)
            { 
                ab1Cooldown = ability.GetStonThrowTotalCooldown();
                ab3Cooldown = ability.GetWhistleTotalCooldown();
                ab4Cooldown = ability.GetRBaitTotalCooldown();
                qty = ability.GetIBaitCount();
            }
        }
    }

    private void Update()
    {
        AbilityController ability = FindFirstObjectByType<AbilityController>();

        if (ability == null) return;


        qty = ability.GetIBaitCount();

        if (qty > 0)
        {
            ability2.RemoveFromClassList(lockedClass);
        }
        else
        {
            ability2.AddToClassList(lockedClass);
        }


        if (ab4Timer > 0)
        {
            ab4Timer -= Time.deltaTime;
        }

        if (ab4Timer <= 0)
        {
            ability4.RemoveFromClassList(lockedClass);
        }

        if (ab3Timer > 0)
        {
            ab3Timer -= Time.deltaTime;
        }

        if (ab3Timer <= 0)
        {
            ability3.RemoveFromClassList(lockedClass);
        }

        if (ab1Timer > 0)
        {
            ab1Timer -= Time.deltaTime;
        }

        if (ab1Timer <= 0)
        {
            ability1.RemoveFromClassList(lockedClass);
        }
    }

    public void AddTask(string text)
    { 
        Task.text = text;
    }

    private void Ability4()
    {
        ActionManager.Instance.OnAbility(4);
        ab4Timer = ab4Cooldown;

        ability4.AddToClassList(lockedClass);
    }

    private void Ability3()
    {
        ActionManager.Instance.OnAbility(3);
        ab3Timer = ab3Cooldown;

        ability3.AddToClassList(lockedClass);

    }

    private void Ability2()
    {
        ActionManager.Instance.OnAbility(2);
    }

    private void Ability1()
    {
        ActionManager.Instance.OnAbility(1);
        ab1Timer = ab1Cooldown;

        ability1.AddToClassList(lockedClass);
    }

    private void OnPause()
    {
        ActionManager.Instance.OnPause();
    }
}