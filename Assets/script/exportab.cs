#if UNITY_EDITOR
using UnityEditor;
public class CreateAssetBundles
{
	[MenuItem ("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles ()
	{	

		BuildPipeline.BuildAssetBundles ("AssetBundles/android",BuildAssetBundleOptions.None,BuildTarget.Android);
//		BuildPipeline.BuildAssetBundles ("AssetBundles");

	}

}
#endif