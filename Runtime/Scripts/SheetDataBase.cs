namespace Violet.Sheet
{
    public class SheetDataBase : ISheetData
    {
        public string key = "";

        public string GetKey()
        {
            return key;
        }

        public virtual void Initialize()
        {
            UnityEngine.Debug.Log($"{key}");
        }
    }
}