﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DestructibleTerrainChunk : MonoBehaviour
{
    List<RLEColumn> columns;

    Texture2D loadedTexture; //Original texture loaded from spriteRenderer.
    Texture2D terrainTexture; //Texture ingame. This texture changes itself.
    Texture2D outlineTexture; //Black pixel outline around solid terrain.
    Texture2D finalTexture; //Final texture shown to the player.
    Sprite sprite; // Sprite used by SpriteRenderer.

    // Start is called before the first frame update
    void Start()
    {
        columns = new List<RLEColumn>();

        Sprite loadedSprite = GetComponent<SpriteRenderer>().sprite;
        loadedTexture = loadedSprite.texture;
        
        terrainTexture = new Texture2D(loadedTexture.width,loadedTexture.height);
        terrainTexture.filterMode = FilterMode.Point;
        terrainTexture.SetPixels(0,0,terrainTexture.width,terrainTexture.height,loadedTexture.GetPixels(0,0,loadedTexture.width,loadedTexture.height));
        terrainTexture.Apply();

        outlineTexture = new Texture2D(loadedTexture.width,loadedTexture.height);
        outlineTexture.filterMode = FilterMode.Point;
        Color[] clrs = new Color[(loadedTexture.width*loadedTexture.height)];
        for(int i = 0; i<(loadedTexture.width*loadedTexture.height);i++) clrs[i] = Color.clear;

        outlineTexture.SetPixels(clrs);

        finalTexture = new Texture2D(loadedTexture.width,loadedTexture.height);
        finalTexture.filterMode = FilterMode.Point;

        UpdateWorld();
        UpdateTexture();

        sprite = Sprite.Create(finalTexture,new Rect(0,0,finalTexture.width,finalTexture.height), new Vector2(0.5f,0.5f), loadedSprite.pixelsPerUnit);    
        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    void UpdateWorld()
    {
        if(terrainTexture!=null)
            PrepareColumns();
    }

    // Update is called once per frame
    void Update()
    {
        MouseDestruction();
       // UpdateTexture();
    }   

    //Returns true if it changed at least one pixel in texture
    public bool DestroyTerrain(int x, int y, float power)
    {
        if(x>=0 && x<terrainTexture.width && y>=0 && y<terrainTexture.height)
        {
            terrainTexture.SetPixel(x,y,terrainTexture.GetPixel(x,y) + new Color(-.05f*power,-.05f*power,-.05f*power,-0.25f*power));

            outlineTexture.SetPixel(x,y,Color.clear);
            columns[x].SingleDelRange(y);
            return true;
        }
        return false;
    }

    public void MakeOutline(int x, int y, Color outlineCol)
    {
        if(x>=0 && x<terrainTexture.width && y>=0 && y<terrainTexture.height)
        {
            if(terrainTexture.GetPixel(x,y).a<=0.2f && terrainTexture.GetPixel(x,y).a>=0.01f)
                outlineTexture.SetPixel(x,y,outlineCol);
            //outlineTexture.SetPixel(x-1,y,outlineCol);
            //outlineTexture.SetPixel(x+1,y,outlineCol);
            //outlineTexture.SetPixel(x,y+1,outlineCol);
            //outlineTexture.SetPixel(x,y-1,outlineCol);
        }
    }

    public void MouseDestruction()
    {
        if(Input.GetMouseButton(0))
        {
            Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int xPos = terrainTexture.width/2-(int)((transform.position.x-mPos.x )*sprite.pixelsPerUnit);
            int yPos = terrainTexture.height/2+(int)(-(transform.position.y-mPos.y )*sprite.pixelsPerUnit);
            //Debug.Log(mPos);
            //Debug.Log(xPos+"      "+yPos);

            //bool changed = DestroyTerrain(xPos,yPos,1f);
            
            
            int r = 3;
            bool changed = false;
            for(int i = xPos-r;i<=xPos+r;i++)
            {
                for(int j = yPos-r;j<=yPos+r;j++)
                {
                    if(i>=0 && i<terrainTexture.width && j>=0 && j<terrainTexture.height)
                    {
                        changed = DestroyTerrain(i,j,5f);
                        if(i==xPos-r || i == xPos+r || j==yPos-r || j ==yPos+r)
                            MakeOutline(i,j,Color.black); 
                    }

                    
                    
                } 

            }
            if(changed)
                UpdateTexture();
        }
    }

    void UpdateTexture()
    {
        terrainTexture.Apply();
        //UpdateOutline();


        Color[] clrs = new Color[(loadedTexture.width*loadedTexture.height)];
        Color[] oclrs = outlineTexture.GetPixels();
        Color[] tclrs = terrainTexture.GetPixels();
        int s = terrainTexture.height*terrainTexture.width;
        for(int i = 0; i<s;i++)
        {
            clrs[i] = oclrs[i].a>0?oclrs[i]:tclrs[i];
        }
        

        finalTexture.SetPixels(clrs);
        finalTexture.Apply();
    }

    void UpdateOutline()
    {
        //Create new texture and make it transparent.
        outlineTexture = new Texture2D(loadedTexture.width,loadedTexture.height);
        outlineTexture.filterMode = FilterMode.Point;
        Color[] clrs = new Color[(loadedTexture.width*loadedTexture.height)];
        for(int i = 0; i<(loadedTexture.width*loadedTexture.height);i++) clrs[i] = Color.clear;

        outlineTexture.SetPixels(clrs);
        
        /*
        foreach(RLEColumn c in columns)
        {
            foreach(Range r in c.ranges)
            {
                if(r.min!=0)
                    outlineTexture.SetPixel(c.x, r.min, Color.black);
                if(r.max!=loadedTexture.height)
                    outlineTexture.SetPixel(c.x, r.max, Color.black);
                
                for(int i=r.min; i<r.max;i++)
                {
                    
                    if(c.x>1)
                        if(!columns[c.x-1].isWithin(i)) outlineTexture.SetPixel(c.x, i, Color.black);

                    if(c.x<columns.Count-1)
                        if(!columns[c.x+1].isWithin(i)) outlineTexture.SetPixel(c.x, i, Color.black);
                        
                }
            }
        }
        */

        outlineTexture.Apply();
    }

    void PrepareColumns()
    {
        Color[] colorMap = new Color[terrainTexture.width*terrainTexture.height];
        colorMap = terrainTexture.GetPixels(0,0,terrainTexture.width,terrainTexture.height,0);
        columns.Clear();
        columns = new List<RLEColumn>();

        for(int x = 0; x<terrainTexture.width; x++)
        {
            RLEColumn c = new RLEColumn(x);
            for(int y = 0; y<terrainTexture.height;y++)
            {
                int potentialMin=y;
                int potentialMax=y-1;
                while(y<terrainTexture.height && terrainTexture.GetPixel(x,y).a>0.01f)
                {
                    y++;
                    potentialMax++;    
                }
                if(potentialMin<=potentialMax)
                {
                    c.AddRange(potentialMin, potentialMax);
                }
            }
            columns.Add(c);
        } 
    }
}
