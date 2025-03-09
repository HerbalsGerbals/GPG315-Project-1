using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

public class SeedItemEditor : EditorWindow
{
    private static List<SeedItem> m_seedList = new List<SeedItem>();
    private VisualElement m_ItemsTab;
    private static VisualTreeAsset m_ItemRowTemplate;
    private ScrollView m_DetailSection;

    private ListView m_ItemListView;
    private Sprite m_DefaultItemIcon;
    private SeedItem m_activeItem;
    private VisualElement m_LargeDisplayIcon;

    private int m_ItemHeight = 60;

    [MenuItem("Window/UI Toolkit/SeedItemEditor")]
    public static void SeedWindow()
    {
        SeedItemEditor wnd = GetWindow<SeedItemEditor>();
        wnd.titleContent = new GUIContent("Seed Item Editor");

        Vector2 size = new Vector2(800, 475);
        wnd.minSize = size;
        wnd.maxSize = size;
    }

    public void CreateGUI()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/SeedItemEditor.uxml");
        VisualElement rootFromUXML = visualTree.Instantiate();
        rootVisualElement.Add(rootFromUXML);

        m_ItemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ItemRowTemplate.uxml");

        //Get references for later.
        m_DetailSection = rootVisualElement.Q<ScrollView>("ScrollView_Details");

        //Load all existing item assets.
        LoadAllSeeds();

        //Populate the listview.
        m_ItemsTab = rootVisualElement.Q<VisualElement>("ItemsTab");
        GenerateSeedView();

        //Allows for Smoother experience.
        m_DetailSection = rootVisualElement.Q<ScrollView>("ScrollView_Details");
        m_DetailSection.style.visibility = Visibility.Hidden;

        //Hook up button click events
        rootVisualElement.Q<Button>("Btn_AddItem").clicked += AddItem_OnClick;
        rootVisualElement.Q<Button>("Btn_DeleteItem").clicked += DeleteItem_OnClick;
    }

    //Looks through all already created Scriptable Object in Items folder and adds them to a list.
    private void LoadAllSeeds()
    {
        m_seedList.Clear();

        string[] allPaths = Directory.GetFiles("Assets/Items", "*.asset", SearchOption.AllDirectories);

        foreach (string path in allPaths)
        {
            string cleanedPath = path.Replace("\\", "/");
            m_seedList.Add((SeedItem)AssetDatabase.LoadAssetAtPath(cleanedPath, typeof(SeedItem)));
        }
    }

    //Adds list of already created Scriptable Object to UI
    private void GenerateSeedView()
    {
        Func<VisualElement> makeItem = () => m_ItemRowTemplate.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            e.Q<Label>("Name").text = m_seedList[i].seedName;
        };

        m_ItemListView = new ListView(m_seedList, 35, makeItem, bindItem);
        m_ItemListView.selectionType = SelectionType.Single;
        m_ItemListView.style.height = m_seedList.Count * m_ItemHeight;
        m_ItemsTab.Add(m_ItemListView);

        m_ItemListView.selectionChanged += ListView_onSelectionChange;
    }
    
    //Binds objects to children
    private void ListView_onSelectionChange(IEnumerable<object> selectedItems)
    {
        m_activeItem = (SeedItem)selectedItems.First();

        SerializedObject so = new SerializedObject(m_activeItem);
        m_DetailSection.Bind(so);
        m_DetailSection.style.visibility = Visibility.Visible;
    }


   //Add a new Item asset to the Items folder.
    private void AddItem_OnClick()
    {
        //Create an instance of the scriptable object
        SeedItem newItem = CreateInstance<SeedItem>();
        newItem.seedName = $"New Item";
    

        //Create the asset 
        AssetDatabase.CreateAsset(newItem, $"Assets/Items/{newItem.ID}.asset");

        //Add it to the item list
        m_seedList.Add(newItem);

        //Refresh the ListView so everything is redrawn again
        m_ItemListView.Rebuild();
        m_ItemListView.style.height = m_seedList.Count * m_ItemHeight + 5;
    }

    //Delete a Item asset from the Items folder. 
    private void DeleteItem_OnClick()
    {
        //Get the path of the fie and delete it through AssetDatabase
        string path = AssetDatabase.GetAssetPath(m_activeItem);
        AssetDatabase.DeleteAsset(path);

        //Purge the reference from the list and refresh the ListView
        m_seedList.Remove(m_activeItem);
        m_ItemListView.Rebuild();

        //Nothing is selected, so hide the details section
        m_DetailSection.style.visibility = Visibility.Hidden;

    }
}
