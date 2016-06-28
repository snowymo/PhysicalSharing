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

	public int debugCount = 10;

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
		if ((thispos - refpos).magnitude < 0.07)
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

					switch (step) {
					case 0:
						if (turnRound ()) {
							++step;
						}
						break;
					case 1:
						if (goStraight ()) {
							++step;
						}
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
			serialCtrl.left ();
			isLastLeft = true;
		} else {
			serialCtrl.right ();
			isLastLeft = false;
		}
	}

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
		print ("this:\t" + transform.position.ToString ("F2") + "ref:\t" + referenceObj.transform.position.ToString ("F2"));
	}

	//step 0: facing the destination
	float lastAngle = 180;
	bool turnRound(){
		//serialCtrl.median ();
		if(isLastRound && (transform.position.Equals(lastPosition)))
			return false;
		else{
			Quaternion facing = Quaternion.identity;
			facing.SetFromToRotation (transform.position, referenceObj.transform.position);
			Vector3 vFacing = facing * Vector3.forward;
			Vector3 vCur = transform.rotation * Vector3.forward;
			// test if these two vectors are correct
			//Debug.DrawRay(this.transform.position,vCur,Color.green);
			//Debug.DrawRay(this.transform.position,vFacing,Color.red);

			float angle = Quaternion.Dot(transform.rotation, facing);
			print ("turnRound:\tangle:\t" + angle);
			if ((Mathf.Abs (Mathf.Abs (angle) - 1.0f) > 0.05f)) {
				print ("rot in turn round:\t" + transform.rotation);
				//turnRobot (angle > lastAngle);
				Vector3 upVector = Vector3.Cross(vCur,vFacing);
				print("turnRound:\tupVector:\t" + upVector.ToString("F2"));
//				if (upVector.y > 0)
//					serialCtrl.left ();
//				else
//					serialCtrl.right ();
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
	bool goStraight(){
		serialCtrl.median ();
		Vector3 dis = referenceObj.transform.position - transform.position;
		print ("goStraight\tdis:\t" + dis.ToString("F3") + "\tref:\t" + referenceObj.transform.position.ToString("F3") + "\tcur:\t" + transform.position.ToString("F3"));
		if (dis.magnitude > 0.07) {
			serialCtrl.forward ();
			return false;
		} else {
			return true;
		}
	}
	bool lastBack = false;
	void turnBack(){
		if (!lastBack || !lastRotation.Equals (transform.rotation)) {
			float angle = Quaternion.Dot(transform.rotation, referenceObj.transform.rotation);
			if ((Mathf.Abs (Mathf.Abs (angle) - 1.0f) > 0.05f)) {
				print ("rot in turn back:\t" + transform.rotation);
				serialCtrl.left ();
				lastBack = true;
			} else
				lastBack = false;
		}
	}

	float lastAngle2 = 180;
	bool turnFace(){
		serialCtrl.median ();
		Quaternion facing = referenceObj.transform.rotation;
		float angle = Quaternion.Dot(transform.rotation, facing);
		print ("turnFace:\tangle:\t" + angle + "\tref:\t" + facing + "\tcur:\t" + transform.rotation);
		if ((Mathf.Abs (Mathf.Abs (angle) - 1.0f) > 0.05f)) {
			print ("rot in turn round:\t" + transform.rotation);
			turnRobot (angle > lastAngle2);
			lastAngle2 = angle;
			return false;
		} else {
			return true;
		}
	}
}
