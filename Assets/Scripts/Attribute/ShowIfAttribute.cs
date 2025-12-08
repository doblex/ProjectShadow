using UnityEngine;


/// <summary>
/// Attribute to conditionally show a field in the Unity Inspector based on a boolean value.
/// </summary>
public class ShowIfAttribute : PropertyAttribute
{
    public string BoolName;
    public bool ExpectedValue;

    public ShowIfAttribute(string boolName, bool expectedValue = true)
    {
        this.BoolName = boolName;
        this.ExpectedValue = expectedValue;
    }
}

