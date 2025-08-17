using UnityEngine;
using Define_Enums;

public class Game_Data_manager : MonoBehaviour
{
    static Game_Data_manager instance;
    [Header("���� ����")]
    [SerializeField] public string PlayerName;
    [SerializeField] public ScenesKind NowScene;
    [SerializeField] public RSP_Result RSPResult;
    [SerializeField] public Role Role;
    [SerializeField] public Role WinnerRole;



    public static Game_Data_manager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
    }



}
