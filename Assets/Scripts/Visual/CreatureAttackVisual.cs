using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CreatureAttackVisual : MonoBehaviour
{
    private OneCreatureManager manager;
    private WhereIsTheCardOrCreature w;

    void Awake()
    {
        manager = GetComponent<OneCreatureManager>();
        w = GetComponent<WhereIsTheCardOrCreature>();
    }

    public void AttackTarget(int targetUniqueID, int damageTakenByTarget, int damageTakenByAttacker, int attackerHealthAfter, int targetHealthAfter)
    {
        Debug.Log(targetUniqueID);
        manager.CanAttackNow = false;
        GameObject target = IDHolder.GetGameObjectWithID(targetUniqueID);

        // bring this creature to front sorting-wise.
        w.BringToFront();
        VisualStates tempState = w.VisualState;
        w.VisualState = VisualStates.Transition;
        //TODO : Make all of this a sequence, when creature reaches target, create the damage effect, then transition back to og position
        //Sequence s = DOTween.Sequence();
        Sequence s = DOTween.Sequence();
        s.Append(transform.DOMove(new Vector3(transform.position.x, transform.position.y, transform.position.z - 2), 0.2f).SetEase(Ease.Linear));
        // Step 1: Move object to target with ease
        s.Append(transform.DOMove(target.transform.position, 0.4f).SetEase(Ease.InExpo));
        

        // Step 2: Create an impact effect
        s.Append(transform.DOShakePosition(0.1f,1,50));

        // Step 3: Show the damage effect and update health values
        s.AppendCallback(() =>
        {
            if (damageTakenByTarget > 0)
                DamageEffect.CreateDamageEffect(target, damageTakenByTarget);
            
            if (damageTakenByAttacker > 0) {
                manager.HealthText.text = attackerHealthAfter.ToString();
                 DamageEffect.CreateDamageEffect(this.gameObject, damageTakenByAttacker);
            }
            
            if (targetUniqueID == GlobalSettings.Instance.LowPlayer.PlayerID || targetUniqueID == GlobalSettings.Instance.TopPlayer.PlayerID)
            {
                // target is a player
                target.GetComponent<PlayerPortraitVisual>().HealthText.text = targetHealthAfter.ToString();
            }
            else
                target.GetComponent<OneCreatureManager>().HealthText.text = targetHealthAfter.ToString();

        });
        s.AppendInterval(0.1f);

        // Step 4: Move object back to its original position
        s.Append(transform.DOMove(transform.position, 0.5f).SetEase(Ease.OutQuart));

        // Step 5: Perform rest of the code
        s.AppendCallback(() =>
        {
            w.SetTableSortingOrder();
            w.VisualState = tempState;

            
        });

        s.AppendInterval(0.3f);
        s.OnComplete(Command.CommandExecutionComplete);

        s.Play();


        // transform.DOMove(target.transform.position, 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InCubic).OnComplete(() =>
        //     {
        //         if(damageTakenByTarget>0)
        //             DamageEffect.CreateDamageEffect(target, damageTakenByTarget);
        //         if(damageTakenByAttacker>0)
        //             DamageEffect.CreateDamageEffect(this.gameObject, damageTakenByAttacker);

        //         if (targetUniqueID == GlobalSettings.Instance.LowPlayer.PlayerID || targetUniqueID == GlobalSettings.Instance.TopPlayer.PlayerID)
        //         {
        //             // target is a player
        //             target.GetComponent<PlayerPortraitVisual>().HealthText.text = targetHealthAfter.ToString();
        //         }
        //         else
        //             target.GetComponent<OneCreatureManager>().HealthText.text = targetHealthAfter.ToString();

        //         w.SetTableSortingOrder();
        //         w.VisualState = tempState;

        //         manager.HealthText.text = attackerHealthAfter.ToString();
        //         Sequence s = DOTween.Sequence();
        //         s.AppendInterval(1f);
        //         s.OnComplete(Command.CommandExecutionComplete);
        //         //Command.CommandExecutionComplete();
        //     });
    }

}
