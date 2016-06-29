using UnityEngine;
using System.Collections;

public class CarCtrl : MonoBehaviour {

	public GameObject referenceObj;

	private Vector3 lastRefPosition;

	private Quaternion lastRefRotation;

	private Vector3 lastPosition;

	private Quaternion lastRotation;

	private bool isLastRound;

	public float thescale;

	private int step;

	private int count;

	SerialCommunication serialCtrl;

	public bool isReadyToMove;

	bool testKey;

	public int debugCount = 5;

	public float disError;

	public float angleError;

	// Use this for initialization
	void Start () {

		lastRefPosition = lastPosition = new Vector3 (0, 0, 0);
		lastRefRotation = lastRotation = Quaternion.identity;
		serialCtrl = GameObject.Find ("RealCar").GetComponent<SerialCommunication> ();
		serialCtrl.open ();
		testKey = false;
		isLastRound = false;
		step = 0;
		count = 0;
		serialCtrl.median ();

	}

	void testRefPosRot(){
		this.transform.rotation = Quaternion.Inverse (lastRefRotation) * referenceObj.transform.rotation*this.transform.rotation;
		this.transform.Translate (referenceObj.transform.position - lastRefPosition);
	}

	bool isCloseEnough(){
		Vector3 thispos = new Vector3(this.transform.position.x,0,this.transform.position.z);
		Vector3 refpos = new Vector3(referenceObj.transform.position.x,0,referenceObj.transform.position.z);
		//print("isCloseEnough:\tdis:\t" + ((thispos - refpos).magnitude));
		if ((thispos - refpos).magnitude < disError)
			return true;
		return false;

	}
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Q))
			testKey = true;
		if(isReadyToMove){
			// move according to invisible tracked objects
			if(!lastRefPosition.Equals(new Vector3(0,0,0))){
				// test rotation
				drawRays();
				// move as reference move
				if (testKey) {
					// do it every 10 frames
					++count;
					if (count != debugCount) {
						return;
					}
					count = 0;
					testKey = true;

					switch (step) {
					case 0:
						if (isCloseEnough ())
							step = 4;
						else {
							if (turnRound ()) {
								++step;
								// for test
								//step = 0;
							}
						}
						break;
					case 1:
						if (goStraight ()) {
							++step;
						} else
							step = 0;
						break;
					case 2:
						if (isCloseEnough ()) {
							step = 4;
						} 
						break;
					case 3:
						turnBack ();
						if(isCloseEnough())
							step = 4;
						break;
					case 4:
						if (turnFace ()) {
							step = 0;
						}
						break;
					default:
						break;
					}
				}
			}
			//testKey = false;
			lastRefPosition = referenceObj.transform.position;
			lastRefRotation = referenceObj.transform.rotation;
			lastPosition = transform.position;
			lastRotation = transform.rotation;
			//print ("pos:\t" + transform.position);
			//testKey = false;
		}
	}

	bool isLastLeft = true;
	void turnRobot(bool change){
		if (isLastLeft ^ change) {
			serialCtrl.right ();
			isLastLeft = true;
		} else {
			serialCtrl.left ();
			isLastLeft = false;
		}
	}

	// test my rotation ctrl
	void drawRays(){
		Quaternion facing = Quaternion.identity;
		facing.SetFromToRotation (transform.rotation * Vector3.forward, referenceObj.transform.position - transform.position);
		Vector3 vFacing = facing * Vector3.forward;
		Vector3 vCur = transform.rotation * Vector3.forward;
		// test if these two vectors are correct
		Debug.DrawRay(this.transform.position,vCur,Color.green);
		Debug.DrawRay(this.transform.position,referenceObj.transform.position-this.transform.position,Color.magenta);
		//Debug.DrawRay(this.transform.position,vFacing,Color.red);
		Debug.DrawRay(this.transform.position,facing * new Vector3(0,0,-1),Color.cyan);
		//print ("this:\t" + transform.position.ToString ("F2") + "ref:\t" + referenceObj.transform.position.ToString ("F2"));
	}

	//step 0: facing the destination
	float lastAngle = 180;
	bool turnRound(){
		//serialCtrl.median ();
		if(isLastRound && (transform.position.Equals(lastPosition)))
			return false;
		else{
			Quaternion facing = Quaternion.identity;
			facing.SetFromToRotation (transform.rotation * Vector3.forward, referenceObj.transform.position - transform.position);
			Vector3 vFacing = referenceObj.transform.position-this.transform.position;
			Vector3 vCur = transform.rotation * Vector3.forward;
			Vector3 vUp = Vector3.Cross (vCur, vFacing);

			float angle = Vector3.Angle(vCur, vFacing);
			print ("turnRound:\tvCur:\t" + vCur.ToString ("F2") + "\tvFacing:\t" + vFacing.ToString ("F2"));
			print ("turnRound:\tangle:\t" + angle);
			if (angle % 360.0f > 6.0f) {
				print("turnRound:\tupVector:\t" + vUp.ToString("F2"));
				if (vUp.y > 0.005)
					serialCtrl.right ();
				else if (vUp.y < -0.005)
					serialCtrl.left ();
				else
					return true;
				lastAngle = angle;
				isLastRound = true;
				return false;
			} else {
				isLastRound = false;
				return true;
			}
		}

	}

	// step 1: go to the destination
	float lastDis = 180;
	bool goStraight(){
		serialCtrl.median ();
		Vector3 dis = referenceObj.transform.position - transform.position;
		print ("goStraight\tdis:\t" + dis.ToString("F3") + "\tref:\t" + referenceObj.transform.position.ToString("F3") + "\tcur:\t" + transform.position.ToString("F3") + "\tlastDis:\t" + lastDis.ToString("F2"));
		if (dis.magnitude > disError) {
			Vector3 vCur = transform.rotation * Vector3.forward;
			Vector3 vUp = Vector3.Cross (dis, vCur);
			print ("goStraight\tvCur:\t" + vCur.ToString ("F2") + "\tvUp:\t" + vUp.ToString ("F2"));
			if ((vCur.x * dis.x >= 0) || (vCur.z * dis.z >= 0))
				serialCtrl.forward ();
			else
				serialCtrl.backward ();
			return false;
		} else
			return true;
	}

	bool lastBack = false;
	void turnBack(){
		if (!lastBack || !lastRotation.Equals (transform.rotation)) {
			Vector3 vCur = transform.rotation * Vector3.forward;
			Vector3 vDes = referenceObj.transform.rotation * Vector3.forward;
			Vector3 vUp = Vector3.Cross (vCur, vDes);
			float angle = Vector3.Angle (vCur, vDes);
			if (angle > angleError) {
				if (vUp.y >= 0)
					serialCtrl.right ();
				else
					serialCtrl.left ();
				print ("rot in turn back:\t" + transform.rotation);
				lastBack = true;
			} else
				lastBack = false;
		}
	}

	float lastAngle2 = 180;
	bool turnFace(){
		Vector3 vCur = transform.rotation * Vector3.forward;
		Vector3 vDes = referenceObj.transform.rotation * Vector3.forward;
		Vector3 vUp = Vector3.Cross (vCur, vDes);
		float angle = Vector3.Angle (vCur, vDes);
		if (angle > angleError) {
			if (vUp.y > 0)
				serialCtrl.right ();
			else
				serialCtrl.left ();
			print ("rot in turn back:\t" + transform.rotation);
			return false;
		} else
			return true;
	}
}
