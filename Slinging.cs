using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slinging : MonoBehaviour
{
    private GameObject hitBox;
    private GameObject slingBottom;
    private float distanceFromBall;
    private float power;
    private Rigidbody2D rb;
    private Vector2 minPower;
    private Vector2 maxPower;
    private Camera cam;
    private Vector2 force;
    private Vector3 startPoint;
    private Vector3 currentPos;
    private Vector3 endPoint;
    private Vector3 direction;
    private float tanAngle;
    private float radius;
    private float distance;
    private Vector3 fromOriginToObject;
    private float offset;

    private GameObject hoop;
    private GameObject slingLeft;
    private GameObject slingRight;
    private LineRenderer leftSling;
    private LineRenderer rightSling;
    private LineRenderer slingLeftLeft;
    private LineRenderer slingLeftRight;
    private LineRenderer slingRightLeft;
    private LineRenderer slingRightRight;

    private GameObject startSlingLeft;
    private GameObject startSlingRight;
    private GameObject endSlingLeft;
    private GameObject endSlingRight;

    private bool moving;
    private bool flung;
    private GameObject pauseOverlord;
    private GameObject pauseMenu;

    private int hitBees;
    private int hitBeesCurrent;
    private int maxHitBees;

    void Start()
    {
        hitBox = transform.GetChild(0).gameObject;
        slingBottom = transform.GetChild(5).gameObject;
        power = 12f;
        rb = gameObject.GetComponent<Rigidbody2D>();
        minPower = new Vector2(-10f, -10f);
        maxPower = new Vector2(8f, 8f);
        startPoint = new Vector3(0f, -0.165f, -0.01f);
        cam = Camera.main;
        radius = 2.1f;

        hoop = GameObject.FindGameObjectWithTag("Hoop");
        slingLeft = transform.GetChild(1).gameObject;
        slingRight = transform.GetChild(2).gameObject;
        leftSling = slingLeft.GetComponent<LineRenderer>();
        rightSling = slingRight.GetComponent<LineRenderer>();
        slingLeftLeft = slingLeft.transform.GetChild(0).gameObject.GetComponent<LineRenderer>();
        slingLeftRight = slingLeft.transform.GetChild(1).gameObject.GetComponent<LineRenderer>();
        slingRightLeft = slingRight.transform.GetChild(0).gameObject.GetComponent<LineRenderer>();
        slingRightRight = slingRight.transform.GetChild(1).gameObject.GetComponent<LineRenderer>();

        startSlingLeft = hoop.transform.GetChild(0).gameObject;
        startSlingRight = hoop.transform.GetChild(1).gameObject;
        endSlingLeft = transform.GetChild(3).gameObject;
        endSlingRight = transform.GetChild(4).gameObject;

        moving = false;
        flung = false;

        pauseOverlord = GameObject.FindGameObjectWithTag("Pause");
        pauseMenu = pauseOverlord.transform.GetChild(0).gameObject;

        hitBees = 0;
        hitBeesCurrent = PlayerPrefs.GetInt("HitBees", 0);
        maxHitBees = 0;
    }

    void Update()
    {
        // If the ball is not being grabbed or flung, it is idle in its starting position
        if (moving == false && flung == false)
        {
            transform.position = startPoint;
            slingBottom.transform.parent = this.transform;
            endSlingLeft.SetActive(true);

            // Unlock condition for the bengal cat: 5 bees hit in a single shot
            endSlingRight.SetActive(true);
            if (hitBees > 0)
            {
                if (hitBees > maxHitBees)
                {
                    maxHitBees = hitBees;
                }
                hitBees = 0;
            }
        }

        // If the game is paused, the ball is reset
        if (pauseMenu.activeSelf)
        {
            transform.position = startPoint;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            leftSling.SetPosition(0, new Vector3(100f, 100f, 100f));
            leftSling.SetPosition(1, new Vector3(100f, 100f, 100f));

            rightSling.SetPosition(0, new Vector3(100f, 100f, 100f));
            rightSling.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingLeftLeft.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingLeftLeft.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingLeftRight.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingLeftRight.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingRightLeft.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingRightLeft.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingRightRight.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingRightRight.SetPosition(1, new Vector3(100f, 100f, 100f));

            // If the game is over, the bengal cat condition is saved
            if (pauseMenu.transform.GetChild(3).gameObject.activeSelf)
            {
                if (maxHitBees > hitBeesCurrent)
                {
                    PlayerPrefs.SetInt("HitBees", maxHitBees);
                    PlayerPrefs.Save();
                }
            }
        }
        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                // Grabbing the ball if it is stationary
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector2.zero);
                distanceFromBall = Vector2.Distance(Camera.main.ScreenToWorldPoint(touch.position), transform.position);
                if (hit.transform == this.gameObject.transform || distanceFromBall < 0.5f)
                {
                    if (flung == false)
                    {
                        moving = true;
                    }
                }
            }

            if (moving)
            {
                // Following touch within a radius and calculating velocity
                currentPos = cam.ScreenToWorldPoint(touch.position);
                currentPos.z = -0.01f;
                distance = Vector3.Distance(currentPos, startPoint);
                if (distance > radius)
                {
                    fromOriginToObject = currentPos - startPoint;
                    fromOriginToObject *= radius / distance;
                    currentPos = startPoint + fromOriginToObject;
                    currentPos.z = -0.01f;
                }
                direction = currentPos - transform.position;
                rb.velocity = new Vector2(direction.x, direction.y) * 20f;

                // Angle of the ball based on its position
                tanAngle = (currentPos.y - startPoint.y) / (currentPos.x - startPoint.x);
                tanAngle = Mathf.Rad2Deg * Mathf.Atan(tanAngle);
                if (currentPos.x >= 0f)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, tanAngle + 90);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, tanAngle - 90);
                }

                // Drawing the rope strings with line renderers
                // Each rope consists of 1 center white line and 2 black lines, 1 on each side of the white line      
                slingLeft.SetActive(true);
                slingRight.SetActive(true);

                leftSling.positionCount = 2;
                rightSling.positionCount = 2;
                slingLeftLeft.positionCount = 2;
                slingLeftRight.positionCount = 2;
                slingRightLeft.positionCount = 2;
                slingRightRight.positionCount = 2;

                if (transform.position.x < -0.2f && transform.position.x > -1.4f)
                {
                    offset = 0.0125f;
                }
                else if (transform.position.x <= -1.4f && transform.position.x > -2f)
                {
                    offset = 0.0195f;
                }
                else if (transform.position.x <= -2f)
                {
                    offset = 0.025f;
                }
                else if (transform.position.x > 0.2f && transform.position.x < 1.4f)
                {
                    offset = -0.0125f;
                }
                else if (transform.position.x >= 1.4f && transform.position.x < 2f)
                {
                    offset = -0.0195f;
                }
                else if (transform.position.x >= 2f)
                {
                    offset = -0.025f;
                }
                else
                {
                    offset = 0f;
                }

                if (transform.position.y < -0.165f)
                {
                    leftSling.SetPosition(0, new Vector3(startSlingLeft.transform.position.x, startSlingLeft.transform.position.y, -1f));
                    leftSling.SetPosition(1, new Vector3(endSlingLeft.transform.position.x, endSlingLeft.transform.position.y, -1f));

                    rightSling.SetPosition(0, new Vector3(startSlingRight.transform.position.x, startSlingRight.transform.position.y, -1f));
                    rightSling.SetPosition(1, new Vector3(endSlingRight.transform.position.x, endSlingRight.transform.position.y, -1f));

                    slingLeftLeft.SetPosition(0, new Vector3(startSlingLeft.transform.position.x - 0.025f, startSlingLeft.transform.position.y + offset, -1.1f));
                    slingLeftLeft.SetPosition(1, new Vector3(endSlingLeft.transform.position.x - 0.025f, endSlingLeft.transform.position.y + offset, -1.1f));

                    slingLeftRight.SetPosition(0, new Vector3(startSlingLeft.transform.position.x + 0.025f, startSlingLeft.transform.position.y - offset, -1.1f));
                    slingLeftRight.SetPosition(1, new Vector3(endSlingLeft.transform.position.x + 0.025f, endSlingLeft.transform.position.y - offset, -1.1f));

                    slingRightLeft.SetPosition(0, new Vector3(startSlingRight.transform.position.x - 0.025f, startSlingRight.transform.position.y + offset, -1.1f));
                    slingRightLeft.SetPosition(1, new Vector3(endSlingRight.transform.position.x - 0.025f, endSlingRight.transform.position.y + offset, -1.1f));

                    slingRightRight.SetPosition(0, new Vector3(startSlingRight.transform.position.x + 0.025f, startSlingRight.transform.position.y - offset, -1.1f));
                    slingRightRight.SetPosition(1, new Vector3(endSlingRight.transform.position.x + 0.025f, endSlingRight.transform.position.y - offset, -1.1f));
                }
                else
                {
                    leftSling.SetPosition(0, new Vector3(startSlingLeft.transform.position.x, startSlingLeft.transform.position.y, -1f));
                    leftSling.SetPosition(1, new Vector3(endSlingRight.transform.position.x, endSlingRight.transform.position.y, -1f));

                    rightSling.SetPosition(0, new Vector3(startSlingRight.transform.position.x, startSlingRight.transform.position.y, -1f));
                    rightSling.SetPosition(1, new Vector3(endSlingLeft.transform.position.x, endSlingLeft.transform.position.y, -1f));

                    slingLeftLeft.SetPosition(0, new Vector3(startSlingLeft.transform.position.x - 0.025f, startSlingLeft.transform.position.y - offset, -1.1f));
                    slingLeftLeft.SetPosition(1, new Vector3(endSlingRight.transform.position.x - 0.025f, endSlingRight.transform.position.y - offset, -1.1f));

                    slingLeftRight.SetPosition(0, new Vector3(startSlingLeft.transform.position.x + 0.025f, startSlingLeft.transform.position.y + offset, -1.1f));
                    slingLeftRight.SetPosition(1, new Vector3(endSlingRight.transform.position.x + 0.025f, endSlingRight.transform.position.y + offset, -1.1f));

                    slingRightLeft.SetPosition(0, new Vector3(startSlingRight.transform.position.x - 0.025f, startSlingRight.transform.position.y - offset, -1.1f));
                    slingRightLeft.SetPosition(1, new Vector3(endSlingLeft.transform.position.x - 0.025f, endSlingLeft.transform.position.y - offset, -1.1f));

                    slingRightRight.SetPosition(0, new Vector3(startSlingRight.transform.position.x + 0.025f, startSlingRight.transform.position.y + offset, -1.1f));
                    slingRightRight.SetPosition(1, new Vector3(endSlingLeft.transform.position.x + 0.025f, endSlingLeft.transform.position.y + offset, -1.1f));
                }
            }

            // Ball being flung by adding force opposite the direction the ball was pulled
            if (touch.phase == TouchPhase.Ended)
            {
                if (moving)
                {
                    moving = false;
                    flung = true;
                    endPoint = cam.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, -0.01f));
                    force = new Vector2(Mathf.Clamp(startPoint.x - endPoint.x, minPower.x, maxPower.x), Mathf.Clamp(startPoint.y - endPoint.y, minPower.y, maxPower.y));
                    if ((Mathf.Abs(force.x) + Mathf.Abs(force.y)) < 0.7f)
                    {
                        rb.AddForce(force * power * 1.5f, ForceMode2D.Impulse);
                    }
                    else if ((Mathf.Abs(force.x) + Mathf.Abs(force.y)) < 2f)
                    {
                        rb.AddForce(force * power * 1.25f, ForceMode2D.Impulse);
                    }
                    else
                    {
                        rb.AddForce(force * power, ForceMode2D.Impulse);
                    }
                }
            }
        }
        else
        {
            if (moving)
            {
                transform.position = currentPos;
            }
        }

        // When the ball goes off the screen, it is done being flung
        if (transform.position.y > 5.5f || transform.position.x > 3.3f || transform.position.x < -3.3f || transform.position.y < -5f)
        {
            flung = false;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            leftSling.SetPosition(0, new Vector3(100f, 100f, 100f));
            leftSling.SetPosition(1, new Vector3(100f, 100f, 100f));

            rightSling.SetPosition(0, new Vector3(100f, 100f, 100f));
            rightSling.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingLeftLeft.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingLeftLeft.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingLeftRight.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingLeftRight.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingRightLeft.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingRightLeft.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingRightRight.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingRightRight.SetPosition(1, new Vector3(100f, 100f, 100f));
        }

        if (flung)
        {
            // Bees can now be hit
            hitBox.SetActive(true);

            // Adjusting the rope strings as the ball comes back towards the center of the screen
            if (transform.position.x < -0.2f && transform.position.x > -1.4f)
            {
                offset = 0.0125f;
            }
            else if (transform.position.x <= -1.4f && transform.position.x > -2f)
            {
                offset = 0.0195f;
            }
            else if (transform.position.x <= -2f)
            {
                offset = 0.025f;
            }
            else if (transform.position.x > 0.2f && transform.position.x < 1.4f)
            {
                offset = -0.0125f;
            }
            else if (transform.position.x >= 1.4f && transform.position.x < 2f)
            {
                offset = -0.0195f;
            }
            else if (transform.position.x >= 2f)
            {
                offset = -0.025f;
            }
            else
            {
                offset = 0f;
            }

            // The next few statements deal with getting rid of the line renderers when the ball passed through the middle of the screen
            // (ie the ball leaves the pulled back strings and officially looks flung)
            // Any Kind of Vertical
            if (currentPos.y > -0.165f)
            {
                if (slingLeft.activeSelf)
                {
                    leftSling.SetPosition(0, new Vector3(startSlingLeft.transform.position.x, startSlingLeft.transform.position.y, -1f));
                    leftSling.SetPosition(1, new Vector3(endSlingRight.transform.position.x, endSlingRight.transform.position.y, -1f));

                    rightSling.SetPosition(0, new Vector3(startSlingRight.transform.position.x, startSlingRight.transform.position.y, -1f));
                    rightSling.SetPosition(1, new Vector3(endSlingLeft.transform.position.x, endSlingLeft.transform.position.y, -1f));

                    slingLeftLeft.SetPosition(0, new Vector3(startSlingLeft.transform.position.x - 0.025f, startSlingLeft.transform.position.y - offset, -1.1f));
                    slingLeftLeft.SetPosition(1, new Vector3(endSlingRight.transform.position.x - 0.025f, endSlingRight.transform.position.y - offset, -1.1f));

                    slingLeftRight.SetPosition(0, new Vector3(startSlingLeft.transform.position.x + 0.025f, startSlingLeft.transform.position.y + offset, -1.1f));
                    slingLeftRight.SetPosition(1, new Vector3(endSlingRight.transform.position.x + 0.025f, endSlingRight.transform.position.y + offset, -1.1f));

                    slingRightLeft.SetPosition(0, new Vector3(startSlingRight.transform.position.x - 0.025f, startSlingRight.transform.position.y - offset, -1.1f));
                    slingRightLeft.SetPosition(1, new Vector3(endSlingLeft.transform.position.x - 0.025f, endSlingLeft.transform.position.y - offset, -1.1f));

                    slingRightRight.SetPosition(0, new Vector3(startSlingRight.transform.position.x + 0.025f, startSlingRight.transform.position.y + offset, -1.1f));
                    slingRightRight.SetPosition(1, new Vector3(endSlingLeft.transform.position.x + 0.025f, endSlingLeft.transform.position.y + offset, -1.1f));
                }

                if (transform.position.y < -0.165f)
                {
                    slingLeft.SetActive(false);
                    slingRight.SetActive(false);
                    endSlingLeft.SetActive(false);
                    endSlingRight.SetActive(false);
                    slingBottom.transform.parent = null;
                    slingBottom.transform.position = new Vector3(0f, -0.365f, 0.009f);
                    slingBottom.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
            }
            if (currentPos.y <= -0.165f)
            {
                if (slingLeft.activeSelf)
                {
                    leftSling.SetPosition(0, new Vector3(startSlingLeft.transform.position.x, startSlingLeft.transform.position.y, -1f));
                    leftSling.SetPosition(1, new Vector3(endSlingLeft.transform.position.x, endSlingLeft.transform.position.y, -1f));

                    rightSling.SetPosition(0, new Vector3(startSlingRight.transform.position.x, startSlingRight.transform.position.y, -1f));
                    rightSling.SetPosition(1, new Vector3(endSlingRight.transform.position.x, endSlingRight.transform.position.y, -1f));

                    slingLeftLeft.SetPosition(0, new Vector3(startSlingLeft.transform.position.x - 0.025f, startSlingLeft.transform.position.y + offset, -1.1f));
                    slingLeftLeft.SetPosition(1, new Vector3(endSlingLeft.transform.position.x - 0.025f, endSlingLeft.transform.position.y + offset, -1.1f));

                    slingLeftRight.SetPosition(0, new Vector3(startSlingLeft.transform.position.x + 0.025f, startSlingLeft.transform.position.y - offset, -1.1f));
                    slingLeftRight.SetPosition(1, new Vector3(endSlingLeft.transform.position.x + 0.025f, endSlingLeft.transform.position.y - offset, -1.1f));

                    slingRightLeft.SetPosition(0, new Vector3(startSlingRight.transform.position.x - 0.025f, startSlingRight.transform.position.y + offset, -1.1f));
                    slingRightLeft.SetPosition(1, new Vector3(endSlingRight.transform.position.x - 0.025f, endSlingRight.transform.position.y + offset, -1.1f));

                    slingRightRight.SetPosition(0, new Vector3(startSlingRight.transform.position.x + 0.025f, startSlingRight.transform.position.y - offset, -1.1f));
                    slingRightRight.SetPosition(1, new Vector3(endSlingRight.transform.position.x + 0.025f, endSlingRight.transform.position.y - offset, -1.1f));
                }

                if (transform.position.y > -0.165f)
                {
                    slingLeft.SetActive(false);
                    slingRight.SetActive(false);
                    endSlingLeft.SetActive(false);
                    endSlingRight.SetActive(false);
                    slingBottom.transform.parent = null;
                    slingBottom.transform.position = new Vector3(0f, -0.365f, 0.009f);
                    slingBottom.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
            }

            // For if Too Left or Right
            if (currentPos.x <= 0f && transform.position.x > 0f)
            {
                slingLeft.SetActive(false);
                slingRight.SetActive(false);
                endSlingLeft.SetActive(false);
                endSlingRight.SetActive(false);
                slingBottom.transform.parent = null;
                slingBottom.transform.position = new Vector3(0f, -0.365f, 0.009f);
                slingBottom.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            if (currentPos.x >= 0f && transform.position.x < 0f)
            {
                slingLeft.SetActive(false);
                slingRight.SetActive(false);
                endSlingLeft.SetActive(false);
                endSlingRight.SetActive(false);
                slingBottom.transform.parent = null;
                slingBottom.transform.position = new Vector3(0f, -0.365f, 0.009f);
                slingBottom.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        else
        {
            hitBox.SetActive(false);
        }

        startSlingLeft.transform.position = new Vector3(-0.318f, -0.213f, 0.009f);
        startSlingRight.transform.position = new Vector3(0.318f, -0.213f, 0.009f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player aimed downward, the ball goes back to its starting position when it hits the ground instead
        if (collision.gameObject.name == "Line")
        {
            flung = false;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            leftSling.SetPosition(0, new Vector3(100f, 100f, 100f));
            leftSling.SetPosition(1, new Vector3(100f, 100f, 100f));

            rightSling.SetPosition(0, new Vector3(100f, 100f, 100f));
            rightSling.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingLeftLeft.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingLeftLeft.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingLeftRight.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingLeftRight.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingRightLeft.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingRightLeft.SetPosition(1, new Vector3(100f, 100f, 100f));

            slingRightRight.SetPosition(0, new Vector3(100f, 100f, 100f));
            slingRightRight.SetPosition(1, new Vector3(100f, 100f, 100f));
        }

        // Counter for hitting bees in one sling
        if (collision.gameObject.tag == "Bee")
        {
            hitBees++;
        }
    }
}
