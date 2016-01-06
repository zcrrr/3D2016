using UnityEngine;
using System.Collections;

public class TextureUtil : MonoBehaviour {
	ArrayList list_display = new ArrayList();//当前正在展示的数据
	ArrayList texture_big = new ArrayList ();
	ArrayList texture_small = new ArrayList ();
	ArrayList texture_image = new ArrayList ();
	ArrayList listImageContent = new ArrayList ();
	ArrayList listPoisAlreadyInScreen = new ArrayList();
	ArrayList listImage = new ArrayList();

	public Font myfont;
	GoogleProjection gp = new GoogleProjection ();
	Camera myCamera;
	KeyAndValue kav = new KeyAndValue ();
	int selectedType = -1;
	public static float label_high = 36f;
	public static float label_stroke_width = 3f;
	PoiClass lastSelectedPoi;
	int imageScale = 1;
	string userScale = "1";
	string deleteIndex = "0";
	bool displayIndex = false;
	bool displayPoi = true;


	//control
	int selectedButton = 0;
	float stepInt = 1f;
	float stepIntSmall = 0.1f;
	// Use this for initialization
	void Start () {
		Main.initById (119);
		myCamera = GetComponent<Camera>();
		string[] spots = Main.testdataPoi.Split (new char[] { ';' });
		for (int i = 0; i < spots.Length; i++) {
			string[] oneSpot = spots[i].Split(new char[] { ',' });
			float lon = float.Parse(oneSpot[0]);
			float lat = float.Parse(oneSpot[1]);
			string name = oneSpot[2];
			float labelLength = name.Length * 30;
			int type = int.Parse(oneSpot[3]);
			int ishot = int.Parse(oneSpot[4]);
			int level = int.Parse(oneSpot[5]);
			PoiClass poi = new PoiClass(lon,lat,name,type,ishot,level,0,1,labelLength);
			list_display.Add(poi);
		}
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
		for (int i=4; i<=Main.ImageCount; i++) {
			Texture2D image = (Texture2D)Resources.Load ("1.1-" + i);
			texture_image.Add (image);
			GUIContent content = new GUIContent ();
			content.image = image;
			listImageContent.Add(content);
		}
		if (Main.imgString.Length > 0) {
			string[] images = Main.imgString.Split (new char[] { ';' });
			for (int i=0; i<images.Length; i++) {
				string[] image = images [i].Split (new char[] { ',' });
				float x = float.Parse (image [0]);
				float y = float.Parse (image [1]);
				float z = float.Parse (image [2]);
				int index = int.Parse (image [3]);
				float selfscale = float.Parse (image [4]);
				Vector3 worldloc = new Vector3(x,y,z);
				ImageClass imageclass = new ImageClass(worldloc,index,selfscale);
				listImage.Add(imageclass);

			}
		}
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
		if (Input.GetKeyUp (KeyCode.Q)) {
			if(listImage.Count > 0){
				listImage.RemoveAt(listImage.Count-1);
			}
		}

//		if (Input.GetKeyUp (KeyCode.Escape))  
//		{  
//			adding = false;
//			statedes = "正常点击";
//		}  
		if (Input.GetKeyUp (KeyCode.F1))  
		{  
			if(displayIndex){
				displayIndex = false;
			}else{
				displayIndex = true;
			}
		} 
		if (Input.GetKeyUp (KeyCode.F2))  
		{  
			if(displayPoi){
				displayPoi = false;
			}else{
				displayPoi = true;
			}
		} 
		if(Input.GetMouseButtonDown(0)){ //a coordinate of screen (left down corner is 0,0 and right up corner is maxWidth,maxHeight)  
			float x = Input.mousePosition.x;//get x of coordinate  
			float y = Input.mousePosition.y;//get y of coordinate  
			print("x:"+x+"  y:"+y); 
			if(y < Screen.height - 240){
				Vector3 locationWorld = myCamera.ScreenToWorldPoint(new Vector3(x,y,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x))));
				//			print("locationWorld:"+locationWorld);
				locationWorld.y = 0;
				ImageClass image = new ImageClass(locationWorld,selectedButton,float.Parse(userScale));
				listImage.Add(image);
			}

		}  
	}
	void OnGUI () { 
		GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
		centeredStyle.alignment = TextAnchor.MiddleCenter;
		centeredStyle.fontSize = 12;
		centeredStyle.font = myfont;
		centeredStyle.normal.textColor = Color.black;
		for (int i=0; i<listImage.Count; i++) {
			ImageClass image = (ImageClass)listImage[i];
			float x = image.worldLocation.x;
			float y = 0;
			float z = image.worldLocation.z;
			Vector3 location = new Vector3(x,y,z);
			int index = image.imgIndex;
			Vector3 screenpos = myCamera.WorldToScreenPoint(location);
//			print("screenpos is "+screenpos);
			float poiDistanceFromCamera = Vector3.Distance(location,transform.position);
			Texture2D texture = (Texture2D)texture_image[index];
			float selfscale = image.scale;
			float width = texture.width/poiDistanceFromCamera*imageScale*selfscale;
			float height = texture.height/poiDistanceFromCamera*imageScale*selfscale;
			if (index == 52 || index == 53) {
				GUI.DrawTexture (new Rect (screenpos.x - width / 2, Screen.height - screenpos.y - height / 2, width, height), texture);
				if (displayIndex) {
					GUI.Label (new Rect (screenpos.x - width / 2, Screen.height - screenpos.y - height / 2, width, height), "" + i);
				}
			} else {
				GUI.DrawTexture(new Rect (screenpos.x-width/2, Screen.height - screenpos.y-height, width, height), texture);
				if(displayIndex){
					GUI.Label(new Rect (screenpos.x-width/2, Screen.height - screenpos.y-height/2, width, height),""+i);
				}
			}

		}
		int n = 0;
		int offsizex = 0;
		int offsizey = 0;
		for (n = 0; n<listImageContent.Count; n++) {
			if(n == selectedButton){
				GUI.backgroundColor = Color.red;
			}else{
				GUI.backgroundColor = Color.white;
			}
			if (GUI.Button (new Rect (10+offsizex, 10+offsizey, 50, 50), (GUIContent) listImageContent[n])) {
				selectedButton = n;
			}
			offsizex += 60;
			if(n > 1 && (n+1)%20 == 0){
				offsizex = 0;
				offsizey += 60;
			}
		}
//		if (GUI.Button (new Rect (Screen.width - 90, 10, 80, 50), "删除上一个")) {
//			if(listImage.Count > 0){
//				listImage.RemoveAt(listImage.Count-1);
//			}
//		}
//		if (GUI.Button (new Rect (Screen.width - 90 - 90, 10, 80, 50), statedes)) {
//			if(adding){
//				adding = false;
//				statedes = "正常点击";
//			}else{
//				adding = true;
//				statedes = "正在增加";
//			}
//		}
		centeredStyle.fontSize = 15;
		centeredStyle.alignment = TextAnchor.MiddleLeft;

		userScale = GUI.TextField(new Rect(Screen.width - 90 - 90, 10, 80, 50),userScale,15);
		if (GUI.Button (new Rect (Screen.width - 90, 10, 80, 50), "save")) {
			string output = "";
			for(int i=0;i<listImage.Count;i++){
				ImageClass image = (ImageClass)listImage[i];
				output += image.worldLocation.x + ","+0+","+image.worldLocation.z+","+image.imgIndex+","+image.scale+";";
			}
			print(output);
		}
		if (GUI.Button (new Rect (Screen.width - 90, 70, 80, 50), "指定删除")) {
			listImage.RemoveAt(int.Parse(deleteIndex));
			GUI.SetNextControlName("");
			GUI.FocusControl("");
		}
		deleteIndex = GUI.TextField(new Rect(Screen.width - 90 - 90, 70, 80, 50),deleteIndex,15);

		centeredStyle.alignment = TextAnchor.UpperLeft;
		GUI.Label(new Rect(10,190,300,200),"使用说明：4个方向键，可以移动场景\nwsad：微调场景\nz:缩小场景,x:放大场景\nF1:影藏/显示 图片的编号\nF2:影藏/显示poi\nQ:删除上一个");

		if (!displayPoi)
			return;
		//data
		GUI.backgroundColor = Color.clear;
		centeredStyle.alignment = TextAnchor.UpperCenter;
		centeredStyle.fontSize = 29;
		listPoisAlreadyInScreen = new ArrayList ();
		for (int i=0; i<list_display.Count; i++) {
			PoiClass poi = (PoiClass)list_display [i];
			int level = poi.level;
			float lon = poi.lon;
			float lat = poi.lat;
			int type = poi.type;
			string name = poi.name;
			int isSelected = poi.isSelected;
			LonLatPoint lonlatpoint = new LonLatPoint (lon, lat);
			PixelPoint point = gp.lonlatToPixel (lonlatpoint, 17);
			Vector3 screenpos = myCamera.WorldToScreenPoint (new Vector3 (-(float)point.pointX / 100f, 0f, (float)point.pointY / 100f));
			if (!isPositonInScreen (screenpos))
				continue;//不在屏幕内的不显示

			float poiDistanceFromCamera = Vector3.Distance (new Vector3 (-(float)point.pointX / 100f, 0f, (float)point.pointY / 100f), transform.position);
			
			if (kav.gdLevel2UnityCameraHeight (level) > poiDistanceFromCamera) {
				poi.screenPosition = new Vector2 (screenpos.x, Screen.height - screenpos.y);
				if (!calculateWhichPositionShouldPlace (poi)) {
					continue;
				}
				listPoisAlreadyInScreen.Add (poi);
				if (type == selectedType || isSelected == 1) {//big
					if (GUI.Button (new Rect (screenpos.x - 31.5f, Screen.height - screenpos.y - 76f, 63f, 76f), (GUIContent)texture_big [type])) {
					}
					if (GUI.Button (new Rect (screenpos.x - poi.labelLength / 2, Screen.height - screenpos.y - 10, poi.labelLength, label_high), "")) {
					}
					centeredStyle.alignment = TextAnchor.UpperCenter;
					centeredStyle.normal.textColor = new Color (1, 1, 1, 1);
					GUI.Label (new Rect (screenpos.x - poi.labelLength / 2, Screen.height - screenpos.y - label_stroke_width - 10, poi.labelLength, label_high), name, centeredStyle);
					GUI.Label (new Rect (screenpos.x - poi.labelLength / 2 - label_stroke_width, Screen.height - screenpos.y - 10, poi.labelLength, label_high), name, centeredStyle);
					GUI.Label (new Rect (screenpos.x - poi.labelLength / 2 + label_stroke_width, Screen.height - screenpos.y - 10, poi.labelLength, label_high), name, centeredStyle);
					GUI.Label (new Rect (screenpos.x - poi.labelLength / 2, Screen.height - screenpos.y + label_stroke_width - 10, poi.labelLength, label_high), name, centeredStyle);
					centeredStyle.normal.textColor = Color.black;
					GUI.Label (new Rect (screenpos.x - poi.labelLength / 2, Screen.height - screenpos.y - 10, poi.labelLength, label_high), name, centeredStyle);
				} else {//small
					if (GUI.Button (new Rect (screenpos.x - 22, Screen.height - screenpos.y - 22, 44, 44), (GUIContent)texture_small [type])) {
						selectOnePoi (poi);
					}
					if (type == 0 || type == 1 || type == 2) {
						float label_position_x = 0.0f;
						float label_position_y = 0.0f;
						switch (poi.textPosition) {
						case 1://right
							label_position_x = screenpos.x + 22f;
							label_position_y = Screen.height - screenpos.y - label_high / 2;
							centeredStyle.alignment = TextAnchor.MiddleLeft;
							break;
						case 2://left
							label_position_x = screenpos.x - 22f - poi.labelLength;
							label_position_y = Screen.height - screenpos.y - label_high / 2;
							centeredStyle.alignment = TextAnchor.MiddleRight;
							break;
						case 3://top
							label_position_x = screenpos.x - poi.labelLength / 2;
							label_position_y = Screen.height - screenpos.y - 22 - label_high + 10;
							centeredStyle.alignment = TextAnchor.LowerCenter;
							break;
						case 4://bottom
							label_position_x = screenpos.x - poi.labelLength / 2;
							label_position_y = Screen.height - screenpos.y + 22 - 10;
							centeredStyle.alignment = TextAnchor.UpperCenter;
							break;
						default://right
							label_position_x = screenpos.x + 22f;
							label_position_y = Screen.height - screenpos.y - label_high / 2;
							centeredStyle.alignment = TextAnchor.MiddleLeft;
							break;
						}
						
						if (GUI.Button (new Rect (label_position_x, label_position_y, poi.labelLength, label_high), "")) {
							selectOnePoi (poi);
						}
						centeredStyle.normal.textColor = new Color (1, 1, 1, 1);
						GUI.Label (new Rect (label_position_x, label_position_y - label_stroke_width, poi.labelLength, label_high), name, centeredStyle);
						GUI.Label (new Rect (label_position_x - label_stroke_width, label_position_y, poi.labelLength, label_high), name, centeredStyle);
						GUI.Label (new Rect (label_position_x + label_stroke_width, label_position_y, poi.labelLength, label_high), name, centeredStyle);
						GUI.Label (new Rect (label_position_x, label_position_y + label_stroke_width, poi.labelLength, label_high), name, centeredStyle);
						centeredStyle.normal.textColor = Color.black;
						GUI.Label (new Rect (label_position_x, label_position_y, poi.labelLength, label_high), name, centeredStyle);
					}
				}
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
	bool calculateWhichPositionShouldPlace(PoiClass poi){
		if (listPoisAlreadyInScreen.Count == 0)
			return true;
		if (poi.isSelected == 1) {//点击选择的，没得说，直接加上
			return true;
		}
		int i, j;
		if (poi.type == selectedType) {//把所有该类型的poi都显示为大图标,有冲突的不予显示
			return true;
		} else {//小图标表示的poi
			if (poi.type == 0 || poi.type == 1 || poi.type == 2) {//有label的图标
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
		}
	}
	void selectOnePoi(PoiClass poi){
		print ("list_display is "+ ((PoiClass)list_display[0]).name + " "+((PoiClass)list_display[1]).name);
		poi.isSelected = 1;
		if (lastSelectedPoi != null) {
			lastSelectedPoi.isSelected = 0;
		}
		lastSelectedPoi = poi;
		//将其加到第一个位置，那么级别将会最高，和他冲突的别的小图标将不予以显示
		list_display.Remove (poi);
		list_display.Insert (0,poi);
		print ("list_display is "+ ((PoiClass)list_display[0]).name + " "+((PoiClass)list_display[1]).name);
	}
	float DegreetoRadians(float x)
	{
		return x * 0.017453292519943295769f;
	}
}
