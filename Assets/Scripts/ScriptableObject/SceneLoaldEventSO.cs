using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/SceneLoadEventSO")]
public class SceneLoadEventSO : ScriptableObject
{
    public UnityAction<GameSceneSO, Vector3, bool> LoadRequestEvent;


    /// <summary>
    /// Request to load screen
    /// </summary>
    /// <param name="locationToLoad">The Scene to load </param>
    /// <param name="posToGo">The position to teleport the player </param>
    /// <param name="fadeScreen">whether or not to fade screen while loading </param>
    public void RaiseLoadRequestEvent(GameSceneSO locationToLoad, Vector3 posToGo, bool fadeScreen)
    {
        LoadRequestEvent?.Invoke(locationToLoad, posToGo, fadeScreen);
    }
}