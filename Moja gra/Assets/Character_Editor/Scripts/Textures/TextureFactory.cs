namespace CharacterEditor
{
    public class TextureFactory
    {
        public static AbstractTexture Create(TextureType type, ITextureLoader loader, string characterRace)
        {
            switch (type)
            {
                case TextureType.Skin:
                    return new Textures.Skin(loader, 0, 0, 1024, 1024, characterRace);
                case TextureType.Eyebrow:
                    return new Textures.Eyebrow(loader, 384, 0, 640, 384, characterRace);
                case TextureType.Scar:
                    return new Textures.Scar(loader, 384, 0, 640, 384, characterRace);
                case TextureType.Beard:
                    return new Textures.Beard(loader, 384, 0, 640, 384, characterRace);
                case TextureType.FaceFeature:
                    return new Textures.FaceFeature(loader, 384, 0, 640, 384, characterRace);
                case TextureType.Hair:
                    return new Textures.Hair(loader, 384, 0, 640, 384, characterRace);
                case TextureType.Eye:
                    return new Textures.Eye(loader, 960, 0, 64, 64, characterRace);
                case TextureType.Head:
                    return new Textures.Head(loader, 384, 0, 640, 384, characterRace);
                case TextureType.Pants:
                    return new Textures.Pants(loader, 0, 0, 512, 576, characterRace);
                case TextureType.Torso:
                    return new Textures.Torso(loader, 0, 512, 1024, 512, characterRace);
                case TextureType.Shoe:
                    return new Textures.Shoes(loader, 0, 0, 384, 448, characterRace);
                case TextureType.Glove:
                    return new Textures.Gloves(loader, 512, 384, 512, 448, characterRace);
                case TextureType.RobeLong:
                    return new Textures.RobeLong(loader, 0, 0, 512, 576, characterRace);
                case TextureType.RobeShort:
                    return new Textures.RobeShort(loader, 0, 0, 512, 576, characterRace);
                case TextureType.Belt:
                    return new Textures.Belt(loader, 0, 512, 512, 256, characterRace);
                case TextureType.Cloak:
                    return new Textures.Cloak(loader, 0, 0, 512, 512, characterRace);
            }

            return null;
        }
    }
}