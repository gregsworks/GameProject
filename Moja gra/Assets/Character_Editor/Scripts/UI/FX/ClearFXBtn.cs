namespace CharacterEditor
{
    public class ClearFXBtn : FXTypeMaskSelector
    {
        protected override void OnClick()
        {
            MeshManager.Instance.OnClearFX(types);
        }
    }
}
