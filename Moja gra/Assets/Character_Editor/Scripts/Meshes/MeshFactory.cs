using CharacterEditor.Mesh;

namespace CharacterEditor
{
    public class MeshFactory
    {
        public static AbstractMesh Create(IMeshLoader loader, MeshTypeBone meshType, Config config)
        {
            var anchor = Helper.FindTransform(config.GetCharacter().transform, meshType.boneName);
            switch (meshType.mesh)
            {
                case MeshType.Beard:
                    return new Beard(loader, anchor, config.folderName);
                case MeshType.FaceFeature:
                    return new FaceFeature(loader, anchor, config.folderName);
                case MeshType.Hair:
                    return new Hair(loader, anchor, config.folderName);
                case MeshType.Helm:
                    return new Helm(loader, anchor, config.folderName);
                case MeshType.Torso:
                    return new Torso(loader, anchor, config.folderName);
                case MeshType.TorsoAdd:
                    return new TorsoAdd(loader, anchor, config.folderName);
                case MeshType.LegLeft:
                    return new Leg(loader, anchor, config.folderName, MeshType.LegLeft);
                case MeshType.LegRight:
                    return new Leg(loader, anchor, config.folderName, MeshType.LegRight);
                case MeshType.ShoulderLeft:
                    return new Shoulder(loader, anchor, config.folderName, MeshType.ShoulderLeft);
                case MeshType.ShoulderRight:
                    return new Shoulder(loader, anchor, config.folderName, MeshType.ShoulderRight);
                case MeshType.ArmLeft:
                    return new Arm(loader, anchor, config.folderName, MeshType.ArmLeft);
                case MeshType.ArmRight:
                    return new Arm(loader, anchor, config.folderName, MeshType.ArmRight);
                case MeshType.Belt:
                    return new Belt(loader, anchor, config.folderName);
                case MeshType.BeltAdd:
                    return new BeltAdd(loader, anchor, config.folderName);
                case MeshType.HandLeft:
                    return new Hand(loader, anchor, config.folderName, MeshType.HandLeft);
                case MeshType.HandRight:
                    return new Hand(loader, anchor, config.folderName, MeshType.HandRight);
            }
            return null;
        }
    }
}
