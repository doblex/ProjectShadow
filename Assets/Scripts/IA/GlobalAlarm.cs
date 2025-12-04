using System;
using UnityEngine;

public static class GlobalAlarm
{
    public static event Action<Vector3> OnPlayerSpotted;

    public static void RaiseAlarm(Vector3 position)
    {
        OnPlayerSpotted?.Invoke(position);
    }
}