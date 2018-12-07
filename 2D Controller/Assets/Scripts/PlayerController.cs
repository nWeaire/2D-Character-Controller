using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{

    public LayerMask CollisionMask;

    const float skinWidth = .04f; // indent of rays on body of character
    public int HorizontalRays = 2; // number of rays horizontal of character
    public int VerticalRays = 2; // number of rays Vertical of character

    float maxClimbAngle = 80;
    float maxDescendAngle = 80;

    float HorizontalRayWidth;
    float VerticalRayWidth;

    /////////////
    private GameObject m_otherPlayer = null;

    private GameObject m_mainCamera = null;
    //////////////

    CapsuleCollider m_Collider;
    Raycast m_Raycast;
    public CollisionInfo m_CollisionInfo; // Info collected by raycasts



    void Start()
    {
        m_Collider = GetComponent<CapsuleCollider>();
        RayWidth(); // Calls function to get ray width

      
    }

    //-----------------------------------------------------
    // Updates the position of the raycasts every frame
    // Creates a bounding box and expands with the skin width of ray casts
    // Sets BL, BR, TL, TR of bounding box
    //-----------------------------------------------------
    void UpdateRaycast()
    {
        Bounds boundingBox = m_Collider.bounds;
        boundingBox.Expand(skinWidth * -2);

        m_Raycast.bottomLeft = new Vector3(boundingBox.min.x, boundingBox.min.y, boundingBox.center.z);
        m_Raycast.bottomRight = new Vector3(boundingBox.max.x, boundingBox.min.y, boundingBox.center.z);
        m_Raycast.topLeft = new Vector3(boundingBox.min.x, boundingBox.max.y, boundingBox.center.z);
        m_Raycast.topRight = new Vector3(boundingBox.max.x, boundingBox.max.y, boundingBox.center.z);
    }

    //-----------------------------------------------------
    // Checks horizontal collisions
    // alters velocity according to collisions detected
    // parameters:
    //         Velocity: for use in checking collisions ahead of time
    //-----------------------------------------------------
    void HorizontalCollisions(ref Vector3 Velocity)
    {
        float dirX = Mathf.Sign(Velocity.x); // checks direction player is moving in
        float rayLength = Mathf.Abs(Velocity.x) + skinWidth; // Sets ray length based on skin width and velocity

        if (Mathf.Abs(Velocity.x) < skinWidth)
        {
            rayLength = 12 * skinWidth; // Sets ray length 
        }



        for (int i = 0; i < HorizontalRays; i++) // Checks all arrays
        {
            Vector3 rayOrigin = (dirX == -1) ? m_Raycast.bottomLeft : m_Raycast.bottomRight; // sets origin of ray to cast from
            rayOrigin += Vector3.up * (HorizontalRayWidth * i); // spreads arrays across the side of the bounding box
            RaycastHit Hit; // Creates raycast
            Ray r1 = new Ray(rayOrigin, Vector3.right * dirX); // Creates a ray cast

            Debug.DrawRay(rayOrigin, Vector3.right * dirX * rayLength, Color.red); // draws array

            if (Physics.Raycast(r1, out Hit, rayLength, CollisionMask)) // checks if ray collided with something
            {

                float slopeAngle = Vector2.Angle(Hit.normal, Vector2.up); // Checks for slope angle

                if (i == 0 && slopeAngle <= maxClimbAngle) // if slope is less then max climb angle
                {
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != m_CollisionInfo.slopeAngleOld)
                    {
                        distanceToSlopeStart = Hit.distance - skinWidth;
                        Velocity.x -= distanceToSlopeStart * dirX;
                    }
                    ClimbSlope(ref Velocity, slopeAngle); 
                    Velocity.x += distanceToSlopeStart * dirX; // Climb slope
                }

                if (!m_CollisionInfo.climbingSlope || slopeAngle > maxClimbAngle) // if climbing slope or slope angle is greater then the ability to climb
                {
                    Velocity.x = (Hit.distance - skinWidth) * dirX; // Alters velocity
                    rayLength = Hit.distance;

                    if (m_CollisionInfo.climbingSlope) 
                    {
                        Velocity.y = Mathf.Tan(m_CollisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(Velocity.x);
                    }
                    m_CollisionInfo.left = dirX == -1;
                    m_CollisionInfo.right = dirX == 1;

                }

            }

        }
    }

    //-----------------------------------------------------
    // Checks Vertical collisions
    // alters velocity according to collisions detected
    // parameters:
    //         Velocity: for use in checking collisions ahead of time
    //-----------------------------------------------------
    void VerticalCollisions(ref Vector3 Velocity)
    {
        float dirY = Mathf.Sign(Velocity.y); // Direction of movement on y axis
        float rayLength = Mathf.Abs(Velocity.y) + skinWidth * 12; // Sets raylength



        for (int i = 0; i < VerticalRays; i++) // Creates all rays
        {
            Vector3 rayOrigin = (dirY == -1) ? m_Raycast.bottomLeft : m_Raycast.topLeft;
            rayOrigin += Vector3.right * (VerticalRayWidth * i + Velocity.x); // Creates and spreads out rays along the top and bottom of the collider
            RaycastHit Hit; // creates raycast
            Ray r1 = new Ray(rayOrigin, Vector3.up * dirY); // Creates ray


            Debug.DrawRay(rayOrigin, Vector2.up * dirY * rayLength, Color.red); // draws ray for debug purposes

            if (Physics.Raycast(r1, out Hit, rayLength, CollisionMask)) // Checks if ray collides with anything on the collision layer
            {
                Velocity.y = (Hit.distance - skinWidth) * dirY;
                rayLength = Hit.distance;

                m_CollisionInfo.bottom = dirY == -1;
                m_CollisionInfo.top = dirY == 1;

            }
        }
    }

    //-----------------------------------------------------
    // Controls velocity and jumping on slopes
    // parameters:
    //          Velocity: for use in checking collisions ahead of time
    //          slopeAngle: angle of the slope currently on
    //-----------------------------------------------------
    void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x); // Sets move distance of absolute of velocity on the x
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance; // Sets climb velocity, based on angle slope and move distance

        if (velocity.y <= climbVelocityY) //if the velocity on the y is less then climb velocity
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            m_CollisionInfo.bottom = true;
            m_CollisionInfo.climbingSlope = true;
            m_CollisionInfo.slopeAngle = slopeAngle; // Climbs slope
        }
    }

    //-----------------------------------------------------
    // Controls velocity and jumping on slopes
    // parameters:
    //          Velocity: for use in checking collisions ahead of time
    //-----------------------------------------------------
    void DescendSlope(ref Vector3 velocity)
    {
        float dirX = Mathf.Sign(velocity.x); // sets direction on x axis
        Vector2 rayOrigin = (dirX == -1) ? m_Raycast.bottomRight : m_Raycast.bottomLeft; // Sets ray origin from bounding box
        RaycastHit Hit; // Creates rayCastHit
        Ray r1 = new Ray(rayOrigin, -Vector3.up); // Creates ray from origin

        if (Physics.Raycast(r1, out Hit, Mathf.Infinity, CollisionMask)) // Checks if the ray collides with anything on the collisons layer
        {
            float slopeAngle = Vector2.Angle(Hit.normal, Vector2.up); // Sets slopeAngle
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) // Checks if player is able to climb slope
            {
                if (Mathf.Sign(Hit.normal.x) == dirX)  // Controls velocity and allows player to stick to slopes when going down them
                {
                    if (Hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        m_CollisionInfo.slopeAngle = slopeAngle;
                        m_CollisionInfo.descendingSlope = true;
                        m_CollisionInfo.bottom = true;
                    }
                }
            }
        }
    }

    //-----------------------------------------------------
    // Sets the width between rays on bounding box
    // Finds bounding box of collider 
    //-----------------------------------------------------
    void RayWidth()
    {
        Bounds boundingBox = m_Collider.bounds; // Finds bounding box of collider
        boundingBox.Expand(skinWidth * -2); // Expand by skinWidth

        HorizontalRays = Mathf.Clamp(HorizontalRays, 2, int.MaxValue); // Clamp rays
        VerticalRays = Mathf.Clamp(VerticalRays, 2, int.MaxValue); // clamp rays

        HorizontalRayWidth = boundingBox.size.y / (HorizontalRays - 1); // sets ray width
        VerticalRayWidth = boundingBox.size.x / (VerticalRays - 1); // sets ray width
    }
    //-----------------------------------------------------
    // Controls velocity and jumping on slopes
    // parameters:
    //          Velocity: changes velocity depending on collisions
    //-----------------------------------------------------
    public void Move(Vector3 Velocity)
    {
        UpdateRaycast(); // Updates position of rays
        m_CollisionInfo.reset(); // Resets ray information
        m_CollisionInfo.velocityOld = Velocity; // Remembers old velocity

        if (Velocity.y < 0) // if falling check for descending slope
        {
            DescendSlope(ref Velocity);
        }
        if (Velocity.x != 0) // If moving on the x check for horizontal collisions
        {
            HorizontalCollisions(ref Velocity);
        }
        if (Velocity.y != 0) // If moving on the y check for vertical collisions
        {
            VerticalCollisions(ref Velocity);
        }

        transform.Translate(Velocity); // Translates player based on velocity
    }

    //-----------------------------------------------------
    // Collision information gathered by raycasting 
    //-----------------------------------------------------
    public struct CollisionInfo
    {
        public bool top, bottom;
        public bool left, right;
        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;

        public void reset()
        {
            top = bottom = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }


    }


    //-----------------------------------------------------
    // Information on bounding boxes for ray cast positions
    //-----------------------------------------------------
    struct Raycast
    {
        public Vector3 topLeft, topRight;
        public Vector3 bottomLeft, bottomRight;
    }

}
