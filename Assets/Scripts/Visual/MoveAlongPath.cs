using UnityEngine;
using DG.Tweening;

public class MoveAlongPath : MonoBehaviour
{
    [SerializeField] private DOTweenPath path;
    [SerializeField] private float duration = 5f;

    private void Start()
    {
        Vector3[] pathPoints = path.wps.ToArray();
        transform.DOScale(Vector3.zero, 0);
        transform.DOPath(pathPoints, duration, path.pathType)
            .OnWaypointChange(waypointIndex => {
                transform.DOScale(Vector3.one * (waypointIndex + 1) / (float)pathPoints.Length, 0.5f);
            });
    }
}