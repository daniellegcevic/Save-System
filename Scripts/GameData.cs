using System.Collections.Generic;

[System.Serializable]

public class GameData
{
    #region Variables

        #region Data

            public List<float> pickableObjectPositions = new List<float>();
            public List<float> pickableObjectRotations = new List<float>();
            public List<bool> surfaceHasAnObject = new List<bool>();
            public List<float> player = new List<float>();
            public bool isHoldingAnObject;
            public int objectPlayerIsHolding;
            public List<string> objectIDSystem = new List<string>();
            public List<bool> interactedWithObjects = new List<bool>();
            public List<int> currentTime = new List<int>();
            public List<bool> actionSurfaceHasAnObject = new List<bool>();
            public List<bool> actionSurfaceAnimationTriggered = new List<bool>();
            public List<bool> actionSurfaceIsMasterKey = new List<bool>();
            public List<string> actionSurfaceIDSystem = new List<string>();
            public List<string> actionSurfaceMasterKeySystem = new List<string>();

            public List<bool> itemsFound = new List<bool>();
            public int itemListIndex;

        #endregion

    #endregion

    #region Constructor

        public GameData(bool serializationFinished)
        {
            if(serializationFinished)
            {
                pickableObjectPositions = new List<float>();
                pickableObjectRotations = new List<float>();
                surfaceHasAnObject = new List<bool>();
                objectIDSystem = new List<string>();
                player = new List<float>();
                interactedWithObjects = new List<bool>();
                currentTime = new List<int>();
                actionSurfaceHasAnObject = new List<bool>();
                actionSurfaceAnimationTriggered = new List<bool>();
                actionSurfaceIsMasterKey = new List<bool>();
                actionSurfaceIDSystem = new List<string>();
                actionSurfaceMasterKeySystem = new List<string>();
                objectPlayerIsHolding = -1;

                itemsFound = new List<bool>();
                itemListIndex = -1;

                if(Clock.instance)
                {
                    currentTime.Add(Clock.instance.currentHourSaved);
                    currentTime.Add(Clock.instance.currentMinuteSaved);
                    currentTime.Add(Clock.instance.currentSecondSaved);
                }

                for(int x = 0; x < InteractableObject.interactables.Count; x++)
                {
                    if(!InteractableObject.interactables[x].doNotSave)
                    {
                        interactedWithObjects.Add(InteractableObject.interactables[x].interactedWithObject);
                    }
                    else
                    {
                        interactedWithObjects.Add(false);
                    }
                }

                player.Add(MovementController.instance.transform.position.x);
                player.Add(MovementController.instance.transform.position.z);
                player.Add(MovementController.instance.transform.rotation.eulerAngles.y);

                isHoldingAnObject = InteractionSystem.instance.isHoldingAnObject;

                for(int x = 0; x < PickableObject.objects.Count; x++)
                {
                    pickableObjectPositions.Add(PickableObject.objects[x].gameObject.transform.position.x);
                    pickableObjectPositions.Add(PickableObject.objects[x].gameObject.transform.position.y);
                    pickableObjectPositions.Add(PickableObject.objects[x].gameObject.transform.position.z);

                    pickableObjectRotations.Add(PickableObject.objects[x].gameObject.transform.rotation.eulerAngles.x);
                    pickableObjectRotations.Add(PickableObject.objects[x].gameObject.transform.rotation.eulerAngles.y);
                    pickableObjectRotations.Add(PickableObject.objects[x].gameObject.transform.rotation.eulerAngles.z);

                    if(PickableObject.objects[x].playerIsHoldingObject)
                    {
                        objectPlayerIsHolding = x;
                    }

                    if(PickableObject.objects[x].actionSurfaceMasterKey)
                    {
                        actionSurfaceMasterKeySystem.Add(PickableObject.objects[x].ID);
                        ActionSurfaceMasterKey surface = PickableObject.objects[x].actionSurfaceMasterKey.GetComponent<ActionSurfaceMasterKey>();
                        actionSurfaceMasterKeySystem.Add(surface.ID);
                    }
                }

                for(int x = 0; x < FlatSurface.surfaces.Count; x++)
                {
                    surfaceHasAnObject.Add(FlatSurface.surfaces[x].hasAnObject);

                    if(FlatSurface.surfaces[x].objectOnSurface)
                    {
                        objectIDSystem.Add(FlatSurface.surfaces[x].ID);
                        objectIDSystem.Add(FlatSurface.surfaces[x].objectOnSurface.ID);
                    }
                }

                for(int x = 0; x < ActionSurface.actionSurfaces.Count; x++)
                {
                    actionSurfaceHasAnObject.Add(ActionSurface.actionSurfaces[x].hasAnObject);
                    actionSurfaceAnimationTriggered.Add(ActionSurface.actionSurfaces[x].animationTriggered);
                    actionSurfaceIsMasterKey.Add(ActionSurface.actionSurfaces[x].isMasterKey);

                    if(ActionSurface.actionSurfaces[x].objectOnActionSurface)
                    {
                        actionSurfaceIDSystem.Add(ActionSurface.actionSurfaces[x].ID);
                        actionSurfaceIDSystem.Add(ActionSurface.actionSurfaces[x].objectOnActionSurface.ID);
                    }
                }

                if(ShoppingBasket.instance)
                {
                    for(int x = 0; x < 9; x++)
                    {
                        itemsFound.Add(ShoppingBasket.instance.itemsCollected[x]);
                    }

                    itemListIndex = ShoppingBasket.instance.listIndex;
                }
            }
        }

    #endregion
}