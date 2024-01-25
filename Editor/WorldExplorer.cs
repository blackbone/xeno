using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using Xeno;

internal sealed class WorldExplorer : EditorWindow
{
    [MenuItem("Window/Xeno/World Explorer")]
    public static void Open()
    {
        var window = GetWindow<WorldExplorer>();
        window.titleContent = new GUIContent("World Explorer");
        window.minSize = new Vector2(400, 300);
    }
    
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
    [SerializeField] private VisualTreeAsset m_SystemListElementTemplate = default;

    [SerializeField] private string systemFilterText = "";
    [SerializeField] private List<TreeViewItemData<SystemData>> systems;

    private SerializedObject serializedObject;
    private SerializedProperty systemsProperty;
    private int activeWorldId = -1;
    private MultiColumnTreeView systemsTreeView;

    private void Update()
    {
        
    }

    public WorldExplorer()
    {
        ListPool<TreeViewItemData<SystemData>>.Get(out systems);
        
        systems.Add(new TreeViewItemData<SystemData>(1, new SystemData { enabled = true, name = "System1", time = 1.2221f}));
        systems.Add(new TreeViewItemData<SystemData>(2,new SystemData { enabled = true, name = "System2", time = 1.2221f}));
        systems.Add(new TreeViewItemData<SystemData>(3, new SystemData { enabled = true, name = "System3", time = 1.2221f}));
        systems.Add(new TreeViewItemData<SystemData>(4, new SystemData { enabled = true, name = "System4", time = 1.2221f}));
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        systemsProperty = serializedObject.FindProperty(nameof(systems));
    }

    private void OnDestroy()
    {
        serializedObject.ApplyModifiedProperties();
        serializedObject.Dispose();
    }

    ~WorldExplorer()
    {
        ListPool<TreeViewItemData<SystemData>>.Release(systems);
        systems = null;
    }

    public void CreateGUI()
    {
        var tree = m_VisualTreeAsset.Instantiate();
        rootVisualElement.Add(tree);
        tree.Bind(serializedObject);
        
        systemsTreeView = tree.Q<MultiColumnTreeView>("systems");

        var menu = tree.Q<ToolbarMenu>("world");
        menu.variant = ToolbarMenu.Variant.Popup;
        menu.menu.ClearItems();
        // menu.menu.ClearHeaderItems();

        menu.menu.AppendAction("[None]", OnWorldSelected, CheckWorldDropdownStatus, -1);
        foreach (var world in Worlds.All())
            menu.menu.AppendAction(world.ToString(), OnWorldSelected, CheckWorldDropdownStatus, world.Id);

        PrepareTreeView(systemsTreeView, systems, 
            (nameof(SystemData.enabled), Make<Toggle>),
            (nameof(SystemData.name), Make<Label>),
            (nameof(SystemData.time), Make<Label>));
        rootVisualElement.schedule.Execute(OnActiveWorldChanged);
    }

    private static T Make<T>() where T : VisualElement, new() => new();

    private void PrepareTreeView<T>(MultiColumnTreeView treeView, IList<TreeViewItemData<T>> rootItems, params (string propertyName, Func<VisualElement> makeCell)[] columns)
    {
        foreach (var (columnName, makeCell) in columns)
        {
            var column = treeView.columns[columnName];
            if (column == null) continue;

            column.title = ObjectNames.NicifyVariableName(columnName);
            column.makeCell = makeCell;
            column.bindCell = (v, i) => {
                var data = treeView.GetItemDataForIndex<T>(i);
                v.SetBinding("value", new DataBinding { dataSource = data, dataSourcePath = PropertyPath.FromName(columnName)});
            };
        }

        treeView.SetRootItems(rootItems);
    }

    private DropdownMenuAction.Status CheckWorldDropdownStatus(DropdownMenuAction action)
    {
        var worldId = (int)action.userData;
        return activeWorldId == worldId ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;
    }

    private void OnWorldSelected(DropdownMenuAction action)
    {
        var worldId = (int)action.userData;
        activeWorldId = worldId;
        OnActiveWorldChanged();
    }

    private void OnActiveWorldChanged()
    {
        if (activeWorldId == -1)
            return;
        
        if (!Worlds.TryGet((byte)activeWorldId, out var world))
            return;
    }

    [Serializable]
    private class SystemData
    {
        public bool enabled;
        public string name;
        public float time;
    }
}
