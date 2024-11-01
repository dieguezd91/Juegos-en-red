using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class FOV : MonoBehaviourPunCallbacks
{
    [SerializeField] private float range;
    [SerializeField] private float angle;
    [SerializeField] private LayerMask obstacle;
    [SerializeField] private LayerMask viewTarget;
    [SerializeField] private float radius;

    private RaycastHit2D[] hits = new RaycastHit2D[17];
    private PhotonView photonView;
    private Dictionary<int, FOVReceiver> cachedReceivers = new Dictionary<int, FOVReceiver>();

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        CheckVisibility();
    }

    private void CheckVisibility()
    {
        int hitCount = Physics2D.CircleCastNonAlloc(transform.position, radius, transform.forward, hits, range, viewTarget);

        foreach (var receiver in cachedReceivers.Values)
        {
            if (receiver != null)
            {
                receiver.HideSprites();
            }
        }

        for (int i = 0; i < hitCount; i++)
        {
            if (hits[i].collider == null) continue;

            GameObject hitObject = hits[i].collider.gameObject;
            if (hitObject == gameObject) continue;

            FOVReceiver receiver = GetOrAddReceiver(hitObject);
            if (receiver == null) continue;

            if (IsTargetVisible(hitObject.transform))
            {
                receiver.ShowSprites();
            }
        }
    }

    private FOVReceiver GetOrAddReceiver(GameObject target)
    {
        PhotonView targetPV = target.GetComponent<PhotonView>();
        if (targetPV == null) return null;

        int viewID = targetPV.ViewID;

        if (!cachedReceivers.TryGetValue(viewID, out FOVReceiver receiver))
        {
            receiver = target.GetComponent<FOVReceiver>();
            if (receiver != null)
            {
                cachedReceivers[viewID] = receiver;
            }
        }

        return receiver;
    }

    private bool IsTargetVisible(Transform target)
    {
        return CheckRange(target) && CheckAngle(target) && InView(target);
    }

    public bool CheckRange(Transform target)
    {
        float distanceToTarget = Vector2.Distance(target.position, Origin);
        return distanceToTarget <= range;
    }

    public bool CheckAngle(Transform target)
    {
        Vector2 dirToTarget = (target.position - Origin).normalized;
        float angleToTarget = Vector2.Angle(dirToTarget, transform.right);
        return angleToTarget <= angle / 2;
    }

    public bool InView(Transform target)
    {
        Vector2 dirToTarget = target.position - Origin;
        return !Physics2D.Raycast(Origin, dirToTarget.normalized, dirToTarget.magnitude, obstacle);
    }

    private Vector3 Origin => transform.position;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Origin, range);

        Vector3 rightDir = transform.right;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Origin, Quaternion.Euler(0, 0, angle / 2) * rightDir * range);
        Gizmos.DrawRay(Origin, Quaternion.Euler(0, 0, -angle / 2) * rightDir * range);
    }
}