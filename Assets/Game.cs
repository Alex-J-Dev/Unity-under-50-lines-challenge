using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Game : MonoBehaviour
{
    public GameObject block;
    public GameObject bullet;
    public GameObject deathScreen;
    public TextMeshProUGUI scoreText;
    public float nextSpawnTime = 1f;
    public List<GameObject> bullets = new List<GameObject>();
    void Update()                                                                                                                                                                                                        
    {                                                                                                                                                                                                                    
        if (deathScreen.activeInHierarchy) return;                                                                                                                                                                       // Cheap death mechanic - return if the deathscreen is active to prevent the game logic from running
        if (Input.GetKeyDown(KeyCode.Space)) bullets.Add(Instantiate(bullet, transform.position, Quaternion.identity));                                                                                                  // When space is pressed shoot. Add this to a list so we can later track active bullets
        foreach (var bullet in GameObject.FindGameObjectsWithTag("Bullet")) bullet.transform.Translate(Vector3.up* 10f * Time.deltaTime);                                                                                // Awful logic (but one line) to move all bullets up
        foreach (var block in GameObject.FindGameObjectsWithTag("Block"))                                                                                                                                                // All blocks have the tag, so we can iterate through them like this
        {                                                                                                                                                                                                                //
            block.transform.Translate(Vector3.down * (5f + Time.timeSinceLevelLoad * 0.1f) * Time.deltaTime, Space.World);                                                                                               // move blocks down (gravity was not used so we could speed them up and keep them consistent)
            if (bullets.Exists(bullet=>block.GetComponent<Collider2D>().bounds.Intersects(bullet.GetComponent<Collider2D>().bounds))) block.transform.localScale -= block.transform.localScale / 10f;                    // DISGUSTING logic, but it checks if the block connected with a bullet. If so, make the block smaller (bullet shaves it essentially)
            if (block.transform.position.y < -5f || block.transform.localScale.x <= 0.2f)                                                                                                                                // If the bullet gets below a specific size, destory it
            {                                                                                                                                                                                                            //
                block.GetComponent<Collider2D>().enabled = false;                                                                                                                                                        // Disable collider to prevent it affecting the player
                block.GetComponent<SpriteRenderer>().enabled = false;                                                                                                                                                    // Disable sprite renderer so particle effect is cleaner
                block.GetComponent<ParticleSystem>().Play();                                                                                                                                                             // Begin particle explosion
                Destroy(block, block.GetComponent<ParticleSystem>().main.duration);                                                                                                                                      // Destroy once particle effect is complete
            }                                                                                                                                                                                                            //
        }                                                                                                                                                                                                                //
        transform.position = new Vector3(Mathf.Clamp(transform.position.x + Input.GetAxisRaw("Horizontal") * 8f * Time.deltaTime, -8.5f, 8.5f), transform.position.y, transform.position.z);                             // Player movement logic - clamp to the screen bounds
        if (Input.GetAxisRaw("Horizontal") != 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * (Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1), transform.localScale.y, transform.localScale.z);  // Detect what way the player is moving to change where they are looking
        if (Time.timeSinceLevelLoad < nextSpawnTime) return;                                                                                                                                                             // If we have elapsed the time between spawning a block, continue. Otherwise, return here. Awful!
        Instantiate(block, new Vector3(Random.Range(-8.5f, 8.5f), 6f, 0f), Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));                                                                                            // Create the new block with a random x value and rotation
        PlayerPrefs.SetInt("HighScore", Mathf.Max(int.Parse(scoreText.text[7..]) + 2, PlayerPrefs.GetInt("HighScore")));                                                                                                 // AWFUL!! Set the highscore if the score is higher EVERY TIME! this should be at the end! EW!
        scoreText.text = deathScreen.transform.Find("Final Score").GetComponent<TextMeshProUGUI>().text = "SCORE: " + (int.Parse(scoreText.text[7..]) + 1);                                                              // GROSS. Get the score from the score text element.. And increment it!
        deathScreen.transform.Find("Final Score").GetComponent<TextMeshProUGUI>().text = "SCORE: " + (int.Parse(scoreText.text[7..]) + 1) + " HIGHSCORE: " + PlayerPrefs.GetInt("HighScore");                            // DO the same for the death screen
        nextSpawnTime = Time.timeSinceLevelLoad + Mathf.Max(0.25f, 1f - (0.02f * int.Parse(scoreText.text[7..])));                                                                                                       // Identify when the next spawn time should be
    }                                                                                                                                                                                                                    
    private void OnTriggerEnter2D(Collider2D collision)                                                                                                                                                                  // When we collide with something
    {                                                                                                                                                                                                                    //
        if (collision.gameObject.CompareTag("Block")) deathScreen.SetActive(true);                                                                                                                                       // Check if it is a block. If so, set the deathscreen.
    }                                                                                                                                                                                                                    
    public void restart()                                                                                                                                                                                                // Public method for the play again button
    {                                                                                                                                                                                                                    //
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);                                                                                                                                                      // Restart the entire scene. Quick and easy cleanup with minimal code.
    }                                                                                                                                                                                                                    
}