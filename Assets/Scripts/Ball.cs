using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : NetworkBehaviour
{
    private Rigidbody2D _rb;

    [SerializeField] private float _speed = 10f;

    // Timer đồng bộ network để delay launch
    [Networked] private TickTimer StartTimer { get; set; }

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody2D>();

        // Chỉ Host setup
        if (Object.HasStateAuthority)
        {
            ResetBall();

            // Delay 1s cho tất cả client sync xong
            StartTimer = TickTimer.CreateFromSeconds(Runner, 1f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Chỉ Host điều khiển physics
        if (!Object.HasStateAuthority) return;

        // Khi timer xong → launch
        if (StartTimer.Expired(Runner))
        {
            Launch();
            StartTimer = TickTimer.None;
        }
    }

    // Reset ball về giữa
    public void ResetBall()
    {
        _rb.linearVelocity = Vector2.zero;
        transform.position = Vector2.zero;
    }

    // Launch ball (CHỈ Host gọi)
    public void Launch()
    {
        if (!Object.HasStateAuthority) return;

        _rb.linearVelocity = Vector2.zero;

        // Random chuẩn network (không dùng Unity Random)
        float angle = Runner.Simulation.Config.Random.Range(-45f, 45f) * Mathf.Deg2Rad;
        float dir = Runner.Simulation.Config.Random.Range(0, 2) == 0 ? -1 : 1;

        Vector2 velocity = new Vector2(
            dir * Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * _speed;

        _rb.linearVelocity = velocity;
    }

    // Gọi khi có điểm → reset và launch lại
    public void ResetAndLaunch()
    {
        if (!Object.HasStateAuthority) return;

        ResetBall();
        StartTimer = TickTimer.CreateFromSeconds(Runner, 1f);
    }

    /
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!Object.HasStateAuthority) return;

        if (collision.gameObject.CompareTag("Paddle"))
        {
            // Tăng tốc nhẹ mỗi lần chạm
            _rb.linearVelocity *= 1.05f;
        }
    }
}