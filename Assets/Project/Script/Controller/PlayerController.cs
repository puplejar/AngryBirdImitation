using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rigid;
    private SoundController soundController;
    // CapsuleCollider collider;

    private UnityEngine.InputSystem.PlayerInput Input;
    private InputAction moveInput;
    private InputAction jumpInput;
    private InputAction mouseLeftInput;
    private InputAction mouseRightInput;

    private CameraController camCon;

    private Vector3 moveDir;
    private bool isGround = true;
    private bool isMovingSound = false;
    private string groundName;

    public float speed = 1.0f;
    public float groundDis = 0.1f;

    private void Awake()
    {
        rigid = gameObject.GetComponent<Rigidbody>();
        soundController = gameObject.GetComponent<SoundController>();
        //collider = gameObject.GetComponent<CapsuleCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Input = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        moveInput = Input.actions["Move"];
        jumpInput = Input.actions["Jump"];
        mouseLeftInput = Input.actions["Shoot"];
        mouseRightInput = Input.actions["Aim"];

        camCon = Camera.main.gameObject.GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDir1 = moveInput.ReadValue<Vector2>().x * Camera.main.transform.right;
        Vector3 moveDir2 = moveInput.ReadValue<Vector2>().y * Camera.main.transform.forward;
        moveDir1.y = 0;
        moveDir2.y = 0;
        moveDir = moveDir1 + moveDir2;

    }

    private void FixedUpdate()
    {
        GroundCheck();
        Move();
        ShootAndAiming();
    }

    public void Move()
    {
        rigid.AddForce(moveDir.normalized * speed, ForceMode.Force);
        if (!isMovingSound) 
        {
            if (moveInput.ReadValue<Vector2>().x != 0 || moveInput.ReadValue<Vector2>().y != 0)
            {
                StartCoroutine(MoveSound());
            }
        }
    }
    public void Jump()
    {
        if (isGround)
        {

        }

    }

    public void GroundCheck()
    {
        RaycastHit raycastHit;
        if (Physics.Raycast(transform.position, Vector3.down, out raycastHit, groundDis))
        {
            isGround = true;
            groundName = raycastHit.transform.name;
        }
        else isGround = false;
    }
    public void ShootAndAiming()
    {
        if (mouseLeftInput.ReadValue<float>() > 0)
        {

        }

        //1인칭 에임
        if (Input.enabled)
        {
            camCon.camType = mouseRightInput.ReadValue<float>() > 0 ? Defines.CamType.OnePerson : Defines.CamType.ThirdPerson;
        }
    }

    IEnumerator MoveSound()
    {
        isMovingSound = true;
        switch (groundName) //분리
        {
            case nameof(Defines.Terrain.Glass):
                soundController.PlayAudio("Footstep_Glass_Jog_1");
                break;
            case nameof(Defines.Terrain.Gravel):
                soundController.PlayAudio("Footstep_Gravel_Boots_Jog_1");
                break;
        }
        yield return new WaitForSeconds(1f);
        isMovingSound = false;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, Vector3.down * groundDis,Color.red);
    }
}
