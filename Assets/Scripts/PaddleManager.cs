using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class PaddleManager : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    public bool isLeft = true;

    public override void FixedUpdateNetwork()
    {
        if(HasStateAuthority == false) return;
        if (GetInput<PlayerInputData>(out var data))
        {
            transform.Translate(Vector3.up * data.direction * _speed * Runner.DeltaTime);

            Vector3 p = transform.position;
            p.y = Mathf.Clamp(p.y, -4f, 4f);
            transform.position = p;
        }
    }
}