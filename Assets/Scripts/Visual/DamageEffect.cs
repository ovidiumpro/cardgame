using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// This class will show damage dealt to creatures or payers
/// </summary>

public class DamageEffect : MonoBehaviour
{

    // an array of sprites with different blood splash graphics
    public Sprite[] Splashes;

    // a UI image to show the blood splashes
    public Image DamageImage;

    // CanvasGropup should be attached to the Canvas of this damage effect
    // It is used to fade away the alpha value of this effect

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text damageText;

    [SerializeField] private float displayDuration = 0.7f; // how long to display before starting fade
    [SerializeField] private float fadeDuration = 0.3f; // how long to fade


    private void Start()
    {
        // DamageImage.sprite = Splashes[Random.Range(0, Splashes.Length)];  
        // Start the sequence immediately when the DamageEffect is instantiated
        StartEffectSequence();
    }

    public void SetDamage(int damage)
    {
        damageText.text = damage.ToString();
    }

    private void StartEffectSequence()
    {
        // Display for a duration then start the fade sequence
        DOVirtual.DelayedCall(displayDuration, () =>
        {
            FadeAndDestroy();
        });
    }

    private void FadeAndDestroy()
    {
        // Fade out and then destroy the game object
        canvasGroup.DOFade(0, fadeDuration).OnComplete(() => Destroy(gameObject));
    }


    public static void CreateDamageEffect(GameObject parent, int amount)
    {
        // // Instantiate a DamageEffect from prefab
        GameObject newDamageEffect = GameObject.Instantiate(GlobalSettings.Instance.DamageEffectPrefab, parent.transform.position, Quaternion.identity, parent.transform) as GameObject;
        newDamageEffect.transform.localPosition = new Vector3(0, 0, -0.1f);
        // // Get DamageEffect component in this new game object
        DamageEffect de = newDamageEffect.GetComponent<DamageEffect>();
        // // Change the amount text to reflect the amount of damage dealt
        de.SetDamage(-amount);


    }
}
