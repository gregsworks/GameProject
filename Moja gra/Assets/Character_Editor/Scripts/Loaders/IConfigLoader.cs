using System;

namespace CharacterEditor
{
    /*
     * Loading and parsing character configs
     */
    public interface IConfigLoader
    {
        void LoadConfigs(Action<Config[]> callback);
    }
}
