using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerControl : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 3f;

    [Header("Jump")]
    public float jumpForce = 6f;
    public int maxJumps = 2;           // total jump count (1 ground + 1 mid-air)
    private int jumpsPerformed = 0;

    [Header("Ground Check (trigger)")]
    public Transform groundCheck;      // GameObject kecil di bawah kaki (CircleCollider2D isTrigger)
    private bool isGrounded = false;
    private bool wasInAir = false;

    [Header("Effects")]
    public ParticleSystem landingDust; // efek debu saat mendarat

    [Header("Die / Restart")]
    public float dieY = 1.0f;

    // internal
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    // tambahan untuk manual movement 
    private Vector2 velocity;
    public float gravity = -20f;

    // PowerUp
    //private Vector3 enlargedScale = new Vector3(0.36f, 0.36f, 0.36f);
    private Vector3 enlargedScale = new Vector3(0.9f, 0.9f, 0.9f);
    private Vector3 originalScale;

    private Color powerUpColor = Color.magenta;
    private Color originalColor;

    public Material glowMaterial;
    private Material originalMaterial;

    private float duration = 3f;
    private float boost = 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //  matikan gravitasi bawaan Rigidbody agar tidak bentrok dengan manual gravity
        rb.gravityScale = 0f;
    }

    void Start()
    {
        originalScale = transform.localScale;
        originalColor = spriteRenderer.color;
        originalMaterial = spriteRenderer.material;
    }

    void Update()
    {
        if (!GameManager.isGameStarted) return;
        // HORIZONTAL MOVEMENT
        float xInput = 0f;
        if (Input.GetKey(KeyCode.A)) xInput = -1f;
        if (Input.GetKey(KeyCode.D)) xInput = 1f;

        velocity.x = xInput * maxSpeed;

        // manual gravity
        velocity.y += gravity * Time.deltaTime;

        // JUMP
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded || jumpsPerformed < maxJumps)
            {
                // tetapkan kecepatan vertikal langsung ke jumpForce (overwrites current vy)
                velocity.y = jumpForce;
                jumpsPerformed++;

                // Jika ini adalah lompatan kedua (Double Jump), lakukan rotasi
                if (!isGrounded && jumpsPerformed == 2)
                {
                    StartCoroutine("DoFlip");
                }

                wasInAir = true;
                isGrounded = false;
            }
        }

        // apply movement (manual transform)
        transform.position += (Vector3)(velocity * Time.deltaTime);

        // DIE / RESTART
        if (transform.position.y < dieY)
        {
            FindObjectOfType<GameManager>().PlayerDied();
        }

    }

    // COROUTINE ROTASI
    IEnumerator DoFlip()
    {
        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Hitung persentase waktu (0.0 sampai 1.0)
            float persenTIme = elapsed / duration;

            // Dari 0 ke -360 posisi ke t
            float currentAngleZ = Mathf.Lerp(0f, -360f, persenTIme);

            // Terapkan rotasi, angle Z berubah sesuai waktu
            transform.rotation = Quaternion.Euler(0, 0, currentAngleZ);

            yield return null;
        }

        // Pastikan di akhir rotasi kembali tegak lurus sempurna
        transform.rotation = Quaternion.identity;
    }


    // TRIGGER DETEKSI TANAH
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpsPerformed = 0;

            // Reset kecepatan vertikal saat mendarat
            velocity.y = 0f;

            // landing hanya jika sebelumnya di udara
            if (wasInAir && landingDust != null)
                landingDust.Play();

            wasInAir = false;
        }

        if (collision.CompareTag("PowerUp"))
        {
            // Membuat player membesar
            transform.localScale = enlargedScale;

            // menambah kecepatan
            maxSpeed += boost;

            // Mengubah warna & material glow
            spriteRenderer.color = powerUpColor;
            spriteRenderer.material = glowMaterial;

            // Hancurkan item
            Destroy(collision.gameObject);

            // Reset kembali normal
            StartCoroutine(ResetAfterDelay());
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // lepas dari tanah = bukan grounded
        if (collision.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private System.Collections.IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(duration);

        // Kembali normal
        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
        spriteRenderer.material = originalMaterial;
        maxSpeed -= boost;
    }
}
