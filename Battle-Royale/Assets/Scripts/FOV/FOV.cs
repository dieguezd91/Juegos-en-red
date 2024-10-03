using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class FOV : MonoBehaviour
{
    [SerializeField] private float range;
    //[SerializeField] private Transform pov;
    [SerializeField] private float angle;
    [SerializeField] private LayerMask obstacle;
    [SerializeField] private LayerMask viewTarget;
    [SerializeField] private float radius;

    private RaycastHit2D[] hits = new RaycastHit2D[17];
    private PhotonView pv;
    //[SerializeField] private float delayToLooseTarget;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Update()
    {

        Physics2D.CircleCastNonAlloc(transform.position, radius, transform.forward, hits, range, viewTarget);

            for (int i = 0; i < hits.Length; i++)
            {
                if(hits != null && hits[i] != false)
            {
                if (hits[i].rigidbody.gameObject == this.gameObject) continue;

                    if (CheckRange(hits[i].rigidbody.transform) && CheckAngle(hits[i].rigidbody.transform) && InView(hits[i].rigidbody.transform))
                    {
                    hits[i].rigidbody.TryGetComponent<SpriteRenderer>(out SpriteRenderer sprite);

                    sprite.enabled = true;
                    
                    //print("turn ON renderer");
                    //print("CheckRange: "+ CheckRange(hits[i].rigidbody.transform));
                    //print("CheckAngle: " + CheckAngle(hits[i].rigidbody.transform));
                    //print("InVew: " + InView(hits[i].rigidbody.transform));
                }
                    else
                    {
                    hits[i].rigidbody.TryGetComponent<SpriteRenderer>(out SpriteRenderer sprite);

                    sprite.enabled = false;
                    //print("turn off renderer");
                    }
                }
                else 
                { 
                //print("No viewTargets detected");
                break;
                }
            }
            
            
    }

    public bool CheckRange(Transform target)
    {
        float distanceToTarget = Vector2.Distance(target.position, Origin);
        return distanceToTarget <= range;
    }

    public bool CheckAngle(Transform target)
    {
        Vector2 dirToTarget = target.position - Origin;
        float angleToTarget = Vector3.Angle(dirToTarget, Foward);
        return angleToTarget <= angle / 2;
    }

    public bool InView(Transform target)
    {
        Vector2 dirToTarget = target.position - Origin;
        //print(Physics2D.Raycast(Origin, dirToTarget.normalized, dirToTarget.magnitude, obstacle).collider.name);
        return !Physics2D.Raycast(Origin, dirToTarget.normalized, dirToTarget.magnitude, obstacle);
    }
    Vector3 Origin
    {
        get
        {
            return transform.position;           

        }
    }

    Vector3 Foward
    {
        get
        {
            return transform.forward;            
        }
    }


    [PunRPC]
    private void HideFromView(SpriteRenderer target)
    {
        target.enabled = false;        
    }

    private void ShowInView(SpriteRenderer target)
    {
        target.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Origin, range);        

        Gizmos.color = Color.red;
        Gizmos.DrawRay(Origin, Quaternion.Euler(0, angle / 2, 0) * Foward * range);
        Gizmos.DrawRay(Origin, Quaternion.Euler(0, -angle / 2, 0) * Foward * range);
    }    
}
