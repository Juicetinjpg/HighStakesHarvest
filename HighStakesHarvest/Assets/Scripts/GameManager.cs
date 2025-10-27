//using UnityEditor.EditorTools;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TurnManager turnManager;
    public ToolsManager toolManager;
    public PlantManager plantManager;
    public CropManager cropManager;
    public PlayerManager playerManager;


    //public UIManager uiManager;

    void Awake()
    {
        // Make sure only one GameManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // persists between scenes

        // Initialize or find managers if not set
        if (turnManager == null)
            turnManager = GetComponentInChildren<TurnManager>();

        if (toolManager == null)
            toolManager = GetComponentInChildren<ToolsManager>();


        if (plantManager == null)
            plantManager = GetComponentInChildren<PlantManager>();


        if (cropManager == null)
            cropManager = GetComponentInChildren<CropManager>();

        if (playerManager == null)
            playerManager = GetComponentInChildren<PlayerManager>();

        //if (uiManager == null)
        //    uiManager = GetComponentInChildren<UIManager>();
    }
}
