using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour
{
	public GameObject fogPlane;
	public float radius = 5f;
	private float radiusSqr { get { return radius * radius; } }

	private Mesh mesh;
	private Vector3[] vertices;
	private Color[] colors;

	// Use this for initialization
	void Start()
	{
		Initialize();
	}

	// Update is called once per frame
	void Update()
	{

	}

	void Initialize()
	{
		mesh = fogPlane.GetComponent<MeshFilter>().mesh;
		vertices = mesh.vertices;
		colors = new Color[vertices.Length];
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = Color.black;
		}
		UpdateColor();
	}

	void UpdateColor()
	{
		mesh.colors = colors;
	}

	public void OnMouseDown()
	{
		if(TreesController.instance && TreesController.instance.canExplore)
        {
			Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mousePos.z = 0;
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 v = fogPlane.transform.TransformPoint(vertices[i]);
				float dist = Vector3.SqrMagnitude(v - mousePos);

				if (dist < radiusSqr)
				{
					float alpha = Mathf.Min(colors[i].a, dist / radiusSqr);
					colors[i].a = alpha;
				}

			}
			UpdateColor();

		}

	}
}
