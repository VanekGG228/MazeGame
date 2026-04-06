using System;
using UnityEngine;

public static class EventManager
{
    public static Action<GameObject> OnPlayerSpawned;
    public static Action OnLevelFinished;
    public static Action OnLevelFailed;

    public static void RaisePlayerSpawned(GameObject player)
    {
        Debug.Log("Event: Player Spawned");
        OnPlayerSpawned?.Invoke(player);
    }

    public static void RaiseLevelFinished()
    {
        Debug.Log("Event: Level Finished");
        OnLevelFinished?.Invoke();
    }

    public static void RaiseLevelFailed()
    {
        Debug.Log("Event: Level Failed");
        OnLevelFailed?.Invoke();
    }
}