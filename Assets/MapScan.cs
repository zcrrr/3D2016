#define STANDALONE
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


public class MapScan : MonoBehaviour {
//	public Font myfont;
	private float minPinchDistance = 10.0f;
	private float angleRangeOfRotate = 100;
	private float minAngle = 45f;
	private float maxAngle = 89f;
	private float pinchDistanceDelta = 0.0f;
	private bool isDistanceChangeHuge = false;
	private bool isRotateBack = false;
	private bool hasRotated = false;
	private bool hasUpDown = false;
	private bool isRotate = false;
	Camera myCamera;
	GoogleProjection gp = new GoogleProjection (); 
	private float myLocationLon = Main.myLocationLon; 
	private float myLocationLat = Main.myLocationLat;
	private float heading = Main.heading;
	string currentGesture = "";//begin,zoom,rotate,updown
	ArrayList gestureList = new ArrayList();  
	Vector2 touchBefore;
	Vector2 touch0before;
	Vector2 touch1before;
	
	ArrayList list_display = new ArrayList();//当前正在展示的数据
	ArrayList list_datasource = new ArrayList();//接收的数据源
	ArrayList texture_big = new ArrayList ();
	ArrayList texture_small = new ArrayList ();
	ArrayList texture_image = new ArrayList ();
	Texture2D texture_myLoc;
	Vector3 targetPosition;
	bool startMove = false;
	bool wantWatch = false;
	bool startWatch = false;
	Vector3 aroundPosition;
	float step = 5;
	KeyAndValue kav = new KeyAndValue ();
	int selectedType = 0;
	PoiClass lastSelectedPoi;
	bool isDraging;
	Texture2D texture_howToPlayBottom;
	Texture2D texture_howToPlayTop;
	public static float label_high = 36f;
	public static float label_stroke_width = 1f;
	//动态规避标注
	ArrayList listPoisAlreadyInScreen = new ArrayList();
	//道路分级显示
	GameObject road2;
	GameObject road3;
	GameObject road4;
	GameObject road6;
	GameObject road8;
	GameObject road10;
	GameObject road11;

	float pcToPhoneScaleImage = 2f;//放到手机运行之前，把这个值改成合适的倍数，使得手机上和电脑上显示的一样
	float pcToPhoneScaleText = 1f;
	float stepInt = 1f;
	float stepIntSmall = 0.1f;
	int[] arr = new int[]{0,1,2};
	ArrayList showLabelPoiTypes = new ArrayList();


	bool canRotate = false;
	bool canUpAndDown = false;
	string testLog = "testlog";

	void setPcToPhoneScaleByPhoneType(){
		print ("screen.width is " + Screen.width);
		//独立运行
		//iphone6p : 1080
		//iphone5 : 640
		//集成进入app
		//iphone6p : 921
		//iphone5:640
		if (Screen.width == 921) {
			pcToPhoneScaleImage = 2.3f;
		} else if (Screen.width == 640) {
			pcToPhoneScaleImage = 1.4f;
		} else if (Screen.width == 1080) {
			pcToPhoneScaleImage = 2.5f;
		}
	}
	
	// Use this for initialization
	void Start () {
		//test
//		Main.initById (43);
		zywx_setPoiDateSource (Main.testdataAll);



//		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
//		plane.transform.position = new Vector3 (0,0.031f,0);
//		Material yourMaterial = (Material)Resources.Load("moutain", typeof(Material));
//		plane.GetComponent<Renderer> ().sharedMaterial = yourMaterial;
//		Main.initById ();

		string[] temp1 = Main.appear_camera.Split (new char[] { ',' });
		string[] temp11 = temp1[0].Split(new char[] { '_' });
		string[] temp12 = temp1[1].Split(new char[] { '_' });
		float appear_position_x = float.Parse(temp11 [0]);
		float appear_position_y = float.Parse(temp11 [1]);
		float appear_position_z = float.Parse(temp11 [2]);

		float appear_rotation_x = float.Parse(temp12 [0]);
		float appear_rotation_y = float.Parse(temp12 [1]);
		float appear_rotation_z = float.Parse(temp12 [2]);

		transform.position = new Vector3 (appear_position_x,appear_position_y,appear_position_z);
		transform.eulerAngles = new Vector3 (appear_rotation_x,appear_rotation_y,appear_rotation_z);

//		print ("appear_position_x:"+appear_position_x+"  appear_position_y:"+appear_position_y+"  appear_position_z:"+appear_position_z);
//		print ("appear_rotation_x:"+appear_rotation_x+"  appear_rotation_x:"+appear_rotation_y+"  appear_rotation_z:"+appear_rotation_z);

			

		foreach (object obj in arr) {
			showLabelPoiTypes.Add (obj);
		}
		LonLatPoint lonlatpoint = new LonLatPoint(116.351856f,39.930587f);
		LonLatPoint lonlatMercator = gp.lonlatToMercator (lonlatpoint, 17);
		print("---------------------lonlatMercator is "+(long)lonlatMercator.lon+"  "+(long)lonlatMercator.lat);	
		setPcToPhoneScaleByPhoneType ();
#if UNITY_EDITOR
		pcToPhoneScaleImage = 1;
#endif
		road2 = GameObject.Find ("R2");
		road3 = GameObject.Find ("R3");
		road4 = GameObject.Find ("R4");
		road6 = GameObject.Find ("R6");
		road8 = GameObject.Find ("R8");
		road10 = GameObject.Find ("R10");
		road11 = GameObject.Find ("R11");
		myCamera = GetComponent<Camera>();

		for (int i = 1; i<=24; i++) {
			Texture2D image_big = (Texture2D)Resources.Load ("map_"+i+"@3x");
			GUIContent content_big = new GUIContent ();
			content_big.image = image_big;
			texture_big.Add(content_big);
			Texture2D image_small = (Texture2D)Resources.Load ("map_icon_"+i);
			GUIContent content_small = new GUIContent ();
			content_small.image = image_small;
			texture_small.Add(content_small);
		}
		texture_howToPlayBottom = (Texture2D)Resources.Load ("map_11@3x");
		texture_howToPlayTop = (Texture2D)Resources.Load ("position_pop");
		for (int i=4; i<=Main.ImageCount; i++) {
			Texture2D image = (Texture2D)Resources.Load ("1.1-" + i);
			texture_image.Add (image);
		}
		texture_myLoc = (Texture2D)Resources.Load ("mylocation");
	}



	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.X)) {
			transform.Translate (Vector3.forward * stepInt);
		}
		if (Input.GetKeyUp (KeyCode.Z)) {
			transform.Translate (-Vector3.forward * stepInt);
		}
		if (Input.GetKeyUp (KeyCode.LeftArrow)) {
			transform.Translate (Vector3.left * stepInt);
		}
		if (Input.GetKeyUp (KeyCode.RightArrow)) {
			transform.Translate (-Vector3.left * stepInt);
		}
		if (Input.GetKeyUp (KeyCode.UpArrow)) {
			transform.Translate (Vector3.up * stepInt);
		}
		if (Input.GetKeyUp (KeyCode.DownArrow)) {
			transform.Translate (-Vector3.up * stepInt);
		}
		if (Input.GetKeyUp (KeyCode.A)) {
			transform.Translate (Vector3.left * stepIntSmall);
		}
		if (Input.GetKeyUp (KeyCode.D)) {
			transform.Translate (-Vector3.left * stepIntSmall);
		}
		if (Input.GetKeyUp (KeyCode.W)) {
			transform.Translate (Vector3.up * stepIntSmall);
		}
		if (Input.GetKeyUp (KeyCode.S)) {
			transform.Translate (-Vector3.up * stepIntSmall);
		}
		//相机正在移动
		if (startMove) {
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, step*Time.deltaTime);
			if(transform.position.Equals(targetPosition)){
				startMove = false;
				if(wantWatch){//说明移动过去是为了观看
					startWatch = true;
					wantWatch = false;
				}
			}
		}
		//相机正在旋转
		if (startWatch) {
			float angleBefore = transform.eulerAngles.y;
			transform.RotateAround (aroundPosition,new Vector3(0,1,0) , 30 * Time.deltaTime);
			float angleAfter = transform.eulerAngles.y;
			if(angleBefore < 180 && angleAfter > 180){
				startWatch = false;
			}
		}

		if (isRotateBack) {//
			Vector3 centerPoint = new Vector3(Screen.width/2,Screen.height/2,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x)));
			float angleBefore = transform.eulerAngles.y;
			float rotateSpeed; 
			if(angleBefore > 180){
				rotateSpeed = -50.0f*Time.deltaTime;
			}else{
				rotateSpeed = 50.0f*Time.deltaTime;
			}
			transform.RotateAround (myCamera.ScreenToWorldPoint(centerPoint), new Vector3(0,1,0), rotateSpeed);
			float angleAfter = transform.eulerAngles.y;
			if(angleBefore <= 180 && angleAfter > 180){
				isRotateBack = false;
				hasRotated = false;
			}
		}
		if (Input.touchCount == 1) {
			if (Input.GetTouch (0).phase == TouchPhase.Began) {//屏幕点击事件
				touchBefore = Input.GetTouch (0).position;
				currentGesture = "";
				startMove = false;
				startWatch = false;
				wantWatch = false;
			}else if (Input.GetTouch (0).phase == TouchPhase.Moved) {
				isDraging = true;
				if(!currentGesture.Equals("")){
					return;
				}
				Vector2 touchAfter = Input.GetTouch (0).position;
				Vector2 touchDeltaPosition = touchAfter - touchBefore;
				float lengthScreen = touchDeltaPosition.magnitude;
				if (lengthScreen <= 0) {
					isDraging = false;
					return;
				}
				Vector3 touchAfterToWorld = myCamera.ScreenToWorldPoint(new Vector3(touchAfter.x,touchAfter.y,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x))));
				Vector3 touchBeforeToWorld = myCamera.ScreenToWorldPoint(new Vector3(touchBefore.x,touchBefore.y,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x))));
				Vector3 deltaWorld = touchAfterToWorld - touchBeforeToWorld;
				float lengthWorld = deltaWorld.magnitude;
				float scaleFromSceenToWorld = lengthWorld/lengthScreen;
				float y_weight = -touchDeltaPosition.y*Mathf.Sin(DegreetoRadians(transform.eulerAngles.x));
				float z_weight = -touchDeltaPosition.y*Mathf.Cos(DegreetoRadians(transform.eulerAngles.x));
				transform.Translate (-touchDeltaPosition.x * scaleFromSceenToWorld, y_weight * scaleFromSceenToWorld, z_weight*scaleFromSceenToWorld );
				touchBefore = touchAfter;
			}else if (Input.GetTouch (0).phase == TouchPhase.Ended) {//屏幕点击事件
				isDraging = false;
			}
		} else if (Input.touchCount == 2) {
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			if(touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began){
				touch0before = Input.GetTouch(0).position;
				touch1before = Input.GetTouch(1).position;
				currentGesture = "begin";
				gestureList.Clear();
			}else if (touchZero.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Ended) {
				isDraging = false;
			}
			else if (touchZero.phase == TouchPhase.Moved && touchOne.phase == TouchPhase.Moved) {
				isDraging = true;
				//判断之前和之后，两点间的距离的变化是否是巨大的
				float pinchDistance = Vector2.Distance(touchZero.position, touchOne.position);
				float prevDistance = Vector2.Distance(touch0before,touch1before);
				pinchDistanceDelta = pinchDistance - prevDistance;
				if (Mathf.Abs(pinchDistanceDelta) > minPinchDistance) {
					isDistanceChangeHuge = true;
				}else{
					isDistanceChangeHuge = false;
				}
				Vector2 vectorbefore01 = new Vector2(touch1before.x-touch0before.x,touch1before.y-touch0before.y);
				float angleZero = VectorAngle(vectorbefore01, touchZero.position - touch0before);
				Vector2 vectorbefore10 = new Vector2(touch0before.x-touch1before.x,touch0before.y-touch1before.y);
				float angleOne = VectorAngle(vectorbefore10, touchOne.position - touch1before);
				if(angleZero * angleOne > 0 && Mathf.Abs(angleZero) > 90-angleRangeOfRotate/2 && Mathf.Abs(angleZero) < 90+angleRangeOfRotate/2 && Mathf.Abs(angleOne) > 90-angleRangeOfRotate/2 && Mathf.Abs(angleOne) < 90+angleRangeOfRotate/2){
					isRotate = true;
				}else{
					isRotate = false;
				}
				if(isRotate){
					if(currentGesture.Equals("begin")){
						gestureList.Add("rotate");
						if(isContinuousSameGesture("rotate")){//连续三个rotate
							currentGesture = "rotate";
						}else{
							touch0before = touchZero.position;
							touch1before = touchOne.position;
							return;
						}
					}else{
						if(!currentGesture.Equals("rotate")){//zoom or updown
							touch0before = touchZero.position;
							touch1before = touchOne.position;
							return;
						}
					}
					Vector2 vectorAfter01 = new Vector2(touchOne.position.x-touchZero.position.x,touchOne.position.y-touchZero.position.y);
					float rotateAngle = VectorAngle(vectorbefore01, vectorAfter01);
					Vector3 centerPoint = new Vector3((touch0before.x+touch1before.x)/2,(touch0before.y+touch1before.y)/2,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x)));
					if (canRotate) {
						transform.RotateAround (myCamera.ScreenToWorldPoint(centerPoint),new Vector3(0,1,0) , -rotateAngle);
					}
					hasRotated = true;
				}else{
					if(isDistanceChangeHuge){
						if(currentGesture.Equals("begin")){
							gestureList.Add("zoom");
							if(isContinuousSameGesture("zoom")){//连续三个zoom
								currentGesture = "zoom";
							}else{
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}else{
							if(!currentGesture.Equals("zoom")){//rotate or updown
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}
						float scaleOfView = pinchDistance/prevDistance;//视野放大了几倍
						Vector2 touchZeroPrevPos = touchZero.position - (touchZero.position - touch0before);
						Vector2 touchOnePrevPos = touchOne.position - (touchOne.position - touch1before);
						float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
						float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
						
						float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
						if(deltaMagnitudeDiff > 0){//zoom out
							if(transform.position.y > Main.maxHigh){
								print("too high");
								hasUpDown = false;
								touch0before = touchZero.position;
								touch1before = touchOne.position;
#if STANDALONE
								if (Main.platform.Equals ("ios")) {
									_unityCallIOS("back");
								}else{
									unityCallAndroid("unityCallAndroid","back");
								}
#endif

								return;
							} 
						}else{//zoom in
							if(transform.position.y < Main.minHigh){
								print("too low");
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}

						float h1 = transform.position.y;
						float afterh = h1 * scaleOfView;
						float h3 = Mathf.Abs(afterh - h1);
						float forwardDis = h3/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x));
						if(deltaMagnitudeDiff > 0){//zoom out
							transform.Translate(-Vector3.forward*forwardDis);
						}else{
							transform.Translate(Vector3.forward*forwardDis);
						}


						if(transform.position.y > Main.fitHigh*9/10){
							if(h1 <= Main.fitHigh*9/10){//转回来
								startRotateBack();
								hasUpDown = false;
							}
						}else{
						}
						if (canRotate&&canUpAndDown) {
							float h2 = transform.position.y;
							float angle1 = 90.0f-transform.eulerAngles.x;
							if(deltaMagnitudeDiff > 0){//zoom out
								if(transform.eulerAngles.x < getMaxAngleByHeight()){
									Vector3 cameraLeftWorldVector = transform.TransformDirection (Vector3.left);
									float anglecorret = getMaxAngleByHeight()-transform.eulerAngles.x;
									transform.RotateAround (transform.position, cameraLeftWorldVector,-anglecorret);
									float dis1 = Mathf.Tan(DegreetoRadians(angle1))*h2;
									float dis2 = Mathf.Tan(DegreetoRadians(angle1-anglecorret))*h2;
									float dis = dis1 - dis2;
									float y_weight = dis*Mathf.Sin(DegreetoRadians(transform.eulerAngles.x));
									float z_weight = dis*Mathf.Cos(DegreetoRadians(transform.eulerAngles.x));
									transform.Translate (0, y_weight, z_weight );
								}
							}else{//zoom in
								if(!hasUpDown){
									Vector3 cameraLeftWorldVector = transform.TransformDirection (Vector3.left);
									float anglecorret = transform.eulerAngles.x - getMaxAngleByHeight();
									transform.RotateAround (transform.position, cameraLeftWorldVector,anglecorret);
									float dis1 = Mathf.Tan(DegreetoRadians(angle1))*h2;
									float dis2 = Mathf.Tan(DegreetoRadians(angle1+anglecorret))*h2;
									float dis = dis2 - dis1;
									float y_weight = dis*Mathf.Sin(DegreetoRadians(transform.eulerAngles.x));
									float z_weight = dis*Mathf.Cos(DegreetoRadians(transform.eulerAngles.x));
									transform.Translate (0, -y_weight, -z_weight );
								}
							}
						}
					}else{
						if(currentGesture.Equals("begin")){
							gestureList.Add("updown");
							if(isContinuousSameGesture("updown")){//连续三个updown
								currentGesture = "updown";
							}else{
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}else{
							if(!currentGesture.Equals("updown")){//zoom or rotate
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}
						float angle = (touchZero.position - touch0before).y*90.0f / Screen.height;
						if(angle > 0){//look up
							if(transform.eulerAngles.x < getMaxAngleByHeight()){
								print("can not look up anymore");
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}else{
							if(transform.eulerAngles.x > maxAngle){
								print("can not look down anymore");
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}
						if(transform.eulerAngles.x - angle > maxAngle){//防止倒过来看
							angle = transform.eulerAngles.x - maxAngle;
						}
						Vector3 cameraLeftWorldVector = transform.TransformDirection (Vector3.left);
						Vector3 centerPoint = new Vector3(Screen.width/2,Screen.height/2,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x)));
						if (canUpAndDown) {
							transform.RotateAround (myCamera.ScreenToWorldPoint(centerPoint), cameraLeftWorldVector, angle);
						}

						hasUpDown = true;
					}
				}
				touch0before = touchZero.position;
				touch1before = touchOne.position;
			}
		}
		float fitHighDivide4 = Main.fitHigh / 4;
		if (transform.position.y > fitHighDivide4*3) {
			if (road2 != null) {
				road2.SetActive(false);
			}
			if (road3 != null) {
				road3.SetActive(false);
			}
			if (road4 != null) {
				road4.SetActive(false);
			}
			if (road6 != null) {
				road6.SetActive(false);
			}
			if (road8 != null) {
				road8.SetActive(false);
			}
			if (road10 != null) {
				road10.SetActive(false);
			}
			if (road11 != null) {
				road11.SetActive(false);
			}

		} else if (transform.position.y > fitHighDivide4*2){
			if (road2 != null) {
				road2.SetActive(true);
			}
			if (road3 != null) {
				road3.SetActive(true);
			}
			if (road4 != null) {
				road4.SetActive(true);
			}
			if (road6 != null) {
				road6.SetActive(false);
			}
			if (road8 != null) {
				road8.SetActive(false);
			}
			if (road10 != null) {
				road10.SetActive(false);
			}
			if (road11 != null) {
				road11.SetActive(false);
			}
		}else if (transform.position.y > fitHighDivide4){
			if (road2 != null) {
				road2.SetActive(true);
			}
			if (road3 != null) {
				road3.SetActive(true);
			}
			if (road4 != null) {
				road4.SetActive(true);
			}
			if (road6 != null) {
				road6.SetActive(true);
			}
			if (road8 != null) {
				road8.SetActive(true);
			}
			if (road10 != null) {
				road10.SetActive(false);
			}
			if (road11 != null) {
				road11.SetActive(false);
			}
		}else{
			if (road2 != null) {
				road2.SetActive(true);
			}
			if (road3 != null) {
				road3.SetActive(true);
			}
			if (road4 != null) {
				road4.SetActive(true);
			}
			if (road6 != null) {
				road6.SetActive(true);
			}
			if (road8 != null) {
				road8.SetActive(true);
			}
			if (road10 != null) {
				road10.SetActive(true);
			}
			if (road11 != null) {
				road11.SetActive(true);
			}
		}
	}
	float VectorAngle(Vector2 from, Vector2 to)
	{
		float angle;
		Vector3 cross=Vector3.Cross(from, to);
		angle = Vector2.Angle(from, to);
		return cross.z > 0 ? -angle : angle;
	}
	float getMaxAngleByHeight(){
		float height = transform.position.y;
		if (height <= Main.minHigh*3) {
			return minAngle;
		} else if (height < Main.fitHigh) {
			return minAngle + (maxAngle-minAngle)/(Main.fitHigh-Main.minHigh*3)*(height-Main.minHigh*3);
		} else {
			return maxAngle;
		}
	}

	void startRotateBack(){
		if(hasRotated){
			isRotateBack = true;
		}
	}
	float DegreetoRadians(float x)
	{
		return x * 0.017453292519943295769f;
	}
	bool isPoiTypeHasLabel(int type){
		return showLabelPoiTypes.Contains (type);
	}
	void OnGUI () { 
		#if UNITY_EDITOR
		if(GUI.Button(new Rect(10,10,100,50),"test"))
		{
			zywx_listenOnePoi("109.501732|18.225855|观海亭");
		}
		#endif
//		GUI.Label (new Rect (Screen.width - 400, 10, 400, 50), ""+myCamera.transform.position.x.ToString("f2")+"_"+myCamera.transform.position.y.ToString("f2")+"_"+myCamera.transform.position.z.ToString("f2"));
//		GUI.Label (new Rect (Screen.width - 400, 70, 400, 50), ""+myCamera.transform.eulerAngles.x.ToString("f2")+"_"+myCamera.transform.eulerAngles.y.ToString("f2")+"_"+myCamera.transform.eulerAngles.z.ToString("f2"));
//		GUI.Label (new Rect (10, 10, Screen.width, 50),testLog);
		GUI.backgroundColor = Color.clear;
		GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
		centeredStyle.alignment = TextAnchor.UpperCenter;
		centeredStyle.fontSize = 29;
//		centeredStyle.font = myfont;
		centeredStyle.normal.textColor = Color.black;

		if (Main.imgString.Length > 0) {
			string[] images = Main.imgString.Split(new char[] { ';' });
			for (int i=0; i<images.Length; i++) {
				string[] image = images[i].Split(new char[] { ',' });
				float x = float.Parse(image[0]);
				float y = float.Parse(image[1]);
				float z = float.Parse(image[2]);
				int index = int.Parse(image[3]);
				float selfscale = float.Parse(image[4]);
				Vector3 screenpos = myCamera.WorldToScreenPoint(new Vector3 (x,y,z));
				float poiDistanceFromCamera = Vector3.Distance(new Vector3 (x,y,z),transform.position);
				Texture2D texture = (Texture2D)texture_image[index];
				
				float width = texture.width/poiDistanceFromCamera*pcToPhoneScaleImage*selfscale;
				float height = texture.height/poiDistanceFromCamera*pcToPhoneScaleImage*selfscale;
				if (index == 52 || index == 53) {
					GUI.DrawTexture (new Rect (screenpos.x - width / 2, Screen.height - screenpos.y - height/2, width, height), texture);
				} else {
					GUI.DrawTexture(new Rect (screenpos.x-width/2, Screen.height - screenpos.y-height, width, height), texture);
				}
			}
		}

		listPoisAlreadyInScreen = new ArrayList ();
		for (int i=0; i<list_display.Count; i++) {
			PoiClass poi = (PoiClass)list_display[i];
			int level = poi.level;
			float lon = poi.lon;
			float lat = poi.lat;
			int type = poi.type;
			if (type != selectedType){
				continue;
			}
			int indexInDataSource = poi.ishot;
			string name = poi.name;
			int isSelected = poi.isSelected;
			LonLatPoint lonlatpoint = new LonLatPoint(lon,lat);
			PixelPoint point = gp.lonlatToPixel (lonlatpoint,17);
			Vector3 screenpos = myCamera.WorldToScreenPoint(new Vector3 (-(float)point.pointX/100f,0f,(float)point.pointY/100f));
			if(!isPositonInScreen(screenpos))continue;//不在屏幕内的不显示
//			if(type == 100){//玩法
//				if (GUI.Button (new Rect (screenpos.x - 31.5f, Screen.height - screenpos.y - 76f, 63f, 76f), texture_howToPlayBottom)) {
//					if(!isDraging){
//						selectOnePoi(poi);
//					}
//				}
//				if (GUI.Button (new Rect (screenpos.x-poi.labelLength/2, Screen.height - screenpos.y-10, poi.labelLength, label_high), "")) {
//					if(!isDraging){
//						selectOnePoi(poi);
//					}
//				}
//				centeredStyle.alignment = TextAnchor.UpperCenter;
//				centeredStyle.normal.textColor = new Color(1,1,1,1);
//				GUI.Label (new Rect (screenpos.x-poi.labelLength/2, Screen.height - screenpos.y-label_stroke_width-10, poi.labelLength, label_high), name,centeredStyle);
//				GUI.Label (new Rect (screenpos.x-poi.labelLength/2-label_stroke_width, Screen.height - screenpos.y-10, poi.labelLength, label_high), name,centeredStyle);
//				GUI.Label (new Rect (screenpos.x-poi.labelLength/2+label_stroke_width, Screen.height - screenpos.y-10, poi.labelLength, label_high), name,centeredStyle);
//				GUI.Label (new Rect (screenpos.x-poi.labelLength/2, Screen.height - screenpos.y+label_stroke_width-10, poi.labelLength, label_high), name,centeredStyle);
//				centeredStyle.normal.textColor = Color.black;
//				GUI.Label (new Rect (screenpos.x-poi.labelLength/2, Screen.height - screenpos.y-10, poi.labelLength, label_high), name,centeredStyle);
//				if(isSelected == 1){
//					if (GUI.Button (new Rect (screenpos.x - 130f, Screen.height - screenpos.y - 76f-152, 259, 152), texture_howToPlayTop)) {
//						if(!isDraging){
//							print ("indexInDataSource is "+indexInDataSource);
//#if STANDALONE
//							if (Main.platform.Equals ("ios")) {
//								_unityCallIOS("go there|"+indexInDataSource);
//							}else{
//								unityCallAndroid("unityCallAndroid","");
//							}
//#endif
//						}
//					}
//					centeredStyle.normal.textColor = Color.black;
//					centeredStyle.alignment = TextAnchor.MiddleCenter;
//					GUI.Label (new Rect (screenpos.x - 130f, Screen.height - screenpos.y - 76f-152, 259, 152), "到这去",centeredStyle);
//				}
//				continue;
//			}

			float poiDistanceFromCamera = Vector3.Distance(new Vector3 (-(float)point.pointX/100f,0f,(float)point.pointY/100f),transform.position);
			if(kav.gdLevel2UnityCameraHeight(level) > poiDistanceFromCamera){
				poi.screenPosition = new Vector2(screenpos.x,Screen.height - screenpos.y);
				if(!calculateWhichPositionShouldPlace(poi)){
					continue;
				}
				listPoisAlreadyInScreen.Add(poi);
				if(isSelected == 1){//big
					
					if (GUI.Button (new Rect (screenpos.x - 31.5f, Screen.height - screenpos.y - 76f, 63f, 76f), (GUIContent)texture_big[type])) {
						if(!isDraging){
#if STANDALONE
							if (Main.platform.Equals ("ios")) {
								_unityCallIOS("clickpoi|"+name);
							}else{
								unityCallAndroid("unityCallAndroid",""+name);
							}
#endif
						}
					}
					if (GUI.Button (new Rect (screenpos.x-poi.labelLength/2, Screen.height - screenpos.y-10, poi.labelLength, label_high), "")) {
						if(!isDraging){
#if STANDALONE
							if (Main.platform.Equals ("ios")) {
								_unityCallIOS("clickpoi|"+name);
							}else{
								unityCallAndroid("unityCallAndroid",""+name);
							}
#endif
						}
					}
					if (isPoiTypeHasLabel(type)) {
						centeredStyle.alignment = TextAnchor.UpperCenter;
						centeredStyle.normal.textColor = new Color(1,1,1,1);
						GUI.Label (new Rect (screenpos.x-poi.labelLength/2, Screen.height - screenpos.y-label_stroke_width-10, poi.labelLength, label_high), name,centeredStyle);
						GUI.Label (new Rect (screenpos.x-poi.labelLength/2-label_stroke_width, Screen.height - screenpos.y-10, poi.labelLength, label_high), name,centeredStyle);
						GUI.Label (new Rect (screenpos.x-poi.labelLength/2+label_stroke_width, Screen.height - screenpos.y-10, poi.labelLength, label_high), name,centeredStyle);
						GUI.Label (new Rect (screenpos.x-poi.labelLength/2, Screen.height - screenpos.y+label_stroke_width-10, poi.labelLength, label_high), name,centeredStyle);
						centeredStyle.normal.textColor = Color.black;
						GUI.Label (new Rect (screenpos.x-poi.labelLength/2, Screen.height - screenpos.y-10, poi.labelLength, label_high), name,centeredStyle);
					}
				}else{//small
					if (GUI.Button (new Rect (screenpos.x - 22, Screen.height - screenpos.y - 22, 44, 44), (GUIContent)texture_small[type])) {
						if (!isDraging) {
							selectOnePoi (poi);
#if STANDALONE
							if (Main.platform.Equals ("ios")) {
								_unityCallIOS("clickpoi|"+name);
							}else{
								unityCallAndroid("unityCallAndroid",""+name);
							}
#endif
						}
					}
					if(isPoiTypeHasLabel(type)){
						float label_position_x = 0.0f;
						float label_position_y = 0.0f;
						switch(poi.textPosition){
						case 1://right
							label_position_x = screenpos.x + 22f;
							label_position_y = Screen.height - screenpos.y - label_high/2;
							centeredStyle.alignment = TextAnchor.MiddleLeft;
							break;
						case 2://left
							label_position_x = screenpos.x - 22f - poi.labelLength;
							label_position_y = Screen.height - screenpos.y - label_high/2;
							centeredStyle.alignment = TextAnchor.MiddleRight;
							break;
						case 3://top
							label_position_x = screenpos.x - poi.labelLength/2;
							label_position_y = Screen.height - screenpos.y - 22 - label_high + 10;
							centeredStyle.alignment = TextAnchor.LowerCenter;
							break;
						case 4://bottom
							label_position_x = screenpos.x - poi.labelLength/2;
							label_position_y = Screen.height - screenpos.y + 22 - 10;
							centeredStyle.alignment = TextAnchor.UpperCenter;
							break;
						default://right
							label_position_x = screenpos.x + 22f;
							label_position_y = Screen.height - screenpos.y - label_high/2;
							centeredStyle.alignment = TextAnchor.MiddleLeft;
							break;
						}

						if (GUI.Button (new Rect (label_position_x, label_position_y, poi.labelLength, label_high), "")) {
							if(!isDraging){
								selectOnePoi(poi);
#if STANDALONE
								if (Main.platform.Equals ("ios")) {
									_unityCallIOS("clickpoi|"+name);
								}else{
									unityCallAndroid("unityCallAndroid",""+name);
								}
#endif
							}
						}
						centeredStyle.normal.textColor = new Color(1,1,1,1);
						GUI.Label (new Rect (label_position_x, label_position_y-label_stroke_width, poi.labelLength, label_high), name,centeredStyle);
						GUI.Label (new Rect (label_position_x-label_stroke_width, label_position_y, poi.labelLength, label_high), name,centeredStyle);
						GUI.Label (new Rect (label_position_x+label_stroke_width, label_position_y, poi.labelLength, label_high), name,centeredStyle);
						GUI.Label (new Rect (label_position_x, label_position_y+label_stroke_width, poi.labelLength, label_high), name,centeredStyle);
						centeredStyle.normal.textColor = new Color(55f/255f,55f/255f,55f/255f,1);
						GUI.Label (new Rect (label_position_x, label_position_y, poi.labelLength, label_high), name,centeredStyle);
					}
				}
			}
		}
		//location

		LonLatPoint myLoc = new LonLatPoint(myLocationLon,myLocationLat);
		PixelPoint myLocPoint = gp.lonlatToPixel (myLoc,17);
		Vector3 myLocScreenPoint = myCamera.WorldToScreenPoint(new Vector3 (-(float)myLocPoint.pointX/100f,0f,(float)myLocPoint.pointY/100f));
		GUIUtility.RotateAroundPivot (90f-(transform.eulerAngles.y-180)+heading, new Vector2(myLocScreenPoint.x,Screen.height - myLocScreenPoint.y));
		if(isPositonInScreen(myLocScreenPoint)){//不在屏幕内的不显示
			GUI.DrawTexture(new Rect (myLocScreenPoint.x-40, Screen.height - myLocScreenPoint.y-40, 80, 80), texture_myLoc);
		}
	}
	bool isContinuousSameGesture(string ges){
		int count = gestureList.Count;
		if (count < 3) {
			return false;
		} else {
			if(gestureList[count-1].Equals(ges)&&gestureList[count-2].Equals(ges)&&gestureList[count-3].Equals(ges)){
				return true;
			}else{
				return false;
			}
		}
	}
	bool isPositonInScreen(Vector3 position){
		float x_inscreen = position.x;
		float y_inscreen = Screen.height - position.y;
		if (x_inscreen < 0 || y_inscreen < 0)
			return false;
		if (x_inscreen > Screen.width || y_inscreen > Screen.height)
			return false;
		return true;
	}
	//如果返回true则说明可以放到屏幕上，返回false则不能放屏幕上
	bool calculateWhichPositionShouldPlace(PoiClass poi){
		if (listPoisAlreadyInScreen.Count == 0)
			return true;
		if (poi.isSelected == 1) {//点击选择的，没得说，直接加上
			return true;
		}
		int i, j;

		if (isPoiTypeHasLabel(poi.type)) {//有label的图标
			for (i = 1; i<=4; i++) {//4 position
				poi.textPosition = i;
				for (j = 0; j<listPoisAlreadyInScreen.Count; j++) {
					PoiClass poiAlreadyInScreen = (PoiClass)listPoisAlreadyInScreen [j];
					if (poi.hasConflict (poiAlreadyInScreen,selectedType)) {
						break;
					}
				}
				if (j == listPoisAlreadyInScreen.Count) {
					return true;
				}
			}
			return false;//放到4个方向哪个都有冲突，不放到屏幕上
		} else {//没有label的图标
			return true;
		}

//		if (poi.type == selectedType) {//把所有该类型的poi都显示为大图标,有冲突的不予显示
//			for (i = 0; i<listPoisAlreadyInScreen.Count; i++) {d
//				PoiClass poiAlreadyInScreen = (PoiClass)listPoisAlreadyInScreen [i];
//				if (poi.hasConflict (poiAlreadyInScreen,selectedType)) {
////					print("has conflict2");
//					break;
//				}
//			}
//			if(i < listPoisAlreadyInScreen.Count){
//				return false;
//			}else{
//				return true;
//			}
			return true;
//		} else {//小图标表示的poi
			
//		}
	}
	//------------------------------------------供自游无限使用的接口-----------------------------------------------------
	//清除地图上的poi
	void removeAllPoi(){
		list_display.Clear ();
		list_datasource.Clear ();
	}
	//init params:lon,lat,以给定经纬度为中心点显示3d景区:example:"116.270928|39.986163"
	void zywx_init3D(string message){
		print ("unity:zywx_init3D:" + message);
		string []s = message.Split(new char[] { '|' });
		//以该点为中心显示
		zywx_moveToLocation(float.Parse(s[0]),float.Parse(s[1]),true,false,10,20);
		print ("unity:zywx_init3D:success" );
	}
	void zywx_moveToLocation(float lon,float lat,bool annimation,bool isWatch,float high,float speed){
		step = speed;
		LonLatPoint lonlatpoint = new LonLatPoint(lon,lat);
		PixelPoint point = gp.lonlatToPixel (lonlatpoint,17);

		if (isWatch) {
			transform.eulerAngles = new Vector3 (60, 180, 0); 
			transform.position = new Vector3(transform.position.x,1.5f,transform.position.z);
			float zoffsize = Mathf.Tan(DegreetoRadians(90-transform.eulerAngles.x))*transform.position.y;
			targetPosition = new Vector3 (-(float)point.pointX / 100f, high, (float)point.pointY / 100f+zoffsize);
			aroundPosition = new Vector3 (-(float)point.pointX / 100f, 0, (float)point.pointY / 100f);
			wantWatch = true;
		} else {
			transform.eulerAngles = new Vector3 (90, 180, 0); 
			targetPosition = new Vector3 (-(float)point.pointX / 100f, high, (float)point.pointY / 100f);
		}
		if (annimation) {
			startMove = true;
		} else {
			transform.position = targetPosition;
		}
	}
	//poi数据传递给unity:
	void zywx_setPoiDateSource(string message){
		print ("unity:zywx_setPoiDateSource:" + message);
		removeAllPoi ();
		string[] spots = message.Split (new char[] { ';' });
		for (int i = 0; i < spots.Length; i++) {
			string[] oneSpot = spots[i].Split(new char[] { ',' });
			float lon = float.Parse(oneSpot[0]);
			float lat = float.Parse(oneSpot[1]);
			string name = oneSpot[2];
			float labelLength = name.Length * 30;
			int type = int.Parse(oneSpot[3]);
//			int ishot = int.Parse(oneSpot[4]);
//			int level = int.Parse(oneSpot[5]);
			PoiClass poi = new PoiClass(lon,lat,name,type,0,0,0,1,labelLength);
			PoiClass poiDisplay = new PoiClass(lon,lat,name,type,i,0,0,1,labelLength);
			list_datasource.Add(poi);
			list_display.Add(poiDisplay);
		}
		print ("unity:zywx_setPoiDateSource:success" );
	}

	//展示热门景点并360度查看
	void zywx_displayOnePoi(string message){
		print ("unity:zywx_displayOnePoi:" + message);
		foreach (PoiClass poi in list_display) {
			if(poi.name.Equals(message)){
				zywx_moveToLocation(poi.lon,poi.lat,true,false,5f,10);
				selectOnePoi(poi);
				break;
			}
		}
		print ("unity:zywx_displayPoi:success" );
	}
	void zywx_highlightDisplayPoiWithType(string message){
		print ("unity:zywx_highlightDisplayPoiWithType:" + message);
		selectedType = int.Parse (message);
		if (lastSelectedPoi != null) {
			lastSelectedPoi.isSelected = 0;
		}
		//根据datasource和类型重新构造list_display
		ArrayList list_temp = new ArrayList();//
		list_display.Clear();
		foreach (PoiClass poi in list_datasource) {
			if(poi.type == selectedType)
			{
				list_display.Add(poi);
			}else{
				list_temp.Add(poi);
			}
		}
		if (list_display.Count == 0) {
			_unityCallIOS("alert|未找到该类型数据!");
		}
		foreach (PoiClass poi in list_temp) {
			list_display.Add (poi);
		}
		print ("unity:zywx_highlightDisplayPoiWithType:success" );
	}
	void zywx_listenOnePoi(string message){
		print ("unity:zywx_listenOnePoi:" + message);
		string []s = message.Split(new char[] { '|' });
		//以该点为中心显示
		zywx_moveToLocation(float.Parse(s[0]),float.Parse(s[1]),true,false,10,20);
		foreach (PoiClass poi in list_display) {
			if (poi.name.Equals (s[2])) {
				selectOnePoi (poi);
			}
		}
		print ("unity:zywx_listenOnePoi:success" );

	}
	void selectOnePoi(PoiClass poi){
		poi.isSelected = 1;
		if (lastSelectedPoi != null) {
			lastSelectedPoi.isSelected = 0;
		}
		lastSelectedPoi = poi;
		//将其加到第一个位置，那么级别将会最高，和他冲突的别的小图标将不予以显示
		list_display.Remove (poi);
		list_display.Insert (0,poi);
	}

	void setCameraHighToSeeAllPois(){
		print ("unity:setCameraHighToSeeAllPois:");
		if(list_display.Count < 1)return;
		PoiClass poi = (PoiClass)list_display[0];
		float minlat = poi.lat;
		float maxlat = poi.lat;
		float minlon = poi.lon;
		float maxlon = poi.lon;
		if (list_display.Count == 1) {
			zywx_moveToLocation(poi.lon,poi.lat,true,false,10f,8);
			return;
		}
		foreach(PoiClass poi2 in list_display){
			if(poi2.lat < minlat) minlat = poi2.lat;
			if(poi2.lat > maxlat) maxlat = poi2.lat;
			if(poi2.lon < minlon) minlon = poi2.lon;
			if(poi2.lon > maxlon) maxlon = poi2.lon;
		}
		zywx_setCameraPositionByBounds (""+minlat+","+maxlat+","+minlon+","+maxlon);
	}
	void zywx_setCameraPositionByBounds(string message){
		print ("unity:zywx_setCameraPositionByBounds:" + message);
		string[] str = message.Split(new char[] { ',' });
		float minlat = float.Parse(str[0]);
		float maxlat = float.Parse(str[1]);
		float minlon = float.Parse(str[2]);
		float maxlon = float.Parse(str[3]);
		LonLatPoint lonlatpoint1 = new LonLatPoint(minlon,minlat);
		PixelPoint point1 = gp.lonlatToPixel (lonlatpoint1,17);
		LonLatPoint lonlatpoint2 = new LonLatPoint(maxlon,minlat);
		PixelPoint point2 = gp.lonlatToPixel (lonlatpoint2,17);
		float yOffsize = Mathf.Abs ((float)point1.pointX / 100f - (float)point2.pointX / 100f);
		float cameraHigh = yOffsize / Mathf.Tan (DegreetoRadians(30));
		print ("cameraHigh is "+cameraHigh);
		if (cameraHigh > Main.fitHigh)
			cameraHigh = Main.fitHigh;
		zywx_moveToLocation((minlon+maxlon)/2,(minlat+maxlat)/2,false,false,cameraHigh,10);
		print ("unity:zywx_setCameraPositionByBounds:success" );
	}
	void zywx_locationChanged(string message){
		string[] str = message.Split(new char[] { ',' });
		myLocationLon = float.Parse(str[0]);
		myLocationLat = float.Parse(str[1]);
		heading = float.Parse (str [2]);
//		LonLatPoint lonlatpoint = new LonLatPoint(myLocationLon,myLocationLat);
//		PixelPoint point = gp.lonlatToPixel (lonlatpoint,17);
//		controller.SimpleMove (controller.transform.InverseTransformPoint((new Vector3 (-(float)point.pointX/100f,0.5f,(float)point.pointY/100f))));
	}
	void zywx_backToMainSceneAndLoadScenic(string message){
		Main.pleaseLoadScenicId = int.Parse (message);
		Application.LoadLevelAsync("LoadAB1202");
	}
	void zywx_backToMainScene(string message){
		Application.LoadLevelAsync("LoadAB1202");
	}
	//------------------------------------------调用自游无限的接口-----------------------------------------------------
	#if STANDALONE
	//unity调用iOS
	[DllImport ("__Internal")]
	private static extern void _unityCallIOS (string message);
	//unity调用android
	void unityCallAndroid(string funcname,string message){
	#if UNITY_ANDROID
		using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
			using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
				obj_Activity.Call (funcname,message);
			}
		}
	#endif
	}
	#endif
}

