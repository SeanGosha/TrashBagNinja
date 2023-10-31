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

    // Powerups
    public bool speedBoost = false; // Flag to track if the player has a speed boost
    private bool speedBoostActive = false; // Flag to track if the speed boost is active
    public bool plusOne = false; // Flag to track if the player has a plus one powerup
    private bool plusOneActive = false; // Flag to track if the plus one powerup is active

    public GameObject plusOnePrefab; // Reference to the plus one prefab




    // Score
    public int score = 0; // Player score
    public int totalTrashbags = 0; // Total bags thrown

    // References
    public TrashBagThrower trashBagThrower; // Reference to the trash bag thrower

    private Animator animator;
    public Shader outlineShader; // Reference to the outline shader
    public SkinnedMeshRenderer playerMesh; // Reference to the player mesh


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

            if(plusOne && !plusOneActive)
            {
                plusOneActive = true;
                StartCoroutine(PlusOne());
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
        // Set the speed shader
        playerMesh.material.shader = outlineShader;
        //Set shader _OutlineColor to yellow
        playerMesh.material.SetColor("_MainColor", new Color(255, 255, 255));
        playerMesh.material.SetColor("_OutlineColor", new Color(255, 255, 0));
        // Wait for the duration
        yield return new WaitForSeconds(7.2f);
        // Reset the shader
        int i = 0;
        while(i < 4)
        {
            playerMesh.material.shader = Shader.Find("Mobile/Diffuse");
            yield return new WaitForSeconds(.1f);
            playerMesh.material.shader = outlineShader;
            yield return new WaitForSeconds(.1f);
            i++;
        }
        playerMesh.material.shader = Shader.Find("Mobile/Diffuse");
        // reset transition duration
        transitionDuration = Mathf.Clamp(0.2f * Mathf.Exp(-totalTrashbags / 275f), 0.13f, 0.2f);        
        
        speedBoost = false;
        speedBoostActive = false;
    }

    private IEnumerator PlusOne()
    {
        // Spawn a plus one prefab
        GameObject plusOneObject = Instantiate(plusOnePrefab, transform.position, Quaternion.identity);
        //change shader to outline
        playerMesh.material.shader = outlineShader;
        //turn outline color to teal
        playerMesh.material.SetColor("_OutlineColor", new Color(0, 255, 255));
        // Set the parent to the player
        plusOneObject.transform.SetParent(transform);
        //set the postition 3 units to the right of the player by x position
        plusOneObject.transform.position = new Vector3(transform.position.x + 3, transform.position.y, transform.position.z);
        yield return new WaitForSeconds(7.2f);
        // Reset the shader
        int i = 0;
        while(i < 4)
        {
            playerMesh.material.shader = Shader.Find("Mobile/Diffuse");
            yield return new WaitForSeconds(.1f);
            playerMesh.material.shader = outlineShader;
            yield return new WaitForSeconds(.1f);
            i++;
        }
        playerMesh.material.shader = Shader.Find("Mobile/Diffuse");
        // Destroy the plus one object
        Destroy(plusOneObject);
        plusOne = false;
        plusOneActive = false;
    }
}
