using UnityEngine;
using System.Collections;
using System;

public abstract class DraggingActions : MonoBehaviour {

    private Transform TargetTransform;

    public abstract Transform GetTargetTransform();
    public abstract void OnStartDrag();

    public abstract void OnEndDrag();

    public abstract void OnDraggingInUpdate();

    public static Action DragFailed =delegate {};

    public virtual bool CanDrag
    {
        get
        {            
            return GlobalSettings.Instance.CanControlThisPlayer(playerOwner);
        }
    }

    protected virtual Player playerOwner
    {
        get{
            
            if (tag.Contains("Low"))
                return GlobalSettings.Instance.LowPlayer;
            else if (tag.Contains("Top"))
                return GlobalSettings.Instance.TopPlayer;
            else
            {
                Debug.LogError("Untagged Card or creature " + transform.parent.name);
                return null;
            }
        }
    }

    protected abstract bool DragSuccessful();

    public static void UpdateTargetPosition(Transform transform, Transform TargetGO, LineRenderer lr, SpriteRenderer triangleSR) {
        // This code only draws the arrow
        Vector3 notNormalized = TargetGO.position - transform.position;
        Vector3 direction = notNormalized.normalized;
        float distanceToTarget = (direction * 2.3f).magnitude;
        if (notNormalized.magnitude > distanceToTarget)
        {
            // draw a line between the creature and the target
            lr.SetPositions(new Vector3[] { transform.position, TargetGO.position - direction * 2.3f });
            lr.enabled = true;

            // position the end of the arrow between near the target.
            triangleSR.enabled = true;
            triangleSR.transform.position = TargetGO.position - 1.5f * direction;

            // proper rotarion of arrow end
            float rot_z = Mathf.Atan2(notNormalized.y, notNormalized.x) * Mathf.Rad2Deg;
            triangleSR.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        }
        else
        {
            // if the target is not far enough from creature, do not show the arrow
            lr.enabled = false;
            triangleSR.enabled = false;
        }
    }
    
}
