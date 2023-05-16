using UnityEngine;
public class PositionManager : MonoBehaviour
{
    private Transform FirstSlot;
    private Transform LastSlot;
    private Vector3 firstPos, lastPos;
    [SerializeField] private float maxRotation = 20f;  // maximum rotation for first and last cards
    [SerializeField] private float radiusFactor = 0.5f;  // adjust this value to control the spread of the cards
    [SerializeField] private int angleFactor = 12;
    [SerializeField] private float yCorrectionFactor = 2f;
    // [SerializeField] private float lengthPerCard = 1f;
    // [SerializeField] private float maxHandLength = 8f;

    private void Awake()
    {
        UpdateFirstAndLastSlots();
        firstPos = FirstSlot.position;
        lastPos = LastSlot.position;
        PositionSlots();
        
    }

    private void OnTransformChildrenChanged()
    {
        UpdateFirstAndLastSlots();
        PositionSlots();
    }
    
    private void PositionSlots()
    {
        if (transform.childCount < 2)
        {
            return;
        }

        float firstRotZ = maxRotation;
        float lastRotZ = -maxRotation;
        float radius = ((lastPos - firstPos).magnitude / 2f) * radiusFactor;  // radius of the arc
        Vector3 centerPos = (lastPos + firstPos) / 2f;  // center of the arc

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            float t = (float)i / (transform.childCount - 1);

            // Calculate angle in radians
            float angle = Mathf.Lerp(-Mathf.PI / angleFactor, Mathf.PI / angleFactor, t);

            // Calculate position on the arc
            Vector3 posOnArc = new Vector3(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle) + centerPos.y * yCorrectionFactor, 0f);
            child.position = centerPos + posOnArc;

            // Calculate rotation
            float rotZ = Mathf.Lerp(firstRotZ, lastRotZ, t);
            child.rotation = Quaternion.Euler(0, 0, rotZ);
        }
    }
    private void UpdateFirstAndLastSlots()
    {
        if (transform.childCount > 0)
        {
            FirstSlot = transform.GetChild(0);
            LastSlot = transform.GetChild(transform.childCount - 1);
        }
    }
}