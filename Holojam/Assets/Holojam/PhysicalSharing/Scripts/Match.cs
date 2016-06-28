using UnityEngine;
using System.Collections;

public class Match : MonoBehaviour {

	public GameObject trackedCar;

	public GameObject referenceCar;

	public Vector3 offset;

	public GameObject car;

	public GameObject frame;

	public float disError;

	public float rotateError;

	public float yError;

	// Use this for initialization
	void Start () {
		//GameObject.Find ("sphere");
	}

	// Update is called once per frame
	void Update () {
		if (!trackedCar.GetComponent<TrackedCar> ().isReadyToMove) {
			if (checkMatchStart ()) {
				trackedCar.GetComponent<TrackedCar> ().isReadyToMove = true;
				trackedCar.GetComponent<CarCtrl> ().isReadyToMove = true;
				print ("match finished");
				// for test now so that I won't hide the start ball
				//this.enabled = false;
				//this.gameObject.SetActive (false);
				//GetComponent(MeshRenderer) = false;
				car.gameObject.SetActive(false);
				frame.gameObject.SetActive(false);

			}
		}
		// place the start ball where reference plus offset is
		this.transform.rotation = referenceCar.transform.rotation;
		this.transform.position = referenceCar.transform.position + offset;
	}

	bool checkMatchStart(){
		// check if tracked ball is very close to start ball

		//print((trackedBall.transform.position));

		// temp version
		Vector3 temp1 = new Vector3(this.transform.position.x,0,this.transform.position.z);
		Vector3 temp2 = new Vector3(trackedCar.transform.position.x,0,trackedCar.transform.position.z);
		print((temp1-temp2).magnitude);
		//		print ("euler angle:\t" + Quaternion.Angle (transform.rotation, trackedBall.transform.rotation));//8
		float matching = Quaternion.Dot(transform.rotation, trackedCar.transform.rotation);
		print ("checkMatchStart:\tQuaternion dot:\t" + (Mathf.Abs(Mathf.Abs(matching)-1.0f)));//
		print("checkMatchStart:\t" + (transform.position.y-trackedCar.transform.position.y));
		// 
		if ((temp1-temp2).magnitude < disError && ((transform.position.y-trackedCar.transform.position.y) < yError)
			&& (Mathf.Abs(Mathf.Abs(matching)-1.0f) < rotateError))
			return true;
		else
			return false;
	}
}
