using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;


namespace StreamingPriorityTool
{
    public class EditorWindowTest : EditorWindow
    {
        private ScrollView algorithmContainer;
        private VisualTreeAsset itemTemplate;
        private RadioButtonGroup radioBtnGroup;
        private DropdownField df;
        private ScrollView sceneContainer;

        [MenuItem("Window/UI Toolkit/EditorWindowTest")]
        public static void ShowExample()
        {
            EditorWindowTest wnd = GetWindow<EditorWindowTest>();
            wnd.titleContent = new GUIContent("EditorWindowTest");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object, empty at first
            VisualElement root = rootVisualElement;

            // Import UXML
            var mainTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI/StreamingPriorityTool/Tests/EditorWindowTest.uxml");
            VisualElement ui = mainTemplate.Instantiate(); // tree "concatenation"
            root.Add(ui);
            itemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI/StreamingPriorityTool/Tests/TestItem/testItem.uxml");

            // display list of selectable scenes
            // retrieve assets of selected scenes
            // frustum/camera specs: position, rotation, near clipping plane, far clipping plane, fov
            // select sorting algorithm
            // run or request extra info for the selected algorithm (eg.: semantic priority) in another window if necessary

            #region list view test
            //Func<VisualElement> makeItem = () => new Label();
            //Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = items[i]; // "as" returns null if the convesion is not "possible"

            //const int itemHeight = 20;

            //ListView simpleLv = root.Q<ListView>("listView1");
            //for (int i = 0; i < 10; i++)
            //    simpleLv.Insert(i, new Label(i.ToString()));


            //simpleLv.fixedItemHeight = itemHeight;
            //simpleLv.makeItem = makeItem;
            //simpleLv.bindItem = bindItem;

            //simpleLv.selectionType = SelectionType.Multiple;
            //simpleLv.onItemsChosen += objs => Debug.Log(objs);
            //simpleLv.onSelectionChange += objs => Debug.Log(objs);

            //simpleLv.style.flexGrow = 1.0f;
            #endregion

            #region binding/unbinding test
            //GameObject go = Selection.activeGameObject as GameObject;
            //Label lbl = new Label();
            //lbl.bindingPath = "name";

            //root.Add(lbl);
            //if (go != null)
            //{
            //    SerializedObject so = new SerializedObject(go);
            //    lbl.Bind(so);
            //}
            //else lbl.Unbind();
            #endregion

            List<string> names = new List<string>(new[] { "camera", "diorama", "marbleMinigame", "ristorante", "arcade" });

            //radioBtnGroup = new RadioButtonGroup("Scenes", names);

            algorithmContainer = root.Q<ScrollView>("algorithmContainer");
            sceneContainer = root.Q<ScrollView>("sceneContainer");
            var btn = root.Q<Button>("btn1");
            df = new DropdownField(names, 0);

            List<SceneRep> scenes = ScenesExample();
            foreach (SceneRep scene in scenes)
            {
                Debug.Log("helo");
                Toggle toggle = new Toggle(scene.sceneName);
                toggle.bindingPath = "isSelected";
                SerializedObject so = new SerializedObject(scene);
                toggle.Bind(so);

                sceneContainer.Add(toggle);
            }
            Debug.Log($"# children: {sceneContainer.childCount}");
            //foreach (SceneRep scene in ScenesExample())
            //{
            //    Debug.Log(scene.sceneName);

            //    SerializedObject sceneSerialized = new SerializedObject(scene);
                
            //    RadioButton radioBtn = new RadioButton();
            //    radioBtn.bindingPath = "isSelected";
            //    radioBtn.Bind(sceneSerialized);
            //    radioBtn.text = scene.sceneName;

            //    //Label radioBtnLabel = radioBtn.Q<Label>();
            //    //radioBtnLabel.bindingPath = "sceneName";
            //    //radioBtnLabel.Bind(sceneSerialized);
                
            //    radioBtnGroup.Add(radioBtn);
            //    // how do i use binding
            //    // do i use bindings to keep track of simple toggles?
            //    // what if i have to bind an element of a list?
            //} 
            //sceneContainer.Add(radioBtnGroup);
            algorithmContainer.Add(df);
            btn.clicked += SelectedAlgorithm;
        }

        private void AddListItem()
        {
            var itemUi = itemTemplate.Instantiate();
            var itemLbl = itemUi.Q<Label>("message");
            var itemBtn = itemUi.Q<Button>("btn");

            itemLbl.text = System.Guid.NewGuid().ToString();
            itemBtn.clicked += () => itemUi.RemoveFromHierarchy();
            //container.Add(itemUi);
        }

        private List<SceneRep> ScenesExample()
        {
            List<string> names = new List<string>(new[] { "camera", "diorama", "marbleMinigame", "ristorante", "arcade" });
            List<SceneRep> sceneReps = new List<SceneRep>();

            int i = 0;
            foreach(string name in names)
            {
                SceneRep sceneRep = ScriptableObject.CreateInstance("SceneRep") as SceneRep;
                sceneRep.sceneName = name;
                sceneRep.isSelected = ++i % 2 == 0;
                sceneReps.Add(sceneRep);
            }

            return sceneReps;
        }

        private void SelectedAlgorithm()
        {
            //Debug.Log($"Group index value: {radioBtnGroup.value}");         // potrei utilizzare un enum con corrispondenza numero -> algoritmo
            //Debug.Log($"Value: {radioBtnGroup.choices.ElementAt(radioBtnGroup.value)}");

            //.Where(x => (x as Toggle).value)
            foreach (VisualElement val in sceneContainer.Children())
            {
                Toggle toggle = val as Toggle;
                Debug.Log($"Toggle: {toggle.label} | {toggle.value}");
            }

            Debug.Log($"Dropdown: {df.choices.ElementAt(df.index)}");
        }
    }
}