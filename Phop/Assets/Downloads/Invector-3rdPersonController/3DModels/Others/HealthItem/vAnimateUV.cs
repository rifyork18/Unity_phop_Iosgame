using UnityEngine;
using System.Collections;

public class vAnimateUV : MonoBehaviour
{
	public Vector2 speed;
	public Renderer _renderer;
    
	private Vector2 offSet;
	
	void Update ()
    {		
		offSet.x += speed.x * Time.deltaTime;
		offSet.y += speed.y * Time.deltaTime;
		_renderer.material.SetTextureOffset ("_MainTex", offSet);
	}
}
