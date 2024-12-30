using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillController : MonoBehaviour, IPointerDownHandler
{

    SkillManager _skillManager;
    public int thisNumber;

    // Start is called before the first frame update
    void Start()
    {
        _skillManager = transform.parent.GetComponent<SkillManager>();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("clicked");
        _skillManager.SeletedSlot(thisNumber);
    }

    
}
