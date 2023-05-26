using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using System.Linq;

namespace StreamingPriorityTool
{
    [ExecuteInEditMode]
    public class StreamingPriorityController : EditorWindow
    {
        private VisualElement mainContainer;
        private VisualElement sceneSelectionContainer;
        private VisualElement entryPointSelectionContainer;
        private VisualElement frustumDetailsContainer;
        private VisualElement algorithmContainer;
        private VisualElement exceptionAssetsContainer;
        private VisualElement exceptionAssetsSearchContainer;
        private ScrollView sceneScrollView;
        private ScrollView exceptedAssetsSl;
        private ListView exceptionAssetslv;
        private DropdownField entryPointDropdown;
        private DropdownField algorithmDropdown;
        private TextField searchScene;
        private TextField exceptionAssetsSearchTf;

        private GameObject chosenEntryPoint = null;

        public static Settings.SortingAlgorithm chosenAlgorithm;
        [MenuItem("Window/UI Toolkit/StreamingPriority")]
        public static void DisplayWindow()
        {
            StreamingPriorityController wnd = GetWindow<StreamingPriorityController>();
            wnd.titleContent = new GUIContent("Asset Streaming Priority");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // Import UXML
            var mainTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI/StreamingPriorityTool/Views/streamingPriority.uxml");
            VisualElement ui = mainTemplate.Instantiate();
            root.Add(ui);

            #region uxml bindings

            #region containers
            mainContainer = root.Q<VisualElement>(nameof(mainContainer));
            sceneSelectionContainer = root.Q<VisualElement>(nameof(sceneSelectionContainer));
            entryPointSelectionContainer = root.Q<VisualElement>(nameof(entryPointSelectionContainer));
            frustumDetailsContainer = root.Q<VisualElement>(nameof(frustumDetailsContainer));
            algorithmContainer = root.Q<VisualElement>(nameof(algorithmContainer));
            exceptionAssetsContainer = root.Q<VisualElement>(nameof(exceptionAssetsContainer));
            exceptionAssetsSearchContainer = root.Q<VisualElement>(nameof(exceptionAssetsSearchContainer));
            #endregion

            sceneScrollView = root.Q<ScrollView>(nameof(sceneScrollView));
            //exceptionAssetsSl = root.Q<ScrollView>(nameof(exceptionAssetsSl));
            exceptedAssetsSl = root.Q<ScrollView>(nameof(exceptedAssetsSl));
            entryPointDropdown = root.Q<DropdownField>(nameof(entryPointDropdown));
            algorithmDropdown = root.Q<DropdownField>(nameof(algorithmDropdown));
            searchScene = root.Q<TextField>(nameof(searchScene));
            exceptionAssetslv = root.Q<ListView>(nameof(exceptionAssetslv));
            exceptionAssetsSearchTf = root.Q<TextField>(nameof(exceptionAssetsSearchTf));
            //exceptedAssetsLv = root.Q<ListView>(nameof(exceptedAssetsLv));

            Button sortBtn = root.Q<Button>(nameof(sortBtn));
            Button paintBtn = root.Q<Button>(nameof(paintBtn));
            Button searchSceneBtn = root.Q<Button>(nameof(searchSceneBtn));
            Button exceptionAssetsSearchBtn = root.Q<Button>(nameof(exceptionAssetsSearchBtn));
            Button exceptionAssetsBtn = root.Q<Button>(nameof(exceptionAssetsBtn));
            Button exceptedAssetsRemoveBtn = root.Q<Button>(nameof(exceptedAssetsRemoveBtn));
            #endregion

            sortBtn.clicked += Sort;
            paintBtn.clicked += PaintGray;
            searchSceneBtn.clicked += SearchScene;
            exceptionAssetsBtn.clicked += PopulateExceptedAssets;
            exceptionAssetsSearchBtn.clicked += SearchAssets; 
            exceptedAssetsRemoveBtn.clicked += RemoveExceptions;
            PopulateSceneList("StreamingPriorityTool");
            PopulateAlgorithms();
        }

        /**
         * Adds a radio button for each existing scene that contains the filter in its name.
         * When clicked the radiobutton corresponding to a scene sends an event that asks the editor to load such scene
         */
        private void PopulateSceneList(string filter = null) // toggles
        {
            // il tipo utilizzato e' solo un mock, verra' sostituito da una migliore rappresentazione di una scena
            List<string> scenes = filter == null ? SceneRetriever.ListScenes() : SceneRetriever.ListScenes(filter);

            sceneScrollView.Clear();
            foreach (string scene in scenes)
            {
                RadioButton toggle = new RadioButton(Path.GetFileNameWithoutExtension(scene));
                toggle.RegisterCallback<ChangeEvent<bool>, string>(OnSceneRadioChanged, scene);
                sceneScrollView.Add(toggle);
            }
        }

        /**
         * Given a scene, populates a list of radiobuttons for each gameobject tagged as entryPoint found
         */
        private void PopulateEntryPoints(string scenePath) // dropdown
        {
            var assets = AssetRetriever.GetAssets(EditorSceneManager.GetActiveScene().path);
            entryPointSelectionContainer.Clear();
            List<GameObject> entryPoints = assets.Where((asset) => asset.CompareTag("entryPoint")).ToList();

            foreach (GameObject entryPoint in entryPoints)
            {
                RadioButton toggle = new RadioButton(entryPoint.name);
                toggle.RegisterCallback<ChangeEvent<bool>, GameObject>(OnEntryPointRadioChanged, entryPoint);
                entryPointSelectionContainer.Add(toggle);
            }
        }
         
        
        /**
         * Populates a dropdown list with elements representing every policy implemented.
         */
        private void PopulateAlgorithms()
        {
            List<string> algorithms = new List<string>();
            foreach(string algo in Enum.GetNames(typeof(Settings.SortingAlgorithm)))
                algorithms.Add(algo);
            
            algorithmDropdown = new DropdownField(algorithms, 0);
            algorithmContainer.Insert(1, algorithmDropdown);
            algorithmDropdown.RegisterCallback<ChangeEvent<string>>(OnAlgorithmChanged);
        }


        /**
         * Populates a listview of elements representing every asset in the current active scene
         */
        private void PopulateSceneAssets(string filter = null)
        {
            exceptionAssetslv.bindItem = null;
            exceptionAssetslv.itemsSource = null;
            exceptionAssetslv.makeItem = null;
            var assets = AssetRetriever.GetAssets(EditorSceneManager.GetActiveScene().path);
            if (assets == null) Debug.Log("ASSETS NULL1"); else if (assets.Count == 0) Debug.Log("ASSETS EMPTY1"); 
            if (filter != null)
            {
                Debug.Log("No filter used");
                assets = assets.Where(asset => asset.name.Contains(filter)).ToList();
            }

            Func<VisualElement> makeItem = () => new Label();
            Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = assets[i].name;
            if (assets == null) Debug.Log("ASSETS NULL1"); else if (assets.Count == 0) Debug.Log("ASSETS EMPTY2");

            exceptionAssetslv.itemsSource = assets;
            exceptionAssetslv.makeItem = makeItem;
            exceptionAssetslv.bindItem = bindItem;
        }

        /**
         * Populate a scrollview by appending labels representing every selection made in the asset list. Such elements represent a priority queue for the sorting policy that will be used
         */
        private void PopulateExceptedAssets()
        {
            foreach (object o in exceptionAssetslv.selectedItems)
            {
                if (VEContains(exceptedAssetsSl, o)) continue;

                Label el = new Label();
                el.userData = o as GameObject;
                el.text = (o as GameObject).name;

                exceptedAssetsSl.Add(el);
            }
        }

        // returns true if any children of ve has userdata equals to o, false otherwise
        private bool VEContains(VisualElement ve, object o)
        {
            foreach (VisualElement e in ve.Children())
                if (e.userData.Equals(o)) return true;
            return false;
        }

        /**
         * retrieves a copy of the current assets in the scene, the assets listed as exceptions, the selected entry point and the selected policy and applies the represented policy to the assets in the scene
         * taking care of the latter exceptions.
         */
        private void Sort()
        {
            if (chosenEntryPoint == null) 
            {
                Debug.Log("Entry point is null");
                return;
            }
            var assets = new List<GameObject>(AssetRetriever.GetAssets(EditorSceneManager.GetActiveScene().path));
            if (assets == null || assets.Count == 0)
            {
                Debug.Log("No assets to sort");
                return;
            }

            if(Enum.TryParse<Settings.SortingAlgorithm>(algorithmDropdown.value, out Settings.SortingAlgorithm algo))
            {
                PriorityAlgorithm pa = null;
                switch (algo)
                {
                    case Settings.SortingAlgorithm.ClosestFirst:
                        pa = new ClosestFirst(); 
                        break;
                    case Settings.SortingAlgorithm.ClosestFirstInView: 
                        pa = new ClosestFirstInView();
                        break;
                    case Settings.SortingAlgorithm.SphereTracing:
                        pa = new SphereTracing();
                        break;
                    case Settings.SortingAlgorithm.SphereTracingSizePriority:
                        pa = new SphereTracingSizePriority();
                        break;
                    case Settings.SortingAlgorithm.OccluderWithMotionPredictor:
                        pa = new OccluderWithMotionPredictor();
                        ((OccluderWithMotionPredictor)pa).Init(false);
                        break;
                    case Settings.SortingAlgorithm.RayTracing:
                        pa = new RayTracing();
                        Light light = FindObjectOfType<Light>();
                        Debug.Log($"LIGHT: {light.name}");
                        ((RayTracing)pa).Init(light, 1f); // let 1 light be on the scene
                        break;
                }
                Settings.SelectedAlgorithm = algo;
                var excepted = RetrieveExceptedAssets();
                _ = pa.Sort(assets, chosenEntryPoint, excepted ?? new List<GameObject>());
                exceptedAssetsSl.Clear();
            }
        }

        /**
         * [only for testing purposes]
         * tries to paint every asset gray
         */
        private void PaintGray()
        {
            Debug.Log($"MESHES: {AssetRetriever.GetRenderers(SceneManager.GetActiveScene()).Count()}");

            //var assets = AssetRetriever.GetAssets(EditorSceneManager.GetActiveScene().path);
            //if (assets.IsNullOrEmpty()) return;
            //foreach(var asset in assets)
            //{
            //    if (asset.TryGetComponent<Renderer>(out Renderer rend))
            //        rend.material.color = Color.gray;
            //}
            //EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        /**
         * retrieves the search value and applies it as a filter to look for the set of scene where each scene contains the filter in their name
         */
        private void SearchScene()
        {
            string src = searchScene.value;
            PopulateSceneList(src);
        }


        /**
        * retrieves the search value and applies it as a filter to look for the set of scene where each scene contains the filter in their name
        */
        private void SearchAssets()
        {
            string src = exceptionAssetsSearchTf.value;
            PopulateSceneAssets(src);
        }

        /**
         * Returns a list of gameobjects listed in the excepted list
         */
        private List<GameObject> RetrieveExceptedAssets()
        {
            List<GameObject> excepted = new List<GameObject>();
            foreach(VisualElement ve in exceptedAssetsSl.Children())
                excepted.Add(ve.userData as GameObject);

            return excepted;
        }

        /**
         * Clears the list of excepted gameobjects
         */
        private void RemoveExceptions()
        {
            exceptedAssetsSl.Clear();
        }

        #region Callbacks
        private void OnSceneRadioChanged(ChangeEvent<bool> evt, string scenePath)
        {
            Debug.Log($"{scenePath} set to {evt.newValue}");
            if (evt.newValue)
            {
                var assets = AssetRetriever.GetAssets(scenePath);
                PopulateEntryPoints(scenePath);
                PopulateSceneAssets();
            }
        }

        private void OnEntryPointRadioChanged(ChangeEvent<bool> evt, GameObject entryPoint)
        {
            Debug.Log($"ENTRY POINT ({evt.newValue}): {entryPoint.name} | POS: {entryPoint.transform.position}");
            chosenEntryPoint = entryPoint;
        }

        private void OnAlgorithmChanged(ChangeEvent<string> evt)
        {
            Debug.Log($"NEW {evt.newValue} | OLD: {evt.previousValue}");
        
            if (Enum.TryParse<Settings.SortingAlgorithm>(evt.newValue, out Settings.SortingAlgorithm algorithm))
                Settings.SelectedAlgorithm = algorithm;
        }

        #endregion
    }
}
