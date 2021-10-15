namespace CharacterEditor
{
    public class PrevFXBtn : FXTypeMaskSelector
    {
        protected override void OnClick()
        {
            MeshManager.Instance.OnPrevFX(types);
        }
    }
}
