using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    // Swipe controls
    public float swipeThreshold = 50.0f; // Minimum swipe distance threshold
    public float swipeRegionPercentage = 0.25f; // Percentage of the screen for swipe controls

    private Vector2 fingerDownPos;
    private Vector2 fingerUpPos;
    private float swipeRegionWidth;

    // Movement controls
    public float laneWidth = 2.5f; // Width of each lane
    public int currentLane = 1; // Starting lane index (0-based)
    public float transitionDuration = 0.2f; // Duration of the transition between lanes
    private Vector3 targetPosition; // The target position to move towards
    public bool isMoving = false; // Flag to track if the player is currently moving
    private Coroutine moveCoroutine; // Reference to the coroutine that moves the player
    [HideInInspector]
    public bool isAttacking = false; // Flag to track if the player is currently attacking
    public bool gameActive = false; // Flag to track if movement is enabled
    public bool speedBoost = false; // Flag to track if the player has a speed boost
    private bool speedBoostActive = false; // Flag to track if the speed boost is active

    // Score
    public int score = 0; // Player score
    public int totalTrashbags = 0; // Total bags thrown

    // References
    public TrashBagThrower trashBagThrower; // Reference to the trash bag thrower

    private Animator animator;


    private void Start()
    {
        // Get the animator component
        animator = GetComponent<Animator>();

        // Calculate the width of the swipe region based on the screen size and the specified percentage
        swipeRegionWidth = Screen.width * swipeRegionPercentage;
        Debug.Log("Swipe region width: " + swipeRegionWidth);
        Debug.Log("Screen width: " + Screen.width);
    }

    private void Update()
    {
        if(gameActive)
        {
            if(!speedBoost)
            {
                // Update the transition duration
                transitionDuration = Mathf.Clamp(0.2f * Mathf.Exp(-totalTrashbags / 275f), 0.13f, 0.2f);
            }
            else
            {
                if(!speedBoostActive)
                {
                    speedBoostActive = true;
                    StartCoroutine(SpeedBoost());
                }
            }

            // Keyboard input for testing
            if (Input.GetKeyDown(KeyCode.LeftArrow) && !isMoving)
            {
                MoveToLane(currentLane - 1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && !isMoving)
            {
                MoveToLane(currentLane + 1);
            }

            // Swipe input for mobile
            if (Input.touchCount > 0 && !isMoving)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    fingerDownPos = touch.position;
                    fingerUpPos = touch.position;
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    fingerUpPos = touch.position;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    fingerUpPos = touch.position;

                    // Check if the touch ended within the swipe region
                    if (fingerDownPos.x <= (Screen.width - swipeRegionWidth) && Mathf.Abs(fingerDownPos.x - fingerUpPos.x) > swipeThreshold && fingerUpPos.x <= (Screen.width - swipeRegionWidth))
                    {
                        if (fingerDownPos.x - fingerUpPos.x < 0)
                        {
                            MoveToLane(currentLane + 1);
                        }
                        else
                        {
                            MoveToLane(currentLane - 1);
                        }
                    }
                }
            }

            // Keyboard input for attack (testing purposes)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Attack();
            }
        }
    }

    private void MoveToLane(int targetLane)
    {
        // Clamp targetLane within the lane range
        targetLane = Mathf.Clamp(targetLane, 0, 3);

        // Calculate the target position based on the lane width and targetLane
        targetPosition = new Vector3((targetLane - 1.5f) * laneWidth, transform.position.y, transform.position.z);

        // Cancel the current movement coroutine if it exists
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        // Start the new movement coroutine
        moveCoroutine = StartCoroutine(MoveCoroutine());

        Debug.Log("Transition duration: " + transitionDuration);

        // Update the current lane
        currentLane = targetLane;
    }

    private IEnumerator MoveCoroutine()
    {
        if (fingerDownPos.x - fingerUpPos.x > 0 && currentLane == 0 || Input.GetKeyDown(KeyCode.LeftArrow) && currentLane == 0)
        {
            isMoving = false;
        }
        else if (fingerDownPos.x - fingerUpPos.x < 0 && currentLane == 3 || Input.GetKeyDown(KeyCode.RightArrow) && currentLane == 3)
        {
            isMoving = false;
        }
        else
        {
            // Set the moving flag to true
            isMoving = true;
            animator.SetBool("isMoving", true);
        }

        // Calculate the starting position and time
        Vector3 startPosition = transform.position;
        float startTime = Time.time;

        // Perform the transition over time
        while (Time.time < startTime + transitionDuration)
        {
            float normalizedTime = (Time.time - startTime) / transitionDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, normalizedTime);
            yield return null;
        }

        // Ensure the final position is reached
        transform.position = targetPosition;

        // Set the moving flag to false
        isMoving = false;
        animator.SetBool("isMoving", false);
    }

    public void Attack()
    {
        if(!isAttacking)
        {
            // Play attack animation
            animator.SetBool("isAttacking", true);
            // Set the attacking flag to true
            isAttacking = true;
            // Add your attack logic here

            // Reset the attack animation after a certain duration
            StartCoroutine(ResetAttackAnimation());
        }
    }

    private IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.30f); // Adjust the duration as needed
        animator.SetBool("isAttacking", false);
        isAttacking = false;
    }

    private IEnumerator SpeedBoost()
    {
        float previousTransitionDuration = transitionDuration;
        // Increase the transition duration
        transitionDuration *= 0.3f;
        // Wait for the duration
        yield return new WaitForSeconds(12f);
        // reset transition duration
        transitionDuration = Mathf.Clamp(0.2f * Mathf.Exp(-totalTrashbags / 275f), 0.13f, 0.2f);        
        
        speedBoost = false;
        speedBoostActive = false;
    }
}
