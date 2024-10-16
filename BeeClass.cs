using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bee : MonoBehaviour
{
    protected float movementSpeed;
    protected float closingInSpeed;
    protected float deathSpeed;
    protected float rotateSpeed;

    protected bool dead;
    protected bool queenDead;
    protected float queenDistance;
    protected GameObject queen;

    protected Animator anim;
    protected GameObject points;

    public virtual void Start()
    {
        movementSpeed = 17.5f;
        closingInSpeed = 20.0f;
        deathSpeed = 30.0f;
        rotateSpeed = 50.0f;

        dead = false;
        queenDead = false;
        queenDistance = 0f;

        anim = GetComponent<Animator>();
        // Every bee has a "100" point gameobject that spawns when the bee dies or a queen dies
        points = transform.Find("Points").gameObject;
    }

    public virtual void Update()
    {
        if (dead)
        {
            DeathMovement();
        }
        else if (queenDead)
        {
            ChaseQueen();
        }
        else
        {
            Move();
        }
    }

    public void Init(float movementSpeed, float closingInSpeed)
    {
        this.movementSpeed = movementSpeed;
        this.closingInSpeed = closingInSpeed;
    }

    // Called by the instantiator, initial base speed increases the longer the round is
    public void IncreaseSpeedByTime(float gameTime)
    {
        for (int i = 60; i <= gameTime; i += 60)
        {
            movementSpeed *= 1.2f;
            closingInSpeed *= 1.2f;
        }
    }

    public virtual void Move()
    {
        // Move down
        transform.position -= new Vector3(0f, movementSpeed * Time.deltaTime, 0f);

        // Close in on cat when far down enough
        if (transform.position.y < -2.5f)
        {
            if (transform.position.x < 0f)
            {
                transform.position += new Vector3(closingInSpeed * Time.deltaTime, 0f, 0f);
            }
            else if (transform.position.x > 0f)
            {
                transform.position += new Vector3(-closingInSpeed * Time.deltaTime, 0f, 0f);
            }
        }
    }

    public virtual void DeathMovement()
    {
        // Bees spin out and fly across the screen when they are hit
        if (transform.position.x >= 0f)
        {
            transform.position += new Vector3(deathSpeed, deathSpeed, 0f);
            transform.Rotate(0f, 0f, rotateSpeed, Space.Self);
            transform.Rotate(0f, 0f, rotateSpeed, Space.Self);
        }
        if (transform.position.x < 0f)
        {
            transform.position += new Vector3(-deathSpeed, deathSpeed, 0f);
            transform.Rotate(0f, 0f, -rotateSpeed, Space.Self);
            transform.Rotate(0f, 0f, -rotateSpeed, Space.Self);
        }

        // Bees are destroyed when they go off the screen
        if (transform.position.y > 6f || transform.position.x < -4f || transform.position.x > 4f)
        {
            Destroy(points);
            Destroy(this.gameObject);
        }
    }

    // Called by the queen when she is hit, sent to all active bees
    public virtual void QueenDeath(GameObject queen)
    {
        // If the bee isn't in the camera's view, it is simply destroyed
        if (transform.position.y > 5.5f)
        {
            Destroy(this.gameObject);
        }

        // Hitting the queen gives points for all bess on the screen
        points.SetActive(true);
        points.transform.parent = null;

        // Distance controls the speed so all bees reach the queen at the same time
        queenDead = true;
        this.queen = queen;
        queenDistance = Vector3.Distance(queen.transform.position, transform.position);
        if (queenDistance < 1f)
        {
            queenDistance = 1f;
        }
    }

    public virtual void ChaseQueen()
    {
        // Bee flies towards the queen; speed is affected by the queenDistance
        transform.position = Vector2.MoveTowards(transform.position, queen.transform.position, queenDistance * 1.5f * Time.deltaTime);

        // Face the queen at all times
        if (queen.transform.position.x > transform.position.x && transform.localScale.x > 0f)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1.0f);
        }
        else if (queen.transform.position.x < transform.position.x && transform.localScale.x < 0f)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1.0f);
        }

        // Destroy when off screen (queen deletes all points objects when she goes off screen so they are synced)
        if (transform.position.y > 6f || transform.position.x < -4f || transform.position.x > 4f)
        {
            Destroy(this.gameObject);
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // Activate death conditions if the bee is hit
        if (collision.gameObject.name == "BasketballHitbox")
        {
            dead = true;
            points.SetActive(true);
            points.transform.parent = null;
            anim.SetBool("Dead", true);
        }
    }
}

// BeeFast is primarily the same as the base class, though speed is increased by 1.5x
public class BeeFast : Bee
{
    public override void Start()
    {
        base.Start();
        base.Init(movementSpeed * 1.5f, closingInSpeed * 1.5f);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Move()
    {
        base.Move();
    }

    public override void DeathMovement()
    {
        base.DeathMovement();
    }

    public override void QueenDeath(GameObject queen)
    {
        base.QueenDeath(queen);
    }

    public override void ChaseQueen()
    {
        base.ChaseQueen();
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }
}

// BeeZigZag travels from side to side while moving down
public class BeeZigZag : Bee
{
    private float zigZagSpeed;

    public override void Start()
    {
        base.Start();
        base.Init(movementSpeed * 1.25f, closingInSpeed * 1.25f);
        zigZagSpeed = 0.05f;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Move()
    {
        // Move down
        transform.position -= new Vector3(0f, movementSpeed, 0f);

        // Switch movement direction when too far
        if (transform.localScale.x > 0f)
        {
            transform.position += new Vector3(-zigZagSpeed, 0f, 0f);
            if (transform.position.x < -2f)
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1.0f);
            }
        }
        if (transform.localScale.x < 0f)
        {
            transform.position += new Vector3(zigZagSpeed, 0f, 0f);
            if (transform.position.x > 2f)
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1.0f);
            }
        }
    }

    public override void DeathMovement()
    {
        base.DeathMovement();
    }

    public override void QueenDeath(GameObject queen)
    {
        base.QueenDeath(queen);
    }

    public override void ChaseQueen()
    {
        base.ChaseQueen();
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }
}
