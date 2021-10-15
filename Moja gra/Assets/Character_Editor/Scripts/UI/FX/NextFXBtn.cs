namespace CharacterEditor
{
    public class NextFXBtn : FXTypeMaskSelector
    {
        protected override void OnClick()
        {
            MeshManager.Instance.OnNextFX(types);
        }
    }
}
