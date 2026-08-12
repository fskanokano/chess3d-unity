using UnityEngine;

public class AppOpenAdController : MonoBehaviour
{
    public static AppOpenAdController instance;
    private bool isLoading, isShowing;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public void LoadAd()
    {
        isLoading = true;
        Debug.Log("AppOpenAd: loading");
    }

    public void ShowAd()
    {
        if (!isLoading) { LoadAd(); return; }
        isShowing = true;
        Debug.Log("AppOpenAd: showing");
    }
}
