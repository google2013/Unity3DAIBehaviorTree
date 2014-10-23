using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Game.AIBehaviorTree;
using System.Xml;
using LitJson;
#if UNITY_EDITOR
using UnityEditor;
#endif

//	BNode.cs
//	Author: Lu Zexi
//	2014-08-07


namespace Game.AIBehaviorTree
{
	//action result
	public enum ActionResult
	{
		SUCCESS,
		RUNNING,
		FAILURE,
		NONE
	}

	/// <summary>
	/// BTree node.
	/// </summary>
	public class BNode
	{
		protected string m_strType;	//type
		protected string m_strName = "node";	//name

		protected BNode m_cParent;  //父节点
		protected List<BNode> m_lstChildren = new List<BNode>();   //子节点

		public BNode()
		{
			this.m_strType = this.GetType().FullName;
			this.m_strName = this.GetType().Name;
		}

		public void ReadJson( JsonData json )
		{
			this.m_strType = json["type"].ToString();
			this.m_strName = json["name"].ToString();
			for( int i = 0 ; i<json["child"].Count ; i++ )
			{
				Type t = Type.GetType(this.m_strType);
				BNode enode = Activator.CreateInstance(t) as BNode;
				enode.ReadJson(json["child"][i]);
				enode.m_cParent = this;
				this.AddChild(enode);
			}
		}

		public JsonData WriteJson()
		{
			JsonData json = new JsonData();
			json["type"] = this.m_strType;
			json["name"] = this.m_strName;
			json["child"] = new JsonData();
			json["child"].SetJsonType(JsonType.Array);
			for(int i = 0 ; i<this.m_lstChildren.Count ; i++)
			{
				JsonData child = this.m_lstChildren[i].WriteJson();
				json["child"].Add(child);
			}
			return json;
		}

//		public JsonData WriteJsonEx()
//		{
//			JsonData json = new JsonData();
//			json["child"] = new JsonData();
//			json["child"].SetJsonType(JsonType.Array);
//			for(int i = 0 ; i<this.m_lstChildren.Count ; i++)
//			{
//				JsonData child = this.m_lstChildren[i].WriteJson();
//				json["child"].Add(child);
//			}
//			return json;
//		}

		//enter
		public virtual void OnEnter(BInput input)
		{
			//
		}
		//excute
		public virtual ActionResult Excute(BInput input)
		{
			return ActionResult.SUCCESS;
		}
		//exit
		public virtual void OnExit(BInput input)
		{
			//
		}

		//get type
		public string GetTypeName()
		{
			return this.m_strType;
		}
		//set type
		public void SetTypeName( string type )
		{
			this.m_strType = type;
		}
		//get node name
		public string GetName()
		{
			return this.m_strName;
		}
		//remove child
		public void RemoveChild( BNode node )
		{
			this.m_lstChildren.Remove(node);
		}
		//add child
		public void AddChild( BNode node )
		{
			this.m_lstChildren.Add(node);
		}
		//insert child
		public void InsertChild( BNode prenode , BNode node )
		{
			int index = this.m_lstChildren.FindIndex((a)=>{return a == prenode;});
			this.m_lstChildren.Insert(index,node);
		}
		//is contain child
		public bool ContainChild(BNode node)
		{
			return this.m_lstChildren.Contains(node);
		}

	#if UNITY_EDITOR
		//menu add decision node
		private void menu_add_callback( object arg)
		{
			BNode node = BNodeFactory.sInstance.Create((int)arg);
			this.AddChild(node);
			node.m_cParent = this;
			BTreeWin.sInstance.Repaint();
		}

		//menu delete node
		private void menu_delete_node(object arg)
		{
			if(this.m_cParent != null )
			{
				this.m_cParent.RemoveChild(this);
			}
			this.m_cParent = null;
			BTreeWin.select = null;
			BTreeWin.cur_node = null;
			BTreeWin.sInstance.Repaint();
		}

		//render
		public virtual void Render( int x ,ref int y)
		{
			Event evt = Event.current;
			if(BTreeWin.cur_node == this)
			{
				Texture2D texRed = new Texture2D(1,1);
				texRed.SetPixel(0,0,Color.blue);
				texRed.Apply();
				GUI.DrawTexture(new Rect(0,y,BTreeWin.sInstance.position.width,BTreeWin.NODE_HEIGHT), texRed);
			}

			Rect moveRect = new Rect(x,y,BTreeWin.sInstance.position.width-BTreeWin.GUI_WIDTH,5);
			bool is_move_node = false;
			if( BTreeWin.select != null && moveRect.Contains(evt.mousePosition))
			{
				is_move_node = true;
				Texture2D tex = new Texture2D(1,1);
				tex.SetPixel(0,0,Color.green);
				tex.Apply();
				GUI.DrawTexture(new Rect(x,y,BTreeWin.sInstance.position.width,2),tex);
				if(evt.button == 0 && evt.type == EventType.MouseUp)
				{
					if(this != BTreeWin.select && this.m_cParent != null)
					{
						BTreeWin.select.m_cParent.RemoveChild(BTreeWin.select);
						BTreeWin.select.m_cParent = this.m_cParent;
						this.m_cParent.InsertChild(this,BTreeWin.select);
					}
					BTreeWin.select = null;
					BTreeWin.sInstance.Repaint();
				}
			}

			Rect rect = new Rect(x,y,BTreeWin.sInstance.position.width-BTreeWin.GUI_WIDTH,BTreeWin.NODE_HEIGHT);
			if( !is_move_node && rect.Contains(evt.mousePosition) )
			{
				if(BTreeWin.select != null )
				{
					Texture2D texRed = new Texture2D(1,1);
					texRed.SetPixel(0,0,Color.red);
					texRed.Apply();
					GUI.DrawTexture(new Rect(0,y,BTreeWin.sInstance.position.width,BTreeWin.NODE_HEIGHT), texRed);
				}
				if(evt.type == EventType.ContextClick)
				{
					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("create/decisions/sequence"), false , menu_add_callback , 0);
					menu.AddItem(new GUIContent("create/decisions/selector"), false , menu_add_callback , 1);
					menu.AddItem(new GUIContent("create/decisions/parallel"), false , menu_add_callback , 2);
					menu.AddItem(new GUIContent("delete"), false , menu_delete_node ,"");
					menu.ShowAsContext();
				}
				if(evt.button == 0 && evt.type == EventType.MouseDown && this != BTreeWin.cur_tree.m_cRoot)
				{
					BTreeWin.select = this;
					BTreeWin.cur_node = this;
				}
				if(evt.button == 0 && evt.type == EventType.MouseUp && BTreeWin.select != null)
				{
					if(this != BTreeWin.select)
					{
						BTreeWin.select.m_cParent.RemoveChild(BTreeWin.select);
						BTreeWin.select.m_cParent = this;
						this.AddChild(BTreeWin.select);
					}
					BTreeWin.select = null;
					BTreeWin.sInstance.Repaint();
				}
			}
			GUI.Label(new Rect(x,y,BTreeWin.sInstance.position.width,BTreeWin.NODE_HEIGHT),this.m_strName);

			/////////////////// line //////////////////////
			Vector3 pos1 = new Vector3(x+BTreeWin.NODE_WIDTH/2,y+BTreeWin.NODE_HEIGHT,0);
			Handles.color = Color.red;
			for( int i = 0 ; i<this.m_lstChildren.Count ; i++ )
			{
				y = y+BTreeWin.NODE_HEIGHT;

				Vector3 pos2 = new Vector3(x+BTreeWin.NODE_WIDTH/2,y+BTreeWin.NODE_HEIGHT/2,0);
				Vector3 pos3 = new Vector3(x+BTreeWin.NODE_WIDTH,y+BTreeWin.NODE_HEIGHT/2,0);
				this.m_lstChildren[i].Render(x+BTreeWin.NODE_WIDTH,ref y);
				Handles.DrawPolyLine(new Vector3[]{pos1,pos2,pos3});
			}
		}
	#endif

	}

}
