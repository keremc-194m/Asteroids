using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScript : MonoBehaviour
{
    GameObject spaceship;
    Rigidbody2D spaceshipRb;

    public float leftConstraint = 0.0f;
    public float rightConstraint = 0.0f;
    public float topConstraint = 0.0f;
    public float bottomConstraint = 0.0f;
    public float buffer = 0.03f;
    public float distanceZ = 10.0f;
    bool thrusting = false;
    bool alive = true;
    bool gameOver = false;
    int score = 0;
    int health = 3;

    float uiWidth;
    float uiBuffer;

    List<GameObject> bullets = new List<GameObject>();
    List<GameObject> asteroids = new List<GameObject>();

    List<GameObject> shieldRenderers = new List<GameObject>();

    float thetaScale = 0.01f;
    int shieldSize = (int)((1f / 0.01f) + 1f);

    void Awake()
    {
        uiWidth = Screen.width * 0.24f;
        uiBuffer = Screen.width * 0.1f;
        leftConstraint = Camera.main.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, distanceZ)).x;
        rightConstraint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - uiWidth, 0.0f, distanceZ)).x - buffer;
        bottomConstraint = Camera.main.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, distanceZ)).y;
        topConstraint = Camera.main.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, distanceZ)).y;
    }

    void Start()
    {
        Vector3 topPoint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - uiWidth, Screen.height, distanceZ));
        Vector3 bottomPoint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - uiWidth, 0, distanceZ));

        GameObject childLineRendererObj = new GameObject("UIBarrierLine");
        LineRenderer lineRenderer = childLineRendererObj.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = .1f;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, topPoint);
        lineRenderer.SetPosition(1, bottomPoint);

        spaceship = new GameObject("spaceship");
        spaceship.tag = "spaceship";

        for (int i = 0; i < health; i++)
        {
            GameObject shieldGO = new GameObject("shield_" + i);
            shieldGO.transform.parent = spaceship.transform;
            LineRenderer shieldRenderer = shieldGO.AddComponent<LineRenderer>();
            shieldRenderer.widthMultiplier = .01f;
            shieldRenderer.positionCount = shieldSize;

            shieldRenderers.Add(shieldGO);
        }

        BoxCollider2D bc2d = spaceship.AddComponent<BoxCollider2D>();
        bc2d.isTrigger = true;
        SpriteRenderer renderer = spaceship.AddComponent<SpriteRenderer>();
        spaceshipRb = spaceship.AddComponent<Rigidbody2D>();
        renderer.sprite = Resources.Load<Sprite>("Spaceship");
        spaceship.transform.position = new Vector3(-2f, 1f);
        spaceship.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        Vector2 S = renderer.sprite.bounds.size;
        bc2d.size = S * 0.9f;
        bc2d.offset = new Vector2(0, 0);
        StartCoroutine(AsteroidGen());
    }

    public void AddScore(int score)
    {
        this.score += score;
    }

    void WrapObject(GameObject go)
    {
        if (go.transform.position.x < leftConstraint - buffer)
        {
            go.transform.position = new Vector3(rightConstraint + buffer, go.transform.position.y, go.transform.position.z);
        }

        if (go.transform.position.x > rightConstraint + buffer)
        {
            go.transform.position = new Vector3(leftConstraint - buffer, go.transform.position.y, go.transform.position.z);
        }

        if (go.transform.position.y < bottomConstraint - buffer)
        {
            go.transform.position = new Vector3(go.transform.position.x, topConstraint + buffer, go.transform.position.z);
        }

        if (go.transform.position.y > topConstraint + buffer)
        {
            go.transform.position = new Vector3(go.transform.position.x, bottomConstraint - buffer, go.transform.position.z);
        }
    }

    string textString = "Enter name";
    bool nameEntered = false;
    bool scoresLoaded = false;
    List<KeyValuePair<string, int>> scores = new List<KeyValuePair<string, int>>();

    void ReadScores()
    {
        scores.Clear();
        if (File.Exists(Application.persistentDataPath + "/scores.txt"))
        {
            string data = System.IO.File.ReadAllText(Application.persistentDataPath + "/scores.txt");
            foreach (string line in data.Split('\n'))
            {
                if (line.Length > 0)
                {
                    int score = System.Int32.Parse(line.Split(':')[0]);
                    string player = line.Substring(line.IndexOf(":") + 1);
                    scores.Add(new KeyValuePair<string, int>(player, score));
                }
            }
        }
    }
    void OnGUI()
    {
        if (gameOver)
        {
            if (!scoresLoaded)
            {
                ReadScores();
                scoresLoaded = true;
            }
            int windowWidth = 100;
            int x = (Screen.width - (windowWidth + (int) uiBuffer + 130)) / 2;
            int y = (int) ((Screen.height) * .33f);

            GUI.Label(new Rect(x, y, windowWidth, 100), "Game Over!");
            GUI.Label(new Rect(x, y + 20, windowWidth, 100), "Score: " + score);

            if (!nameEntered)
            {
                textString = GUI.TextField(new Rect(x, y + 50, windowWidth, 30), textString, 25);
                if (GUI.Button(new Rect(x + windowWidth, y + 50, 70, 30), "Submit"))
                {
                    using (StreamWriter w = File.AppendText(Application.persistentDataPath + "/scores.txt"))
                    {
                        w.WriteLine(score + ":" + textString);
                    }
                    scoresLoaded = false;
                    nameEntered = true;
                }
            }
            GUI.Label(new Rect(x, y + 90, windowWidth, 100), "High Scores:");
            int offset = 0;

            foreach (KeyValuePair<string, int> score in scores.OrderByDescending(kv => kv.Value).ToList())
            {
                GUI.Label(new Rect(x, y + 90 + (++offset * 20), 2000, 100), offset + ". " + score.Key + ": " + score.Value);
                if (offset >= 10) break;
            }

            if (GUI.Button(new Rect(x, y + 330, windowWidth, 30), "Play again"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        } else
        {
           GUI.Label(new Rect(Screen.width - uiWidth / 2.0f, Screen.height * 0.15f, 500, 500), "Score: " + score);
           GUI.Label(new Rect(Screen.width - uiWidth / 2.0f, Screen.height * 0.15f + 30, 500, 500), "Shields: " + System.Math.Max(health, 0));

        }
    }

    void Update() 
    {
        for (int idx = 0; idx < health; idx++)
        {
            GameObject go = shieldRenderers[idx];
            LineRenderer lr = go.GetComponent<LineRenderer>();
            float theta = 0f;
            float radius = 0.3f + (idx * .1f);
            for (int i = 0; i < shieldSize; i++)
            {
                theta += (2.0f * Mathf.PI * thetaScale);
                float x = radius * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(theta);
                x += spaceship.transform.position.x;
                y += spaceship.transform.position.y;
                Vector3 pos = new Vector3(x, y, 0);
                lr.SetPosition(i, pos);
            }

        }

        foreach (GameObject bullet in bullets.Reverse<GameObject>())
        {
            if (bullet.transform.position.x < leftConstraint - buffer)
            {
                DeleteBullet(bullet);
                bullets.Remove(bullet);
            }

            if (bullet.transform.position.x > rightConstraint + buffer)
            {
                Destroy(bullet);
                bullets.Remove(bullet);
            }

            if (bullet.transform.position.y < bottomConstraint - buffer)
            {
                Destroy(bullet);
                bullets.Remove(bullet);
            }

            if (bullet.transform.position.y > topConstraint + buffer)
            {
                Destroy(bullet);
                bullets.Remove(bullet);
            }
        }

        foreach (GameObject asteroid in asteroids)
        {
            WrapObject(asteroid);
        }

        if (alive) { 
            WrapObject(spaceship);

            if (Input.GetKey(KeyCode.J))
            {
                spaceship.transform.Rotate(Vector3.forward);
            }
            if (Input.GetKey(KeyCode.L))
            {
                spaceship.transform.Rotate(Vector3.back);
            }

            if (Input.GetKey(KeyCode.K))
            {
                spaceshipRb.AddForce(spaceship.transform.up * 1.15f);
                spaceshipRb.velocity = Vector3.ClampMagnitude(spaceshipRb.velocity, 7);

                AudioSource src = GameObject.Find("thruster_snd").GetComponent<AudioSource>();
                if (!thrusting)
                {
                    SpriteRenderer sr = spaceship.GetComponent<SpriteRenderer>();
                    sr.sprite = Resources.Load<Sprite>("SpaceshipThrusting");
                    src.Play();
                    thrusting = true;
                }
            }
            else
            {
                spaceshipRb.velocity = spaceshipRb.velocity / 1.001f;
                AudioSource src = GameObject.Find("thruster_snd").GetComponent<AudioSource>();
                src.Stop();
                if (thrusting)
                {
                    SpriteRenderer sr = spaceship.GetComponent<SpriteRenderer>();
                    sr.sprite = Resources.Load<Sprite>("Spaceship");
                }
                thrusting = false;
            }

            if (Input.GetKeyDown("space"))
            {
                AudioSource src = GameObject.Find("laser_snd").GetComponent<AudioSource>();
                src.Play();

                GameObject bullet = new GameObject("bullet");
                bullet.tag = "bullet";

                BoxCollider2D bc2d = bullet.AddComponent<BoxCollider2D>();
                bc2d.isTrigger = true;
                SpriteRenderer renderer = bullet.AddComponent<SpriteRenderer>();
                Rigidbody2D rb2d = bullet.AddComponent<Rigidbody2D>();
                renderer.sprite = Resources.Load<Sprite>("Bullet");
                Vector2 S = renderer.sprite.bounds.size;
                bc2d.size = S;
                bc2d.offset = new Vector2(0, 0);
                bullet.transform.position = spaceship.transform.position;
                bullet.transform.rotation = spaceship.transform.rotation;
                rb2d.AddForce(bullet.transform.up * 1000f);
                bullets.Add(bullet);
            }
        }
    }

    public void onShipHit()
    {
        if (--health < 0)
        {
            AudioSource src = GameObject.Find("ship_explode_snd").GetComponent<AudioSource>();
            src.Play();
            Destroy(spaceship);
            onShipDeath();
        }
        else
        {
            AudioSource src = GameObject.Find("ship_hit_snd").GetComponent<AudioSource>();
            src.Play();
            GameObject shieldGO = shieldRenderers[health];
            shieldRenderers.Remove(shieldGO);
            Destroy(shieldGO);
        }

    }

    public void onShipDeath()
    {
        alive = false;
        AudioSource src = GameObject.Find("thruster_snd").GetComponent<AudioSource>();
        src.Stop();

        foreach (GameObject asteroid in asteroids.Reverse<GameObject>())
        {
            Destroy(asteroid);
            DeleteAsteroid(asteroid);
        }

        StartCoroutine(GameOver());


    }

    public void DeleteBullet(GameObject bullet)
    {
        bullets.Remove(bullet);
    }

    public void DeleteAsteroid(GameObject asteroid)
    {
        asteroids.Remove(asteroid);
    }

    public void AddAsteroid(int size, Vector3? seedPos)
    {
        float tempX, tempY;
        if (seedPos.HasValue)
        {
            tempX = seedPos.Value.x;
            tempY = seedPos.Value.y;
        }
        else
        {
            float side = Random.Range(0.0f, 12.0f);
            if (side <= 3)
            {
                tempY = topConstraint;
                tempX = Random.Range(leftConstraint, rightConstraint);
            }
            else if (side <= 6)
            {
                tempY = bottomConstraint;
                tempX = Random.Range(leftConstraint, rightConstraint);
            }
            else if (side <= 9)
            {
                tempY = Random.Range(bottomConstraint, topConstraint);
                tempX = leftConstraint;
            }
            else
            {
                tempY = Random.Range(bottomConstraint, topConstraint);
                tempX = rightConstraint;
            }
        }

        GameObject asteroid = new GameObject("asteroid");

        asteroid.AddComponent<AsteroidCollisionDetector>();
        BoxCollider2D bc2d = asteroid.AddComponent<BoxCollider2D>();
        bc2d.isTrigger = true;
        SpriteRenderer renderer = asteroid.AddComponent<SpriteRenderer>();
        Rigidbody2D rb2d = asteroid.AddComponent<Rigidbody2D>();
        rb2d.AddTorque((Random.Range(0, 2) * 2 - 1) * Mathf.Lerp(50f, 130f, Random.value));
        asteroid.AddComponent<AudioSource>();
        renderer.sprite = Resources.Load<Sprite>("Asteroid");
        Vector2 S = renderer.sprite.bounds.size;
        bc2d.size = S;
        bc2d.offset = new Vector2(0, 0);
        asteroid.transform.position = new Vector3(tempX, tempY);

        switch (size)
        {
            case 1:
                asteroid.tag = "asteroid_small";
                asteroid.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                break;
            case 2:
                asteroid.tag = "asteroid_medium";
                asteroid.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                break;
            case 3:
                asteroid.tag = "asteroid_big";
                asteroid.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                break;
        }


        rb2d.AddForce(Random.insideUnitCircle.normalized * Mathf.Lerp(100f, 150f + (size * 25f), Random.value));

        asteroids.Add(asteroid);
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(4);
        gameOver = true;
    }

    private IEnumerator AsteroidGen()
    {
        while (true)
        {
            if (alive)
            {
                AddAsteroid(3, null);
            } else
            {
                break;
            }
            //asteroid.transform.rotation = Quaternion.LookRotation(Random.insideUnitCircle.normalized, Vector3.up);
            yield return new WaitForSeconds(Mathf.Lerp(2f, 4f, Random.value));
        }
    }

}