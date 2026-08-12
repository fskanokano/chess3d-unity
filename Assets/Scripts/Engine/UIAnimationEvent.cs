using UnityEngine;

public class UIAnimationEvent : MonoBehaviour
{
    public void OnButtonClick()
    {
        AudioManager.instance?.PlayAudio(0);
    }

    public void OnPanelOpen()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Open");
    }

    public void OnPanelClose()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Close");
    }
}
