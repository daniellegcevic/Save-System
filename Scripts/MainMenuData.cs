using System.Collections.Generic;

[System.Serializable]

public class MainMenuData
{
    #region Variables

        #region Data

            public SceneLoader.Scene playerScene;

            public bool enableContinue;
            public int previousListIndex;

        #endregion

    #endregion

    #region Constructor

        public MainMenuData(bool serializationFinished)
        {
            if(serializationFinished)
            {
                playerScene = GameManager.instance.currentPlayerScene;

                enableContinue = GameManager.instance.enableContinue;
                previousListIndex = GameManager.instance.previousListIndex;
            }
        }

    #endregion
}