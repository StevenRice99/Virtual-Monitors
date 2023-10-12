using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class TestingController : MonoBehaviour
{
    [Tooltip("The speed to move the camera at.")]
    [Min(float.Epsilon)]
    [SerializeField]
    private float speed = 1;
    
    private Transform _transform;

    private void Start()
    {
        _transform = transform;
    }

    private void Update()
    {
        Vector3 movement = Vector3.zero;

        float deltaSpeed = speed * Time.deltaTime;
        
        if (Keyboard.current.upArrowKey.isPressed)
        {
            movement += new Vector3(0, deltaSpeed, 0);
        }
        
        if (Keyboard.current.downArrowKey.isPressed)
        {
            movement -= new Vector3(0, deltaSpeed, 0);
        }
        
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            movement -= new Vector3(deltaSpeed, 0, 0);
        }
        
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            movement += new Vector3(deltaSpeed, 0, 0);
        }

        _transform.localPosition += movement;
    }
}