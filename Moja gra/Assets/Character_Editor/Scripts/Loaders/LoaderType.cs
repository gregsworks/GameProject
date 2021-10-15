namespace CharacterEditor
{
    public enum LoaderType
    {
        AssetBundle,
#if UNITY_EDITOR
        AssetDatabase
#endif
    }
}
