using System;
using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;   

    private Rigidbody2D body;
    private Animator animator;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private AudioSource sound;
    private BoxCollider2D box_collider;

    private bool doubleJump = true;
    private bool wasInAir = false;

    private int maxScoreValue = 0;

    private states state
    {
        get { return (states)animator.GetInteger("state"); }
        set { animator.SetInteger("state", (int)value); }
    }


    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        box_collider = GetComponent<BoxCollider2D>();
    }



    private void Update()
    {
        checkScore();

        if (onGround()) state = states.idle;
        else if (onWall()) state = states.on_wall;
        else state = states.jump;

        if (!onWall()) Physics2D.gravity = new Vector3(0f, -9.81f, 0f); // Нормальная гравитация если персонаж не висит на стене

        if (!onGround() && !onWall()) wasInAir = true;

        if (onGround() || onWall()) doubleJump = true; // Просто возможность второго прыжка 

        if (onWall() && wasInAir)
        {                               // Добавил bool wasInAir чтобы Flip и Vector2.zero применялись только один раз на 1 стену,  
                                        // меняю гравитацию чтобы персонаж медленно сползал со стены
            Flip();
            body.velocity = Vector2.zero;
            wasInAir = false;
            Physics2D.gravity = new Vector3(0f, -0.1f, 0f);
        }

        if (onGround() && Input.GetButton("Horizontal")) Run();
        if (Input.GetKeyDown(KeyCode.Space) && (onGround() || onWall() || doubleJump))  
        {
            if (!onGround() && !onWall()) doubleJump = false; // двойной прыжок обращается в false только если нужно прыгнуть в воздухе(рядом нет стен или земли)
            sound.Play();
            Jump();
        }        
    }

    private void checkScore()
    {
        int cScore = (int)body.position.y + 10;
        cScore /= 2;
        maxScoreValue = Math.Max(maxScoreValue, cScore);
        ScoreSystem.scoreValue = maxScoreValue;
    }

    private void Run()
    {
        state = states.run;

        Vector3 distance = transform.right * Input.GetAxis("Horizontal");
        transform.position = Vector3.MoveTowards(transform.position, transform.position + distance, speed * Time.deltaTime);

        sprite.flipX = distance.x < 0.0f;
    }

    private void Jump() //Если прыжок со стены или в воздухе, flipX
    {
        if (onGround())
        {
            if (sprite.flipX) body.velocity = new Vector2(-0.5f * jumpForce, jumpForce);
            else body.velocity = new Vector2(0.5f * jumpForce, jumpForce);
        }
        else if (onWall())
        {
            if (sprite.flipX) body.velocity = new Vector2(0.5f * jumpForce, jumpForce);
            else body.velocity = new Vector2(-0.5f * jumpForce, jumpForce);
            sprite.flipX = !sprite.flipX;
        }
        else
        {
            if (!sprite.flipX) body.velocity = new Vector2(-0.5f * jumpForce, jumpForce);
            else body.velocity = new Vector2(0.5f * jumpForce, jumpForce);
            sprite.flipX = !sprite.flipX;
        }
    }

    private bool onGround()
    {
        RaycastHit2D raycast_hit = Physics2D.BoxCast(box_collider.bounds.center, box_collider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);

        return (raycast_hit.collider != null);
    }

    private bool onWall()
    {
        RaycastHit2D raycast_hit1 = Physics2D.BoxCast(box_collider.bounds.center, new Vector2(0.8425932f, 0.1f), 0, Vector2.right, 0.1f, wallLayer);
        RaycastHit2D raycast_hit2 = Physics2D.BoxCast(box_collider.bounds.center, new Vector2(0.8425932f, 0.1f), 0, Vector2.left, 0.1f, wallLayer);

        return (raycast_hit1.collider != null) || (raycast_hit2.collider != null);
    }

    private void Flip()
    {
        if (!onWall()) return;
        RaycastHit2D raycast_hit1 = Physics2D.BoxCast(box_collider.bounds.center, box_collider.bounds.size, 0, Vector2.right, 0.1f, wallLayer);
        RaycastHit2D raycast_hit2 = Physics2D.BoxCast(box_collider.bounds.center, box_collider.bounds.size, 0, Vector2.left, 0.1f, wallLayer);

        if (raycast_hit1.collider != null) sprite.flipX = false;
        if (raycast_hit2.collider != null) sprite.flipX = true;

    }
}

public enum states { idle, run, jump, on_wall }
