using System;
using System.Collections;
using UnityEngine;

public class MeshScratchpad
{
	public Vector3[] vbuf;
	public Vector3[] nbuf;
	public Color32[] cbuf;
	public Vector2[] uv;
	public int[] tbuf;
	
	public Vector3[] Init(int vcount) {
		GetVertexBuffer(vcount);
		GetNormalBuffer(vcount);
		if (cbuf == null || cbuf.Length != vcount) {
			FillColors(Color.white);
		}
		GetTextureBuffer();
		return vbuf;
	}
	
	public Vector3[] GetVertexBuffer(int count) 
	{
		Array.Resize(ref vbuf, count);
		return vbuf;
	}
	
	public Vector3[] GetNormalBuffer(int count) 
	{
		Array.Resize(ref nbuf, count);
		return nbuf;
	}
	
	public int[] GetIndexBuffer(int triangleCount) 
	{
		Array.Resize(ref tbuf, 3 * triangleCount);
		return tbuf;
	}
	
	public Color32[] GetColorBuffer(int count) 
	{
		Array.Resize(ref cbuf, count);
		return cbuf;
	}
	
	public void FillNormals(Vector3 n) 
	{
		var nb = GetNormalBuffer(vbuf.Length);
		for(int i=0; i<nb.Length; ++i) {
			nb[i] = n;
		}
	}
	
	public void FillColors(Color32 c) 
	{
		var cb = GetColorBuffer(vbuf.Length);
		for(int i=0; i<cb.Length; ++i) {
			cb[i] = c;
		}
	}
	
	public void PlotTriangleStrip(int vertexCount) 
	{
		var t = GetIndexBuffer(vertexCount-2);
		for(int i=0; i<vertexCount-2; ++i) {
			t[3*i] = i;
			t[3*i+1] = i%2 == 0 ? i+1 : i+2;
			t[3*i+2] = i%2 == 0 ? i+2 : i+1;
		}
	}
	
	public Vector2[] GetTextureBuffer() {
		if (uv == null) {
			uv = new Vector2[vbuf.Length];
			for(int i=0; i<uv.Length; ++i) {
				uv[i] = Vector2.zero;
			}
		}
		return uv;
	}
	
	public void SetRepeatingStripTexture(int repeatU, int repeatV) {
		GetTextureBuffer();
		var halfLen = uv.Length/2;
		var v = 0f;
		var dv = (repeatV) / (halfLen-1f);
		for(int i=0; i<halfLen; ++i) {
			uv[i+i  ].x = repeatU;
			uv[i+i  ].y = v;
			uv[i+i+1].x = 0f;
			uv[i+i+1].y = v;
			v += dv;
		}
	}
	
	public void SetRampTexture(float vtop, float vbottom, int repeatU=1) {
		GetTextureBuffer();
		var halfLen = uv.Length/2;
		var u=0f;
		var du = repeatU / (halfLen-1f);
		for(int i=0; i<halfLen; ++i) {
			uv[i+i  ].x = u;
			uv[i+i  ].y = vtop;
			uv[i+i+1].x = u;
			uv[i+i+1].y = vbottom;
			u+=du;
		}
	}
	
	public Mesh CreateMesh(string name) 
	{
		GetTextureBuffer();
		
		var result = new Mesh() { 
			name = name,
			vertices = vbuf,
			uv = uv,
			colors32 = cbuf,
			triangles = tbuf
		};
		result.Optimize();
		return result;
	}
	
	public void ApplyToMesh(Mesh mesh) {
		GetTextureBuffer();
		
		mesh.vertices = vbuf;
		mesh.uv = uv;
		mesh.colors32 = cbuf;
		mesh.triangles = tbuf;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
}



