﻿#if UNITY_EDITOR
using UnityEditor;
public class CreateAssetBundles
{
	[MenuItem ("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles ()
	{	

		BuildPipeline.BuildAssetBundles ("AssetBundles",BuildAssetBundleOptions.None,BuildTarget.iOS);
//		BuildPipeline.BuildAssetBundles ("AssetBundles");

	}

}
#endif