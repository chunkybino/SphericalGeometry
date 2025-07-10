using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public PlayerInputActions inputs;

    public bool up {get{return m_up.IsPressed();}}
    public bool left {get{return m_left.IsPressed();}}
    public bool down {get{return m_down.IsPressed();}}
    public bool right {get{return m_right.IsPressed();}}
    public bool forward {get{return m_forward.IsPressed();}}
    public bool backward {get{return m_backward.IsPressed();}}


    public bool upArrow {get{return m_upArrow.IsPressed();}}
    public bool downArrow {get{return m_downArrow.IsPressed();}}
    public bool leftArrow {get{return m_leftArrow.IsPressed();}}
    public bool rightArrow {get{return m_rightArrow.IsPressed();}}

    public bool space {get{return m_space.IsPressed();}}
    public bool spacePress {get{return m_space.triggered;}}

    public Vector2 mouseDelta {get{return m_mouse.ReadValue<Vector2>();}}

    [HideInInspector] public InputAction m_up;
    [HideInInspector] public InputAction m_down;
    [HideInInspector] public InputAction m_left;
    [HideInInspector] public InputAction m_right;
    [HideInInspector] public InputAction m_forward; 
    [HideInInspector] public InputAction m_backward;

    [HideInInspector] public InputAction m_upArrow;
    [HideInInspector] public InputAction m_downArrow;
    [HideInInspector] public InputAction m_leftArrow;
    [HideInInspector] public InputAction m_rightArrow;

    [HideInInspector] public InputAction m_space;

    [HideInInspector] public InputAction m_mouse;

    void Awake()
    {
        inputs = new PlayerInputActions();
    }

    void OnEnable()
    {
        if (inputs == null) inputs = new PlayerInputActions();
        inputs.Player.Enable();
        SetUpActions();
    }
    void OnDisable()
    {
        inputs.Player.Disable();
    }

    void SetUpActions()
    {
        m_up = inputs.Player.Up;
        m_left = inputs.Player.Left;
        m_down = inputs.Player.Down;
        m_right = inputs.Player.Right;
        m_forward = inputs.Player.Forward;
        m_backward = inputs.Player.Backward;

        m_upArrow = inputs.Player.UpArrow;
        m_downArrow = inputs.Player.DownArrow;
        m_leftArrow = inputs.Player.LeftArrow;
        m_rightArrow = inputs.Player.RightArrow;

        m_space = inputs.Player.Space;

        m_mouse = inputs.Player.MouseDelta;
    }
}
