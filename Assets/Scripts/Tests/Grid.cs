using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    int rows = 6; 

    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void OnRenderObject()
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0);
        float lengthScale = 3.75f;

        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(Color.green);
        /* Horizontal lines. */
        for (int i = ((-rows) / 2); i <= rows / 2; i++)
        {
            float s = (float)i / 2;
            // floor
            GL.Vertex(this.transform.position + new Vector3(-rows / lengthScale, -1, s));
            GL.Vertex(this.transform.position + new Vector3(rows / lengthScale, -1, s));


            //left side

            GL.Vertex(this.transform.position + new Vector3(-1.5f, -rows / lengthScale, s));
            GL.Vertex(this.transform.position + new Vector3(-1.5f, rows / lengthScale,  s));

            //right side
            GL.Vertex(this.transform.position + new Vector3(1.5f, -rows / lengthScale, s));
            GL.Vertex(this.transform.position + new Vector3(1.5f, rows / lengthScale, s));

            //back side
            GL.Vertex(this.transform.position + new Vector3( s, -rows / lengthScale, -1.5f));
            GL.Vertex(this.transform.position + new Vector3(s, rows / lengthScale, -1.5f));

            //front side
            GL.Vertex(this.transform.position + new Vector3(s, -rows / lengthScale, 1.5f));
            GL.Vertex(this.transform.position + new Vector3(s, rows / lengthScale, 1.5f));

        }
        /* Vertical lines. */
        for (int i = (-rows / 2); i <= rows / 2; i++)
        {
            float s = (float)i / 2;
            //floor
            GL.Vertex(this.transform.position + new Vector3(s, -1, -rows / lengthScale));
            GL.Vertex(this.transform.position + new Vector3(s, -1, rows / lengthScale));


            //left side
            GL.Vertex(this.transform.position + new Vector3(-1.5f, s, -rows / lengthScale));
            GL.Vertex(this.transform.position + new Vector3(-1.5f, s, rows / lengthScale));


            //right side
            GL.Vertex(this.transform.position + new Vector3(1.5f, s, -rows / lengthScale));
            GL.Vertex(this.transform.position + new Vector3(1.5f, s, rows / lengthScale));

            //back side
            GL.Vertex(this.transform.position + new Vector3(-rows / lengthScale, s, -1.5f));
            GL.Vertex(this.transform.position + new Vector3(rows / lengthScale, s, -1.5f));

            //front side
            GL.Vertex(this.transform.position + new Vector3(-rows / lengthScale, s, 1.5f));
            GL.Vertex(this.transform.position + new Vector3(rows / lengthScale, s, 1.5f));
        }
        GL.End();
        GL.PopMatrix();
    }
}
