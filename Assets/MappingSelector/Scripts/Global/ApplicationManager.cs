using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    public bool m_DontDestroy = false;
    public bool m_CursorVisible = false;
    public int m_FrameRateLimit = 60;


    [Header("Singleton")]
    private static ApplicationManager m_Instance = null;
    public static ApplicationManager Instance
    {
        get { return m_Instance; }
    }

    void Awake()
    {
        if (m_DontDestroy)
        {
            CheckSingleton();
        }
    }

    private void Start()
    {
        //Limit the frame rate to save heat and energy
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = m_FrameRateLimit; //En unity se acerca mas o menos. No es estricto. +-10 frames

        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Hide mouse
        Cursor.visible = m_CursorVisible;

    }

    void Update()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("---Escape pressed: Quitting app---");
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.LeftShift))
        {
            Cursor.visible = !Cursor.visible;
        }
#endif
    }

    private void CheckSingleton() 
    {
        if (m_Instance != null && m_Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            m_Instance = this;
        }

        //Unparent for the sake of the DontDestroyOnLoad
        this.transform.parent = null;
        DontDestroyOnLoad(this.gameObject);
    }
}
