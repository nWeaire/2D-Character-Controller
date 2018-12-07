using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
public class Player : Character
{

    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;
    public float accelerationTimeAirborne = .37f;
    public float accelerationTimeGrounded = .02f;
    public float moveSpeed = 12;

    public Vector2 wallLeap;

    public float jumpDelay = 10;
    float jumpDelayTimer;

    public float wallSlideSpeedMax = 0;
    public float wallStickTime = 0.0f;
    float timeToWallUnstick;

    float gravity;
    float jumpVelocity;
    public Vector3 velocity;
    float velocityXSmoothing;

    PlayerController pController;

    public bool isFirstPlayer;


    //private GameObject m_childRenderer = null;

    void Start()
    {
        pController = GetComponent<PlayerController>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;

        SetHealth(m_healthMax);

        //Get child with the renderer
        for (int i = 0; i < this.gameObject.transform.childCount; i++)
        {
           // if (this.gameObject.transform.GetChild(i).GetComponentInChildren<SkinnedMeshRenderer>() != null)
                //m_childRenderer = this.gameObject.transform.GetChild(i).gameObject;
        }
    }

    public override void CharaterActions()
    {
        //Stop crashes due to delta time being set to 0.0f
        if (Time.timeScale == 0.0f)
            return;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), 0);

        if (IsDead())
        {
            input = new Vector2(0, 0);
        }

        if(pController.m_CollisionInfo.bottom)
        {
            jumpDelayTimer = jumpDelay;
        }
        jumpDelayTimer -= Time.deltaTime;

        int wallDirX = (pController.m_CollisionInfo.left) ? -1 : 1;

        

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (pController.m_CollisionInfo.bottom) ? accelerationTimeGrounded : accelerationTimeAirborne);

        bool wallSliding = false;
        if ((pController.m_CollisionInfo.left || pController.m_CollisionInfo.right) && !pController.m_CollisionInfo.bottom && !IsDead())
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            
        }

        if (pController.m_CollisionInfo.top || pController.m_CollisionInfo.bottom)
        {
            velocity.y = 0;
        }



        if (Input.GetButtonDown("Jump") && !IsDead())
        {
            if (wallSliding)
            {
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
            }
            if (pController.m_CollisionInfo.bottom || jumpDelayTimer > 0)
            {
                velocity.y = jumpVelocity;
            }
        }

        //if(velocity.x > 0)
        //{
        //    m_childRenderer.transform.rotation = (Quaternion.Euler(0, 90, 0));
        //}
        //if(velocity.x < 0)
        //{
        //    m_childRenderer.transform.rotation = (Quaternion.Euler(0, 270, 0));
        //}

        velocity.y += gravity * Time.deltaTime;
        pController.Move(velocity * Time.deltaTime);
    }
}
