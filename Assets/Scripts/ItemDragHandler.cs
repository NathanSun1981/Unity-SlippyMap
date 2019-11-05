using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnitySlippyMap.Map;
using UnitySlippyMap.Markers;
using UnityEngine.UI;


public class ItemDragHandler : MonoBehaviour, IDragHandler, IDropHandler
{
    private GameObject canvas;
    private GameObject container;
    private MapBehaviour Map;

    

    public void OnDrag(PointerEventData eventData)
    {

        canvas = GameObject.Find("Canvas");
        transform.position = Input.mousePosition;
        this.transform.SetParent(canvas.transform, false);

    }

    public void OnDrop(PointerEventData eventData)
    {
        RectTransform invPanel = GameObject.Find("Menu").GetComponent<RectTransform>();


        //check to see if the mouse pointer is out of the Menu box
        if (!RectTransformUtility.RectangleContainsScreenPoint(invPanel, Input.mousePosition))
        {
            //hide the Image of 3d model
            this.gameObject.SetActive(false);
            string fileName = this.name;

            //figure out where to drop the modelin world space.
            RaycastHit hit = new RaycastHit();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, 1000))
            {
                GameObject building = Resources.Load<GameObject>("SKPFiles/" + fileName);
                building.transform.position = hit.point;

                //add components
                var prefab = Instantiate(building, building.transform.position, Quaternion.identity);
                foreach (Transform tran in prefab.transform.GetComponentsInChildren<Transform>())
                {
                    tran.gameObject.tag = "CaseMeshCollider";
                }

                    //adjusting scale
                Vector3 scale = prefab.transform.localScale;
                scale.Set(0.0005f, 0.0005f/100, 0.0005f);
                prefab.transform.localScale = scale;
                prefab.AddComponent<CaseDragHandler>();

                //create building as marker

                double[] displacementMeters = new double[2] {
                            prefab.transform.position.x / Map.RoundedScaleMultiplier,
                            prefab.transform.position.z / Map.RoundedScaleMultiplier
                        };

                double[] centerMeters = new double[2] {
                            Map.CenterEPSG900913 [0],
                            Map.CenterEPSG900913 [1]
                        };
                centerMeters[0] += displacementMeters[0];
                centerMeters[1] += displacementMeters[1];

                double[] PostionWGS84 = Map.EPSG900913ToWGS84Transform.Transform(centerMeters);
                Map.CreateMarker<MarkerBehaviour>(fileName, PostionWGS84, prefab);


            }

        }


    }

    // Start is called before the first frame update
    void Start()
    {
        Map = GameObject.Find("Test").GetComponent<TestMap>().map;
    }

    // Update is called once per frame
    void Update()
    {
        
    }



 
}
