using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneController : MonoBehaviour
{
    public GameObject player;
    private Rigidbody rigid;
    private CameraController camCon;
    private GameObject cam;

    private UnityEngine.InputSystem.PlayerInput Input;
    private InputAction moveInput;
    private InputAction moveUpInput;
    private InputAction moveDownInput;
    private InputAction exitInput;

    private Vector3 moveDir;
    public float speed = 1.0f;

    private void Awake()
    {
        rigid = gameObject.GetComponent<Rigidbody>();
        camCon = Camera.main.gameObject.GetComponent<CameraController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Input = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        moveInput = Input.actions["Move"];
        moveUpInput = Input.actions["Up"];
        moveDownInput = Input.actions["Down"];
        exitInput = Input.actions["Exit"];

        foreach(Transform child in gameObject.transform)
        {
            if(child.name == "Cam") { cam = child.gameObject; }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDir1 = moveInput.ReadValue<Vector2>().x * Camera.main.transform.right;
        Vector3 moveDir2 = moveInput.ReadValue<Vector2>().y * Camera.main.transform.forward;
        moveDir1.y = 0;
        moveDir2.y = 0;
        moveDir = moveDir1 + moveDir2;

        if (exitInput.IsPressed()) { Exit(); }
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void Move()
    {
        rigid.AddForce(moveDir.normalized * speed, ForceMode.Force);
        if (moveUpInput.IsPressed()) { transform.position = Vector3.up * Time.deltaTime; }
        if (moveDownInput.IsPressed()) { transform.position = Vector3.down * Time.deltaTime; }
    }

    public void OnComponents()
    {
        cam.SetActive(true);
    }

    public void Exit()
    {

        Input.enabled = false;
        UnityEngine.InputSystem.PlayerInput inputs = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        inputs.enabled = true;

        camCon.camType = Defines.CamType.ThirdPerson;

        gameObject.SetActive(false);

    }
}
