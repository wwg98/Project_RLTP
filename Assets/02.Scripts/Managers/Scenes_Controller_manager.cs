using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Define_Enums;
using UnityEngine.SceneManagement;

public class Scenes_Controller_manager : MonoBehaviour
{
    static Scenes_Controller_manager _uniqueinstance;

    ScenesKind _nowLoadScene;
    Loading_UI_Manager _loading_window;
    AsyncOperation _loadProGress;

    RSP_Result RSP_result;

    public static Scenes_Controller_manager _instance
    {
        get { return _uniqueinstance; }
    }

    private void Awake()
    {
        if (_uniqueinstance == null)
        {
            _uniqueinstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        Screen.SetResolution(1920, 1080, true);
    }
    private void Start()
    {
        _nowLoadScene = ScenesKind.Title_Scene;
        Game_Data_manager.Instance.NowScene = _nowLoadScene;
    }

    #region[æ¿ ¿Ãµø]
    public void StartLobbyScene()
    {
        _nowLoadScene = ScenesKind.Lobby_Scene;
        Game_Data_manager.Instance.NowScene = _nowLoadScene;
        SceneManager.LoadScene((int)_nowLoadScene);
    }
    public void StartSettingScene()
    {
        _nowLoadScene = ScenesKind.Setting_Scene;
        Game_Data_manager.Instance.NowScene = _nowLoadScene;
        SceneManager.LoadScene((int)_nowLoadScene);

    }
    public void StartSmallMapScene()
    {
        _nowLoadScene = ScenesKind.Small_Map_Scene;
        Game_Data_manager.Instance.NowScene = _nowLoadScene;
        SceneManager.LoadScene((int)_nowLoadScene);

    }
    public void StartLargeMapScene()
    {
        _nowLoadScene = ScenesKind.Large_Map_Scene;
        Game_Data_manager.Instance.NowScene = _nowLoadScene;
        SceneManager.LoadScene((int)_nowLoadScene);

    }
    public void StartTitleScene()
    {
        _nowLoadScene = ScenesKind.Title_Scene;
        Game_Data_manager.Instance.NowScene = _nowLoadScene;
        SceneManager.LoadScene((int)_nowLoadScene);
    }
    #endregion

    #region [Get, Set]
    public void SetNowScene(ScenesKind LoadScene)
    {
        _nowLoadScene = LoadScene;
    }
    public ScenesKind GetNowScene()
    {
        return _nowLoadScene;
    }
    public void GetGameResult(RSP_Result result)
    {
        RSP_result = result;
    }

    public RSP_Result SetGameResult()
    {
        return RSP_result;
    }
    #endregion

    public void ResetCurrentScene()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(buildIndex);
    }

}
