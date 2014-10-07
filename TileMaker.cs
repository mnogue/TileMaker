
using UnityEngine;
using UnityEditor;
using System.Collections;

public class TileMaker : EditorWindow
{
	public string name      = "tilemap-id";
	private string savePath = "Assets/Models/TileMaker/";
	public int mapWidth     = 10;
	public int mapHeight    = 10;
	public int mapNumTilesX = 2;
	public int mapNumTilesY = 2;
	
	public Material material;
	public int textureNumTilesX = 4;
	public int textureNumTilesY = 4;
	
	private Vector2[,] previewThumbsIndex = new Vector2[0,0];  // index to textureThumbs
	private Texture2D[] textureThumbs = new Texture2D[0];      // contains thumbnails previews of the texture tiles
	
	private int meshWidth  = 1;
	private int meshHeight = 1;
	private int meshQuadsX = 1;
	private int meshQuadsY = 1;
	
	private int textureNumTilesXDefined = 4;
	private int textureNumTilesYDefined = 4;
	
	private Mesh mesh = null;
	private Vector2[] meshUVs = new Vector2[0];
	
	// painting:
	private int brushIndex = 0;
	private Vector2 scrollPosition = Vector2.zero;
	
	// MAIN METHODS: ---------------------------------------------------------------------------------------------------
	
	[MenuItem ("Window/New TileMap...")]
	public static void ShowWindow () 
	{
		EditorWindow.GetWindow(typeof(TileMaker));
	}
	
	void OnGUI()
	{
		EditorGUILayout.Space();
		this.name = EditorGUILayout.TextField("Map name", this.name);
		EditorGUILayout.Space();
		
		int oldMapWidth  = this.mapWidth;
		int oldMapHeight = this.mapHeight;
		this.mapWidth  = EditorGUILayout.IntField("Map width", this.mapWidth);
		this.mapHeight = EditorGUILayout.IntField("Map height", this.mapHeight);
		
		EditorGUILayout.Space();
		
		int oldMapNumTilesX = this.mapNumTilesX;
		int oldMapNumTilesY = this.mapNumTilesY;
		this.mapNumTilesX = EditorGUILayout.IntField("Num tiles X", this.mapNumTilesX);
		this.mapNumTilesY = EditorGUILayout.IntField("Num tiles Y", this.mapNumTilesY);
		
		if (oldMapWidth != this.mapWidth || oldMapHeight != this.mapHeight ||
		    oldMapNumTilesX != this.mapNumTilesX || oldMapNumTilesY != this.mapNumTilesY)
		{
			SceneView.RepaintAll();
		}
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		this.material = (Material)EditorGUILayout.ObjectField(this.material, typeof(Material), false);
		this.textureNumTilesX = EditorGUILayout.IntField("Num tiles Horizontal", this.textureNumTilesX);
		this.textureNumTilesY = EditorGUILayout.IntField("Num tiles Vertical",   this.textureNumTilesY);
		EditorGUILayout.Space();
		
		if (GUILayout.Button("Create Mesh"))
		{
			if (this.material)
			{
				this.CreateMesh();
			}
		}
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		if (this.mesh != null && this.previewThumbsIndex.GetLength(0) > 0 && this.previewThumbsIndex.GetLength(1) > 0)
		{
			EditorGUILayout.LabelField("Brushes:");

			this.brushIndex = GUILayout.SelectionGrid(this.brushIndex, this.textureThumbs, 8);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Map:");
			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
			EditorGUILayout.BeginVertical();
			
			for (int y = 0; y < this.meshQuadsY; ++y)
			{
				EditorGUILayout.BeginHorizontal();
				
				for (int x = 0; x < this.meshQuadsX; ++x)
				{
					int index = (int)this.previewThumbsIndex[x,y].y * this.meshQuadsX + (int)this.previewThumbsIndex[x,y].x;
					Texture2D thumb = this.textureThumbs[index];
					
					if (GUILayout.Button(thumb)) this.PaintUV(x, y);
				}
				
				EditorGUILayout.EndHorizontal();
			}
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// CREATE TILE MAP: ------------------------------------------------------------------------------------------------
	
	private void CreateMesh()
	{
		Mesh m = new Mesh();
		
		int hCount2 = (this.mapNumTilesX * 2);
		int vCount2 = (this.mapNumTilesY * 2);
		int numTriangles = this.mapNumTilesX * this.mapNumTilesY * 6;
		int numVertices = hCount2 * vCount2;
		
		Vector3[] vertices = new Vector3[numVertices];
		Vector3[] normals  = new Vector3[numVertices];
		Vector2[] uvs = new Vector2[numVertices];
		int[] triangles = new int[numTriangles];
		
		int index = 0;
		float uvFactorX = 1.0f/(float)this.textureNumTilesX;
		float uvFactorY = 1.0f/(float)this.textureNumTilesY;
		float scaleX = (float)this.mapWidth/(float)this.mapNumTilesX;
		float scaleY = (float)this.mapHeight/(float)this.mapNumTilesY;
		
		for (int y = 0; y < this.mapNumTilesY; y++)
		{
			for (int x = 0; x < this.mapNumTilesX; x++)
			{
				vertices[index] = new Vector3(
					x*scaleX - (float)this.mapWidth/2f, 
					0.0f, 
					y*scaleY - (float)this.mapHeight/2f
				);
				
				normals[index] = Vector3.up;
				uvs[index++] = new Vector2(0.0f, 0.0f);
				
				vertices[index] = new Vector3(
					(x+1.0f)*scaleX - (float)this.mapWidth/2f, 
					0.0f, 
					y*scaleY - (float)this.mapHeight/2f
				);
				
				normals[index] = Vector3.up;
				uvs[index++] = new Vector2(uvFactorX, 0.0f);
				
				vertices[index] = new Vector3(
					x*scaleX - (float)this.mapWidth/2f, 
					0.0f, 
					(y+1.0f)*scaleY - (float)this.mapHeight/2f
				);
				
				normals[index] = Vector3.up;
				uvs[index++] = new Vector2(0.0f, uvFactorY);
				
				vertices[index] = new Vector3(
					(x+1.0f)*scaleX - (float)this.mapWidth/2f, 
					0.0f, 
					(y+1.0f)*scaleY - (float)this.mapHeight/2f
				);
				
				normals[index] = Vector3.up;
				uvs[index++] = new Vector2(uvFactorX, uvFactorY);
			}
		}
		
		index = 0;
		int vertexIndex = 0;
		
		for (int i = 0; i < this.mapNumTilesX * this.mapNumTilesY; ++i)
		{
			triangles[index]   = vertexIndex+0;
			triangles[index+1] = vertexIndex+2;
			triangles[index+2] = vertexIndex+3;
			
			triangles[index+3] = vertexIndex+0;
			triangles[index+4] = vertexIndex+3;
			triangles[index+5] = vertexIndex+1;
			index += 6;
			vertexIndex += 4;
		}
		
		m.vertices = vertices;
		m.uv = uvs;
		m.triangles = triangles;
		m.normals = normals;
		
		this.meshUVs = uvs;
		this.meshWidth  = this.mapWidth;
		this.meshHeight = this.mapHeight;
		this.meshQuadsX = this.mapNumTilesX;
		this.meshQuadsY = this.mapNumTilesY;
		
		this.textureNumTilesXDefined = this.textureNumTilesX;
		this.textureNumTilesYDefined = this.textureNumTilesY;
		
		AssetDatabase.CreateAsset(m, this.savePath + (this.name + ".asset"));
		AssetDatabase.SaveAssets();
		m.RecalculateBounds();
		
		this.previewThumbsIndex = new Vector2[this.mapNumTilesX, this.mapNumTilesY];
		for (int y = 0; y < this.meshQuadsY; ++y)
		{
			for (int x = 0; x < this.meshQuadsX; ++x)
			{
				this.previewThumbsIndex[x,y] = new Vector2(0.0f, 0.0f);
			}
		}
		
		this.textureThumbs = new Texture2D[this.textureNumTilesX * this.textureNumTilesY];
		Texture2D texture = (Texture2D)this.material.GetTexture("_MainTex");
		
		for (int y = 0; y < this.textureNumTilesYDefined; ++y)
		{
			for (int x = 0; x < this.textureNumTilesXDefined; ++x)
			{
				int offsetX = texture.width  / this.textureNumTilesXDefined;
				int offsetY = texture.height / this.textureNumTilesYDefined;
				
				int startX = offsetX * x;
				int startY = offsetY * y;
				
				Color[] colors = texture.GetPixels(startX, startY, offsetX, offsetY);
				Texture2D thumb = new Texture2D(offsetX, offsetY);
				
				thumb.SetPixels(colors);
				thumb.Apply();
				this.textureThumbs[(y * this.textureNumTilesXDefined) + x] = this.ScaleTexture(thumb, 32, 32);
			}
		}
		
		this.mesh = m;
	}
	
	/*
	private void CreateTileMap()
	{
		GameObject tilemap = new GameObject(this.name);
		
		if (!string.IsNullOrEmpty(this.name)) tilemap.name = this.name;
		else tilemap.name = "DefaultName";
		
		tilemap.transform.position = Vector3.zero;
		
		MeshFilter meshFilter = (MeshFilter)tilemap.AddComponent(typeof(MeshFilter));
		tilemap.AddComponent(typeof(MeshRenderer));
		
		string assetName = tilemap.name + ".asset";
		Mesh m = new Mesh();
		m.name = tilemap.name;
		
		int hCount2 = (this.mapNumTilesX * 2);
		int vCount2 = (this.mapNumTilesY * 2);
		int numTriangles = this.mapNumTilesX * this.mapNumTilesY * 6;
		int numVertices = hCount2 * vCount2;
		
		Vector3[] vertices = new Vector3[numVertices];
		Vector3[] normals  = new Vector3[numVertices];
		Vector2[] uvs = new Vector2[numVertices];
		int[] triangles = new int[numTriangles];
		
		int index = 0;
		float uvFactorX = 1.0f/(float)this.textureNumTilesX;
		float uvFactorY = 1.0f/(float)this.textureNumTilesY;
		float scaleX = (float)this.mapWidth/(float)this.mapNumTilesX;
		float scaleY = (float)this.mapHeight/(float)this.mapNumTilesY;
		
		for (int y = 0; y < this.mapNumTilesY; y++)
		{
			for (int x = 0; x < this.mapNumTilesX; x++)
			{
				vertices[index] = new Vector3(
					x*scaleX - (float)this.mapWidth/2f, 
					0.0f, 
					y*scaleY - (float)this.mapHeight/2f
				);
				
				normals[index] = Vector3.up;
				uvs[index++] = new Vector2(0.0f, 0.0f);
				
				vertices[index] = new Vector3(
					(x+1.0f)*scaleX - (float)this.mapWidth/2f, 
					0.0f, 
					y*scaleY - (float)this.mapHeight/2f
				);
				
				normals[index] = Vector3.up;
				uvs[index++] = new Vector2(uvFactorX, 0.0f);
				
				vertices[index] = new Vector3(
					x*scaleX - (float)this.mapWidth/2f, 
					0.0f, 
					(y+1.0f)*scaleY - (float)this.mapHeight/2f
				);
				
				normals[index] = Vector3.up;
				uvs[index++] = new Vector2(0.0f, uvFactorY);
				
				vertices[index] = new Vector3(
					(x+1.0f)*scaleX - (float)this.mapWidth/2f, 
					0.0f, 
					(y+1.0f)*scaleY - (float)this.mapHeight/2f
				);
				
				normals[index] = Vector3.up;
				uvs[index++] = new Vector2(uvFactorX, uvFactorY);
			}
		}
		
		index = 0;
		int vertexIndex = 0;
		
		for (int i = 0; i < this.mapNumTilesX * this.mapNumTilesY; ++i)
		{
			triangles[index]   = vertexIndex+0;
			triangles[index+1] = vertexIndex+2;
			triangles[index+2] = vertexIndex+3;
			
			triangles[index+3] = vertexIndex+0;
			triangles[index+4] = vertexIndex+3;
			triangles[index+5] = vertexIndex+1;
			index += 6;
			vertexIndex += 4;
		}
		
		m.vertices = vertices;
		m.uv = uvs;
		m.triangles = triangles;
		m.normals = normals;
		
		this.uv = uvs;
		
		AssetDatabase.CreateAsset(m, this.savePath + assetName);
		AssetDatabase.SaveAssets();
		
		meshFilter.sharedMesh = m;
		m.RecalculateBounds();
		
		tilemap.renderer.material = this.material;
		Selection.activeObject = tilemap;
		
		this.mapUVIndexes = new Vector2[this.mapNumTilesX, this.mapNumTilesY];
		for (int j = 0; j < this.mapNumTilesY; ++j)
		{
			for (int i = 0; i < this.mapNumTilesX; ++i)
			{
				this.mapUVIndexes[i,j] = new Vector2(0, 0);
			}
		}
		
		this.textureThumbs = new Texture2D[this.textureNumTilesX * this.textureNumTilesY];
		Texture2D texture = (Texture2D)this.material.GetTexture("_MainTex");
		
		for (int j = 0; j < this.textureNumTilesY; ++j)
		{
			for (int i = 0; i < this.textureNumTilesX; ++i)
			{
				int offsetX = texture.width/this.textureNumTilesX;
				int offsetY = texture.height/this.textureNumTilesY;
				
				int startX = offsetX * i;
				int startY = offsetY * j;
				
				Color[] colors = texture.GetPixels(startX, startY, offsetX, offsetY);
				Texture2D thumb = new Texture2D(offsetX, offsetY);
				thumb.SetPixels(colors);
				thumb.Apply();
				this.textureThumbs[i + (this.textureNumTilesX * j)] = this.ScaleTexture(thumb, 32, 32);
				
			}
		}
		
		this.tilemapGenerated = tilemap;
	}
	*/
	
	private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight) 
	{
		Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
		Color[] rpixels=result.GetPixels(0);
		float incX=(1.0f / (float)targetWidth);
		float incY=(1.0f / (float)targetHeight); 
		
		for(int px = 0; px < rpixels.Length; px++) 
		{ 
			rpixels[px] = source.GetPixelBilinear(incX*((float)px%targetWidth), incY*((float)Mathf.Floor(px/targetWidth))); 
		} 
		
		result.SetPixels(rpixels,0);
		result.Apply();
		return result;
	}
	
	private void PaintUV(int x, int y)
	{
		int index = 0;
		float uvFactorX = 1.0f/(float)this.textureNumTilesXDefined;
		float uvFactorY = 1.0f/(float)this.textureNumTilesYDefined;
		
		int inverseBrush = this.textureThumbs.Length - this.brushIndex;
		float uvx = uvFactorX * (int)(this.brushIndex % this.meshQuadsY);
		float uvy = uvFactorY * (int)Mathf.Floor(this.brushIndex / this.meshQuadsY);
		
		this.meshUVs[(x + (y * this.mapNumTilesX))*4+0] = new Vector2(uvx,             uvy);
		this.meshUVs[(x + (y * this.mapNumTilesX))*4+1] = new Vector2(uvx + uvFactorX, uvy);
		this.meshUVs[(x + (y * this.mapNumTilesX))*4+2] = new Vector2(uvx,             uvy + uvFactorY);
		this.meshUVs[(x + (y * this.mapNumTilesX))*4+3] = new Vector2(uvx + uvFactorX, uvy + uvFactorY);
		
		this.mesh.uv = this.meshUVs;
		
		int indexx = (int)(this.brushIndex % this.meshQuadsX);
		int indexy = (int)Mathf.Floor(this.brushIndex / this.meshQuadsX);
		this.previewThumbsIndex[x,y] = new Vector2(indexx, indexy);
	}
	
	// GIZMOS: ---------------------------------------------------------------------------------------------------------
	
	public void OnFocus() 
	{
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;
	}
	
	void OnDestroy() 
	{
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
	}
	
	void OnSceneGUI(SceneView sceneView) 
	{
		Handles.color = Color.yellow;
		Vector3 v1 = new Vector3( this.mapWidth/2.0f, 0.0f,  this.mapHeight/2.0f);
		Vector3 v2 = new Vector3( this.mapWidth/2.0f, 0.0f, -this.mapHeight/2.0f);
		Vector3 v3 = new Vector3(-this.mapWidth/2.0f, 0.0f,  this.mapHeight/2.0f);
		Vector3 v4 = new Vector3(-this.mapWidth/2.0f, 0.0f, -this.mapHeight/2.0f);
		
		Handles.DrawLine(v1, v2);
		Handles.DrawLine(v2, v4);
		Handles.DrawLine(v3, v4);
		Handles.DrawLine(v1, v3);
		
		for (int i = 0; i < this.mapNumTilesX; ++i)
		{
			float x = (-(float)this.mapWidth/2.0f) + ((float)this.mapWidth/(float)this.mapNumTilesX)*((float)i);
			
			Vector3 p1 = new Vector3(x, 0.0f, (float)this.mapHeight/2.0f);
			Vector3 p2 = new Vector3(x, 0.0f, -(float)this.mapHeight/2.0f);
			Handles.DrawDottedLine(p1, p2, 5.0f);
		}
		
		for (int i = 0; i < this.mapNumTilesY; ++i)
		{
			float y = (-(float)this.mapHeight/2.0f) + ((float)this.mapHeight/(float)this.mapNumTilesY)*((float)i);
			
			Vector3 p1 = new Vector3( (float)this.mapWidth/2.0f, 0.0f, y);
			Vector3 p2 = new Vector3(-(float)this.mapWidth/2.0f, 0.0f, y);
			Handles.DrawDottedLine(p1, p2, 5.0f);
		}
	}
}