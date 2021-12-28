using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighting : MonoBehaviour
{
    public int oreHardness;
    public bool left;
    public int enemyHealth;
    public float timer;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioSource audioPlayer;
    public EnemyMovement enmov;

    void Start()
    {
        enemyHealth = 4;
        timer = 0f;
    }

    void Update()
    {
        if (gameObject.activeSelf)
        {
            if (left)
            {
                AttackingLeft();
            }
            if (left == false)
            {
                AttackingRight();
            }
        }
    }

    // Attacking animation for the sword to the left
    public void AttackingLeft()
    {
        if (timer < 0.5f)
        {
            transform.position += transform.up * 1f * Time.deltaTime;
            transform.position -= transform.right * 1f * Time.deltaTime;
            transform.Rotate(Vector3.forward * 180 * Time.deltaTime);
            timer += Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false);
            transform.localPosition = new Vector3(-0.275f, 0.05f, -0.01f);
            transform.rotation = Quaternion.Euler(0, 0, 90);
            timer = 0;
        }
    }

    // Attacking animation for the sword to the right
    public void AttackingRight()
    {
        if (timer < 0.5f)
        {
            transform.position -= transform.up * 1f * Time.deltaTime;
            transform.position += transform.right * 1f * Time.deltaTime;
            transform.Rotate(Vector3.forward * -180 * Time.deltaTime);
            timer += Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false);
            transform.localPosition = new Vector3(0.275f, 0.05f, -0.01f);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            timer = 0;
        }
    }

    // Attacking an Enemy
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            // First get the script attached to the enemy and compare the hardness of the enemy to the hardness of the sword
            enmov = other.gameObject.GetComponent<EnemyMovement>();
            if (enmov.monHardness > oreHardness)
            {
                // No damage is done if the enemy is harder
                audioPlayer.clip = hitSound;
                audioPlayer.Play();
            }
            else
            {
                // If the enemy is not harder, the sword deals damage
                if (enemyHealth > 0)
                {
                    enemyHealth--;
                    audioPlayer.clip = hitSound;
                    audioPlayer.Play();
                }
                else
                {
                    // When the enemy is defeated, it drops its rock piece
                    enemyHealth = 4;
                    if (other.name != "PracticeEnemy")
                    {
                        GameObject child = other.transform.GetChild(0).gameObject;
                        child.SetActive(true);
                        child.transform.parent = null;
                    }
                    audioPlayer.clip = deathSound;
                    audioPlayer.Play();
                    Destroy(other.gameObject);
                }
            }
        }
    }
}
