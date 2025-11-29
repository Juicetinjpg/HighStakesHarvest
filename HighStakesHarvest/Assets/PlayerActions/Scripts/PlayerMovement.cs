using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private string farmDoggoName = "Farm Doggo";
    private Rigidbody2D rb;
    private Vector2 moveInput;
    public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Ensure the player never collides with the farm doggo but keeps colliding with everything else.
        Collider2D[] playerColliders = GetComponents<Collider2D>();
        if (playerColliders.Length > 0)
        {
            Collider2D[] dogColliders = FindObjectsOfType<Collider2D>();
            foreach (Collider2D dogCollider in dogColliders)
            {
                if (dogCollider.gameObject.name == farmDoggoName)
                {
                    foreach (Collider2D playerCollider in playerColliders)
                    {
                        Physics2D.IgnoreCollision(playerCollider, dogCollider, true);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        if (Input.GetAxis("Horizontal") < 0)
            GetComponent<SpriteRenderer>().flipX = true;
        else if (Input.GetAxis("Horizontal") > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }

            rb.linearVelocity = moveInput * speed; 
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
