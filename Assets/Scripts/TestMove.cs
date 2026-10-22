using UnityEngine;
using UnityEngine.InputSystem;

public class TestMove : MonoBehaviour
{
    public Animator animator;
    private Vector2 moveInput;

    // Этот метод должен вызываться PlayerInput (Send Messages)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log("Я получил ввод: " + moveInput);
    }

    void Update()
    {
        if (animator != null)
        {
            // Просто передаем сырой ввод в аниматор, без всякой физики
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetBool("IsMoving", moveInput.sqrMagnitude > 0.01f);
        }
    }
}