using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float rotVar = 10f;
    public float currentSpeed;
    float minSpeed = 80f;
    [SerializeField] [Range(100f, 500f)] float topSpeed = 300f;
    [SerializeField] [Range(0f,1f)] float turnSens = 0.2f;
    GameController gc;
    [SerializeField] AudioClip[] engines;
    AudioSource aSrc;

    int livesLeft;
    void Start()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        currentSpeed = minSpeed;

        aSrc = GetComponent<AudioSource>();
        aSrc.clip = engines[0];

        aSrc.Play();
        livesLeft = 3;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        if (currentSpeed > 250f) { aSrc.pitch = 1.3f; }
        else if (currentSpeed > 200f) { aSrc.pitch = 1.2f; }
        else if (currentSpeed > 175f) { aSrc.pitch = 1.1f; }

        Vector3 sideVec = new Vector3(turnSens, 0f, 0f);

        if (Input.GetKey(gc.keys[0]))
        {
            gameObject.transform.SetPositionAndRotation(transform.position - sideVec, Quaternion.Euler(0f, 0f, rotVar));
        }
        else if (Input.GetKey(gc.keys[1]))
        {
            gameObject.transform.SetPositionAndRotation(transform.position + sideVec, Quaternion.Euler(0f, 0f, -rotVar));
        }

        if (Input.GetKey(gc.keys[2]) && currentSpeed > minSpeed) { currentSpeed -= Time.deltaTime * 5; }
        else if (currentSpeed > topSpeed) { currentSpeed = topSpeed; }
        else { currentSpeed += Time.deltaTime * 2.5f; }

        if (!Input.anyKey) { gameObject.transform.SetPositionAndRotation(transform.position, Quaternion.identity); }

        if (transform.position.x > 3.85f || transform.position.x < -3.85f)
        {
            transform.position = new Vector3((Mathf.Abs(transform.position.x) / transform.position.x) * 3.85f, transform.position.y, transform.position.z);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            case "Car":
                if (collision.transform.GetComponent<CarScript>().active)
                {
                    collision.transform.GetComponent<CarScript>().active = false;
                    gc.Lose(transform.position);
                }
                break;
            case "Barrier":
                gc.Lose(transform.position); break;
            default:
                transform.position = transform.position; break;
        }
    }
}
