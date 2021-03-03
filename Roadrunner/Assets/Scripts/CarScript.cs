using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarScript : MonoBehaviour
{
    GameController gc;
    GameObject player;
    PlayerController pScr;

    [SerializeField]AudioClip honk;
    AudioClip eng;
    AudioSource aSrc;
    public float speed;

    bool honked;
    public bool active;
    public float side;

    float lBorder;
    float rBorder;

    float honkRadius;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        pScr = player.GetComponent<PlayerController>();

        side = transform.position.x > 0 ? 1 : -0.1f;

        if (side < 1) { GetComponent<SpriteRenderer>().flipY = true; }

        if (GetComponent<SpriteRenderer>().sprite.name.StartsWith("truck"))
        {
            honk = gc.horns[Random.Range(3, 4)];
            eng = gc.engines[Random.Range(1, 2)];
        }
        else
        {
            honk = gc.horns[Random.Range(0, 2)];
            eng = gc.engines[0];
        }
        aSrc = GetComponent<AudioSource>();
        aSrc.clip = eng;
        aSrc.Play();

        honked = false;
        active = true;

        honkRadius = UnityEngine.Random.Range(0.1f, 0.3f);
        lBorder = transform.position.x - GetComponent<BoxCollider2D>().size.x * honkRadius * 0.5f;
        rBorder = transform.position.x + GetComponent<BoxCollider2D>().size.x * honkRadius * 0.5f;
    }
    private void FixedUpdate()
    {
        gameObject.transform.position -= side > 0 ? new Vector3(0f, (pScr.currentSpeed - speed * side) / 300f, 0f) : new Vector3(0f, (pScr.currentSpeed / 1.2f + speed * side) / 300f, 0f);
        float audioDist = Mathf.Abs((transform.position.y - player.transform.position.y) / gc.lowPoint);
        float yDif = Mathf.Abs(transform.position.y - player.transform.position.y);

        aSrc.volume = 1 - audioDist;

        //adjusting speed to other cars in traffic
        Vector3 frontPos = new Vector3(transform.position.x, transform.position.y + (side * 2f), transform.position.z);
        RaycastHit2D hit = Physics2D.Raycast(frontPos, Vector2.zero);
        if (hit && hit.collider.tag == "Car") { speed = hit.collider.gameObject.GetComponent<CarScript>().speed; }

        RaycastHit2D hitLborder = Physics2D.Raycast(new Vector3(lBorder,transform.position.y - yDif,transform.position.z),Vector2.zero);
        RaycastHit2D hitRborder = Physics2D.Raycast(new Vector3(rBorder, transform.position.y - yDif, transform.position.z), Vector2.zero);

        if ((hitLborder || hitRborder || player.transform.position.x > lBorder || player.transform.position.y < rBorder) && player.transform.position.x < 0 && yDif < 5f)
        {
            if (!honked)
            {
                aSrc.PlayOneShot(honk);
                honked = true;
            }
        }

        if (gameObject.transform.position.y < player.transform.position.y && side/Mathf.Abs(side) == player.transform.position.x/Mathf.Abs(player.transform.position.x) && active)
        {
            float xDif = Mathf.Abs(transform.position.x - player.transform.position.x);
            int spDif = (int)(pScr.currentSpeed - speed);
            float closeBonus = xDif < 1f ? 2 : xDif < 1.5f ? 1f : 0.3f;
            float pow = side > 0 ? 1 : 2f;

            gc.AddPoints(spDif, closeBonus, pow);
            active = false;
        }
    }
}
