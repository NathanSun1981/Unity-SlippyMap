using System.Collections;
using System.Collections.Generic;
using TouchScript;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapLockButton : MonoBehaviour, IPointerClickHandler
{
    // Start is called before the first frame update
    public Sprite[] sprites;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("Test").GetComponent<TestMap>().isMapRotateLocked)    
            this.GetComponent<Image>().sprite = sprites[1];      
        else
            this.GetComponent<Image>().sprite = sprites[0];
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            if (GameObject.Find("Test").GetComponent<TestMap>().isMapRotateLocked)
            {
                this.GetComponent<Image>().sprite = sprites[0];
                GameObject.Find("Test").GetComponent<TestMap>().isMapRotateLocked = false;
            }
            else
            {
                this.GetComponent<Image>().sprite = sprites[1];
                GameObject.Find("Test").GetComponent<TestMap>().isMapRotateLocked = true;
            }
        }
       
            
    }

}
