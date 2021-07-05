using KSerialization;

namespace TagnumElite.AdvancedElectrolyzers
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class IndustrialElectrolyzer : IndustrialElectrolyzerMachine, ISecondaryOutput
    {
        public bool HasSecondaryConduitType(ConduitType type)
        {
            return portInfo.conduitType == type;
        }

        public CellOffset GetSecondaryConduitOffset(ConduitType type)
        {
            return portInfo.offset;
        }
    }
}

