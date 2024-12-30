using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHandler : MonoBehaviour
{
    private GameObject player;
    private UnityEngine.InputSystem.PlayerInput Input;
    private InputAction interactInput;
    public Defines.Interacts interact = Defines.Interacts.None;

    private bool isInteract = false;


    private void Awake()
    {
        player = transform.parent.gameObject;
        Input = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
    }
    void Start()
    {
        interactInput = Input.actions["Interact"];
    }

    // Update is called once per frame
    void Update()
    {
        if (interactInput.triggered) isInteract = true;
    }

    private void OnTriggerStay(Collider other)
    {
        IInteracts currentinteract = other.gameObject.GetComponent<IInteracts>();

        if (currentinteract != null && isInteract)
        {
            interact = currentinteract.interactType;
            switch (interact)
            {
                case Defines.Interacts.SiegeWeapon:
                    OnControlSiegeWeapon(other.gameObject);
                    break;
            }
        }

    }

    public void OnControlSiegeWeapon(GameObject siegeweapon)
    {
        //첫번째 자식은 반드시 캐릭터를 고정시킬 위치가 되어야함
        player.transform.position = siegeweapon.gameObject.transform.GetChild(0).transform.position;
        Input.enabled = false;

        SiegeWeaponController siegeMode = siegeweapon.GetComponent<SiegeWeaponController>();
        siegeMode.OnComponents(player);

        isInteract = false;
    }
}
