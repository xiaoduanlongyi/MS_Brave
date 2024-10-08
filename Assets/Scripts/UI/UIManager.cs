using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public PlayerStatBar playerStatBar;
    [Header("Event Monitor")]
    public CharacterEvent_SO healthEvent;
    public SceneLoadEventSO sceneLoadEvent;

    private void OnEnable()
    {
        healthEvent.OnEventRaised += OnHealthEvent;
        sceneLoadEvent.LoadRequestEvent += OnSceneLoadEvent;
    }

    private void OnDisable()
    {
        healthEvent.OnEventRaised -= OnHealthEvent;
        sceneLoadEvent.LoadRequestEvent -= OnSceneLoadEvent;
    }

    private void OnSceneLoadEvent(GameSceneSO sceneToLoad, Vector3 arg1, bool arg2)
    {
        var isMenu = sceneToLoad.sceneType == SceneType.Menu;
        playerStatBar.gameObject.SetActive(!isMenu);
    }

    private void OnHealthEvent(Character character)
    {
        var percentage = character.currentHealth / character.maxHealth;
        playerStatBar.OnHealthChange(percentage);

        playerStatBar.OnPowerChange(character);
    }
}
