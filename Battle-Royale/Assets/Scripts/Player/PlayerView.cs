using UnityEngine;
using Photon.Pun;

public class PlayerView : MonoBehaviourPunCallbacks
{
    public Rigidbody2D Rb { get; private set; }
    public PhotonView Pv { get; private set; }
    public Animator Animator { get; private set; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Pv = GetComponent<PhotonView>();
        Animator = GetComponent<Animator>();
    }

    public void UpdateMovement(Vector2 velocity)
    {
        Rb.velocity = velocity;
        Rb.rotation = 0f;
    }

    public void UpdateAnimator(bool isMoving, bool isDashing, bool isHealing, float horizontalDirection)
    {
        Animator.SetBool("IsMoving", isMoving);
        Animator.SetBool("IsDashing", isDashing);
        Animator.SetBool("IsHealing", isHealing);
        if (horizontalDirection != 0)
        {
            Animator.SetFloat("HorizontalDirection", horizontalDirection);
        }
    }
}