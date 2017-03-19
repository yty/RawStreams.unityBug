using UnityEngine;
using System.Collections;
using Intel.RealSense;
using Intel.RealSense.Face;
// For each subsequent algorithm module "using Intel.RealSense.AlgorithmModule;"

public class RawStreamsController : MonoBehaviour {

	[Header("Color Settings")]
	public int colorWidth = 640;
	public int colorHeight = 480;
	public float colorFPS = 30f;
	public Material RGBMaterial;

	[Header("Depth Settings")]
	public int depthWidth = 640;
	public int depthHeight = 480;
	public float depthFPS = 30f;
	public Material DepthMaterial;

	private SenseManager sm = null;
	private SampleReader sampleReader =  null;
	private NativeTexturePlugin texPlugin = null;
	
	private System.IntPtr colorTex2DPtr = System.IntPtr.Zero;
	private System.IntPtr depthTex2DPtr = System.IntPtr.Zero;

    /////////////////////////////////////////////////////// terry add start
    FaceModule faceModule;
    /////////////////////////////////////////////////////// terry add end
    void SampleArrived (object sender, SampleArrivedEventArgs args)
	{
		if(args.sample.Color != null) texPlugin.UpdateTextureNative (args.sample.Color, colorTex2DPtr);
		if(args.sample.Depth != null) texPlugin.UpdateTextureNative (args.sample.Depth, depthTex2DPtr);
	}
	
	// Use this for initialization
	void Start () {

		/* Create SenseManager Instance */
		sm = SenseManager.CreateInstance ();
		
		/* Create a SampleReader Instance */
		sampleReader = SampleReader.Activate (sm);
		
		/* Enable Color & Depth Stream */
		sampleReader.EnableStream (StreamType.STREAM_TYPE_COLOR, colorWidth, colorHeight, colorFPS);
		sampleReader.EnableStream (StreamType.STREAM_TYPE_DEPTH, depthWidth, depthHeight, depthFPS);
		
		/* Subscribe to sample arrived event */
		sampleReader.SampleArrived += SampleArrived;

        /////////////////////////////////////////////////////// terry add start

        faceModule = FaceModule.Activate(sm);
        if (faceModule == null)
        {
            Debug.LogError("FaceModule Initialization Failed");

        }
        //faceModule.FrameProcessed += FaceModule_FrameProcessed;

        FaceConfiguration moduleConfiguration = faceModule.CreateActiveConfiguration();
        if (moduleConfiguration == null)
        {
            Debug.LogError("FaceConfiguration Initialization Failed");

        }
        moduleConfiguration.TrackingMode = TrackingModeType.FACE_MODE_COLOR; 
        moduleConfiguration.Strategy = TrackingStrategyType.STRATEGY_RIGHT_TO_LEFT; 
        moduleConfiguration.Detection.maxTrackedFaces = 1;
        moduleConfiguration.Landmarks.maxTrackedFaces = 1;

        moduleConfiguration.Detection.isEnabled = true;
        moduleConfiguration.Landmarks.isEnabled = true;

        moduleConfiguration.Pose.isEnabled = false;

        moduleConfiguration.EnableAllAlerts(); 
        //moduleConfiguration.AlertFired += OnFiredAlert;

        Status applyChangesStatus = moduleConfiguration.ApplyChanges();
        Debug.Log(applyChangesStatus.ToString());


        ////////////////////////////////////////////////////// terry add end
        /* Initialize pipeline */
        sm.Init ();

		/* Create NativeTexturePlugin to render Texture2D natively */
		texPlugin = NativeTexturePlugin.Activate ();

		RGBMaterial.mainTexture = new Texture2D (colorWidth, colorHeight, TextureFormat.BGRA32, false); // Update material's Texture2D with enabled image size.
		RGBMaterial.mainTextureScale = new Vector2 (-1, -1); // Flip the image
		colorTex2DPtr = RGBMaterial.mainTexture.GetNativeTexturePtr ();// Retrieve native Texture2D Pointer
		
		DepthMaterial.mainTexture = new Texture2D (depthWidth, depthHeight, TextureFormat.BGRA32, false); // Update material's Texture2D with enabled image size.
		DepthMaterial.mainTextureScale = new Vector2 (-1, -1); // Flip the image
		depthTex2DPtr = DepthMaterial.mainTexture.GetNativeTexturePtr (); // Retrieve native Texture2D Pointer

		/* Start Streaming */
		sm.StreamFrames (false);

	}

	// Use this for clean up
	void OnDisable () {

		/* Clean Up */
		if (sampleReader != null) {
			sampleReader.SampleArrived -= SampleArrived;
			sampleReader.Dispose ();
		}

		if (sm != null) sm.Dispose ();
	}

}