﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {

	public ChunkCoord coord;
	GameObject chunkObject;
	public MeshRenderer meshRenderer;
	public MeshFilter meshFilter;


	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	List<Vector2> uvs = new List<Vector2>();

	byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
	World world; 


    public Chunk(World _world, ChunkCoord _coord)
    {
		coord = _coord;
		chunkObject = new GameObject();
		chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);

		meshRenderer = chunkObject.AddComponent<MeshRenderer>();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		world = _world;

		chunkObject.transform.SetParent(world.transform);
		meshRenderer.material = world.material;

		chunkObject.name = coord.x + ", " + coord.z;

		PopulateVoxelMap();
		CreateMeshData();
		CreateMesh();
	}

	public bool IsActive
	{
		get { return chunkObject.activeSelf; }
		set { chunkObject.SetActive(value); }
	}

	public Vector3 position
    {
		get { return chunkObject.transform.position; }
    }

	bool IsVoxelInChunk(int x, int y, int z)
	{
		if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
			return false;
		else
			return true;
	}

	void PopulateVoxelMap ()
    {
		for (int y = 0; y < VoxelData.ChunkHeight; y++)
		{
			for (int x = 0; x < VoxelData.ChunkWidth; x++)
			{
				for (int z = 0; z < VoxelData.ChunkWidth; z++)
				{
					voxelMap[x,y,z] = world.GetVoxel(new Vector3(x, y, z) + position);
					
				}
			}
		}
	}

	void CreateMeshData()
    {
		for (int y = 0; y < VoxelData.ChunkHeight; y++)
		{
			for (int x = 0; x < VoxelData.ChunkWidth; x++)
			{
				for (int z = 0; z < VoxelData.ChunkWidth; z++)
				{
					AddVoxelDataToChunk(new Vector3(x, y, z));
				}
			}
		}
	}

	bool CheckVoxel(Vector3 pos)
    {
		int x = Mathf.FloorToInt(pos.x);
		int y = Mathf.FloorToInt(pos.y);
		int z = Mathf.FloorToInt(pos.z);


		if (!IsVoxelInChunk(x, y, z))
			return world.blockTypes[world.GetVoxel(pos + position)].isSolid;

		return world.blockTypes[voxelMap[x, y, z]].isSolid;
	}

	void AddVoxelDataToChunk(Vector3 pos)
	{
		for (int p = 0; p < 6; p++)
		{
			//for every face check if face needs to be drawn by
			//looking at the adjacent voxel and whether it is solid/there
			if (!CheckVoxel(pos + VoxelData.faceChecks[p]))
			{
				byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                AddTexture(world.blockTypes[blockID].GetTextureID(p));

				triangles.Add(vertexIndex);
				triangles.Add(vertexIndex + 1);
				triangles.Add(vertexIndex + 2);
				triangles.Add(vertexIndex + 2);
				triangles.Add(vertexIndex + 1);
				triangles.Add(vertexIndex + 3);
				vertexIndex += 4;

			}
		}
	}

	void CreateMesh(){

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();

		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

	void AddTexture (int textureID)
    {
		float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
		float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

		x *= VoxelData.NormalizedBlockTextureSize;
		y *= VoxelData.NormalizedBlockTextureSize;

		y = 1f - y - VoxelData.NormalizedBlockTextureSize;


		uvs.Add(new Vector2(x, y));
		uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
		uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
		uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
	}

}

public class ChunkCoord
{
	public int x;
	public int z;

    public ChunkCoord(int _x, int _z) {
		x = _x;
		z = _z;
	}

	public bool Equals (ChunkCoord other)
    {
		if (other == null)
			return false;
		else if (other.x == x && other.z == z)
			return true;
		else
			return false;
    }


}
