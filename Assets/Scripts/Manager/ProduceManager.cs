using System;
using Building;
namespace Manager
{
    public class ProduceManager : Singleton<ProduceManager>
    {

        public void DoProduce()
        {
            var buildings = MapManager.Instance.GetAllBuildings();
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i] is IProduct build)
                {
                    build.Output();
                    build.Input();
                } 
            }
        }
    }
}
