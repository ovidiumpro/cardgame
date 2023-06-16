using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class MessageManager : MonoBehaviour 
{
    public Text MessageText;
    public GameObject MessagePanel;
    public GameObject MiniMessagePanel;
    public Text MiniMessageText;
    private CanvasGroup canvasGroup;
    private CanvasGroup miniCanvasGroup;

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
        MiniMessagePanel.SetActive(false);
        canvasGroup = MessagePanel.GetComponent<CanvasGroup>();
        miniCanvasGroup = MiniMessagePanel.GetComponent<CanvasGroup>();
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
                .OnComplete(() =>
                {
                    MessagePanel.SetActive(false);
                    canvasGroup.alpha = 1f; // reset the transparency to be ready for the next message

                });
    }
    IEnumerator RefreshMessage(string Message, float duration) {
        FadeOutMessage(duration);
        yield return new WaitForSeconds(duration);
        FadeInMessage(Message, duration);
    }
    

    private void FadeOutMessage(float fadeDuration)
    {
        miniCanvasGroup.DOFade(0, fadeDuration)
                .OnComplete(() =>
                {
                    MiniMessagePanel.SetActive(false);
                    miniCanvasGroup.alpha = 1f; // reset the transparency to be ready for the next message

                });
    }

    private void FadeInMessage(string Message, float fadeDuration)
    {
        MiniMessageText.text = Message;
        MiniMessagePanel.SetActive(true);

        // Fade in and scale up
        miniCanvasGroup.DOFade(1, fadeDuration);
        MiniMessageText.transform.DOScale(scaleValue, scaleDuration).SetLoops(2, LoopType.Yoyo);
    }
    public void RefreshMessagePanel(string Message, float duration) {
        StartCoroutine(RefreshMessage(Message, duration));
    }
    public void ShowMessageInstant(string Message) {
        MiniMessageText.text = Message;
        MiniMessagePanel.SetActive(true);
        miniCanvasGroup.alpha = 1f;
    }
    public void HideMessageInstant() {
        MiniMessagePanel.SetActive(false);
        miniCanvasGroup.alpha = 0f;
    }

    
}