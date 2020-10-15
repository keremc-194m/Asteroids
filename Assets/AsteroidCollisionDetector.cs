using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidCollisionDetector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        GameObject other = col.gameObject;
        GameScript gs = (GameScript)Camera.main.GetComponent<GameScript>();
        if (other.tag != null)
        {
            switch (other.tag)
            {
                case "spaceship":
                    gs.onShipHit();
                    gs.DeleteAsteroid(gameObject);
                    Destroy(gameObject);
                    break;
                case "bullet":
                    AudioSource src = GameObject.Find("asteroid_explode_snd").GetComponent<AudioSource>();
                    src.Play();
                    switch (tag)
                    {
                        case "asteroid_big":
                            for (int i = 0; i < 3; i++)
                            {
                                gs.AddAsteroid(2, gameObject.transform.position);
                            }
                            gs.AddScore(20);
                            break;
                        case "asteroid_medium":
                            for (int i = 0; i < 3; i++)
                            {
                                gs.AddAsteroid(1, gameObject.transform.position);
                            }
                            gs.AddScore(50);
                            break;
                        case "asteroid_small":
                            gs.AddScore(100);
                            break;
                    }
                    gs.DeleteAsteroid(gameObject);
                    gs.DeleteBullet(other);
                    Destroy(other);
                    Destroy(gameObject);
                    break;
            }
        }
    }
}
