using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Transform playerTrans;
    public Vector3 playerStartPosition;
    public Vector3 menuPosition;

    [Header("Evenet Monitor")]
    public SceneLoadEventSO loadEventSO;
    public VoidEventSO newGameEvent;

    [Header("Annoucement")]
    public VoidEventSO afterSceneLoadedEvent;
    public FadeEventSO fadeEvent;
    public SceneLoadEventSO unloadedSceneEvent;

    [Header("Scenes")]
    public GameSceneSO firstLoadScene;
    public GameSceneSO menuScene;
    private GameSceneSO currentLoadScene;
    private GameSceneSO sceneToLoad;
    private Vector3 positionToGo;
    private bool fadeScreen;
    private bool isLoading;

    public float fadeDuration;

    private void Awake()
    {
        //Addressables.LoadSceneAsync(firstLoadScene.sceneReference, LoadSceneMode.Additive);
        //currentLoadScene = firstLoadScene;
        //currentLoadScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
    }

    private void Start()
    {
        //NewGame();
        loadEventSO.RaiseLoadRequestEvent(menuScene, menuPosition, true);
    }

    private void OnEnable()
    {
        loadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        newGameEvent.OnEventRaised += NewGame;
    }

    private void OnDisable()
    {
        loadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        newGameEvent.OnEventRaised -= NewGame;
    }

    private void NewGame()
    {
        sceneToLoad = firstLoadScene;
        //OnLoadRequestEvent(sceneToLoad, playerStartPosition, true);
        loadEventSO.RaiseLoadRequestEvent(sceneToLoad, playerStartPosition, true);
    }

    /// <summary>
    /// Event Load Request
    /// </summary>
    /// <param name="locationToLoad"></param>
    /// <param name="posToGo"></param>
    /// <param name="fadeScreen"></param>
    private void OnLoadRequestEvent(GameSceneSO locationToLoad, Vector3 posToGo, bool fadeScreen)
    {
        if (isLoading)
        {
            return;
        }
        isLoading = true;
        sceneToLoad = locationToLoad;
        positionToGo = posToGo;
        this.fadeScreen = fadeScreen;

        if (currentLoadScene != null)
        {
            StartCoroutine(UnloadPreviousScene());
        }
        else
        {
            LoadNewScene();
        }
    }

    private IEnumerator UnloadPreviousScene()
    {
        if (fadeScreen)
        {
            fadeEvent.FadeIn(fadeDuration); //场景变黑
        }

        yield return new WaitForSeconds(fadeDuration);//等待场景完全变黑

        //hide health stat bar
        unloadedSceneEvent.RaiseLoadRequestEvent(sceneToLoad, positionToGo, true);

        yield return currentLoadScene.sceneReference.UnLoadScene();

        playerTrans.gameObject.SetActive(false); //deactivate player让玩家无法在加载时看到角色

        LoadNewScene();
    }

    private void LoadNewScene()
    {
        var LoadingOperation = sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
        LoadingOperation.Completed += OnLoadCompleted; //after loading is completed, trigger OnLoadCompleted fucntion
    }

    private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> obj)
    {
        currentLoadScene = sceneToLoad;

        playerTrans.position = positionToGo; //move player to designated spot

        if (fadeScreen)
        {
            fadeEvent.FadeOut(fadeDuration);
        }

        isLoading = false;
        playerTrans.gameObject.SetActive(true);

        if(currentLoadScene.sceneType != SceneType.Menu)
        {
        afterSceneLoadedEvent.RaiseEvent();
        }
    }
}
