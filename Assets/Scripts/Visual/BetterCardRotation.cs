using UnityEngine;
using System.Collections;

/// <summary>
/// This script should be attached to the card game object to display card`s rotation correctly.
/// </summary>

[ExecuteInEditMode]
public class BetterCardRotation : MonoBehaviour {

    // parent game object for all the card face graphics
    public RectTransform CardFront;

    // parent game object for all the card back graphics
    public RectTransform CardBack;

    // an empty game object that is placed a bit above the face of the card, in the center of the card
    public Transform targetFacePoint;

    public LayerMask cardLayer;

    // 3d collider attached to the card (2d colliders like BoxCollider2D won`t work in this case)
    [SerializeField]
    private Collider col;
    private Transform mainCameraTransform;

    // if this is true, our players currently see the card Back
    private bool showingBack = false;

    void Start() {
        mainCameraTransform = Camera.main.transform;
    }

	// Update is called once per frame
	void Update () 
    {
        // Raycast from Camera to a target point on the face of the card
        // If it passes through the card`s collider, we should show the back of the card
        RaycastHit[] hits;
        hits = Physics.RaycastAll(origin: mainCameraTransform.position, 
                                  direction: (-mainCameraTransform.position + targetFacePoint.position).normalized, 
            maxDistance: (-mainCameraTransform.position + targetFacePoint.position).magnitude) ;
        bool passedThroughColliderOnCard = false;
        foreach (RaycastHit h in hits)
        {
            if (h.collider == col)
                passedThroughColliderOnCard = true;
        }
        //Debug.Log("TotalHits: " + hits.Length); 
        if (passedThroughColliderOnCard!= showingBack)
        {
            // something changed
            showingBack = passedThroughColliderOnCard;
            CardFront.gameObject.SetActive(!showingBack);
            CardBack.gameObject.SetActive(showingBack);
        }

	}
}
