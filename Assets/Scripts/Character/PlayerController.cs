using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public LayerMask interactableLayer;
    public LayerMask solidObjectLayer;
    public LayerMask grassLayer;

    public event Action OnEncauntered;

    Vector2 input;
    bool isMoving = false;
    public float walkSpeed = 3f;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Allows player movement depeding on which key is pressed
    public void HandleUpdate()
    {
        if (!isMoving)
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            // No diagonal movement
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                input.y = 0;
            else
                input.x = 0;

            // Check that the input is non zero
            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPosition = transform.position;
                targetPosition.x += input.x;
                targetPosition.y += input.y;

                // Move on the map where you can
                if(isWalkable(targetPosition))
                    StartCoroutine(Move(targetPosition));
            }

            if (Input.GetKeyDown(KeyCode.A))
                Interact();
        }

        animator.SetBool("isMoving", isMoving);
    }

    // Function that moves player on the map
    IEnumerator Move(Vector3 targetPosition)
    {
        isMoving = true;

        while((targetPosition - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;

        isMoving = false;

        CheckForEncounter();
    }

    // Check if the next tile can be walked on
    private bool isWalkable(Vector3 targetPosition)
    {
        if(Physics2D.OverlapCircle(targetPosition, 0.2f, solidObjectLayer | interactableLayer) != null)
            return false;

        return true;
    }

    private void Interact()
    {
        // Get space that player is looking at
        var directionFacing = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPosition = transform.position + directionFacing;

        var collider = Physics2D.OverlapCircle(interactPosition, 0.3f, interactableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }

    }

    private void CheckForEncounter()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("isMoving", false);
                OnEncauntered();
            }
        }
    }

    // Handle the image that changes the screen
    public void FadeIn()
    {
        
    }

    public void FadeOut()
    {

    }
}