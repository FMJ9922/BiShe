[System.Serializable]
public class LocalizationData
{
    public LocalizationCombine[] combines;
    
}

[System.Serializable]
public struct LocalizationCombine
{
    public string code;
    public string chinese;
    public string english;
    public LocalizationCombine(string code,string chinese,string english)
    {
        this.code = code;
        this.chinese = chinese;
        this.english = english;
    }
}