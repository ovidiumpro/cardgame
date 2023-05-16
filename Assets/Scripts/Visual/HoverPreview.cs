using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class HoverPreview : MonoBehaviour
{
    // PUBLIC FIELDS
    public GameObject TurnThisOffWhenPreviewing;  // if this is null, will not turn off anything 
    public Vector3 TargetPosition;
    public float TargetScale;
    public GameObject previewGameObject;
    public bool ActivateInAwake = false;

    // PRIVATE FIELDS
    private static HoverPreview currentlyViewing = null;

    // PROPERTIES WITH UNDERLYING PRIVATE FIELDS
    private static Dictionary<int, Sequence> runningSequences = new Dictionary<int, Sequence>();
    private static bool _PreviewsAllowed = true;
    public static bool PreviewsAllowed
    {
        get { return _PreviewsAllowed; }

        set
        {
            //Debug.Log("Hover Previews Allowed is now: " + value);
            _PreviewsAllowed = value;
            if (!_PreviewsAllowed)
                StopAllPreviews();
        }
    }

    private bool _thisPreviewEnabled = false;
    public bool ThisPreviewEnabled
    {
        get { return _thisPreviewEnabled; }

        set
        {
            _thisPreviewEnabled = value;
            if (!_thisPreviewEnabled)
                StopThisPreview();
        }
    }

    public bool OverCollider { get; set; }

    // MONOBEHVIOUR METHODS
    void Awake()
    {
        ThisPreviewEnabled = ActivateInAwake;
    }

    void OnMouseEnter()
    {
        OverCollider = true;
        if (PreviewsAllowed && ThisPreviewEnabled)
            PreviewThisObject();
    }

    void OnMouseExit()
    {
        OverCollider = false;

        if (!PreviewingSomeCard())
            StopAllPreviews();
    }

    // OTHER METHODS
    void PreviewThisObject()
    {
        StopAllPreviews();

        currentlyViewing = this;

        previewGameObject.SetActive(true);

        if (TurnThisOffWhenPreviewing != null)
            TurnThisOffWhenPreviewing.SetActive(false);

        previewGameObject.transform.localPosition = Vector3.zero;
        previewGameObject.transform.localScale = Vector3.one;
        // Set rotation to match the inverse rotation of the world
        Vector3 previewRotation = transform.eulerAngles;
        previewRotation.z = 0;
        previewGameObject.transform.rotation = Quaternion.Euler(previewRotation);



        // Create or restart the sequence for this HoverPreview
        if (!runningSequences.ContainsKey(GetInstanceID()))
        {
            runningSequences[GetInstanceID()] = DOTween.Sequence();
        }
        else
        {
            runningSequences[GetInstanceID()].Restart();
        }

        Sequence mySequence = runningSequences[GetInstanceID()];
        mySequence.Kill();
        mySequence = DOTween.Sequence();
        mySequence.Append(previewGameObject.transform.DOLocalMove(TargetPosition, 1f).SetEase(Ease.OutQuint));
        mySequence.Join(previewGameObject.transform.DOScale(TargetScale, 1f).SetEase(Ease.OutQuint));
    }

    void StopThisPreview()
    {
        if (runningSequences.ContainsKey(GetInstanceID()))
        {
            Sequence mySequence = runningSequences[GetInstanceID()];
            mySequence.Kill();
            mySequence = DOTween.Sequence();
            mySequence.Append(previewGameObject.transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.InQuint));
            mySequence.Join(previewGameObject.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuint));
            mySequence.AppendCallback(() =>
            {
                previewGameObject.SetActive(false);
                if (TurnThisOffWhenPreviewing != null)
                    TurnThisOffWhenPreviewing.SetActive(true);
            });
        }
        else
        {
            previewGameObject.SetActive(false);
            previewGameObject.transform.localScale = Vector3.one;
            previewGameObject.transform.localPosition = Vector3.zero;
            if (TurnThisOffWhenPreviewing != null)
                TurnThisOffWhenPreviewing.SetActive(true);
        }
    }

    // STATIC METHODS
    private static void StopAllPreviews()
    {
        if (currentlyViewing != null)
        {
            // Call StopThisPreview() to handle stopping and animations properly
            currentlyViewing.StopThisPreview();
            // Clear the currentlyViewing variable
            currentlyViewing = null;
        }

    }

    private static bool PreviewingSomeCard()
    {
        if (!PreviewsAllowed)
            return false;

        HoverPreview[] allHoverBlowups = GameObject.FindObjectsOfType<HoverPreview>();

        foreach (HoverPreview hb in allHoverBlowups)
        {
            if (hb.OverCollider && hb.ThisPreviewEnabled)
                return true;
        }

        return false;
    }


}
