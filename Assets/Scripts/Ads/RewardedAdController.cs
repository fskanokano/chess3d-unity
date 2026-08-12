using UnityEngine;

public class RewardedAdController : MonoBehaviour
{
    public static RewardedAdController instance;
    private System.Action onRewardedCallback;
    private bool isLoading;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public void LoadAd() { isLoading = true; }

    public void ShowAd(System.Action onRewarded)
    {
        onRewardedCallback = onRewarded;
        LoadAd();
        Debug.Log("RewardedAd: showing");
    }

    private void OnUserEarnedReward()
    {
        onRewardedCallback?.Invoke();
    }
}
