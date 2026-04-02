using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : NetworkBehaviour
{
    private Rigidbody2D _rb;

    [SerializeField] private float _speed = 10f;
    private GameManager _gameManager;
    // Timer delay launch
    [Networked] private TickTimer StartTimer { get; set; }

    // RNG đồng bộ
    [Networked] private NetworkRNG Random { get; set; }

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody2D>();
        _gameManager = FindFirstObjectByType<GameManager>();
        if (Object.HasStateAuthority)
        {
            Random = new NetworkRNG(Runner.Tick);

            ResetBall();

            
            StartTimer = TickTimer.None;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // if (!Object.HasStateAuthority) return;

        if (StartTimer.Expired(Runner))
        {
            Launch();
            StartTimer = TickTimer.None;
        }
    }

    // Reset về giữa
    public void ResetBall()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        transform.position = Vector2.zero;
    }

    // Launch ball
    public void Launch()
    {
        if (!Object.HasStateAuthority) return;

        _rb.linearVelocity = Vector2.zero;

        // Random góc (-45 → 45)
        float angle = Mathf.Lerp(-45f, 45f, (float)Random.Next()) * Mathf.Deg2Rad;

        // Random trái/phải
        int dir = Random.Next() % 2 == 0 ? -1 : 1;

        Vector2 velocity = new Vector2(
            dir * Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * _speed;

        _rb.linearVelocity = velocity;
    }

    // Reset + launch lại
    public void ResetAndLaunch()
    {
        if (!Object.HasStateAuthority) return;

        ResetBall();
        StartTimer = TickTimer.CreateFromSeconds(Runner, 1f);
    }
    private void ForceReset()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        // Teleport ngay lập tức
        transform.position = Vector2.zero;

        // Delay launch
        StartTimer = TickTimer.CreateFromSeconds(Runner, 1f);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!Object.HasStateAuthority) return;

        if (collision.gameObject.CompareTag("Paddle"))
        {
            
            _rb.linearVelocity *= 1.05f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Object.HasStateAuthority) return;

        if (other.CompareTag("LeftBorder"))
        {
            _gameManager.AddScoreRight(); 
            ForceReset();
        }
        else if (other.CompareTag("RightBorder"))
        {
            _gameManager.AddScoreLeft(); 
            ForceReset();
        }
    }
}