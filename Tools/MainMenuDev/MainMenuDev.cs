﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.VR.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VR.Tools;

[ExecuteInEditMode]
public class MainMenuDev : MonoBehaviour, IRay, IInstantiateUI, IMainMenu
{
	[SerializeField]
	private Canvas m_MainMenuPrefab;
	private Canvas m_MenuCanvas;

	private RectTransform m_Layout;
	private GameObject m_ButtonTemplate;

	public Transform rayOrigin { get; set; }

	public List<Type> menuTools { private get; set; }
	public List<Type> menuWorkspaces { private get; set; }
	public Func<IMainMenu, Type, bool> selectTool { private get; set; }
	public Action<Type> createWorkspace { private get; set; }

	public Func<GameObject, GameObject> instantiateUI { private get; set; }

	void Start()
	{
		if (m_MenuCanvas == null)
		{
			var go = instantiateUI(m_MainMenuPrefab.gameObject);
			m_MenuCanvas = go.GetComponent<Canvas>();
			m_Layout = m_MenuCanvas.GetComponentInChildren<GridLayoutGroup>().GetComponent<RectTransform>();
			m_ButtonTemplate = m_Layout.GetChild(0).gameObject;
			m_ButtonTemplate.SetActive(false);
		}
		m_MenuCanvas.transform.SetParent(rayOrigin, false);
		CreateToolButtons();
		CreateWorkspaceButtons();
	}

	void OnDestroy()
	{
		if (m_MenuCanvas != null)
			U.Object.Destroy(m_MenuCanvas.gameObject);
	}

	private void CreateToolButtons()
	{
		foreach (var menuTool in menuTools)
		{
			var newButton = U.Object.InstantiateAndSetActive(m_ButtonTemplate, m_Layout, false);
			newButton.name = menuTool.Name;
			var text = newButton.GetComponentInChildren<Text>();
			text.text = menuTool.Name;
			var button = newButton.GetComponent<Button>();
			AddToolButtonListener(button, menuTool);
		}
	}

	private void AddToolButtonListener(Button b, Type t)
	{
		b.onClick.RemoveAllListeners();
		b.onClick.AddListener(() =>
		{
			if (selectTool(this, t))
				U.Object.Destroy(this);
		});
		b.onClick.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
	}

	private void CreateWorkspaceButtons()
	{
		foreach (var menuWorkspace in menuWorkspaces)
		{
			var newButton = U.Object.InstantiateAndSetActive(m_ButtonTemplate, m_Layout, false);
			newButton.name = ObjectNames.NicifyVariableName(menuWorkspace.Name);
			var text = newButton.GetComponentInChildren<Text>();
			text.text = newButton.name;
			var button = newButton.GetComponent<Button>();
			AddWorkspaceButtonListener(button, menuWorkspace);
		}
	}

	private void AddWorkspaceButtonListener(Button b, Type t)
	{
		b.onClick.RemoveAllListeners();
		b.onClick.AddListener(() =>
		{
			//HACK: Workspace.m_BasePrefab is coming up null when we call CreateWorkspace from this context.
			EditorApplication.delayCall += () =>
			{
				createWorkspace(t);
				U.Object.Destroy(this);
			};
		});
		b.onClick.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
	}
}