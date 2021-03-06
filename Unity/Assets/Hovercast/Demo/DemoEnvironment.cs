﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hovercast.Demo {

	/*================================================================================================*/
	public class DemoEnvironment : MonoBehaviour {

		public int RandomSeed = 0;

		private const int Count = 400;

		public enum ColorMode {
			White,
			Random,
			Custom
		}

		public enum MotionType {
			Orbit,
			Spin,
			Bob,
			Grow
		}

		public enum CameraPlacement {
			Center,
			Back,
			Top
		}

		private readonly GameObject[] vHolds;
		private readonly GameObject[] vCubes;

		private readonly DemoMotion vOrbitMotion;
		private readonly DemoMotion vSpinMotion;
		private readonly DemoMotion vBobMotion;
		private readonly DemoMotion vGrowMotion;

		private readonly DemoAnimFloat vLightSpotAnim;
		private readonly DemoAnimVector3 vCameraAnim;
		private readonly DemoAnimQuaternion vCameraRotAnim;

		private readonly IDictionary<MotionType, DemoMotion> vMotionMap;
		private readonly IDictionary<CameraPlacement, Vector3> vCameraMap;
		private readonly IDictionary<CameraPlacement, Quaternion> vCameraRotMap;

		private System.Random vRandom;
		private GameObject vCubesObj;
		private Light vLight;
		private Light vSpotlight;
		private GameObject vEnviro;
		private ColorMode vColorMode;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public DemoEnvironment() {
			vHolds = new GameObject[Count];
			vCubes = new GameObject[Count];

			vOrbitMotion = new DemoMotion(10, 600);
			vSpinMotion = new DemoMotion(45, 600);
			vBobMotion = new DemoMotion(0.5f, 600);
			vGrowMotion = new DemoMotion(0.5f, 600);

			vLightSpotAnim = new DemoAnimFloat(600);
			vCameraAnim = new DemoAnimVector3(6000);
			vCameraRotAnim = new DemoAnimQuaternion(6000);

			vMotionMap = new Dictionary<MotionType, DemoMotion> {
				{ MotionType.Orbit,	vOrbitMotion },
				{ MotionType.Spin,	vSpinMotion },
				{ MotionType.Bob,	vBobMotion },
				{ MotionType.Grow,	vGrowMotion }
			};

			vCameraMap = new Dictionary<CameraPlacement, Vector3> {
				{ CameraPlacement.Center,	Vector3.zero },
				{ CameraPlacement.Back,	new Vector3(0, 0, 20) },
				{ CameraPlacement.Top,	new Vector3(0, 0, 20) }
			};

			vCameraRotMap = new Dictionary<CameraPlacement, Quaternion> {
				{ CameraPlacement.Center, Quaternion.identity },
				{ CameraPlacement.Back,	Quaternion.identity },
				{ CameraPlacement.Top,	Quaternion.FromToRotation(Vector3.forward, Vector3.up) }
			};
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void Awake() {
			if ( RandomSeed == 0 ) {
				vRandom = new System.Random();
			}
			else {
				vRandom = new System.Random(RandomSeed);
				UnityEngine.Random.seed = RandomSeed;
			}

			vCubesObj = new GameObject("Cubes");
			vCubesObj.transform.SetParent(gameObject.transform, false);
			
			vLight = GameObject.Find("Light").GetComponent<Light>();
			vSpotlight = GameObject.Find("Spotlight").GetComponent<Light>();
			vEnviro = GameObject.Find("DemoEnvironment");

			for ( int i = 0 ; i < Count ; ++i ) {
				BuildCube(i);
			}

			vSpotlight.enabled = false;

			////

			GameObject ovrObj = GameObject.Find("LeapOVRPlayerController");

			if ( ovrObj != null ) {
				OVRPlayerController ovrPlayer = ovrObj.GetComponent<OVRPlayerController>();
				ovrPlayer.SetSkipMouseRotation(true);
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public void Update() {
			if ( Input.GetKey(KeyCode.Escape) ) {
				Application.Quit();
				return;
			}

			UpdateOculus();

			vOrbitMotion.Update();
			vSpinMotion.Update();
			vBobMotion.Update();
			vGrowMotion.Update();

			for ( int i = 0 ; i < Count ; ++i ) {
				UpdateCube(i);
			}
			
			vSpotlight.intensity = vLightSpotAnim.GetValue();
			vSpotlight.enabled = (vSpotlight.intensity > 0);

			vEnviro.transform.localPosition = vCameraAnim.GetValue();
			vEnviro.transform.localRotation = vCameraRotAnim.GetValue();
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public ColorMode GetColorMode() {
			return vColorMode;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public void SetColorMode(ColorMode pMode, float pHue=0) {
			vColorMode = pMode;

			Color color = Color.white;

			if ( vColorMode == ColorMode.Custom ) {
				color = HsvToColor(pHue, 1, 1);
			}

			for ( int i = 0 ; i < Count ; ++i ) {
				GameObject cube = vCubes[i];

				if ( vColorMode == ColorMode.Random ) {
					color = cube.GetComponent<DemoCube>().ColorRandom;
				}

				cube.renderer.sharedMaterial.color = color;
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public void ToggleMotion(MotionType pType, bool pIsEnabled) {
			vMotionMap[pType].Enable(pIsEnabled);
		}

		/*--------------------------------------------------------------------------------------------*/
		public void SetMotionSpeed(float pSpeed) {
			vOrbitMotion.GlobalSpeed = pSpeed;
			vSpinMotion.GlobalSpeed = pSpeed;
			vBobMotion.GlobalSpeed = pSpeed;
			vGrowMotion.GlobalSpeed = pSpeed;
		}

		/*--------------------------------------------------------------------------------------------*/
		public void SetLightPos(float pPosition) {
			vLight.gameObject.transform.localPosition = new Vector3(0, pPosition, 0);
		}

		/*--------------------------------------------------------------------------------------------*/
		public void SetLightIntensitiy(float pIntensity) {
			vLight.intensity = pIntensity;
		}

		/*--------------------------------------------------------------------------------------------*/
		public void ShowSpotlight(bool pShow) {
			vLightSpotAnim.Start(vSpotlight.intensity, (pShow ? 3 : 0));
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public void SetCameraPlacement(CameraPlacement pPlace) {
			vCameraAnim.Start(vEnviro.transform.localPosition, vCameraMap[pPlace]);
			vCameraRotAnim.Start(vEnviro.transform.localRotation, vCameraRotMap[pPlace]);
		}

		/*--------------------------------------------------------------------------------------------*/
		public void ReorientCamera() {
			if ( OVRManager.display != null ) {
				OVRManager.display.RecenterPose();
			}
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private static void UpdateOculus() {
			if ( OVRManager.capiHmd == null ) {
				return;
			}

			if ( Input.GetKey(KeyCode.R) ) {
				OVRManager.display.RecenterPose();
			}

			if ( !OVRManager.capiHmd.GetHSWDisplayState().Displayed ) {
				return;
			}

			OVRManager.capiHmd.DismissHSWDisplay();
			OVRManager.display.RecenterPose();
		}

		/*--------------------------------------------------------------------------------------------*/
		private void BuildCube(int pIndex) {
			float radius = RandomFloat(4, 10);
			float radiusPercent = (radius-4)/6f;
			float orbitSpeed = (float)Math.Pow(1-radiusPercent, 2)*0.2f + 0.8f;

			var hold = new GameObject("Hold"+pIndex);
			hold.transform.parent = vCubesObj.transform;
			vHolds[pIndex] = hold;

			DemoCubeHold holdData = hold.AddComponent<DemoCubeHold>();
			holdData.OrbitAxis = RandomUnitVector();
			holdData.OrbitSpeed = RandomFloat(0.7f, 1, 2)*orbitSpeed;
			holdData.OrbitInitRot = UnityEngine.Random.rotationUniform;

			////

			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.transform.parent = hold.transform;
			cube.name = "Cube"+pIndex;
			cube.renderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
			vCubes[pIndex] = cube;

			DemoCube cubeData = cube.AddComponent<DemoCube>();
			cubeData.ColorRandom = RandomUnitColor(0.1f, 1);
			cubeData.SpinAxis = RandomUnitVector();
			cubeData.SpinSpeed = RandomFloat(0.5f, 1, 2);
			cubeData.SpinInitRot = UnityEngine.Random.rotationUniform;
			cubeData.BobSpeed = RandomFloat(0.5f, 1, 2);
			cubeData.BobInitPos = RandomFloat(-1, 1);
			cubeData.BobRadiusMin = radius;
			cubeData.BobRadiusMax = cubeData.BobRadiusMin+3;
			cubeData.GrowSpeed = RandomFloat(0.5f, 1, 2);
			cubeData.GrowInitPos = RandomFloat(-1, 1);
			cubeData.GrowScaleMin = RandomUnitVector(0.4f)*0.6f;
			cubeData.GrowScaleMax = RandomUnitVector(0.4f)*1.2f;
		}

		/*--------------------------------------------------------------------------------------------*/
		private void UpdateCube(int pIndex) {
			GameObject hold = vHolds[pIndex];
			GameObject cube = vCubes[pIndex];
			DemoCubeHold holdData = hold.GetComponent<DemoCubeHold>();
			DemoCube cubeData = cube.GetComponent<DemoCube>();

			float orbitAngle = vOrbitMotion.Position*holdData.OrbitSpeed;
			hold.transform.localRotation = holdData.OrbitInitRot*
				Quaternion.AngleAxis(orbitAngle, holdData.OrbitAxis);

			float spinAngle = vSpinMotion.Position*cubeData.SpinSpeed;
			cube.transform.localRotation = cubeData.SpinInitRot*
				Quaternion.AngleAxis(spinAngle, cubeData.SpinAxis);

			float bobPos = cubeData.BobInitPos+vBobMotion.Position*cubeData.BobSpeed;
			bobPos = (float)Math.Sin(bobPos*Math.PI)/2f + 0.5f;
			bobPos = Mathf.Lerp(cubeData.BobRadiusMin, cubeData.BobRadiusMax, bobPos);
			cube.transform.localPosition = new Vector3(0, 0, bobPos);

			float growPos = cubeData.GrowInitPos+vGrowMotion.Position*cubeData.GrowSpeed;
			growPos = (float)Math.Sin(growPos*Math.PI)/2f + 0.5f;
			cube.transform.localScale = 
				Vector3.Lerp(cubeData.GrowScaleMin, cubeData.GrowScaleMax, growPos);
		}
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private Vector3 RandomUnitVector() {
			var v = new Vector3(
				RandomFloat(-1, 1),
				RandomFloat(-1, 1),
				RandomFloat(-1, 1)
			);

			return v.normalized;
		}

		/*--------------------------------------------------------------------------------------------*/
		private Color RandomUnitColor(float pMin, float pMax) {
			int major = vRandom.Next()%3;
			int minor = (major+(vRandom.Next()%2)+1)%3;

			Func<int, float> getVal = (i => {
				if ( i == major ) {
					return pMax;
				}

				if ( i == minor ) {
					return RandomFloat(pMin, pMax);
				}

				return RandomFloat(0, pMin);
			});

			return new Color(getVal(0), getVal(1), getVal(2));
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private Vector3 RandomUnitVector(float pMinDimension) {
			var v = RandomUnitVector();
			v.x = Math.Max(v.x, pMinDimension);
			v.y = Math.Max(v.y, pMinDimension);
			v.z = Math.Max(v.z, pMinDimension);
			return v.normalized;
		}

		/*--------------------------------------------------------------------------------------------*/
		private float RandomFloat(float pMin, float pMax) {
			return (float)vRandom.NextDouble()*(pMax-pMin) + pMin;
		}

		/*--------------------------------------------------------------------------------------------*/
		private float RandomFloat(float pMin, float pMax, float pPow) {
			return (float)Math.Pow(RandomFloat(pMin, pMax), pPow);
		}

		/*--------------------------------------------------------------------------------------------*/
		//based on: http://stackoverflow.com/questions/1335426
		public static Color HsvToColor(float pHue, float pSat, float pVal) {
			float hue60 = pHue/60f;
			int i = (int)Math.Floor(hue60)%6;
			float f = hue60 - (int)Math.Floor(hue60);

			float v = pVal;
			float p = pVal * (1-pSat);
			float q = pVal * (1-f*pSat);
			float t = pVal * (1-(1-f)*pSat);

			switch ( i ) {
				case 0: return new Color(v, t, p);
				case 1: return new Color(q, v, p);
				case 2: return new Color(p, v, t);
				case 3: return new Color(p, q, v);
				case 4: return new Color(t, p, v);
				case 5: return new Color(v, p, q);
			}

			return Color.black;
		}

	}

}
