using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeActions : MonoBehaviour
{
    private InstantiateBees GameTimerScript;
    private float gameTimeNum;
    private GameObject pauseOverlord;
    private GameObject pauseMenu;

    private int movement;
    private GameObject left;
    private GameObject right;
    private GameObject leftHit;
    private GameObject rightHit;
    private GameObject points;
    private GameObject pointsBees;
    private float speed;
    private bool dead;

    private GameObject queen;
    private GameObject queenHurt1;
    private GameObject queenHurt2;
    private bool queenDead;
    private bool firstPass;
    private float distance;
    private Vector3 queenPos;

    void Start()
    {
        // Getting the game's timer from one of the main camera's scripts
        GameTimerScript = Camera.main.gameObject.GetComponent<InstantiateBees>();
        gameTimeNum = GameTimerScript.gameTimer;

        pauseOverlord = GameObject.FindGameObjectWithTag("Pause");
        pauseMenu = pauseOverlord.transform.GetChild(0).gameObject;

        // In the first 20 seconds, bees just do the simple movement
        // After that, movement type is randomly selected
        if (gameTimeNum < 20)
        {
            movement = 3;
        }
        else if (gameTimeNum >= 20 && gameTimeNum <= 150)
        {
            movement = Random.Range(1, 8);
        }
        else
        {
            movement = Random.Range(0, 9);
        }

        left = transform.GetChild(0).gameObject;
        right = transform.GetChild(1).gameObject;
        leftHit = transform.GetChild(2).gameObject;
        rightHit = transform.GetChild(3).gameObject;
        points = transform.GetChild(4).gameObject;
        pointsBees = GameObject.FindGameObjectWithTag("PointsBees");

        // Speed increases the longer the game goes on
        speed = 0.03f;
        for (int i = 60; i <= gameTimeNum; i += 60)
        {
            speed *= 1.2f;
        }

        dead = false;
        queenDead = false;
        firstPass = false;

        if (transform.position.x > 0f)
        {
            left.SetActive(true);
            right.SetActive(false);
        }
        else
        {
            right.SetActive(true);
            left.SetActive(false);
        }
    }

    void Update()
    {
        // Bees halt if the game is paused
        if (pauseMenu.activeSelf)
        {
            if (pauseMenu.transform.GetChild(3).gameObject.activeSelf)
            {
                points.SetActive(false);
            }
        }
        else if (dead) // The bee is hit by the basketball
        {
            if (points != null)
            {
                points.SetActive(true);
                points.transform.parent = pointsBees.transform;
            }

            if (left.activeSelf)
            {
                leftHit.SetActive(true);
            }
            if (right.activeSelf)
            {
                rightHit.SetActive(true);
            }
            left.SetActive(false);
            right.SetActive(false);

            // Bees spin out and fly across the screen when they are hit
            if (transform.position.x > 0f)
            {
                transform.position += new Vector3(0.05f, 0.05f, 0f);
                leftHit.transform.Rotate(0f, 0f, 10f, Space.Self);
                rightHit.transform.Rotate(0f, 0f, 10f, Space.Self);
            }
            if (transform.position.x < 0f)
            {
                transform.position += new Vector3(-0.05f, 0.05f, 0f);
                leftHit.transform.Rotate(0f, 0f, -10f, Space.Self);
                rightHit.transform.Rotate(0f, 0f, -10f, Space.Self);
            }
            if (transform.position.x == 0f)
            {
                if (leftHit.activeSelf)
                {
                    transform.position += new Vector3(0.05f, 0.05f, 0f);
                    leftHit.transform.Rotate(0f, 0f, 10f, Space.Self);
                }
                else
                {
                    transform.position += new Vector3(-0.05f, 0.05f, 0f);
                    rightHit.transform.Rotate(0f, 0f, -10f, Space.Self);
                }
            }

            // They are destroyed when the they go off the screen
            if (transform.position.y > 6f || transform.position.x < -4f || transform.position.x > 4f)
            {
                Destroy(points);
                Destroy(this.gameObject);
            }
        }
        else if (queenDead) // A queen dies while the bee is still alive
        {
            // If the bee isn't in the camera's view, it is simply destroyed
            if (transform.position.y > 5.5f)
            {
                Destroy(this.gameObject);
            }

            // As soon as a Queen's death is detected, the points are displayed and the distance between the bee and queen is found
            if (firstPass == false)
            {
                firstPass = true;
                points.SetActive(true);
                points.transform.parent = pointsBees.transform;
                distance = Vector3.Distance(queen.transform.position, transform.position);
                if (distance < 1f)
                {
                    distance = 1f;
                }
            }

            // The bee flies towards the queen; speed is affected by the distance
            if (queen != null)
            {
                queenPos = queen.transform.position;
                if (queen.transform.position.x > transform.position.x)
                {
                    left.SetActive(false);
                    right.SetActive(true);
                }
                else
                {
                    left.SetActive(true);
                    right.SetActive(false);
                }
                transform.position = Vector2.MoveTowards(transform.position, queen.transform.position, distance * 1.5f * Time.deltaTime);
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, queenPos, distance * 1.8f * Time.deltaTime);
            }

            if (transform.position.y > 6f || transform.position.x < -4f || transform.position.x > 4f)
            {
                Destroy(this.gameObject);
            }
        }
        else // Movement
        {
            // Faster Moving Down
            if (movement < 2)
            {
                // Moving Down
                transform.position -= new Vector3(0f, speed * 1.5f, 0f);

                // Closing in on Cat
                if (transform.position.y < -2.5f)
                {
                    if (transform.position.x < 0f)
                    {
                        transform.position += new Vector3(0.08f, 0f, 0f);
                    }
                    else if (transform.position.x > 0f)
                    {
                        transform.position += new Vector3(-0.08f, 0f, 0f);
                    }
                }
            }
            // Moving Down Normally
            if (movement >= 2 && movement <= 6)
            {
                // Moving Down
                transform.position -= new Vector3(0f, speed, 0f);

                // Closing in on Cat
                if (transform.position.y < -2.5f)
                {
                    if (transform.position.x < 0f)
                    {
                        transform.position += new Vector3(0.04f, 0f, 0f);
                    }
                    else if (transform.position.x > 0f)
                    {
                        transform.position += new Vector3(-0.04f, 0f, 0f);
                    }
                }
            }
            // Moving Side to Side
            if (movement > 6)
            {
                // Moving Down
                transform.position -= new Vector3(0f, speed * 1.25f, 0f);

                // Switching Movement
                if (left.activeSelf)
                {
                    transform.position -= new Vector3(0.04f, 0f, 0f);
                    if (transform.position.x < -2f)
                    {
                        left.SetActive(false);
                        right.SetActive(true);
                    }
                }
                if (right.activeSelf)
                {
                    transform.position += new Vector3(0.04f, 0f, 0f);
                    if (transform.position.x > 2f)
                    {
                        right.SetActive(false);
                        left.SetActive(true);
                    }
                }
            }
        }

        // Searching for a queen to prep for the queen's death
        if (queen == null)
        {
            queen = GameObject.FindGameObjectWithTag("Queen");
        }
        else
        {
            queenHurt1 = queen.transform.GetChild(2).gameObject;
            queenHurt2 = queen.transform.GetChild(3).gameObject;

            if (queenHurt1.activeSelf || queenHurt2.activeSelf)
            {
                queenDead = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "BasketballHitbox")
        {
            dead = true;
        }
    }
}
