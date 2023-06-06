using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class MessageManager : MonoBehaviour 
{
    public Text MessageText;
    public GameObject MessagePanel;
    private CanvasGroup canvasGroup;

    // Serialized fields
    [SerializeField]
    private float fadeDuration = 1f; // default value is 1
    [SerializeField]
    private float scaleDuration = 1f; // default value is 1
    [SerializeField]
    private float scaleValue = 1.2f; // default value is 1.2

    public static MessageManager Instance;

    void Awake()
    {
        Instance = this;
        MessagePanel.SetActive(false);
        canvasGroup = MessagePanel.GetComponent<CanvasGroup>();
    }

    public void ShowMessage(string Message, float Duration)
    {
        StartCoroutine(ShowMessageCoroutine(Message, Duration));
        
    }

    IEnumerator ShowMessageCoroutine(string Message, float Duration)
    {
        MessageText.text = Message;
        MessagePanel.SetActive(true);

        // Fade in and scale up
        canvasGroup.DOFade(1, fadeDuration);
        MessageText.transform.DOScale(scaleValue, scaleDuration).SetLoops(2, LoopType.Yoyo);

        yield return new WaitForSeconds(Duration);
        Command.CommandExecutionComplete();
        // Fade out
        canvasGroup.DOFade(0, fadeDuration)
        .OnComplete(() => {
            MessagePanel.SetActive(false);
            canvasGroup.alpha = 1f; // reset the transparency to be ready for the next message
            
        });
    }
    
}