using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

public class MyAdsInitialize : MonoBehaviour
{
    public static MyAdsInitialize instance;
    private bool isInitialized = false;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
#if UNITY_ADS
        Advertisement.Initialize("YOUR_GAME_ID", true);
        isInitialized = true;
#else
        isInitialized = true;
#endif
    }

    public bool IsInitialized() { return isInitialized; }
}
