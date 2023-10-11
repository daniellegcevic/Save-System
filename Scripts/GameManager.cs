using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Variables

        #region Singleton

            public static GameManager instance = null;

        #endregion

        #region DEBUG

            [HideInInspector] public bool continueGame = true;
            [HideInInspector] public bool dataSaved = false;
            [HideInInspector] public bool dataLoaded = false;
            [HideInInspector] public bool loadingData = true;
            [HideInInspector] public bool startClock = false;
            [HideInInspector] public bool sceneTransition = false;
            [HideInInspector] public bool cutsceneTransition = false;

        #endregion

        #region Components

            private SceneLoader sceneLoader;
            private GameData gameData;
            private MainMenuData mainMenuData;

        #endregion

        #region Data

            [HideInInspector] public List<bool> animationsComplete = new List<bool>();

            [HideInInspector] public SceneLoader.Scene currentPlayerScene;
            [HideInInspector] public SceneLoader.Scene currentScene;

            [HideInInspector] public bool enableContinue;

            [HideInInspector] public int previousListIndex;

        #endregion

    #endregion

    #region Built-in Methods

        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            sceneLoader = GetComponent<SceneLoader>();
        }

        private void Start()
        {
            currentPlayerScene = SceneLoader.Scene.Wellco;
            currentScene = SceneLoader.Scene.MainMenu;
            sceneLoader.GameSetup();
        }

    #endregion

    #region Custom Methods

        public void SaveGame()
        {
            SaveGameData();
        }

        public void LoadGame()
        {
            loadingData = true;

            if(!sceneTransition)
            {
                if(continueGame)
                {
                    LoadGameData();

                    if(continueGame)
                    {
                        if(gameData != null)
                        {
                            StartCoroutine(LoadGameDataCoroutine());
                        }
                        else
                        {
                            dataLoaded = true;
                        }
                    }
                    else
                    {
                        dataLoaded = true;
                    }
                }
                else
                {
                    DeleteGameData();
                    ItemListManager.instance.NewItemList();
                    dataLoaded = true;
                }
            }
            else
            {
                dataLoaded = true;
            }
        }

        #region Scene Management

            public void UnloadPreviousScene()
            {
                sceneLoader.UnloadScene(currentScene);
            }

            public void LoadMainMenu()
            {
                sceneLoader.Load(SceneLoader.Scene.MainMenu);
            }

            public void LoadGameWorld(bool newGame, SceneLoader.Scene scene)
            {
                continueGame = !newGame;
                sceneLoader.Load(scene);
            }

        #endregion

        #region Save System

            public void SaveGameData()
            {
                if(!sceneTransition)
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    string path = Application.persistentDataPath + "/game.data";

                    FileStream fileStream = new FileStream(path, FileMode.Create);
                    gameData = new GameData(true);

                    binaryFormatter.Serialize(fileStream, gameData);
                    fileStream.Close();
                }
                else
                {
                    DeleteGameData();
                }

                PickableObject.objects = new List<PickableObject>();
                FlatSurface.surfaces = new List<FlatSurface>();
                InteractableObject.interactables = new List<InteractableObject>();
                ActionSurface.actionSurfaces = new List<ActionSurface>();
                ActionSurfaceMasterKey.masterKeySurfaces = new List<ActionSurfaceMasterKey>();
                animationsComplete = new List<bool>();

                dataSaved = true;
            }

            public void LoadGameData()
            {
                string path = Application.persistentDataPath + "/game.data";

                if(File.Exists(path))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    FileStream fileStream = new FileStream(path, FileMode.Open);

                    try
                    {
                        gameData = (GameData)binaryFormatter.Deserialize(fileStream);
                    }
                    catch
                    {
                        gameData = new GameData(false);
                        continueGame = false;
                    }

                    fileStream.Close();
                }
                else
                {
                    gameData = new GameData(false);
                    continueGame = false;
                }
            }

            public void DeleteGameData()
            {
                string path = Application.persistentDataPath + "/game.data";

                if(File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch { }
                }
            }

            public void DeleteMenuData()
            {
                string path = Application.persistentDataPath + "/menu.data";

                if(File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch { }
                }
            }

            public void SaveMainMenuData()
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                string path = Application.persistentDataPath + "/menu.data";

                FileStream fileStream = new FileStream(path, FileMode.Create);
                mainMenuData = new MainMenuData(true);

                binaryFormatter.Serialize(fileStream, mainMenuData);
                fileStream.Close();

                dataSaved = true;
            }

            public void LoadMainMenuData()
            {
                string path = Application.persistentDataPath + "/menu.data";
                bool dataDeserialized = true;

                if(File.Exists(path))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    FileStream fileStream = new FileStream(path, FileMode.Open);

                    try
                    {
                        mainMenuData = (MainMenuData)binaryFormatter.Deserialize(fileStream);
                    }
                    catch
                    {
                        mainMenuData = new MainMenuData(false);
                        dataDeserialized = false;
                    }

                    fileStream.Close();
                }
                else
                {
                    mainMenuData = new MainMenuData(false);
                    dataDeserialized = false;
                }

                if(dataDeserialized)
                {
                    currentPlayerScene = mainMenuData.playerScene;

                    enableContinue = mainMenuData.enableContinue;
                    previousListIndex = mainMenuData.previousListIndex;

                    EnableContinue();
                }
                else
                {
                    previousListIndex = -1;
                }

                dataLoaded = true;
            }

        #endregion

        public void EnableContinue()
        {
            if(enableContinue)
            {
                MainMenu.instance.continueButton.SetActive(true);
            }
        }

    #endregion

    #region Coroutines

        private IEnumerator LoadGameDataCoroutine()
        {
            int animations = 0;

            for(int x = 0; x < InteractableObject.interactables.Count; x++)
            {
                if(gameData.interactedWithObjects[x])
                {
                    animations++;
                    InteractableObject.interactables[x].PerformAction(true);

                    if(InteractableObject.interactables[x].unlockedByActionSurface)
                    {
                        InteractableObject.interactables[x].actionSurface.PerformObjectAction();
                    }

                    if(InteractableObject.interactables[x].unlockedByKeypad)
                    {
                        InteractableObject.interactables[x].keypad.PerformObjectAction();
                    }
                }
            }

            while(animationsComplete.Count != animations)
            {
                yield return null;
            }

            if(Clock.instance)
            {
                Clock.instance.hourHandStartAngle = (-360 * ((float)gameData.currentTime[0] / 12)) - 90;
                Clock.instance.minuteHandStartAngle = (-360 * ((float)gameData.currentTime[1] / 60)) - 90;
                Clock.instance.secondHandStartAngle = (-360 * ((float)gameData.currentTime[2] / 60)) - 90;

                Clock.instance.hourHand.localRotation = Quaternion.Euler(Clock.instance.hourHandStartAngle, 0, 0);
                Clock.instance.minuteHand.localRotation = Quaternion.Euler(Clock.instance.minuteHandStartAngle, 0, 0);
                Clock.instance.secondHand.localRotation = Quaternion.Euler(Clock.instance.secondHandStartAngle, 0, 0);

                Clock.instance.currentHour = gameData.currentTime[0];
                Clock.instance.currentMinute = gameData.currentTime[1];
                Clock.instance.currentSecond = gameData.currentTime[2];

                Clock.instance.minuteTimer = 60f - (float)gameData.currentTime[2];
                Clock.instance.hourTimer = 3600f - (float)gameData.currentTime[2] - ((float)gameData.currentTime[1] * 60f);
            }

            MovementController.instance.transform.position = new Vector3(gameData.player[0], 0, gameData.player[1]);
            MovementController.instance.transform.rotation = Quaternion.Euler(0, gameData.player[2], 0);
            CameraController.instance.smoothCameraYaw = gameData.player[2];
            CameraController.instance.cameraYaw = gameData.player[2];

            InteractionSystem.instance.isHoldingAnObject = gameData.isHoldingAnObject;

            if(gameData.objectPlayerIsHolding != -1)
            {
                PickableObject.objects[gameData.objectPlayerIsHolding].PickUpObject(false, true, false);
                PickableObject.objects[gameData.objectPlayerIsHolding].ShowIcon();
                PickableObject.objects[gameData.objectPlayerIsHolding].isResting = false;
                InteractionSystem.instance.scriptOfObjectPlayerIsHolding = PickableObject.objects[gameData.objectPlayerIsHolding];
            }

            int y = 0;

            for(int x = 0; x < FlatSurface.surfaces.Count; x++)
            {
                FlatSurface.surfaces[x].hasAnObject = gameData.surfaceHasAnObject[x];

                if(gameData.objectIDSystem.Contains(FlatSurface.surfaces[x].ID))
                {
                    for(int z = 0; z < PickableObject.objects.Count; z++)
                    {
                        if(PickableObject.objects[z].ID == gameData.objectIDSystem[gameData.objectIDSystem.IndexOf(FlatSurface.surfaces[x].ID) + 1])
                        {
                            FlatSurface.surfaces[x].objectOnSurface = PickableObject.objects[z];
                            PickableObject.objects[z].PutDownObject(FlatSurface.surfaces[x], true);
                        }
                    }
                }
            }

            for(int x = 0; x < PickableObject.objects.Count; x++)
            {
                PickableObject.objects[x].gameObject.transform.rotation = Quaternion.Euler(gameData.pickableObjectRotations[y], gameData.pickableObjectRotations[y + 1], gameData.pickableObjectRotations[y + 2]);
                y += 3;

                if(gameData.actionSurfaceMasterKeySystem.Count != 0)
                {
                    if(PickableObject.objects[x].ID == gameData.actionSurfaceMasterKeySystem[0])
                    {
                        for(int z = 0; z < ActionSurfaceMasterKey.masterKeySurfaces.Count; z++)
                        {
                            if(ActionSurfaceMasterKey.masterKeySurfaces[z].ID == gameData.actionSurfaceMasterKeySystem[1])
                            {
                                PickableObject.objects[x].actionSurfaceMasterKey = ActionSurfaceMasterKey.masterKeySurfaces[z].gameObject;
                            }
                        }
                    }
                }
            }

            for(int x = 0; x < ActionSurface.actionSurfaces.Count; x++)
            {
                ActionSurface.actionSurfaces[x].hasAnObject = gameData.actionSurfaceHasAnObject[x];
                ActionSurface.actionSurfaces[x].animationTriggered = gameData.actionSurfaceAnimationTriggered[x];
                ActionSurface.actionSurfaces[x].isMasterKey = gameData.actionSurfaceIsMasterKey[x];

                if(gameData.actionSurfaceIDSystem.Contains(ActionSurface.actionSurfaces[x].ID))
                {
                    for(int z = 0; z < PickableObject.objects.Count; z++)
                    {
                        if(PickableObject.objects[z].ID == gameData.actionSurfaceIDSystem[gameData.actionSurfaceIDSystem.IndexOf(ActionSurface.actionSurfaces[x].ID) + 1])
                        {
                            PickableObject.objects[z].PickUpObject(false, true, false);

                            if(PickableObject.objects[z].actionSurfaceMasterKey)
                            {
                                PickableObject.objects[z].UseObject(PickableObject.objects[z].actionSurfaceMasterKey);
                            }
                            else
                            {
                                PickableObject.objects[z].UseObject(null);
                            }
                        }
                    }
                }
            }

            if(ShoppingBasket.instance)
            {
                ItemListManager.instance.LoadItemList(gameData.itemListIndex);

                for (int x = 0; x < 9; x++)
                {
                    ShoppingBasket.instance.itemsCollected[x] = gameData.itemsFound[x];
                }

                ShoppingBasket.instance.CollectPreviousItems();
            }

            dataLoaded = true;
        }

    #endregion
}