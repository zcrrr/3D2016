using UnityEngine;
using System.Collections;

public class ImageClass{
	//109.211543,18.298011,22,0,2
	public Vector3 worldLocation;
	public int imgIndex;
	public float scale;

	public ImageClass(Vector3 worldLocation,int imgIndex,float scale){
		this.worldLocation = worldLocation;
		this.imgIndex = imgIndex;
		this.scale = scale;
	}
}
