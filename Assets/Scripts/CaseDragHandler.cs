using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;
using Esri.APP;
using System.IO;
using Newtonsoft.Json.Linq;
using TouchScript;
using TablePlus.ElementsDB.DBBridge;
using TMPro;

public class CaseDragHandler : MonoBehaviour
{

    public enum ControlCommand { Rotate, Move, Zoom, Delete, Duplicate, information, None };
    public ControlCommand CaseControlCommand = ControlCommand.None;

    private DateTime m_datetime;
    private Vector3 initScale;


    private void OnEnable()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.PointersPressed += pointersPressedHandler;
        }
    }

    private void OnDisable()
    {
        if (TouchManager.Instance != null)
        {
            //press handler
            TouchManager.Instance.PointersPressed -= pointersPressedHandler;
        }

    }

    public void pointersPressedHandler(object sender, PointerEventArgs e)
    {
        Debug.Log("press case");

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.touches[i];

                //Debug.Log(i.ToString() + ":" + touch.phase.ToString());

                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {
                    GameObject gameObj = hitInfo.collider.gameObject;
                    try
                    {
                        // Debug.Log("hit" + hitInfo.collider.gameObject.name);

                        if (FindUpParent(gameObj.transform))
                        {
                            //generate button menu
                            m_datetime = DateTime.Now;
                            if (transform.Find("CaseControlMenu") == null)
                            {                             
                                GameObject menu = (GameObject)Instantiate(Resources.Load("Menu"));
                                menu.name = "CaseControlMenu";
                                menu.transform.SetParent(this.transform);
                                menu.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

                                menu.transform.localPosition = this.transform.localPosition + new Vector3(0f, 200f, 0f);
                                menu.transform.eulerAngles = new Vector3(90, 0, 0);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }

                }

            }
        }
        
    }

    private void Start()
    {
        m_datetime = DateTime.Now;
        initScale = transform.localScale;
    }

    void Update()
    {

        //DetectTouchAndGenerateMenu();
        
        DateTime dTimeNow = DateTime.Now;
        TimeSpan ts = dTimeNow.Subtract(m_datetime);
        float tsf = float.Parse(ts.TotalSeconds.ToString());
        if (tsf > 2)
        {
            if (transform.Find("CaseControlMenu") != null)
            {
                Destroy(transform.Find("CaseControlMenu").gameObject);
            }
        }       

        switch (CaseControlCommand)
        {
                case ControlCommand.Move:                   
                    FollowFingerMove();
                    break;
                case ControlCommand.Rotate:
                    FollowFingerRotate();
                    break;
                case ControlCommand.Zoom:
                    FollowFingerZoom();
                    break;
                case ControlCommand.Delete:
                    break;
                case ControlCommand.information:
                    break;
                default:
                    break;
        }

        
    }


    private DBCase GetCaseInfo(string name)
    {

        string filePath = Path.Combine(Application.streamingAssetsPath, "cases.json");

        if (File.Exists(filePath))
        {
            string dataAsJson;
            dataAsJson = File.ReadAllText(filePath);
            var cases = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(dataAsJson);
            foreach (JObject dbcase in cases)
            {
                if (dbcase.GetValue("_CaseNumber").ToString().Equals(name))
                {
                    return Elements.GetCaseByID(dbcase.GetValue("_CaseId").ToString());
                }
            }

        }
        else
        {
            Debug.LogError("Cannot load task data!");
        }

        return null;


    }

    /*
    private void DetectTouchAndGenerateMenu()
    {
        DateTime dTimeNow = DateTime.Now;
        TimeSpan ts = dTimeNow.Subtract(m_datetime);
        float tsf = float.Parse(ts.TotalSeconds.ToString());

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.touches[i];

                //Debug.Log(i.ToString() + ":" + touch.phase.ToString());

                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {               
                    GameObject gameObj = hitInfo.collider.gameObject;
                    try
                    {
                       // Debug.Log("hit" + hitInfo.collider.gameObject.name);

                        if (FindUpParent(gameObj.transform))
                        {
                            //generate button menu

                            RaycastHit hitTile;
                            LayerMask mask = 1 << LayerMask.NameToLayer("Tiles");
                            if (Physics.Raycast(ray, out hitTile, Mathf.Infinity, mask))
                            {
                                this.transform.position = hitTile.point;
                            }
                        }

                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                        
                 }
                   
            }
        }
        else if (tsf > 2)
        {
            if (transform.Find("CaseControlMenu") != null)
            {
                Destroy(transform.Find("CaseControlMenu").gameObject);
            }
        }
    }
    */

    public void CaseCommand(string cc)
    {
        switch (cc)
        {
            case "Move":
                GameObject.Find("Test").GetComponent<TestMap>().isMapRotateLocked = true;
                CaseControlCommand = ControlCommand.Move;
                break;
            case "Rotate":
                GameObject.Find("Test").GetComponent<TestMap>().isMapRotateLocked = true;
                CaseControlCommand = ControlCommand.Rotate;
                break;
            case "Zoom":
                GameObject.Find("Test").GetComponent<TestMap>().isMapRotateLocked = true;
                CaseControlCommand = ControlCommand.Zoom;
                break;
            case "Delete":
                CaseControlCommand = ControlCommand.Delete;
                Destroy(this.transform.parent.gameObject);
                break;
            case "Info":
                DBCase dbcase = GetCaseInfo(this.transform.parent.gameObject.name);

                //get needed properties
                string name, caseNumber, caption, caseID;
                dbcase.Properties.TryGetValue("case_name", out name);
                dbcase.Properties.TryGetValue("case_number", out caseNumber);
                dbcase.Properties.TryGetValue("caption", out caption);
                dbcase.Properties.TryGetValue("case_id", out caseID);

                string far,uph, hect;
                dbcase.Properties.TryGetValue("cal_FAR", out far);
                dbcase.Properties.TryGetValue("cal_UPH", out uph);
                dbcase.Properties.TryGetValue("par_site_area", out hect);
                hect = (float.Parse(hect) / 10000).ToString();


                GameObject.Find("Test").GetComponent<TestMap>().isMapRotateLocked = true;
                if (transform.Find("ScrollInfoControl") == null)
                {
                    GameObject scrollInfoControl = (GameObject)Instantiate(Resources.Load("ScrollInfoControl"));
                    scrollInfoControl.transform.SetParent(this.transform);
                    scrollInfoControl.transform.name = "ScrollInfoControl";
                    RectTransform rt = scrollInfoControl.GetComponent<RectTransform>();
                    rt.localPosition = new Vector3(0, 0, 0);
                    rt.offsetMax = new Vector2(0, 200f);
                    rt.offsetMin = new Vector2(150f, 0);

                    scrollInfoControl.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                    scrollInfoControl.transform.eulerAngles = transform.Find("CaseControlMenu").transform.eulerAngles;

                    //scrollInfoControl.transform.Find("Title").Find("Name").GetComponent<TextMeshProUGUI>().SetText(name);
                    scrollInfoControl.transform.Find("Title").Find("Name").GetComponent<Text>().text = name;
                    scrollInfoControl.transform.Find("Title").Find("CaseNumber").GetComponent<Text>().text = caseNumber;
                    scrollInfoControl.transform.Find("Title").Find("Caption").GetComponent<Text>().text = caption;

                    scrollInfoControl.transform.Find("GeneralDescribe").Find("FloorAreaRatioValue").GetComponent<Text>().text = far + "FAR";
                    scrollInfoControl.transform.Find("GeneralDescribe").Find("HousingDensityValue").GetComponent<Text>().text = uph + "UPH";
                    scrollInfoControl.transform.Find("GeneralDescribe").Find("SiteAreaValue").GetComponent<Text>().text = hect + " hect";

                    scrollInfoControl.transform.Find("3DModelView").GetComponent<Image>().sprite = Resources.Load<Sprite>("Buttons/"+ caseNumber);


                    string UniqueCaseNumber, LandUse = "";
                    string Jobs, Population;
                    string FloorAreaRatio, DwellingDensity, SiteArea, SiteWidth, SiteDepth, EffectiveImpervious, BoundingBoxWidth, BoundingBoxDepth;
                    string MaxFloors, TotalConditionedArea, ResidentialArea, CommercialArea, CivicArea, IndustrialArea, OtherArea, ParkingArea;
                    string Dwellings, Bedrooms, Bathrooms, CommercialUnits;

                    UniqueCaseNumber = caseNumber;

                    foreach(DBLanduse dblanduse in dbcase.LanduseList)
                    {
                        LandUse += dblanduse._Type + "|";
                    }           

                    dbcase.Properties.TryGetValue("cal_FAR", out FloorAreaRatio);
                    dbcase.Properties.TryGetValue("cal_UPH", out DwellingDensity);
                    dbcase.Properties.TryGetValue("par_site_area", out SiteArea);
                    dbcase.Properties.TryGetValue("par_avg_site_width", out SiteWidth);
                    dbcase.Properties.TryGetValue("par_avg_site_depth", out SiteDepth);
                    dbcase.Properties.TryGetValue("cal_s_site_effective_impervious", out EffectiveImpervious);

                    BoundingBoxWidth = this.transform.localScale.x.ToString();
                    BoundingBoxDepth = this.transform.localScale.y.ToString();


                    dbcase.Properties.TryGetValue("cal_T_f_max_occupied_stories", out MaxFloors);
                    dbcase.Properties.TryGetValue("cal_T_f_total_cond_residential", out ResidentialArea);
                    dbcase.Properties.TryGetValue("cal_T_f_total_cond_commercial", out CommercialArea);
                    dbcase.Properties.TryGetValue("cal_T_f_total_cond_civic", out CivicArea);
                    dbcase.Properties.TryGetValue("cal_T_f_total_cond_industrial", out IndustrialArea);
                    dbcase.Properties.TryGetValue("cal_T_f_total_cond_other", out OtherArea);
                    dbcase.Properties.TryGetValue("cal_T_f_structured_parking", out ParkingArea);
                    TotalConditionedArea= (double.Parse((CivicArea == null) ? "0" : CivicArea) + double.Parse((CommercialArea == null) ? "0" : CommercialArea)
                        + double.Parse((IndustrialArea == null) ? "0" : IndustrialArea) + double.Parse((OtherArea == null) ? "0" : OtherArea) + double.Parse((ResidentialArea == null) ? "0" : ResidentialArea)).ToString();

                    //(int)((double)CaseFacts[CaseFactKeys.CommercialArea]/33
                    Jobs = ((int)(double.Parse((CommercialArea == null) ? "0" : CommercialArea) / 33)).ToString();
                    //(int)((double)CaseFacts[CaseFactKeys.ResidentialArea]/56)
                    Population = ((int)(double.Parse((ResidentialArea == null) ? "0" : ResidentialArea) / 56)).ToString();


                    dbcase.Properties.TryGetValue("cal_T_total_residential_units", out Dwellings);
                    dbcase.Properties.TryGetValue("cal_T_u_residential_bedrooms", out Bedrooms);
                    dbcase.Properties.TryGetValue("cal_T_u_residential_bathrooms", out Bathrooms);
                    dbcase.Properties.TryGetValue("cal_T_total_commercial_units", out CommercialUnits);


                    //Load case attributes
                    Transform content = scrollInfoControl.transform.Find("CaseAttributes").Find("Content");

                    content.Find("Case Number Value").GetComponent<Text>().text = (UniqueCaseNumber == null) ? "0" : UniqueCaseNumber;
                    content.Find("Land Use Value").GetComponent<Text>().text = (LandUse == null) ? "0" : LandUse;

                    content.Find("Jobs Value").GetComponent<Text>().text = (Jobs == null) ? "0" : Jobs; ;
                    content.Find("Population Value").GetComponent<Text>().text = (Population == null) ? "0" : Population;

                    content.Find("Floor Area Ratio Value").GetComponent<Text>().text = (FloorAreaRatio == null) ? "0" : FloorAreaRatio;
                    content.Find("Dewlling Density Value").GetComponent<Text>().text = (DwellingDensity == null) ? "0" : DwellingDensity;
                    content.Find("Site Area Value").GetComponent<Text>().text = (SiteArea == null) ? "0" : SiteArea;
                    content.Find("Site Width Value").GetComponent<Text>().text = (SiteWidth == null) ? "0" : SiteWidth;
                    content.Find("Site Depth Value").GetComponent<Text>().text = (SiteDepth == null) ? "0" : SiteDepth;
                    content.Find("Effective Impervious Value").GetComponent<Text>().text = (EffectiveImpervious == null) ? "0" : EffectiveImpervious;
                    content.Find("Bounding Box Width Value").GetComponent<Text>().text = (BoundingBoxWidth == null) ? "0" : BoundingBoxWidth;
                    content.Find("Bounding Box Depth Value").GetComponent<Text>().text = (BoundingBoxDepth == null) ? "0" : BoundingBoxDepth;

                    content.Find("Max Floors Value").GetComponent<Text>().text = (MaxFloors == null) ? "0" : MaxFloors;
                    content.Find("Total Conditioned Area Value").GetComponent<Text>().text = (TotalConditionedArea == null) ? "0" : TotalConditionedArea;
                    content.Find("Residential Area Value").GetComponent<Text>().text = (ResidentialArea == null) ? "0" : ResidentialArea;
                    content.Find("Commercial Area Value").GetComponent<Text>().text = (CommercialArea == null) ? "0" : CommercialArea;
                    content.Find("Civic Area Value").GetComponent<Text>().text = (CivicArea == null) ? "0" : CivicArea;
                    content.Find("Industrial Area Value").GetComponent<Text>().text = (IndustrialArea == null) ? "0" : IndustrialArea;
                    content.Find("Other Area Value").GetComponent<Text>().text = (OtherArea == null) ? "0" : OtherArea;
                    content.Find("Parking Area Value").GetComponent<Text>().text = (ParkingArea == null) ? "0" : ParkingArea;

                    content.Find("Dwellings Value").GetComponent<Text>().text = (Dwellings == null) ? "0" : Dwellings;
                    content.Find("Bedrooms Value").GetComponent<Text>().text = (Bedrooms == null) ? "0" : Bedrooms;
                    content.Find("Bathrooms Value").GetComponent<Text>().text = (Bathrooms == null) ? "0" : Bathrooms;
                    content.Find("Commercial Units Value").GetComponent<Text>().text = (CommercialUnits == null) ? "0" : CommercialUnits;



                    foreach (DBCaseResource dbcaseresource in dbcase.ResourceList)
                    {
                        if (dbcaseresource._Extension != "skp")
                            scrollInfoControl.transform.Find("Viewport").Find("Content").gameObject.GetComponent<LoadAdditionalImages>().loadAdditionalImage(dbcaseresource);
                    }

                }
                //CaseControlCommand = ControlCommand.information;
                break;
            default:
                CaseControlCommand = ControlCommand.None;
                break;

        }
    }

    public void FollowFingerMove()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.touches[i];

                //Debug.Log(i.ToString() + ":" + touch.phase.ToString());

                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {
                    GameObject gameObj = hitInfo.collider.gameObject;
                    try
                    {
                        // Debug.Log("hit" + hitInfo.collider.gameObject.name);

                        if (FindUpParent(gameObj.transform))
                        {
                            //generate button menu

                            RaycastHit hitTile;
                            LayerMask mask = 1 << LayerMask.NameToLayer("Tiles");
                            if (Physics.Raycast(ray, out hitTile, Mathf.Infinity, mask))
                            {
                                this.transform.position = hitTile.point;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }

                }

            }
        }
    }

    public void FollowFingerRotate()
    {
        if (DetectClosestFingerTouch(2))
            this.gameObject.transform.RotateAround(Vector3.up, -DetectClosestTouchMovement.turnAngleDelta * 0.5f * Time.deltaTime);
    }

    public void FollowFingerZoom()
    {
        if (DetectClosestFingerTouch(2))
        {
            //Debug.Log(1 + DetectClosestTouchMovement.pinchAmount / 50);
            this.gameObject.transform.localScale *= (1 + DetectClosestTouchMovement.pinchAmount / 50);
            this.gameObject.transform.localScale = new Vector3(Mathf.Clamp(this.gameObject.transform.localScale.x, initScale.x / 1.5f, initScale.x * 1.5f),
                 Mathf.Clamp(this.gameObject.transform.localScale.y, initScale.y / 1.5f, initScale.y * 1.5f),
                 Mathf.Clamp(this.gameObject.transform.localScale.z, initScale.z / 1.5f, initScale.z * 1.5f));

        }
    }

    public bool DetectClosestFingerTouch(int FingerNum)
    {
        //DetectClosestFingerTouch in a certain distance
        Touch[] allTouches = Input.touches;
        Touch[] closestTouchs = new Touch[FingerNum];

        if (Input.touchCount >= FingerNum)
        {
            float[] distance = new float[Input.touchCount];
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.touches[i];

                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {
                    Vector2 hitVector2 = new Vector2(hitInfo.point.x, hitInfo.point.z);
                    Vector2 caseVector2 = new Vector2(transform.position.x, transform.position.z);
                    distance[i] = Vector2.Distance(caseVector2, hitVector2);
                }

            }

            if (FingerNum == 2)
            {
                int[] indexs = new int[FingerNum];
                float[] mindistance = FindMinumFromArray(distance, FingerNum, out indexs);
                for (int i = 0; i < FingerNum; i++)
                {
                    //Debug.Log(mindistance[i]);
                    if (mindistance[i] > 0.1f)
                    {
                        return false;
                    }

                    closestTouchs[i] = Input.touches[indexs[i]];
                }

                DetectClosestTouchMovement.Calculate(closestTouchs);
                return true;
            }
        }
        return false;
    }

    public float[] FindMinumFromArray(float[] ins, int k, out int[] indexsofClosestTouches)
    {
        float[] ks = new float[k];
        indexsofClosestTouches = new int[k];

        if (ins.Length < k)
        {
            for (int i = 0; i < ins.Length; i++)
            {
                indexsofClosestTouches[i] = i;
            }
            return ins;
        }


        for (int i = 0; i < k; i++)
        {
            ks[i] = ins[i];
            indexsofClosestTouches[i] = i;
        }
        for (int i = k; i < ins.Length; i++)
        {
            if (getMax(ks, ref indexsofClosestTouches) > ins[i])
            {
                ks[0] = ins[i];
                indexsofClosestTouches[0] = i;
            }
        }
        return ks;
    }
    public static float getMax(float[] arr, ref int[] indexs)
    {
        int radix = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[radix] < arr[i])
            {
                float temp = arr[radix];
                arr[radix] = arr[i];
                arr[i] = temp;

                int tempindex = indexs[radix];
                indexs[radix] = indexs[i];
                indexs[i] = tempindex;
            }
        }
        return arr[radix];
    }

    bool FindUpParent(Transform zi)
    {
        if (zi.gameObject.name == this.transform.gameObject.name)
            return true;
        else if (zi.parent == null)
            return false;
        else
            return FindUpParent(zi.parent);
    }

}
