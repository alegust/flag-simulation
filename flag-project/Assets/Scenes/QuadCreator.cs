using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class QuadCreator : MonoBehaviour {

	Mesh mesh;
	Point[] points;
	int[] triangles;
	Vector3[] verticeList;
	private float springStiffness = 10.0f;
	private float clothDamping = 1.0f;


	[SerializeField]
	public float cellSize = 1;
	public Vector3 gridOffset;
	public int gridSize;
	[SerializeField]
	private Vector3 gravity = new Vector3(0f, -9.82f, 0f);
	private float integratorTimeStep = 1.0f / 60.0f;
	private float timeStep = 1.0f/60.0f;






void Awake(){
	mesh = GetComponent<MeshFilter> ().mesh;
}


void Start(){
	makeGrid();
	UpdateMesh();
}

void makeGrid(){
	points = new Point[(gridSize + 1) * (gridSize + 1)];
	triangles = new int[gridSize * gridSize * 6];

	int v = 0;
	int t = 0;

	float vertexOffSet = cellSize * 0.5f;
// Fixar punkter
	for(int x = 0; x <= gridSize; x++){
		for(int z = 0; z <= gridSize; z++) {
			points[v] = new Point(new Vector3((x * cellSize) - vertexOffSet, (z*cellSize) - vertexOffSet, 0));
			v++;
		}
	}
	v = 0;
// Fixar trianglar
	for(int x = 0; x < gridSize; x++){
		for(int z = 0; z < gridSize; z++) {
				triangles[t] = v;
				triangles[t + 1] = triangles[t + 4] = v + 1;
				triangles[t + 2] = triangles[t + 3] = v + (gridSize + 1);
				triangles[t + 5] = v + (gridSize + 1) + 1;
				v++;
				t += 6;
		}
		v++;
	}
}


public void applyGravity() {
	foreach (var point in points) {
		point.ClearForce();
		point.ApplyForce(point.mass * gravity);
	}
}


public void applyShearingForce() {
	int rowEnd = gridSize;
	int nextElemNextRow = gridSize + 2;
	float segmentLength = cellSize;
	float lenBefore = cellSize;
	for (int i = 0; i < points.Length - 1; i++) {
		if((i % (gridSize + 1) != gridSize) && (i < (points.Length - (gridSize + 1) ))) { // alla noder utom den sista på alla rader utom den sista
			Point ij = points[i];
			Debug.Log(i + nextElemNextRow);
			Point i1j1 = points[i + nextElemNextRow];

			Vector3 ijVelocity = ij.force*(timeStep/ij.mass);
			Vector3 i1j1Velocity = i1j1.force*(timeStep/i1j1.mass);
			Vector3 relativVel = ijVelocity - i1j1Velocity;

			Vector3 r = i1j1.position - ij.position; // vector mellan punkt 1 och 2
			float normR = r.magnitude; // Normen av vektor r
			Vector3 xdif = (normR - segmentLength)*(r/normR);
			Vector3 springForce = springStiffness*xdif;
			Vector3 dampeningForce = clothDamping*relativVel;
			Vector3 totalForce = springForce - dampeningForce;
			ij.ApplyForce(totalForce);
			i1j1.ApplyForce(-totalForce);
		}

		if((i > gridSize) && ((i % (gridSize + 1)) != 3)){
			Point ij = points[i];
			Point i1j_1 = points[i - gridSize];

			Vector3 ijVelocity = ij.force*(timeStep/ij.mass);
			Vector3 i1j_1Velocity = i1j_1.force*(timeStep/i1j_1.mass);
			Vector3 relativVel = ijVelocity - i1j_1Velocity;

			Vector3 r = i1j_1.position - ij.position; // vector mellan punkt 1 och 2
			float normR = r.magnitude; // Normen av vektor r
			Vector3 xdif = (normR - segmentLength)*(r/normR);
			Vector3 springForce = springStiffness*xdif;
			Vector3 dampeningForce = clothDamping*relativVel;
			Vector3 totalForce = springForce - dampeningForce;
			ij.ApplyForce(totalForce);
			i1j_1.ApplyForce(-totalForce);
		}
	}
}

public void applyBendingForce() {

	float segmentLength = cellSize;
	float lenBefore = cellSize;
	int rowEnd = gridSize ; // Sista elementet i första raden
	int nextRow = gridSize; // indexet för första elementet i nästa rad
	int twoRowsDown = (2*(gridSize + 1)); //
	for (int i = 0; i < points.Length - 1; i++) {
		if(i <= rowEnd - 2) {

			Point ij = points[i];
			Point i2j = points[i + 2]; // [i + 1, j]
			Vector3 ijVelocity = ij.force*(timeStep/ij.mass);
			Vector3 i2jVelocity = i2j.force*(timeStep/i2j.mass);
			Vector3 relativVel = ijVelocity - i2jVelocity;

			Vector3 r = i2j.position - ij.position; // vector mellan punkt 1 och 2
			float normR = r.magnitude; // Normen av vektor r
			Vector3 xdif = (normR - segmentLength)*(r/normR);
			Vector3 springForce = springStiffness*xdif;
			Vector3 dampeningForce = clothDamping*relativVel;
			Vector3 totalForce = springForce - dampeningForce;
			ij.ApplyForce(totalForce);
			i2j.ApplyForce(-totalForce);
			//rowEnd+= gridSize + 1;
		}

		if(i < points.Length-(2*gridSize + 2)) {
				Point ij = points[i];
				Point ij2 = points[twoRowsDown];
				Vector3 ijVelocity = ij.force*(timeStep/ij.mass);
				Vector3 ij2Velocity = ij2.force*(timeStep/ij2.mass);
				Vector3 relativVel = ijVelocity - ij2Velocity;
				Vector3 r = ij2.position - ij.position; // vector mellan punkt 1 och 2
				float normR = r.magnitude; // Normen av vektor r

				Vector3 xdif = (normR - segmentLength)*(r/normR);
				Vector3 springForce = springStiffness*xdif;
				Vector3 dampeningForce = clothDamping*relativVel;
				Vector3 totalForce = springForce - dampeningForce;
				ij.ApplyForce(totalForce);
				ij2.ApplyForce(-totalForce);
				twoRowsDown++;
		}
	}
}

public void Advance(Point[] points, float timeStep)
{
		//updateForcesFunc(timeStep);
		applyGravity();
		//lockOrigin();
		applyShearingForce();
		ApplySpringForces();
		applyBendingForce();
		lockOrigin();
		foreach (var point in points)
		{

				Vector3 x = point.position;
				Vector3 temp = x;
				Vector3 oldx = point.oldPosition;
				Vector3 a = point.force/point.mass;
				x += x - oldx + a * timeStep * timeStep;
				Vector3 test = a + oldx;
				Vector3 test2 = a + x;
				point.position = x;
				point.oldPosition = temp;
		}
}

void lockOrigin(){
	int len = points.Length;
	Point origin = points[len-1];
	origin.ClearForce();
}
void ApplySpringForces()
{
		float segmentLength = cellSize;
		float lenBefore = cellSize;
		int rowEnd = gridSize ; // Sista elementet i första raden
		int nextRow = gridSize; // indexet för första elementet i nästa rad
		for (int i = 0; i < points.Length - 1; i++)
		{
				if(i == rowEnd) {
				//	Debug.Log(rowEnd);
					rowEnd += gridSize + 1; // Nästa rads sista element position
					Point lastP = points[i];
					Point nextLastP = points[i + gridSize]; // sista elementet i raden under

					Vector3 lastPVelocity = lastP.force*(timeStep/lastP.mass);
					Vector3 NextLastPVelocity = nextLastP.force*(timeStep/nextLastP.mass);

					Vector3 relativeVel = lastPVelocity - NextLastPVelocity;

					Vector3 r = nextLastP.position - lastP.position;
					float normR = r.magnitude;
					Vector3 xdif = (normR - segmentLength)*(r/normR);
					Vector3 springForce = springStiffness*xdif;
					Vector3 dampeningForce = clothDamping*relativeVel;
					Vector3 totalForce;
					totalForce = springForce - dampeningForce;
					lastP.ApplyForce(totalForce);
					nextLastP.ApplyForce(-totalForce);

				}

				/*
					Sista raden så räknar vi endast kraften från grannen till höger
				*/
				else if((points.Length - gridSize) <= i && i < points.Length - 1){
					Point ij = points[i];
					Point i1j = points[i + 1]; // [i + 1, j]
					Vector3 ijVelocity = ij.force*(timeStep/ij.mass);
					Vector3 i1jVelocity = i1j.force*(timeStep/i1j.mass);
					Vector3 relativVel = ijVelocity - i1jVelocity;

					Vector3 r = i1j.position - ij.position; // vector mellan punkt 1 och 2
					float normR = r.magnitude; // Normen av vektor r
					Vector3 xdif = (normR - segmentLength)*(r/normR);
					Vector3 springForce = springStiffness*xdif;
					Vector3 dampeningForce = clothDamping*relativVel;
					Vector3 totalForce = springForce - dampeningForce;
					ij.ApplyForce(totalForce);
					i1j.ApplyForce(-totalForce);
				}
				/*
					Kolumnen efter ursprungs punkten fjäderkraft
				*/
				else {
					Point i1j = points[i + 1]; // [i + 1, j]
					Point ij = points[i];
					Vector3 ijVelocity = ij.force*(timeStep/ij.mass);
					Vector3 i1jVelocity = i1j.force*(timeStep/i1j.mass);
					Vector3 relativVel = ijVelocity - i1jVelocity;

					Vector3 r = i1j.position - ij.position; // vector mellan punkt 1 och 2
					float normR = r.magnitude; // Normen av vektor r
					Vector3 xdif = (normR - segmentLength)*(r/normR);
					Vector3 springForce = springStiffness*xdif;
					Vector3 dampeningForce = clothDamping*relativVel;
					Vector3 totalForce = springForce - dampeningForce;
					ij.ApplyForce(totalForce);
					i1j.ApplyForce(-totalForce);

					//relativVel = 0; // nollställer för o använda igen nedan.
					/*
						Raden nedanför ursprungs punkten fjäderkraft.
					*/

					Point ij1 = points[i + nextRow];
					Vector3 ij1Velocity = ij1.force*(timeStep/ij1.mass);
					relativVel = ijVelocity - ij1Velocity;
					r = ij1.position - ij.position;
					normR = r.magnitude;
					xdif = (normR - segmentLength)*(r/normR);
					springForce = springStiffness*xdif;
					dampeningForce = clothDamping*relativVel;
					totalForce = springForce - dampeningForce;
					ij.ApplyForce(totalForce);
					ij1.ApplyForce(-totalForce);
					//Debug.Log(i);

				}

		}
}





void UpdateMesh(){
	//Mesh localMesh = GetComponent<MeshFilter>().mesh;
	//Vector3[] vertices = mesh.vertices;

	Vector3[] vertices = new Vector3[points.Length];
	for(int i = 0; i < points.Length; i++) {
		//Debug.Log(points[i].getPosition());
		vertices[i] = points[i].getPosition();
	}
	mesh.Clear();
	mesh.vertices = vertices;
	mesh.triangles = triangles;
	mesh.RecalculateBounds();
	mesh.RecalculateNormals();

}


void Update(){
	Advance(points,integratorTimeStep);
	//Debug.Log(points.Length);

	Vector3[] vertices = new Vector3[points.Length];
	for(int i = 0; i < points.Length; i++) {
		//Debug.Log(points[i].getPosition());
		vertices[i] = points[i].getPosition();
	}
	mesh.Clear();
	mesh.vertices = vertices;
	mesh.triangles = triangles;
	mesh.RecalculateBounds();
	mesh.RecalculateNormals();

}
// Kamera position x = 55, y = 25.8, Z = -60,2

}
